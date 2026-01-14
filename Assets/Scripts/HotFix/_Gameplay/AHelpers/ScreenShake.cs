using UnityEngine;

namespace MarbleHero;

public class ScreenShake
{
    float x;
    float duration;
    float startDuration;
    float intensityValue;
    float intervalSpeed;
    bool vertical;

    GameCamera camera;

    Camera mainCamera;
    Vector3 cameraInitialPosition;
    Timer cameraShakeTimer;
    float cameraShakePower;

    public ScreenShake(GameCamera c)
    {
        camera = c;
        mainCamera = c.getCamera();
        cameraInitialPosition = mainCamera.transform.localPosition;
    }

    public enum ShakeIntensity
    {
        LOW,
        MED,
        HIGH
    }

    public enum ShakeDur
    {
        SHORT,
        MED,
        LONG,
        XLONG
    }

    public void shake(ShakeIntensity intensity, ShakeDur dur, bool isVertical)
    {
        duration = getDuration(dur);
        startDuration = duration;
        intensityValue = getIntensity(intensity);
        vertical = isVertical;
        intervalSpeed = 0.3F;
    }


    public void shakeCamera(float power, float time = 0.2F)
    {
        cameraShakePower = power;
        cameraShakeTimer = time;
    }

    public void rumble(float dur)
    {
        duration = dur;
        startDuration = dur;
        intensityValue = 10.0F;
        vertical = false;
        intervalSpeed = 0.7F;
    }

    public void mildRumble(float dur)
    {
        duration = dur;
        startDuration = dur;
        intensityValue = 3.0F;
        vertical = false;
        intervalSpeed = 0.7F;
    }

    public void update(float dt)
    {
        handleCameraShake(dt);

        if (Settings.HORIZ_LETTERBOX_AMT != 0 || Settings.VERT_LETTERBOX_AMT != 0)
            return;

        if (duration != 0.0F)
        {
            duration -= dt;
            if (duration < 0.0F)
            {
                duration = 0.0F;
                mainCamera.transform.localPosition = cameraInitialPosition;
                return;
            }

            float tmp = MMLerp.fade.apply(0.1F, intensityValue, duration / startDuration);
            x = MathUtils.cosDeg(TimeUtility.getNowTimeStampMS() % 360L / intervalSpeed) * tmp;
            if (Settings.SCREEN_SHAKE)
            {
                // if (vertical)
                //     viewport.update(Settings.M_W, (int) (Settings.M_H + abs(x)));
                // else
                //     viewport.update((int) (Settings.M_W + x), Settings.M_H);
            }
        }
    }

    void handleCameraShake(float dt)
    {
        if (cameraShakeTimer)
        {
            Vector3 pos = cameraInitialPosition + Random.insideUnitSphere * cameraShakePower;
            pos.z = cameraInitialPosition.z;
            mainCamera.transform.localPosition = pos;
            if (cameraShakeTimer.update(dt))
            {
                mainCamera.transform.localPosition = cameraInitialPosition;
            }
        }
    }

    float getIntensity(ShakeIntensity intensity)
    {
        switch (intensity)
        {
            case ShakeIntensity.LOW:
                return 20.0F * Settings.scale;
            case ShakeIntensity.MED:
                return 50.0F * Settings.scale;
        }

        return 100.0F * Settings.scale;
    }

    float getDuration(ShakeDur dur)
    {
        switch (dur)
        {
            case ShakeDur.SHORT:
                return 0.3F;
            case ShakeDur.MED:
                return 0.5F;
            case ShakeDur.LONG:
                return 1.0F;
            case ShakeDur.XLONG:
                return 3.0F;
        }

        return 1.0F;
    }
}