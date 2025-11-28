using System;
using UnityEngine;

public class ColliderTrigger : MonoBehaviour
{
    public Action<Collider> onTriggerEnter;
    public Action<Collider> onTriggerExit;

    void OnTriggerEnter(Collider other)
    {
        onTriggerEnter?.Invoke(other);
    }

    void OnTriggerExit(Collider other)
    {
        onTriggerExit?.Invoke(other);
    }
}