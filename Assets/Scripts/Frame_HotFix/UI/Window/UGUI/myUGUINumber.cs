using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static MathUtility;

// 可显示数字的窗口,支持带+-符号,小数点
// 性能不如myUGUIImageNumber,如果有性能需求,首选myUGUIImageNumber
public class myUGUINumber : myUGUIImage
{
    protected List<myUGUIImageSimple> numbers = new(); // 数字窗口列表
    protected DOCKING_POSITION dockingPosition; // 数字停靠方式
    protected NUMBER_DIRECTION direction; // 数字显示方向
    protected Sprite[] sprites; // 该列表只有10个数字的图片
    protected Sprite addSprite; // +号的图片
    protected Sprite minusSprite; // -号的图片
    protected Sprite dotSprite; // .号的图片
    protected string numberStyle; // 数字图集名
    protected string number = EMPTY; // 当前显示的数字
    protected const char ADD_MARK = '+'; // +号
    protected const char MINUS_MARK = '-'; // -号
    protected const char DOT_MARK = '.'; // .号
    protected static string allMark = EMPTY + ADD_MARK + MINUS_MARK + DOT_MARK; // 允许显示的除数字以外的符号
    protected int interval = 5; // 数字显示间隔
    protected int maxCount; // 数字最大个数

    public myUGUINumber()
    {
        sprites = new Sprite[10];
        dockingPosition = DOCKING_POSITION.LEFT;
        direction = NUMBER_DIRECTION.HORIZONTAL;
    }

    public override void init()
    {
        base.init();
        if (image == null)
            return;

        numberStyle = image.sprite.name.rangeToLast('_');
        for (int i = 0; i < 10; ++i)
        {
            sprites[i] = getSpriteInAtlas(numberStyle + "_" + IToS(i));
        }

        addSprite = getSpriteInAtlas(numberStyle + "_add");
        minusSprite = getSpriteInAtlas(numberStyle + "_minus");
        dotSprite = getSpriteInAtlas(numberStyle + "_dot");
        setMaxCount(10);
        image.enabled = false;
    }

    public override void notifyAnchorApply()
    {
        base.notifyAnchorApply();
        // 此处默认数字窗口都是以ASPECT_BASE.AB_AUTO的方式等比放大
        Vector2 screenScale = getScreenScale(ASPECT_BASE.AUTO);
        interval = (int)(interval * (direction == NUMBER_DIRECTION.HORIZONTAL ? screenScale.x : screenScale.y));
    }

    public override void cloneFrom(myUGUIObject obj)
    {
        base.cloneFrom(obj);
        var source = obj as myUGUINumber;
        interval = source.interval;
        numberStyle = source.numberStyle;
        number = source.number;
        addSprite = source.addSprite;
        minusSprite = source.minusSprite;
        dotSprite = source.dotSprite;
        int count = sprites.Length;
        for (int i = 0; i < count; ++i)
        {
            sprites[i] = source.sprites[i];
        }

        direction = source.direction;
        dockingPosition = source.dockingPosition;
        setMaxCount(source.maxCount);
    }

    // 获得内容横向排列时的实际显示内容总宽度
    public int getContentWidth()
    {
        int width = 0;
        int count = getMin(numbers.Count, number.Length);
        for (int i = 0; i < count; ++i)
        {
            width += (int)numbers[i].getWindowSize().x;
        }

        return width + interval * (number.Length - 1);
    }

    // 获得内容横向排列时的图片总宽度
    public int getAllSpriteWidth()
    {
        int width = 0;
        int count = getMin(numbers.Count, number.Length);
        for (int i = 0; i < count; ++i)
        {
            width += (int)numbers[i].getSpriteSize().x;
        }

        return width + interval * (number.Length - 1);
    }

    // 获得内容纵向排列时的实际显示内容总高度
    public int getContentHeight()
    {
        int height = 0;
        int count = getMin(numbers.Count, number.Length);
        for (int i = 0; i < count; ++i)
        {
            height += (int)numbers[i].getWindowSize().y;
        }

        return height + interval * (number.Length - 1);
    }

