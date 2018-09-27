using UnityEngine;

/* NOTES:
 * - Jumping state when presses Jump button and travelling upwards. Falling state begins as soon
 * as player starts travelling downward.
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
 * X If player is crouching and presses sprint button, player stands up before/while increasing speed 
 * - Add ladder climbing functionality. 
 * - PROBLEM: walking up slope should reduce forward speed and walking down should increase but it does NOT.
 * It reduces the forward speed regardless of direction.
 * - Check if each private variable is referenced in multiple functions. Change to local if NOT or perhaps
 * pass the value by reference through a parameter in the function (perhaps when only used in two functions)
 * - CharacterController.Move in FixedUpdate()? Will it fix when player gets stuck on dynamic object edges?
 * - Stop CheckCrouch from calling Crouch or JumpCrouch on every frame unless needed.
 * Return if CharacterController.height is at standing or crouch height
 * 
 */

// Enumeration of Movement to choose between state of movement
public enum MoveState { Idle, Walking, Running, Jumping, Falling, Crouching, Leaning, Sliding };

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FPSController : MonoBehaviour {
    /*
    [System.Serializable]
    public class Movement
    {
        public float moveSpeed;
        [Range(0f, 1f)] float moveBackwardFactor ;
        [Range(0f, 1f)] float moveSideFactor;

        public Movement(float speed = 5f, float backwardFactor = .8f, float sideFactor = .8f)
        {
            moveSpeed = speed;
            moveBackwardFactor = backwardFactor;
            moveSideFactor = sideFactor;
        }
    }
    */
    [Header("General")]
    [SerializeField] private MoveState m_moveState = MoveState.Idle;
    [SerializeField] private bool m_showStatsGUI = false;
    public bool playerControl = true;

    [Header("Movement")]
    [SerializeField] [Range(0f, 1f)] private float m_moveBackwardFactor = .8f;
    [SerializeField] [Range(0f, 1f)] private float m_moveSideFactor = .8f;
    [SerializeField] private float m_antiBumpFactor = .75f;
    [SerializeField] private bool m_airControl = true;
    [SerializeField] private bool m_airAssist = false;
    [SerializeField] [Range(0f,1f)] private float m_airSpeed = 2f;
    private CharacterController m_characterController;
    private bool m_grounded = true;
    private float m_speed;
    private Vector2 m_input = Vector2.zero;
    private Vector3 m_moveDirection = Vector3.zero;
    private Vector3 m_moveVelocity = Vector3.zero;
    private float m_stepCycle; // for step audio
    private float m_nextStep; // for step audio
    private CollisionFlags m_collisionFlags;

    [Header("Walking")]
    [SerializeField] private float m_walkSpeed = 5f;
    [SerializeField] private float m_stepInterval = 5f; // for step audio

    [Header("Running")]
    [SerializeField] private float m_runSpeed = 8f;
    [SerializeField] [Range(0f, 1f)] private float m_runstepLenghten = .7f; // for step audio and weaponBob

    [Header("Jumping")]
    [SerializeField] private float m_jumpVerticalSpeed = 7f;
    [SerializeField] private float m_jumpHorizontalSpeed = 2f;
    //Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
    [SerializeField] private float m_framesGroundedBetweenJumps = 1;
    private float m_jumpFrameCounter;

    [Header("Falling")]
    [SerializeField] private float m_fallingDamageThreshold = 10f;
    private float m_fallStartLevel;
    private bool m_falling = false;

    [Header("Crouching")]
    [SerializeField] private float m_crouchHeight = 1f;
    [SerializeField] private float m_crouchDropSpeed = 2f;
    [SerializeField] private float m_crouchSpeed = 3f;
    [SerializeField] [Range(0f, 2f)] private float m_crouchStepReduction = 1f; // for step audio and weaponBob
    [SerializeField] bool m_startCrouched = false;
    private bool m_crouching;
    private float m_originalCameraLocalHeight;
    private float m_originalCharacterControllerHeight;
    private float m_originalCharacterControllerCenterY;
    private float m_originalCharacterControllerPositionY;

    [Header("Leaning")]

    [Header("Sliding")]

    [Header("Camera")]
    [SerializeField] private FPSMouseLook m_mouseLook;
    private Camera m_fpsCamera;

    [Header("Physics")]
    [SerializeField] private float m_gravity = 20f;
    [SerializeField] private float m_pushPower = .1f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] m_footstepSounds; // array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_jumpSound; // sound played when character leaves the ground.
    [SerializeField] private AudioClip m_landSound; // sound played when character touches back on ground.
    private AudioSource m_audioSource;

    // ---------- Properties ---------- 

    public float Speed
    {
        get { return m_speed; }
        set { m_speed = Mathf.Clamp(value, 0, m_runSpeed); }
    }
    public MoveState CurrentMoveState
    {
        get { return m_moveState; }
    }

    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_fpsCamera = Camera.main; // GetComponentInChildren<Camera>() but there is another camera that renders only weapons
        m_audioSource = GetComponent<AudioSource>();
    }

	private void Start ()
    {
        m_mouseLook.Init(transform, m_fpsCamera.transform);
        m_speed = m_walkSpeed;
        m_jumpFrameCounter = m_framesGroundedBetweenJumps;
        m_stepCycle = m_nextStep = 0f;

        // Crouching
        m_originalCameraLocalHeight = m_fpsCamera.transform.localPosition.y;
        m_originalCharacterControllerHeight = m_characterController.height;
        m_originalCharacterControllerCenterY = m_characterController.center.y;

        m_crouching = m_startCrouched ? true : false;
        CheckCrouch();
    }
	
	private void Update ()
    {
        if (!playerControl)
            return;

        // Rotate fpsCamera and CharacterController depending on MouseX/MouseY input
        m_mouseLook.LookRotation();

        if (m_grounded)
        {
            // Check if player was falling, and fell a vertical distance greater than the threshold, run a falling damage routine
            if (m_falling)
            {
                PlayLandingSound();
                m_falling = false;
                if (transform.position.y < m_fallStartLevel - m_fallingDamageThreshold)
                    FallingDamageAlert(m_fallStartLevel - transform.position.y);
            }

            // Set speed (either walking, running, crouching)
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
        }
        // Else player is NOT grounded
        else
        {
            if (!m_falling)
            {
                m_falling = true;
                m_fallStartLevel = transform.position.y;
            }

            // Check crouch while falling/jumping (for crouch-jumping but should I allow when falling? YES)
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
            {
                m_originalCharacterControllerPositionY = m_characterController.transform.position.y;
                m_crouching = !m_crouching;
            }
        }

        // Move CharacterController depending on Horizontal/Vertical input and speed
        // Checks for jumping
        MovePosition();

        // Check crouch
        CheckCrouch();

        // Update step cycle for step audio
        ProgressStepCycle(m_speed);

        // Update moveState
        UpdateMoveState();
    }

    // Move CharacterController depending on Horizontal/Vertical input and speed
    private void MovePosition()
    {
        // Get input Vector2
        m_input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (m_input.y < 0)
            m_input.y *= m_moveBackwardFactor;
        if (m_input.x != 0)
            m_input.x *= m_moveSideFactor;
        if (m_input.sqrMagnitude > 1)
            m_input.Normalize();

        if (m_grounded)
        {
            // Set desired move Vector3
            m_moveDirection = transform.forward * m_input.y + transform.right * m_input.x;

            // Get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position + m_characterController.center, m_characterController.radius, Vector3.down, out hitInfo,
                               m_characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            // Get normalized Vector3 projected on plane of surface being touched
            // NOTE: Should moveDirection be normalized before multiplying speed
            m_moveDirection = Vector3.ProjectOnPlane(m_moveDirection, hitInfo.normal).normalized;

            // Set downward force to stick to ground when walking on slopes
            // NOTE: Shouldn't gravity be enough to counter this? Maybe not. Basically magnifying gravity
            // when grounded vs in air.
            m_moveDirection.y = -m_antiBumpFactor;

            // Get move velocity Vector3 by multiplying speed to moveDirection Vector3
            m_moveVelocity = m_moveDirection * m_speed;

            // Check jumping
            if (!Input.GetButton("Jump"))
                m_jumpFrameCounter++;
            else if (m_jumpFrameCounter >= m_framesGroundedBetweenJumps)
            {
                m_moveVelocity.y = m_jumpVerticalSpeed;
                m_moveVelocity += m_moveDirection * m_jumpHorizontalSpeed;
                m_jumpFrameCounter = 0;

                PlayJumpingSound();

                if (m_crouching)
                    m_crouching = false;
            }
        }
        // Else player is NOT grounded
        // NOTE: Use speed at point player left the ground instead of m_currSpeed which is set depending on MoveState
        // This allows other factors that affect speed while grounded to carry over while in air.
        // Use CharacterController.velocity.x,y,z so the speed can also be affected while falling.
        // Add to this speed if using airControl or airAssist
        else
        {
            // If airControl is allowed (DON'T change y-velocity; let gravity accumulate)
            if (m_airControl)
            {   
                m_moveVelocity.x = m_input.x * m_speed;
                m_moveVelocity.z = m_input.y * m_speed;
                m_moveVelocity = transform.TransformDirection(m_moveVelocity);
            }
            else if (m_airAssist)
            {
                // NOTE: Should I use SimpleMove instead of Move? SimpleMove takes velocity argument and automatically applies gravity.
                m_moveVelocity.x = m_input.x * m_airSpeed;
                m_moveVelocity.z = m_input.y * m_airSpeed;
                m_moveVelocity = m_characterController.velocity + transform.TransformDirection(m_moveVelocity);
            }
        }

        // Apply gravity
        m_moveVelocity.y -= m_gravity * Time.deltaTime; // acceleration x time = velocity

        // Move character controller, and set grounded depending on whether player is standing on collider
        m_collisionFlags = m_characterController.Move(m_moveVelocity * Time.deltaTime);
        m_grounded = (m_collisionFlags & CollisionFlags.Below) != 0;
    }

    // ---------- Crouching ---------- 

    private enum CrouchState { Down, Up, Jump };
    private CrouchState m_crouchState;

    private void CheckCrouch()
    {
        if (m_crouching)
        {
            if (!m_grounded)
                m_crouchState = CrouchState.Jump;
            else
                m_crouchState = CrouchState.Down;
        }
        else
            m_crouchState = CrouchState.Up;

        SetCrouch();
    }

    private void SetCrouch()
    {
        switch(m_crouchState)
        {
            case (CrouchState.Down):
                CrouchDown();
                break;
            case (CrouchState.Up):
                CrouchUp();
                break;
            case (CrouchState.Jump):
                CrouchJump();
                break;
        }
    }

    private void CrouchDown()
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
        if (m_fpsCamera.transform.localPosition.y != m_crouchHeight)
            m_fpsCamera.transform.localPosition = new Vector3(m_fpsCamera.transform.localPosition.x,
                Mathf.MoveTowards(m_fpsCamera.transform.localPosition.y, m_crouchHeight, Time.deltaTime * m_crouchSpeed),
                m_fpsCamera.transform.localPosition.z);
    }

    private void CrouchUp()
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
        if (m_fpsCamera.transform.localPosition.y != m_originalCameraLocalHeight)
            m_fpsCamera.transform.localPosition = new Vector3(m_fpsCamera.transform.localPosition.x,
                Mathf.MoveTowards(m_fpsCamera.transform.localPosition.y, m_originalCameraLocalHeight, Time.deltaTime * m_crouchSpeed),
                m_fpsCamera.transform.localPosition.z);
    }

    private void CrouchJump()
    {
        // Set character controller height
        if (m_characterController.height != m_crouchHeight)
        {
            float newHeight = Mathf.MoveTowards(m_characterController.height, m_crouchHeight, Time.deltaTime * m_crouchSpeed);
            // Delta height (difference in height changed from MoveTowards)
            float heightDelta = newHeight - m_characterController.height;
            // Set characterController newHeight
            m_characterController.height = newHeight;

            // Set character controller position (rises as much as characterController height falls)
            m_characterController.transform.position = new Vector3(m_characterController.transform.position.x,
                m_characterController.transform.position.y - heightDelta,
                m_characterController.transform.position.z);
        }

        // Set character controller center
        float characterControllerHeightDelta = m_originalCharacterControllerHeight - m_characterController.height;
        if (m_characterController.center.y != m_originalCharacterControllerCenterY - m_crouchHeight * .5f)
        {
            m_characterController.center = new Vector3(m_characterController.center.x,
                m_originalCharacterControllerCenterY - characterControllerHeightDelta * .5f,
                m_characterController.center.z);
        }

        // Set camera height
        if (m_fpsCamera.transform.localPosition.y != m_crouchHeight)
            m_fpsCamera.transform.localPosition = new Vector3(m_fpsCamera.transform.localPosition.x,
                Mathf.MoveTowards(m_fpsCamera.transform.localPosition.y, m_crouchHeight, Time.deltaTime * m_crouchSpeed),
                m_fpsCamera.transform.localPosition.z);
    }
    
    // ---------- Falling ---------- 

    // If falling damage occured, this is the place to do something about it. You can make the player
    // have hitpoints and remove some of them based on the distance fallen, add sound effects, etc.
    private void FallingDamageAlert(float fallDistance)
    {
        print("Ouch! Fell " + fallDistance + " units!");
    }

    // ---------- Audio ---------- 

    private void PlayJumpingSound()
    {
        m_audioSource.clip = m_jumpSound;
        m_audioSource.Play();
    }

    private void PlayLandingSound()
    {
        m_audioSource.clip = m_landSound;
        m_audioSource.Play();
        m_nextStep = m_stepCycle + .5f;
    }

    private void ProgressStepCycle(float speed)
    {
        if (m_characterController.velocity.sqrMagnitude > 0 && (m_input.x != 0 || m_input.y != 0))
        {
            float stepLengthChange;
            if (m_moveState == MoveState.Running)
                stepLengthChange = m_runstepLenghten;
            else if (m_moveState == MoveState.Crouching)
                stepLengthChange = m_crouchStepReduction;
            else
                stepLengthChange = 1f;

            m_stepCycle += (m_characterController.velocity.magnitude + (speed * stepLengthChange)) * Time.deltaTime;
        }

        if (!(m_stepCycle > m_nextStep))
            return;

        m_nextStep = m_stepCycle + m_stepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!m_characterController.isGrounded)
            return;

        // Pick & play a random footstep sound from the array, excluding sound at index 0
        int n = Random.Range(1, m_footstepSounds.Length);
        m_audioSource.clip = m_footstepSounds[n];
        m_audioSource.PlayOneShot(m_audioSource.clip);
        // Move picked sound to index 0 so it's not picked next time
        m_footstepSounds[n] = m_footstepSounds[0];
        m_footstepSounds[0] = m_audioSource.clip;
    }

    //  ---------- Move State ---------- 

    private void UpdateMoveState()
    {
        if (!m_grounded)
        {
            if (m_characterController.velocity.y < 0)
                m_moveState = MoveState.Falling;
            else
                m_moveState = MoveState.Jumping;
            return;
        }

        if (m_crouching)
        {
            m_moveState = MoveState.Crouching;
            return;
        }

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //Vector2 rotateInput = new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));

        if (moveInput.sqrMagnitude == 0)
        {
            m_moveState = MoveState.Idle;
            return;
        }

        if (m_speed == m_runSpeed)
            m_moveState = MoveState.Running;
        else if (m_speed == m_walkSpeed)
            m_moveState = MoveState.Walking;
    }

    // ---------- OnControllerColliderHit ----------

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        /*
        if (hit.moveDirection.y >= -.3f)
            Debug.Log("MoveDirectionY: " + hit.moveDirection.y);
        */
        //MoveObject01(hit);
        MoveObject02(hit);
        //MoveObject03(hit);
    }

    private void MoveObject01(ControllerColliderHit hit)
    {
        //Don't move the rigidbody if the character is on top of it
        if (m_collisionFlags == CollisionFlags.Below)
            return;

        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
            return;

        body.AddForceAtPosition(m_characterController.velocity * m_pushPower, hit.point, ForceMode.Impulse);
    }

    private void MoveObject02(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
            return;
        
        Vector3 force;
        float m_weight = 10f;
        if (hit.moveDirection.y < -.3f)
            force = new Vector3(0, -.5f, 0) * m_gravity * m_weight;
        else
            force = m_characterController.velocity * m_pushPower;

        body.AddForceAtPosition(force, hit.point);
    }

    // NOTE: Better to use AddForce instead of changing Rigidbody.velocity directly
    private void MoveObject03(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic || hit.moveDirection.y < -.3f)
            return;

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * m_pushPower * m_speed;
    }

    // ---------- OnGUI ---------- 

    private void OnGUI()
    {
        if (!m_showStatsGUI)
            return;

        float xInput = Input.GetAxis("Mouse X");
        float yInput = Input.GetAxis("Mouse Y");

        float yRot = xInput * m_mouseLook.xSensitivity;
        float xRot = yInput * m_mouseLook.ySensitivity;

        string text =
            "MoveState: " + m_moveState +
            "\nGrounded: " + m_grounded +
            "\nSpeed: " + m_speed +
            "\nCCSpeed: " + m_characterController.velocity.magnitude.ToString("F2") +
            "\nInput-Move: " + m_input +
            "\nDesiredMove: " + transform.InverseTransformDirection(m_moveDirection) + 
            "\nMoveVelocity: " + transform.InverseTransformDirection(m_moveVelocity) + 
            "\nCameraInput: " + new Vector2(xInput, yInput) + 
            "\nCameraRotation: " + new Vector2(yRot, xRot).ToString("F1");

        //GUI.contentColor = Color.white;
        GUIStyle myGUIStyle = new GUIStyle();
        myGUIStyle.normal.textColor = Color.white;
        myGUIStyle.clipping = TextClipping.Clip;

        GUI.Label(new Rect(5f, Screen.height * .06f, Screen.width * .3f, Screen.height * .5f), 
            text, myGUIStyle);
    }
}
