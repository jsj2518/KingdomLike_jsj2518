using UnityEditor;
using UnityEngine;

public abstract class HostileNPCAI : MonoBehaviour
{
    public HostileNPC NPC { get; protected set; }
    public HostileNPCAnimatorController AnimationController { get; protected set; }
    public bool StateLock;

    public abstract bool Initialize(HostileNPC hostileNPC);
    public abstract void Destroy();
    public abstract void UpdateAI();

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
    /// 지정된 위치 순찰, 스포너 위치 부여
    /// </summary>
    public abstract void SetCommand_Wander(float wanderBoundaryLeft, float wanderBoundaryRight, float spawnerPosition);
    /// <summary>
    /// 지정된 위치 공격, 스포너 위치 부여
    /// </summary>
    public abstract void SetCommand_Rush(float destination, float spawnerPosition);


    /// <summary>
    /// 도주 상태
    /// </summary>
    public virtual void SetState_Run() { }

    /// <summary>
    /// 사망 상태
    /// </summary>
    public abstract void SetState_Dead();

    /// <summary>
    /// 육탄공격 적중
    /// <para>Use For : Greedling</para> 
    /// </summary>
    public virtual void TackleAttackHit(GameObject target) { }


    /// <summary>
    /// 아이템 약탈
    /// <para>Use For : Greedling</para> 
    /// </summary>
    public virtual void PlunderCoin(Coin coin) { }
    public virtual void PlunderTool(ToolObject tool) { }
}
