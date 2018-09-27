using UnityEngine;

/* NOTES:
 * - Do I need lockCursor in MouseLook class?
 */

namespace LoneWolf.FPS
{
    [System.Serializable]
    public class MouseLook
    {
        public float xSensitivity = 2f;
        public float ySensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float minimumX = -90F;
        public float maximumX = 90F;
        public bool smooth = false;
        public float smoothTime = 2f;
        public bool lockCursor = true;

        // Transform references for character and camera
        private Transform m_character;
        private Transform m_camera;

        // Rotation references for character and camera
        private Quaternion m_characterTargetRot;
        private Quaternion m_cameraTargetRot;

        public void Init(Transform character, Transform camera)
        {
            m_character = character;
            m_camera = camera;

            m_characterTargetRot = character.localRotation;
            m_cameraTargetRot = camera.localRotation;

            UpdateCursorLock();
        }

        public void LookRotation()
        {
            float yRot = Input.GetAxis("Mouse X") * xSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * ySensitivity;

            m_characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            // If vertical rotation is clamped, change m_CameraRot
            if (clampVerticalRotation)
                m_cameraTargetRot = ClampRotationAroundXAxis(m_cameraTargetRot);

            // Set character and camera rotation
            m_character.localRotation = m_characterTargetRot;
            m_camera.localRotation = m_cameraTargetRot;
        }

        private void UpdateCursorLock()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private Quaternion ClampRotationAroundXAxis(Quaternion q)
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
}
