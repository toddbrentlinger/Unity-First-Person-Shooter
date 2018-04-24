using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* NOTES:
 * - Set initial position of bullet at targetEndPosition, instead of bulletFireStartPosition and move lineRenderer backwards?
 * Don't have to set position twice, but will have to set rotation twice (one for gun orientation and then surface normal to set bulletHole.
 * - Should I have one bullet prefab that holds all bulletHole types or should I pool certain number of each bulletHole type and get correct one from pool?
 */

public class Bullet : MonoBehaviour
{
    private LineRenderer m_bulletTrail;

    private void Awake()
    {
        m_bulletTrail = GetComponentInChildren<LineRenderer>();
    }

    IEnumerator FireBullet(float targetDistance, float bulletSpeed = 400f)
    {
        // Bullet Trail

        float currDistance = 0;
        while (currDistance < targetDistance)
        {
            currDistance += bulletSpeed * Time.deltaTime;
            currDistance = Mathf.Clamp(currDistance, 0, targetDistance);
            m_bulletTrail.SetPosition(1, Vector3.forward * currDistance);

            yield return null;
        }

        // Reset bulletTrail
        m_bulletTrail.SetPosition(1, Vector3.zero);

        // Bullet Hole

        // Set Bullet position to targetDistance
    }
}
