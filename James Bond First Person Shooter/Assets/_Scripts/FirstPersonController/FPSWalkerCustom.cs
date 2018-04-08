using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* NOTES:
 * - Use transform.TransformDirection instead of adding vector3.forward and vector3.right. 
 * Both FPSWalker and Unity3D reference page for CharacterController.Move
 * - Add airControlReductionFactor to minimize control while NOT grounded
 * - The slope sliding is quite basic: the player is either sliding or not, and has 
 * no lateral control when sliding. One issue that may surface is that, under some 
 * circumstances, attempting to force oneself up a slippery slope will result in 
 * annoying jittering, as the character controller moves forward a bit one frame 
 * only to slide back the next frame, then moves forward a bit again, etc.
 * - Allow crouch while jumping, and falling?, to allow crouch jumping. Can I make certain jumps by crouch jumping?
 * - Try using SmoothDamp for WASD movement. Simulate slow to start and slow to stop movement but still keeping responsive mouse movement
 */

[RequireComponent(typeof(CharacterController))]
public class FPSWalkerCustom : MonoBehaviour {

    [Header("Movement")]
    public bool playerControl = true;
    [SerializeField] private bool m_airControl = false;
    [SerializeField] private float m_antiBumpFactor = .75f;
    private CharacterController m_characterController;
    //private Vector2 m_input;
    //private Vector3 m_moveVelocity = Vector3.zero;
    //private bool m_previouslyGrounded = true;
    private float m_speed;
    private Vector3 m_moveDirection = Vector3.zero;
    //private bool m_slidingDownSurface;
    private bool m_grounded = true;

    [Header("Walking")]
    [SerializeField] private float m_walkSpeed = 5f;
    [SerializeField] private float m_stepInterval = 5f;
    private float m_stepCycle;
    private float m_nextStep;

    [Header("Running")]
    [SerializeField] private float m_runSpeed = 10f;
    [SerializeField] [Range(0f, 1f)] private float m_runstepLenghten = .7f;

    [Header("Jumping")]
    [SerializeField] private float m_jumpSpeed = 8f;
    //Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
    [SerializeField] private float m_framesGroundedBetweenJumps = 1;
    private float m_jumpFrameCounter;

    [Header("Falling")]
    //Units that player can fall before a falling damage function is run. To disable, type "infinity" in the inspector.
    //[SerializeField] private bool m_takeFallingDamage = true;
    [SerializeField] private float m_fallingDamageThreshold = 10f;
    private float m_fallStartLevel;
    private bool m_falling;

    [Header("Crouching")]
    [SerializeField] private float m_crouchHeight = .65f;
    [SerializeField] private float m_crouchDropSpeed = 2f;
    [SerializeField] private float m_crouchSpeed = 3f;
    [SerializeField] [Range(0f, 1f)] private float m_crouchStepReduction = .8f;
    [SerializeField] bool m_startCrouched = false;
    private bool m_crouching;
    private float m_originalCameraLocalHeight;
    private float m_originalCharacterControllerHeight;
    private float m_originalCharacterControllerCenterY;

    [Header("Camera")]
    [SerializeField] private MouseLookCustom m_mouseLook;
    private Camera m_camera;

    [Header("Physics")]
    [SerializeField] private float m_gravity = 20f; // for CharacterController
    [SerializeField] private float m_pushPower = .1f;
    //[SerializeField] private float m_stickToGroundGravityMultiplier = 2f;
    //private CollisionFlags m_collisionFlags;

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
    private void Start () {
        m_mouseLook.Init(transform, m_camera.transform);
        m_speed = m_walkSpeed;
        m_jumpFrameCounter = m_framesGroundedBetweenJumps;
        m_stepCycle = 0f;
        m_nextStep = m_stepInterval;

        // Crouching
        m_originalCameraLocalHeight = m_camera.transform.localPosition.y;
        m_originalCharacterControllerHeight = m_characterController.height;
        m_originalCharacterControllerCenterY = m_characterController.center.y;

        m_crouching = m_startCrouched ? true : false;
        CheckCrouch();
    }
	
	// Update is called once per frame
	private void Update ()
    {
        // Change camera and player rotation
        m_mouseLook.LookRotation();

        // Get input
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        // If both horizontal and vertical are used simultaneously, limit speed, so the total doesn't exceed normal move speed
        float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f) ? .7071f : 1.0f;

