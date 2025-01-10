using System.Collections;
using UnityEditor;
using UnityEngine;

public class AI_Rabbit : PreyNPCAI
{
    private enum Command
    {
        NONE,
        Wander
    }
    private Command CurrentCommand;
    private Command AppliedCommand;
    private float commandVal_WanderLeft;
    private float commandVal_WanderRight;


    private enum State
    {
        Wander,
        Dead
    }

    private StateMachine_Rabbit StateMachine;
    private State CurrentState;

    private bool alreadyChangeState;

    public override void AwakeAI(PreyNPC preyNPC)
    {
        NPC = preyNPC;
        StateMachine = new StateMachine_Rabbit(NPC);

        debugLabelOffset = new Vector3(0, NPC.PreyData.initialoffsety + 0.5f, 0);
    }

    public override void Initialize()
    {
        CurrentCommand = Command.NONE;
        AppliedCommand = Command.NONE;
    }


    public override void SetCommand_Wander(float left, float right)
    {
        CurrentCommand = Command.Wander;
        commandVal_WanderLeft = left;
        commandVal_WanderRight = right;
    }
    private void ApplyCurrentCommand()
    {
        AppliedCommand = CurrentCommand;

        switch (AppliedCommand)
        {
            case Command.Wander:
                SetState_Wander(commandVal_WanderLeft, commandVal_WanderRight);
                break;
        }
    }

    public override void UpdateAI()
    {
        alreadyChangeState = false;

        switch (CurrentState)
        {
            case State.Wander:
                ExecuteState_Wander();
                break;
            case State.Dead: // Lock Check
                ExecuteState_Dead();
                break;
        }

        if (StateLock == false)
        {
            if (AppliedCommand != CurrentCommand)
            {
                ApplyCurrentCommand();
            }
        }

        StateMachine?.Execute();

        alreadyChangeState = false;
    }

    private void ExecuteState_Wander()
    {
        
    }
    private void ExecuteState_Dead() // Lock Check
    {
        if (StateLock == false)
        {
            PoolManager.Instance.ReleaseComponent(NPC.gameObject, NPC.PreyData.id);
        }
    }


    private void SetState_Wander(float left, float right)
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Wander;
        debugCurrentState = "Wander";

        StateMachine.State_Wander.SetValue(left, right);
        StateMachine.ChangeState(StateMachine.State_Wander);
    }

    public override void SetState_Dead()
    {
        if (alreadyChangeState) return;
        alreadyChangeState = true;

        CurrentState = State.Dead;
        debugCurrentState = "Dead";

        StateMachine.ChangeState(StateMachine.State_Dead);
    }
}
