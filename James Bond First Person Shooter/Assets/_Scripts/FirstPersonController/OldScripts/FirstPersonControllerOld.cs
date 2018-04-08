using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonControllerOld : MonoBehaviour {

    /* NOTES:
     * - Lean Left/Right - Q/E
     * - Crouch - C    
     */

    [SerializeField] private float m_movementSpeed = 8.0f;
    [SerializeField] private float m_mouseSensitivity = 3.0f;
    [SerializeField] private float m_verticalAngleLimit = 60.0f;
    [SerializeField] private float m_jumpSpeed = 20.0f;

    private float m_verticalRotation = 0;
    private float m_verticalVelocity = 0;
    private CharacterController m_characterController;

    private Transform m_camera;

	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        m_characterController = GetComponent<CharacterController>();
        m_camera = Camera.main.transform;
	}
	
	// Update is called once per frame
	void Update () {

        // Rotation
        float horizontalRotation = Input.GetAxis("Mouse X") * m_mouseSensitivity;
        transform.Rotate(0, horizontalRotation, 0);

        m_verticalRotation -= Input.GetAxis("Mouse Y") * m_mouseSensitivity;
        m_verticalRotation = Mathf.Clamp(m_verticalRotation, -m_verticalAngleLimit, m_verticalAngleLimit);
        m_camera.localRotation = Quaternion.Euler(m_verticalRotation, 0, 0);

        // Movement
        float forwardSpeed = Input.GetAxis("Vertical") * m_movementSpeed;
        float sideSpeed = Input.GetAxis("Horizontal") * m_movementSpeed;

        m_verticalVelocity += Physics.gravity.y * Time.deltaTime;

        if (m_characterController.isGrounded && Input.GetButton("Jump"))
        {
            m_verticalVelocity = m_jumpSpeed;
        }

        Vector3 speed = new Vector3(sideSpeed, m_verticalVelocity, forwardSpeed);

        speed = transform.rotation * speed;

        m_characterController.Move(speed * Time.deltaTime);

	}
}
