# MyFramework：InventorySystem 背包系统设计

> 子文档，承接总览 [球管理·商店·背包 系统拆分与协作设计](MyFramework:球管理·商店·背包%20系统拆分与协作设计.md)。

---

## 一、目标与边界

### 1.1 系统目标

背包系统（`InventorySystem`，简称 `IS`）只关心**两件事**：

1. **球背包**（`BallBag`）里现在装着哪些球、有几个空位、能不能再塞一个
2. **遗物背包**（`RelicBag`）里现在装着哪些遗物、有几个空位、能不能再塞一个

任何系统想让球/遗物"入包"或"出包"，**只能**调用本系统。

### 1.2 不在范围内

| 不在范围内 | 谁来做 |
| --- | --- |
| 球本身的升级 / 融合 / 出售 | `BallManagementSystem` |
| 遗物效果触发 | `RelicSystem`（已有的 `_Gameplay/ARelics/`） |
| 商店选品 / 售价 | `ShopSystem` |
| 格子 UI 视觉 | `UI` 层（订阅本系统事件重绘） |
| 金币扣减 | `CoinManager` |

### 1.3 与其他系统的契约

```text
interface IInventoryItem {
    int      ItemId       // 配置表 ID（球 defId 或遗物 id）
    string   DisplayName  // 多语言键
    int      SellPrice    // 半价售出时的回收价
    ItemKind Kind         // Ball / Relic
}

[Flags]
enum ItemKind { Ball = 1, Relic = 2 }

// 唯一写入入口
InventorySystem.Instance.BallBag.Add(item);
InventorySystem.Instance.RelicBag.Add(item);

// 唯一查询入口
InventorySystem.Instance.BallBag.CanAdd();         // 还有空位?
InventorySystem.Instance.BallBag.FreeSlots;        // 剩余空位数
InventorySystem.Instance.RelicBag.CanAdd(item);    // (item 占 1 格的实际判断，最简实现等于 CanAdd())
```

---

## 二、领域模型

### 2.1 `InventoryBag` — 泛型格子集合

`InventoryBag<T> where T : IInventoryItem` 是一个**带容量上限的集合**：

| 字段 | 类型 | 含义 |
| --- | --- | --- |
| `Capacity` | `int` | 总容量（策划配置，运行时可扩容） |
| `MaxCapacity` | `int` | 扩容上限（防越界） |
| `Items` | `IReadOnlyList<T>` | 当前持有物品的列表，顺序 = 玩家的视觉顺序 |

| 操作 | 行为 | 抛出 |
| --- | --- | --- |
| `Add(T item)` | 追加到末尾 | `InventoryFullException`（容量满） |
| `AddAt(int index, T item)` | 插入到指定槽位，挤掉原内容并落到末尾 | `InventoryFullException`（容量满）或 `IndexOutOfRangeException` |
| `Remove(T item)` / `RemoveAt(int index)` | 移除，**不**自动补位 | `InventoryItemNotFoundException` |
| `Swap(int a, int b)` | 两槽交换 | — |
| `CanAdd()` | 剩余容量 > 0 | — |
| `Expand(int delta)` | 扩容 | `InventoryExpansionLimitException`（超过 `MaxCapacity`） |
| `Shrink(int delta)` | 缩容（要求尾部空位够多） | `InventoryShrinkInvalidException` |

> **重要**：背包的「Add/Remove」只有这两个动词。
> 升级 / 融合这种**业务操作**，由 `BallManagementSystem` 组合 "Remove 多 + Add 一" 完成，背包不需要懂业务。

### 2.2 `BallBag` — 球背包

```text
class BallBag : InventoryBag<BallInstance> {
    const int DefaultCapacity = 9;
    InventorySystemConfig ConfigRef;
    override int Capacity => ConfigRef.BallBagCapacity;

    event OnBallAdded / OnBallRemoved / OnBallBagChanged
}
```

