using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class WeaponCollision : MonoBehaviour
{
    public class CollisionEvent : UnityEvent<Transform> { }
    public   CollisionEvent _onHit;

private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            _onHit.Invoke(other.transform);
        }
    }


}
