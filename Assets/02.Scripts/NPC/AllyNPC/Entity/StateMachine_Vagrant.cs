using System;

public class StateMachine_Vagrant : NPCStateMachine
{
    public AllyNPCState_Wander State_Wander { get; private set; }
    public AllyNPCState_MoveTo State_MoveTo { get; private set; }
    public AllyNPCState_BlownAway State_BlownAway { get; private set; }
    public AllyNPCState_Crouch State_Crouch { get; private set; }

    public StateMachine_Vagrant(AllyNPC allyNPC)
    {
        State_Wander = new AllyNPCState_Wander(allyNPC);
        State_MoveTo = new AllyNPCState_MoveTo(allyNPC);
        State_BlownAway = new AllyNPCState_BlownAway(allyNPC);
        State_Crouch = new AllyNPCState_Crouch(allyNPC);
    }
}
