using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains
{
    /// <summary>
    /// 通用按钮动画组件
    /// 功能：
    /// - 鼠标进入时缓动放大至1.1倍
    /// - 鼠标移出时缓动恢复至1倍
    /// - 鼠标按下时缓动缩小至1.05倍
    /// - 鼠标抬起时缓动放大至1.1倍
    /// </summary>
    public class ButtonScaleAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("缩放配置")]
        [Tooltip("鼠标进入时的目标缩放值")]
        public float hoverScale = 1.1f;

        [Tooltip("鼠标按下时的目标缩放值")]
        public float pressScale = 1.05f;

        [Tooltip("正常状态缩放值")]
        public float normalScale = 1f;

        [Header("动画配置")]
        [Tooltip("动画时长（秒）")]
        public float tweenDuration = 0.1f;

        [Tooltip("缓动类型")]
        public Ease tweenEase = Ease.OutCubic;

        bool _isHovering;
        bool _isPressed;

        void Awake()
        {
            transform.localScale = Vector3.one * normalScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            float targetScale = _isPressed ? pressScale : hoverScale;
            Tween.Scale(transform, endValue: targetScale, duration: tweenDuration, ease: tweenEase);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            Tween.Scale(transform, endValue: normalScale, duration: tweenDuration, ease: tweenEase);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            Tween.Scale(transform, endValue: pressScale, duration: tweenDuration, ease: tweenEase);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            float targetScale = _isHovering ? hoverScale : normalScale;
            Tween.Scale(transform, endValue: targetScale, duration: tweenDuration, ease: tweenEase);
        }

        /// <summary>
        /// 快速设置为默认配置
        /// </summary>
        public void SetDefaultConfig()
        {
            hoverScale = 1.1f;
            pressScale = 1.05f;
            normalScale = 1f;
            tweenDuration = 0.1f;
            tweenEase = Ease.OutCubic;
        }

        /// <summary>
        /// 重置为正常状态
        /// </summary>
        public void ResetToNormal()
        {
            _isHovering = false;
            _isPressed = false;
            transform.localScale = Vector3.one * normalScale;
        }
    }
}