> `BallInstance`（由 `BallManagementSystem` 创建）会自带 `IInventoryItem` 实现。
> `BallBag` 完全不必关心"球是几级、能不能发射"。

### 2.3 `RelicBag` — 遗物背包

```text
class RelicBag : InventoryBag<ARelic> {
    const int DefaultCapacity = 15;
    InventorySystemConfig ConfigRef;
    override int Capacity => ConfigRef.RelicBagCapacity;

    event OnRelicAdded / OnRelicRemoved / OnRelicBagChanged
}
```

> 已有的 `Assets/Scripts/HotFix/_Gameplay/ARelics/ARelic.cs` 加上 `IInventoryItem` 实现就能直接落到这个袋里。

### 2.4 两种背包**为什么分开**？

- 球背包的逻辑复杂：槽位互通、升级、融合、半价回收
- 遗物背包的逻辑简单：买来就生效、不能合并、半价回收
- UI 上两套格子**长得完全不一样**（球卡有等级图标 / 遗物有描述），不能共用一个容器

**但**接口上都继承 `InventoryBag<T>`，通用逻辑（容量、事件、查询）只写一遍。

---

## 三、`InventorySystem` 单例

```text
[FrameSystemHotFix]  // 跟随 MyFramework 的 FrameSystem 注册方式
class InventorySystem : FrameSystem {
    public static InventorySystem Instance;

    public BallBag  BallBag;     // 默认容量 9
    public RelicBag RelicBag;    // 默认容量 15

    // 给其他系统的便利方法（也可以不写，访问 BallBag / RelicBag 即可）
    public bool CanAddBall()  => BallBag.CanAdd();
    public bool CanAddRelic() => RelicBag.CanAdd();

    // 扩容接口（必须由升级道具 / 关卡奖励等明确原因触发，不允许 GameAction 自由调）
    public void ExpandBallBag(int delta)  => BallBag.Expand(delta);
    public void ExpandRelicBag(int delta) => RelicBag.Expand(delta);
}
```

> `Instance` 模式与项目里 `CoinManager.Instance`、`WaveManager.Instance` 风格一致，方便业务侧快速调用。

---

## 四、容量配置：`InventorySystemConfig`

```text
[CreateAssetMenu(menuName="MyFramework/Gameplay/InventorySystemConfig")]
class InventorySystemConfig : ScriptableObject {
    [Header("Bag Capacity")]
    public int BallBagCapacity  = 9;
    public int RelicBagCapacity = 15;

    [Header("Expansion")]
    public int MaxBallBagCapacity  = 30;
    public int MaxRelicBagCapacity = 40;
    public bool AllowExpansion = true;   // 总开关，关掉后任何 Expand 都拒绝
}
```

策划改这两个数即可。运行时扩容通过道具 / 关卡奖励 / 升级系统传入 `Expand(delta)`。

---

## 五、异常类型

```text
class InventoryFullException : Exception {
    public ItemKind BagKind;
    public InventoryFullException(ItemKind k) : base($"{k} bag is full") {}
}
class InventoryItemNotFoundException : Exception {}
class InventoryExpansionLimitException : Exception {}
class InventoryShrinkInvalidException : Exception {}
```

> 商店流程 `CanAdd` 返回 false 时**优先提示**玩家，不抛异常；只有"绕过 CanAdd 强行 Add"才走异常路径。异常是给程序员做调用错误的兜底，不是给玩家看的信息。

---

## 六、事件总线：`InventoryEvents`

```text
static class InventoryEvents {
    public static event Action<BallInstance>   OnBallAdded;
    public static event Action<BallInstance>   OnBallRemoved;
    public static event Action                OnBallBagChanged;   // 任一变更后 bulk 通知

    public static event Action<ARelic>         OnRelicAdded;
    public static event Action<ARelic>         OnRelicRemoved;
    public static event Action                OnRelicBagChanged;
}
```

订阅方举例：

