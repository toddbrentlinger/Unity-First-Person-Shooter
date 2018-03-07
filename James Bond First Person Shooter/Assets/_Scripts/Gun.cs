using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

    const float k_cooldown = 0.1f;

    [SerializeField]
    GameObject bulletHole;

    RaycastHit hit;
    Ray ray;
    float lastShotTime;
    Vector3 initialPosition;

	// Use this for initialization
	void Start ()
    {
        initialPosition = transform.localPosition;
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Play gun barrel fire effects

		if (Input.GetButton("Fire1") && Time.time > lastShotTime + k_cooldown)
        {
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);

            ray = Camera.main.ScreenPointToRay(screenCenterPoint);

            if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane) && bulletHole != null)
            {
                // ADD : If hit.gameObject is bulletHole, don't add small offset

                Vector3 bulletHolePosition = hit.point + hit.normal * .01f;

                Quaternion bulletHoleRotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);

                GameObject hole = (GameObject)Instantiate(bulletHole, bulletHolePosition, bulletHoleRotation);
                hole.transform.SetParent(hit.transform);
            }

            lastShotTime = Time.time;

            transform.localPosition = initialPosition + new Vector3(RandomCoordinate(), RandomCoordinate(), RandomCoordinate());
        }
        else if (transform.localPosition != initialPosition)
        {
            transform.localPosition = initialPosition;
        }
	}

    float RandomCoordinate()
    {
        return Random.Range(-.01f, .01f);
    }
}
