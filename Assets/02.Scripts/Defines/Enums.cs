/// <summary>
/// Enum들을 모아두는 곳 입니다.
/// 내부 클래스가 아닌 외부 클래스에 enum 선언 후 사용.
/// </summary>
public class Enums
{
    
}

// 예시용 Enum
public enum Example
{
    None,
    Test,
    Example,
    Count 
}

// 아군 NPC 직업 분류
public enum AllyNPCClass
{
    NONE = -1,
    Vagrant,
    Villager,
    Archer,
    Worker,
    Explorer
}

// 시민이 승급 시 불러올 NPC ID List의 직업 인덱스
public enum PromotionIDIndex
{
    Archer,
    Worker,
    Explorer
}

// 몬스터가 들고 있는 아이템 타입
public enum HostileHoldItemType
{
    NONE,
    Coin,
    Bow,
    Hammer
}
