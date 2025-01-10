using UnityEngine;

public class AllyNPCState_Wander : AllyNPCStateBase
{
    public AllyNPCState_Wander(AllyNPC allyNPC) : base(allyNPC)
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

        waitTime = Random.Range(NPC.ClassData.waittimemin, NPC.ClassData.waittimemax);
    }

    public override void Execute()
    {
        switch (sequence)
        {
            case Sequence.Stand:
                // Stand - Wait
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                }
                // Stand - Goto Walk
                else
                {
                    NPC.AI.AnimationController.SetWalk(true);
                    isAnimWalkTrue = true;

                    destination = new Vector3(Random.Range(left, right), NPC.transform.position.y, NPC.transform.position.z);
                    NPC.SetLookLeft(NPC.transform.position.x > destination.x);

                    sequence = Sequence.Walk;
                }
                break;

            case Sequence.Walk:
                // Stand - Move to Destination
                if (Mathf.Abs(NPC.transform.position.x - destination.x) > 0.01f)
                {
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, destination, NPC.ClassData.walkspeed * Time.deltaTime);
                }
                // Stand - Goto Stand
                else
                {
                    NPC.AI.AnimationController.SetWalk(false);
                    isAnimWalkTrue = false;

                    waitTime = Random.Range(NPC.ClassData.waittimemin, NPC.ClassData.waittimemax);

                    sequence = Sequence.Stand;
                }
                break;
        }
    }

    public override void Exit()
    {
        if (isAnimWalkTrue)
        {
            NPC.AI.AnimationController.SetWalk(false);
        }
    }
}
