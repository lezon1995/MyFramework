# MyFramework：球管理 · 商店 · 背包 系统拆分与协作设计

> 这是一份**总览文档**。它只负责说明：
>
> - 三个系统各自的边界与职责
> - 它们如何**互相调用**而不是互相耦合
> - 整个「球 / 背包 / 商店」玩法在 `WaveSystem` 中的嵌入位置
>
> 每个系统的**详细设计、字段、流程图、配置点**放在各自的子文档中。

---

## 一、需求回顾

需求拆开看其实是三件事，但它们互相牵扯：

| 需求 | 直接相关的子系统 | 间接相关的子系统 |
| --- | --- | --- |
| 1. 球：槽位 / 背包 / 等级 / 升级 / 融合 / 价格 | **球管理系统** | **背包系统**（球背包）· **金币系统** |
| 2. 商店：战后随机 X 个球、随机 X 个遗物，可重新随机，下一步切阶段 | **商店系统** | **背包系统**（满则拦截）· **金币系统**（付款）· **遗物系统**（购买遗物）· **波次系统**（驱动阶段） |
| 3. 背包：球背包 + 遗物背包，容量上限，购买占格、出售腾格 | **背包系统** | **球管理系统**· **商店系统**· **遗物系统** |

可以看到三个需求里，**背包系统**是真正的"枢纽"：球背包归背包管，遗物背包归背包管，商店买完直接落到背包，满了必须由背包拒绝。

因此整个拆分里必须贯彻一个原则：

> **谁拥有数据，谁做权威判断；别的系统只能问 + 通知，不允许直接改对方的私有容器。**

---

## 二、系统拆分结果

三个**业务系统** + 两个**支撑系统**（项目里已经存在，不需要重做）。

### 2.1 球管理系统 — `BallManagementSystem`

| 项 | 内容 |
| --- | --- |
| 缩写 | `BMS` |
| 职责 | 球的**定义**（种类 / 等级 / 价格 / 升级配方 / 融合配方）、球的**持有**（槽位 + 球背包映射）、球的操作（升级 / 融合 / 出售） |
| 不负责 | UI 怎么画、格子怎么显、金币怎么扣、阶段怎么走 |
| 入口目录 | `Assets/Scripts/HotFix/_Gameplay/ABall/` |
| 关键类型 | `BallDef`、`BallInstance`、`BallSlotGroup`、`BallInventoryAdapter`、`BallUpgradeService`、`BallMergeService`、`BallShopService` |
| 入口单例 | `BallManagementSystem.Instance` |
| 子文档 | [BallManagementSystem 球管理系统设计.md](MyFramework:BallManagementSystem%20球管理系统设计.md) |

### 2.2 背包系统 — `InventorySystem`

| 项 | 内容 |
| --- | --- |
| 缩写 | `IS` |
| 职责 | **两套独立但同构**的格数容器：`BallBag`（球背包，9 格） + `RelicBag`（遗物背包，15 格）。负责「加东西 / 移除东西 / 剩余容量 / 是否能塞下」 |
| 不负责 | 球 / 遗物的业务逻辑，只关心它们是不是一个 `IInventoryItem` |
| 入口目录 | `Assets/Scripts/HotFix/_Gameplay/AInventory/` |
| 关键类型 | `IInventoryItem`、`InventoryBag<T>`、`BallBag`、`RelicBag`、`InventoryFullException`、`InventoryEvents` |
| 入口单例 | `InventorySystem.Instance` |
| 子文档 | [InventorySystem 背包系统设计.md](MyFramework:InventorySystem%20背包系统设计.md) |

### 2.3 商店系统 — `ShopSystem`

| 项 | 内容 |
| --- | --- |
| 缩写 | `SS` |
| 职责 | **战备阶段**展示：随机 X 个球商品 → 玩家购买/出售/重新随机 → 点下一步 → 随机 X 个遗物商品 → 玩家购买/出售/重新随机 → 点下一步 → 通知 WaveSystem 进入下一波 |
| 不负责 | 球 / 遗物的业务逻辑（它只调对方接口），也不负责背包容量本身（它问背包） |
| 入口目录 | `Assets/Scripts/HotFix/_Gameplay/AShop/` |
| 关键类型 | `ShopController`、`ShopBoardKind { BallBoard, RelicBoard }`、`ShopOffer`、`ShopRefreshService`、`ShopUiBinder` |
| 入口单例 | `ShopSystem.Instance` |
| 子文档 | [ShopSystem 商店系统设计.md](MyFramework:ShopSystem%20商店系统设计.md) |

