public abstract class PlayerBaseState
{
    protected bool isRootState = false;
    protected PlayerStateMachine ctx;
    protected PlayerStateFactory factory;
    PlayerBaseState currentSubState;
    PlayerBaseState currentSuperState;

    protected bool IsRootState { set { isRootState = value; } }
    protected PlayerStateMachine Ctx { get { return ctx; } }
    protected PlayerStateFactory Factory { get { return factory; } }


    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        ctx = currentContext;
        factory = playerStateFactory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchState();
    public abstract void InitializeSubState();

    public void UpdateStates()
    {
        UpdateState();
        if (currentSubState != null)
        {
            currentSubState.UpdateStates(); //Llama a su hijo para que llame a sus hijos
        }
    }

    /*
    //No hace falta
    public void ExitStates()
    {
        ExitState();
        if (currentSubsState != null)
        {
            currentSubsState.ExitStates();
        }
    }
    */

    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();
        newState.EnterState();

        if (isRootState) { ctx.CurrentState = newState; }  //cambia el contexto si es root
        else if (currentSuperState != null) { currentSuperState.SetSubState(newState); } //si tiene subState lo reasigna
    }
    protected void SetSuperState(PlayerBaseState newSuperState)
    {
        currentSuperState = newSuperState;
        //UnityEngine.Debug.Log("currentSuperState: "+ currentSuperState);
    }
    protected void SetSubState(PlayerBaseState newSubState)
    {
        currentSubState = newSubState;
        //UnityEngine.Debug.Log("currentSubsState: " + currentSubsState);
        newSubState.SetSuperState(this); //Le estas pasando la Clase padre para que el hijo sepa que padre tiene
    }
}