- `UIBagGrid`：订阅 `OnBallBagChanged` / `OnRelicBagChanged` 重绘
- `ShopSystem`：订阅 `OnBallAdded` 来给"我已拥有同类球"打勾
- `Save`：订阅两类 `OnBagChanged` 写存档
- `AGameActions/Shop/SellBallAction`：订阅 `OnBallRemoved` 进 CommandSystem

---

## 七、与其它系统的协作

### 7.1 BallManagementSystem 写入背包（升级 / 融合 / 出售）

```text
class BallManagementSystem {

    BallInstance Upgrade(...) {
        // 1. 校验
        // 2. 从容器移除 X 个 (Skill: 通过 IInventoryHolder 接口拿)
        //    因为升级可能在槽位上也可能在背包里，所以需要 holder 抽象
        // 3. 创建新球
        // 4. 写回 holder
    }
}
```

为了让 BMS 不直接 new 出 `BallBag / BallSlotGroup`，引入 `IInventoryHolder`：

```text
interface IInventoryHolder {
    bool TryRemoveByInstance(IInventoryItem item);   // 找到并移除
    bool TryInsert(IInventoryItem item);             // 加进来
    int  FindIndex(IInventoryItem item);             // -1 表示不在
    string Name { get; }                              // "BallBag" / "Slot#2"
}
```

`BallBag`、`RelicBag`、`BallSlotGroup` 全部实现 `IInventoryHolder`。

> 这样 BMS 只面对 `IInventoryHolder`，不直接感知"是哪个容器"，升级 / 融合的语义就统一了。

### 7.2 ShopSystem 写入背包（购买）

```text
ShopController.TryBuyOffer(IPurchasable offer):
    1. if not InventorySystem.Instance.CorrespondingBag.CanAdd():
           Toast.Show($"{offer.Kind} 背包已满，请先出售腾出格子");
           return false;
    2. if not CoinManager.Instance.CanPay(offer.Price):
           Toast.Show("金币不足");
           return false;
    3. BuyBallAction.Run(offer);   // 也可写 BuyRelicAction
           ├ CoinManager.Pay(offer.Price, reason)
           ├ InventorySystem.BallBag.Add(item)        // 球 → BallBag
           └ raise ShopEvents.OnPurchased(...)
    4. offer.MarkSold()  // 置灰
```

### 7.3 售出回收

```text
SellController.Sell(IInventoryItem item):
    if item is BallInstance b: SellBallAction.Run(b)
    if item is ARelic       r: SellRelicAction.Run(r)
```

实现（举球为例）：

```text
class SellBallAction : AGameAction {
    BallInstance target;
    int refund;
    public override void execute() {
        target.OnBeforeSold?.Invoke();
        // 找到 holder 并移除
        var holder = InventoryLocate.FindHolderOf(target);  // 槽位或背包
        holder.TryRemoveByInstance(target);
        refund = target.Def.BasePrice / 2;       // 半价
        CoinManager.Instance.Earn(refund, reason: "ball_sell");
        BallEvents.RaiseSold(target, refund);
    }
}
```

---

## 八、与现有项目代码的对接

| 现有文件 | 对接方式 |
| --- | --- |
| `Assets/Scripts/HotFix/_Inventory/Core/Inventory.cs`（MoreMountains 自带） | 项目原本用 MM 的 Inventory 做"道具通用容器"。**本系统不是它的替代**，而是"球 + 遗物专用"的领域容器，但**实现可参考它的 API**（`Add/Remove/CanAdd`）。如果不需要通用道具格，本系统即可独立运行 |
| `Assets/Scripts/HotFix/_Gameplay/ARelics/ARelic.cs` | 加 `IInventoryItem` 接口实现，落到 `RelicBag` |
| `Assets/Scripts/HotFix/_Gameplay/Ball/BallManager.cs` | 不动；`BallInstance` 是新的、抽象层 |
| `Assets/Scripts/HotFix/_Gameplay/AGameActions/Player/ReturnBallsAction.cs` | 这是"把发射出去的球收回来"的动作，不在背包系统。回收的球入 `BallBag` 时调用 `InventorySystem.BallBag.Add` 即可 |

