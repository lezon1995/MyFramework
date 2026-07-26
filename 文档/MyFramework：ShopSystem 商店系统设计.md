# MyFramework：ShopSystem 商店系统设计

> 子文档，承接总览 [球管理·商店·背包 系统拆分与协作设计](MyFramework:球管理·商店·背包%20系统拆分与协作设计.md)。

---

## 一、目标与边界

### 1.1 系统目标

商店系统（`ShopSystem`，简称 `SS`）模拟**「云顶之弈棋盘式商店」**在战备阶段的体验，整体目标：

- 在战备阶段为玩家提供 **X 个随机球商品**
- 玩家可点击 **购买 / 重新随机（扣金币） / 下一步**
- 球流程结束后，再展示 **X 个随机遗物商品**
- 玩家可购买 / 重新随机 / 下一步
- 整段流程结束 → 通知波次系统进入下一波

### 1.2 不在范围内

| 不在范围内 | 谁来做 |
| --- | --- |
| 球的升级 / 融合 | `BallManagementSystem` |
| 球 / 遗物的背包格子 | `InventorySystem` |
| 金币增减 | `CoinSystem` |
| 战斗波次推进 | `WaveSystem` |
| 商店面板 UI 视觉 | `UI` 层（订阅 ShopEvents 重绘） |

### 1.3 与其他系统的契约

`SS` **只对外暴露两个动作**：

1. **进入商店**：`SS.EnterShop(ShopBoardKind)`，由 `WaveSystem` 在阶段切换时调用
2. **退出商店**：`SS.ExitShop()`，由 `SS` 内部阶段推进完毕或玩家主动点击"跳过"时调用，**回送** `WaveManager.RequestNextPhase(...)`

`SS` **只对外暴露一个查询**：

- `SS.OfferCount { get; }` 当前展柜的可购买商品总数

`SS` **只消费其他系统的两个能力**：

- `InventorySystem.CanAddBall()` / `CanAddRelic()`
- `CoinManager.CanPay(amount)` + `Pay/Earn(amount, reason)`

`SS` **不知道**球是什么、遗物怎么生效 —— 它只看到 `IPurchasable` 和 `IInventoryItem`。

---

## 二、领域模型

### 2.1 `ShopController` — 主控

商店的最核心调度器，状态机 `Idle / ShowingBallBoard / ShowingRelicBoard / Done`：

```text
class ShopController {
    enum BoardKind { Ball, Relic }
    enum BoardState { Idle, ShowingBallBoard, ShowingRelicBoard, Done }

    BoardState State;

    // ── 由 WaveSystem 在阶段 = SHOPPING 时调用
    public void EnterShop() {
        // 进入第一阶段：随机 X 个球商品
        OpenBoard(BoardKind.Ball);
    }

    // 内部推进 / 外部也可点「下一步」调用
    public void OnPlayerClickNext() {
        if (State == ShowingBallBoard)  OpenBoard(BoardKind.Relic);
        else if (State == ShowingRelicBoard) FinishShopAndNotify();
        else { /* idle, do nothing */ }
    }

    // ── 玩家点击「重新随机」
    public bool OnPlayerClickReroll() {
        var cost = CurrentBoardRerollCost();
        if (!CoinManager.Instance.CanPay(cost)) {
            Toast.Show("金币不足以重新随机");
            return false;
        }
        CoinManager.Pay(cost, "shop_reroll");
        RerollCurrentBoard();
        return true;
    }

    // ── 玩家点击商品卡
    public bool OnPlayerClickOffer(ShopOffer offer) {
        if (offer.Sold) return false;
        return TryBuyOffer(offer);
    }

    // ── 玩家在背包里点"出售"也走 SS 一侧
    public bool OnPlayerSellFromBag(IInventoryItem item) {
        return SellService.Sell(item);
    }
}
```

### 2.2 `ShopOffer` — 商品卡（球、遗物的通用包装）

`IPurchasable` 是一个解耦的关键接口，它的实现类不直接出现在 SS 的视野里。

```text
interface IPurchasable {
    ItemKind Kind { get; }            // Ball / Relic
    IInventoryItem Prototype { get; } // 模板（玩家尚未拥有时按这个显示）
    int Price { get; }
    bool Sold { get; }
    void MarkSold();
}

class ShopOffer : IPurchasable {
    public ItemKind Kind;
    public IInventoryItem Prototype;
    public int Price;
    public bool Sold;

    // 重新随机时，这个对象会被替换掉
    public void Refresh(ShopOffer newOffer) { /* 由 SS 调用 */ }
}
```