    // 获得内容纵向排列时的图片总高度
    public int getAllSpriteHeight()
    {
        int height = 0;
        int count = getMin(numbers.Count, number.Length);
        for (int i = 0; i < count; ++i)
        {
            height += (int)numbers[i].getSpriteSize().y;
        }

        return height + interval * (number.Length - 1);
    }

    public void setInterval(int i)
    {
        interval = i;
        refreshNumber();
    }

    public void setDockingPosition(DOCKING_POSITION p)
    {
        dockingPosition = p;
        refreshNumber();
    }

    public void setDirection(NUMBER_DIRECTION d)
    {
        direction = d;
        refreshNumber();
    }

    public void setMaxCount(int count)
    {
        if (maxCount == count)
            return;

        maxCount = count;
        // 设置的数字字符串不能超过最大数量
        if (number.Length > maxCount)
        {
            number = number.startString(maxCount);
        }

        numbers.Clear();
        for (int i = 0; i < maxCount + 1; ++i)
        {
            numbers.Add(layout.getScript().createUGUIObject<myUGUIImageSimple>(this, name + "_" + IToS(i), true));
        }

        refreshNumber();
    }

    public void setNumber(int num, int limitLen = 0)
    {
        setNumber(IToS(num, limitLen));
    }

    public void setNumber(string num)
    {
        number = num;
        checkUIntString(number, allMark);
        // 设置的数字字符串不能超过最大数量
        if (number.Length > maxCount)
        {
            number = number.startString(maxCount);
        }

        refreshNumber();
    }

    public int getMaxCount()
    {
        return maxCount;
    }

    public string getNumber()
    {
        return number;
    }

    public int getInterval()
    {
        return interval;
    }

    public string getNumberStyle()
    {
        return numberStyle;
    }

