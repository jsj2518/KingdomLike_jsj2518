using System;
using UnityEngine;

public class HostileNPC : MonoBehaviour, IDamageable
{
    static int globalOrderInLayer = 0;

    [field: SerializeField] public LayerMask TargetLayer { get; private set; }
    [field: SerializeField] public LayerMask RootLayer { get; private set; }
    [field: SerializeField] public LayerMask WallLayer { get; private set; }

    // 이벤트
    public event Action<HostileNPC> OnDead;
    public event Action<HostileNPC> OnEscaped;

    // 컴포넌트
    public BoxCollider2D HitCollider { get; private set; }

    // 오브젝트 속성
    public int orderInLayer { get; private set; }

    // 몬스터 속성
    public MonsterSO MonsterData { get; private set; }
    public HostileNPCAI AI { get; private set; }
    public bool CanHoldItem;

    // 현재 상태
    public bool IsLookLeft { get; private set; }
    public int CurrentHP { get; private set; }
    public bool IsHoldingItem { get; private set; }

    // 스폰 정보
    public float SpawnPosition { get; private set; }
    public bool RushToLeft { get; private set; }

    private void Awake()
    {
        HitCollider = GetComponent<BoxCollider2D>();

        orderInLayer = AppConstants.AllyNPCOrderinlayerStart + globalOrderInLayer;
        globalOrderInLayer += 2;//Hold Item을 위해 중간값 자리 남겨둠
        if (globalOrderInLayer >= AppConstants.AllyNPCOrderinlayerRange)
        {
            globalOrderInLayer = 0;
        }
    }

    public bool Initialize(string id)
    {
        string SOPath = $"ScriptableObjects/MonsterSO/{id}";
        MonsterData = ResourceManager.Instance.LoadAsset<MonsterSO>(SOPath);
        if (MonsterData == null) return false;

        string prefabPath = $"Prefabs/NPC/HostileNPC/{id}";
        GameObject PrefabAI = ResourceManager.Instance.LoadAsset<GameObject>(prefabPath);
        if (PrefabAI == null) return false;

        GameObject go = Instantiate(PrefabAI, transform);
        AI = go.GetComponent<HostileNPCAI>();
        if (AI == null) return false;
        AI.Initialize(this);

        gameObject.name = MonsterData.npcname;

        HitCollider.size = new Vector2(MonsterData.collidersizex, MonsterData.collidersizey);
        transform.position = new Vector3(transform.position.x, MonsterData.initialoffsety, transform.position.z);
        if (transform.localScale.x < 0)
        {
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        }

        IsLookLeft = false;
        CurrentHP = MonsterData.maxhp;
        IsHoldingItem = false;

        SetTargetTag(true);

        return true;
    }

    private void Update()
    {
        if (AI != null) AI.UpdateAI();
    }

    public void SetLookLeft(bool isLookLeft)
    {
        if (IsLookLeft != isLookLeft)
        {
            IsLookLeft = isLookLeft;
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        }
    }

    public GameObject DetectTarget()
    {
        // 현재 오브젝트의 위치를 중심으로 원 범위 내의 Collider2D 탐색
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, MonsterData.detectdistance, TargetLayer);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.CompareTag(Tags.T_CanBecomeTarget))
            {
                return collider.gameObject;
            }
        }

        return null;
    }
    public GameObject DetectTarget_Raycast()
    {
        float minDistance = float.MaxValue;
        int minDistanceIdx = -1;
        GameObject returnObject = null;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, IsLookLeft ? Vector2.left : Vector2.right, MonsterData.detectdistance, TargetLayer | WallLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.CompareTag(Tags.T_CanBecomeTarget))
            {
                if (hits[i].distance < minDistance)
                {
                    minDistance = hits[i].distance;
                    minDistanceIdx = i;
                }
            }
        }

        if (minDistanceIdx >= 0)
        {
            returnObject = hits[minDistanceIdx].collider.gameObject;
        }

        return returnObject;
    }
    public GameObject DetectItem()
    {
        // 현재 오브젝트의 위치를 중심으로 원 범위 내의 Collider2D 탐색
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, MonsterData.detectdistance, RootLayer);

        if (colliders.Length > 0)
        {
            return colliders[0].gameObject;
        }
        else
        {
            return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (AI.StateLock == false && collision.gameObject.IsLayerMatched(RootLayer))
        {
            if (CanHoldItem && !IsHoldingItem)
            {
                if (collision.gameObject.CompareTag(Tags.T_CanBecomeTarget) == false) return;

                if (collision.TryGetComponent<Coin>(out Coin coin))
                {
                    if (!coin.IsPickUpable(OwnerType.Hostile))
                        return;

                    AI.PlunderCoin(coin);
                    IsHoldingItem = true;
                }
                else if (collision.TryGetComponent<ToolObject>(out ToolObject tool))
                {
                    if(!tool.IsPickUpable())
                        return;
                    
                    AI.PlunderTool(tool);
                    IsHoldingItem = true;
                }
            }
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

    public bool IsTargetLost(GameObject target)
    {
        if (target == null)
        {
            return true;
        }

        if (target.activeInHierarchy == false
            || target.CompareTag(Tags.T_CanBecomeTarget) == false
            || Mathf.Abs(target.transform.position.x - transform.position.x) > MonsterData.targetlostdistance)
        {
            return true;
        }

        return false;
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
        if (AI == null) return;
        AI.AnimationController.TriggerHit();
    }
    private void Dead()
    {
        OnDead?.Invoke(this);
        SetTargetTag(false);
        AI.SetState_Dead();
    }

    public void Disable()
    {
        AI.Destroy();
        AI = null;
        Destroy(transform.GetChild(0).gameObject);

        MonsterData = null;
        gameObject.name = "Hostile";

        PoolManager.Instance.ReleaseComponent(gameObject, "HostileNPC");
    }

    public void CallEscapedEvent()
    {
        OnEscaped?.Invoke(this);
    }


    [ContextMenu("DebugFunc/Die")]
    private void DebugFunc1()
    {
        TakeDamage(1000, false);
    }
}
