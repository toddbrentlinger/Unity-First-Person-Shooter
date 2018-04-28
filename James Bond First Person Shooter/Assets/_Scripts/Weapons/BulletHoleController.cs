using System;
using UnityEngine;

public class BulletHoleController {

    private GameObject m_bulletHole;

    //public GameObject GetBulletHole(PhysicMaterial sharedPhysicMaterial)
    public GameObject GetBulletHole(RaycastHit hit)
    {
        PhysicMaterial sharedPhysicMaterial = hit.collider.material;

        if (sharedPhysicMaterial == null)
            return ObjectPooler.sharedInstance.GetPooledObject("BulletHole");

        switch (sharedPhysicMaterial.name)
        {
            case ("Metal (Instance)"):
                m_bulletHole = ObjectPooler.sharedInstance.GetPooledObject("BulletImpactMetalEffect");
                CheckBulletHole();
                /*
                // Change albedo color to match texture of attached collider gameobject
                if (m_bulletHole)
                {
                    Renderer colliderRenderer = hit.transform.GetComponent<Renderer>();
                    ChangeMetalBulletHoleColor(colliderRenderer, hit);
                }
                else
                {
                    m_bulletHole = ObjectPooler.sharedInstance.GetPooledObject("BulletHole");
                }
                */
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

    private void ChangeMetalBulletHoleColor(Renderer colliderRenderer, RaycastHit hit)
    {
        if (colliderRenderer == null || colliderRenderer.sharedMaterial == null || colliderRenderer.sharedMaterial.mainTexture == null)
            return;

        // Get color of Renderer
        // NOTE: Cannot read texture unless Read/Write Enabled is true (default is false)
        Texture2D colliderTexture2D = colliderRenderer.material.mainTexture as Texture2D;
        Color materialColor = colliderTexture2D.GetPixel((int)hit.textureCoord.x,(int)hit.textureCoord.y);

        // Change color of metal bullet hole
        m_bulletHole.GetComponentInChildren<Renderer>().material.color = materialColor;
    }
}
