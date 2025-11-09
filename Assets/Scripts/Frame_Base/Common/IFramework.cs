public interface IFramework
{
    public void update(float dt);
    public void fixedUpdate(float dt);
    public void lateUpdate(float dt);
    public void drawGizmos();
    public void onApplicationFocus(bool focus);
    public void onApplicationQuit();
    public void destroy();
}