### 2.3 `BallOffer` / `RelicOffer` — 球 / 遗物各自的 "Proxied Offer"

```text
class BallOffer : IPurchasable {
    public BallDef Def;
    public ItemKind Kind => ItemKind.Ball;
    public IInventoryItem Prototype => Def.ToDisplayItem();   // 一个不动数据的"展示实例"
    public int Price => Def.BasePrice;
    public bool Sold { get; private set; }
    public void MarkSold() => Sold = true;
}

class RelicOffer : IPurchasable {
    public ARelicDef Def;
    public ItemKind Kind => ItemKind.Relic;
    public IInventoryItem Prototype => Def.ToDisplayItem();
    public int Price => Def.BasePrice;
    public bool Sold { get; private set; }
    public void MarkSold() => Sold = true;
}
```

> SS 的 `IPurchasable` 模型在增加新商品类型（如"宝石"）时不需要改 `ShopController`，只要再加一个 `xxxOffer` 实现即可。

### 2.4 `ShopRefreshService` — 随机展柜

```text
class ShopRefreshService {
    int BallOfferCount;       // 默认 5
    int RelicOfferCount;      // 默认 5
    RandomPool<BallDef> BallPool;
    RandomPool<RelicDef> RelicPool;

    List<BallOffer>  GenerateBallOffers(int count);
    List<RelicOffer> GenerateRelicOffers(int count);

    // 重新随机：先把已 Sold 的剔除，再补足 count 个
    void Reroll<T>(IList<T> offers, Func<int, List<T>> generateFn) where T : IPurchasable;
}
```

> **"已 Sold 的就置灰不要再随机出现"**，这个需求通过"剔除已售"自然实现。
> **"重新随机扣金币"**走 `CoinManager.Pay`，避免直接动数字。

### 2.5 `ShopUiBinder` — UI 绑定层（与现有 UGUIGenerator 配合）

```text
class ShopUiBinder {
    UGUIForm Form;                                // 自动生成的 UGUI Form
    List<UIShopCardItem> CardItems;               // 卡片控件，UGUIGenerator 生成

    void Render(IList<IPurchasable> offers);      // 把 offers 渲到 CardItems
    void MarkSold(int slotIndex);                 // 置灰某卡
    void OnClickRerollButton();
    void OnClickNextButton();
    void OnClickCard(int slotIndex);
}
```

---

## 三、状态机（简化时序图）

```text
[Idle]
  ▲                                    (下次战备再回来)
  │
  │   WaveManager → ShopSystem.EnterShop()
  ▼
[ShowingBallBoard]  ────玩家点「下一步」──→  [ShowingRelicBoard]
  ▲ │                                          │
  │ └─ 玩家点「重新随机」                       │ 玩家点「下一步」
  │    （扣金币，重新随机 X 个球）              ▼
  │                                       [ShowingRelicBoard]
  │                                          │ 玩家点「重新随机」
  │                                          │ （扣金币，重新随机 X 个遗物）
  │                                          ▼
  └────────────────────────────────────────────┐
                                               ▼
                                          [Done]
                                               │   WaveManager.RequestNextPhase("shopping_done")
                                               └──► 进下一波
```

---

## 四、配置：`ShopSystemConfig`

```text
[CreateAssetMenu(menuName="MyFramework/Gameplay/ShopSystemConfig")]
class ShopSystemConfig : ScriptableObject {
    [Header("Board Size")]
    public int BallOfferCount  = 5;
    public int RelicOfferCount = 5;

    [Header("Reroll Cost")]
    public int BallBoardRerollCost  = 2;
    public int RelicBoardRerollCost = 1;

    [Header("Refresh Pool")]
    public BallDef[]   BallOfferPool;     // 策划能配的"会出现哪些球"
    public RelicDef[]  RelicOfferPool;    // 策划能配的"会出现哪些遗物"
    // 也可以改成 IDRange + BallDefLibrary.Get

    [Header("Per Wave")]
    public bool OverridePoolPerWave = false;
    public List<PerWaveShopConfig> PerWave;   // 可选
}

[Serializable]
class PerWaveShopConfig {
    public int WaveIndex;
    public int BallOfferCountOverride;
    public int RelicOfferCountOverride;
    public BallDef[]  BallPoolOverride;
    public RelicDef[] RelicPoolOverride;
}
```

