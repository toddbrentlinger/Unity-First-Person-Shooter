using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour {

    public float movementSpeed = 8.0f;
    public float mouseSensitivity = 3.0f;
    public float verticalAngleLimit = 60.0f;
    public float jumpSpeed = 20.0f;

    private float verticalRotation = 0;
    private float verticalVelocity = 0;
    private CharacterController characterController;

    private Transform mainCam;

	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
        mainCam = Camera.main.transform;
	}
	
	// Update is called once per frame
	void Update () {

        // Rotation
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, horizontalRotation, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalAngleLimit, verticalAngleLimit);
        mainCam.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        // Movement
        float forwardSpeed = Input.GetAxis("Vertical") * movementSpeed;
        float sideSpeed = Input.GetAxis("Horizontal") * movementSpeed;

        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        if (characterController.isGrounded && Input.GetButton("Jump"))
        {
            verticalVelocity = jumpSpeed;
        }

        Vector3 speed = new Vector3(sideSpeed, verticalVelocity, forwardSpeed);

        speed = transform.rotation * speed;

        characterController.Move(speed * Time.deltaTime);

	}
}
