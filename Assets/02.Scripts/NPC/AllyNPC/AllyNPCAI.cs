using System;
using UnityEditor;
using UnityEngine;


public abstract class AllyNPCAI : MonoBehaviour
{
    public AllyNPC NPC { get; protected set; }
    public AllyNPCAnimatorController AnimationController { get; protected set; }
    public bool StateLock;
    public bool IsInteractWithPlayer;

    // 디버그
    protected string debugCurrentState = "";
    protected Vector3 debugLabelOffset;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (NPC == null) return;
        Handles.Label(NPC.transform.position + debugLabelOffset, debugCurrentState, NPCDebug.GuiStyle);
    }
#endif

    /// <summary>
    /// 작업자 작업 위치 도착 후 작업 시작 시 발생
    /// <para>Use For : Worker</para> 
    /// </summary>
    public virtual event Action<AllyNPC> OnWorkStart;
    /// <summary>
    /// 작업자 작업 중지 시 발생
    /// <para>Use For : Worker</para> 
    /// </summary>
    public virtual event Action<AllyNPC> OnWorkStop;

    public abstract bool Initialize(AllyNPC allyNPC);

    /// <summary>
    /// 최적화를 위해 부모 오브젝트 Update에서 호출
    /// </summary>
    public abstract void UpdateAI();

    public abstract void Destroy();


    /// <summary>
    /// 기본 명령(배회) : 커맨드는 다른 작업이 없을 때 Idle State를 지정
    /// </summary>
    public abstract void SetCommand_Wander(float wanderBoundaryLeft, float wanderBoundaryRight);

    /// <summary>
    /// 작업 등록
    /// <para>Use For : Worker, Explorer</para> 
    /// </summary>
    public virtual void SetCommand_Work(float workBoundaryLeft, float workBoundaryRight) { }

    /// <summary>
    /// 지정된 위치 방어
    /// <para>Use For : Archer, Worker</para> 
    /// </summary>
    public virtual void SetCommand_DefensePosition(float defendBoundaryLeft, float defendBoundaryRight, bool standLeft, bool isRetreat) { }




    /// <summary>
    /// 강등 시 초기 애니메이션
    /// <para>Use For : Vagrant, Villager</para> 
    /// </summary>
    public virtual void SetState_BlownAway(bool isBlownLeft) { }
}