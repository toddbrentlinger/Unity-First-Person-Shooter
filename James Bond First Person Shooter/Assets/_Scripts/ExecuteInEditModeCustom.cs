using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ExecuteInEditModeCustom : MonoBehaviour {

	void OnEnable ()
    {
        Rigidbody[] children = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in children)
            rb.mass *= 0.25f;
	}
}
