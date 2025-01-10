using System;
using UnityEditor;
using UnityEngine;

public class AI_Villager : AllyNPCAI
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;


    // 커맨드는 다른 작업이 없을 때 Idle State를 지정
    private enum Command
    {
        NONE,
        Wander
    }
    private Command CurrentCommand;
    private Command AppliedCommand;
    private float commandVal_WanderLeft;
    private float commandVal_WanderRight;

    private enum State
    {
        Wander,
        OfferCoin,
        Attracted,
        BlownAway,
        Flee
    }

    private StateMachine_Villager StateMachine;
    private State CurrentState;

    private float detectTime;
    private bool detectTick;
    private bool alreadyChangeState;
    private GameObject targetMonster;
    private GameObject targetTool;
    private float attractDelay;
    private float fleeTimeRemain;

    public override bool Initialize(AllyNPC allyNPC)
    {
        NPC = allyNPC;

        spriteRenderer.sortingOrder = NPC.orderInLayer;
        AnimationController = new AllyNPCAnimatorController();
        AnimationController.Initialize(animator);
        StateMachine = new StateMachine_Villager(NPC);

        CurrentCommand = Command.NONE;
        AppliedCommand = Command.NONE;

        debugLabelOffset = new Vector3(0, NPC.ClassData.initialoffsety + 0.5f, 0);

        return true;
    }

    public override void Destroy()
    {
        NPC = null;
        AnimationController = null;
        StateMachine = null;
    }

    public override void SetCommand_Wander(float left, float right)
    {
        CurrentCommand = Command.Wander;
        commandVal_WanderLeft = left;
        commandVal_WanderRight = right;
        StateMachine.State_Wander.SetValue(commandVal_WanderLeft, commandVal_WanderRight);
    }
    private void ApplyCurrentCommand()
    {
        AppliedCommand = CurrentCommand;

        switch (AppliedCommand)
        {
            case Command.Wander:
                SetState_Wander(commandVal_WanderLeft, commandVal_WanderRight);
                break;
        }
    }

    public override void UpdateAI()
    {
        alreadyChangeState = false;

        detectTime -= Time.deltaTime;
        if (detectTime < 0)
        {
            detectTime = NPC.ClassData.detectinterval;
            detectTick = true;
        }
        else
        {
            detectTick = false;
        }
        attractDelay -= Time.deltaTime;

        switch (CurrentState)
        {
            case State.Wander:
                ExecuteState_Wander();
                break;

            case State.OfferCoin: // Lock Check
                ExecuteState_OfferCoin();
                break;

            case State.Attracted:
                ExecuteState_Attracted();
                break;

            case State.BlownAway: // Lock Check
                ExecuteState_BlownAway();
                break;

            case State.Flee:
                ExecuteState_Flee();
                break;
        }

        if (StateLock == false)
        {
            if (AppliedCommand != CurrentCommand)
            {
                ApplyCurrentCommand();
            }
            else if (IsInteractWithPlayer == true && NPC.CurrentCoin > 0 && CurrentState != State.OfferCoin)
            {
                SetState_OfferCoin();
            }
        }

        StateMachine?.Execute();

        alreadyChangeState = false;
    }

    private void ExecuteState_Wander()
    {
        if (detectTick)
        {
            targetMonster = NPC.DetectTargetEnemy();
            if (targetMonster != null)
            {
                float curPos = NPC.transform.position.x;
                float baseLeft = StageManager.Instance.MapBoundaryController.AllyBase.Left;
                float baseRight = StageManager.Instance.MapBoundaryController.AllyBase.Right;
                bool isGoLeft = curPos < targetMonster.transform.position.x;
                // 기지 끝까지 몰리면 반대로 도망감
                if (isGoLeft && curPos < (baseLeft + 4 * baseRight) / 5) isGoLeft = false;
                else if (isGoLeft == false && curPos > (4 * baseLeft + baseRight) / 5) isGoLeft = true;

                SetState_Flee(isGoLeft);
                return;
            }

            if (NPC.IsAttractLost(targetTool))
            {
                targetTool = NPC.DetectTool();

                attractDelay = NPC.ClassData.attractmovedelay;
            }
            else
            {
                if (attractDelay < 0)
                {
                    SetState_Attracted(targetTool.transform.position.x);
                }
            }
        }
    }
    private void ExecuteState_OfferCoin() // Lock Check
    {
        if (StateLock == false && (IsInteractWithPlayer == false || NPC.CurrentCoin <= 0))
        {
            ApplyCurrentCommand();
        }
    }
    private void ExecuteState_Attracted()
    {
        if (NPC.IsAttractLost(targetTool))
        {
            targetTool = null;
            ApplyCurrentCommand();
        }
    }
    private void ExecuteState_BlownAway() // Lock Check
    {
        if (StateLock == false)
        {
            NPC.SetTargetTag(true);
            NPC.CanColliderDetect = true;

            ApplyCurrentCommand();
        }
    }
    private void ExecuteState_Flee()
    {
        if (detectTick)
        {
            if (NPC.IsAttractLost(targetTool))
            {
                targetTool = NPC.DetectTool();

                attractDelay = NPC.ClassData.attractmovedelay;
            }
            else
            {
                if (attractDelay < 0)
                {
                    SetState_Attracted(targetTool.transform.position.x);
                    return;
                }
            }
        }

        fleeTimeRemain -= Time.deltaTime;
        if (fleeTimeRemain < 0)
        {
            ApplyCurrentCommand();
        }
    }



    private void SetState_Wander(float left, float right)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Wander;
        debugCurrentState = "Wander";

        StateMachine.State_Wander.SetValue(left, right);
        StateMachine.ChangeState(StateMachine.State_Wander);
    }

    private void SetState_OfferCoin()
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.OfferCoin;
        debugCurrentState = "OfferCoin";

        StateMachine.ChangeState(StateMachine.State_OfferCoin);
    }

    private void SetState_Attracted(float attractPosition)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Attracted;
        debugCurrentState = "Attracted";

        StateMachine.State_MoveTo.SetValue(attractPosition);
        StateMachine.ChangeState(StateMachine.State_MoveTo);
    }

    public override void SetState_BlownAway(bool isBlownLeft)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.BlownAway;
        debugCurrentState = "BlownAway";

        StateMachine.State_BlownAway.SetValue(isBlownLeft);
        StateMachine.ChangeState(StateMachine.State_BlownAway);
    }

    private void SetState_Flee(bool isGoLeft)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Flee;
        debugCurrentState = "Flee";

        StateMachine.State_Flee.SetValue(isGoLeft);
        StateMachine.ChangeState(StateMachine.State_Flee);

        fleeTimeRemain = NPC.ClassData.frightentime;
    }
}