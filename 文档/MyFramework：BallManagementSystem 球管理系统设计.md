# MyFramework：BallManagementSystem 球管理系统设计

> 子文档，承接总览 [球管理·商店·背包 系统拆分与协作设计](MyFramework:球管理·商店·背包%20系统拆分与协作设计.md)。

---

## 一、目标与边界

### 1.1 系统目标

球管理系统（`BallManagementSystem`，简称 `BMS`）只关心**球是什么、球怎么改、球怎么被外部消费**。

| 在范围内 | 不在范围内 |
| --- | --- |
| 球的静态定义（种类 / 等级上限 / 价格 / 升级配方 / 融合配方） | 球背包的格子数（归 `InventorySystem`） |
| 球的运行时实例（升级、融合、出售后的形态） | 金币增减（归 `CoinSystem`） |
| **发射槽位**的占用与释放 | UI 怎么画、按钮文案 |
| 槽位 ↔ 球背包之间的**移动** | 战斗里球的物理轨迹 |
| 升级 X 合 1 的事务逻辑 | 波次阶段切换 |

### 1.2 与其他系统的交互

```text
InventorySystem    <── 球背包 = BallBag，球实例通过 IInventoryItem 进出
ShopSystem         <── 通过 IPurchasable 买/卖球
WaveSystem         <── 间接：ShopSystem 在战备阶段调用 BMS
CoinSystem         <── 只在「售出」「融合扣费」时调用，不直接改金币数
GameActions        <── 升级/融合/购买/出售都封装成 AGameAction
```

---

## 二、领域模型

### 2.1 `BallDef` — 静态定义

策划面 / 配置表驱动。建议作为 `ScriptableObject` 或由项目 CSV 配置表生成器生成。

| 字段 | 类型 | 含义 |
| --- | --- | --- |
| `BallDefId` | `int` | 唯一 id，配置表主键 |
| `DisplayName` | `string` | 多语言键 |
| `Kind` | `enum BallKind` | Fire / Ice / Thunder / ... |
| `BasePrice` | `int` | 售价（一手购买价） |
| `MaxLevel` | `int` | 上限等级，暂定 3 |
| `UpgradeRecipe` | `MergeRecipe` | 升级配方：多少个 + 多少金币（默认 0） |
| `MergeRecipe` | `MergeRecipe?` | 融合后的目标球 def id + 必需金币 |
| `Sprite` / `Prefab` | 资源引用 | 显示与物理表现 |
| `UnderlyingBallPrefab` | `Ball` | 引用现有 `_TopDownEngine/Balls/Ball.cs`，仅用于关卡里实例化物理球 |

```text
struct MergeRecipe {
    int CombineCount    // 默认 2，可由策划改成 3
    int GoldCost        // 升级时扣的金币（球升级一般不扣，默认 0）
    BallDefId? ResultDefId   // 升级后还是同种球，故同 def，字段语义主要用于融合
}
```

> **关键**：升级产物仍然是"同种 + Level+1"，所以升级不需要 `ResultDefId`；合并/融合才需要。`MergeRecipe` 用一个结构表达两种语义更省事。

### 2.2 `BallInstance` — 运行时实例

| 字段 | 类型 | 含义 |
| --- | --- | --- |
| `DefId` | `int` | 反查 `BallDef` |
| `Level` | `int` | 1..`MaxLevel` |
| `Uid` | `Guid` | 实例唯一 id（升级前后 Uid 变，跟统计、Buff 等回放有关） |
| `Owner` | `enum` | `Slot` / `BallBag` / `PendingSale` |
| `UnderlyingBallRef` | `Ball` *(弱引用 / 对象池 id)* | 已发射球的物理实体；未发射时为 null |

实现 `IInventoryItem`：

```text
int      ItemId      => DefId
string   DisplayName => Def.DisplayName
int      SellPrice   => Def.BasePrice / 2   // 半价回收
ItemKind Kind        => ItemKind.Ball
```

### 2.3 `BallSlot` 与 `BallSlotGroup` — 发射槽位

```text
class BallSlot {
    int   Index              // 0..SlotCount-1
    BallInstance Current     // null 表示空槽位
    void Set(BallInstance b) // 装填
    void Clear()             // 卸下
    bool IsEmpty => Current == null
}

class BallSlotGroup {
    IReadOnlyList<BallSlot> Slots
    int Capacity             // 默认 3，由 BallSystemConfig.SlotCount 配置
    int Expand(int delta)    // 运行时扩容接口，留出扩展槽位
    bool TryPlaceAt(int slotIndex, BallInstance b)
    BallInstance PullFrom(int slotIndex)
    void MoveTo(int src, int dst)  // 槽位之间交换
}
```

