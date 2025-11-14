using System.Threading;
using static FrameBaseUtility;
using static FrameBaseHotFix;

// 命令接收者基类,只有命令接收者的子类可以接收命令
public class CommandReceiver : ClassObject
{
    protected string name; // 接收者名字
    protected long delayCmdCountSubThread; // 子线程中此对象剩余未执行的延迟命令数量
    protected long delayCmdCountMainThread; // 主线程中此对象剩余未执行的延迟命令数量

    public override void resetProperty()
    {
        base.resetProperty();
        name = null;
        delayCmdCountSubThread = 0;
        delayCmdCountMainThread = 0;
    }

    public string getName()
    {
        return name;
    }

    public virtual void setName(string name)
    {
        this.name = name;
    }

    public void addReceiveDelayCmd()
    {
        if (isMainThread())
        {
            ++delayCmdCountMainThread;
        }
        else
        {
            Interlocked.Increment(ref delayCmdCountSubThread);
        }
    }

    public void removeReceiveDelayCmd()
    {
        if (isMainThread())
        {
            --delayCmdCountMainThread;
        }
        else
        {
            Interlocked.Decrement(ref delayCmdCountSubThread);
        }
    }

    public override void destroy()
    {
        base.destroy();
        // 通知命令系统有一个命令接受者已经被销毁了,需要取消命令缓冲区中的即将发给该接受者的命令
        if (Interlocked.Read(ref delayCmdCountSubThread) + delayCmdCountMainThread > 0)
        {
            mCommandSystem?.notifyReceiverDestroyed(this);
        }
    }
}