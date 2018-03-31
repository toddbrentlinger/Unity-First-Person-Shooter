using UnityEngine;

public class FPSWalkerMouseLook : MonoBehaviour {

    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    public bool clampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public bool lockCursor = true;

    public Transform character;
    public Transform camera;

    private float m_CharacterRot;
    private float m_CameraRot;
    private bool m_cursorIsLocked = true;

    private void Start()
    {
        m_CharacterRot = character.localRotation.y;
        m_CameraRot = camera.localRotation.x;

        UpdateCursorLock();
    }

    private void Update()
    {
        m_CharacterRot = Input.GetAxis("Mouse X") * XSensitivity;
        m_CameraRot -= Input.GetAxis("Mouse Y") * YSensitivity;

        // If vertical rotation is clamped, change m_CameraRot
        if (clampVerticalRotation)
            m_CameraRot = Mathf.Clamp(m_CameraRot, MinimumX, MaximumX);

        // Set character and camera rotations
        character.Rotate(0, m_CharacterRot, 0);
        camera.localRotation = Quaternion.Euler(m_CameraRot, 0, 0);

        // If lockCursor is true, check input & properly lock the cursor
        if (lockCursor)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
                UpdateCursorLock();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
                UpdateCursorLock();
            }
        }
    }

    private void UpdateCursorLock()
    {
        if (m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
