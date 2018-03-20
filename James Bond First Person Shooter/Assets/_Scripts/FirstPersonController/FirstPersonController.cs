using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonController : MonoBehaviour {

    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField]
    [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private MouseLook m_MouseLook;
    [SerializeField] private float m_StepInterval;
    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

    private Camera m_Camera;
    private bool m_Jump;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook.Init(transform, m_Camera.transform);
    }
	
	private void Update () {

        // Rotate camera
        m_MouseLook.LookRotation(transform, m_Camera.transform);

        // Jump state needs to be read in Update() to be sure it's NOT missed
        if (!m_Jump)
            m_Jump = Input.GetButtonDown("Jump");

        // If player lands from jumping
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }

        // NOTE: Will this make player float if walks off edge or is it just a check in case character sinks through floor?
        // If player is NOT grounded AND NOT jumping AND previously grounded
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;		
	}

    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Check if player is pressing button to run
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);

        // Set speed to walking or running
        float speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;

        // Set Input Vector2
        m_Input = new Vector2(horizontal, vertical);
        // Normalize input if it exceeds 1 in combined length
        if (m_Input.sqrMagnitude > 1)
            m_Input.Normalize();

        // Always move the camera along the forward direction since it's the direction being used to aim
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // Get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;
    }
}