策划改这个 SO：
- 想加新球到球池 → 加到 `BallOfferPool`
- 想某一波只卖传奇球 → `PerWave[3]`
- 想改重新随机金币 → 改 `BallBoardRerollCost`

---

## 五、关键流程（详细时序图见末篇）

### 5.1 进入商店

```text
WaveManager.ToPhase = SHOPPING
  ├─ WaveEvents.OnPhaseChanged(SHOPPING)
  └─ ShopSystem.Subscribe.onPhaseChanged(SHOPPING)
        └─ ShopSystem.EnterShop()
              ├─ CoinManager.EnsureEnoughFor(RerollCost)  // 预检查，避免面板还没渲染就被截断
              ├─ ShopUiBinder.Show()
              ├─ offers = ShopRefreshService.GenerateBallOffers(5)
              ├─ ShopUiBinder.Render(offers)
              └─ ShopController.State = ShowingBallBoard
```

### 5.2 玩家点商品卡「购买」

```text
ShopUiBinder.OnClickCard(slotIndex):
  ShopController.OnPlayerClickOffer(offers[slotIndex])

ShopController.TryBuyOffer(offer):
  if offer.Sold:
      Toast.Show("已售出")
      return false

  if !InventorySystem.Instance.CorrespondingBag.CanAdd():
      Toast.Show($"{offer.Kind} 背包已满，请先出售")
      return false

  if !CoinManager.Instance.CanPay(offer.Price):
      Toast.Show("金币不足")
      return false

  BuyBallAction.Run(offer)   // 或 BuyRelicAction.Run(offer)

  if ok:
      offer.MarkSold()
      ShopUiBinder.MarkSold(slotIndex)   // 卡片置灰
      ShopEvents.OnOfferSold(offer)
```

### 5.3 玩家点「重新随机」

```text
ShopUiBinder.OnClickRerollButton()
  ShopController.OnPlayerClickReroll():
    var cost = (State == BallBoard) ? cfg.BallBoardRerollCost : cfg.RelicBoardRerollCost
    if !CoinManager.CanPay(cost):
        Toast.Show("金币不足以重新随机")
        return false

    CoinManager.Pay(cost, reason: "shop_reroll")
    // 重抽
    if State == BallBoard:
        offers = ShopRefreshService.GenerateBallOffers(cfg.BallOfferCount, excludeSold: true)
    else:
        offers = ShopRefreshService.GenerateRelicOffers(cfg.RelicOfferCount, excludeSold: true)
    ShopUiBinder.Render(offers)
    ShopEvents.OnRerolled(State)
```

> 已售的 offer **不会**被重新随机覆盖。代码上由 `Reroll` 时剔除已售保证。

### 5.4 玩家点「下一步」

```text
ShopUiBinder.OnClickNextButton():
  ShopController.OnPlayerClickNext()
    if State == BallBoard:
        OpenBoard(Relic)
    else if State == RelicBoard:
        FinishShopAndNotify():

ShopController.FinishShopAndNotify():
    ShopUiBinder.Hide()
    ShopController.State = Done
    ShopEvents.OnShopClosed()
    WaveManager.Instance.RequestNextPhase(reason: "shopping_done")
```

### 5.5 玩家从背包里「出售」

```text
UI:        玩家点背包里一颗球 / 遗物 →「出售」
SellService.Sell(item):
  if item is BallInstance b:
      SellBallAction.Run(b):
        holder = InventoryLocate.FindHolderOf(b)
        holder.TryRemoveByInstance(b)
        CoinManager.Earn(b.Def.BasePrice / 2, reason: "ball_sell")
        BallEvents.RaiseSold(b, refund)
        InventoryEvents.OnBallRemoved(b)

  if item is ARelic r:
      SellRelicAction.Run(r):
        RelicBag.Remove(r)
        CoinManager.Earn(r.Def.SellPrice, reason: "relic_sell")
        RelicEvents.RaiseSold(r, refund)
        InventoryEvents.OnRelicRemoved(r)

  ShopEvents.OnSoldFromBag(item)   // 通知 SS，但 SS 此时通常已 Done
```

---

## 六、事件总线：`ShopEvents`

