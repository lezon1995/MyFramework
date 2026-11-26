
// 可回收对象接口,被引用计数管理的对象需实现此接口
public interface IRecyclable
{
	long id => getAssignID();
	public void setAssignID(long assignID);
	public long getAssignID();
}