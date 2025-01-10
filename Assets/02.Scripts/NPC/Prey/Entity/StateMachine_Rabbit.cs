using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine_Rabbit : NPCStateMachine
{
    public PreyNPCState_Wander State_Wander { get; private set; }
    public PreyNPCState_Dead State_Dead { get; private set; }

    public StateMachine_Rabbit(PreyNPC PreyNPC)
    {
        State_Wander = new PreyNPCState_Wander(PreyNPC);
        State_Dead = new PreyNPCState_Dead(PreyNPC);
    }
}
