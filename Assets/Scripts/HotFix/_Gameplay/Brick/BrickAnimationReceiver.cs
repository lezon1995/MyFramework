using System;
using UnityEngine;

namespace MoreMountains
{
    public class BrickAnimationReceiver : MonoBehaviour
    {
        Action onAnimationEnd;

        public void setOnAnimationEnd(Action a) => onAnimationEnd = a;

        public void OnAnimationEnd()
        {
            onAnimationEnd?.Invoke();
        }
    }
}