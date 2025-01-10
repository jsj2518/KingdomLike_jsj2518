/// <summary>
/// 상수 데이터를 모아둔 클래스 입니다. 
/// </summary>

public static class AppConstants
{
    
    //TODO / 없는 상태로 통일하기 ! !! ! ! ! 
    public const string StageSoPath = "ScriptableObjects/StageSO";
    public const string MissionSoPath = "ScriptableObjects/MissionSO";
    public const string SMissionSoPath = "ScriptableObjects/MissionSO/SMissionSO";
    public const string ArchitectureSoPath = "ScriptableObjects/ArchitectureSO";
    public const string ArchitectureSpritesPath = "Sprites/Architecture";
    public const string KeySpritesPath = "Sprites/Key";
    public const string DialogueSoPath = "ScriptableObjects/DialogueSO";

    
    
    public const string CoinPath = "Stage/Coin/ItemCoin";
    public const int DataIDLength = 5;
    public const string CanBecomeTargetTag = "CanBecomeTarget";

    //NPC Prefab Path
    public const string HostilePrefabPath =  "Prefabs/NPC/HostileNPC";
    public const string AllyPrefabPath =  "Prefabs/NPC/AllyNPC";
    public const string PreyPrefabPath =  "Prefabs/NPC/PreyNPC";


    // Fake Item Prefab Path for Hostile NPC
    public const string FakeItemPath_Coin = "Stage/FakeItem/FakeCoin";
    public const string FakeItemPath_Bow = "Stage/FakeItem/FakeBow";
    public const string FakeItemPath_Hammer = "Stage/FakeItem/FakeHammer";

    // Object OrderInLayer Range
    public const int AllyNPCOrderinlayerStart = 200;
    public const int AllyNPCOrderinlayerRange = 100;
    public const int HostileNPCOrderinlayerStart = 300;
    public const int HostileNPCOrderinlayerRange = 100;
    public const int PreyNPCOrderinlayerStart = 0;
    public const int PreyNPCOrderinlayerRange = 100;

    // MapBoundary Gap Distance for AI
    public const float BaseWanderInner = 3f;
    public const float BasePatrolInner = 2f;
    public const float BasePatrolOuter = 10f;
    public const float BaseDefenseOuter = 1.5f;
    public const float BaseDefenseInner = 2.5f;
    
    
    // Architecture Data 
    /// <summary>
    /// 건설 Interval당 진행률 증가량
    /// </summary>
    public const float BuildProgressPerInterval = 10f;
    /// <summary>
    /// 수리 Interval당 진행률 증가량 
    /// </summary>
    public const int RepairProgressPerInterval = 5;
    /// <summary>
    /// Sprite가 변하는 손상량 설정 
    /// </summary>
    public const float ChangeSpriteAtDamageRatio = 0.5f;
    
    
    //Coin Data
    /// <summary>
    /// 코인이 Owner에게 이동 시작하기 전 지연 시간
    /// </summary>
    public const float CoinMoveToTargetDuration = 0.1f;
    public const float GroundDetectLength = 0.35f;

    
}
