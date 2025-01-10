
using Cinemachine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 건설,채취 등 작업자에게 지시를 내리고, 작업자의 최상위 명령을 관리
/// </summary>
public class WorkerNPCScheduler
{
    private class WorkOrder
    {
        public INPCTaskRegistrable Orderer;
        public List<AllyNPC> Workers;      // 해당 작업에 투입된 작업자
        public float LastAssignWorkerTime; // 가장 최근에 작업자가 투입된 시간

        public WorkOrder(INPCTaskRegistrable building)
        {
            Orderer = building;
            Workers = new List<AllyNPC>();
            LastAssignWorkerTime = float.MinValue;
        }
    }

    private readonly LinkedList<AllyNPC> freeWorkers = new();

    private readonly Dictionary<INPCTaskRegistrable, WorkOrder> OrderFromOrderer = new(); // 주문자에서 접근
    private readonly Dictionary<AllyNPC, WorkOrder> OrderFromWorker = new();              // worker에서 접근, 전체 Worker에서 freeWorkers를 뺀 부분에 해당
    private readonly LinkedList<INPCTaskRegistrable> Orderer_0Worker = new(); // 작업자가 없는 주문자 list(빠른 우선순위 검색)
    private readonly LinkedList<INPCTaskRegistrable> Orderer_1Worker = new(); // 작업자가 1명 있는 주문자 list(빠른 우선순위 검색)

    private bool IsDefenseTime = false;
    private readonly LinkedList<AllyNPC> leftDefenseWorkers = new();  // 좌측 영역 DefensePosition 중인 Worker
    private readonly LinkedList<AllyNPC> rightDefenseWorkers = new(); // 우측 영역 DefensePosition 중인 Worker
    private readonly LinkedList<AllyNPC> freeDefenseWorkers = new();  // DefensePosition 수행하지 않는 Worker

    private const float assignSecondWorkerDelay = 5f; // 작업자가 1명 붙은 주문에 두번째 작업자가 할당되기까지 최소시간(초)
    private const float updateTick = 0.1f;
    private float updateTimeRemain;

    public void Initialize()
    {
        StageManager.Instance.DayCycleController.OnPreSunrise += DaySunriseEvent;
        StageManager.Instance.DayCycleController.OnEvening += DayEveningEvent;

        updateTimeRemain = 1.02f;
    }

    public void UpdateScheduler()
    {
        updateTimeRemain -= Time.deltaTime;
        if (updateTimeRemain < 0)
        {
            updateTimeRemain = updateTick;
        }
        else
        {
            return;
        }

        AssignWorkerToOrder();
        AssignWorkerToDefense();
    }



    public void AddWorker(AllyNPC worker)
    {
        freeWorkers.AddLast(worker);
        freeDefenseWorkers.AddLast(worker);

        SetCommand_Wander(worker);
    }

    public void WorkerDead(AllyNPC worker)
    {
        // 작업중인 Worker 리스트에서 제거
        if (OrderFromWorker.TryGetValue(worker, out WorkOrder buildOrder))
        {
            buildOrder.Workers.Remove(worker);

            int workerCount = buildOrder.Workers.Count;
            if (workerCount == 0)
            {
                Orderer_1Worker.Remove(buildOrder.Orderer);
                Orderer_0Worker.AddLast(buildOrder.Orderer);
            }
            else if (workerCount == 1)
            {
                Orderer_1Worker.AddLast(buildOrder.Orderer);
            }

            buildOrder.Orderer.DecreaseNPC();

            OrderFromWorker.Remove(worker);
        }
        // 작업하지 않는 Worker 리스트에서 제거
        else
        {
            freeWorkers.Remove(worker);
        }

        // Defense Worker 리스트에서 제거
        leftDefenseWorkers.Remove(worker);
        rightDefenseWorkers.Remove(worker);
        freeDefenseWorkers.Remove(worker);
    }



    public void RegistOrder(INPCTaskRegistrable Orderer)
    {
        OrderFromOrderer.Add(Orderer, new WorkOrder(Orderer));
        Orderer_0Worker.AddLast(Orderer);
    }

    private void AssignWorkerToOrder()
    {
        if (freeWorkers.Count == 0) return;

        LinkedListNode<INPCTaskRegistrable> currentNode;

        // 작업자가 붙지 않은 주문
        currentNode = Orderer_0Worker.First;
        while (currentNode != null)
        {
            var nextNode = currentNode.Next; // 다음 노드 저장

            INPCTaskRegistrable Orderer = currentNode.Value;
            WorkOrder buildOrder = OrderFromOrderer[Orderer];
            AllyNPC worker = PopClosestWorker((Orderer.BoundaryLeft + Orderer.BoundaryRight) / 2f);
            worker.AI.SetCommand_Work(Orderer.BoundaryLeft, Orderer.BoundaryRight);

            buildOrder.Workers.Add(worker);
            buildOrder.LastAssignWorkerTime = Time.time;
            OrderFromWorker.Add(worker, buildOrder);

            Orderer_0Worker.Remove(currentNode);
            Orderer_1Worker.AddLast(Orderer);

            if (freeWorkers.Count == 0) return;

            currentNode = nextNode; // 다음 노드로 이동
        }

        // 작업자가 1명 붙은 주문
        currentNode = Orderer_1Worker.First;
        while (currentNode != null)
        {
            var nextNode = currentNode.Next; // 다음 노드 저장

            INPCTaskRegistrable Orderer = currentNode.Value;
            // 주문자의 작업자 최대 호출 수가 1이면 건너뜀
            if (Orderer.NPCCountMax == 1)
            {
                currentNode = nextNode; // 다음 노드로 이동
                continue;
            }

            WorkOrder buildOrder = OrderFromOrderer[Orderer];
            // 첫 작업자가 배정된 뒤 일정 시간이 지나야 다음 작업자를 받을 수 있음
            if (Time.time - buildOrder.LastAssignWorkerTime < assignSecondWorkerDelay)
            {
                currentNode = nextNode; // 다음 노드로 이동
                continue;
            }

            AllyNPC worker = PopClosestWorker((Orderer.BoundaryLeft + Orderer.BoundaryRight) / 2f);
            // 작업 구현(건설 영역은 임시로 임의로 지정)
            worker.AI.SetCommand_Work(Orderer.BoundaryLeft, Orderer.BoundaryRight);

            buildOrder.Workers.Add(worker);
            buildOrder.LastAssignWorkerTime = Time.time;
            OrderFromWorker.Add(worker, buildOrder);

            Orderer_1Worker.Remove(currentNode);

            if (freeWorkers.Count == 0) return;

            currentNode = nextNode; // 다음 노드로 이동
        }
    }

