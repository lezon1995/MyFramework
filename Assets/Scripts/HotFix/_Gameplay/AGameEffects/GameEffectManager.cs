using System.Collections.Generic;

namespace MarbleHero;

public class GameEffectManager
{
    public GameEffectManager()
    {
    }

    public List<ARenderEffect> renderEffects = new();
    public List<ARenderEffect> renderEffectsQueue = new();
    public List<ALogicEffect> logicEffects = new();
    public List<ALogicEffect> logicEffectsQueue = new();

    protected void updateRenderEffects(float dt)
    {
        for (var i = 0; i < renderEffects.Count;)
        {
            var e = renderEffects[i];
            if (e.update(dt))
            {
                renderEffects.RemoveAt(i);
                UN_CLASS(e);
            }
            else
                i++;
        }
    }

    protected void clearRenderEffects()
    {
        for (var i = renderEffects.Count - 1; i >= 0; i--)
        {
            var e = renderEffects[i];
            renderEffects.RemoveAt(i);
            UN_CLASS(e);
        }

        renderEffectsQueue.Clear();
    }

    protected void fixedUpdateLogicEffects(float dt)
    {
        for (var i = 0; i < logicEffects.Count;)
        {
            var e = logicEffects[i];
            if (e.fixedUpdate(dt))
            {
                logicEffects.RemoveAt(i);
                UN_CLASS(e);
            }
            else
                i++;
        }
    }

    protected void updateLogicEffects(float dt)
    {
        for (var i = 0; i < logicEffects.Count; i++)
        {
            var e = logicEffects[i];
            e.update(dt);
        }
    }

    protected void clearLogicEffects()
    {
        for (var i = logicEffects.Count - 1; i >= 0; i--)
        {
            var e = logicEffects[i];
            logicEffects.RemoveAt(i);
            UN_CLASS(e);
        }

        logicEffectsQueue.Clear();
    }

    public void updateRender(float dt)
    {
        updateRenderEffects(dt);
        updateLogicEffects(dt);

        renderEffects.AddRange(renderEffectsQueue);
        renderEffectsQueue.Clear();
    }

    public void fixedUpdateLogic(float dt)
    {
        fixedUpdateLogicEffects(dt);
        
        logicEffects.AddRange(logicEffectsQueue);
        logicEffectsQueue.Clear();
    }
    
    public T addRender<T>() where T : ARenderEffect
    {
        var effect = CLASS<ARenderEffect>(typeof(T));
        renderEffects.Add(effect);
        return effect as T;
    }

    public T addLogic<T>() where T : ALogicEffect
    {
        var effect = CLASS<ALogicEffect>(typeof(T));
        logicEffects.Add(effect);
        return effect as T;
    }

    public void clear()
    {
        clearLogicEffects();
        clearRenderEffects();
    }
}