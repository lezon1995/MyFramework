using PrimeTween;
using UnityEngine;

namespace MarbleHero;

public abstract class IntentItem : ClassObject
{
    public abstract Intent type { get; }
    public abstract string name { get; }
    public string content => name;
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
        o = mPrefabPoolManager.createObject(path, 0, false, true);
        o.TryGetComponent(out rect);
        findComponent(o, "Rect", out iconRect);
        findComponent(o, "Rect", out iconCanvasGroup);
        findComponent(o, "Desc", out descCanvasGroup);

        button = LayoutScript.newUIObject<myUGUIButton>(o);
        button.setName(name);
        button.setUGUIButtonClick(onClick);
        button.setUGUIMouseEnter((pointer, go) => { Tween.Scale(o.transform, endValue: 1.2F, duration: 0.1F, ease: Ease.OutCubic); });
        button.setUGUIMouseExit((pointer, go) => { Tween.Scale(o.transform, endValue: 1F, duration: 0.1F, ease: Ease.OutCubic); });

        var t1 = button.transform.Find("Rect/Level");
        level = LayoutScript.newUIObject<myUGUIText>(button, null, t1.gameObject);

        var t2 = button.transform.Find("Rect/Icon");
        icon = LayoutScript.newUIObject<myUGUIImageSimple>(button, null, t2.gameObject);
        
        var t3 = button.transform.Find("Desc");
        description = LayoutScript.newUIObject<myUGUIText>(button, null, t3.gameObject);
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