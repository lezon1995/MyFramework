using System;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreMountains.Tools
{
    public class MMCinemachineScreenCenterMaintainer : MonoBehaviour
    {
        public CinemachineBrain brain;

        void Awake()
        {
            if (brain == null)
            {
                brain = FindFirstObjectByType<CinemachineBrain>();
            }
        }

        void LateUpdate()
        {
            var cam = brain.ActiveVirtualCamera;
            if (cam == null)
                return;

            transform.position = cam.State.GetFinalPosition();
        }
    }
}