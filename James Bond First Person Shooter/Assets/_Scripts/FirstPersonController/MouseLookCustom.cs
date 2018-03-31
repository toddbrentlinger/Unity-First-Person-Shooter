using UnityEngine;

/* NOTES:
 * - Transform.Rotate for camera instead of directly changing transform.localRotation
 * - Create private references to camera.transform and character.transform instead of passing references each time to LookRotation()
 */

[System.Serializable]
public class MouseLookCustom {

    public float xSensitivity = 2f;
    public float ySensitivity = 2f;
    public bool clampVerticalRotation = true;
    public float minimumX = -90F;
    public float maximumX = 90F;
    public bool lockCursor = true;

    private Transform m_camera;
    private Transform m_character;
    private float m_characterRot;
    private float m_cameraRot;
    private bool m_cursorIsLocked = true;

    public void Init(Transform character, Transform camera)
    {
        m_character = character;
        m_camera = camera;
        m_characterRot = character.localRotation.y;
        m_cameraRot = camera.localRotation.x;

        UpdateCursorLock();
    }

    // Called in Update() method on FirstPersonController
    public void LookRotation()
    {
        m_characterRot = Input.GetAxis("Mouse X") * xSensitivity;
        m_cameraRot -= Input.GetAxis("Mouse Y") * ySensitivity;

        // If vertical rotation is clamped, change m_CameraRot
        if (clampVerticalRotation)
            m_cameraRot = Mathf.Clamp(m_cameraRot, minimumX, maximumX);

        // Rotate character and set camera rotation
        m_character.Rotate(0, m_characterRot, 0);
        m_camera.localRotation = Quaternion.Euler(m_cameraRot, 0, 0);
        /*
        // If lockCursor is true, check input & properly lock the cursor
        if (lockCursor)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
                UpdateCursorLock();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                m_cursorIsLocked = true;
                UpdateCursorLock();
            }
        }
        */
    }
    
    public void UpdateCursorLock()
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
