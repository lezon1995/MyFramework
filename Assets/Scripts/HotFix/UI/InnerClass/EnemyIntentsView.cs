using System.Collections.Generic;
using MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public class EnemyIntentsView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject intentsParent;
	protected myUGUIObject[] intents = new myUGUIObject[5];
	protected myUGUIObject intentEffecting;
	protected WindowStructPool<EnemyIntentItem> EnemyIntentItemPool;
	// auto generate member end
	
	public Dictionary<int, EnemyIntentItem> intentItems = new();
	
	public EnemyIntentsView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		EnemyIntentItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out intentsParent, "V");
		for (int i = 0; i < intents.Length; ++i)
		{
			newObject(out intents[i], "V/Intent" + i.IToS());
		}
		newObject(out intentEffecting, "V/IntentEffecting");
		EnemyIntentItemPool.assignTemplate(mRoot, "V/EnemyIntentItem");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
		
		intents.For(o => o.setActive(false));
		intentEffecting.setActive(false);
	}
	public override void onShow()
	{
		base.onShow();
	}

	public void addIntent(EnemyMoveInfo info)
	{
		var item = EnemyIntentItemPool.newItem();
		item.setIntentType(info.intent);
		addIntent(info, item);
	}

	public void addIntent(EnemyMoveInfo info, EnemyIntentItem item)
	{
		item.setParent(intentsParent);
		var index = intentItems.Count;
		intentItems.add(info.index, item);
		var placeholder = intents.get(index);
		item.setAnchoredPosition(placeholder.getAnchoredPosition());
	}

	public void clearIntentItems()
	{
		EnemyIntentItemPool.unuseAll();
		intentItems.Clear();
	}

	public void updateIntentItemsPos(EnemyMoveInfo effectingMoveInfo, float t)
	{
		var index = effectingMoveInfo.index;
		foreach (var (idx, item) in intentItems)
		{
			if (idx == index)
			{
				var (startPos, endPos) = (intents[0].getAnchoredPosition(), intentEffecting.getAnchoredPosition());
				var pos = lerp(startPos, endPos, t);
				item.setAnchoredPosition(pos);
			}
			else if (idx < index)
			{
			}
			else
			{
				var (startPos, endPos) = (intents[idx - index].getAnchoredPosition(), intents[idx - index - 1].getAnchoredPosition());
				var pos = lerp(startPos, endPos, t);
				item.setAnchoredPosition(pos);
			}
		}
	}

	public void hideIntentItemBefore(EnemyMoveInfo effectingMoveInfo)
	{
		var index = effectingMoveInfo.index;
		foreach (var (idx, item) in intentItems)
		{
			if (idx < index)
				item.setActive(false);
		}
	}

	public void updateIntentItemScale(EnemyMoveInfo effectingMoveInfo, float t)
	{
		var item = intentItems[effectingMoveInfo.index];
		item.setScale(lerp(1F, 2F, t));
		item.setAlpha(lerp(1F, 0F, t));
		item.setDescAlpha(lerp(0F, 1F, t));
		item.setDescAnchoredPosition(t);
	}
}