using System;

public class StateMachine_Archer : NPCStateMachine
{
    public AllyNPCState_Wander State_Wander { get; private set; }
    public AllyNPCState_OfferCoin State_OfferCoin { get; private set; }
    public AllyNPCState_MoveTo State_MoveTo { get; private set; }
    public AllyNPCState_AttackTarget State_AttackTarget { get; private set; }
    public AllyNPCState_DefendPosition State_DefendPosition { get; private set; }

    public StateMachine_Archer(AllyNPC allyNPC)
    {
        State_Wander = new AllyNPCState_Wander(allyNPC);
        State_OfferCoin = new AllyNPCState_OfferCoin(allyNPC);
        State_MoveTo = new AllyNPCState_MoveTo(allyNPC);
        State_AttackTarget = new AllyNPCState_AttackTarget(allyNPC);
        State_DefendPosition = new AllyNPCState_DefendPosition(allyNPC);
    }
}
