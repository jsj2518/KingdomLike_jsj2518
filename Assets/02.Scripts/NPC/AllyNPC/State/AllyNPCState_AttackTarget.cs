using System;
using System.Collections;
using UnityEngine;

public class AllyNPCState_AttackTarget : AllyNPCStateBase
{
    public AllyNPCState_AttackTarget(AllyNPC allyNPC) : base(allyNPC)
    {
    }

    public void SetValue(GameObject targetAttack, Action measureShooting, Action shoot)
    {
        target = targetAttack;
        MeasureShooting = measureShooting;
        Shoot = shoot;
    }
    public void SetTarget(GameObject targetAttack)
    {
        target = targetAttack;
    }

    private enum Sequence
    {
        Chase,
        AttackPreDelay,
        AttackMotion,
        AttackPostDelay,
        TargetLost
    }

    // 세팅 변수
    private GameObject target;
    private Action MeasureShooting;
    private Action Shoot;

    // 초기화 변수
    private Sequence sequence;
    private bool isAnimSprintTrue;

    // 기타 변수
    private Vector3 destination;
    private float waitTime;
    private float shootTriggerTime;

    public override void Enter()
    {
        isAnimSprintTrue = false;

        // Enter - Goto TargetLost
        if (target == null)
        {
            sequence = Sequence.TargetLost;
        }
        // Enter - Goto Chase
        else if (Mathf.Abs(NPC.transform.position.x - target.transform.position.x) > NPC.ClassData.attackdistance)
        {
            NPC.AI.AnimationController.SetSprint(true);
            isAnimSprintTrue = true;

            NPC.SetLookLeft(NPC.transform.position.x > target.transform.position.x);

            sequence = Sequence.Chase;
        }
        // Enter - Goto AttackPreDelay
        else
        {
            waitTime = NPC.ClassData.attackpredelay;
            NPC.SetLookLeft(NPC.transform.position.x > target.transform.position.x);
            MeasureShooting?.Invoke();

            sequence = Sequence.AttackPreDelay;
            NPC.AI.StateLock = true;
        }
    }

    public override void Execute()
    {
        switch (sequence)
        {
            case Sequence.Chase:
                // Chase - Goto TargetLost
                if (target == null)
                {
                    NPC.AI.AnimationController.SetSprint(false);
                    isAnimSprintTrue = false;

                    sequence = Sequence.TargetLost;
                }
                // Chase - Move to Target
                else if (Mathf.Abs(NPC.transform.position.x - target.transform.position.x) > NPC.ClassData.attackdistance)
                {
                    destination = new Vector3(target.transform.position.x, NPC.transform.position.y, NPC.transform.position.z);
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, destination, NPC.ClassData.sprintspeed * Time.deltaTime);
                }
                // Chase - Goto AttackPreDelay
                else
                {
                    NPC.AI.AnimationController.SetSprint(false);
                    isAnimSprintTrue = false;

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
                    waitTime = NPC.ClassData.attackmotiondelay;
                    shootTriggerTime = NPC.ClassData.shootdelay;
                    NPC.AI.AnimationController.TriggerAttack();

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
                    // AttackPostDelay - Goto TargetLost
                    if (target == null)
                    {
                        sequence = Sequence.TargetLost;
                        NPC.AI.StateLock = false;
                    }
                    // AttackPostDelay - Goto AttackPreDelay
                    else if (Mathf.Abs(NPC.transform.position.x - target.transform.position.x) < NPC.ClassData.attackdistance)
                    {
                        waitTime = NPC.ClassData.attackpredelay;
                        NPC.SetLookLeft(NPC.transform.position.x > target.transform.position.x);
                        MeasureShooting?.Invoke();

                        sequence = Sequence.AttackPreDelay;
                    }
                    // AttackPostDelay - Goto Chase
                    else
                    {
                        NPC.AI.AnimationController.SetSprint(true);
                        isAnimSprintTrue = true;

                        NPC.SetLookLeft(NPC.transform.position.x > target.transform.position.x);

                        sequence = Sequence.Chase;
                        NPC.AI.StateLock = false;
                    }
                }
                break;

            case Sequence.TargetLost:
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
