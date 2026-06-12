using PrimeTween;
using UnityEngine;

namespace MarbleHero;

public abstract class IntentItem : ClassObject
{
    public abstract Intent type { get; }
    public abstract string name { get; }
    public string content => name;
    protected myUGUIObject obj;
    protected GameObject o;
    protected RectTransform rect, iconRect;
    protected myUGUIButton button;
    protected myUGUIImageSimple icon;
    protected myUGUIText level, description;
    protected CanvasGroup iconCanvasGroup, descCanvasGroup;

    float defaultDescY, targetDescY;

    public override void onCtor()
    {
        var path = $"{GAMEPLAY_PATH}/Prefabs/UI/IntentItem.prefab";
        o = mPrefabPoolManager.createObject(path);
        
        obj = LayoutScript.newUIObject<myUGUIObject>(o);

        obj.find(out rect);
        obj.find(out iconRect, "Rect");
        obj.find(out iconCanvasGroup, "Rect");
        obj.find(out descCanvasGroup, "Desc");

        obj.newObject(out button);
        button.setName(name);
        button.setUGUIButtonClick(onClick);
        button.setUGUIMouseEnter((pointer, go) => { Tween.Scale(o.transform, endValue: 1.2F, duration: 0.1F, ease: Ease.OutCubic); });
        button.setUGUIMouseExit((pointer, go) => { Tween.Scale(o.transform, endValue: 1F, duration: 0.1F, ease: Ease.OutCubic); });

        obj.newObject(out level, "Rect/Level");
        obj.newObject(out icon, "Rect/Icon");
        obj.newObject(out description, "Desc");

        description.setText(content);
        defaultDescY = description.getRectTransform().anchoredPosition.y;
        targetDescY = defaultDescY + 50;
    }

    public override void onCreate()
    {
        base.onCreate();
        setActive(true);
        setScale(1);
        setAlpha(1);
        setDescAlpha(0);
    }

    public override void destroy()
    {
        base.destroy();
        setActive(false);
        setScale(1);
        setAlpha(1);
        setDescAlpha(0);
    }

    public void setParent(Transform parent)
    {
        o.transform.SetParent(parent);
    }

    public void setAnchoredPosition(Vector2 pos)
    {
        rect.anchoredPosition = pos;
    }

    public void setScale(float scale)
    {
        iconRect.localScale = scale * Vector3.one;
    }

    public void setAlpha(float alpha)
    {
        iconCanvasGroup.alpha = alpha;
    }
    
    public void setDescAlpha(float alpha)
    {
        descCanvasGroup.alpha = alpha;
    }
    
    public void setDescAnchoredPosition(float t)
    {
        var y = lerp(defaultDescY, targetDescY, t);
        description.getRectTransform().anchoredPosition = new(0, y);
    }

    public void setActive(bool active)
    {
        o.SetActive(active);
    }

    protected virtual void onClick()
    {
    }
}

public class BRICK_GENERATE : IntentItem
{
    public override Intent type => Intent.BRICK_GENERATE_X;
    public override string name => "BRICK_GENERATE";
}

public class BRICK_MOVE_DOWN : IntentItem
{
    public override Intent type => Intent.BRICK_MOVE_DOWN_X;
    public override string name => "BRICK_MOVE_DOWN";
}