### 2.4 支撑系统（已存在，仅约定调用方式）

| 系统 | 入口 | 约定调用方式 |
| --- | --- | --- |
| **金币** `CoinSystem` | `Assets/Scripts/HotFix/_Gameplay/ACoin/CoinManager.cs` | **唯一**金币出入口：`CoinManager.Instance.Pay(amount, reason)` / `Earn(amount, reason)`。其它系统禁止直接改金币数字。`reason` 用于审计与回放 |
| **波次** `WaveSystem` | `Assets/Scripts/HotFix/_Gameplay/AWaves/WaveManager.cs` | 通过 `WavePhase` 阶段事件驱动 `ShopSystem` 启动 / 结束，不允许 `ShopSystem` 反向操作波次内部状态 |
| **遗物** `RelicSystem` | `Assets/Scripts/HotFix/_Gameplay/ARelics/ARelic.cs` | 遗物走 `IInventoryItem` 抽象，落到 `InventorySystem.RelicBag`。商店买遗物只调此接口 |
| **动作/命令** `GameAction` | `Assets/Scripts/HotFix/_Gameplay/AGameActions/` | 跨系统的复杂操作（球升级、球融合、购买、出售）**全部封装为 `AGameAction`**，可被 Command/CommandSystem 重放 |

---

## 三、模块依赖图

```
                    +---------------------+
                    |     WaveSystem      |
                    | (战备/商店/战斗阶段) |
                    +----------+----------+
                               | 阶段事件
                               v
                    +---------------------+
                    |     ShopSystem      |
                    +----+-----+-----+----+
                         |     |     |
        +----------------+     |     +-----------------+
        | 问能否购买     |     |       | 问价格/创建购买 |
        v                      v     v                   v
+--------------+   +-------------------+   +----------------------+
| InventorySys |<--+     CoinSystem    |   |   BallManagementSys  |
| BallBag/     |   | (Pay / Earn)      |   | BallDef / 升级 / 融合|
| RelicBag     |   +-------------------+   +----------------------+
+------+-------+                                  |
       |                                          |
       |                                          | 槽位 ↔ 背包 互通
       +<-----------------------------------------+
                      球背包就是 BallBag
                       遗物背包就是 RelicBag
```

依赖方向（不允许反向）：

```text
WaveSystem  ──▶ ShopSystem  ──▶ BallManagementSystem
                          └─▶  InventorySystem  ──▶  CoinManager
```

---

## 四、跨系统契约（接口而非实现）

让三个系统不互相耦合的关键，是**只通过接口和事件说话**。

### 4.1 `IInventoryItem`（背包契约）

```text
interface IInventoryItem
{
    int      ItemId        // 配置表 ID
    string   DisplayName   // 名字（多语言键）
    int      SellPrice     // 半价售出时的回收价（配置生成）
    ItemKind Kind          // Ball / Relic
}
```

`BallInstance`、`ARelic` 都实现 `IInventoryItem`。`InventoryBag<T>` 完全不知道里面是球还是遗物。

### 4.2 `IPurchasable`（商店契约）

```text
interface IPurchasable
{
    IInventoryItem Prototype       // 模板
    int            Price           // 售价（金币）
    bool           Sold            // 已卖出则置灰
    void OnPurchased(IInventoryItem item, InventorySystem inv, CoinSystem coin)
}
```

`BallOffer`、`RelicOffer` 实现 `IPurchasable`。`ShopController` 只看到 `IPurchasable`，不知道是球还是遗物。

### 4.3 阶段事件（WaveSystem → ShopSystem）

```text
enum ShopPhaseEvent { EnterBallShop, EnterRelicShop, ExitShop }

event ShopSystem.OnShopPhaseChanged(ShopPhaseEvent ev)
```

`ShopSystem` 订阅 `WaveManager` 的 `OnPhaseChanged`，检查新阶段是不是 `PREPARE` 还是 `SHOPPING`，决定启动哪个面板。

`ShopSystem` 不能反过来调 `WaveManager` 改阶段，**只能通过 `WaveManager.RequestNextPhase(reason)` 这种命令式接口触发推进**。

---

## 五、典型跨系统流程（先清单，详细时序图见末篇）

### 5.1 战后商店买球

