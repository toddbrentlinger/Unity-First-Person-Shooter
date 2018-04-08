using UnityEngine;

[System.Serializable]
public class MouseLookSmooth {

    public float xSensitivity = 2f;
    public float ySensitivity = 2f;
    public bool clampVerticalRotation = true;
    public float minimumX = -90F;
    public float maximumX = 90F;
    public bool lockCursor = true;

    public bool m_smooth = true;
    public float m_smoothTime = 5f;

    private Transform m_camera;
    private Transform m_character;

    private Quaternion m_characterRot;
    private Quaternion m_cameraRot;
    private bool m_cursorIsLocked = true;

    public void Init(Transform character, Transform camera)
    {
        m_character = character;
        m_camera = camera;
        m_characterRot = character.localRotation;
        m_cameraRot = camera.localRotation;

        UpdateCursorLock();
    }

    // Called in Update() method on FirstPersonController
    public void LookRotation()
    {
        //m_characterRot = Input.GetAxis("Mouse X") * xSensitivity;
        //m_cameraRot -= Input.GetAxis("Mouse Y") * ySensitivity;
        m_characterRot *= Quaternion.Euler(0f, Input.GetAxis("Mouse X") * ySensitivity, 0f);
        m_cameraRot *= Quaternion.Euler(-Input.GetAxis("Mouse Y") * xSensitivity, 0f, 0f);

        // If vertical rotation is clamped, change m_CameraRot
        if (clampVerticalRotation)
            m_cameraRot = ClampRotationAroundXAxis(m_cameraRot);

        // Rotate character and set camera rotation
        //m_character.Rotate(0, m_characterRot, 0);
        //m_camera.localRotation = Quaternion.Euler(m_cameraRot, 0, 0);
        if (m_smooth)
        {
            m_character.localRotation = Quaternion.Slerp(m_character.localRotation, m_characterRot, m_smoothTime * Time.deltaTime);
            m_camera.localRotation = Quaternion.Slerp(m_camera.localRotation, m_cameraRot, m_smoothTime * Time.deltaTime);
        }
        else
        {
            m_character.localRotation = m_characterRot;
            m_camera.localRotation = m_cameraRot;
        }
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

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, minimumX, maximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}
