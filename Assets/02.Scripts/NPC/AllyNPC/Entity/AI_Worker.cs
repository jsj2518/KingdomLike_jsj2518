using System;
using UnityEngine;

public class AI_Worker : AllyNPCAI
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;


    private enum Command
    {
        NONE,
        Wander,
        Work,
        DefensePosition
    }
    private Command CurrentCommand;
    private Command AppliedCommand;
    private float commandVal_WanderLeft;
    private float commandVal_WanderRight;
    private float commandVal_WorkLeft;
    private float commandVal_WorkRight;
    private float commandVal_DefenseLeft;
    private float commandVal_DefenseRight;


    private enum State
    {
        Wander,
        OfferCoin,
        PrepareDefence,
        Work,
        Flee
    }

    public override event Action<AllyNPC> OnWorkStart;
    public override event Action<AllyNPC> OnWorkStop;

    private StateMachine_Worker StateMachine;
    private State CurrentState;

    private float detectTime;
    private bool detectTick;
    private bool alreadyChangeState;
    private bool workStarted;
    private float fleeTimeRemain;

    public override bool Initialize(AllyNPC allyNPC)
    {
        NPC = allyNPC;

        spriteRenderer.sortingOrder = NPC.orderInLayer;
        AnimationController = new AllyNPCAnimatorController();
        AnimationController.Initialize(animator);
        StateMachine = new StateMachine_Worker(NPC);

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

        workStarted = false; // 작업이 끝났을 경우
    }
    public override void SetCommand_Work(float left, float right)
    {
        CurrentCommand = Command.Work;
        commandVal_WorkLeft = left;
        commandVal_WorkRight = right;

        // CurrentCommand가 1프레임 이내에 2번 바뀌어 Command.Work에서 다시 Command.Work가 되는 경우 기존 작업을 완료하지 않을 수 있음. 따라서 Command.Work로 덧씌울 수 있도록 변경
        if (AppliedCommand == Command.Work) AppliedCommand = Command.NONE;
    }
    public override void SetCommand_DefensePosition(float left, float right, bool standLeft, bool isRetreat)
    {
        CurrentCommand = Command.DefensePosition;
        commandVal_DefenseLeft = left;
        commandVal_DefenseRight = right;
    }
    private void ApplyCurrentCommand()
    {
        AppliedCommand = CurrentCommand;

        switch (AppliedCommand)
        {
            case Command.Wander:
                SetState_Wander(commandVal_WanderLeft, commandVal_WanderRight);
                break;

            case Command.Work:
                SetState_Work(commandVal_WanderLeft, commandVal_WanderRight);
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

        switch (CurrentState)
        {
            case State.Wander:
                ExecuteState_Wander();
                break;

            case State.OfferCoin: // Lock Check
                ExecuteState_OfferCoin();
                break;

            case State.PrepareDefence:
                ExecuteState_PrepareDefence();
                break;

            case State.Work:
                ExecuteState_Work();
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
        DetectMonsterAndSetFlee();
    }
    private void ExecuteState_OfferCoin() // Lock Check
    {
        if (StateLock == false && (IsInteractWithPlayer == false || NPC.CurrentCoin <= 0))
        {
            ApplyCurrentCommand();
        }
    }
    private void ExecuteState_PrepareDefence()
    {
        DetectMonsterAndSetFlee();
    }
    private void ExecuteState_Work()
    {
        DetectMonsterAndSetFlee();
    }
    private void ExecuteState_Flee()
    {
        fleeTimeRemain -= Time.deltaTime;
        if (fleeTimeRemain < 0)
        {
            ApplyCurrentCommand();
        }
    }

    private void DetectMonsterAndSetFlee()
    {
        if (detectTick)
        {
            GameObject targetMonster = NPC.DetectTargetEnemy();
            if (targetMonster != null)
            {
                bool isGoLeft = NPC.transform.position.x < targetMonster.transform.position.x;
                SetState_Flee(isGoLeft);
            }
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

        if (workStarted)
        {
            workStarted = false;
            OnWorkStop?.Invoke(NPC);
        }
    }

    private void SetState_PrepareDefence(float attractPosition)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.PrepareDefence;
        debugCurrentState = "PrepareDefence";

        StateMachine.State_MoveTo.SetValue(attractPosition);
        StateMachine.ChangeState(StateMachine.State_MoveTo);
    }

    private void SetState_Work(float workBoundaryLeft, float workBoundaryRight)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Work;
        debugCurrentState = "Work";

        StateMachine.State_Work.SetValue(commandVal_WorkLeft, commandVal_WorkRight, WorkStart);
        StateMachine.ChangeState(StateMachine.State_Work, false);
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

        if (workStarted)
        {
            workStarted = false;
            OnWorkStop?.Invoke(NPC);
        }
    }


    private void WorkStart()
    {
        workStarted = true;
        OnWorkStart?.Invoke(NPC);
    }
}
