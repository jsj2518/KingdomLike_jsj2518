using UnityEngine;

public class AI_Grredling : HostileNPCAI
{
    [SerializeField] private BoxCollider2D hitCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;


    // 커맨드는 다른 작업이 없을 때 Idle State를 지정
    private enum Command
    {
        NONE,
        Patrol,
        Rush
    }
    private Command CurrentCommand;
    private Command AppliedCommand;
    private float spawnVal_SpawnerPosition;
    private float commandVal_PatrolLeft;
    private float commandVal_PatrolRight;
    private float commandVal_RushDestination;


    private enum State
    {
        Wander,
        Rush,
        AttackTarget,
        Run,
        Dead
    }

    private StateMachine_Grredling StateMachine;
    private State CurrentState;

    [SerializeField] private Transform HoldItemPivot;
    [SerializeField] private ToolObject DropBowPrefab;
    [SerializeField] private ToolObject DropHammerPrefab;

    private float detectTime;
    private bool detectTick;
    private bool alreadyChangeState;
    private GameObject targetAttack;
    private bool tackleHitIgnore; // 태클 공격 1회만 적중
    private HostileHoldItemType plunderItem = HostileHoldItemType.NONE;

    public override bool Initialize(HostileNPC HostileNPC)
    {
        NPC = HostileNPC;
        NPC.CanHoldItem = true;

        AnimationController = new HostileNPCAnimatorController();
        AnimationController.Initialize(animator);

        spriteRenderer.sortingOrder = NPC.orderInLayer;
        StateMachine = new StateMachine_Grredling(NPC);

        CurrentCommand = Command.NONE;
        AppliedCommand = Command.NONE;

        debugLabelOffset = new Vector3(0, NPC.MonsterData.initialoffsety + 0.5f, 0);

        return true;
    }

    public override void Destroy()
    {
        // 약탈 아이템 제거
        PlunderRemove();

        NPC = null;
        AnimationController = null;
        StateMachine = null;
    }

    public override void SetCommand_Wander(float left, float right, float spawnerPosition)
    {
        spawnVal_SpawnerPosition = spawnerPosition;

        CurrentCommand = Command.Patrol;
        commandVal_PatrolLeft = left;
        commandVal_PatrolRight = right;
        StateMachine.State_Wander.SetValue(commandVal_PatrolLeft, commandVal_PatrolRight);
    }
    public override void SetCommand_Rush(float destination, float spawnerPosition)
    {
        spawnVal_SpawnerPosition = spawnerPosition;

        CurrentCommand = Command.Rush;
        commandVal_RushDestination = destination;
    }
    private void ApplyCurrentCommand()
    {
        AppliedCommand = CurrentCommand;

        switch (AppliedCommand)
        {
            case Command.Patrol:
                SetState_Wander(commandVal_PatrolLeft, commandVal_PatrolRight);
                break;

            case Command.Rush:
                SetState_Rush(commandVal_RushDestination);
                break;
        }
    }


    public override void UpdateAI()
    {
        alreadyChangeState = false;

        detectTime -= Time.deltaTime;
        if (detectTime < 0)
        {
            detectTime = NPC.MonsterData.detectinterval;
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

            case State.Rush:
                ExecuteState_Rush();
                break;

            case State.AttackTarget: // Lock Check
                ExecuteState_AttackTarget();
                break;

            case State.Run:
                ExecuteState_Run();
                break;

            case State.Dead: // Lock Check
                ExecuteState_Dead();
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
            targetAttack = NPC.DetectTarget_Raycast();
            if (targetAttack != null)
            {
                SetState_AttackTarget(targetAttack);
                return;
            }
        }
    }
    private void ExecuteState_Rush()
    {
        if (detectTick)
        {
            targetAttack = NPC.DetectTarget_Raycast();
            if (targetAttack != null)
            {
                SetState_AttackTarget(targetAttack);
                return;
            }
        }
    }
    private void ExecuteState_AttackTarget() // Lock Check
    {
        if (NPC.IsTargetLost(targetAttack))
        {
            targetAttack = null;
            StateMachine.State_AttackTargetTackle.SetTarget(targetAttack);

            if (StateLock == false)
                ApplyCurrentCommand();
        }
        else if (StateLock == false && detectTick == true)
        {
            GameObject targetUnit;
            // 벽을 때리다가 유닛이 벽 밖으로 나오면 해당 유닛을 공격대상으로 변경
            if (targetAttack.IsLayerMatched(NPC.WallLayer))
            {
                targetUnit = NPC.DetectTarget_Raycast();
                if (targetUnit != null)
                {
                    if (targetUnit.IsLayerMatched(NPC.TargetLayer))
                    {
                        targetAttack = targetUnit;
                        StateMachine.State_AttackTargetTackle.SetTarget(targetAttack);
                    }
                }
            }
            // 유닛을 쫓다가 유닛이 벽 안으로 들어가면 해당 벽을 공격대상으로 변경
            else
            {
                targetUnit = NPC.DetectTarget_Raycast();
                if (targetUnit != null)
                {
                    if (targetUnit.IsLayerMatched(NPC.WallLayer))
                    {
                        targetAttack = targetUnit;
                        StateMachine.State_AttackTargetTackle.SetTarget(targetAttack);
                    }
                }
            }
        }
    }
    private void ExecuteState_Run()
    {
        float curretPosition = NPC.transform.position.x;

        if (spawnVal_SpawnerPosition - 0.1f <= curretPosition && curretPosition <= spawnVal_SpawnerPosition + 0.1f)
        {
            NPC.CallEscapedEvent();
            NPC.Disable();
        }
    }
    private void ExecuteState_Dead() // Lock Check
    {
        if (StateLock == false)
        {
            NPC.Disable();
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

    private void SetState_Rush(float destination)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Rush;
        debugCurrentState = "Rush";

        StateMachine.State_MoveTo.SetValue(destination, false);
        StateMachine.ChangeState(StateMachine.State_MoveTo, false);
    }

    private void SetState_AttackTarget(GameObject attckTarget)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.AttackTarget;
        debugCurrentState = "AttackTarget";

        StateMachine.State_AttackTargetTackle.SetValue(attckTarget, TackleStart, TackleEnd);
        StateMachine.ChangeState(StateMachine.State_AttackTargetTackle);
    }

