public class AllyNPCStateBase : IState
{
    protected AllyNPC NPC;
    protected AllyNPCStateBase(AllyNPC allyNPC)
    {
        NPC = allyNPC;
    }

    public virtual void Enter()
    {

    }

    public virtual void Execute()
    {
        
    }

    public virtual void Exit()
    {

    }
}
