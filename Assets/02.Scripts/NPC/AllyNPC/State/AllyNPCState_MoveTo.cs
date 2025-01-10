using UnityEngine;

public class AllyNPCState_MoveTo : AllyNPCStateBase
{
    public AllyNPCState_MoveTo(AllyNPC allyNPC) : base(allyNPC)
    {
    }

    public void SetValue(float targetPosition)
    {
        position = targetPosition;
    }

    // 세팅 변수
    private float position;

    // 초기화 변수
    private bool isAnimSprintTrue;

    // 기타 변수
    private Vector3 destination;

    public override void Enter()
    {
        destination = new Vector3(position, NPC.transform.position.y, NPC.transform.position.z);

        if (Mathf.Abs(NPC.transform.position.x - destination.x) > 0.01f)
        {
            NPC.AI.AnimationController.SetSprint(true);
            isAnimSprintTrue = true;

            NPC.SetLookLeft(NPC.transform.position.x > destination.x);
        }
    }

    public override void Execute()
    {
        if (Mathf.Abs(NPC.transform.position.x - destination.x) > 0.01f)
        {
            NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, destination, NPC.ClassData.sprintspeed * Time.deltaTime);
        }
        else if (isAnimSprintTrue)
        {
            NPC.AI.AnimationController.SetSprint(false);
            isAnimSprintTrue = false;
        }
    }

    public override void Exit()
    {
        if (isAnimSprintTrue)
        {
            NPC.AI.AnimationController.SetSprint(false);
        }
    }
}

