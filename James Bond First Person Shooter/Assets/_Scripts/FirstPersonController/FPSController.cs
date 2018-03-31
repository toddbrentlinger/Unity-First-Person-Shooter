using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour {

    // Enumeration of Movement to choose between state of movement
    private enum MoveState { Walking, Running, Crouching, Falling };

    [Header("Movement")]
    private CharacterController m_characterController;
    private bool m_grounded = true;

    [Header("Camera")]
    [SerializeField] private MouseLookCustom m_mouseLook;
    private Camera m_camera;

    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_camera = Camera.main; // GetComponentInChildren<Camera>() but there is another camera that renders only weapons
    }

	// Use this for initialization
	void Start ()
    {
        m_mouseLook.Init(transform, m_camera.transform);
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Change camera and character controller rotation
        m_mouseLook.LookRotation();

        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        // If both horizontal and vertical are used simultaneously, limit speed, so the total doesn't exceed normal move speed
        float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f) ? .7071f : 1.0f;

        // If player is grounded
        if (m_grounded)
        {

        }
        // Else player is NOT grounded
        else
        {

        }
    }
}
