
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickWall : MonoBehaviour {

    public Transform brick;
    public int brickRows = 5;
    public int brickColumns = 5;

    // Use this for initialization
    void Start() {
        for (int y = 0; y < brickRows; y++) {
            for (int x = 0; x < brickColumns; x++) {
                // If even column
                if (y % 2 == 0) {
                    // Add brick as child to this gameObject
                    Instantiate(brick, new Vector3(transform.position.x + x, 0.5f * (transform.position.y + y), transform.position.z), Quaternion.identity, transform);
                } else {
                    // Add brick offset by half-width as child to this gameObject
                    Instantiate(brick, new Vector3(transform.position.x + 0.5f + x, 0.5f * (transform.position.y + y), transform.position.z), Quaternion.identity, transform);
                }
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

