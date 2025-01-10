using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostileNPCState_MoveTo : HostileNPCStateBase
{
    public HostileNPCState_MoveTo(HostileNPC allyNPC) : base(allyNPC)
    {
    }

    public void SetValue(float targetPosition, bool runAnim)
    {
        position = targetPosition;
        isRun = runAnim;
    }

    // 세팅 변수
    private float position;
    private bool isRun;

    // 초기화 변수
    private bool isAnimSprintTrue;
    private float speed;

    // 기타 변수
    private Vector3 destination;

    public override void Enter()
    {
        destination = new Vector3(position, NPC.transform.position.y, NPC.transform.position.z);

        if (Mathf.Abs(NPC.transform.position.x - destination.x) > 0.01f)
        {
            if (isRun)
            {
                // Animation 
            }
            else
            {
                // Animation 
            }
            isAnimSprintTrue = true;

            NPC.SetLookLeft(NPC.transform.position.x > destination.x);
        }

        speed = isRun ? NPC.MonsterData.runspeed : NPC.MonsterData.sprintspeed;
    }

    public override void Execute()
    {
        if (Mathf.Abs(NPC.transform.position.x - destination.x) > 0.01f)
        {

            NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, destination, speed * Time.deltaTime);
        }
        else if (isAnimSprintTrue)
        {
            if (isRun)
            {
                // Animation 
            }
            else
            {
                // Animation 
            }
            isAnimSprintTrue = false;
        }
    }

    public override void Exit()
    {
        if (isAnimSprintTrue)
        {
            if (isRun)
            {
                // Animation 
            }
            else
            {
                // Animation 
            }
        }
    }
}