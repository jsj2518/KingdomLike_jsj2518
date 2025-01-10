using System;

public class StateMachine_Villager : NPCStateMachine
{
    public AllyNPCState_BlownAway State_BlownAway { get; private set; }
    public AllyNPCState_Wander State_Wander { get; private set; }
    public AllyNPCState_OfferCoin State_OfferCoin { get; private set; }
    public AllyNPCState_MoveTo State_MoveTo { get; private set; }
    public AllyNPCState_Flee State_Flee { get; private set; }

    public StateMachine_Villager(AllyNPC allyNPC)
    {
        State_BlownAway = new AllyNPCState_BlownAway(allyNPC);
        State_Wander = new AllyNPCState_Wander(allyNPC);
        State_OfferCoin = new AllyNPCState_OfferCoin(allyNPC);
        State_MoveTo = new AllyNPCState_MoveTo(allyNPC);
        State_Flee = new AllyNPCState_Flee(allyNPC);
    }
}