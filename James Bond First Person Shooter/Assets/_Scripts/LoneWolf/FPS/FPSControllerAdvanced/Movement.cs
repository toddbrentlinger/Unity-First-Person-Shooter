using UnityEngine;

namespace LoneWolf.FPS
{
    /* NOTES:
     * Could instead use m_fpsController.Speed to return m_currSpeed from FPSAdvancedController script.
     * Could do the same with m_grounded, with m_fpsController.Grounded, and m_moveState, with m_fpsController.MoveState.
     * ***Or set all three variables as private members of this Movement class, accessible from outside scripts with properties.
     */
    // Enumeration to choose between different states of movement
    public enum MoveState { Idle, Walking, Running, Jumping, Falling, Error };

    [System.Serializable]
    public class Movement
    {
        [Header("Walking")]
        [SerializeField] private float m_walkingSpeed = 5f;
        [SerializeField] private float m_stepInterval = 5f; // for step audio

        [Header("Running")]
        [SerializeField] private float m_runningSpeed = 8f;
        [SerializeField] [Range(0f, 1f)] private float m_runStepLengthen = .7f; // for step audio and weapon bob

        [Header("Jumping")]
        [SerializeField] private float m_jumpVerticalSpeed = 7f;
        [SerializeField] private float m_jumpHorizontalSpeed = 2f;
        //Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
        [SerializeField] private int m_framesGroundedBetweenJumps = 1;
        private int m_jumpFrameCounter;

        [Header("Falling")]
        [SerializeField] private float m_fallingDamageThreshold = 10f;
        private float m_fallStartLevel = 0f;
        // private bool m_falling = false;

        [Header("Physics")]
        [SerializeField] private float m_gravity = 20f;

        [Header("General")]
        [SerializeField] [Range(0f, 1f)] private float m_moveBackwardFactor = .8f;
        [SerializeField] [Range(0f, 1f)] private float m_moveSideFactor = .8f;
        [SerializeField] private float m_antiBumpFactor = .75f;
        private FPSControllerAdvanced m_fpsController; // set in Init()
        private Transform m_playerTransform; // set in Init()
        private CharacterController m_characterController; // set in Init()
        private bool m_grounded = true; // use in controller
        private float m_currSpeed = 0f; // use in controller
        private Vector2 m_input = Vector2.zero;
        private Vector3 m_moveDirection = Vector3.zero;
        private Vector3 m_moveVelocity = Vector3.zero;
        private CollisionFlags m_collisionFlags; // use in controller?
        private RaycastHit m_hitInfo;
        private MoveState m_moveState = MoveState.Idle; // show in stats in OnGUI() method

        // ---------- Properties ----------

        /* NOTE:
         * Use the setter to change other variables as I would in the Update method
         */
        public float Speed
        {
            get { return m_currSpeed; }
            private set { m_currSpeed = Mathf.Clamp(value, 0, m_runningSpeed); }
        }

        public MoveState CurrentMoveState
        {
            get { return m_moveState; }
        }

        // For Awake() in FPSController
        public void Init(FPSControllerAdvanced fpsController, Transform playerTransform, CharacterController characterController)
        {
            m_fpsController = fpsController;
            m_playerTransform = playerTransform;
            m_characterController = characterController;
        }

        // For Start() in FPSController
        // NOTE: Could instead use m_fpsController.Speed to return m_currSpeed from FPSAdvancedController script
        public void SetupSpeed()
        {
            m_currSpeed = m_walkingSpeed;
            m_jumpFrameCounter = m_framesGroundedBetweenJumps;
        }

        public void UpdateMovement()
        {
            // If Player is grounded
            if (m_grounded)
            {
                // Check if Player was falling, and fell a vertical distance greater than the threshold, run a falling damage routine
                if (m_moveState == MoveState.Falling)
                {
                    if (m_playerTransform.position.y < m_fallStartLevel - m_fallingDamageThreshold)
                        FallingDamageAlert(m_fallStartLevel - m_playerTransform.position.y);
                }

                // Set Player speed
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (m_currSpeed != m_runningSpeed) // if (m_moveState != MoveState.Running)
                        m_currSpeed = m_runningSpeed;
                }
                else
                {
                    if (m_currSpeed != m_walkingSpeed) // if (m_moveState != MoveState.Walking)
                        m_currSpeed = m_walkingSpeed;
                }
            }
            // Else Player is NOT grounded
            else
            {
                // If NOT already falling, set state to falling and set fall damage start level
                if (m_moveState != MoveState.Falling)
                {
                    m_moveState = MoveState.Falling;
                    m_fallStartLevel = m_playerTransform.position.y;
                }
            }