```text
static class ShopEvents {
    public static event Action                     OnShopOpened;
    public static event Action                     OnShopClosed;
    public static event Action<ShopBoardKind>      OnBoardOpened;       // Ball / Relic
    public static event Action<ShopBoardKind>      OnBoardRerolled;
    public static event Action<IPurchasable>       OnOfferSold;         // 包括购买
    public static event Action<IInventoryItem>     OnSoldFromBag;       // 玩家从背包出售
}
```

订阅方举例：

- `UI`：订阅全部
- `Save`：订阅 `OnOfferSold / OnSoldFromBag` 写持久化
- `WaveRewardManager`：订阅 `OnShopClosed` 进入下一波前结算奖励
- `AchievementSystem`（如果有）：订阅 `OnOfferSold` 触发"买满 10 个球"成就

---

## 七、与现有项目代码的对接

| 现有文件 | 对接方式 |
| --- | --- |
| `Assets/Scripts/HotFix/_Gameplay/ARooms/Phase/8_ShoppingPhase.cs` | **改为只持有** `ShopSystem.Instance` 的引用；`onBegin` 调 `ShopSystem.EnterShop()`；玩家点「下一步」时 `ToPhase = BATTLE` 由 SS 通过 `WaveManager.RequestNextPhase` 通知，phase 不再主动监听 key |
| `Assets/Scripts/HotFix/_Gameplay/ARooms/Phase/4_PreparePhase.cs` | `wave.StartGame(...)` 触发 `OnPhaseChanged`，SS 不会进入。**PreparePhase 不动**，只是该阶段没了购物入口 |
| `Assets/Scripts/HotFix/_Gameplay/AWaves/WaveManager.cs` | 在 `ToPhase` 切换时统一发 `WaveEvents.OnPhaseChanged(Phase)`，SS 订阅即可 |
| `Assets/Scripts/HotFix/_Gameplay/ACoin/CoinManager.cs` | 不动。SS 全程走 `CoinManager.Pay / Earn(reason)`，增加 reason string：`shop_reroll` / `ball_sell` / `relic_sell` |
| `Assets/Scripts/HotFix/_Gameplay/ARelics/ARelic.cs` | 不动。`RelicOffer` 仅做包装 |
| `Assets/Scripts/HotFix/_TopDownEngine/Balls/Ball.cs` | 不动。SS 不涉及物理球 |
| `Assets/Scripts/HotFix/_Gameplay/AGameActions/Player/ShootBallsAction.cs` | 不动。SS 不在战斗阶段 |
| `Assets/Scripts/HotFix/_Gameplay/AGameActions/Player/DealClaimRewardsAction.cs` | 战斗结束奖励可补球，与 SS 解耦：`RewardService.GrantBall(def)` → 走 `InventorySystem.CanAddBall` 检查 → 通过则入包，否则入挂起奖励队列 |

---

## 八、UI 形状（参考"云顶之弈商店"）

```text
+----------------------------------------------------+
|  [金币: 42]   [下一波倒计时: --:--]    [下一步 →]   |
+----------------------------------------------------+
|  [Reroll 2g]                                       |
+----------------------------------------------------+
|  ┌───┐  ┌───┐  ┌───┐  ┌───┐  ┌───┐                 |
|  │ 球 │  │ 球 │  │ 球 │  │ 球 │  │ 球 │   <- X=5    |
|  │ Lv1│  │ Lv2│  │ Lv1│  │ Lv3│  │ Lv1│            |
|  └───┘  └───┘  └───┘  └───┘  └───┘                |
|  [已售]                                          |
+----------------------------------------------------+
|  ←  球商店        遗物商店  →                     |
|      ● 当前                                        |
+----------------------------------------------------+
```

### 关键交互

| 控件 | 触发 | 行为 |
| --- | --- | --- |
| 卡片 | 点击 | 若是球 → `TryBuyOffer(BallOffer)`；若是遗物同理。已售置灰 |
| 卡片 | 长按 | 弹出"详情"面板（`BallDef` / `RelicDef` 的描述） |
| "下一步" | 点击 | `ShopController.OnPlayerClickNext` |
| "重新随机" | 点击 | `ShopController.OnPlayerClickReroll` |
| 标签栏 | 球 / 遗物切页 | 一般在玩家主动点击下一步前不允许左右切；只读展示当前 board |
| 顶部金币 | 自动订阅 `CoinEvents.OnBalanceChanged` | 实时刷新 |

