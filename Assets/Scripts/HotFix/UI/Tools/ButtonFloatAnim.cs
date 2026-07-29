using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains
{
    /// <summary>
    /// 通用按钮位移动画组件
    /// 功能：
    /// - 鼠标进入时缓动向上移动20像素
    /// - 鼠标移出时缓动恢复原始位置
    /// - 鼠标按下时缓动向下移动10像素
    /// - 鼠标抬起时缓动向上移动10像素
    /// </summary>
    public class ButtonFloatAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("位移配置")]
        [Tooltip("鼠标进入时向上的偏移量（像素）")]
        public float hoverOffset = 20f;

        [Tooltip("鼠标按下时向下的偏移量（像素），相对当前位置计算")]
        public float pressOffset = 10f;

        [Header("动画配置")]
        [Tooltip("动画时长（秒）")]
        public float tweenDuration = 0.1f;

        [Tooltip("缓动类型")]
        public Ease tweenEase = Ease.OutCubic;

        private Vector3 _originalPosition;
        private bool _isHovering;
        private bool _isPressed;

        private void Awake()
        {
            _originalPosition = transform.localPosition;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            float targetY = _isPressed ? _originalPosition.y + hoverOffset - pressOffset : _originalPosition.y + hoverOffset;
            Tween.LocalPositionY(transform, endValue: targetY, duration: tweenDuration, ease: tweenEase);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            Tween.LocalPositionY(transform, endValue: _originalPosition.y, duration: tweenDuration, ease: tweenEase);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            float baseY = _isHovering ? _originalPosition.y + hoverOffset : _originalPosition.y;
            Tween.LocalPositionY(transform, endValue: baseY - pressOffset, duration: tweenDuration, ease: tweenEase);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            float targetY = _isHovering ? _originalPosition.y + hoverOffset : _originalPosition.y;
            Tween.LocalPositionY(transform, endValue: targetY, duration: tweenDuration, ease: tweenEase);
        }

        /// <summary>
        /// 快速设置为默认配置
        /// </summary>
        public void SetDefaultConfig()
        {
            hoverOffset = 20f;
            pressOffset = 10f;
            tweenDuration = 0.1f;
            tweenEase = Ease.OutCubic;
        }

        /// <summary>
        /// 重置为原始位置
        /// </summary>
        public void ResetToOriginal()
        {
            _isHovering = false;
            _isPressed = false;
            Tween.LocalPositionY(transform, endValue: _originalPosition.y, duration: tweenDuration, ease: tweenEase);
        }
    }
}