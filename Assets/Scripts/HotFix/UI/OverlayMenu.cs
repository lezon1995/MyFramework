using System.Collections.Generic;
using Obfuz;
using UnityEngine;

namespace MarbleHero;

// auto generate member start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class OverlayMenu : LayoutScript
{
	protected ExpView mExpView;
	protected RelicsView mRelicsView;
	protected PlayerHealthView mPlayerHealthView;
	protected EnemyHealthView mEnemyHealthView;
	protected EnemyIntentsView mEnemyIntentsView;
	// auto generate member end
	
	public Intents intents;
	public OverlayMenu()
	{
		// auto generate constructor start
		mExpView = new(this);
		mRelicsView = new(this);
		mPlayerHealthView = new(this);
		mEnemyHealthView = new(this);
		mEnemyIntentsView = new(this);
		// auto generate constructor end
		mNeedUpdate = false;
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		mExpView.assignWindow(mRoot, "Content/Bot/ExpView");
		mRelicsView.assignWindow(mRoot, "Content/Left/PlayerInfo/RelicsView");
		mPlayerHealthView.assignWindow(mRoot, "Content/Left/PlayerInfo/PlayerHealthView");
		mEnemyHealthView.assignWindow(mRoot, "Content/Right/EnemyInfo/V/EnemyHealthView");
		mEnemyIntentsView.assignWindow(mRoot, "Content/Right/EnemyInfo/V/EnemyIntentsView");
		// auto generate assignWindow end

		// intents = new(mIntents);
		// expBar = new(mExpBar);
		// relics = new(mRelics);
		// playerInfo = new(mPlayerInfo);
		// enemyInfo = new(mEnemyInfo);
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
	}
	public override void onGameState()
	{
		base.onGameState();
	}

	public class Intents : UIObject
	{
		RectTransform intentsParent;
		public Vector2[] intentPositions;
		public Vector2 intentEffectingPos;
		public Dictionary<int, IntentItem> intentItems = new();

		public Intents(myUGUIObject t) : base(t)
		{
			intentsParent = t.find("V");
			intentPositions = new Vector2[5];
			for (var i = 0; i < intentPositions.Length; i++)
			{
				var id = i + 1;
				var tt = intentsParent.find($"Intent{id}");
				tt.gameObject.SetActive(false);
				intentPositions[i] = tt.anchoredPosition;
			}

			var t1 = intentsParent.find("IntentEffecting");
			t1.gameObject.SetActive(false);
			intentEffectingPos = t1.anchoredPosition;
		}

		public void addIntent(EnemyMoveInfo info)
		{
			IntentItem item = info.intent switch
			{
				Intent.BRICK_GENERATE_X => CLASS<BRICK_GENERATE>(),
				Intent.BRICK_MOVE_DOWN_X => CLASS<BRICK_MOVE_DOWN>(),
				_ => null
			};

			if (item)
				addIntent(info, item);
		}

		public void addIntent(EnemyMoveInfo info, IntentItem item)
		{
			item.setParent(intentsParent);
			var index = intentItems.Count;
			intentItems.add(info.index, item);
			var pos = intentPositions.get(index);
			item.setAnchoredPosition(pos);
		}

		public void clearIntentItems()
		{
			foreach (var (index, item) in intentItems)
				UN_CLASS(item);

			intentItems.Clear();
		}

		public void updateIntentItemsPos(EnemyMoveInfo effectingMoveInfo, float t)
		{
			var index = effectingMoveInfo.index;
			foreach (var (idx, item) in intentItems)
			{
				if (idx == index)
				{
					var (startPos, endPos) = (intentPositions[0], intentEffectingPos);
					var pos = lerp(startPos, endPos, t);
					item.setAnchoredPosition(pos);
				}
				else if (idx < index)
				{
				}
				else
				{
					var (startPos, endPos) = (intentPositions[idx - index], intentPositions[idx - index - 1]);
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
}
