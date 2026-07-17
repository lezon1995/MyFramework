namespace MoreMountains;

public interface ITriggerAction
{
}

public interface ITriggerAction<in T> : ITriggerAction
{
    void trigger(T v);
}