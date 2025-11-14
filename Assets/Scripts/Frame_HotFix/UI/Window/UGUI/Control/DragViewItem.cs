public abstract class DragViewItem<DataType> : WindowRecyclableUGUI where DataType : ClassObject
{
    protected int index;

    protected DragViewItem(IWindowObjectOwner parent) : base(parent)
    {
    }

    public abstract void setData(DataType data);

    public void setIndex(int idx)
    {
        index = idx;
    }

    public int getIndex()
    {
        return index;
    }

    public override void update()
    {
        base.update();
    }
}