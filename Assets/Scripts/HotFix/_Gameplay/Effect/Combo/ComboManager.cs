using UnityEngine;

namespace MoreMountains;

// 角色管理器
public class ComboManager : FrameSystem
{
    Sprite[] comboSprites;

    public ComboManager()
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
    }

    public void load()
    {
        comboSprites = new Sprite[10];

        for (int i = 0; i < comboSprites.Length; i++)
        {
            var id = (i + 1) * 10;
            var path = $"{GAMEPLAY_PATH}/Sprites/Play/_Combo/combo_{id}.png";
            var sprite = resource.loadGameResource<Sprite>(path);
            comboSprites[i] = sprite.get();
        }
    }

    public void createComboEffect(int comboCount, Vector2 pos)
    {
        var e = CLASS<ComboEffect>();

        var path = $"{GAMEPLAY_PATH}/Prefabs/ComboEffect.prefab";
        var o = prefabPool.createObject(path);
        e.setObject(o);
        e.setName($"ComboEffect_{comboCount * 10}");
        e.setWorldPosition(pos);

        var index = comboCount - 1;
        if (comboSprites.tryGet(index, out var sprite))
        {
            e.setSprite(sprite);
        }
        
        e.setCombo(comboCount);
    }

    public void destroyComboEffect(ComboEffect effect)
    {
        if (effect == null)
            return;

        prefabPool.destroyObject(effect.gameObject, false);

        UN_CLASS(ref effect);
    }
}