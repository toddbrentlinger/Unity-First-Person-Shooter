using UnityEngine;

public class EnemyV2 : MonoBehaviour {

    private Rigidbody[] limbRigidbodies;
    private Animator enemyAnimator;

	// Use this for initialization
	void Awake ()
    {
        limbRigidbodies = GetComponentsInChildren<Rigidbody>();
        enemyAnimator = GetComponent<Animator>();
	}

    void Start()
    {
        foreach (Rigidbody rb in limbRigidbodies)
        {
            rb.isKinematic = true;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.Tab) && limbRigidbodies.Length > 0)
        {
            foreach (Rigidbody rb in limbRigidbodies)
            {
                rb.isKinematic = false;
                enemyAnimator.enabled = false;
            }
        }
	}
}
