using System;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class AllyNPC : MonoBehaviour, IDamageable
{
    static int globalOrderInLayer = 0;

    [field: SerializeField] public LayerMask InteractLayer { get; private set; }
    [field: SerializeField] public LayerMask MonsterLayer { get; private set; }
    [field: SerializeField] public LayerMask PreyLayer { get; private set; }
    [field: SerializeField] public LayerMask CoinLayer { get; private set; }
    [field: SerializeField] public LayerMask ToolLayer { get; private set; }
    [field: SerializeField] public GameObject DropCoinPrefab { get; private set; }
    [SerializeField] private ToolObject DropBowPrefab;
    [SerializeField] private ToolObject DropHammerPrefab;

    // 이벤트
    public event Action<AllyNPC> OnClassSpawn;
    public event Action<AllyNPC> OnClassDespawn;

    // 컴포넌트
    public BoxCollider2D HitCollider { get; private set; }

    // 오브젝트 속성
    public int orderInLayer { get; private set; }

    // NPC 속성
    public AllyNPCClass CurrentClass { get; private set; }
    public NpcSO ClassData { get; private set; }
    public AllyNPCAI AI { get; private set; }

    // 현재 상태
    public bool IsLookLeft { get; private set; }
    public int CurrentCoin { get; private set; }
    public int CurrentHP { get; private set; }
    public bool CanColliderDetect;

    private void Awake()
    {
        HitCollider = GetComponent<BoxCollider2D>();

        orderInLayer = AppConstants.AllyNPCOrderinlayerStart + globalOrderInLayer;
        globalOrderInLayer++;
        if (globalOrderInLayer >= AppConstants.AllyNPCOrderinlayerRange)
        {
            globalOrderInLayer = 0;
        }
    }

    // Vagrant Start
    public bool Initialize(string id)
    {
        if (transform.localScale.x < 0)
        {
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        }

        if (LoadClass(id) == false) return false;

        CanColliderDetect = true;

        return true;
    }

    private bool LoadClass(string id)
    {
        string SOPath = $"ScriptableObjects/NpcSO/{id}";
        ClassData = ResourceManager.Instance.LoadAsset<NpcSO>(SOPath);
        if (ClassData == null) return false;

        if (Enum.TryParse<AllyNPCClass>(ClassData.classname, out AllyNPCClass currentClass))
        {
            CurrentClass = currentClass;
        }
        else
        {
            return false;
        }

        string prefabPath = $"Prefabs/NPC/AllyNPC/{id}";
        GameObject PrefabAI = ResourceManager.Instance.LoadAsset<GameObject>(prefabPath);
        if (PrefabAI == null) return false;

        GameObject go = Instantiate(PrefabAI, transform);
        AI = go.GetComponent<AllyNPCAI>();
        if (AI == null) return false;
        AI.Initialize(this);

        gameObject.name = ClassData.npcname;
        HitCollider.size = new Vector2(ClassData.collidersizex, ClassData.collidersizey);
        transform.position = new Vector3(transform.position.x, ClassData.initialoffsety, transform.position.z);
        CurrentHP = ClassData.maxhp;

        OnClassSpawn?.Invoke(this);

        return true;
    }

    private void Update()
    {
        if (AI != null)
        {
            AI.UpdateAI();

            // TODO : 플레이어가 달리는 상태인지 확인
            if (false)
            {
                AI.IsInteractWithPlayer = false;
            }
        }
    }

    public void SetLookLeft(bool isLookLeft)
    {
        if (IsLookLeft != isLookLeft)
        {
            IsLookLeft = isLookLeft;
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (CanColliderDetect == false) return;

        if (collision.gameObject.IsLayerMatched(InteractLayer))
        {
            if (collision.gameObject.CompareTag(Tags.T_AllyInteraction))
            {
                // TODO : 플레이어가 달리는 상태인지 확인

                AI.IsInteractWithPlayer = true;
            }
        }
        else if (collision.gameObject.IsLayerMatched(CoinLayer))
        {
            if (collision.gameObject.CompareTag(Tags.T_CanBecomeTarget) == false) return;

            InteractWithCoin(collision);
        }
        else if (collision.gameObject.IsLayerMatched(ToolLayer))
        {
            if (collision.gameObject.CompareTag(Tags.T_CanBecomeTarget) == false) return;

            InteractWithTool(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.IsLayerMatched(InteractLayer))
        {
            if (AI != null && collision.gameObject.CompareTag(Tags.T_AllyInteraction))
            {
                AI.IsInteractWithPlayer = false;
            }
        }
    }


    private void InteractWithCoin(Collider2D collision)
    {
        if (CurrentCoin < ClassData.coincapacity)
        {
            if (!collision.TryGetComponent<Coin>(out Coin coin))
                return;

            bool groundCheck = true;
            if (CurrentClass == AllyNPCClass.Vagrant) groundCheck = false;
            if (!coin.IsPickUpable(OwnerType.Ally, groundCheck))
                return;

            CurrentCoin++;
            coin.GetCoin(transform);

            // 부랑자는 돈 먹으면 시민으로 업그레이드
            if (CurrentClass == AllyNPCClass.Vagrant)
            {
                Invoke("PromoteToVillager", 0.25f);
            }
        }
    }

    private void PromoteToVillager()
    {
        CurrentCoin = 0; // 소지금 다시 0으로 초기화
        ChangeClass(ClassData.promotionid[0]); // Villager Index는 0으로 고정
        SetTargetTag(true);
    }

    private void InteractWithTool(Collider2D collision)
    {
        if (CurrentClass != AllyNPCClass.Villager) return;

        if (collision.TryGetComponent<ToolObject>(out ToolObject tool))
        {
            if (!tool.IsPickUpable())
                return;



            switch (tool.Type)
            {
                case ToolType.Bow:
                    ChangeClass(ClassData.promotionid[(int)PromotionIDIndex.Archer]);
                    break;

                case ToolType.Hammer:
                    ChangeClass(ClassData.promotionid[(int)PromotionIDIndex.Worker]);
                    break;
            }


            tool.Acquire();
        }
    }

    private bool ChangeClass(string id)
    {
        OnClassDespawn?.Invoke(this);

        AI.Destroy();
        AI = null;

        Destroy(transform.GetChild(0).gameObject);

        HitCollider.enabled = false; // OnTriggerEnter2D 이벤트를 위한 처리

        if (LoadClass(id) == false) return false;

        HitCollider.enabled = true;  // OnTriggerEnter2D 이벤트를 위한 처리

        return true;
    }

    public void TakeDamage(int damage, bool isBlownLeft)
    {
        if (CurrentHP <= 0) return;

        CurrentHP -= damage;
        if (CurrentHP <= 0)
        {
            Dead(isBlownLeft);
        }
    }

    private void Dead(bool isBlownLeft)
    {
        SetTargetTag(false);
        CanColliderDetect = false;

        // 도구 놓침
        if (CurrentClass == AllyNPCClass.Archer)
        {
            ToolObject bow = Instantiate<ToolObject>(DropBowPrefab, transform.position, Quaternion.identity);
            bow.Drop();
        }
        else if (CurrentClass == AllyNPCClass.Worker)
        {
            ToolObject hammer = Instantiate<ToolObject>(DropHammerPrefab, transform.position, Quaternion.identity);
            hammer.Drop();
        }

        ChangeClass(ClassData.demotionid);

        // 동전 잃음
        int coinLose = 0;
        if (CurrentClass == AllyNPCClass.Vagrant)
        {
            CurrentCoin++;
            coinLose = CurrentCoin;
        }
        else
        {
            if (CurrentCoin > ClassData.coincapacity)
                coinLose = CurrentCoin - ClassData.coincapacity;
        }
        for (int i = 0; i < coinLose; i++)
        {
            DropCoin(i, coinLose);
        }

        AI.SetState_BlownAway(isBlownLeft);
    }

    private void DropCoin(int index, int total)
    {
        Coin coin = (Coin)PoolManager.Instance.GetComponentFromPool("Coin", transform.position, Quaternion.identity);

        if (coin != null)
        {
            CurrentCoin--;
            // total 작을 때 index간의 간격이 1에 가깝고, total 커지면 범위가 -5 ~ 5에 수렴
            float throwStrength = (float)(10 * index - 5 * total + 5) / (total + 10);
            coin.DropCoin(OwnerType.Ally, new Vector2(throwStrength, Random.Range(1f, 2f)));
        }
    }
    public void DropCoinToPlayer()
    {
        Coin coin = (Coin)PoolManager.Instance.GetComponentFromPool("Coin", transform.position, Quaternion.identity);
        if (coin != null)
        {
            CurrentCoin--;
            float throwStrength = Mathf.Clamp((StageManager.Instance.Player.transform.position.x - transform.position.x) * 1.5f, -3f, 3f);
            coin.DropCoin(OwnerType.Ally, new Vector2(throwStrength, 3f));
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

    /// <summary>
    /// noChase : 거리 비교를 target lost distance 대신 attack distance 사용
    /// </summary>
    public bool IsTargetLost(GameObject target, bool noChase = false)
    {
        if (target == null)
        {
            return true;
        }

        float targetLostDistance = ClassData.targetlostdistance;
        if (noChase)
        {
            targetLostDistance = ClassData.attackdistance;
        }

        if (target.activeInHierarchy == false
            || target.CompareTag(Tags.T_CanBecomeTarget) == false
            || Mathf.Abs(target.transform.position.x - transform.position.x) > targetLostDistance)
        {
            return true;
        }

        return false;
    }
    public bool IsAttractLost(GameObject target)
    {
        if (target == null)
        {
            return true;
        }

        if (target.activeInHierarchy == false
            || target.CompareTag(Tags.T_CanBecomeTarget) == false
            || Mathf.Abs(target.transform.position.x - transform.position.x) > ClassData.attractdetectdistance)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// noChase : 탐지 범위를 target detect distance 대신 attack distance 사용
    /// </summary>
    public GameObject DetectTargetEnemy(bool noChase = false)
    {
        float detectDistance = ClassData.targetdetectdistance;
        if (noChase)
        {
            detectDistance = ClassData.attackdistance;
        }

        // 현재 오브젝트의 위치를 중심으로 원 범위 내의 Collider2D 탐색
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectDistance, MonsterLayer);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.CompareTag(Tags.T_CanBecomeTarget))
            {
                return collider.gameObject;
            }
        }

        return null;
    }
    /// <summary>
    /// noChase : 탐지 범위를 target detect distance 대신 attack distance 사용
    /// </summary>
    public GameObject DetectTargetWithPrey(bool noChase = false)
    {
        float detectDistance = ClassData.targetdetectdistance;
        if (noChase)
        {
            detectDistance = ClassData.attackdistance;
        }

        // 현재 오브젝트의 위치를 중심으로 원 범위 내의 Collider2D 탐색
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectDistance, MonsterLayer | PreyLayer);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.CompareTag(Tags.T_CanBecomeTarget))
            {
                return collider.gameObject;
            }
        }

        return null;
    }
    public GameObject DetectCoin()
    {
        // 보유 코인이 가득 차면 detect fail
        if (CurrentCoin >= ClassData.coincapacity) return null;

        // 현재 오브젝트의 위치를 중심으로 원 범위 내의 Collider2D 탐색
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ClassData.attractdetectdistance, CoinLayer);

        if (colliders.Length > 0)
        {
            return colliders[0].gameObject;
        }
        else
        {
            return null;
        }
    }
    public GameObject DetectTool()
    {
        // 현재 오브젝트의 위치를 중심으로 원 범위 내의 Collider2D 탐색
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ClassData.attractdetectdistance, ToolLayer);

        if (colliders.Length > 0)
        {
            return colliders[0].gameObject;
        }
        else
        {
            return null;
        }
    }





    [ContextMenu("DebugFunc/Die")]
    private void DebugFunc1()
    {
        TakeDamage(1000, false);
    }
    [ContextMenu("DebugFunc/Promotion Villager to Explorer")]
    private void DebugFunc2()
    {
        if (CurrentClass != AllyNPCClass.Villager) return;
        ChangeClass(ClassData.promotionid[(int)PromotionIDIndex.Explorer]);
    }
}