> **设计原则**：槽位集合只管"球在不在这里、能挪到哪里"，不关心具体等级。挪动即发起事件，由 UI 重画。

### 2.4 `BallInventoryAdapter` — 槽位 ↔ 球背包搬运

把"槽位"和"球背包"在用户视角当成**两个互通的容器**，但实际操作都委托给各自容器：

```text
class BallInventoryAdapter {
    BallSlotGroup   Slots
    BallBag         Bag      // 由 InventorySystem 注入

    void EquipFromBag(BagIndex idx, SlotIndex dst)   // 把背包里的球装到槽位
    void UnequipToBag(SlotIndex src, BagIndex idx)   // 卸到背包
    void Swap(SlotIndex a, SlotIndex b)
    void SwapSlotAndBag(SlotIndex s, BagIndex b)
}
```

> 槽位和背包**不能同时持有同一颗球**。`Adapter` 在搬运过程中加锁，避免双持。

---

## 三、子服务（核心逻辑入口）

### 3.1 `BallUpgradeService` — 升级

**输入**：`List<BallInstance> sameKindSameLevel`
**前置条件**：

1. 个数 ≥ `BallSystemConfig.UpgradeCombineCount`（默认 2）
2. 每个球都是同种、同等级
3. 升级后等级 ≤ `BallDef.MaxLevel`

**执行**：

1. 校验前置条件，失败抛 `BallUpgradeInvalidException`
2. 从 `BallSlotGroup` / `BallBag` 里移除 `count` 个球
3. 创建 1 个 `BallInstance`，`Level = oldLevel + 1`
4. 放回**其中一个**被移出的位置（位置策略：放入 `count - 1` 的其他位置，保持背包干净；多余的空位清空）
5. 发 `BallEvents.OnBallUpgraded(...)`

```text
// 伪代码
class BallUpgradeService {
    BallInstance Upgrade(BallInstance representative, IEnumerable<BallInstance> materials) {
        var def = representative.Def;
        var newLevel = representative.Level + 1;
        if (newLevel > def.MaxLevel) throw BallUpgradeInvalidException("max_level");
        // 升级可扣金币（默认 0）
        if (def.UpgradeRecipe.GoldCost > 0)
            CoinManager.Instance.Pay(def.UpgradeRecipe.GoldCost, "ball_upgrade");
        // 销毁材料
        foreach (var m in materials) DestroyOrPool(m);
        // 创建新球
        var upgraded = BallInstance.CreateNew(def, newLevel);
        // 放回最具代表性的槽位（通常是代表球所在位置）
        ReplaceInContainer(representative, upgraded);
        BallEvents.RaiseUpgraded(representative, upgraded);
        return upgraded;
    }
}
```

> **X=2 时**「2 个 1 级火焰球 → 1 个 2 级火焰球」「2 个 2 级火焰球 → 1 个 3 级火焰球」都自然满足。
> **X=3 时**只把 `UpgradeCombineCount=3` 改了即可，不需要改代码。

### 3.2 `BallMergeService` — 融合

**输入**：`BallInstance a, BallInstance b, int extraGold`
**前置条件**：

1. `a.DefId != b.DefId`（不同种类）
2. `a.Level == a.Def.MaxLevel && b.Level == b.Def.MaxLevel`（都满级）
3. `CoinManager.Balance >= def.MergeRecipe.GoldCost`

**执行**：

1. 校验前置条件，失败抛 `BallMergeInvalidException`
2. `CoinManager.Pay(mergeGoldCost, "ball_merge")`
3. 销毁 `a`、`b`
4. 创建 1 个 `BallInstance`：种=融合配方 `ResultDef`，等级=1
5. 放入 `a` 所在位置（`b` 所在位置清空）
6. 发 `BallEvents.OnBallMerged(a, b, result)`

```text
class BallMergeService {
    BallInstance Merge(BallInstance a, BallInstance b) {
        if (a.DefId == b.DefId) throw BallMergeInvalidException("same_kind");
        if (a.Level != a.Def.MaxLevel || b.Level != b.Def.MaxLevel)
            throw BallMergeInvalidException("not_max_level");
        var target = BallDefLibrary.Instance.Get(a.Def.MergeRecipe.ResultDefId);
        // 收金币
        CoinManager.Instance.Pay(a.Def.MergeRecipe.GoldCost, "ball_merge");
        // 拆 a,b
        var holder = FindHolder(a);  // SlotGroup or Bag
        holder.Remove(a);
        FindHolder(b).Remove(b);
        // 新球
        var merged = BallInstance.CreateNew(target, level: 1);
        holder.TryInsertAt(holder.LastRemovedIndex, merged);
        BallEvents.RaiseMerged(a, b, merged);
        return merged;
    }
}
```