---

## 九、典型流程（按需查阅）

### 9.1 玩家背包满了仍想从商店购买

```text
ShopSystem: 玩家点商品卡
ShopSystem: ShopController.TryBuyOffer(offer)
ShopSystem:   canAdd = InventorySystem.CanAddBall() ⇒ false
ShopSystem:   Toast.Show("球背包已满，请先出售一些球腾出空间")
ShopSystem:   弹出售面板快捷入口（可选）
```

### 9.2 玩家从背包里出售一颗球

```text
UI:       玩家点球 → 「出售」
Command:  SellBallAction.Run(ball)
Action:   holder.TryRemoveByInstance(ball)
          ├ if holder is BallBag       → BallBag.Remove
          ├ if holder is BallSlotGroup → slot.Clear()
Action:   CoinManager.Earn(refund, "ball_sell")
Action:   BallEvents.OnBallSold(ball, refund)
UI:       重绘
```

### 9.3 玩家背包得到升级产物

```text
BMS.Upgrade:
  1) holder.Remove(material1)
  2) holder.Remove(material2)         ← 在 Bag 或 Slot 上
  3) var upgraded = BallInstance.Create(def, level+1)
  4) holder.TryInsert(upgraded)        ← 插回原代表球的位置
  5) OnBallAdded event                ← 走 Bag/Slot 事件
UI 重绘
```

---

## 十、扩展点

| 演化 | 扩展方式 |
| --- | --- |
| 背包格子数量可变 | `InventoryBag.Expand(delta)` / `Shrink(delta)` |
| 球的"叠加"展示（同类同等级 5 颗 → 显示 1 个 "x5"） | 在 `BallBag` 上加 `GetDisplayItem(int)`，按等级聚合；BMS 仍按"1 个 instance = 1 个槽位"放，UI 层做聚合 |
| 同种不同级球的合并（升级）后保留位置 | BMS 升级路径已保留 |
| 遗物可以「使用即销毁」 | 已有 `ARelic`，在 `RelicBag.Remove(...)` 后，调用方自己决定 |
| 格子按权重排序 | 加 `InventoryBag.Sort(IComparer<T>)` |
| 跨包搬运 | 已有 `IInventoryHolder.TryRemove + TryInsert`，跨包本质是「两个 ItemId 的 Remove + Insert」 |
| 存档 | 订阅 `OnBagChanged`，序列化 `Items + Capacity` |

---

## 十一、与本框架其它系统对应

| MyFramework 已有 | 背包系统的用法 |
| --- | --- |
| `EventSystem` (`Frame_HotFix`) | `InventoryEvents` |
| `CommandSystem` (`Frame_HotFix`) | `SellBallAction / SellRelicAction` 都进栈，便于回放 / 复盘 |
| `ClassPool` (`Frame_HotFix`) | `BallInstance` 出入包走对象池，UI 卡片也走 PrefabPool |
| `UI 自动生成` | 球背包格子 UI、遗物背包格子 UI 用 UGUIGenerator 自动生成 |
| `配置表生成` | `InventorySystemConfig` 是常量级 SO；球 / 遗物的具体配置由 CSV 工具链生成 |
| `BallManagementSystem` | 升级 / 融合 / 出售时操作背包 |
| `ShopSystem` | 购物和出售时操作背包 |

---

## 十二、单元测试切入点

- `InventoryBag.Expand/Shrink` 边界（含 `MaxCapacity` 拒绝）
- `InventoryBag.Add` 容量满抛 `InventoryFullException`
- `InventoryBag.Remove` 不存在抛 `InventoryItemNotFoundException`
- `InventorySystem.CanAddBall / CanAddRelic` 真实反映两袋容量
- `IInventoryHolder` 协作：BMS 升级时只面对 holder，不直接引用 `BallBag / BallSlotGroup`
- `SellBallAction`：扣金币、移除球、抛事件三个动作要在 execute() 中按顺序完成