```text
WaveSystem
  └─ 阶段 PREPARE 结束、SHOPPING 开始
ShopSystem
  ├─ ShopController.OpenBoard(BoardKind.Ball)
  ├─ ShopRefreshService.GenerateBallOffers(X)
  └─ 等待玩家操作
玩家点击「购买」
ShopSystem
  ├─ IPurchasable.OnPurchased(...)
  ├─ InventorySystem.RelicBag.CanAdd(item)  -> true
  ├─ CoinManager.Pay(price, "shop_ball")
  ├─ InventorySystem.BallBag.Add(item)
  ├─ ShopOffer.MarkSold()  -> 面板置灰
  └─ ShopEvents.OnBallPurchased()
玩家点击「下一步」
ShopSystem
  ├─ OpenBoard(BoardKind.Relic)
  ... 直到退出
ShopSystem.RequestNextPhase("shopping_done")
WaveManager -> 下一波
```

### 5.2 球升级 / 融合

```text
玩家点「升级」按钮（在球背包/槽位 UI 上）
BallManagementSystem
  ├─ BallUpgradeService.TryUpgrade(BallInstance, count)
  │     ├─ 校验：同类同等级 >= X
  │     ├─ 扣除 X 个球
  │     ├─ 创建 1 个 level+1 的球
  │     └─ 写回槽位/背包
  ├─ BallInventoryAdapter.NotifyChange()
  └─ ShopSystem 监听变更，刷新 UI（如果该球在商店列表里被预览过）
类似：BallMergeService.TryMerge(a, b)
  ├─ 校验：双方均满级 + 金币足够
  ├─ CoinManager.Pay(mergePrice, "merge")
  ├─ 销毁 a, b
  └─ 创建 1 个 1 级融合球
```

### 5.3 球出售

```text
玩家在背包里点球 / 遗物 → 「出售」
ShopSystem.SellController
  ├─ BallManagementSystem.SellToShop(ball)        // 球专用逻辑（拆出售流程）
  ├─ InventorySystem.BallBag.Remove(ball)
  └─ CoinManager.Earn(SellPrice, "shop_sell")

RelicSystem.HandleSell(relic)
  ├─ InventorySystem.RelicBag.Remove(relic)
  └─ CoinManager.Earn(SellPrice, "shop_sell")
```

---

## 六、配置面（都给预留）

每个系统都暴露一个 `XXXConfig` ScriptableObject 或 CSV 表，作为策划面：

| Config | 字段 | 默认值 |
| --- | --- | --- |
| `BallSystemConfig` | `MaxBallLevel = 3`、`UpgradeCombineCount = 2`、`DefaultBallBagSize = 9`、`SlotCount = 3`、`SlotExpandable = true` | 3 / 2 / 9 / 3 / true |
| `InventorySystemConfig` | `BallBagCapacity = 9`、`RelicBagCapacity = 15`、`CapacityExpandable = true` | 9 / 15 / true |
| `ShopSystemConfig` | `BallOfferCount = 5`、`RelicOfferCount = 5`、`BallRefreshCost = 2`、`RelicRefreshCost = 1` | 5 / 5 / 2 / 1 |

策划改这三个文件即可调参；不需要碰系统代码。

---

## 七、扩展点（防止后期改代码）

| 需求演化 | 扩展点位置 |
| --- | --- |
| 槽位从 3 个变 5 个 | `BallSystemConfig.SlotCount`，并支持 `BallSlotGroup.AddSlot()` 运行时扩容 |
| 球背包从 9 变 12 | `InventorySystemConfig.BallBagCapacity`，并支持 `BallBag.Expand(delta)` |
| 遗物背包从 15 变 20 | `InventorySystemConfig.RelicBagCapacity`，并支持 `RelicBag.Expand(delta)` |
| 升级 X 从 2 变 3（三合一升级） | `BallSystemConfig.UpgradeCombineCount` |
| 融合配方变化 | `BallDef.MergeRecipe`（升级/融合都走 per-def 配方，不再硬编码 in 系统） |
| 商店面板出现"打折球" | `BallOffer` 加 `Discount` 字段，`ShopRefreshService` 概率生成 |
| 波次不同给不同商品池 | `ShopSystemConfig.PoolPerWave` 或 `ShopRefreshService.SetPool(poolId)` |

---

## 八、对现有项目代码的影响

