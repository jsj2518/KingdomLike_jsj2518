using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class AllyNPCState_OfferCoin : AllyNPCStateBase
{
    public AllyNPCState_OfferCoin(AllyNPC allyNPC) : base(allyNPC)
    {
    }

    private enum Sequence
    {
        Stand,
        DropCoins
    }

    // 초기화 변수
    private Sequence sequence;
    private bool stateUnlock;

    // 기타 변수
    private float waitTime;

    public override void Enter()
    {
        sequence = Sequence.Stand;

        NPC.SetLookLeft(NPC.transform.position.x > StageManager.Instance.Player.transform.position.x);
        waitTime = 0.5f; // 동전 던지기 전 대기시간
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
                    NPC.AI.StateLock = true;
                    stateUnlock = false;
                    sequence = Sequence.DropCoins;
                }
                break;

            case Sequence.DropCoins:
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                }
                else if (NPC.CurrentCoin > 0)
                {
                    NPC.DropCoinToPlayer();
                    waitTime = 0.1f;
                }
                else if (stateUnlock == false)
                {
                    NPC.AI.StateLock = false;
                    stateUnlock = true;
                }
                break;
        }
    }

    public override void Exit()
    {

    }
}
