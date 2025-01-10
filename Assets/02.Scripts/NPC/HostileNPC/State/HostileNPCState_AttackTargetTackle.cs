using System;
using UnityEngine;

public class HostileNPCState_AttackTargetTackle : HostileNPCStateBase
{
    public HostileNPCState_AttackTargetTackle(HostileNPC allyNPC) : base(allyNPC)
    {
    }

    public void SetValue(GameObject targetAttack, Action tackleStart, Action tackleEnd)
    {
        target = targetAttack;
        TackleStart = tackleStart;
        TackleEnd = tackleEnd;
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
    private Action TackleStart;
    private Action TackleEnd;

    // 초기화 변수
    private Sequence sequence;
    private bool isAnimSprintTrue;

    // 기타 변수
    private Vector3 destination;
    private float waitTime;
    private float flySpeed;
    private bool bounced;

    public override void Enter()
    {
        isAnimSprintTrue = false;

        if (target == null)
        {
            sequence = Sequence.TargetLost;
        }
        if (Mathf.Abs(NPC.transform.position.x - target.transform.position.x) > NPC.MonsterData.attackdistance)
        {
            // Animation Sprint
            isAnimSprintTrue = true;

            NPC.SetLookLeft(NPC.transform.position.x > target.transform.position.x);

            sequence = Sequence.Chase;
            NPC.AI.StateLock = false;
        }
        else
        {
            waitTime = NPC.MonsterData.attackpredelay;
            NPC.SetLookLeft(NPC.transform.position.x > target.transform.position.x);

            sequence = Sequence.AttackPreDelay;
            NPC.AI.StateLock = true;
        }
    }

    public override void Execute()
    {
        switch (sequence)
        {
            case Sequence.Chase:
                if (target == null)
                {
                    // Animation Sprint false
                    isAnimSprintTrue = false;

                    sequence = Sequence.TargetLost;
                }
                else if (Mathf.Abs(NPC.transform.position.x - target.transform.position.x) > NPC.MonsterData.attackdistance)
                {
                    destination = new Vector3(target.transform.position.x, NPC.transform.position.y, NPC.transform.position.z);
                    NPC.transform.position = Vector3.MoveTowards(NPC.transform.position, destination, NPC.MonsterData.sprintspeed * Time.deltaTime);
                }
                else
                {
                    // Animation Sprint false
                    isAnimSprintTrue = false;

                    waitTime = NPC.MonsterData.attackpredelay;
                    NPC.SetLookLeft(NPC.transform.position.x > target.transform.position.x);

                    sequence = Sequence.AttackPreDelay;
                    NPC.AI.StateLock = true;
                }
                break;

            case Sequence.AttackPreDelay:
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                }
                else
                {
                    waitTime = NPC.MonsterData.attackmotiondelay;
                    NPC.AI.AnimationController.TriggerAttack();

                    TackleStart?.Invoke();

                    flySpeed = NPC.IsLookLeft ? -NPC.MonsterData.attackflyspeed : NPC.MonsterData.attackflyspeed;
                    bounced = false;
                    sequence = Sequence.AttackMotion;
                }
                break;

            case Sequence.AttackMotion:
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;

                    float flyDistance = flySpeed * Time.deltaTime;

                    if (bounced == false)
                    {
                        RaycastHit2D hitWall = Physics2D.Raycast(NPC.transform.position, Vector2.right, flyDistance, NPC.WallLayer);
                        // 벽에 부딪힘
                        if (hitWall.collider != null
                            && hitWall.collider.gameObject.CompareTag(Tags.T_CanBecomeTarget))
                        {
                            NPC.transform.Translate(new Vector3(hitWall.distance, 0, 0));
                            bounced = true;
                        }
                        // Keep flying
                        else
                        {
                            NPC.transform.Translate(new Vector3(flyDistance, 0, 0));
                        }
                    }
                    // Bounced
                    else
                    {
                        NPC.transform.Translate(new Vector3(flyDistance * -0.5f, 0, 0));
                    }
                }
                else
                {
                    TackleEnd?.Invoke();

                    waitTime = NPC.MonsterData.attackpostdelay;
                    sequence = Sequence.AttackPostDelay;
                }
                break;

            case Sequence.AttackPostDelay:
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                }
                else
                {
                    if (target == null)
                    {
                        sequence = Sequence.TargetLost;
                        NPC.AI.StateLock = false;
                    }
                    else if (Mathf.Abs(NPC.transform.position.x - target.transform.position.x) < NPC.MonsterData.attackdistance)
                    {
                        waitTime = NPC.MonsterData.attackpredelay;
                        NPC.SetLookLeft(NPC.transform.position.x > target.transform.position.x);

                        sequence = Sequence.AttackPreDelay;
                    }
                    else
                    {
                        // Animation Sprint true
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
            // Animation Walk false
        }
    }
}
