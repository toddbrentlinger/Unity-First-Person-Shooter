using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    //void TakeDamage(Vector3 hitPoint, Vector3 hitForce, int damage);
    //void TakeDamage(Vector3 forceVector, RaycastHit colliderHit, int damage);
    void TakeDamage(Vector3 hitPoint, Vector3 hitForce, int damage, Rigidbody rigidbodyHit);
}
