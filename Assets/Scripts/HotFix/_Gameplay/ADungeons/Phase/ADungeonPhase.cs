using System;

namespace MoreMountains;

public enum DungeonPhaseType
{
    NONE,
    SELECT_CHARACTER,//角色选择
    SELECT_WEAPON,//武器选择
    SELECT_DIFFICULTY,//难度选择
}

public abstract class ADungeonPhase : IDisposable
{
    protected ADungeon _dungeon;
    protected float timeElapsed;

    protected ADungeonPhase(ADungeon dungeon)
    {
        _dungeon = dungeon;
    }

    public virtual void onBegin(ADungeonPhase last)
    {
        timeElapsed = 0F;
        onBindListeners();
    }

    public virtual void update(float dt)
    {
        timeElapsed += dt;
        // Draw.ingame.xy.Label2D(new Vector2(Screen.width / 4F, 0F), $"({timeElapsed:F2}) {GetType().Name}", 20, LabelAlignment.Center, Color.darkOrange);
    }

    public virtual void fixedUpdate(float dt)
    {
    }

    public virtual void onEnd()
    {
        timeElapsed = 0F;
        onUnbindListeners();
    }

    protected abstract void onBindListeners();
    protected abstract void onUnbindListeners();


    public virtual void Dispose()
    {
    }
}