using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static MathUtility;
using static WidgetUtility;
using static FrameUtility;

// 滑动区域窗口
// 如果希望Item既可以响应点击,ScrollRect又能够正常滑动,则需要Item是有button组件进行点击,这是UGUI的做法
// 如果使用GlobalTouchSystem则两者没有任何冲突,滑动和点击互不影响
public class myUGUIScrollRect : myUGUIObject
{
    protected ScrollRect scrollRect; // UGUI的ScrollRect组件
    protected myUGUIObject viewport; // ScrollRect下的Viewport节点
    protected myUGUIObject content; // Viewport下的Content节点
    protected Image scrollRectImage; // mScrollRect节点上的Image组件
    protected Image viewportImage; // mViewport节点上的Image组件

    public myUGUIScrollRect()
    {
        needUpdate = true;
    }

    public override void init()
    {
        base.init();
        if (!go.TryGetComponent(out scrollRect))
        {
            if (!isNewObject)
            {
                logError("需要添加一个ScrollRect组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            scrollRect = go.AddComponent<ScrollRect>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }
    }

    public void initScrollRect(myUGUIObject _viewport, myUGUIObject _content, float verticalPivot = 1.0f, float horizontalPivot = 0.5f)
    {
        scrollRect.content = _content.getRectTransform();
        scrollRect.viewport = _viewport.getRectTransform();
        content = _content;
        viewport = _viewport;
        setContentPivotVertical(verticalPivot);
        setContentPivotHorizontal(horizontalPivot);
        alignContentPivotVertical();
        alignContentPivotHorizontal();
        // 需要ScrollRect和viewport至少有一个的Image组件是开启射线检测的,不完全准确,但是这样保险一些
        go.TryGetComponent(out scrollRectImage);
        viewport.tryGetUnityComponent(out viewportImage);
        if (scrollRectImage == null || viewportImage == null)
        {
            logError("需要ScrollRect和viewport都有一个的Image组件,window:" + name + ", layout:" + layout.getName());
        }

        if (scrollRectImage)
            scrollRectImage.raycastTarget = true;

        if (viewportImage)
            viewportImage.raycastTarget = true;
    }

    public override void update(float dt)
    {
        base.update(dt);
        if (viewport == null || content == null)
        {
            logError("未找到viewport或content,请确保已经在布局的init中调用了initScrillRect函数进行ScrollRect的初始化");
        }

        // 始终确保ScrollRect和Viewport的宽高都是偶数,否则可能会引起在mContent调整位置时总是重新被ScrollRect再校正回来导致Content不停抖动
        makeSizeEven(this);
        makeSizeEven(viewport);
        // 矫正Content的位置,使之始终为整数
        if (scrollRect.vertical && isFloatZero(scrollRect.velocity.y) || scrollRect.horizontal && isFloatZero(scrollRect.velocity.x))
        {
            content.setPosition(round(content.getPosition()));
        }
    }

    public void setScrollEnable(bool enable)
    {
        scrollRect.StopMovement();
        scrollRect.enabled = enable;
    }

    public myUGUIObject getContent()
    {
        return content;
    }

    public myUGUIObject getViewport()
    {
        return viewport;
    }

    public ScrollRect getScrollRect()
    {
        return scrollRect;
    }

    // 设置content竖直方向的轴心
    // 1.0表示将content的轴心设置到最上边,使其顶部对齐viewport顶部
    // 0.5表示将content的轴心设置到中间,使其中间对齐viewport中间
    // 0.0表示将content的轴心设置到最下边,使其底部对齐viewport底部
    public void setContentPivotVertical(float pivot)
    {
        content.setPivot(new(content.getPivot().x, pivot));
    }

    // 设置content水平方向的轴心
    // 1.0表示将content的轴心设置到最右边,使其右边界对齐viewport右边界
    // 0.5表示将content的轴心设置到中间,使其中间对齐viewport中间
    // 0.0表示将content的轴心设置到最左边,使其左边界对齐viewport左边界
    public void setContentPivotHorizontal(float pivot)
    {
        content.setPivot(new(pivot, content.getPivot().y));
    }

    public Vector2 getNormalizedPosition()
    {
        return scrollRect.normalizedPosition;
    }

    // 设置Content的相对位置，x，y分别为横向和纵向，值的范围是0-1
    // 0表示Content的左边或下边与父节点的左边或下边对齐，1表示Content的右边或上边与父节点的右边或上边对齐
    public void setNormalizedPosition(Vector2 pos)
    {
        scrollRect.normalizedPosition = pos;
    }

    public void setNormalizedPositionX(float x)
    {
        scrollRect.horizontalNormalizedPosition = x;
    }

    public void setNormalizedPositionY(float y)
    {
        scrollRect.verticalNormalizedPosition = y;
    }

    // 根据content的pivot.y计算出并改变content的当前位置
    public void alignContentPivotVertical()
    {
        alignContentY(content.getPivot().y);
    }

    // 根据content的pivot.x计算出并改变content的当前位置
    public void alignContentPivotHorizontal()
    {
        alignContentX(content.getPivot().x);
    }

    // 使Content的上边界与ScrollRect的上边界对齐,实际上是跟Viewport对齐
    public void alignContentTop()
    {
        alignContentY(1.0f);
    }

    // 使Content的下边界与ScrollRect的下边界对齐,实际上是跟Viewport对齐
    public void alignContentBottom()
    {
        alignContentY(0.0f);
    }

    // 使Content的左边界与ScrollRect的左边界对齐,实际上是跟Viewport对齐
    public void alignContentLeft()
    {
        alignContentX(0.0f);
    }

    // 使Content的右边界与ScrollRect的右边界对齐,实际上是跟Viewport对齐
    public void alignContentRight()
    {
        alignContentX(1.0f);
    }

    public void autoAdjustContent(Vector2 itemSize)
    {
        autoAdjustContent(CONTENT_ADJUST.FIXED_WIDTH_OR_HEIGHT, itemSize);
    }

    public void autoAdjustContent(CONTENT_ADJUST type = CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE)
    {
        autoAdjustContent(type, Vector2.zero);
    }

    // 当子节点在不断增加,需要实时计算content的位置时使用
    // 自动排列content下的子节点,并且计算然后设置content的位置
    public void autoAdjustContent(CONTENT_ADJUST adjustType, Vector2 itemSize)
    {
        if (scrollRect.vertical)
        {
            switch (adjustType)
            {
                case CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE:
                    autoGridVertical(content);
                    break;
                case CONTENT_ADJUST.FIXED_WIDTH_OR_HEIGHT:
                    autoGridFixedRootWidth(content, itemSize);
                    break;
            }

            // 当Content的大小小于Viewport时,Content顶部对齐Viewport顶部(实际是根据content的pivot计算)
            if (viewport.getWindowSize().y >= content.getWindowSize().y)
            {
                alignContentY(content.getPivot().y);
            }
            // 当Content的大小大于Viewport时,Content底部对齐Viewport底部(实际是根据content的pivot计算)
            else
            {
                alignContentY(1.0f - content.getPivot().y);
            }
        }
        else if (scrollRect.horizontal)
        {
            switch (adjustType)
            {
                case CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE:
                    autoGridHorizontal(content);
                    break;
                case CONTENT_ADJUST.FIXED_WIDTH_OR_HEIGHT:
                    autoGridFixedRootHeight(content, itemSize);
                    break;
            }

            if (viewport.getWindowSize().x >= content.getWindowSize().x)
            {
                alignContentLeft();
            }
            else
            {
                alignContentRight();
            }
        }
    }

    // 设置Content的顶部在Viewport中的坐标,一般在Content高度变化时,会保持顶部的位置不变,向下拉伸Content长度
    public void setContentTopPos(float top)
    {
        content.setPositionY(top - content.getWindowSize().y * 0.5f);
        scrollRect.velocity = new(scrollRect.velocity.x, 0.0f);
    }

    public float getContentTopPos()
    {
        return content.getPosition().y + content.getWindowSize().y * 0.5f;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void alignContentY(float y)
    {
        scrollRect.velocity = new(scrollRect.velocity.x, 0.0f);
        scrollRect.verticalNormalizedPosition = y;
    }

    protected void alignContentX(float x)
    {
        scrollRect.velocity = new(0.0f, scrollRect.velocity.y);
        scrollRect.horizontalNormalizedPosition = x;
    }
}