### 3.3 `BallShopService` — 买球 / 卖球

**买球**（商店流程）由 `ShopSystem` 触发，本服务只提供**最小入口**：

```text
class BallShopService {
    // 商店层会先调用 InventorySystem.CanAdd + CoinSystem.Pay
    // 这里只负责创建实例并放入背包
    BallInstance PurchaseAndStore(BallDef def, InventorySystem inv) {
        if (!inv.BallBag.CanAdd()) throw InventoryFullException("ball_bag_full");
        var ball = BallInstance.CreateNew(def, level: 1);
        inv.BallBag.Add(ball);
        BallEvents.RaisePurchased(ball, source: "shop");
        return ball;
    }

    // 售出，半价回收
    int SellToShop(BallInstance ball, InventorySystem inv, CoinSystem coin) {
        inv.BallBag.Remove(ball);   // 或 BallSlotGroup.Remove(ball) 如果球在槽位上
        var refund = ball.Def.BasePrice / 2;
        coin.Earn(refund, "ball_sell");
        BallEvents.RaiseSold(ball, refund);
        return refund;
    }
}
```

---

## 四、状态机：球的一生

```text
[创建]─→ 进入 BallBag
       └→ 进入 BallSlot（玩家「装备」时）

在槽位上 ──玩家「卸下」──→ BallBag
在 BallBag ──玩家「装备」──→ BallSlot

任意位置 ──玩家「升级」+X 颗──→ 1 颗 Level+1（位置=代表球所在）
任意位置 ──玩家「融合」+金币──→ 1 颗不同种类 Lv1
任意位置 ──玩家「出售」──→ [销毁] + 金币回收
```

> **销毁**一律走 `BallObjectPool`（项目已有），避免 GC。

---

## 五、配置：`BallSystemConfig`

```text
[CreateAssetMenu(menuName="MyFramework/Gameplay/BallSystemConfig")]
class BallSystemConfig : ScriptableObject {
    [Header("Slot")]
    public int SlotCount = 3;                  // 槽位数，可运行时扩容
    public int MaxSlotCount = 8;               // 扩容上限（防越界）

    [Header("Level & Upgrade")]
    public int DefaultMaxLevel = 3;            // 球最大等级
    public int UpgradeCombineCount = 2;        // 升级 X 合 1
    public int UpgradeGoldCost = 0;            // 升级是否扣金币，默认不扣

    [Header("Price")]
    public int SellRefundRate = 50;            // 出售时回收比例，默认 50%

    [Header("Reference to Inventory")]
    public InventorySystemConfig InventoryConfigRef;  // 引用过去，避免重复配置
}
```

策划改这个 SO 即可影响 BMS 全局行为。

---

## 六、事件总线：`BallEvents`

```text
static class BallEvents {
    public static event Action<BallInstance>                    OnBallCreated;
    public static event Action<BallInstance, int /*slotIdx*/>   OnBallEquipped;
    public static event Action<BallInstance, int /*slotIdx*/>   OnBallUnequipped;
    public static event Action<BallInstance, BallInstance>      OnBallUpgraded;     // (from, to)
    public static event Action<BallInstance, BallInstance, BallInstance> OnBallMerged;  // (a, b, merged)
    public static event Action<BallInstance>                    OnBallPurchased;
    public static event Action<BallInstance, int /*goldRefund*/>OnBallSold;
}
```

订阅方举例：

- `ShopSystem` 订阅 `OnBallPurchased / OnBallSold` 来刷新「已拥有」标记
- `WaveRewardManager`（已有）订阅 `OnBallCreated` 把赠送的球推进奖励面板
- UI（`UIBagGrid`）订阅全部，按变化重绘
- `AGameActions/Shop/BuyBallAction` 订阅全部，进 `CommandSystem` 栈用于回放/调试

---

## 七、与现有项目代码的对接

| 现有文件 | 对接方式 |
| --- | --- |
| `Assets/Scripts/HotFix/_TopDownEngine/Balls/Ball.cs` | **保持现状**，作为「发射出的物理球」。`BallInstance.UnderlyingBallRef` 在玩家发射时通过 `BallObjectPool` 取一个物理 Ball，结束后归还 |
| `Assets/Scripts/HotFix/_Gameplay/Ball/BallManager.cs` | 现有 `BallManager` 是「关卡里物理球的总数管理器」，**保留不动**。新 `BallManagementSystem` 是「持有 / 配置 / 升级 / 融合」的元数据层，**互不引用**：物理层在战斗阶段被驱动，元数据层在商店/背包里被驱动 |
| `Assets/Scripts/HotFix/_Gameplay/AGameActions/Player/ShootBallsAction.cs` | 玩家发射时，本系统的 `BallSlotGroup` 取当前 `BallInstance`，把它交给 `ShootBallsAction`（该 action 已经存在），物理 / 伤害逻辑走它 |

