using UnityEditor;
using UnityEngine;

public abstract class PreyNPCAI : MonoBehaviour
{
    public PreyNPC NPC { get; protected set; }
    [HideInInspector] public bool StateLock;

    public abstract void AwakeAI(PreyNPC preyNPC);
    public abstract void Initialize();
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
    /// 지정된 위치 배회
    /// </summary>
    public abstract void SetCommand_Wander(float wanderBoundaryLeft, float wanderBoundaryRight);

    /// <summary>
    /// 사망 상태
    /// </summary>
    public abstract void SetState_Dead();
}
