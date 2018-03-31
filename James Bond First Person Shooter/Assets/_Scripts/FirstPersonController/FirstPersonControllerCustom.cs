using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* NOTES:
 * - 
 * - Should I put camera rotation in LateUpdate?
 */

[RequireComponent(typeof(CharacterController))]
public class FirstPersonControllerCustom : MonoBehaviour {

    // Enumeration of Movement to choose between state of movement
    private enum MoveState { Idling, Walking, Running, Jumping, Falling, Crouching, Leaning, Sliding };

    [Header("Movement")]
    [SerializeField] private MoveState m_moveState = MoveState.Walking;
    public bool m_playerControl = true;
    private CharacterController m_characterController;
    private Vector2 m_input;
    private Vector3 m_moveVelocity = Vector3.zero;
    private bool m_previouslyGrounded = true;

    [Header("Walking")]
    [SerializeField] private float m_walkSpeed = 6f;
    [SerializeField] private float m_stepInterval = 5f;
    private float m_stepCycle;
    private float m_nextStep;

    [Header("Running")]
    [SerializeField] private float m_runSpeed = 11f;
    [SerializeField] [Range(0f, 1f)] private float m_runstepLenghten = .7f;

    [Header("Jumping")]
    [SerializeField] private float m_jumpSpeed = 8f;
    // Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
    [SerializeField] private int antiBunnyHopFactor = 1;

    [Header("Falling")]
    // Units that player can fall before a falling damage function is run. To disable, type "infinity" in the inspector.
    [SerializeField] private bool m_takeFallingDamage = true;
    [SerializeField] private float m_fallingDamageThreshold = 10f;

    [Header("Crouching")]
    [SerializeField] private float m_crouchHeight = .65f;
    [SerializeField] private float m_crouchDropSpeed = 2f;
    [SerializeField] private float m_crouchSpeed = 3f;
    [SerializeField] [Range(0f, 1f)] private float m_crouchStepReduction = .8f;

    [Header("Camera")]
    [SerializeField] private MouseLookCustom m_mouseLook;
    private Camera m_camera;

    [Header("Physics")]
    [SerializeField] private float m_gravity = 20f;
    [SerializeField] private float m_pushPower = .1f;
    [SerializeField] private float m_stickToGroundGravityMultiplier = 2f;
    private CollisionFlags m_collisionFlags;

    [Header("Audio")]
    [SerializeField] private AudioClip[] m_footstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_jumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_landSound;           // the sound played when character touches back on ground.
    private AudioSource m_audioSource;

    // Use this to assign references
    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_camera = Camera.main; // GetComponentInChildren<Camera>() but there is another camera that renders only weapons
        m_audioSource = GetComponent<AudioSource>();
    }

    // Use this for initialization
    private void Start ()
    {
        m_mouseLook.Init(transform, m_camera.transform);
        m_stepCycle = 0f;
        m_nextStep = m_stepInterval;
    }
	
	private void Update ()
    {
        // Rotate camera in MouseLook script
        m_mouseLook.LookRotation();

        // Move character controller
        MovePosition();

        // Set previouslyGrounded
        // m_previouslyGrounded = m_characterController.isGrounded;

        // Debug.Log("isGrounded:" + m_characterController.isGrounded + " - moveVelocity:" + m_moveVelocity + " - velocity:" + m_characterController.velocity);
    }

    // Set speed in Update depending on currentState, then pass speed into MovePosition(float speed)
    private void MovePosition()
    {
        // Set movement speed depending on current moveState
        float speed;
        if (Input.GetKey(KeyCode.LeftShift) && m_characterController.isGrounded)
        {
            m_moveState = MoveState.Running;
            speed = m_runSpeed;
        }    
        else
        {
            m_moveState = MoveState.Walking;
            speed = m_walkSpeed;
        }

        // Set movement input Vector2
        m_input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // Normalize input if it exceeds 1 in combined length
        if (m_input.sqrMagnitude > 1)
            m_input.Normalize();

        // Always move the camera along the forward direction since it's the direction being used to aim
        Vector3 desiredMove = transform.forward * m_input.y + transform.right * m_input.x;

        // Get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position + m_characterController.center, m_characterController.radius, Vector3.down, out hitInfo,
                           m_characterController.height * .5f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
        Debug.DrawRay(hitInfo.point, hitInfo.normal * 2f);

        // Set desired move direction
        m_moveVelocity.x = desiredMove.x * speed;
        m_moveVelocity.z = desiredMove.z * speed;

        // Apply gravity. When isGrounded, add multiple of gravity to prevent stepping motion on slopes
        if (m_previouslyGrounded)
            m_moveVelocity.y = -(m_stickToGroundGravityMultiplier * m_gravity);
        else
            m_moveVelocity.y -= m_gravity * Time.deltaTime;

        // Move character controller and set previouslyGrounded depending on if Player is standin on something
        m_previouslyGrounded = (m_characterController.Move(m_moveVelocity * Time.deltaTime) & CollisionFlags.Below) != 0;
    }

    private void CheckInput()
    {
        // Check input and change currentState

        // Depending on currentState, set property values and run logic for state
        switch (m_moveState)
        {
            case MoveState.Idling:
                break;
            case MoveState.Walking:
                break;
            case MoveState.Running:
                break;
            case MoveState.Jumping:
                break;
            case MoveState.Falling:
                break;
            case MoveState.Crouching:
                break;
            case MoveState.Leaning:
                break;
            case MoveState.Sliding:
                break;
            default:
                break;
        }
    }
}
