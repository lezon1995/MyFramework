using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public class SafeBorder : MonoBehaviour
    {
        LayerMask BallLayers = LayerManager.Ball_Mask;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!BallLayers.MMContains(other.gameObject.layer))
                return;

            if (other.TryGetComponent(out Ball ball))
            {
                if (player)
                    player.recollectBall(ball, 0F, true);
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (!BallLayers.MMContains(other.gameObject.layer))
                return;

            if (other.TryGetComponent(out Ball ball))
            {
                if (player)
                    player.recollectBall(ball, 0F, true);
            }
        }
    }
}