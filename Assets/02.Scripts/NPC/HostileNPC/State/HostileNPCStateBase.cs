public class HostileNPCStateBase : IState
{
    protected HostileNPC NPC;
    protected HostileNPCStateBase(HostileNPC hostileNPC)
    {
        NPC = hostileNPC;
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
