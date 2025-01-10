using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ArcherNPCScheduler
{
    private DayCycleController DayCycleController => StageManager.Instance.DayCycleController;

    public static bool IsDefenseTime { get; private set; } = false;
    private readonly LinkedList<AllyNPC> leftDefenseArchers = new();  // 좌측 영역 Defense 중인 Archer
    private readonly LinkedList<AllyNPC> rightDefenseArchers = new(); // 우측 영역 Defense 중인 Archer

    private const float archerCrossInterval = 15f; // 궁수가 기지를 횡단하는 간격(초)
    private const float updateTick = 0.1f;
    private float updateTimeRemain;
    private float archerCrossTimeRemain;


    public void Initialize()
    {
        DayCycleController.OnPreSunrise += DaySunriseEvent;
        DayCycleController.OnEvening += DayEveningEvent;

        updateTimeRemain = 1.04f;
        archerCrossTimeRemain = 20f;
    }

    public void UpdateScheduler()
    {
        updateTimeRemain -= Time.deltaTime;
        archerCrossTimeRemain -= Time.deltaTime;
        if (updateTimeRemain < 0)
        {
            updateTimeRemain = updateTick;
        }
        else
        {
            return;
        }

        ArcherCrossesBase();
    }



    public void AddArcher(AllyNPC archer)
    {
        bool addLeft;
        if (leftDefenseArchers.Count < rightDefenseArchers.Count) addLeft = true;
        else if (leftDefenseArchers.Count > rightDefenseArchers.Count) addLeft = false;
        else
        {
            addLeft = (Random.Range(0, 2) == 0);
        }

        GoToSite(archer, goLeft: addLeft);
    }

    public void ArcherDead(AllyNPC archer)
    {
        // Defense Archer 리스트에서 제거
        leftDefenseArchers.Remove(archer);
        rightDefenseArchers.Remove(archer);
    }

    private void GoToSite(AllyNPC archer, bool goLeft)
    {
        if (goLeft)
        {
            leftDefenseArchers.AddFirst(archer);

            if (IsDefenseTime == false)
            {
                SetCommand_Wander(archer, leftSide: true);
            }
            else
            {
                SetCommand_DefensePosition(archer, leftSide: true, isRetreat: false);
            }
        }
        else
        {
            rightDefenseArchers.AddFirst(archer);

            if (IsDefenseTime == false)
            {
                SetCommand_Wander(archer, leftSide: false);
            }
            else
            {
                SetCommand_DefensePosition(archer, leftSide: false, isRetreat: false);
            }
        }
    }

    private void ArcherCrossesBase()
    {
        if (archerCrossTimeRemain < 0)
        {
            if (IsDefenseTime) archerCrossTimeRemain = updateTick; // 방어 중엔 자주 수행
            else archerCrossTimeRemain = archerCrossInterval;
        }
        else
        {
            return;
        }

        bool formLeft;
        if (IsDefenseTime == false)
        {
            if (leftDefenseArchers.Count < rightDefenseArchers.Count) formLeft = false;
            else if (leftDefenseArchers.Count > rightDefenseArchers.Count) formLeft = true;
            else
            {
                formLeft = (Random.Range(0, 2) == 0);
            }
        }
        else
        {
            if (leftDefenseArchers.Count < rightDefenseArchers.Count - 1) formLeft = false;
            else if (leftDefenseArchers.Count - 1 > rightDefenseArchers.Count) formLeft = true;
            else
            {
                return; // 방어 중엔 좌우 숫자 2명 이상 차이나지 않는 경우 이동 안함
            }
        }
        

        if (formLeft)
        {
            AllyNPC archer = PopRichestArcher(leftDefenseArchers);
            if (archer == null) return;

            leftDefenseArchers.Remove(archer);

            GoToSite(archer, goLeft: false);
        }
        else
        {
            AllyNPC archer = PopRichestArcher(rightDefenseArchers);
            if (archer == null) return;

            rightDefenseArchers.Remove(archer);

            GoToSite(archer, goLeft: true);
        }
    }

    private AllyNPC PopRichestArcher(LinkedList<AllyNPC> archers)
    {
        if (archers.Count == 0) return null;

        AllyNPC richestArcher = null;
        int richestCoin = -1;

        foreach (AllyNPC archer in archers)
        {
            int coin = archer.CurrentCoin;
            if (coin > richestCoin)
            {
                richestCoin = coin;
                richestArcher = archer;
            }
        }

        if (richestArcher != null)
        {
            // Defense Archer 리스트에서 제거
            archers.Remove(richestArcher);
        }

        return richestArcher;
    }



    private void DaySunriseEvent()
    {
        IsDefenseTime = false;

        foreach (AllyNPC archer in leftDefenseArchers)
        {
            SetCommand_Wander(archer, leftSide: true);
        }
        foreach (AllyNPC archer in rightDefenseArchers)
        {
            SetCommand_Wander(archer, leftSide: false);
        }

        archerCrossTimeRemain = archerCrossInterval; // 남은 시간 초기화
    }
    private void DayEveningEvent()
    {
        IsDefenseTime = true;

        foreach (AllyNPC archer in leftDefenseArchers)
        {
            SetCommand_DefensePosition(archer, leftSide: true, isRetreat: false);
        }
        foreach (AllyNPC archer in rightDefenseArchers)
        {
            SetCommand_DefensePosition(archer, leftSide: false, isRetreat: false);
        }

        archerCrossTimeRemain = updateTick; // 남은 시간 초기화
    }


    public void AllyBaseChanged(bool isLeftSide, bool isRetreat)
    {
        if (IsDefenseTime == false)
        {
            if (isLeftSide)
            {
                foreach (AllyNPC archer in leftDefenseArchers)
                {
                    SetCommand_Wander(archer, leftSide: true);
                }
            }
            else
            {
                foreach (AllyNPC archer in rightDefenseArchers)
                {
                    SetCommand_Wander(archer, leftSide: false);
                }
            }
        }
        else
        {
            if (isLeftSide)
            {
                foreach (AllyNPC archer in leftDefenseArchers)
                {
                    SetCommand_DefensePosition(archer, leftSide: true, isRetreat);
                }
            }
            else
            {
                foreach (AllyNPC archer in rightDefenseArchers)
                {
                    SetCommand_DefensePosition(archer, leftSide: false, isRetreat);
                }
            }
        }
    }


    private void SetCommand_Wander(AllyNPC archer, bool leftSide)
    {
        if (leftSide)
        {
            archer.AI.SetCommand_Wander(StageManager.Instance.MapBoundaryController.AllyBase.Left - AppConstants.BasePatrolOuter,
                                        StageManager.Instance.MapBoundaryController.AllyBase.Left - AppConstants.BasePatrolInner);
        }
        else
        {
            archer.AI.SetCommand_Wander(StageManager.Instance.MapBoundaryController.AllyBase.Right + AppConstants.BasePatrolInner,
                                        StageManager.Instance.MapBoundaryController.AllyBase.Right + AppConstants.BasePatrolOuter);
        }
    }
    private void SetCommand_DefensePosition(AllyNPC archer, bool leftSide, bool isRetreat)
    {
        if (leftSide)
        {
            archer.AI.SetCommand_DefensePosition(StageManager.Instance.MapBoundaryController.AllyBase.Left + AppConstants.BaseDefenseOuter,
                                                 StageManager.Instance.MapBoundaryController.AllyBase.Left + AppConstants.BaseDefenseInner,
                                                 standLeft: true, isRetreat);
        }
        else
        {
            archer.AI.SetCommand_DefensePosition(StageManager.Instance.MapBoundaryController.AllyBase.Right - AppConstants.BaseDefenseInner,
                                                 StageManager.Instance.MapBoundaryController.AllyBase.Right - AppConstants.BaseDefenseOuter,
                                                 standLeft: false, isRetreat);
        }
    }
}
