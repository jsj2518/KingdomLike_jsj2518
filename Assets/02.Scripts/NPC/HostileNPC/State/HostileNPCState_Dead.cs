
using UnityEngine;

public class HostileNPCState_Dead : HostileNPCStateBase
{
    public HostileNPCState_Dead(HostileNPC hostileNPC) : base(hostileNPC)
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

        NPC.AI.AnimationController.TriggerDeath();

        waitTime = NPC.MonsterData.deathdelay;
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
