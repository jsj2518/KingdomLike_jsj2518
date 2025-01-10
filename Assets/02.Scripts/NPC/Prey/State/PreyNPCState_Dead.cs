
using UnityEngine;

public class PreyNPCState_Dead : PreyNPCStateBase
{
    public PreyNPCState_Dead(PreyNPC PreyNPC) : base(PreyNPC)
    {
    }

    // 초기화 변수
    private bool stateUnlock;

    // 기타 변수
    private float waitTime;

    public override void Enter()
    {
        NPC.AI.StateLock = true;
        stateUnlock = false;

        NPC.AnimationController.TriggerDeath();

        waitTime = NPC.PreyData.deathdelay;
    }

    public override void Execute()
    {
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
        else if (stateUnlock == false)
        {
            NPC.AI.StateLock = false;
            stateUnlock = true;
        }
    }

    public override void Exit()
    {

    }
}
