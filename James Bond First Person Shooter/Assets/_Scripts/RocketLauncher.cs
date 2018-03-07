using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour {

    // public float rocketSpeed = 10f;  // Initial velocity of rocket

    // public Rigidbody rocket;
    // public float speed = 10f;
    // public int maxRockets = 20;

    // private GameObject[] rocketArr;
    // private float[] testArr;

    private Rocket rocketClone;

    // Update is called once per frame
    void Update () {
        // Call fireRocket() method when holding down CTRL or Right-Mouse
        if (Input.GetButtonDown("Fire2")) {

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            // If raycast hits collider
            if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane))
            {
                // Rotate rocket to face contact point
            }
            // Else raycast does NOT hit collider
            else
            {
                // Rotate rocket to face straight or to Camera.main.farClipPlane in middle of screen
            }

            FireRocket();
        }
    }

    // Function: Fire rocket
    void FireRocket()
    {
        // Instantiate rocket prefab, combining quaternion rotations of rocket launcher and rocket
        // Rigidbody rocketClone = (Rigidbody)Instantiate(rocket, transform.position, transform.rotation * rocket.transform.rotation);

        // Create the rocket from ObjectPooler
        rocketClone = ObjectPooler.SharedInstance.GetPooledObject("Rocket").GetComponent<Rocket>();
        // If rocket is null, there is no rocket to fire
        if (rocketClone == null) return;

        rocketClone.transform.position = transform.position;
        // rocketClone.transform.rotation = Quaternion.FromToRotation(rocketClone.transform.forward, transform.forward);
        rocketClone.transform.rotation = transform.rotation;
        rocketClone.gameObject.SetActive(true);

        // Fire rocketClone
        rocketClone.Fire();

        /*
        // Check array rocketArr length
        rocketArr = GameObject.FindGameObjectsWithTag("Rocket");
        if (rocketArr.Length > maxRockets) {
            // Remove rocket from beginning of array rocketArr and destroy rocket instance
            Destroy(rocketArr[0]);
        }
        */
    }
}