    private AllyNPC PopClosestWorker(float position)
    {
        if (freeWorkers.Count == 0) return null;

        AllyNPC closestWorker = null;
        float closestDistance = float.MaxValue;

        foreach (AllyNPC worker in freeWorkers)
        {
            float distance = Mathf.Abs(worker.transform.position.x - position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestWorker = worker;
            }
        }

        if (closestWorker != null)
        {
            // 작업하지 않는 Worker 리스트에서 제거
            freeWorkers.Remove(closestWorker);

            // Defense Worker 중에서 가져다 쓸 수 있음
            leftDefenseWorkers.Remove(closestWorker);
            rightDefenseWorkers.Remove(closestWorker);
        }

        return closestWorker;
    }

    public void WorkStart(AllyNPC worker)
    {
        OrderFromWorker[worker].Orderer.IncreaseNPC();
    }
    public void WorkStop(AllyNPC worker)
    {
        OrderFromWorker[worker].Orderer.DecreaseNPC();
    }

    public void TaskComplete(INPCTaskRegistrable Orderer)
    {
        WorkOrder buildOrder = OrderFromOrderer[Orderer];

        // 작업중인 Orderer 리스트에서 제거
        int workerCount = buildOrder.Workers.Count;
        if (workerCount == 0)
        {
            Orderer_0Worker.Remove(Orderer);
        }
        else if (workerCount == 1)
        {
            Orderer_1Worker.Remove(Orderer);
        }

        // 작업중인 Worker 리스트에서 제거
        foreach (AllyNPC worker in buildOrder.Workers)
        {
            OrderFromWorker.Remove(worker);
            freeWorkers.AddLast(worker);
            // 작업 완료
            SetCommand_Wander(worker);
        }

        OrderFromOrderer.Remove(Orderer);
    }


    
    private void DaySunriseEvent()
    {
        IsDefenseTime = false;
    }
    private void DayEveningEvent()
    {
        IsDefenseTime = true;
    }

    private void AssignWorkerToDefense()
    {
        if (freeDefenseWorkers.Count == 0) return;

        if (IsDefenseTime)
        {
            // TODO : 방어 구조물 확인
            if (leftDefenseWorkers.Count < 2)
            {
                // TODO : 방어 위치
                //AllyNPC worker = PopClosestWorkerForDefense(방어 위치);
                //worker.AI.SetCommand_DefensePosition(방어 left, 방어 right);

                //leftDefenseWorkers.AddLast(worker);

                //if (freeDefenseWorkers.Count == 0) return;
            }

            // TODO : 방어 구조물 확인
            if (rightDefenseWorkers.Count < 2)
            {
                // TODO : 방어 위치
                //AllyNPC worker = PopClosestWorkerForDefense(방어 위치);
                //worker.AI.SetCommand_DefensePosition(방어 left, 방어 right);

                //rightDefenseWorkers.AddLast(worker);
            }
        }
        else
        {
            foreach (AllyNPC worker in leftDefenseWorkers)
            {
                SetCommand_Wander(worker);

                freeDefenseWorkers.AddLast(worker);
            }
            leftDefenseWorkers.Clear();

            foreach (AllyNPC worker in rightDefenseWorkers)
            {
                SetCommand_Wander(worker);

                freeDefenseWorkers.AddLast(worker);
            }
            rightDefenseWorkers.Clear();
        }
    }

    private AllyNPC PopClosestWorkerForDefense(float position)
    {
        if (freeDefenseWorkers.Count == 0) return null;

        AllyNPC closestWorker = null;
        float closestDistance = float.MaxValue;

        foreach (AllyNPC worker in freeDefenseWorkers)
        {
            // 작업중인 Worker는 제외
            if (OrderFromWorker.ContainsKey(worker)) continue;

            float distance = Mathf.Abs(worker.transform.position.x - position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestWorker = worker;
            }
        }

        if (closestWorker != null)
        {
            // Defense Worker 리스트에서 제거
            freeDefenseWorkers.Remove(closestWorker);
        }

        return closestWorker;
    }
    

    public void AllyBaseChanged(bool isLeftSide)
    {
        foreach (AllyNPC worker in freeWorkers)
        {
            SetCommand_Wander(worker);
        }
    }


    private void SetCommand_Wander(AllyNPC worker)
    {
        worker.AI.SetCommand_Wander(StageManager.Instance.MapBoundaryController.AllyBase.Left + AppConstants.BaseWanderInner,
                                    StageManager.Instance.MapBoundaryController.AllyBase.Right - AppConstants.BaseWanderInner);
    }
}