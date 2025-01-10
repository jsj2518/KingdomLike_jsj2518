using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PreyNPC : MonoBehaviour, IDamageable
{
    static int globalOrderInLayer = 0;

    [field: SerializeField] public LayerMask AvoidLayer { get; private set; }

    // 이벤트
    public event Action<PreyNPC> OnDead;

    // 컴포넌트
    [SerializeField] private BoxCollider2D HitCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public PreyNPCAnimatorController AnimationController { get; protected set; }

    // 사냥감 속성
    [field: SerializeField] public PreySO PreyData { get; private set; }
    [field: SerializeField] public PreyNPCAI AI { get; private set; }

    // 현재 상태
    public bool IsLookLeft { get; private set; }
    public int CurrentHP { get; private set; }

    // 스폰 정보
    public float SpawnPosition { get; private set; }

    private void Awake()
    {
        HitCollider = GetComponent<BoxCollider2D>();
        AnimationController = new PreyNPCAnimatorController();
        AnimationController.Initialize(animator);

        spriteRenderer.sortingOrder = AppConstants.AllyNPCOrderinlayerStart + globalOrderInLayer;
        globalOrderInLayer++;
        if (globalOrderInLayer >= AppConstants.AllyNPCOrderinlayerRange)
        {
            globalOrderInLayer = 0;
        }

        AI.AwakeAI(this);
    }

    private void OnEnable()
    {
        gameObject.name = PreyData.npcname;

        IsLookLeft = false;
        CurrentHP = PreyData.maxhp;

        AI.Initialize();

        transform.position = new Vector3(transform.position.x, PreyData.initialoffsety, transform.position.z);
        if (transform.localScale.x < 0)
        {
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        }

        SetTargetTag(true);
    }

    private void Update()
    {
        if (AI != null) AI.UpdateAI();
    }

    private void OnDisable()
    {
        gameObject.name = PreyData.id;
    }

    public void SetLookLeft(bool isLookLeft)
    {
        if (IsLookLeft != isLookLeft)
        {
            IsLookLeft = isLookLeft;
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        }
    }

    /// <summary>
    /// 태그는 AI의 대상 지정 여부를 결정함
    /// </summary>
    public void SetTargetTag(bool enable)
    {
        if (enable) gameObject.tag = Tags.T_CanBecomeTarget;
        else gameObject.tag = Tags.T_Untagged;
    }

    public void TakeDamage(int damage, bool isBlownLeft)
    {
        if (CurrentHP <= 0) return;

        CurrentHP -= damage;
        if (CurrentHP <= 0)
        {
            Dead();
        }
        else
        {
            Hit();
        }
    }

    private void Hit()
    {
        AnimationController.TriggerHit();
    }
    private void Dead()
    {
        OnDead?.Invoke(this);
        SetTargetTag(false);
        AI.SetState_Dead();
        for (int i = 0; i < PreyData.dropcoin; i++)
        {
            DropCoin(i, PreyData.dropcoin);
        }
    }


    private void DropCoin(int index, int total)
    {
        Coin coin = (Coin)PoolManager.Instance.GetComponentFromPool("Coin", transform.position, Quaternion.identity);

        if (coin != null)
        {
            // total 작을 때 index간의 간격이 1에 가깝고, total 커지면 범위가 -5 ~ 5에 수렴
            float throwStrength = (float)(10 * index - 5 * total + 5) / (total + 10);
            float range = 2f / (total + 10);
            coin.DropCoin(new Vector2(throwStrength + Random.Range(-range, range), 2.5f));
        }
    }


    [ContextMenu("DebugFunc/TakeDamage")]
    private void DebugFunc2()
    {
        TakeDamage(1, false);
    }
    [ContextMenu("DebugFunc/Die")]
    private void DebugFunc1()
    {
        TakeDamage(1000, false);
    }
}
