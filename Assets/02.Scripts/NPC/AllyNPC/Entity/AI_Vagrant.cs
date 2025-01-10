using UnityEngine;

public class AI_Vagrant : AllyNPCAI
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
        Attracted,
        BlownAway,
        Crouch
    }


    private StateMachine_Vagrant StateMachine;
    private State CurrentState;

    private bool loop;
    private Coroutine activeCoroutine;

    private float detectTime;
    private bool detectTick;
    private bool alreadyChangeState;
    private GameObject targetMonster;
    private GameObject targetCoin;
    private float attractDelay;
    private float crouchTimeRemain;

    public override bool Initialize(AllyNPC allyNPC)
    {
        NPC = allyNPC;

        spriteRenderer.sortingOrder = NPC.orderInLayer;
        AnimationController = new AllyNPCAnimatorController();
        AnimationController.Initialize(animator);
        StateMachine = new StateMachine_Vagrant(NPC);

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

            case State.Attracted:
                ExecuteState_Attracted();
                break;

            case State.BlownAway: // Lock Check
                ExecuteState_BlownAway();
                break;

            case State.Crouch:
                ExecuteState_Crouch();
                break;
        }

        if (StateLock == false)
        {
            if (AppliedCommand != CurrentCommand)
            {
                ApplyCurrentCommand();
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
                SetState_Crouch();
            }

            if (NPC.IsAttractLost(targetCoin))
            {
                targetCoin = NPC.DetectCoin();

                attractDelay = NPC.ClassData.attractmovedelay;
            }
            else
            {
                if (attractDelay < 0)
                {
                    SetState_Attracted(targetCoin.transform.position.x);
                }
            }
        }
    }
    private void ExecuteState_Attracted()
    {
        if (NPC.IsAttractLost(targetCoin))
        {
            targetCoin = null;

            ApplyCurrentCommand();
        }
    }
    private void ExecuteState_BlownAway() // Lock Check
    {
        if (StateLock == false)
        {
            NPC.CanColliderDetect = true;

            ApplyCurrentCommand();
        }
    }
    private void ExecuteState_Crouch()
    {
        crouchTimeRemain -= Time.deltaTime;
        if (crouchTimeRemain < 0 && NPC.IsTargetLost(targetMonster))
        {
            targetMonster = NPC.DetectTargetEnemy();
            if (targetMonster == null)
            {
                ApplyCurrentCommand();
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

    private void SetState_Crouch()
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Crouch;
        debugCurrentState = "Crouch";

        StateMachine.ChangeState(StateMachine.State_Crouch);

        crouchTimeRemain = NPC.ClassData.frightentime;
    }
}
