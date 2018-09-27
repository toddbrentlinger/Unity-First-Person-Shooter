using UnityEngine;

namespace LoneWolf.FPS
{
    [System.Serializable]
    public class Crouch
    {
        private enum CrouchState { Down, Up, Jump };
        private CrouchState m_crouchState;

        private bool m_crouching;
        private bool m_grounded;
        private CharacterController m_characterController;
        private float m_crouchHeight;
        private float m_originalCharacterControllerCenterY;
        private float m_originalCharacterControllerHeight;
        private Camera m_fpsCamera;
        private float m_originalCameraLocalHeight;
        private float m_crouchSpeed;

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
            switch (m_crouchState)
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
    } 
}
