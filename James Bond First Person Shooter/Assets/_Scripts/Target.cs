using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

    private Transform m_camera;

	private void Awake()
    {
        m_camera = Camera.main.transform;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_camera != null)
        {
            Vector3 target = m_camera.transform.position;
            //target.y = transform.position.y;
            transform.LookAt(target);
        }
	}
}
