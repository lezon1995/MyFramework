using System;
using UnityEngine;

namespace MoreMountains.Tools
{
    [Serializable]
    public class MMLayer
    {
        [SerializeField]
        protected int _layerIndex;

        public virtual int LayerIndex
        {
            get { return _layerIndex; }
        }

        public virtual void Set(int layerIndex)
        {
            if (layerIndex is > 0 and < 32)
            {
                _layerIndex = layerIndex;
            }
        }

        public virtual int Mask
        {
            get { return 1 << _layerIndex; }
        }
    }
}