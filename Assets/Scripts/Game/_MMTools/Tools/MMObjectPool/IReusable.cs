namespace MoreMountains.Tools;

public interface IReusable
{
    bool inUse { get; set; }
    void onAcquire();
    void onRelease();
}