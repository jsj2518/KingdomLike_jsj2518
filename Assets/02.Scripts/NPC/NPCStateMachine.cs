public class NPCStateMachine
{
    public IState currentState { get; private set; }

    public void ChangeState(IState state, bool avoidRepetition = true)
    {
        if (avoidRepetition && currentState == state) return;

        currentState?.Exit();
        currentState = state;
        currentState?.Enter();
    }

    public void Execute()
    {
        currentState?.Execute();
    }
}
