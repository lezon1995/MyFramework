using System;
using System.Collections.Generic;
using Obfuz;
using PrimeTween;

namespace MarbleHero;

// auto generate member start
// generate from:Assets/GameResources/UI/UIPrefab/RewardChoosePanel.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class RewardChoosePanel : LayoutScript
{
	protected myUGUIButton mTemplate1;
	protected myUGUIButton mTemplate2;
	protected myUGUIButton mTemplate3;
	// auto generate member end

	static Action onChose;
	List<Item> items = new();
	
	public RewardChoosePanel()
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out myUGUIObject content, "Content", false);
		newObject(out myUGUIObject mid, content, "Mid", false);
		newObject(out myUGUIObject h, mid, "H", false);
		newObject(out mTemplate1, h, "Template1");
		newObject(out mTemplate2, h, "Template2");
		newObject(out mTemplate3, h, "Template3");
		// auto generate assignWindow end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
		
		var item1 = CLASS<Item>();
		item1.with(mTemplate1);
		var item2 = CLASS<Item>();
		item2.with(mTemplate2);
		var item3 = CLASS<Item>();
		item3.with(mTemplate3);
		items.add(item1);
		items.add(item2);
		items.add(item3);
	}
	public override void onGameState()
	{
		base.onGameState();
	}

	public override void destroy()
	{
		UN_CLASS_LIST(items);
		base.destroy();
	}

	public override void onHide()
	{
		base.onHide();
	}

	public override void close()
	{
		base.close();
	}

	public void setOnChose(Action value)
	{
		onChose = value;
	}

	class Item : ClassObject
		, IArgs<myUGUIButton>
		, IRefresh<string>
	{
		myUGUIButton button;
		myUGUIText title, desc;
		string relicId;

		public void onCreate(myUGUIButton btn)
		{
			button = btn;
			button.setUGUIButtonClick(onClick);
			button.setUGUIMouseEnter((pointer, go) => { Tween.Scale(button.transform, endValue: 1.2F, duration: 0.1F, ease: Ease.OutCubic); });
			button.setUGUIMouseExit((pointer, go) => { Tween.Scale(button.transform, endValue: 1F, duration: 0.1F, ease: Ease.OutCubic); });

			button.newObject(out title, "Title");
			button.newObject(out desc, "Desc");
		}

		public void refresh(string id)
		{
			relicId = id;
			title.setText(RelicLibrary.getRelic(relicId).relicId);
			desc.setText(RelicLibrary.getRelic(relicId).relicId);
			button.setScale(1);
		}

		void onClick()
		{
			RelicLibrary.getRelic(relicId).makeCopy().instantObtain(player, player.relics.Count , true);
			onChose?.Invoke();
		}
	}
}

public partial class RewardChoosePanel : IArgs<string, string ,string>
{
	public void onCreate(string p1, string p2, string p3)
	{
		items.element(1).refresh(p1);
		items.element(2).refresh(p2);
		items.element(3).refresh(p3);
	}
}