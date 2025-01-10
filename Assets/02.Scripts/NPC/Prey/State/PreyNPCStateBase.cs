public class PreyNPCStateBase : IState
{
    protected PreyNPC NPC;
    protected PreyNPCStateBase(PreyNPC preyNPC)
    {
        NPC = preyNPC;
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