> **绝不**让 `BallManagementSystem` 直接引用 `Ball.cs`。这就是我们留 `UnderlyingBallRef` 间接引用的原因。

---

## 八、典型行为路径（按需查阅）

### 8.1 玩家背包里点「升级」

```text
UI:        玩家点背包里 1 颗 Lv1 火球「升级」
UI:        弹出「选择材料球」多选面板（同类同等级）
UI:        玩家选满 1 颗 Lv1 火球
Command:   BallUpgradeAction.Run(代表球, 材料球列表)
BMS:       BallUpgradeService.Upgrade(...)
BMS:       BallInventoryAdapter / BallSlotGroup / BallBag 自动调整
BMS:       BallEvents.OnBallUpgraded(...)
UI:        收到事件，重绘
```

### 8.2 玩家点「融合」

```text
UI:        玩家点背包里两颗满级不同球的「融合」按钮
UI:        提示「融合需要 100 金币，是否继续？」
玩家点确定
Command:   BallMergeAction.Run(a, b)
BMS:       BallMergeService.Merge(...)
BMS:       CoinManager.Pay(100, "ball_merge")
BMS:       BallEvents.OnBallMerged(...)
UI:        重绘
```

### 8.3 玩家从商店「买球」

```text
ShopSystem: 玩家点击商品卡
ShopSystem: ShopController.TryBuyOffer(offer)
ShopSystem:   ask InventorySystem.CanAdd(BallBag, def) => true
ShopSystem:   ask CoinManager.CanPay(price) => true
Command:     BuyBallAction.Run(offer, inv, coin)
BMS:         BallShopService.PurchaseAndStore(def, inv)
BMS:         BallEvents.OnBallPurchased(...)
ShopUI:      收到事件，置灰商品卡
```

### 8.4 玩家从背包里「出售球」

```text
UI:      玩家在背包里点球 → 「出售」
Command: SellBallAction.Run(ball)
BMS:     BallShopService.SellToShop(ball, inv, coin)
         ├ InventorySystem.BallBag.Remove(ball)
         ├ CoinManager.Earn(refund, "ball_sell")
         └ BallEvents.OnBallSold(ball, refund)
UI:      重绘
```

---

## 九、扩展点 / 防后期改代码

| 演化 | 扩展方式 |
| --- | --- |
| 槽位从 3 → 5 | `BallSlotGroup.Expand(2)`，UI 重绘 |
| 升级 X=3 | 改 `BallSystemConfig.UpgradeCombineCount = 3` |
| 升级要扣金币 | 改 `BallSystemConfig.UpgradeGoldCost`（或 per-def `MergeRecipe.GoldCost`） |
| 球可以附魔 | 在 `BallInstance` 上加 `IEnchantment[]`，不影响 BMS 接口 |
| 球的"特性"分职业 | `BallDef.Tags`，过滤时按 tag |
| 出售 1/3 价格 | 改 `BallSystemConfig.SellRefundRate = 33` |
| 球融合后不是固定种类 | per-def `MergeRecipe.ResultDefId`，策划填表 |
| 球的"传说 / 史诗"稀有度 | `BallDef.Rarity`，商店按稀有度加权 |

---

## 十、单元测试切入点

- `BallUpgradeService`：X=2 时各种等级组合
- `BallMergeService`：同种不行、未满级不行、金币不够不行
- `BallSlotGroup.Expand`：扩容后旧槽位的球仍在
- `BallInventoryAdapter`：防"槽位和背包同时持同一颗球"的双持情况
- `BallShopService.PurchaseAndStore`：背包满时抛 `InventoryFullException`
- `BallShopService.SellToShop`：回收金额 = `BasePrice / 2`，向下取整

---

## 十一、与本框架其它系统的对应关系

| MyFramework 已有 | 球管理的用法 |
| --- | --- |
| `EventSystem` (`Frame_HotFix`) | 用作 `BallEvents` 的总线 |
| `CommandSystem` (`Frame_HotFix`) | 升级 / 融合 / 购买 / 出售走 `Command`，支持回放 |
| `ClassPool` (`Frame_HotFix`) | `BallInstance` 进出池，`Ball` 物理体走 PrefabPool |
| `UI 自动生成` | 槽位 UI、球卡面板等都可用现有 UGUIGenerator |
| `配置表生成` | `BallDef` 由项目已有的 CSV→代码 工具链生成 |
| `ShopSystem`（本文档设计的） | 商店是消费者的对接方 |
| `InventorySystem`（本文档设计的） | 球背包的承载方 |
