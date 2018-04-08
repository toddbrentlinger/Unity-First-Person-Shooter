using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ExecuteInEditModeCustom : MonoBehaviour {

    public float reductionFactor = .25f;

	void OnDisable ()
    {
        Rigidbody[] children = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in children)
            rb.mass *= reductionFactor;
	}
}
