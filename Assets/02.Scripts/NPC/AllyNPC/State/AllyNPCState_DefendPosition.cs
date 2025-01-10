using System;
using UnityEngine;

public class AllyNPCState_DefendPosition : AllyNPCStateBase
{
    public AllyNPCState_DefendPosition(AllyNPC allyNPC) : base(allyNPC)
    {
    }

    public void SetValue(float defendPosition, bool isStandLeft, Action measureShooting, Action shoot)
    {
        position = defendPosition;
        standLeft = isStandLeft;
        MeasureShooting = measureShooting;
        Shoot = shoot;
    }
    public void SetTarget(GameObject targetAttack)
    {
        target = targetAttack;
    }

    private enum Sequence
    {
        Move,
        Stand,
        AttackPreDelay,
        AttackMotion,
        AttackPostDelay
    }

    // 세팅 변수
    private float position;
    private bool standLeft;
    private Action MeasureShooting;
    private Action Shoot;

    // 초기화 변수
    private Sequence sequence;
    private bool isAnimSprintTrue;
    private GameObject target;

    // 기타 변수
    private Vector3 destination;
    private float waitTime;
    private float shootTriggerTime;

    // 상태 변수
    public bool IsMoving { get; private set; }

    public override void Enter()
    {
        destination = new Vector3(position, NPC.transform.position.y, NPC.transform.position.z);

        // Enter - Goto Move
        if (Mathf.Abs(NPC.transform.position.x - destination.x) > 0.01f)
        {
            NPC.AI.AnimationController.SetSprint(true);
            isAnimSprintTrue = true;

            NPC.SetLookLeft(NPC.transform.position.x > destination.x);
            IsMoving = true;
            sequence = Sequence.Move;
        }
        // Enter - Goto Stand
        else
        {
            isAnimSprintTrue = false;

            NPC.SetLookLeft(standLeft);
            IsMoving = false;
            sequence = Sequence.Stand;
        }

        target = null;
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
                // Move - Goto Stand
                else
                {
                    NPC.AI.AnimationController.SetSprint(false);
                    isAnimSprintTrue = false;

                    NPC.SetLookLeft(standLeft);
                    IsMoving = false;
                    sequence = Sequence.Stand;
                }
                break;

            case Sequence.Stand:
                // Stand - Goto AttackPreDelay
                if (target != null && Mathf.Abs(NPC.transform.position.x - target.transform.position.x) < NPC.ClassData.attackdistance)
                {
                    waitTime = NPC.ClassData.attackpredelay;
                    NPC.SetLookLeft(NPC.transform.position.x > target.transform.position.x);
                    MeasureShooting?.Invoke();

                    sequence = Sequence.AttackPreDelay;
                    NPC.AI.StateLock = true;
                }
                break;

            case Sequence.AttackPreDelay:
                // AttackPreDelay - Wait
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                }
                // AttackPreDelay - Goto AttackMotion
                else
                {
                    NPC.AI.AnimationController.TriggerAttack();

                    waitTime = NPC.ClassData.attackmotiondelay;
                    shootTriggerTime = NPC.ClassData.shootdelay;

                    sequence = Sequence.AttackMotion;
                }
                break;

            case Sequence.AttackMotion:
                // AttackMotion - Wait to Shoot
                if (shootTriggerTime > 0)
                {
                    shootTriggerTime -= Time.deltaTime;

                    if (shootTriggerTime <= 0)
                    {
                        Shoot?.Invoke();
                    }
                }

                // AttackMotion - Wait
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                }
                // AttackMotion - Goto AttackPostDelay
                else
                {
                    waitTime = NPC.ClassData.attackpostdelay;

                    sequence = Sequence.AttackPostDelay;
                }
                break;

            case Sequence.AttackPostDelay:
                // AttackPostDelay - Wait
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                }
                else
                {
                    // AttackPostDelay - Goto AttackPreDelay
                    if (target != null && Mathf.Abs(NPC.transform.position.x - target.transform.position.x) < NPC.ClassData.attackdistance)
                    {
                        waitTime = NPC.ClassData.attackpredelay;
                        NPC.SetLookLeft(NPC.transform.position.x > target.transform.position.x);
                        MeasureShooting?.Invoke();

                        sequence = Sequence.AttackPreDelay;
                    }
                    // AttackPostDelay - Goto Stand
                    else
                    {
                        NPC.SetLookLeft(standLeft);

                        sequence = Sequence.Stand;
                        NPC.AI.StateLock = false;
                    }
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
    }
}

