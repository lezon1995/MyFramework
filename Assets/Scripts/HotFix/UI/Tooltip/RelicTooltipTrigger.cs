using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains
{
    /// <summary>
    /// Relic Tooltip触发器
    /// 鼠标进入时显示RelicTooltipItem，位置在触发器Rect的左上点
    /// </summary>
    public class RelicTooltipTrigger : TooltipTrigger
    {
        RelicDef relicDef;
        RelicTooltipItem relicTooltipItem;

        public void setRelicTooltipItem(RelicTooltipItem item) => relicTooltipItem = item;
        public void setRelicDef(RelicDef def) => relicDef = def;

        protected override bool CanShowTooltip()
        {
            if (relicDef == null)
                return false;

            return base.CanShowTooltip();
        }

        protected override void ShowTooltipInternal()
        {
            if (!CanShowTooltip())
                return;

            if (relicTooltipItem == null)
                return;

            var tooltipItem = relicTooltipItem;
            if (tooltipItem == null)
                return;

            var tooltipRect = tooltipItem.getRoot();
            if (tooltipRect == null)
                return;

            Vector2 topLeftPos = CalculateTopLeftPosition();
            tooltipRect.setAnchoredPosition(topLeftPos);
            tooltipItem.setActive(true);

            _isTooltipShown = true;

            tooltipItem.Refresh(relicDef);
        }

        protected override void HideTooltipInternal()
        {
            if (!_isTooltipShown)
                return;

            if (relicTooltipItem != null)
            {
                relicTooltipItem.setActive(false);
            }

            _isTooltipShown = false;
        }

        protected override void Awake()
        {
            base.Awake();
            ShowDelay = 0f;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
            _hoverTimer = 0f;
            _isHoverTimerRunning = true;
            ShowTooltipInternal();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
            _isHoverTimerRunning = false;
            _hoverTimer = 0f;
            HideTooltipInternal();
        }

        private Vector2 CalculateTopLeftPosition()
        {
            RectTransform triggerRect = rectTransform;
            if (triggerRect == null)
                return Vector2.zero;

            var tooltipRect = relicTooltipItem.getRoot();
            if (tooltipRect == null)
                return Vector2.zero;

            Vector2 topLeftLocalPos = new Vector2(
                -triggerRect.sizeDelta.x * triggerRect.pivot.x,
                triggerRect.sizeDelta.y * (1f - triggerRect.pivot.y)
            );

            Vector3 worldTopLeft = triggerRect.TransformPoint(topLeftLocalPos);
            Vector3 localPos = tooltipRect.transform.parent.InverseTransformPoint(worldTopLeft);

            return localPos;
        }
    }
}