using System.Collections.Generic;
using Obfuz;
using UnityEngine;

namespace MarbleHero;

// auto generate member start
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class OverlayMenu : LayoutScript
{
	protected myUGUIObject mPlayerInfo;
	protected myUGUIObject mEnemyInfo;
	// auto generate member end
	
	public Intents intents;
	public PlayerInfo playerInfo;
	public EnemyInfo enemyInfo;
	public OverlayMenu()
	{
		// auto generate constructor start
		// auto generate constructor end
		mNeedUpdate = false;
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out myUGUIObject content, "Content", false);
		newObject(out myUGUIObject left, content, "Left", false);
		newObject(out mPlayerInfo, left, "PlayerInfo");
		newObject(out myUGUIObject right, content, "Right", false);
		newObject(out mEnemyInfo, right, "EnemyInfo");
		// auto generate assignWindow end

		intents = new(mRoot.transform.find("Intents"));
		playerInfo = new(this);
		enemyInfo = new(this);
	}
	public override void init()
	{
		base.init();
	}
	public override void onGameState()
	{
		base.onGameState();
	}

	public class Intents
	{
		Transform intentsParent;
		public Vector2[] intentPositions;
		public Vector2 intentEffectingPos;
		public Dictionary<int, IntentItem> intentItems = new();

		public Intents(Transform t)
		{
			intentsParent = t.Find("V");
			intentPositions = new Vector2[5];
			for (var i = 0; i < intentPositions.Length; i++)
			{
				var id = i + 1;
				var tt = (RectTransform)intentsParent.Find($"Intent{id}");
				tt.gameObject.SetActive(false);
				intentPositions[i] = tt.anchoredPosition;
			}

			var t1 = ((RectTransform)intentsParent.Find("IntentEffecting"));
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
