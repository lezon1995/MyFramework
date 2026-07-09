using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.EventSystems;

namespace MoreMountains.InventoryEngine
{
    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// This class handles the selection marker, that will mark the currently selected slot
    /// </summary>
    public class InventorySelectionMarker : MonoBehaviour
    {
        [MMInformation("The selection marker will highlight the current selection. Here you can define its transition speed and minimal distance threshold (it's usually ok to leave it to default).")]
        /// the speed at which the selection marker will move from one slot to the other
        public float TransitionSpeed = 5f;

        /// the threshold distance at which the marker will stop moving
        public float MinimalTransitionDistance = 0.01f;

        protected RectTransform _rectTransform;
        protected GameObject _selection;
        protected Vector3 _originPosition;
        protected Vector3 _originLocalScale;
        protected Vector3 _originSizeDelta;
        protected float _originTime;
        protected bool _originIsNull = true;
        protected float _deltaTime;

        /// <summary>
        /// On Start, we get the associated rect transform
        /// </summary>
        void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// On Update, we get the current selected object, and we move the marker to it if necessary
        /// </summary>
        void Update()
        {
            _selection = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (_selection == null)
                return;

            if (!_selection.TryGetComponent<InventorySlot>(out _))
                return;

            if (Vector3.Distance(transform.position, _selection.transform.position) > MinimalTransitionDistance)
            {
                if (_originIsNull)
                {
                    _originIsNull = false;
                    _originPosition = transform.position;
                    _originLocalScale = _rectTransform.localScale;
                    _originSizeDelta = _rectTransform.sizeDelta;
                    _originTime = Time.unscaledTime;
                }

                _deltaTime = (Time.unscaledTime - _originTime) * TransitionSpeed;
                transform.position = Vector3.Lerp(_originPosition, _selection.transform.position, _deltaTime);
                _rectTransform.localScale = Vector3.Lerp(_originLocalScale, _selection.GetComponent<RectTransform>().localScale, _deltaTime);
                _rectTransform.sizeDelta = Vector3.Lerp(_originSizeDelta, _selection.GetComponent<RectTransform>().sizeDelta, _deltaTime);
            }
            else
            {
                _originIsNull = true;
            }
        }
    }
}