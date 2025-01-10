using System;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class AllyNPCState_Work : AllyNPCStateBase
{
    public AllyNPCState_Work(AllyNPC allyNPC) : base(allyNPC)
    {
    }

    public void SetValue(float workBoundaryLeft, float workBoundaryRight, Action onWorkStart)
    {
        left = workBoundaryLeft;
        right = workBoundaryRight;
        OnWorkStart = onWorkStart;
    }

    private enum Sequence
    {
        Move,
        Work,
        WorkMove
    }

    // 세팅 변수
    private float left;
    private float right;
    private Action OnWorkStart;

    // 초기화 변수
    private Sequence sequence;
    private bool isAnimSprintTrue;
    private bool isAnimWorkTrue;

    // 기타 변수
    private Vector3 destination;
    private float waitTime;

    public override void Enter()
    {
        destination = new Vector3(Random.Range(left, right), NPC.transform.position.y, NPC.transform.position.z);

        // Enter - Goto Move
        if (Mathf.Abs(NPC.transform.position.x - destination.x) > 0.01f)
        {
            NPC.AI.AnimationController.SetSprint(true);
            isAnimSprintTrue = true;
            isAnimWorkTrue = false;

            NPC.SetLookLeft(NPC.transform.position.x > destination.x);
            sequence = Sequence.Move;
        }
        // Enter - Goto Work
        else
        {
            NPC.AI.AnimationController.SetWork(true);
            isAnimSprintTrue = false;
            isAnimWorkTrue = true;

            NPC.SetLookLeft(destination.x > (left + right) / 2f);
            waitTime = Random.Range(NPC.ClassData.waittimemin, NPC.ClassData.waittimemax);

            sequence = Sequence.Work;
            OnWorkStart?.Invoke();
        }
    }

    public override void Execute()
    {
        switch (sequence)
        {
            case Sequence.Move:
                // Move - Move to Destination
                if (Mathf.Abs(NPC.transform.position.x - destination.x) > 0.01f)
                {
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, destination, NPC.ClassData.sprintspeed * Time.deltaTime);
                }
                // Move - Goto Work
                else
                {
                    NPC.AI.AnimationController.SetSprint(false);
                    NPC.AI.AnimationController.SetWork(true);
                    isAnimSprintTrue = false;
                    isAnimWorkTrue = true;

                    NPC.SetLookLeft(destination.x > (left + right) / 2f);
                    waitTime = Random.Range(NPC.ClassData.waittimemin, NPC.ClassData.waittimemax);

                    sequence = Sequence.Work;
                    OnWorkStart?.Invoke();
                }
                break;

            case Sequence.Work:
                // Work - Wait
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                }
                // Work - Goto WorkMove
                else
                {
                    NPC.AI.AnimationController.SetSprint(true);
                    NPC.AI.AnimationController.SetWork(false);
                    isAnimSprintTrue = true;
                    isAnimWorkTrue = false;

                    destination = new Vector3(Random.Range(left, right), NPC.transform.position.y, NPC.transform.position.z);
                    NPC.SetLookLeft(NPC.transform.position.x > destination.x);

                    sequence = Sequence.WorkMove;
                }
                break;

            case Sequence.WorkMove:
                // WorkMove - Move to Destination
                if (Mathf.Abs(NPC.transform.position.x - destination.x) > 0.01f)
                {
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, destination, NPC.ClassData.sprintspeed * Time.deltaTime);
                }
                // WorkMove - Goto Work
                else
                {
                    NPC.AI.AnimationController.SetSprint(false);
                    NPC.AI.AnimationController.SetWork(true);
                    isAnimSprintTrue = false;
                    isAnimWorkTrue = true;

                    NPC.SetLookLeft(destination.x > (left + right) / 2f);
                    waitTime = Random.Range(NPC.ClassData.waittimemin, NPC.ClassData.waittimemax);

                    sequence = Sequence.Work;
                }
                break;
        }
    }

    public override void Exit()
    {
        if (isAnimSprintTrue)
        {
            NPC.AI.AnimationController.SetSprint(false);
        }
        if (isAnimWorkTrue)
        {
            NPC.AI.AnimationController.SetWork(false);
        }
    }
}