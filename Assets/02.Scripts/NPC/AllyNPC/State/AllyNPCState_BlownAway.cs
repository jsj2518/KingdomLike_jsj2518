using System;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class AllyNPCState_BlownAway : AllyNPCStateBase
{
    public AllyNPCState_BlownAway(AllyNPC allyNPC) : base(allyNPC)
    {
    }

    public void SetValue(bool isBlownLeft)
    {
        blownLeft = isBlownLeft;
    }

    private enum Sequence
    {
        Blown,
        KnockDown,
        StandUp
    }

    // 세팅 변수
    private bool blownLeft;

    // 초기화 변수
    private Sequence sequence;
    private bool stateUnlock;

    // 기타 변수
    private float waitTime;

    public override void Enter()
    {
        NPC.AI.StateLock = true;
        stateUnlock = false;

        NPC.SetLookLeft(blownLeft);

        // TODO : Animation

        DOBlown();
        sequence = Sequence.Blown;
    }

    public override void Execute()
    {
        switch (sequence)
        {
            case Sequence.KnockDown:
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                }
                else
                {
                    // TODO : Animation

                    sequence = Sequence.StandUp;
                    waitTime = 1f;
                }
                break;

            case Sequence.StandUp:
                if (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
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
        NPC.HitCollider.enabled = true;
    }


    private void DOBlown()
    {
        int resolution = 20; // 포물선의 세밀도
        Vector3 startPos = NPC.transform.position;
        float sign = blownLeft ? -1f : 1f;
        Vector3[] BlownPath = new Vector3[resolution + 1];
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            float x = t * 3f;
            float y = Mathf.Sin(Mathf.PI * t) * 0.8f;
            BlownPath[i] = new Vector3(startPos.x + sign * x, startPos.y + y, 0);
        }

        NPC.transform.DOPath(BlownPath, 0.5f, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .OnComplete(BlownEnd);
    }
    private void BlownEnd()
    {
        // TODO : Animation

        sequence = Sequence.KnockDown;
        waitTime = 1f;
    }
}