using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ExplorerNPCScheduler
{
    private INPCTaskRegistrable orderer;
    private bool isRecruiting;

    public static bool IsDefenseTime { get; private set; } = false;
    private readonly LinkedList<AllyNPC> freeExplorers = new();
    private readonly LinkedList<AllyNPC> boardingExplorers = new();
    private readonly HashSet<AllyNPC> leftDefenseExplorers = new();  // 좌측 영역 Defense 중인 Explorer
    private readonly HashSet<AllyNPC> rightDefenseExplorers = new(); // 우측 영역 Defense 중인 Explorer

    //private bool IsDefenseTime = false;

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

        AssignExplorerToOrder();
        //AssignExplorerToDefense();
    }



    public void AddExplorer(AllyNPC explorer)
    {
        freeExplorers.AddLast(explorer);

        SetCommand_Wander(explorer);
    }

    public void ExplorerDead(AllyNPC explorer)
    {
        freeExplorers.Remove(explorer);
        boardingExplorers.Remove(explorer);
    }



    public void RegistOrder(INPCTaskRegistrable Orderer)
    {
        if (orderer != Orderer) orderer = Orderer;
        isRecruiting = true;
    }

    private void AssignExplorerToOrder()
    {
        if (freeExplorers.Count == 0 || isRecruiting == false) return;

        int recruitCount = orderer.NPCCountMax - boardingExplorers.Count;

        if (freeExplorers.Count <= recruitCount)
        {
            foreach (AllyNPC explorer in freeExplorers)
            {
                boardingExplorers.AddLast(explorer);
                explorer.AI.SetCommand_Work(orderer.BoundaryLeft, orderer.BoundaryRight);
            }
            freeExplorers.Clear();
        }
        else
        {
            foreach (AllyNPC explorer in PopClosestExplorerN((orderer.BoundaryLeft + orderer.BoundaryRight) / 2f, recruitCount))
            {
                boardingExplorers.AddLast(explorer);
                explorer.AI.SetCommand_Work(orderer.BoundaryLeft, orderer.BoundaryRight);
            }
        }
    }

    private List<AllyNPC> PopClosestExplorerN(float position, int N)
    {
        // Max-Heap 생성 (거리 기준으로 비교)
        MaxHeap<AllyNPC, float> maxHeap = new(N);

        // 삭제할 탐험가 임시 저장
        List<LinkedListNode<AllyNPC>> nodesToRemove = new();

        // 탐험가 거리순으로 검색
        var currentNode = freeExplorers.First;
        while (currentNode != null)
        {
            var nextNode = currentNode.Next;

            AllyNPC explorer = currentNode.Value;
            float distance = position - explorer.transform.position.x;

            if (maxHeap.Count < N)
            {
                maxHeap.Add(explorer, distance);
                nodesToRemove.Add(currentNode); // 삭제 예정
            }
            else if (distance < maxHeap.Peek().Value)
            {
                freeExplorers.AddLast(maxHeap.Remove().Data); // 이전 최대값 복구
                maxHeap.Add(explorer, distance);
                nodesToRemove.Add(currentNode); // 삭제 예정
            }

            currentNode = nextNode;
        }

        // 한 번에 삭제
        if (IsDefenseTime)
        {
            foreach (var node in nodesToRemove)
            {
                leftDefenseExplorers.Remove(node.Value);
                rightDefenseExplorers.Remove(node.Value);
            }
        }
        foreach (var node in nodesToRemove)
        {
            freeExplorers.Remove(node);
        }

        // Max-Heap의 모든 요소를 결과 리스트로 반환
        return maxHeap.Heap.Select(item => item.Data).ToList();
    }

    public void ExplorerBoarding(AllyNPC explorer)
    {
        orderer.IncreaseNPC();
    }
    public void ExplorerExit(AllyNPC explorer)
    {
        orderer.DecreaseNPC();
    }

    public void RemoveOrder(INPCTaskRegistrable Orderer)
    {
        isRecruiting = false;
    }

    public void TaskComplete(INPCTaskRegistrable Orderer)
    {
        // 작업중인 Explorer 리스트에서 제거
        foreach (AllyNPC explorer in boardingExplorers)
        {
            freeExplorers.AddLast(explorer);
            // 작업 완료
            SetCommand_Wander(explorer);
        }

        boardingExplorers.Clear();
    }



    private void DaySunriseEvent()
    {
        IsDefenseTime = false;

        foreach (AllyNPC explorer in freeExplorers)
        {
            SetCommand_Wander(explorer);
        }

        leftDefenseExplorers.Clear();
        rightDefenseExplorers.Clear();
    }
    private void DayEveningEvent()
    {
        IsDefenseTime = true;

        // 현재 위치 기반으로 좌우 반반 나뉘어 방어
        AllyNPC[] sortedByPos = freeExplorers.OrderBy(e => e.transform.position.x).ToArray();
        int num = sortedByPos.Length;
        int halfNum = num / 2;
        if (num % 2 == 0)
        {
            for (int i = 0; i < halfNum; i++)
            {
                leftDefenseExplorers.Add(sortedByPos[i]);
            }
            for (int i = halfNum; i < num; i++)
            {
                rightDefenseExplorers.Add(sortedByPos[i]);
            }
        }
        else
        {
            for (int i = 0; i < halfNum; i++)
            {
                leftDefenseExplorers.Add(sortedByPos[i]);
            }
            if (Random.Range(0,2) == 0) leftDefenseExplorers.Add(sortedByPos[halfNum]);
            else rightDefenseExplorers.Add(sortedByPos[halfNum]);
            for (int i = halfNum + 1; i < num; i++)
            {
                rightDefenseExplorers.Add(sortedByPos[i]);
            }
        }

        foreach (AllyNPC explorer in leftDefenseExplorers)
        {
            SetCommand_DefensePosition(explorer, leftSide: true, isRetreat: false);
        }
        foreach (AllyNPC explorer in rightDefenseExplorers)
        {
            SetCommand_DefensePosition(explorer, leftSide: false, isRetreat: false);
        }
    }


    public void AllyBaseChanged(bool isLeftSide, bool isRetreat)
    {
        if (IsDefenseTime == false)
        {
            foreach (AllyNPC explorer in freeExplorers)
            {
                SetCommand_Wander(explorer);
            }
        }
        else
        {
            if (isLeftSide)
            {
                foreach (AllyNPC explorer in leftDefenseExplorers)
                {
                    SetCommand_DefensePosition(explorer, leftSide: true, isRetreat);
                }
            }
            else
            {
                foreach (AllyNPC explorer in rightDefenseExplorers)
                {
                    SetCommand_DefensePosition(explorer, leftSide: false, isRetreat);
                }
            }
        }
    }


    private void SetCommand_Wander(AllyNPC archer)
    {
        archer.AI.SetCommand_Wander(StageManager.Instance.MapBoundaryController.AllyBase.Left + AppConstants.BaseWanderInner,
                                    StageManager.Instance.MapBoundaryController.AllyBase.Right - AppConstants.BaseWanderInner);
    }
    private void SetCommand_DefensePosition(AllyNPC archer, bool leftSide, bool isRetreat)
    {
        if (leftSide)
        {
            archer.AI.SetCommand_DefensePosition(StageManager.Instance.MapBoundaryController.AllyBase.Left + AppConstants.BaseDefenseOuter - 1f,
                                                 StageManager.Instance.MapBoundaryController.AllyBase.Left + AppConstants.BaseDefenseInner - 1f,
                                                 standLeft: true, isRetreat);
        }
        else
        {
            archer.AI.SetCommand_DefensePosition(StageManager.Instance.MapBoundaryController.AllyBase.Right - AppConstants.BaseDefenseInner + 1f,
                                                 StageManager.Instance.MapBoundaryController.AllyBase.Right - AppConstants.BaseDefenseOuter + 1f,
                                                 standLeft: false, isRetreat);
        }
    }
}