    public DOCKING_POSITION getDockingPosition()
    {
        return dockingPosition;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void refreshNumber()
    {
        if (number.isEmpty())
        {
            foreach (myUGUIImageSimple item in numbers)
            {
                item.getImage().enabled = false;
            }

            return;
        }

        int numberStartPos = getFirstNumberPos(number);
        // 数字前最多只允许有一个加号或者减号
        if (numberStartPos == 1)
        {
            if (number[0] == ADD_MARK)
            {
                numbers[0].setSpriteOnly(addSprite);
            }
            else if (number[0] == MINUS_MARK)
            {
                numbers[0].setSpriteOnly(minusSprite);
            }
        }

        int dotPos = number.LastIndexOf(DOT_MARK);
        if (dotPos == 0 || dotPos == number.Length - 1)
        {
            logError("number can not start or end with dot!");
            return;
        }

        // 只有整数,且不带符号
        if (dotPos == -1 && numberStartPos == 0)
        {
            for (int i = 0; i < number.Length; ++i)
            {
                numbers[i].setSpriteOnly(sprites[number[i] - '0']);
            }
        }
        // 带小数或者带符号
        else
        {
            // 整数部分
            // 带符号
            if (numberStartPos > 0)
            {
                string intPart = number.range(numberStartPos, dotPos);
                for (int i = 0; i < intPart.Length; ++i)
                {
                    numbers[i + numberStartPos].setSpriteOnly(sprites[intPart[i] - '0']);
                }
            }
            // 不带符号
            else
            {
                string intPart = dotPos != -1 ? number.startString(dotPos) : number;
                for (int i = 0; i < intPart.Length; ++i)
                {
                    numbers[i].setSpriteOnly(sprites[intPart[i] - '0']);
                }
            }

            // 小数点和小数部分
            if (dotPos != -1)
            {
                numbers[dotPos].setSpriteOnly(dotSprite);
                string floatPart = number.removeStartCount(dotPos + 1);
                for (int i = 0; i < floatPart.Length; ++i)
                {
                    numbers[i + dotPos + 1].setSpriteOnly(sprites[floatPart[i] - '0']);
                }
            }
        }

        // 根据当前窗口的大小调整所有数字的大小
        Vector2 windowSize = getWindowSize();
        int numberLength = number.Length;
        Vector2 numberSize = Vector2.zero;
        float numberScale = 0.0f;
        Sprite sprite = sprites[number[numberStartPos] - '0'];
        if (direction == NUMBER_DIRECTION.HORIZONTAL)
        {
            float inverseHeight = divide(1.0f, sprite.rect.height);
            numberSize.x = windowSize.y * sprite.rect.width * inverseHeight;
            numberSize.y = windowSize.y;
            numberScale = windowSize.y * inverseHeight;
        }
        else if (direction == NUMBER_DIRECTION.VERTICAL)
        {
            float inverseWidth = divide(1.0f, sprite.rect.width);
            numberSize.x = windowSize.x;
            numberSize.y = windowSize.x * sprite.rect.height * inverseWidth;
            numberScale = windowSize.x * inverseWidth;
        }

        for (int i = 0; i < numberLength; ++i)
        {
            char curChar = number[i];
            if (curChar <= '9' && curChar >= '0')
            {
                numbers[i].setWindowSize(numberSize);
            }
            else if (curChar == DOT_MARK)
            {
                numbers[i].setWindowSize(dotSprite.rect.size * numberScale);
            }
            else if (curChar == ADD_MARK)
            {
                numbers[i].setWindowSize(addSprite.rect.size * numberScale);
            }
            else if (curChar == MINUS_MARK)
            {
                numbers[i].setWindowSize(minusSprite.rect.size * numberScale);
            }
        }

        int count = numbers.Count;
        for (int i = 0; i < count; ++i)
        {
            numbers[i].getImage().enabled = i < numberLength;
        }

        // 调整窗口位置,隐藏不需要显示的窗口
        Vector2 leftOrTop = Vector2.zero;
        if (direction == NUMBER_DIRECTION.HORIZONTAL)
        {
            int contentWidth = getContentWidth();
            if (dockingPosition == DOCKING_POSITION.RIGHT)
            {
                leftOrTop = new Vector2(windowSize.x - contentWidth, 0) - windowSize * 0.5f;
            }
            else if (dockingPosition == DOCKING_POSITION.CENTER)
            {
                leftOrTop = new Vector2((windowSize.x - contentWidth) * 0.5f, 0) - windowSize * 0.5f;
            }
            else if (dockingPosition == DOCKING_POSITION.LEFT)
            {
                leftOrTop = -windowSize * 0.5f;
            }

            for (int i = 0; i < numberLength; ++i)
            {
                myUGUIImageSimple number = numbers[i];
                Vector2 size = number.getWindowSize();
                number.setPosition(leftOrTop + size * 0.5f);
                leftOrTop.x += size.x + interval;
            }
        }
        else if (direction == NUMBER_DIRECTION.VERTICAL)
        {
            int contentHeight = getContentHeight();
            if (dockingPosition == DOCKING_POSITION.BOTTOM)
            {
                leftOrTop = new Vector2(0, windowSize.y - contentHeight) - new Vector2(-windowSize.x * 0.5f, windowSize.y * 0.5f);
            }
            else if (dockingPosition == DOCKING_POSITION.CENTER)
            {
                leftOrTop = new Vector2(0, (windowSize.y - contentHeight) * 0.5f) - new Vector2(-windowSize.x * 0.5f, windowSize.y * 0.5f);
            }
            else if (dockingPosition == DOCKING_POSITION.TOP)
            {
                leftOrTop = new(-windowSize.x * 0.5f, windowSize.y * 0.5f);
            }

            for (int i = 0; i < numberLength; ++i)
            {
                myUGUIImageSimple number = numbers[i];
                Vector2 size = number.getWindowSize();
                number.setPosition(leftOrTop + new Vector2(size.x * 0.5f, -size.y * 0.5f));
                leftOrTop.y -= size.y + interval;
            }
        }
    }
}