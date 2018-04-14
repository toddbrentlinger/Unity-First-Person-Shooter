using UnityEngine;

public class BulletDestroy : MonoBehaviour {

    // When bullet is actived
	void OnEnable()
    {
        Invoke("Destroy", 2f);
    }

    void Destroy()
    {
        gameObject.SetActive(false);
    }

    // When bullet is inactive
    void OnDisable()
    {
        // To prevent bullet from being disabled twice, cancel invoke when bullet is disabled
        CancelInvoke();
    }
}
