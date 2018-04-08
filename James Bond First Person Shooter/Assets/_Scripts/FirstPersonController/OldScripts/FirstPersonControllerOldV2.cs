using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Simplified version of StandardAssets FirstPersonController script.
 * Uses simplified MouseLook script
 * 
 * NOTES:
 * - If player is crouching and presses sprint button, player stands up before/while increasing speed 
 */

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonControllerOldV2 : MonoBehaviour
{

    // Enumeration of Movement to choose between state of movement
    // NOTE: Add Jumping MoveState
    private enum MoveState { Walking, Running, Crouching };
    [SerializeField]
    private MoveState m_moveState = MoveState.Walking; // moveState initialized to idle

    [Header("Movement")]
    // [SerializeField] private bool m_IsWalking;
    [SerializeField]
    private float m_WalkSpeed = 5f;
    [SerializeField]
    private float m_RunSpeed = 10f;
    [SerializeField]
    [Range(0f, 1f)]
    private float m_RunstepLenghten = .7f;
    [SerializeField]
    private float m_JumpSpeed = 8f;
    [SerializeField]
    private float m_pushPower = .1f;
    [SerializeField]
    private float m_StickToGroundForce = 10f;
    [SerializeField]
    private float m_GravityMultiplier = 2f;
    [SerializeField]
    private MouseLook m_MouseLook;
    [SerializeField]
    private float m_StepInterval = 5f;
    [SerializeField]
    private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField]
    private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField]
    private AudioClip m_LandSound;           // the sound played when character touches back on ground.

    private Camera m_Camera;
    private bool m_Jump;
    private Vector2 m_Input;
    private Vector3 m_MoveVelocity = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded = true;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;

    // Crouching
    [Header("Crouching")]
    [SerializeField]
    private float m_crouchHeight = .65f;
    [SerializeField]
    private float m_crouchSpeed = 2f;
    [SerializeField]
    private float m_crouchWalkingSpeed = 3f;

    // private bool m_isCrouching = false;
    private float m_originalCameraLocalHeight;
    private float m_originalCharacterControllerHeight;
    private float m_originalCharacterControllerCenterY;

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f; // NOTE: Why divide when result is 0? Assign to 0f?
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook.Init(transform, m_Camera.transform);

        // Crouching
        m_originalCameraLocalHeight = m_Camera.transform.localPosition.y;
        m_originalCharacterControllerHeight = m_CharacterController.height;
        m_originalCharacterControllerCenterY = m_CharacterController.center.y;

        // Check if Player starts crouched
        if (m_moveState == MoveState.Crouching)
        {
            // Set character controller height
            m_CharacterController.height = m_crouchHeight;

            m_CharacterController.center = new Vector3(m_CharacterController.center.x,
                m_CharacterController.center.y - (m_originalCharacterControllerHeight - m_crouchHeight) * .5f,
                m_CharacterController.center.z);

            // Set camera height
            m_Camera.transform.localPosition = new Vector3(m_Camera.transform.localPosition.x,
                m_crouchHeight,
                m_Camera.transform.localPosition.z);
        }
    }

    private void Update()
    {

        // Rotate camera
        m_MouseLook.LookRotation(transform, m_Camera.transform);

        // Move character controller
        MovePosition();

        // Crouching
        CheckCrouch();

        // If player lands from jumping
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            PlayLandingSound();
            m_MoveVelocity.y = 0f;
            m_Jumping = false;
        }

        // NOTE: Will this make player float if walks off edge or is it just a check in case character sinks through floor?
        // If player is NOT grounded AND NOT jumping AND previously grounded
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            Debug.Log("NOT grounded AND NOT jumping AND previously grounded");
            m_MoveVelocity.y = 0f;
        }

        // Set previouslyGrounded
        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }

    private void MovePosition()
    {
        // Set movement input Vector2
        m_Input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // Normalize input if it exceeds 1 in combined length
        if (m_Input.sqrMagnitude > 1)
            m_Input.Normalize();

        // If player is NOT crouching AND isGrounded AND pressing Sprint button, set moveState to Running
        //if (m_moveState != MoveState.Crouching && m_CharacterController.isGrounded && Input.GetKey(KeyCode.LeftShift))
        //    m_moveState = MoveState.Running;

        if (m_moveState != MoveState.Crouching)
        {
            if (Input.GetKey(KeyCode.LeftShift) && m_CharacterController.isGrounded)
            {
                m_moveState = MoveState.Running;
            }
            else
            {
                m_moveState = MoveState.Walking;
            }
        }

        // Set movement speed depending on current moveState
        float speed;
        switch (m_moveState)
        {
            case MoveState.Running:
                speed = m_RunSpeed;
                break;
            case MoveState.Crouching:
                speed = m_crouchSpeed;
                break;
            default:
                speed = m_WalkSpeed;
                break;
        }

        // Always move the camera along the forward direction since it's the direction being used to aim
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // Get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        // Set desired move direction
        m_MoveVelocity.x = desiredMove.x * speed;
        m_MoveVelocity.z = desiredMove.z * speed;

        // Jump state needs to be read in Update() to be sure it's NOT missed if jumping is affected in FixedUpdate() like the original FirstPersonController StandardAsset. My script does NOT handle it in FixedUpdate().
        if (!m_Jump)
            m_Jump = Input.GetButtonDown("Jump");

        if (m_CharacterController.isGrounded)
        {
            // NOTE: What is the purpose of this if the character controller isGrounded?
            m_MoveVelocity.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                // Set vertical, y-axis, velocity
                m_MoveVelocity.y = m_JumpSpeed;
                // Play jump sound
                m_AudioSource.clip = m_JumpSound;
                m_AudioSource.Play();
                // Set jump parameters
                m_Jump = false;
                m_Jumping = true;
            }
        }
        else
        {
            // Apply gravity to NOT isGrounded character controller
            m_MoveVelocity += Physics.gravity * m_GravityMultiplier * Time.deltaTime;
        }

        // Move character controller returning collision flags used in OnControllerColliderHit()
        m_CollisionFlags = m_CharacterController.Move(m_MoveVelocity * Time.deltaTime);

        ProgressStepCycle(speed);
    }

    private void CheckCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (m_moveState == MoveState.Crouching)
                m_moveState = MoveState.Walking;
            else
                m_moveState = MoveState.Crouching;

        }

        if (m_moveState == MoveState.Crouching)
        {
            // if (m_CharacterController.height == m_crouchHeight)
            //     return;

            // float previousCharacterControllerHeight = m_CharacterController.height;

            // Set character controller height
            m_CharacterController.height = Mathf.MoveTowards(m_CharacterController.height, m_crouchHeight, Time.deltaTime * m_crouchSpeed);

            m_CharacterController.center = new Vector3(m_CharacterController.center.x,
                m_originalCharacterControllerCenterY - (m_originalCharacterControllerHeight - m_CharacterController.height) * .5f,
                m_CharacterController.center.z);

            /*
            // Set character controller center
            m_CharacterController.center = new Vector3(m_CharacterController.center.x,
                Mathf.MoveTowards(m_CharacterController.center.y, (m_originalCharacterControllerHeight - m_crouchHeight)*.5f, Time.deltaTime * m_crouchSpeed * .5f),
                m_CharacterController.center.z);
            */
            // Set camera height
            m_Camera.transform.localPosition = new Vector3(m_Camera.transform.localPosition.x,
                Mathf.MoveTowards(m_Camera.transform.localPosition.y, m_crouchHeight, Time.deltaTime * m_crouchSpeed),
                m_Camera.transform.localPosition.z);

        }
        else
        {
            // if (m_CharacterController.height == m_originalCharacterControllerHeight)
            //     return;

            // float previousCharacterControllerHeight = m_CharacterController.height;

            // Reset character controller height
            m_CharacterController.height = Mathf.MoveTowards(m_CharacterController.height, m_originalCharacterControllerHeight, Time.deltaTime * m_crouchSpeed);

            m_CharacterController.center = new Vector3(m_CharacterController.center.x,
                m_originalCharacterControllerCenterY - (m_originalCharacterControllerHeight - m_CharacterController.height) * .5f,
                m_CharacterController.center.z);

            /*
            // Reset character controller center
            m_CharacterController.center = new Vector3(m_CharacterController.center.x,
                Mathf.MoveTowards(m_CharacterController.center.y, (m_originalCharacterControllerHeight - m_crouchHeight) * 2f, Time.deltaTime * m_crouchSpeed * .5f),
                m_CharacterController.center.z);
            */
            // Reset camera height
            m_Camera.transform.localPosition = new Vector3(m_Camera.transform.localPosition.x,
                Mathf.MoveTowards(m_Camera.transform.localPosition.y, m_originalCameraLocalHeight, Time.deltaTime * m_crouchSpeed),
                m_Camera.transform.localPosition.z);

        }
    }

    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_moveState == MoveState.Running ? m_RunstepLenghten : 1f))) *
                         Time.deltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
            return;

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
            return;

        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Do NOT move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
            return;

        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
            return;

        // Limit collision to y-direction CharacterCollider was moving when collision occured
        // if (hit.moveDirection.y < -.3f)
        // return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.AddForceAtPosition(pushDir * m_pushPower, hit.point, ForceMode.Impulse);
        // body.velocity = pushDir * m_pushPower;

        // body.AddForceAtPosition(m_CharacterController.velocity * m_pushPower, hit.point, ForceMode.Impulse);
    }
}
