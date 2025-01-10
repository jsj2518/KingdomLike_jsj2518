using System;
using Unity.VisualScripting;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using Random = UnityEngine.Random;

public class AI_Explorer : AllyNPCAI
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;


    // 커맨드는 다른 작업이 없을 때 Idle State를 지정
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
    private bool commandVal_DefenseStandLeft;
    private bool commandVal_DefenseRetreat;

    private enum State
    {
        Wander,
        OfferCoin,
        MoveToWork,
        AttackTarget,
        DefendPosition
    }

    public override event Action<AllyNPC> OnWorkStart;

    private StateMachine_Explorer StateMachine;
    private State CurrentState;

    private float detectTime;
    private bool detectTick;
    private bool alreadyChangeState;
    private GameObject targetAttack;
    private bool workStarted;
    private const int pelletNum = 8;
    private Vector2[] shootingVector = new Vector2[pelletNum];

    public override bool Initialize(AllyNPC allyNPC)
    {
        NPC = allyNPC;

        spriteRenderer.sortingOrder = NPC.orderInLayer;
        AnimationController = new AllyNPCAnimatorController();
        AnimationController.Initialize(animator);
        StateMachine = new StateMachine_Explorer(NPC);

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

        if (workStarted) WorkEnd(); // 작업이 끝났을 경우
    }
    public override void SetCommand_Work(float left, float right)
    {
        CurrentCommand = Command.Work;
        commandVal_WorkLeft = left;
        commandVal_WorkRight = right;
    }
    public override void SetCommand_DefensePosition(float left, float right, bool standLeft, bool isRetreat)
    {
        CurrentCommand = Command.DefensePosition;
        commandVal_DefenseLeft = left;
        commandVal_DefenseRight = right;
        commandVal_DefenseStandLeft = standLeft;
        commandVal_DefenseRetreat = isRetreat;
        // 같은 DefensePosition Command일 때는 Command 덧씌움
        if (AppliedCommand == Command.DefensePosition)
        {
            AppliedCommand = Command.NONE;
        }
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
                SetState_MoveToWork((commandVal_WorkLeft + commandVal_WorkRight) / 2f);
                break;

            case Command.DefensePosition:
                SetState_DefensePosition(commandVal_DefenseLeft, commandVal_DefenseRight, commandVal_DefenseStandLeft);
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

            case State.MoveToWork:
                ExecuteState_GotoWork();
                break;

            case State.AttackTarget: // Lock Check
                ExecuteState_AttackTarget();
                break;

            case State.DefendPosition: // Lock Check
                ExecuteState_DefendPosition();
                break;
        }

        if (StateLock == false && workStarted == false)
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
            targetAttack = NPC.DetectTargetEnemy();
            if (targetAttack != null)
            {
                SetState_AttackTarget(targetAttack);
                return;
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
    private void ExecuteState_GotoWork()
    {
        if (workStarted == false)
        {
            float curretPosition = NPC.transform.position.x;
            if (commandVal_WorkLeft <= curretPosition && curretPosition <= commandVal_WorkRight)
            {
                WorkStart();
            }
        }
    }
    private void ExecuteState_AttackTarget() // Lock Check
    {
        if (NPC.IsTargetLost(targetAttack))
        {
            StateMachine.State_AttackTarget.SetTarget(null);

            if (StateLock == false)
            {
                ApplyCurrentCommand();
            }
        }
    }
    private void ExecuteState_DefendPosition() // Lock Check
    {
        // 후퇴중엔 타겟을 찾지 않음
        if (commandVal_DefenseRetreat && StateMachine.State_DefendPosition.IsMoving) return;

        if (NPC.IsTargetLost(targetAttack, noChase: true))
        {
            StateMachine.State_DefendPosition.SetTarget(null);

            if (StateLock == false)
            {
                if (detectTick)
                {
                    targetAttack = NPC.DetectTargetEnemy();
                    if (targetAttack != null)
                    {
                        StateMachine.State_DefendPosition.SetTarget(targetAttack);
                    }
                }
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
    }

    private void SetState_MoveToWork(float workPosition)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.MoveToWork;
        debugCurrentState = "MoveToWork";

        StateMachine.State_MoveTo.SetValue(workPosition);
        StateMachine.ChangeState(StateMachine.State_MoveTo);
    }

    private void SetState_AttackTarget(GameObject attckTarget)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.AttackTarget;
        debugCurrentState = "AttackTarget";

        StateMachine.State_AttackTarget.SetValue(attckTarget, MeasureShooting, Shoot);
        StateMachine.ChangeState(StateMachine.State_AttackTarget);
    }

    private void SetState_DefensePosition(float left, float right, bool standLeft)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.DefendPosition;
        debugCurrentState = "DefendPosition";

        StateMachine.State_DefendPosition.SetValue(Random.Range(left, right), standLeft, MeasureShooting, Shoot);
        StateMachine.ChangeState(StateMachine.State_DefendPosition, false);
    }


    private void WorkStart()
    {
        workStarted = true;

        NPC.HitCollider.enabled = false;
        NPC.SetTargetTag(false);
        spriteRenderer.enabled = false;

        OnWorkStart?.Invoke(NPC);
    }
    private void WorkEnd()
    {
        workStarted = false;

        NPC.HitCollider.enabled = true;
        NPC.SetTargetTag(true);
        spriteRenderer.enabled = true;
    }


    private void MeasureShooting()
    {
        if (targetAttack == null) return;

        float V = 12f;
        float vRange = 3f;
        float spread = 0.5f;
        float spreadIndividual = 0.1f;
        float x = targetAttack.transform.position.x - NPC.transform.position.x;

        float vRangeHalf = vRange / 2f;
        float spreadIndividualHalf = spreadIndividual / 2f;
        for (int i = 0; i < pelletNum; i++)
        {
            float v = V + Random.Range(-vRangeHalf, vRangeHalf);
            float theta = spread * ((float)i / (pelletNum - 1) - 0.5f) + Random.Range(-spreadIndividualHalf, spreadIndividualHalf);

            // 초기 속도의 x, y 성분 계산
            float Vx = v * Mathf.Cos(theta);
            float Vy = v * Mathf.Sin(theta);
            if (x < 0) Vx = -Vx;

            shootingVector[i] = new Vector3(Vx, Vy);
        }
    }
    private void Shoot()
    {
        for (int i = 0; i < pelletNum; i++)
        {
            Projectile projectile = (Projectile)PoolManager.Instance.GetComponentFromPool(NPC.ClassData.projectileid[0], NPC.transform.position, Quaternion.identity);
            if (projectile == null) return;
            projectile.Initialize(shootingVector[i]);
        }
    }





    [ContextMenu("DebugFunc/WorkStart")]
    private void DebugFunc1()
    {
        WorkStart();
    }
    [ContextMenu("DebugFunc/WorkEnd")]
    private void DebugFunc2()
    {
        WorkEnd();
    }
}
