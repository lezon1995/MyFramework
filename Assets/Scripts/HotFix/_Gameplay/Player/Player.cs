namespace MarbleHero;

public class Player : MovableObject
{
    public bool isReturnBall;
    
    public void shotBall()
    {
        // CtrUI.instance.SetReturnBallButton(true);


        isReturnBall = false;
        // shotRot.transform.position = guideLine.transform.position;
        // shotRot.transform.rotation = guideLine.transform.rotation;
        //
        // CtrGame.instance.IsLock = true;
        //
        // StartCoroutine(ShotBallCo());
        // guideLine.GuidelineOff();
    }
}