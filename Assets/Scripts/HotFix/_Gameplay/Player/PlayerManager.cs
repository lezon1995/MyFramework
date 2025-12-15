using System;
using UnityEngine;

namespace MarbleHero;

// 角色管理器
public class PlayerManager : FrameSystem
{
    protected Player player;

    public PlayerManager()
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
        destroyPlayer();
        player = null;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        if (player && player.isActiveInHierarchy())
        {
            var dt = !player.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime;
            player.update(dt);
        }
    }

    public override void lateUpdate(float elapsedTime)
    {
        base.lateUpdate(elapsedTime);
        if (player && player.isActiveInHierarchy())
        {
            var dt = !player.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime;
            player.lateUpdate(dt);
        }
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);
        if (player && player.isActiveInHierarchy())
        {
            var dt = !player.isIgnoreTimeScale() ? elapsedTime : Time.fixedUnscaledDeltaTime;
            player.fixedUpdate(dt);
        }
    }

    public Player getPlayer()
    {
        return player;
    }

    public T createPlayer<T>(string name) where T : Player
    {
        return createPlayer(name, typeof(T)) as T;
    }

    public Player createPlayer(string name, Type type)
    {
        var id = generateGUID();

        if (player)
        {
            logError("there is a player id : " + id + "! can not create again!");
            return null;
        }

        player = CLASS<Player>();
        player.setName(name);
        player.setObject(getRootGameObject("Player"));
        player.init();

        return player;
    }

    void onPlayerDead()
    {
        destroyPlayer();
    }

    public void destroyPlayer()
    {
        if (player == null)
            return;

        UN_CLASS(ref player);
    }
}