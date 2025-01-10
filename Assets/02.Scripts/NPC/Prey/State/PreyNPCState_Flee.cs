using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyNPCState_Flee : PreyNPCStateBase
{
    public PreyNPCState_Flee(PreyNPC preyNPC) : base(preyNPC)
    {
    }

    public void SetValue(bool isGoLeft)
    {
        goLeft = isGoLeft;
    }

    // 세팅 변수
    private bool goLeft;

    // 초기화 변수
    private bool isAnimFleeTrue;

    // 기타 변수
    private Vector3 destination;
    private float waitTime;

    public override void Enter()
    {
        // Animation Flee
        isAnimFleeTrue = true;

        destination = new Vector3(goLeft ? -1000f : 1000f, NPC.transform.position.y, NPC.transform.position.z);
        waitTime = NPC.PreyData.fleetime;
        NPC.SetLookLeft(goLeft);
    }

    public override void Execute()
    {
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
            NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, destination, NPC.PreyData.sprintspeed * Time.deltaTime);
        }
    }

    public override void Exit()
    {
        if (isAnimFleeTrue)
        {
            // Animation Flee
        }
    }
}
