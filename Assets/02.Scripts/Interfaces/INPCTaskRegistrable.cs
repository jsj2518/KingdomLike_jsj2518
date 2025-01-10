using System;
using UnityEngine;

public interface INPCTaskRegistrable
{
    enum Type
    {
        Worker,
        Explorer
    }
    /// <summary>
    /// 호출 NPC 타입
    /// </summary>
    Type NPCType { get; }

    /// <summary>
    /// 최대로 호출 가능한 NPC 수
    /// </summary>
    int NPCCountMax { get; }
    /// <summary>
    /// 영역의 왼쪽 x 좌표
    /// </summary>
    float BoundaryLeft { get; }
    /// <summary>
    /// 영역의 오른쪽 x 좌표
    /// </summary>
    float BoundaryRight { get; }

    /// <summary>
    /// 작업 스케쥴러에 등록
    /// </summary>
    event Action<INPCTaskRegistrable> OnTaskRegist;
    /// <summary>
    /// 작업 스케쥴러에서 제외(NPC 모집 종료)
    /// </summary>
    event Action<INPCTaskRegistrable> OnTaskStart;
    /// <summary>
    /// 작업 스케쥴러에서 제외(완료 시)
    /// </summary>
    event Action<INPCTaskRegistrable> OnTaskComplete;

    /// <summary>
    /// 작업중인 NPC 증가됨
    /// </summary>
    void IncreaseNPC();
    /// <summary>
    /// 작업중인 NPC 감소됨
    /// </summary>
    void DecreaseNPC();
}
