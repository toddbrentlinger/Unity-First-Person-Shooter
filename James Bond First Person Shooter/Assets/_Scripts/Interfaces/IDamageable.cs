using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(Vector3 hitPoint, Vector3 hitForce);
}
