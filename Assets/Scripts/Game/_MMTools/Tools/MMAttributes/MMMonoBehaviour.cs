using Drawing;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMMonoBehaviour : MonoBehaviourGizmos
    {
        Transform _transform;

        public new Transform transform => _transform ??= base.transform;

        GameObject _gameObject;

        public new GameObject gameObject => _gameObject ??= base.gameObject;
    }
}