using UnityEngine;

namespace LoneWolf.FPS
{
    [RequireComponent(typeof(CharacterController))]
    public class FPSControllerAdvanced : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private bool m_playerControl = true;

        [Header("Camera")]
        [SerializeField] private MouseLook m_mouseLook;
        private Camera m_fpsCamera;

        [Header("Movement")]
        [SerializeField] private Movement m_movement;

        [Header("Editor/GUI")]
        [SerializeField] private bool m_showStatsGUI = false;

        private void Awake()
        {
            m_movement.Init(this, transform, GetComponent<CharacterController>());
            m_fpsCamera = Camera.main; // GetComponentInChildren<Camera>() but there is 
            // another camera that renders only weapons. Perhaps use drag/drop in editor to manually
            // connect this FPSController to a specific camera.
        }

        private void Start()
        {
            m_mouseLook.Init(transform, m_fpsCamera.transform);
            m_movement.SetupSpeed();
        }

        private void Update()
        {
            if (!m_playerControl)
                return;

            // Rotate fpsCamera and CharacterController depending on MouseX/MouseY input
            m_mouseLook.LookRotation();

            // Update Movement script to move Player
            m_movement.UpdateMovement();
        }

        // ---------- Falling ---------- 

        // If falling damage occured, this is the place to do something about it. You can make the player
        // have hitpoints and remove some of them based on the distance fallen, add sound effects, etc.
        public void FallingDamageAlert(float fallDistance)
        {
            print("Ouch! Fell " + fallDistance + " units!");
        }

        // ---------- OnGUI ---------- 
        /*
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
                "\nSpeed: " + m_currSpeed +
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
        */
    }
}
