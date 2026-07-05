using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

public class FxMaster
{
    const string VFX_DIR = $"{GAMEPLAY_PATH}/Effects/";
    Dictionary<FxDefine, Vfx> map = new();

    public FxMaster()
    {
        long startTime = TimeUtility.getNowTimeStampMS();
        map.Add(FxDefine.BALL_HIT_BRICK, load("Fx_BallHitBrick.prefab"));
        map.Add(FxDefine.BRICK_DESTROY, load("Fx_BrickDestroy.prefab"));

        log("Loaded " + map.Count + " Visual Effects");
        log("VFX load time: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
    }

    Vfx load(string filename) => new(VFX_DIR + filename);

    public int play(FxDefine key) => play(key, Vector3.zero, 0F);
    public int play(FxDefine key, Vector3 pos) => play(key, pos, 0F);

    public int play(FxDefine key, Vector3 pos, float lifeTime)
    {
        if (map.TryGetValue(key, out var vfx))
        {
            return vfx.play(pos, lifeTime);
        }

        log("Missing VFX: " + key);
        return 0;
    }
}