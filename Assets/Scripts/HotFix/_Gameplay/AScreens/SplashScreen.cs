using UnityEngine;

namespace MarbleHero;

public partial class SplashScreen
{
    Timer timer;
    // static float BOUNCE_DUR = 1.2F;
    // static float FADE_DUR = 3.0F;
    // static float WAIT_DUR = 1.5F;
    // static float FADE_OUT_DUR = 1.0F;
    static float BOUNCE_DUR = 0.25F;
    static float FADE_DUR = 0.25F;
    static float WAIT_DUR = 0.25F;
    static float FADE_OUT_DUR = 0.25F;
    Color color = new Color(1.0F, 1.0F, 1.0F, 0.0F);
    Color bgColor = new Color(0.0F, 0.0F, 0.0F, 1.0F);
    Color shadowColor = new Color(0.0F, 0.0F, 0.0F, 0.0F);
    Phase phase = Phase.INIT;
    public bool isDone;
    static float OFFSET_Y = 8.0F * Settings.scale;
    static float OFFSET_X = 12.0F * Settings.scale;
    float x = Settings.WIDTH / 2.0F;
    float y = Settings.HEIGHT / 2.0F - OFFSET_Y;
    float sX = Settings.WIDTH / 2.0F;
    float sY = Settings.HEIGHT / 2.0F;
    Color cream, bgBlue;
    bool playSfx;
    int sfxId = -1;
    string sfxKey = null;

    enum Phase
    {
        INIT,
        BOUNCE,
        FADE,
        WAIT,
        FADE_OUT
    }

    public override void onCtor()
    {
        ColorUtility.TryParseHtmlString("#ffffdbff", out cream);
        ColorUtility.TryParseHtmlString("#074254ff", out bgBlue);
    }

    public override void update(float dt)
    {
        base.update(dt);
        if ((InputHelper.justClickedLeft /*|| CInputActionSet.select.isJustPressed()*/) && phase != Phase.FADE_OUT)
        {
            phase = Phase.FADE_OUT;
            timer = FADE_OUT_DUR;
            if (sfxKey != null)
                sound.fadeOut(sfxKey, sfxId);
        }

        switch (phase)
        {
            case Phase.INIT:
                if (timer.update(dt))
                {
                    phase = Phase.BOUNCE;
                    timer = BOUNCE_DUR;
                }

                break;
            case Phase.BOUNCE:
                timer.update(dt);
                var t = timer.remain / BOUNCE_DUR;
                color.a = MMLerp.fade.apply(1.0F, 0.0F, t);
                y = MMLerp.elasticIn.apply(Settings.HEIGHT / 2.0F, Settings.HEIGHT / 2.0F - 200.0F * Settings.scale, t);
                if (timer.remain < 0.96000004F && !playSfx)
                {
                    playSfx = true;
                    sfxId = sound.play("SPLASH");
                }

                if (timer.finished)
                {
                    phase = Phase.FADE;
                    timer = FADE_DUR;
                }

                break;
            case Phase.FADE:
                timer.update(dt);
                var f = timer.remain / FADE_DUR;
                color.r = MMLerp.fade.apply(cream.r, 1.0F, f);
                color.g = MMLerp.fade.apply(cream.g, 1.0F, f);
                color.b = MMLerp.fade.apply(cream.b, 1.0F, f);
                bgColor.r = MMLerp.fade.apply(bgBlue.r, 0.0F, f);
                bgColor.g = MMLerp.fade.apply(bgBlue.g, 0.0F, f);
                bgColor.b = MMLerp.fade.apply(bgBlue.b, 0.0F, f);
                sX = MMLerp.exp5Out.apply(Settings.WIDTH / 2.0F + OFFSET_X, Settings.WIDTH / 2.0F, f);
                sY = MMLerp.exp5Out.apply(Settings.HEIGHT / 2.0F - OFFSET_Y, Settings.HEIGHT / 2.0F, f);
                if (timer.finished)
                {
                    phase = Phase.WAIT;
                    timer = WAIT_DUR;
                }

                break;
            case Phase.WAIT:
                if (timer.update(dt))
                {
                    phase = Phase.FADE_OUT;
                    timer = FADE_OUT_DUR;
                }

                break;
            case Phase.FADE_OUT:
                bgColor.a = MMLerp.fade.apply(0.0F, 1.0F, timer);
                color.a = MMLerp.fade.apply(0.0F, 1.0F, timer);
                if (timer.update(dt))
                {
                    isDone = true;
                }

                break;
        }
        
        setBgColor(bgColor);
        setLogoColor(color);
        using var _ = new MyStringBuilderScope(out var sb);
        sb.addLine($"phase={phase.ToString()}");
        sb.addLine($"timer={timer.remain:F2}");
        setDebugText(sb.ToString());
    }
}