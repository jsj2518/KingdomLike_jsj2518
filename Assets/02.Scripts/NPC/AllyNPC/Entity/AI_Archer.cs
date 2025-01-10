using UnityEngine;
using Random = UnityEngine.Random;

public class AI_Archer : AllyNPCAI
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;


    // 커맨드는 다른 작업이 없을 때 Idle State를 지정
    private enum Command
    {
        NONE,
        Wander,
        DefensePosition
    }
    private Command CurrentCommand;
    private Command AppliedCommand;
    private float commandVal_WanderLeft;
    private float commandVal_WanderRight;
    private float commandVal_DefenseLeft;
    private float commandVal_DefenseRight;
    private bool commandVal_DefenseStandLeft;
    private bool commandVal_DefenseRetreat;

    private enum State
    {
        Wander,
        OfferCoin,
        Moveto,
        AttackTarget,
        Attracted,
        DefendPosition
    }


    private StateMachine_Archer StateMachine;
    private State CurrentState;

    private float detectTime;
    private bool detectTick;
    private bool alreadyChangeState;
    private GameObject targetAttack;
    private GameObject targetCoin;
    private float attractDelay;
    private Vector2 shootingVector;

    public override bool Initialize(AllyNPC allyNPC)
    {
        NPC = allyNPC;

        spriteRenderer.sortingOrder = NPC.orderInLayer;
        AnimationController = new AllyNPCAnimatorController();
        AnimationController.Initialize(animator);
        StateMachine = new StateMachine_Archer(NPC);

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
        // 같은 Wander Command에 이동중일 때는 Command 덧씌움
        if (AppliedCommand == Command.Wander
            && (CurrentState == State.Moveto || CurrentState == State.Wander))
        {
            AppliedCommand = Command.NONE;
        }
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
                float curretPosition = NPC.transform.position.x;
                if (curretPosition < commandVal_WanderLeft)
                {
                    SetState_MoveTo(commandVal_WanderLeft + 0.1f);
                }
                else if (curretPosition > commandVal_WanderRight)
                {
                    SetState_MoveTo(commandVal_WanderRight - 0.1f);
                }
                else
                {
                    SetState_Wander(commandVal_WanderLeft, commandVal_WanderRight);
                }
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
        attractDelay -= Time.deltaTime;

        switch (CurrentState)
        {
            case State.Wander:
                ExecuteState_Wander();
                break;

            case State.OfferCoin: // Lock Check
                ExecuteState_OfferCoin();
                break;

            case State.Moveto:
                ExecuteState_MoveTo();
                break;

            case State.AttackTarget: // Lock Check
                ExecuteState_AttackTarget();
                break;

            case State.Attracted:
                ExecuteState_Attracted();
                break;

            case State.DefendPosition: // Lock Check
                ExecuteState_DefendPosition();
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
            targetAttack = NPC.DetectTargetWithPrey();
            if (targetAttack != null)
            {
                SetState_AttackTarget(targetAttack);
                return;
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
    private void ExecuteState_OfferCoin() // Lock Check
    {
        if (StateLock == false && (IsInteractWithPlayer == false || NPC.CurrentCoin <= 0))
        {
            ApplyCurrentCommand();
        }
    }
    private void ExecuteState_MoveTo()
    {
        if (detectTick)
        {
            targetAttack = NPC.DetectTargetWithPrey();
            if (targetAttack != null)
            {
                SetState_AttackTarget(targetAttack);
                return;
            }
        }
        
        float curretPosition = NPC.transform.position.x;
        if (commandVal_WanderLeft <= curretPosition && curretPosition <= commandVal_WanderRight)
        {
            ApplyCurrentCommand();
        }
    }
    private void ExecuteState_AttackTarget() // Lock Check
    {
        // 사냥감보다 몬스터를 공격 우선순위로 둠, 밤에는 사냥감을 추적하지 않음
        bool IsTargetLost;
        if (targetAttack != null && targetAttack.IsLayerMatched(NPC.PreyLayer))
        {
            if (detectTick)
            {
                GameObject targetEnemy = NPC.DetectTargetEnemy();
                if (targetEnemy != null)
                {
                    targetAttack = targetEnemy;
                    StateMachine.State_AttackTarget.SetTarget(targetAttack);
                    return;
                }
            }

            IsTargetLost = NPC.IsTargetLost(targetAttack, noChase:ArcherNPCScheduler.IsDefenseTime);
        }
        else
        {
            IsTargetLost = NPC.IsTargetLost(targetAttack);
        }

        if (IsTargetLost)
        {
            StateMachine.State_AttackTarget.SetTarget(null);

            if (StateLock == false)
            {
                ApplyCurrentCommand();
            }
        }
    }
    private void ExecuteState_Attracted()
    {
        if (NPC.IsAttractLost(targetCoin))
        {
            ApplyCurrentCommand();
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
                    targetAttack = NPC.DetectTargetWithPrey(noChase: true);
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

    private void SetState_AttackTarget(GameObject attckTarget)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.AttackTarget;
        debugCurrentState = "AttackTarget";

        StateMachine.State_AttackTarget.SetValue(attckTarget, MeasureShooting, Shoot);
        StateMachine.ChangeState(StateMachine.State_AttackTarget);
    }

    private void SetState_MoveTo(float destination)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Moveto;
        debugCurrentState = "Moveto";

        StateMachine.State_MoveTo.SetValue(destination);
        StateMachine.ChangeState(StateMachine.State_MoveTo, false);
    }

    private void SetState_Attracted(float attractPosition)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Attracted;
        debugCurrentState = "Attracted";

        StateMachine.State_MoveTo.SetValue(attractPosition);
        StateMachine.ChangeState(StateMachine.State_MoveTo, false);
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


    private void MeasureShooting()
    {
        if (targetAttack == null) return;

        //float v = 10f;
        float x = targetAttack.transform.position.x - NPC.transform.position.x;
        float y = targetAttack.transform.position.y - NPC.transform.position.y;
        float g = 9.8f;

        float vx = x > 0 ? 300f : -300f;
        vx /= (Mathf.Abs(x) + 20);
        float t = x != 0 ? x / vx : 0.01f;
        float vy = y / t + g * t / 2f;

        shootingVector = new Vector2(vx, vy);
    }
    private void Shoot()
    {
        Projectile projectile = (Projectile)PoolManager.Instance.GetComponentFromPool(NPC.ClassData.projectileid[0], NPC.transform.position, Quaternion.identity);
        if (projectile == null) return;
        projectile.Initialize(shootingVector);
    }
}
