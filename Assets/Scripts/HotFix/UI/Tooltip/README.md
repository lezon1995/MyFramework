# UITooltip System

通用UITooltip系统，支持多种显示模式、位置计算、屏幕边界检测和Meta关键字解析。

## 目录结构

```
Assets/Scripts/HotFix/UI/Tooltip/
├── TooltipEnums.cs                    # 枚举定义
├── TooltipSettings.cs                 # 全局配置
├── TooltipContent.cs                  # 内容数据结构
├── TooltipManager.cs                  # 全局管理器
├── TooltipBox.cs                     # Tooltip显示组件
├── MetaTooltipBox.cs                  # MetaTooltip显示组件
├── TooltipTrigger.cs                  # 触发器组件
├── TooltipPositionCalculator.cs       # 位置计算器
├── TooltipEvents.cs                   # 事件系统
├── Editor/
│   └── TooltipEditor.cs              # 自定义编辑器
├── Extensions/
│   └── ExampleGenerators.cs          # 内容生成器示例
└── Examples/
    └── TooltipUsageExamples.cs        # 使用示例
```

## 快速开始

### 1. 添加TooltipManager

将 `TooltipManager` 组件添加到场景中的持久化GameObject上，或者它会自动在需要时创建。

### 2. 创建TooltipTrigger

在需要显示Tooltip的UI元素上添加 `TooltipTrigger` 组件：

```csharp
// 在UI元素上添加组件
TooltipTrigger trigger = gameObject.AddComponent<TooltipTrigger>();
trigger.SetContent("物品名称", "这是物品描述");
trigger.SetShowDelay(0.5f);        // 延迟0.5秒显示
trigger.SetDisplayDuration(5f);    // 显示5秒后自动关闭
```

### 3. 手动显示/隐藏Tooltip

```csharp
// 创建请求
TooltipRequest request = new TooltipRequest
{
    content = new TooltipContent("标题", "描述内容"),
    positionMode = TooltipPositionMode.PivotAnchored,
    anchorDirection = TooltipAnchorDirection.Top,
    durationMode = TooltipDurationMode.Permanent
};

// 显示
TooltipManager.Instance.ShowTooltip(request);

// 隐藏
TooltipManager.Instance.HideTooltip();
```

## 功能特性

### 显示位置模式

| 模式 | 说明 |
|------|------|
| `Fixed` | 在固定位置显示 |
| `MousePosition` | 跟随鼠标位置 |
| `PivotAnchored` | 相对于目标UI元素锚点显示 |

### 锚点方向

支持8个方向：`Top`, `Bottom`, `Left`, `Right`, `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`, `Center`

### 显示时长模式

| 模式 | 说明 |
|------|------|
| `Permanent` | 永久显示，直到鼠标移出 |
| `Timed` | 固定时长后自动隐藏 |

### MetaTooltip关键字

系统会自动检测Tooltip内容中的关键字（如 `[灼烧]`），并显示对应的MetaTooltipBox：

```csharp
// 注册Meta关键字
TooltipManager.Instance.RegisterMetaKeyword("灼烧", new MetaTooltipContent(
    MetaKeywordType.Buff,
    "灼烧",
    "灼烧",
    "持续造成火焰伤害"
));

// 显示包含关键字的Tooltip
var content = new TooltipContent("火球术", "给敌人施加[灼烧]效果");
```

## 配置参数

在 `TooltipSettings` 中配置全局默认参数：

- `defaultShowDelay`: 默认显示延迟
- `defaultDisplayDuration`: 默认显示时长
- `defaultPositionMode`: 默认位置模式
- `screenEdgePadding`: 屏幕边缘间距
- `autoAdjustPosition`: 是否自动调整位置避免超出屏幕
- `keywordPattern`: 关键字匹配正则表达式

## 使用示例

### 示例1：基础使用

```csharp
TooltipRequest request = new TooltipRequest
{
    content = new TooltipContent("物品", "这是一个物品描述"),
    positionMode = TooltipPositionMode.PivotAnchored,
    anchorDirection = TooltipAnchorDirection.Top,
    durationMode = TooltipDurationMode.Permanent
};

TooltipManager.Instance.ShowTooltip(request);
```

### 示例2：定时显示

```csharp
TooltipRequest request = new TooltipRequest
{
    content = new TooltipContent("公告", "5秒后自动关闭"),
    positionMode = TooltipPositionMode.Fixed,
    fixedPosition = new Vector2(Screen.width / 2, Screen.height / 2),
    durationMode = TooltipDurationMode.Timed,
    displayDuration = 5f
};

TooltipManager.Instance.ShowTooltip(request);
```

### 示例3：鼠标跟随

```csharp
TooltipRequest request = new TooltipRequest
{
    content = new TooltipContent("坐标", $"X: {mouseX}, Y: {mouseY}"),
    positionMode = TooltipPositionMode.MousePosition,
    mouseOffset = new Vector2(20, -20)
};

TooltipManager.Instance.ShowTooltip(request);
```

### 示例4：带Meta关键字

```csharp
// 先注册关键字
TooltipManager.Instance.RegisterMetaKeyword("灼烧", new MetaTooltipContent(
    MetaKeywordType.Buff, "灼烧", "灼烧", "持续造成伤害"
));

// 显示包含关键字的Tooltip
var request = new TooltipRequest
{
    content = new TooltipContent("火球", "给敌人施加[灼烧]效果"),
    isMetaEnabled = true
};

TooltipManager.Instance.ShowTooltip(request);
```

## 事件系统

```csharp
// 监听全局Tooltip事件
TooltipEventSystem.Instance.OnShow += (eventData) =>
{
    Debug.Log($"显示: {eventData.content.title}");
};

TooltipEventSystem.Instance.OnHide += (eventData) =>
{
    Debug.Log($"隐藏: {eventData.content.title}");
};
```

## 自定义内容生成器

实现 `TooltipContentGenerator` 接口来自定义内容生成逻辑：

```csharp
public class MyContentGenerator : MonoBehaviour, TooltipContentGenerator
{
    public TooltipContent GenerateContent(TooltipTrigger trigger)
    {
        // 根据trigger动态生成内容
        return new TooltipContent("动态标题", "动态描述");
    }
}

// 使用
trigger.SetCustomContentGenerator(myGenerator);
```
