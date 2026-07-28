using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Tooltip位置计算器
    /// 提供高级位置计算功能，包括屏幕边界检测、动态方向选择等
    /// </summary>
    public static class TooltipPositionCalculator
    {
        #region Screen Boundary Detection

        /// <summary>
        /// 检测指定位置和尺寸是否会超出屏幕边界
        /// </summary>
        public static bool IsOutOfScreen(Vector2 position, Vector2 size, Camera uiCamera = null)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            if (position.x < 0 || position.x + size.x > screenWidth)
            {
                return true;
            }

            if (position.y < 0 || position.y + size.y > screenHeight)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取超出边界的信息
        /// </summary>
        public static ScreenBoundaryInfo GetBoundaryInfo(Vector2 position, Vector2 size, Camera uiCamera = null)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float rightOverflow = (position.x + size.x) - screenWidth;
            float bottomOverflow = position.y - size.y;
            float leftOverflow = -position.x;
            float topOverflow = (position.y + size.y) - screenHeight;

            return new()
            {
                isOverflowLeft = leftOverflow > 0,
                isOverflowRight = rightOverflow > 0,
                isOverflowTop = topOverflow > 0,
                isOverflowBottom = bottomOverflow > 0,
                leftOverflow = leftOverflow,
                rightOverflow = rightOverflow,
                topOverflow = topOverflow,
                bottomOverflow = bottomOverflow
            };
        }

        #endregion

        #region Smart Position Calculation

        /// <summary>
        /// 根据目标UI元素位置智能计算Tooltip显示位置
        /// </summary>
        public static SmartPositionResult CalculateSmartPosition(
            RectTransform targetRect,
            RectTransform tooltipRect,
            TooltipAnchorDirection preferredDirection,
            Vector2 offset,
            float screenPadding,
            Camera uiCamera = null)
        {
            if (targetRect == null || tooltipRect == null)
            {
                return new()
                {
                    position = Vector2.zero,
                    direction = preferredDirection,
                    wasAdjusted = false
                };
            }

            Vector2 targetPosition = GetRectWorldPosition(targetRect, uiCamera);
            Vector2 targetSize = GetRectSize(targetRect);

            Vector2 preferredPosition = CalculatePositionForDirection(
                targetPosition, targetSize, preferredDirection, offset);

            bool isOutOfScreen = IsOutOfScreenWithRect(preferredPosition, tooltipRect, screenPadding);

            if (!isOutOfScreen)
            {
                return new ()
                {
                    position = preferredPosition,
                    direction = preferredDirection,
                    wasAdjusted = false
                };
            }

            TooltipAnchorDirection[] alternativeDirections = GetAlternativeDirections(preferredDirection);

            foreach (var direction in alternativeDirections)
            {
                Vector2 alternativePosition = CalculatePositionForDirection(
                    targetPosition, targetSize, direction, offset);

                if (!IsOutOfScreenWithRect(alternativePosition, tooltipRect, screenPadding))
                {
                    return new ()
                    {
                        position = alternativePosition,
                        direction = direction,
                        wasAdjusted = true
                    };
                }
            }

            return new ()
            {
                position = AdjustPositionToScreen(preferredPosition, tooltipRect, screenPadding),
                direction = preferredDirection,
                wasAdjusted = true
            };
        }

        static bool IsOutOfScreenWithRect(Vector2 position, RectTransform rect, float padding)
        {
            if (rect == null) 
                return false;

            var size = GetRectSize(rect);
            var adjustedPos = new Vector2(
                position.x - size.x * rect.pivot.x,
                position.y - size.y * (1f - rect.pivot.y));

            return IsOutOfScreen(adjustedPos, size) ||
                   position.x < padding ||
                   position.y < padding ||
                   position.x + size.x > Screen.width - padding ||
                   position.y + size.y > Screen.height - padding;
        }

        static Vector2 AdjustPositionToScreen(Vector2 position, RectTransform rect, float padding)
        {
            if (rect == null) return position;

            var size = GetRectSize(rect);

            float minX = padding;
            float maxX = Screen.width - size.x - padding;
            float minY = padding;
            float maxY = Screen.height - size.y - padding;

            return new(
                Mathf.Clamp(position.x, minX, maxX),
                Mathf.Clamp(position.y, minY, maxY)
            );
        }

        #endregion

        #region Direction Helpers

        static TooltipAnchorDirection[] GetAlternativeDirections(TooltipAnchorDirection original)
        {
            return original switch
            {
                TooltipAnchorDirection.Top => new[] { TooltipAnchorDirection.Bottom, TooltipAnchorDirection.TopLeft, TooltipAnchorDirection.TopRight },
                TooltipAnchorDirection.Bottom => new[] { TooltipAnchorDirection.Top, TooltipAnchorDirection.BottomLeft, TooltipAnchorDirection.BottomRight },
                TooltipAnchorDirection.Left => new[] { TooltipAnchorDirection.Right, TooltipAnchorDirection.TopLeft, TooltipAnchorDirection.BottomLeft },
                TooltipAnchorDirection.Right => new[] { TooltipAnchorDirection.Left, TooltipAnchorDirection.TopRight, TooltipAnchorDirection.BottomRight },
                TooltipAnchorDirection.TopLeft => new[] { TooltipAnchorDirection.TopRight, TooltipAnchorDirection.BottomLeft, TooltipAnchorDirection.Top },
                TooltipAnchorDirection.TopRight => new[] { TooltipAnchorDirection.TopLeft, TooltipAnchorDirection.BottomRight, TooltipAnchorDirection.Top },
                TooltipAnchorDirection.BottomLeft => new[] { TooltipAnchorDirection.BottomRight, TooltipAnchorDirection.TopLeft, TooltipAnchorDirection.Bottom },
                TooltipAnchorDirection.BottomRight => new[] { TooltipAnchorDirection.BottomLeft, TooltipAnchorDirection.TopRight, TooltipAnchorDirection.Bottom },
                _ => new[] { TooltipAnchorDirection.Top, TooltipAnchorDirection.Bottom, TooltipAnchorDirection.Left, TooltipAnchorDirection.Right }
            };
        }

        #endregion

        #region Rect Helpers

        /// <summary>
        /// 获取RectTransform的世界坐标位置
        /// </summary>
        public static Vector2 GetRectWorldPosition(RectTransform rect, Camera uiCamera = null)
        {
            if (rect == null)
                return Vector2.zero;

            using var _ = new ArrayScope<Vector3>(out var corners, 4);
            rect.GetWorldCorners(corners);
            return corners[0];
        }

        /// <summary>
        /// 获取RectTransform的尺寸
        /// </summary>
        public static Vector2 GetRectSize(RectTransform rect)
        {
            if (rect == null)
                return Vector2.zero;

            return rect.rect.size;
        }

        /// <summary>
        /// 计算特定方向的位置
        /// </summary>
        public static Vector2 CalculatePositionForDirection(
            Vector2 targetPosition,
            Vector2 targetSize,
            TooltipAnchorDirection direction,
            Vector2 offset)
        {
            float halfWidth = targetSize.x * 0.5f;
            float halfHeight = targetSize.y * 0.5f;

            return direction switch
            {
                TooltipAnchorDirection.Top => new Vector2(targetPosition.x + halfWidth, targetPosition.y + targetSize.y) + offset,
                TooltipAnchorDirection.Bottom => new Vector2(targetPosition.x + halfWidth, targetPosition.y) + offset,
                TooltipAnchorDirection.Left => new Vector2(targetPosition.x, targetPosition.y + halfHeight) + offset,
                TooltipAnchorDirection.Right => new Vector2(targetPosition.x + targetSize.x, targetPosition.y + halfHeight) + offset,
                TooltipAnchorDirection.TopLeft => new Vector2(targetPosition.x, targetPosition.y + targetSize.y) + offset,
                TooltipAnchorDirection.TopRight => new Vector2(targetPosition.x + targetSize.x, targetPosition.y + targetSize.y) + offset,
                TooltipAnchorDirection.BottomLeft => new Vector2(targetPosition.x, targetPosition.y) + offset,
                TooltipAnchorDirection.BottomRight => new Vector2(targetPosition.x + targetSize.x, targetPosition.y) + offset,
                TooltipAnchorDirection.Center => new Vector2(targetPosition.x + halfWidth, targetPosition.y + halfHeight) + offset,
                _ => new Vector2(targetPosition.x + halfWidth, targetPosition.y + targetSize.y) + offset
            };
        }

        #endregion

        #region Mouse Position Helpers

        /// <summary>
        /// 计算鼠标位置，考虑屏幕边界
        /// </summary>
        public static Vector2 CalculateMousePositionWithBoundary(
            Vector2 mousePosition,
            Vector2 tooltipSize,
            Vector2 offset,
            float screenPadding)
        {
            float x = mousePosition.x + offset.x;
            float y = mousePosition.y + offset.y;

            x = Mathf.Clamp(x, screenPadding, Screen.width - tooltipSize.x - screenPadding);
            y = Mathf.Clamp(y, screenPadding, Screen.height - tooltipSize.y - screenPadding);

            return new(x, y);
        }

        /// <summary>
        /// 获取鼠标在UI空间的位置
        /// </summary>
        public static Vector2 GetMouseUIPosition(Camera uiCamera)
        {
            Vector3 mousePos = Input.mousePosition;

            if (uiCamera != null)
            {
                return uiCamera.ScreenToWorldPoint(mousePos);
            }

            return mousePos;
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// 屏幕边界信息
    /// </summary>
    public struct ScreenBoundaryInfo
    {
        public bool isOverflowLeft;
        public bool isOverflowRight;
        public bool isOverflowTop;
        public bool isOverflowBottom;

        public float leftOverflow;
        public float rightOverflow;
        public float topOverflow;
        public float bottomOverflow;

        public bool IsOverflowing => isOverflowLeft || isOverflowRight || isOverflowTop || isOverflowBottom;

        public bool IsOverflowingHorizontal => isOverflowLeft || isOverflowRight;

        public bool IsOverflowingVertical => isOverflowTop || isOverflowBottom;
    }

    /// <summary>
    /// 智能位置计算结果
    /// </summary>
    public struct SmartPositionResult
    {
        public Vector2 position;
        public TooltipAnchorDirection direction;
        public bool wasAdjusted;

        public bool IsValid => position != Vector2.zero;
    }

    #endregion
}