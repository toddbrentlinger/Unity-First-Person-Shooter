using System;
using UnityEngine;

public class BulletHoleController {

    private GameObject m_bulletHole;

    public GameObject GetBulletHole(PhysicMaterial sharedPhysicMaterial)
    {
        if (sharedPhysicMaterial == null)
            return ObjectPooler.sharedInstance.GetPooledObject("BulletHole");

        switch (sharedPhysicMaterial.name)
        {
            case ("Metal (Instance)"):
                m_bulletHole = ObjectPooler.sharedInstance.GetPooledObject("BulletImpactMetalEffect");
                CheckBulletHole();
                break;
            case ("Stone (Instance)"):
                m_bulletHole = ObjectPooler.sharedInstance.GetPooledObject("BulletImpactStoneEffect");
                CheckBulletHole();
                break;
            case ("Wood (Instance)"):
                m_bulletHole = ObjectPooler.sharedInstance.GetPooledObject("BulletImpactWoodEffect");
                CheckBulletHole();
                break;
            case ("Flesh (Instance)"):
                m_bulletHole = ObjectPooler.sharedInstance.GetPooledObject("BulletImpactFleshSmallEffect");
                CheckBulletHole();
                break;
            default:
                m_bulletHole = ObjectPooler.sharedInstance.GetPooledObject("BulletHole");
                break;
        }

        return m_bulletHole;
    }

    // Check if special case PhysicMaterial returned a GameObject from ObjectPooler
    private void CheckBulletHole()
    {
        if (m_bulletHole == null)
            m_bulletHole = ObjectPooler.sharedInstance.GetPooledObject("BulletHole");
    }
}