        Vector3 debugTemp = Vector3.zero, debugProject = Vector3.zero;
        // If player is grounded
        if (m_grounded)
        {
            // If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
            if (m_falling)
            {
                m_falling = false;
                if (transform.position.y < m_fallStartLevel - m_fallingDamageThreshold)
                    FallingDamageAlert(m_fallStartLevel - transform.position.y);
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (m_crouching)
                    m_crouching = false;

                m_speed = m_runSpeed;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
                    m_crouching = !m_crouching;

                m_speed = m_crouching ? m_crouchSpeed : m_walkSpeed;
            }

            //CheckCrouch();
            /*
            // Check for crouching
            if (Input.GetKeyDown(KeyCode.C))
                m_crouching = !m_crouching;
            CheckCrouch();

            // Set speed
            m_speed = Input.GetKey(KeyCode.LeftShift) ? m_runSpeed : m_walkSpeed;
            */
            m_moveDirection = (transform.forward * inputY + transform.right * inputX);
            debugTemp = m_moveDirection;

            // Get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position + m_characterController.center, m_characterController.radius, Vector3.down, out hitInfo,
                               m_characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            m_moveDirection = Vector3.ProjectOnPlane(m_moveDirection, hitInfo.normal).normalized;
            debugProject = m_moveDirection;

            m_moveDirection *= inputModifyFactor;
            m_moveDirection.y = -m_antiBumpFactor;
            m_moveDirection *= m_speed;

            // Recalculate moveDirection directly from axes, adding a bit of -y to avoid bumping down inclines
            // m_moveDirection = new Vector3(inputX * inputModifyFactor, -m_antiBumpFactor, inputY * inputModifyFactor);
            // m_moveDirection = transform.TransformDirection(m_moveDirection) * m_speed;
            //playerControl = true;

            if (!Input.GetButton("Jump"))
                m_jumpFrameCounter++;
            else if (m_jumpFrameCounter >= m_framesGroundedBetweenJumps)
            {
                m_moveDirection.y = m_jumpSpeed;
                m_jumpFrameCounter = 0;

                if (m_crouching)
                {
                    m_crouching = false;
                    //CheckCrouch();
                }
            }
        }
        // Else player is NOT grounded
        else
        {
            if (!m_falling)
            {
                m_falling = true;
                m_fallStartLevel = transform.position.y;
            }

            // If air control is allowed, check movement but don't touch the y component
            if (m_airControl && playerControl)
            {
                m_moveDirection.x = inputX * m_speed * inputModifyFactor;
                m_moveDirection.z = inputY * m_speed * inputModifyFactor;
                m_moveDirection = transform.TransformDirection(m_moveDirection);
            }
        }

        // Apply gravity
        m_moveDirection.y -= m_gravity * Time.deltaTime;

        // Move character controller, and set grounded depending on whether player is standing on collider
        m_grounded = (m_characterController.Move(m_moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

        CheckCrouch();

        //Debug.Log("isGrounded:" + m_characterController.isGrounded + " - moveDirection:" + m_moveDirection + " - velocity:" + m_characterController.velocity + " - speed:"+m_characterController.velocity.magnitude+"("+m_speed+") - inputModifyFactor:"+inputModifyFactor + " - Time.deltaTime: " + Time.deltaTime);
        Debug.Log("Input: " + inputX + ", " + inputY + " - InputVector: " + m_camera.transform.InverseTransformDirection(debugTemp) + " - ProjectionVector: " + m_camera.transform.InverseTransformDirection(debugProject) + " - moveDirection:" + m_camera.transform.InverseTransformDirection(m_moveDirection) + " - velocity:" + m_camera.transform.InverseTransformDirection(m_characterController.velocity));

    }

    private void CheckCrouch()
    {
        if (m_crouching)
        {
            // Set character controller height
            if (m_characterController.height != m_crouchHeight)
                m_characterController.height = Mathf.MoveTowards(m_characterController.height, m_crouchHeight, Time.deltaTime * m_crouchSpeed);

            // Set character controller center
            if (m_characterController.center.y != m_originalCharacterControllerCenterY - m_crouchHeight * .5f)
                m_characterController.center = new Vector3(m_characterController.center.x,
                    m_originalCharacterControllerCenterY - (m_originalCharacterControllerHeight - m_characterController.height) * .5f,
                    m_characterController.center.z);

            // Set camera height
            if (m_camera.transform.localPosition.y != m_crouchHeight)
                m_camera.transform.localPosition = new Vector3(m_camera.transform.localPosition.x,
                    Mathf.MoveTowards(m_camera.transform.localPosition.y, m_crouchHeight, Time.deltaTime * m_crouchSpeed),
                    m_camera.transform.localPosition.z);
        }
        else
        {
            // Reset character controller height
            if (m_characterController.height != m_originalCharacterControllerHeight)
                m_characterController.height = Mathf.MoveTowards(m_characterController.height, m_originalCharacterControllerHeight, Time.deltaTime * m_crouchSpeed);

            // Reset character controller center
            if (m_characterController.center.y != m_originalCharacterControllerCenterY)
                m_characterController.center = new Vector3(m_characterController.center.x,
                    m_originalCharacterControllerCenterY - (m_originalCharacterControllerHeight - m_characterController.height) * .5f,
                    m_characterController.center.z);

            // Reset camera height
            if (m_camera.transform.localPosition.y != m_originalCameraLocalHeight)
                m_camera.transform.localPosition = new Vector3(m_camera.transform.localPosition.x,
                    Mathf.MoveTowards(m_camera.transform.localPosition.y, m_originalCameraLocalHeight, Time.deltaTime * m_crouchSpeed),
                    m_camera.transform.localPosition.z);
        }
    }

    // If falling damage occured, this is the place to do something about it. You can make the player
    // have hitpoints and remove some of them based on the distance fallen, add sound effects, etc.
    private void FallingDamageAlert(float fallDistance)
    {
        print("Ouch! Fell " + fallDistance + " units!");
    }
}
