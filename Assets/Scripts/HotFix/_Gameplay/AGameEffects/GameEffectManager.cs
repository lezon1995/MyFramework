using System.Collections.Generic;

namespace MarbleHero;

public class GameEffectManager
{
    public GameEffectManager()
    {
    }

    public List<AGameEffect> topLevelEffects = new();
    public List<AGameEffect> topLevelEffectsQueue = new();
    public List<AGameEffect> effectList = new();
    public List<AGameEffect> effectsQueue = new();

    protected void updateTopLevelEffects(float dt)
    {
        for (var i = 0; i < topLevelEffects.Count;)
        {
            var e = topLevelEffects[i];
            if (e.update(dt))
            {
                topLevelEffects.RemoveAt(i);
                UN_CLASS(e);
            }
            else
                i++;
        }
    }

    protected void clearTopLevelEffects()
    {
        for (var i = topLevelEffects.Count - 1; i >= 0; i--)
        {
            var e = topLevelEffects[i];
            topLevelEffects.RemoveAt(i);
            UN_CLASS(e);
        }

        topLevelEffectsQueue.Clear();
    }

    protected void updateEffects(float dt)
    {
        for (var i = 0; i < effectList.Count;)
        {
            var e = effectList[i];
            if (e.update(dt))
            {
                effectList.RemoveAt(i);
                UN_CLASS(e);
            }
            else
                i++;
        }
    }

    protected void clearEffects()
    {
        for (var i = effectList.Count - 1; i >= 0; i--)
        {
            var e = effectList[i];
            effectList.RemoveAt(i);
            UN_CLASS(e);
        }

        effectsQueue.Clear();
    }

    public void update(float dt)
    {
        updateTopLevelEffects(dt);
        updateEffects(dt);

        effectList.AddRange(effectsQueue);
        effectsQueue.Clear();

        topLevelEffects.AddRange(topLevelEffectsQueue);
        topLevelEffectsQueue.Clear();
    }
    
    public void addToTop(AGameEffect effect)
    {
        topLevelEffects.Add(effect);
    }

    public void add(AGameEffect effect)
    {
        effectList.Add(effect);
    } 
    
    public void addToTop<T>() where T : AGameEffect
    {
        var effect = CLASS<AGameEffect>(typeof(T));
        topLevelEffects.Add(effect);
    }

    public void add<T>() where T : AGameEffect
    {
        var effect = CLASS<AGameEffect>(typeof(T));
        effectList.Add(effect);
    }

    public void clear()
    {
        clearEffects();
        clearTopLevelEffects();
    }
}