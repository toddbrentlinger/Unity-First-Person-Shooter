using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManoeuvreCameraController : MonoBehaviour {

    [Header("-- Common Properties --")]
    public float lookSensitivity = 5;
    public float lookSmoth = 0.1f;

    public Vector2 MinMaxAngle = new Vector2(65, -65);

    float yRot;
    float xRot;

    float currentYRot;
    float currentXRot;

    float yRotVelocity;
    float xRotVelocity;

    ManoeuvreFPSController fpsController;

    Vector3 camPos;

    CharacterController charController;

    // Use this for initialization
    void Start()
    {
        camPos = transform.localPosition;
        charController = GetComponentInParent<CharacterController>();
        fpsController = GetComponentInParent<ManoeuvreFPSController>();
    }

    // Update is called once per frame
    void Update()
    {

        yRot += Input.GetAxis("Mouse X") * lookSensitivity;
        xRot -= Input.GetAxis("Mouse Y") * lookSensitivity;

        xRot = Mathf.Clamp(xRot, MinMaxAngle.x, MinMaxAngle.y);

        currentXRot = Mathf.SmoothDamp(currentXRot, xRot, ref xRotVelocity, lookSmoth);
        currentYRot = Mathf.SmoothDamp(currentYRot, yRot, ref yRotVelocity, lookSmoth);

        transform.rotation = Quaternion.Euler(currentXRot, currentYRot, 0);

    }

    /// <summary>
    /// Shakes the Camera Rotation
    /// </summary>
    /// <param name="shakeDuration"></param>
    /// <param name="shakeAmount"></param>
    /// <param name="decreaseFactor"></param>
    /// <returns></returns>
    public IEnumerator ShakeCamera(float shakeDuration, float shakeAmount = 0.2f, float decreaseFactor = 0.3f)
    {
        Vector3 originalRot = transform.eulerAngles;
        float currentShakeDuration = shakeDuration;
        while (currentShakeDuration > 0)
        {
            transform.eulerAngles += Random.insideUnitSphere * shakeAmount;
            currentShakeDuration -= Time.deltaTime * decreaseFactor;
            yield return null;
        }

    }
}
