public class StateMachine_Grredling : NPCStateMachine
{
    public HostileNPCState_Wander State_Wander { get; private set; }
    public HostileNPCState_MoveTo State_MoveTo { get; private set; }
    public HostileNPCState_AttackTargetTackle State_AttackTargetTackle { get; private set; }
    public HostileNPCState_Dead State_Dead { get; private set; }

    public StateMachine_Grredling(HostileNPC HostileNPC)
    {
        State_Wander = new HostileNPCState_Wander(HostileNPC);
        State_MoveTo = new HostileNPCState_MoveTo(HostileNPC);
        State_AttackTargetTackle = new HostileNPCState_AttackTargetTackle(HostileNPC);
        State_Dead = new HostileNPCState_Dead(HostileNPC);
    }
}
