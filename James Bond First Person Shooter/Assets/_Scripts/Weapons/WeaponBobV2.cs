using UnityEngine;

public class WeaponBobV2 : MonoBehaviour {

    private Transform m_weapon;
    [SerializeField] private bool m_useWeaponBob = true;
    [SerializeField] private CurveControlledBob m_weaponBob = new CurveControlledBob();

    [SerializeField] private float m_strideInterval = 5f;
    [SerializeField] [Range(0f, 1f)] private float m_runningStrideLengthen = .7f;
    private CharacterController m_characterController;
    private FPSController m_fpsController;

    private void Awake()
    {
        m_weapon = transform;
        m_characterController = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
        m_fpsController = m_characterController.GetComponent<FPSController>();
    }

    private void Start()
    {
        m_weaponBob.Setup(m_weapon, m_strideInterval);
    }
	
	// Update is called once per frame
	private void Update ()
    {
        UpdateWeaponPosition(m_fpsController.Speed);
	}

    private void UpdateWeaponPosition(float speed)
    {
        if (!m_useWeaponBob || m_weapon == null)
            return;
        /*
        if (m_characterController.velocity.magnitude > 0 && m_characterController.isGrounded)
        {
            m_weapon.localPosition = 
                m_weaponBob.DoObjectBob(m_characterController.velocity.magnitude +
                (speed * (m_fpsController.CurrentMoveState != MoveState.Running ? 1f : m_runningStrideLengthen)));
        }
        */
        if (m_characterController.velocity.magnitude > 0 && m_characterController.isGrounded)
        {
            float speedFactor;
            switch(m_fpsController.CurrentMoveState)
            {
                case MoveState.Walking:
                    speedFactor = 1f;
                    break;
                case MoveState.Running:
                    speedFactor = m_runningStrideLengthen;
                    break;
                case MoveState.Crouching:
                    speedFactor = 1.5f;
                    break;
                default:
                    speedFactor = 0;
                    break;
            }

            m_weapon.localPosition =
                m_weaponBob.DoObjectBob(m_characterController.velocity.magnitude + speed * speedFactor);

        }
    }
}
