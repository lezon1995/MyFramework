using System;
using Obfuz;
using PrimeTween;

namespace MarbleHero;

// auto generate member start
// generate from:Assets/GameResources/UI/UIPrefab/RewardChoosePanel.prefab
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class RewardChoosePanel : LayoutScript
{
	protected myUGUIObject mTemplate;
	// auto generate member end

	static Action onChose;
	Item item;
	
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
		newObject(out mTemplate, h, "Template");
		// auto generate assignWindow end
	}
	public override void init()
	{
		base.init();
		
		mTemplate.setActive(true);

		var o = LayoutScript.newUIObject<myUGUIObject>(mTemplate.getParent(), null, mTemplate.gameObject);
		item = CLASS<Item>();
		item.with(o, ImpactHammer.ID);
	}
	public override void onGameState()
	{
		base.onGameState();
	}

	public override void destroy()
	{
		base.destroy();
		UN_CLASS(ref item);
	}

	public void setOnChose(Action value)
	{
		onChose = value;
	}

	class Item : ClassObject, IArgs<myUGUIObject, string>
	{
		myUGUIObject obj;
		myUGUIButton button;
		myUGUIText title, desc;
		string relicId;

		public void onCreate(myUGUIObject o, string relic)
		{
			obj = o;
			relicId = relic;
			obj.newObject(out button);
			button.setUGUIButtonClick(onClick);
			button.setUGUIMouseEnter((pointer, go) => { Tween.Scale(obj.transform, endValue: 1.2F, duration: 0.1F, ease: Ease.OutCubic); });
			button.setUGUIMouseExit((pointer, go) => { Tween.Scale(obj.transform, endValue: 1F, duration: 0.1F, ease: Ease.OutCubic); });

			obj.newObject(out title, "Title");
			obj.newObject(out desc, "Desc");
			
			title.setText(RelicLibrary.getRelic(relicId).relicId);
			desc.setText(RelicLibrary.getRelic(relicId).relicId);
		}

		void onClick()
		{
			RelicLibrary.getRelic(relicId).makeCopy().instantObtain(player, player.relics.Count , true);
			onChose?.Invoke();
		}
	}
}