| 现有模块 | 影响 |
| --- | --- |
| `_Gameplay/Ball/BallManager.cs` | 引入 `ABall/BallManagementSystem` 作为「上层」，但**不动**现有的 `_TopDownEngine/Balls/Ball.cs`、`BallStats.cs` 等物理层。`BallInstance` 在内部**持有**一个底层 `Ball` 的引用，但不直接动物理 |
| `_Gameplay/ARelics/ARelic.cs` | 不动其实现，加 `IInventoryItem` 接口 |
| `_Inventory/*`（MoreMountains 自带） | 不直接用它的 `Inventory` 当背包装球/遗物。我们的 `Bag` 抽象与它解耦，但 UI 层可以参考它的 `InventoryDisplayGrid` 思路做 UGUI 生成 |
| `_Gameplay/ACoin/CoinManager.cs` | 不动其实现，仅约定所有外部调用都走 `Pay/Earn`，并加 `reason` 字段 |
| `_Gameplay/ARooms/Phase/8_ShoppingPhase.cs` | **改为只持有** `ShopSystem.Instance` 的引用，把所有「购买 / 出售 / 重新随机 / 下一步」逻辑全部委托给 `ShopSystem` |
| `_Gameplay/AWaves/WaveManager.cs` | 增加 `OnPhaseChanged(PREPARE → SHOPPING)` 事件，`ShopSystem` 订阅即可 |

---

## 九、文件落点速查

```text
Assets/Scripts/HotFix/_Gameplay/
├── ABall/                          新增 - 球管理系统
│   ├── Core/
│   │   ├── BallDef.cs              静态数据：id/name/价格/升级配方/融合配方
│   │   ├── BallInstance.cs         运行时数据：种类+等级+绑定的底层 Ball
│   │   └── BallManagementSystem.cs 单例 + 入口
│   ├── Slot/
│   │   ├── BallSlot.cs             单个发射槽位
│   │   └── BallSlotGroup.cs        槽位集合（默认 3 个，可扩容）
│   ├── Adapter/
│   │   └── BallInventoryAdapter.cs 槽位↔球背包移动
│   ├── Service/
│   │   ├── BallUpgradeService.cs   X 个同种同等级合成更高等级
│   │   └── BallMergeService.cs     双满级 + 金币 → 融合球
│   ├── Shop/
│   │   ├── BallOffer.cs            IPurchasable 实现（球商品）
│   │   └── BallShopService.cs      购买/出售球的出入口
│   ├── Event/
│   │   └── BallEvents.cs           变更事件
│   └── Config/
│       └── BallSystemConfig.cs     策划配置 SO
│
├── AInventory/                     新增 - 背包系统
│   ├── Core/
│   │   ├── IInventoryItem.cs
│   │   ├── InventoryBag.cs         泛型格子集合
│   │   ├── BallBag.cs              球背包：9 格
│   │   ├── RelicBag.cs             遗物背包：15 格
│   │   ├── InventorySystem.cs      单例 + 总入口
│   │   └── InventoryFullException.cs
│   ├── Event/
│   │   └── InventoryEvents.cs
│   └── Config/
│       └── InventorySystemConfig.cs
│
├── AShop/                          新增 - 商店系统
│   ├── Controller/
│   │   ├── ShopController.cs       主控：球面板 / 遗物面板切换
│   │   ├── ShopBoardKind.cs
│   │   └── ShopUiBinder.cs         与 UGUI 绑定
│   ├── Offer/
│   │   ├── IPurchasable.cs
│   │   ├── ShopOffer.cs            包装 + 已售标记
│   │   └── RelicOffer.cs
│   ├── Service/
│   │   ├── ShopRefreshService.cs   随机 + 重新随机
│   │   └── SellService.cs          售出回收（半价）
│   ├── Event/
│   │   └── ShopEvents.cs
│   └── Config/
│       └── ShopSystemConfig.cs
│
└── AGameActions/                   复用现有 - 复杂操作都封装成 Action
    └── Shop/
        ├── BuyBallAction.cs
        ├── BuyRelicAction.cs
        ├── SellBallAction.cs
        ├── SellRelicAction.cs
        ├── RerollShopBoardAction.cs
        └── GoNextShopPhaseAction.cs
```

> 详细时序、状态机、表格字段，见各系统的子文档。

## 十、子文档导航

1. [BallManagementSystem 球管理系统设计](MyFramework:BallManagementSystem%20球管理系统设计.md)
2. [InventorySystem 背包系统设计](MyFramework:InventorySystem%20背包系统设计.md)
3. [ShopSystem 商店系统设计](MyFramework:ShopSystem%20商店系统设计.md)
4. [三大系统与波次协作时序图](MyFramework:三大系统与波次协作时序图.md)
