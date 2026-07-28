using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Tooltip使用示例
    /// </summary>
    public class TooltipUsageExamples : MonoBehaviour
    {
        [Header("Example Items")]
        [SerializeField]
        private GameObject[] exampleItems;

        private void Start()
        {
            SetupGlobalManager();
        }

        private void SetupGlobalManager()
        {
            // 确保TooltipManager存在
            if (TooltipManager.Instance == null)
            {
                GameObject managerObj = new GameObject("TooltipManager");
                TooltipManager manager = managerObj.AddComponent<TooltipManager>();
                DontDestroyOnLoad(managerObj);
            }
        }

        /// <summary>
        /// 示例1：基础使用
        /// </summary>
        public void Example1_BasicUsage()
        {
            // 创建Tooltip请求
            TooltipRequest request = new TooltipRequest
            {
                content = new TooltipContent("物品名称", "这是一个物品描述"),
                positionMode = TooltipPositionMode.PivotAnchored,
                anchorDirection = TooltipAnchorDirection.Top,
                durationMode = TooltipDurationMode.Permanent
            };

            // 显示Tooltip
            TooltipManager.Instance.ShowTooltip(request);
        }

        /// <summary>
        /// 示例2：使用定时显示
        /// </summary>
        public void Example2_TimedTooltip()
        {
            TooltipRequest request = new TooltipRequest
            {
                content = new TooltipContent("公告", "此公告将在5秒后自动关闭"),
                positionMode = TooltipPositionMode.Fixed,
                fixedPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f),
                durationMode = TooltipDurationMode.Timed,
                displayDuration = 5f
            };

            TooltipManager.Instance.ShowTooltip(request);
        }

        /// <summary>
        /// 示例3：鼠标位置跟随
        /// </summary>
        public void Example3_MouseFollow()
        {
            TooltipRequest request = new TooltipRequest
            {
                content = new TooltipContent("坐标信息", $"当前鼠标位置: {Input.mousePosition}"),
                positionMode = TooltipPositionMode.MousePosition,
                mouseOffset = new Vector2(20, -20),
                durationMode = TooltipDurationMode.Permanent
            };

            TooltipManager.Instance.ShowTooltip(request);
        }

        /// <summary>
        /// 示例4：带Meta关键字的Tooltip
        /// </summary>
        public void Example4_WithMetaTooltip()
        {
            // 首先注册Meta关键字
            TooltipManager.Instance.RegisterMetaKeyword("灼烧", new MetaTooltipContent(
                MetaKeywordType.Buff,
                "灼烧",
                "灼烧",
                "持续造成火焰伤害，每秒损失5%最大生命值"
            ));

            TooltipManager.Instance.RegisterMetaKeyword("冰冻", new MetaTooltipContent(
                MetaKeywordType.Buff,
                "冰冻",
                "冰冻",
                "使目标无法移动和攻击，持续2秒"
            ));

            // 显示包含关键字的Tooltip
            TooltipRequest request = new TooltipRequest
            {
                content = new TooltipContent("火球术", "向敌人发射火球，给玩家造成持续3秒的[灼烧]Buff，并附加2秒的[冰冻]效果"),
                positionMode = TooltipPositionMode.PivotAnchored,
                anchorDirection = TooltipAnchorDirection.Top,
                durationMode = TooltipDurationMode.Permanent,
                isMetaEnabled = true
            };

            TooltipManager.Instance.ShowTooltip(request);
        }

        /// <summary>
        /// 示例5：程序化配置Tooltip内容
        /// </summary>
        public void Example5_ProgrammaticContent()
        {
            // 动态生成内容
            TooltipContent content = new TooltipContent("");

            int playerLevel = 10;
            int damage = 100;
            float critRate = 0.25f;

            content.title = $"<color=yellow>圣剑·裁决</color>";
            content.description = $@"
基础伤害: {damage}
暴击率: {critRate * 100}%

<color=orange>被动技能</color>
攻击时有20%几率对目标施加[灼烧]效果

<color=skyblue>主动技能</color>
点击释放剑气，伤害提升50%，持续5秒
冷却时间: 30秒
            ";

            TooltipRequest request = new TooltipRequest
            {
                content = content,
                positionMode = TooltipPositionMode.PivotAnchored,
                anchorDirection = TooltipAnchorDirection.Right,
                durationMode = TooltipDurationMode.Permanent,
                isMetaEnabled = true
            };

            TooltipManager.Instance.ShowTooltip(request);
        }

        /// <summary>
        /// 示例6：为GameObject添加TooltipTrigger组件
        /// </summary>
        public void Example6_AddTooltipTrigger()
        {
            foreach (var item in exampleItems)
            {
                if (item != null)
                {
                    TooltipTrigger trigger = item.GetComponent<TooltipTrigger>();
                    if (trigger == null)
                    {
                        trigger = item.AddComponent<TooltipTrigger>();
                    }

                    // 设置内容
                    trigger.SetContent("物品", "这是一个示例物品");

                    // 设置显示参数
                    trigger.SetShowDelay(0.5f);
                    trigger.SetPositionMode(TooltipPositionMode.PivotAnchored);
                    trigger.SetAnchorDirection(TooltipAnchorDirection.Top);
                }
            }
        }

        /// <summary>
        /// 示例7：隐藏Tooltip
        /// </summary>
        public void Example7_HideTooltip()
        {
            // 立即隐藏（无动画）
            TooltipManager.Instance.HideTooltipImmediate();

            // 带动画隐藏
            TooltipManager.Instance.HideTooltip();
        }

        /// <summary>
        /// 示例8：监听Tooltip事件
        /// </summary>
        public void Example8_EventListening()
        {
            // 订阅全局事件
            TooltipEventSystem.Instance.OnShow += (eventData) =>
            {
                Debug.Log($"Tooltip显示: {eventData.content?.title}");
            };

            TooltipEventSystem.Instance.OnHide += (eventData) =>
            {
                Debug.Log($"Tooltip隐藏: {eventData.content?.title}");
            };
        }

        /// <summary>
        /// 示例9：使用智能位置计算
        /// </summary>
        public void Example9_SmartPositioning()
        {
            RectTransform targetRect = GetComponent<RectTransform>();
            if (targetRect == null) return;

            // 使用ToolipPositionCalculator计算最佳位置
            var result = TooltipPositionCalculator.CalculateSmartPosition(
                targetRect,
                null, // 如果没有预设的tooltip rect，可以传null
                TooltipAnchorDirection.Top,
                new Vector2(0, -10),
                10f // 屏幕边距
            );

            Debug.Log($"计算结果: 方向={result.direction}, 位置={result.position}, 已调整={result.wasAdjusted}");
        }

        /// <summary>
        /// 示例10：自定义Tooltip内容生成器
        /// </summary>
        public void Example10_CustomContentGenerator()
        {
            foreach (var item in exampleItems)
            {
                if (item != null)
                {
                    TooltipTrigger trigger = item.GetComponent<TooltipTrigger>();
                    if (trigger == null)
                    {
                        trigger = item.AddComponent<TooltipTrigger>();
                    }

                    // 添加自定义内容生成器
                    ExampleContentGenerator generator = item.GetComponent<ExampleContentGenerator>();
                    if (generator == null)
                    {
                        generator = item.AddComponent<ExampleContentGenerator>();
                    }

                    trigger.SetCustomContentGenerator(generator);
                }
            }
        }
    }
}
