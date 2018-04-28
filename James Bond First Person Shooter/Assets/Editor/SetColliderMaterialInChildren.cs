using UnityEngine;

[ExecuteInEditMode]
public class SetColliderMaterialInChildren : MonoBehaviour {

    public PhysicMaterial physicMaterialToAdd;

    void OnDisable()
    {
        Collider[] children = GetComponentsInChildren<Collider>();

        foreach (Collider collider in children)
            collider.material = physicMaterialToAdd;
    }
}