    public override void SetState_Run()
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Run;
        debugCurrentState = "Run";

        StateMachine.State_MoveTo.SetValue(spawnVal_SpawnerPosition, true);
        StateMachine.ChangeState(StateMachine.State_MoveTo, false);
    }

    public override void SetState_Dead()
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        // 약탈한 아이템 뱉기
        switch (plunderItem)
        {
            case HostileHoldItemType.Coin:
                Coin coin = (Coin)PoolManager.Instance.GetComponentFromPool("Coin", HoldItemPivot.position, Quaternion.identity);
                if (coin != null)
                {
                    coin.DropCoin();
                }
                break;

            case HostileHoldItemType.Bow:
                ToolObject bow = Instantiate<ToolObject>(DropBowPrefab, HoldItemPivot.position, Quaternion.identity);
                bow.Drop();
                break;

            case HostileHoldItemType.Hammer:
                ToolObject hammer = Instantiate<ToolObject>(DropHammerPrefab, HoldItemPivot.position, Quaternion.identity);
                hammer.Drop();
                break;
        }

        // 약탈 아이템 제거
        PlunderRemove();

        CurrentState = State.Dead;
        debugCurrentState = "Dead";

        StateMachine.ChangeState(StateMachine.State_Dead);
    }


    private void TackleStart()
    {
        tackleHitIgnore = false;
        hitCollider.enabled = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 하나의 대상에게만 피해를 입힘
        if (tackleHitIgnore) return;

        if (collision.gameObject.IsLayerMatched(NPC.TargetLayer))
        {
            if (collision.gameObject.CompareTag(Tags.T_CanBecomeTarget) == false) return;

            TackleAttackHit(collision.gameObject);
        }
        else if (collision.gameObject.IsLayerMatched(NPC.WallLayer))
        {
            if (collision.gameObject.CompareTag(Tags.T_CanBecomeTarget) == false) return;

            TackleAttackHit(collision.gameObject);
        }
    }
    public override void TackleAttackHit(GameObject target)
    {
        if (target.TryGetComponent<IDamageable>(out IDamageable health))
        {
            tackleHitIgnore = true;
            health?.TakeDamage(NPC.MonsterData.attackpower, NPC.IsLookLeft);
        }
    }
    private void TackleEnd()
    {
        hitCollider.enabled = false;
    }


    public override void PlunderCoin(Coin coin)
    {
        plunderItem = HostileHoldItemType.Coin;
        Transform fakeCoin = Instantiate(ResourceManager.Instance.LoadAsset<Transform>(AppConstants.FakeItemPath_Coin));
        fakeCoin.parent = HoldItemPivot;
        fakeCoin.localPosition = Vector3.zero;
        if (fakeCoin.TryGetComponent<SpriteRenderer>(out SpriteRenderer skin))
        {
            skin.sortingOrder = NPC.orderInLayer + 1; //Hold Item은 Monster보다 위에 그려짐
        }

        PoolManager.Instance.ReleaseComponent(coin.gameObject, "Coin");

        SetState_Run();
    }
    public override void PlunderTool(ToolObject tool)
    {
        Transform fakeTool = null;

        switch (tool.Type)
        {
            case ToolType.Bow:
                plunderItem = HostileHoldItemType.Bow;
                fakeTool = Instantiate(ResourceManager.Instance.LoadAsset<Transform>(AppConstants.FakeItemPath_Bow));
                break;

            case ToolType.Hammer:
                plunderItem = HostileHoldItemType.Hammer;
                fakeTool = Instantiate(ResourceManager.Instance.LoadAsset<Transform>(AppConstants.FakeItemPath_Hammer));
                break;
        }
        if (fakeTool == null) return;

        fakeTool.parent = HoldItemPivot;
        fakeTool.localPosition = Vector3.zero;
        if (fakeTool.TryGetComponent<SpriteRenderer>(out SpriteRenderer skin))
        {
            skin.sortingOrder = NPC.orderInLayer + 1; //Hold Item은 Monster보다 위에 그려짐
        }

        tool.Acquire();

        SetState_Run();
    }
    private void PlunderRemove()
    {
        foreach (Transform child in HoldItemPivot)
        {
            Destroy(child.gameObject);
        }

        plunderItem = HostileHoldItemType.NONE;
    }
}
