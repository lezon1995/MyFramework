using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MMGameEvents are used throughout the game for general game events (game started, game ended, life lost, etc.)
/// </summary>
public struct MMGameEvent
{
    static MMGameEvent e;

    public string EventName;
    public int IntParameter;
    public Vector2 Vector2Parameter;
    public Vector3 Vector3Parameter;
    public bool BoolParameter;
    public string StringParameter;

    public static void Trigger(string eventName, int intParameter = 0, Vector2 vector2Parameter = default(Vector2), Vector3 vector3Parameter = default(Vector3), bool boolParameter = false, string stringParameter = "")
    {
        e.EventName = eventName;
        e.IntParameter = intParameter;
        e.Vector2Parameter = vector2Parameter;
        e.Vector3Parameter = vector3Parameter;
        e.BoolParameter = boolParameter;
        e.StringParameter = stringParameter;
        EventManager.trigger(e);
    }
}

public interface IEventRouter
{
    IEventRouter eventRouter { get; }
    void trigger<T>(T e) => EventRouter.get(this).trigger(e);
    void addListener<T>(IEvent<T> listener) where T : struct => EventRouter.get(this).addListener(listener);
    void removeListener<T>(IEvent<T> listener) where T : struct => EventRouter.get(this).removeListener(listener);

    void addListener<T>(Action<T> listener) where T : struct => EventRouter.get(this).addListener(listener);
    void removeListener<T>(Action<T> listener) where T : struct => EventRouter.get(this).removeListener(listener);
}

/*public class ActionListener<T> : IEvent<T>
{
    Action<T> _action;

    public void Bind(Action<T> action) => _action = action;

    void IEvent<T>.onEvent(T e) => _action(e);

    static RefPool<ActionListener<T>> Pool = new();
    static Dictionary<Action<T>, ActionListener<T>> Dict = new();

    public static ActionListener<T> Get(Action<T> action)
    {
        if (!Dict.TryGetValue(action, out var listener))
        {
            listener = Pool.Get();
            listener.Bind(action);
            Dict[action] = listener;
        }

        return listener;
    }

    public static void Release(ActionListener<T> listener)
    {
        Dict.Remove(listener._action);
        listener.Bind(null);
        Pool.Return(listener);
    }
}*/

public class EventRouter
{
    Dictionary<Type, List<IEvent>> _subscribersList = new();

    static Dictionary<IEventRouter, EventRouter> _dict = new();

    public static EventRouter get(IEventRouter router)
    {
        if (!_dict.TryGetValue(router, out var eventRouter))
        {
            eventRouter = new();
            _dict.TryAdd(router, eventRouter);
        }

        return eventRouter;
    }

    bool exists(Type type, IEvent listener)
    {
        if (!_subscribersList.TryGetValue(type, out var list))
            return false;

        bool exists = false;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == listener)
            {
                exists = true;
                break;
            }
        }

        return exists;
    }

    public void trigger<T>(T e)
    {
        if (!_subscribersList.TryGetValue(typeof(T), out var list))
            return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            ((IEvent<T>)list[i]).onEvent(e);
        }
    }

    public void addListener<T>(IEvent<T> listener) where T : struct
    {
        var type = typeof(T);
        addListener(listener, type);
    }

    public void addListener(IEvent listener, Type type)
    {
        if (!_subscribersList.ContainsKey(type))
            _subscribersList[type] = new();

        if (!exists(type, listener))
        {
            _subscribersList[type].Add(listener);
        }
    }

    public void removeListener<T>(IEvent<T> listener) where T : struct
    {
        var type = typeof(T);
        removeListener(listener, type);
    }

    public void removeListener(IEvent listener, Type type)
    {
        if (!exists(type, listener))
            return;

        var subscribers = _subscribersList[type];
        for (int i = subscribers.Count - 1; i >= 0; i--)
        {
            if (subscribers[i] == listener)
            {
                subscribers.Remove(subscribers[i]);
                if (subscribers.Count == 0)
                {
                    _subscribersList.Remove(type);
                }

                return;
            }
        }
    }

    public void addListener<T>(Action<T> action) where T : struct
    {
        // var listener = ActionListener<T>.Get(action);
        // addListener(listener);
    }

    public void removeListener<T>(Action<T> action) where T : struct
    {
        // var listener = ActionListener<T>.Get(action);
        // removeListener(listener);
        // ActionListener<T>.Release(listener);
    }
}

