using UnityEngine;

[ExecuteInEditMode]
public class ReduceRigidbodyMassInChildren : MonoBehaviour {

    public float reductionFactor = .25f;

	void OnDisable ()
    {
        Rigidbody[] children = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in children)
            rb.mass *= reductionFactor;
	}
}
