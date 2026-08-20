namespace MoreMountains;

public partial class CharacterExpView
{
    public void refresh(float dt, Exp exp)
    {
        if (targetProgress != exp.progress)
        {
            if (exp.progress < targetProgress)
                curProgress = 0;
            else
                curProgress = targetProgress;

            targetProgress = exp.progress;
            timeElapsed = 0;
            curExp.setText(exp.currentExp.IToS());
            maxExp.setText(exp.currentLevelRequiredExp.IToS());
        }

        timeElapsed = (timeElapsed + dt).clamp(0, tweenDuration);
        var t = timeElapsed / tweenDuration;
        var f = lerp(curProgress, targetProgress, t);
        expBar.setFillPercent(f);
    }

    public void SetLevel(int lv)
    {
        textLevel.setText(lv);
    }

    public void SetExp(int cur, int max)
    {
        curExp.setText(cur);
        maxExp.setText(max);
        expBar.setFillPercent(cur / (float)max);
    }
}