/// <summary>
/// This class handles event management, and can be used to broadcast events throughout the game, to tell one class (or many) that something's happened.
/// Events are structs, you can define any kind of events you want. This manager comes with MMGameEvents, which are 
/// basically just made of a string, but you can work with more complex ones if you want.
/// 
/// To trigger a new event, from anywhere, do YOUR_EVENT.Trigger(YOUR_PARAMETERS)
/// So MMGameEvent.Trigger("Save"); for example will trigger a Save MMGameEvent
/// 
/// you can also call MMEventManager.TriggerEvent(YOUR_EVENT);
/// For example : MMEventManager.TriggerEvent(new MMGameEvent("GameStart")); will broadcast an MMGameEvent named GameStart to all listeners.
///
/// To start listening to an event from any class, there are 3 things you must do : 
///
/// 1 - tell that your class implements the MMEventListener interface for that kind of event.
/// For example: public class GUIManager : Singleton<GUIManager>, MMEventListener<MMGameEvent>
/// You can have more than one of these (one per event type).
///
/// 2 - On Enable and Disable, respectively start and stop listening to the event :
/// void OnEnable()
/// {
/// 	this.MMEventStartListening<MMGameEvent>();
/// }
/// void OnDisable()
/// {
/// 	this.MMEventStopListening<MMGameEvent>();
/// }
/// 
/// 3 - Implement the MMEventListener interface for that event. For example :
/// public void OnMMEvent(MMGameEvent gameEvent)
/// {
/// 	if (gameEvent.EventName == "GameOver")
///		{
///			// DO SOMETHING
///		}
/// } 
/// will catch all events of type MMGameEvent emitted from anywhere in the game, and do something if it's named GameOver
/// </summary>
[ExecuteAlways]
public static class EventManager
{
    static EventRouter _router;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void InitializeStatics() => _router = new();

    static EventManager() => _router = new();

    public static void addListener(IEvent listener, Type type) => _router.addListener(listener, type);
    public static void addListener<T>(IEvent<T> listener) where T : struct => _router.addListener(listener);
    public static void removeListener(IEvent listener, Type type) => _router.removeListener(listener, type);
    public static void removeListener<T>(IEvent<T> listener) where T : struct => _router.removeListener(listener);
    public static void trigger<T>(T e) where T : struct => _router.trigger(e);

    public static void addListener<T>(Action<T> listener) where T : struct => _router.addListener(listener);
    public static void removeListener<T>(Action<T> listener) where T : struct => _router.removeListener(listener);

    static List<Type> list = new();

    public static void addAllListener(object o)
    {
        list.Clear();
        if (!tryFillListeners(o))
            return;

        if (o is IEvent listener)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var typeT = list[i].GetGenericArguments()[0];
                listener.addListener(typeT);
            }
        }

        list.Clear();
    }

    public static void removeAllListener(object o)
    {
        list.Clear();
        if (!tryFillListeners(o))
            return;

        if (o is IEvent listener)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var typeT = list[i].GetGenericArguments()[0];
                listener.removeListener(typeT);
            }
        }

        list.Clear();
    }

    static bool tryFillListeners(object o)
    {
        var interfaces = o.GetType().GetInterfaces();
        if (interfaces.Length == 0)
            return false;

        for (var i = 0; i < interfaces.Length; i++)
        {
            var type = interfaces[i];
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEvent<>))
            {
                list.Add(type);
            }
        }

        if (list.Count == 0)
            return false;

        return true;
    }
}

/// <summary>
/// Static class that allows any class to start or stop listening to events
/// </summary>
public static class EventRegister
{
    public static void addAllListener(this object o) => EventManager.addAllListener(o);
    public static void addListener(this IEvent listener, Type type) => EventManager.addListener(listener, type);
    public static void addListener(this IEvent listener, Type type, EventRouter router) => router.addListener(listener, type);
    public static void addListener<T>(this IEvent<T> listener) where T : struct => EventManager.addListener(listener);
    public static void addListener<T>(this IEvent<T> listener, EventRouter router) where T : struct => router.addListener(listener);

    public static void removeAllListener(this object o) => EventManager.removeAllListener(o);
    public static void removeListener(this IEvent listener, Type type) => EventManager.removeListener(listener, type);
    public static void removeListener(this IEvent listener, Type type, EventRouter router) => router.removeListener(listener, type);
    public static void removeListener<T>(this IEvent<T> listener) where T : struct => EventManager.removeListener(listener);
    public static void removeListener<T>(this IEvent<T> listener, EventRouter router) where T : struct => router.removeListener(listener);

    public static void trigger<T>(this T e) where T : struct => EventManager.trigger(e);
    public static void trigger<T>(this T e, IEventRouter router) where T : struct => router.trigger(e);
}

public interface IEvent
{
}

public interface IEvent<in T> : IEvent
{
    void onEvent(T e);
}

public interface IEvent<in T1, in T2> : IEvent
{
    void onEvent(T1 e1, T2 e2);
}

public class MMEventListenerWrapper<Owner, Target, T> : IEvent<T>, IDisposable where T : struct
{
    Action<Target> _callback;
    Owner _owner;

    public MMEventListenerWrapper(Owner owner, Action<Target> callback)
    {
        _owner = owner;
        _callback = callback;
        this.addListener();
    }

    public void Dispose()
    {
        this.removeListener();
        _callback = null;
    }

    protected virtual Target OnEvent(T eventType) => default;

    public void onEvent(T e)
    {
        var item = OnEvent(e);
        _callback?.Invoke(item);
    }
}