namespace MarbleHero;

public abstract class ATrigger : ClassObject
{
    public ITriggerAction triggerAction;

    public void setTriggerAction(ITriggerAction value)
    {
        triggerAction = value;
    }
}