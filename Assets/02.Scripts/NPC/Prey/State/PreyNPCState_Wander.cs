using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyNPCState_Wander : PreyNPCStateBase
{
    public PreyNPCState_Wander(PreyNPC preyNPC) : base(preyNPC)
    {
    }

    public void SetValue(float wanderBoundaryLeft, float wanderBoundaryRight)
    {
        left = wanderBoundaryLeft;
        right = wanderBoundaryRight;
    }

    private enum Sequence
    {
        Stand,
        Walk
    }

    // 세팅 변수
    private float left;
    private float right;

    // 초기화 변수
    private Sequence sequence;
    private bool isAnimWalkTrue;

    // 기타 변수
    private Vector3 destination;
    private float waitTime;

    public override void Enter()
    {
        sequence = Sequence.Stand;
        isAnimWalkTrue = false;

        waitTime = Random.Range(NPC.PreyData.waittimemin, NPC.PreyData.waittimemax);
    }

    public override void Execute()
    {
        switch (sequence)
        {
            case Sequence.Stand:
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                }
                else
                {
                    // Animation Walk true
                    isAnimWalkTrue = true;

                    destination = new Vector3(Random.Range(left, right), NPC.transform.position.y, NPC.transform.position.z);
                    NPC.SetLookLeft(NPC.transform.position.x > destination.x);

                    sequence = Sequence.Walk;
                }
                break;

            case Sequence.Walk:
                if (Mathf.Abs(NPC.transform.position.x - destination.x) > 0.01f)
                {
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, destination, NPC.PreyData.walkspeed * Time.deltaTime);
                }
                else
                {
                    // Animation Walk false
                    isAnimWalkTrue = false;

                    waitTime = Random.Range(NPC.PreyData.waittimemin, NPC.PreyData.waittimemax);

                    sequence = Sequence.Stand;
                }
                break;
        }
    }

    public override void Exit()
    {
        if (isAnimWalkTrue)
        {
            // Animation Walk false
        }
    }
}