using UnityEngine;

namespace MarbleHero;

public static class RelicExtensions
{
    public static Sprite getSprite(this ARelic relic)
    {
        var path = $"{GAMEPLAY_PATH}/Sprites/Relics/{relic.imgUrl}";
        var resource = mResourceManager.loadGameResource<Sprite>(path);
        return resource.getResource();
    }
}