---

## 九、扩展点

| 演化 | 扩展方式 |
| --- | --- |
| 重新随机金币变 0（免费刷） | `ShopSystemConfig.BallBoardRerollCost = 0`，**不改 SS 代码** |
| 重新随机对已售商品免费 | 同上 + `Reroll` 逻辑里"已售保留"已实现 |
| 整轮固定 N 个商品 + 不允许 reroll | `BallOfferCount = N` + UI 隐藏 reroll 按钮 |
| 不同波次商店不同商品池 | `ShopSystemConfig.PerWave[waveIndex].BallPoolOverride` |
| 打折球 / 打折遗物 | 在 `BallOffer` / `RelicOffer` 加 `DiscountPercent`，UI 按比例显示价格；`TryBuyOffer` 按 `price * (100-discount)/100` 支付 |
| 锁定某个球 / 遗物（不让 reroll 换掉） | `ShopOffer` 加 `Locked` 字段，UI 加锁定按钮；Reroll 时跳过 |
| 商品带"标签"（"近战友好"） | `BallDef.Tags`，按 tag 加权 |
| 同一波 N 个商品里"至少 1 个紫球" | `ShopRefreshService` 改权重 + 配置，**不动 SS 主体** |
| 关卡中临时道具拓展 | 加 `IPurchasable` 子接口 `IPowerupPurchasable`；SS 只看 `IPurchasable` |

---

## 十、不允许 SS 直接做的事

| 不允许 | 原因 |
| --- | --- |
| 直接 `coin.balance -= N` | 必须走 `CoinManager.Pay` |
| 直接 `ballBag.Add(item)` 创建球实例 | 必须走 `InventorySystem` + 走 `BuyBallAction` 便于回放 / 重做 |
| 直接 `waveManager.ToPhase = X` | 必须走 `WaveManager.RequestNextPhase(reason)` |
| 修改已售商品 | 已被置灰逻辑兜住，强行改会被对账检查复位 |
| 把战备面板和战斗面板 UI 混用同一根 Canvas | `ShopUiBinder` 仅绑定 shop form，避免互相干扰 |

---

## 十一、单元测试切入点

- `ShopController.EnterShop`：进入时应 `ShopUiBinder.Show()` + `offers.Count == cfg.BallOfferCount`
- `ShopController.OnPlayerClickReroll`：金币不足时拒绝且扣的金币 = 0
- `ShopController.TryBuyOffer`：背包满时拒绝 + 金币不扣 + offer 不置灰
- `ShopController.OnPlayerClickNext`：从 BallBoard → RelicBoard；从 RelicBoard → Done + 触发 `WaveManager.RequestNextPhase`
- `ShopRefreshService.GenerateBallOffers(count)`：长度恰好 = count，**不重复**（除非池子不足）
- `ShopRefreshService.Reroll`：已售的不被替换，非已售的被替换
- `ShopEvents`：每个状态切换都触发对应事件
- `SellBallAction` / `SellRelicAction`：扣金币 / 移除 / 事件三件事顺序执行

---

## 十二、与本框架其它系统对应

| MyFramework 已有 | 商店系统的用法 |
| --- | --- |
| `EventSystem` (`Frame_HotFix`) | `ShopEvents` / `InventoryEvents` / `WaveEvents` 都通过它 |
| `CommandSystem` (`Frame_HotFix`) | 购买 / 出售 / 重新随机 都走 `AGameAction` |
| `ClassPool` (`Frame_HotFix`) | 卡片控件走 `UIPrefabPool` |
| `UI 自动生成`（`UGUIGenerator`） | `ShopUiBinder` 的控件由 UGUIGenerator 自动生成 |
| `配置表生成` | `BallDef / RelicDef / ShopSystemConfig` 全由策划表 + CSV 工具链生成 |
| `WaveSystem` (`AWaves`) | `WaveManager.ToPhase = SHOPPING` 触发 SS |
| `CoinSystem` (`ACoin`) | `CoinManager.Pay/Earn` |
| `InventorySystem`（本文档设计的） | `CanAddBall / CanAddRelic` / `Add` |
| `BallManagementSystem`（本文档设计的） | `BallShopService.PurchaseAndStore`，球出售 |
| `RelicSystem` (`ARelics`) | 遗物出售逻辑 |
