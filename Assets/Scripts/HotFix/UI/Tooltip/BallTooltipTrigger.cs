using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains
{
    /// <summary>
    /// Ball Tooltip触发器
    /// 鼠标进入时显示BallTooltipItem，位置在触发器Rect的右上点
    /// </summary>
    public class BallTooltipTrigger : TooltipTrigger
    {
        BallItem ballItem;
        BallTooltipItem ballTooltipItem;
        
        public void setBallTooltipItem(BallTooltipItem item) => ballTooltipItem = item;
        public void setBallItem(BallItem def) => ballItem = def;

        protected override bool CanShowTooltip()
        {
            if (ballItem == null)
                return false;

            return base.CanShowTooltip();
        }

        protected override void ShowTooltipInternal()
        {
            if (!CanShowTooltip())
                return;

            if (ballTooltipItem == null)
                return;

            var tooltipItem = ballTooltipItem;
            if (tooltipItem == null)
                return;

            var tooltipRect = tooltipItem.getRoot();
            if (tooltipRect == null)
                return;

            Vector2 topRightPos = CalculateTopRightPosition();
            tooltipRect.setAnchoredPosition(topRightPos);
            tooltipItem.setActive(true);

            _isTooltipShown = true;
            tooltipItem.Refresh(ballItem);
        }

        protected override void HideTooltipInternal()
        {
            if (!_isTooltipShown)
                return;

            if (ballTooltipItem != null)
            {
                ballTooltipItem.setActive(false);
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

        private Vector2 CalculateTopRightPosition()
        {
            RectTransform triggerRect = rectTransform;
            if (triggerRect == null)
                return Vector2.zero;

            var tooltipRect = ballTooltipItem.getRoot();
            if (tooltipRect == null)
                return Vector2.zero;

            Vector2 topRightLocalPos = new Vector2(
                triggerRect.sizeDelta.x * (1f - triggerRect.pivot.x),
                triggerRect.sizeDelta.y * (1f - triggerRect.pivot.y)
            );

            Vector3 worldTopRight = triggerRect.TransformPoint(topRightLocalPos);
            Vector3 localPos = tooltipRect.transform.parent.InverseTransformPoint(worldTopRight);

            return localPos;
        }
    }
}
