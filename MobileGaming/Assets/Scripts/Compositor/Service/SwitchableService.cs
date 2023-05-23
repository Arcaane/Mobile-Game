public class SwitchableService : ISwitchableService
{
    public bool enable { get; private set; }

    public SwitchableService(bool startState)
    {
        enable = startState;
    }

    public virtual void Toggle()
    {
        enable = !enable;
        if(enable) Enable();
        else Disable();
    }

    public virtual void Enable()
    {
        enable = true;
    }

    public virtual void Disable()
    {
        enable = false;
    }
    
}