            // Move CharacterController based on Horiz/Vert input and speed (checks for jumping)
            MovePosition();

            // Update move state
            UpdateMoveState();
        }

        // Move CharacterController depending on Horizontal/Vertical input and speed
        private void MovePosition()
        {
            // Set and adjust input Vector2
            m_input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (m_input.y < 0)
                m_input.y *= m_moveBackwardFactor;
            if (m_input.x != 0)
                m_input.x *= m_moveSideFactor;
            if (m_input.sqrMagnitude > 1)
                m_input.Normalize();

            // If Player is grounded
            if (m_grounded)
            {
                // Set desired move Vector3
                m_moveDirection = m_playerTransform.forward * m_input.y + m_playerTransform.right * m_input.x;

                // Use SphereCast to get normal for the surface that is being touched to move along it
                Physics.SphereCast(m_playerTransform.position + m_characterController.center, m_characterController.radius, Vector3.down, out m_hitInfo,
                    m_characterController.height * .5f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

                // Get normalized Vector3 projected on plane of surface Player is on top of
                m_moveDirection = Vector3.ProjectOnPlane(m_moveDirection, m_hitInfo.normal).normalized;

                // Set downward force to stick to ground when walking on slopes
                m_moveDirection.y = -m_antiBumpFactor;

                // Set move velocity Vector3 by multiplying speed to move direction Vector3
                m_moveVelocity = m_moveDirection * m_currSpeed;

                // Check jumping
                if (!Input.GetButton("Jump"))
                    m_jumpFrameCounter++;
                else if (m_jumpFrameCounter >= m_framesGroundedBetweenJumps)
                {
                    m_moveVelocity.y = m_jumpVerticalSpeed;
                    m_moveVelocity += m_moveDirection * m_jumpHorizontalSpeed;
                    m_jumpFrameCounter = 0;
                }
            }
            // Else Player is NOT grounded
            // NOTE: Use speed at point player left the ground instead of m_currSpeed which is set depending on MoveState
            // This allows other factors that affect speed while grounded to carry over while in air.
            // Use CharacterController.velocity.x,y,z so the speed can also be affected while falling.
            // Add to this speed if using airControl or airAssist
            else
            {
                m_moveVelocity.x = m_input.x * m_currSpeed;
                m_moveVelocity.z = m_input.y * m_currSpeed;
                m_moveVelocity = m_playerTransform.TransformDirection(m_moveVelocity);
            }

            // Apply gravity
            m_moveVelocity.y -= m_gravity * Time.deltaTime;

            // Move CharacterController and set grounded depending on whether Player is standing on collider
            m_collisionFlags = m_characterController.Move(m_moveVelocity * Time.deltaTime);
            m_grounded = (m_collisionFlags & CollisionFlags.Below) != 0;
        }

        // ---------- Falling ----------

        // If falling damage occured, this is the place to do something about it. You can make the player
        // have hitpoints and remove some of them based on the distance fallen, add sound effects, etc.
        private void FallingDamageAlert(float fallDistance)
        {
            m_fpsController.FallingDamageAlert(fallDistance);
        }

        // ---------- Move State ----------

        private void UpdateMoveState()
        {
            // Check if NOT grounded
            if (!m_grounded)
            {
                if (m_characterController.velocity.y < 0)
                    m_moveState = MoveState.Falling;
                else
                    m_moveState = MoveState.Jumping;
                return;
            }

            // Check if not moving (no input is entered)
            if (m_input.sqrMagnitude == 0)
            {
                m_moveState = MoveState.Idle;
                return;
            }

            // Check moving speed
            if (m_currSpeed == m_runningSpeed)
                m_moveState = MoveState.Running;
            else if (m_currSpeed == m_walkingSpeed)
                m_moveState = MoveState.Walking;
            else
                m_moveState = MoveState.Error; // if all cases are false, default to Error state
        }
    }
}
