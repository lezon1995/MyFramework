using System;
using UnityEngine;

namespace MarbleHero;

// 角色管理器
public class NexusManager : FrameSystem
{
    protected Nexus nexus;

    public NexusManager()
    {
        mCreateObject = true;
    }

    public override void init()
    {
        base.init();
    }

    public override void destroy()
    {
        base.destroy();
        destroyNexus();
        nexus = null;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        if (nexus && nexus.isActiveInHierarchy())
        {
            var dt = !nexus.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime;
            nexus.update(dt);
        }
    }

    public override void lateUpdate(float elapsedTime)
    {
        base.lateUpdate(elapsedTime);
        if (nexus && nexus.isActiveInHierarchy())
        {
            var dt = !nexus.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime;
            nexus.lateUpdate(dt);
        }
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);
        if (nexus && nexus.isActiveInHierarchy())
        {
            var dt = !nexus.isIgnoreTimeScale() ? elapsedTime : Time.fixedUnscaledDeltaTime;
            nexus.fixedUpdate(dt);
        }
    }

    public Nexus getNexus()
    {
        return nexus;
    }

    public T createNexus<T>(string name, Vector2 pos, float radius) where T : Nexus
    {
        return createNexus(name, typeof(T), pos, radius) as T;
    }

    public Nexus createNexus(string name, Type type, Vector2 pos, float radius)
    {
        var id = generateGUID();

        if (nexus)
        {
            logError("there is a nexus id : " + id + "! can not create again!");
            return null;
        }

        nexus = CLASS<Nexus>(type);
        nexus.setName(name);
        nexus.setNexusType(type);

        // 将角色挂接到管理器下
        nexus.setID(id);

        var path = $"{GAMEPLAY_PATH}/{name}.prefab";
        var o = mPrefabPoolManager.createObject(path, 0, false, true);
        nexus.setObject(o);

        nexus.init();
        return nexus;
    }

    void onNexusDead()
    {
        destroyNexus();
    }

    public void destroyNexus()
    {
        if (nexus == null)
            return;

        mPrefabPoolManager.destroyObject(nexus.gameObject, false);
        UN_CLASS(ref nexus);
    }
}