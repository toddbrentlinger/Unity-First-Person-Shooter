using UnityEngine;
using System.Collections;

public class CameraFollower : MonoBehaviour {

    private float smoothTime = 0.10f;
    private float smoothTimeHeight = 0.2f;
    private float MAX_DISTANCE = 4.0f;
    private float MIN_DISTANCE = 0.5f; // 1.0

    private Camera m_cam;
    private Light m_light;

    private Vector3 rotationVelocity = Vector3.zero;
    private Vector3 relativePosition;

    private float focalDistance;
    private float focalDistanceSpeed = 10.0f; // 10
    private float heightSpeed = 1.5f;

    private bool keepLightOff;

    private Ray ray;
    private RaycastHit hit;

    // Light intensity parameters
    private float minLightIntensity = 0.4f;
    private float maxLightIntensity = 2.0f;
    private float startLightIntensity;

    // Flashlight states
    private enum flashlightState { IsWorking, Flickering, Resetting };
    private flashlightState currentState;

    // IsWorking state parametersv
    private float minWorkingTime = 60.0f; // 60
    private float maxWorkingTime = 120.0f; // 120
    private float workingTimer;
    private float workingTimeLimit;

    // Flickering state parameters
    private float minFlickerSpeed = 0.05f;
    private float maxFlickerSpeed = 0.2f;
    private float flickerCounter;

    private float minFlickerTime = 0.6f;
    private float maxFlickerTime = 1.4f;
    private float flickerTimer;
    private float flickerTimeLimit;

    // Resetting state parameters
    private float resetTimer = 0.0f;

    // Player layer mask to ignore Player collider for raycast
    private int m_layerMask;

    // Use this for initialization
    void Start()
    {
        m_cam = Camera.main;
        m_light = GetComponent<Light>();
        relativePosition = m_cam.transform.position - transform.position;
        focalDistance = MAX_DISTANCE;
        keepLightOff = false;

        startLightIntensity = m_light.intensity;
        currentState = flashlightState.IsWorking;
        workingTimeLimit = Random.Range(minWorkingTime, maxWorkingTime);

        // Initialize timers
        workingTimer = 0.0f;
        flickerCounter = 0.0f;
        flickerTimer = 0.0f;

        // Set layer mask
        // Bit shift the index of the layer (8) to get a bit mask that would
        // cast rays against colliders only in layer (8)
        m_layerMask = 1 << 8;   
        // Invert bit mask to cast rays against colliders in all layers but layer (8)
        m_layerMask = ~m_layerMask;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            StartCoroutine("ToggleLight");

        MoveFlashlight();

        if (keepLightOff)
            return;

        switch (currentState)
        {
            case flashlightState.IsWorking:
                IsWorking();
                break;

            case flashlightState.Flickering:
                FlickerFlashlight();
                break;

            case flashlightState.Resetting:
                // Resetting();
                break;
        }
    }
    /*
    void LateUpdate()
    {
        MoveFlashlight();
    }
    */
    void MoveFlashlight()
    {
        // Set flashlight transform relative to Camera.main
        // transform.position = m_cam.transform.position - relativePosition;

        // Find focalDistance from Camera.main that the flashlight will target
        // ray = m_cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        ray = new Ray(m_cam.transform.position, m_cam.transform.TransformDirection(Vector3.forward));

        if (Physics.Raycast(ray, out hit, MAX_DISTANCE, m_layerMask, QueryTriggerInteraction.Ignore))
        {
            //focalDistance = Mathf.Clamp(hit.distance, MIN_DISTANCE, MAX_DISTANCE);

            focalDistance = Mathf.MoveTowards(focalDistance, hit.distance, focalDistanceSpeed * Time.deltaTime);
            focalDistance = Mathf.Clamp(focalDistance, MIN_DISTANCE, MAX_DISTANCE);
        }

        // Move flashlight height closer to camera relative to focalDistance
        float t = Mathf.InverseLerp(MIN_DISTANCE, MAX_DISTANCE, focalDistance);

        /*
        float targetHeightY = Mathf.Lerp(m_cam.transform.position.y, 
            (m_cam.transform.position - relativePosition).y, t);

        Vector3 targetVector = m_cam.transform.position - relativePosition;

        targetVector.y = Mathf.MoveTowards(transform.position.y, targetHeightY, heightSpeed * Time.deltaTime);
        targetVector.y = Mathf.Clamp(targetVector.y, (m_cam.transform.position - relativePosition).y,
            m_cam.transform.position.y);
        */

        float targetHeightY = Mathf.Lerp(0, relativePosition.y, t);
        Vector3 targetVector = m_cam.transform.position - relativePosition + 
            new Vector3(0, relativePosition.y - targetHeightY, 0);

        // Set flashlight position
        transform.position = targetVector;

        // Find relative vector from flashlight to target
        // Vector3 target = m_cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, focalDistance));
        Vector3 target = m_cam.transform.TransformPoint(Vector3.forward * focalDistance);
        Vector3 relativeTarget = target - transform.position;

        Quaternion rotation = Quaternion.LookRotation(relativeTarget);

        Vector3 targetRotation = rotation.eulerAngles;
        Vector3 eulerAngles = transform.rotation.eulerAngles;

        
        eulerAngles.x = Mathf.SmoothDampAngle(eulerAngles.x, targetRotation.x,
        ref rotationVelocity.x, smoothTime);
        eulerAngles.y = Mathf.SmoothDampAngle(eulerAngles.y, targetRotation.y,
            ref rotationVelocity.y, smoothTime);
        eulerAngles.z = Mathf.SmoothDampAngle(eulerAngles.z, targetRotation.z,
            ref rotationVelocity.z, smoothTime);
        
        // Set flashlight rotation
        transform.rotation = Quaternion.Euler(eulerAngles);
    }

    IEnumerator ToggleLight()
    {
        // Play flashlight click audioClip
        m_light.gameObject.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.5f * m_light.gameObject.GetComponent<AudioSource>().clip.length - 0.1f);

        // if light is off, about to be turned on, reset timers
        if (!m_light.enabled)
            ResetTimers();

        // if light is not kept off, toggle light
        if (!keepLightOff)
            m_light.enabled = !m_light.enabled;
    }

    public void KeepLightOff(bool choice)
    {
        keepLightOff = choice;
    }

    private void ResetTimers()
    {
        currentState = flashlightState.IsWorking;
        workingTimeLimit = Random.Range(minWorkingTime, maxWorkingTime);

        // Initialize timers
        workingTimer = 0.0f;
        flickerCounter = 0.0f;
        flickerTimer = 0.0f;
    }

    private void IsWorking()
    {
        // Add deltaTime to workingTimer
        workingTimer += Time.deltaTime;

        // if workingTimer exceeds the limit, set up flickering state
        if (workingTimer > workingTimeLimit)
        {
            // Set parameters
            flickerTimer = 0.0f;

            // Time to initial flicker of light
            flickerCounter = Time.time + Random.Range(minFlickerSpeed, maxFlickerSpeed);

            // Set flickerTimeLimit (total duration of flicker state)
            flickerTimeLimit = Random.Range(minFlickerTime, maxFlickerTime);

            // Set currentState to Flickering
            currentState = flashlightState.Flickering;
        }
    }

    private void FlickerFlashlight()
    {
        // Add deltaTime to flickerTimer
        flickerTimer += Time.deltaTime;

        // if time to next flick exceeds current time, toggle light
        if (Time.time > flickerCounter)
        {
            // if light is on, turn off
            if (m_light.enabled)
                m_light.enabled = false;
            // else 
            else
            {
                // Turn light on
                m_light.enabled = true;

                // Set random light intensity
                m_light.intensity = Random.Range(minLightIntensity, maxLightIntensity);
            }

            // Time to next flicker of light
            flickerCounter = Time.time + Random.Range(minFlickerSpeed, maxFlickerSpeed);
        }

        // if flickerTimer exceeds the limit, set up isWorking state
        if (flickerTimer > flickerTimeLimit)
        {
            // Set parameters
            workingTimer = 0.0f;

            // Make sure light is on
            m_light.enabled = true;
            m_light.intensity = startLightIntensity;

            // Set new workingTimeLimit
            workingTimeLimit = Random.Range(minWorkingTime, maxWorkingTime);

            // Set currentState to IsWorking
            currentState = flashlightState.IsWorking;
        }
    }
    /*
    private void Resetting()
    {
        resetTimer += Time.deltaTime;

        if (resetTimer > 0.75f)
        {
            resetTimer = 0.0f;
            workingTimer = 0.0f;
            workingTimeLimit = Random.Range(minWorkingTime, maxWorkingTime);
            m_light.enabled = true;
            m_light.intensity = startLightIntensity;
            currentState = flashlightState.IsWorking;
        }

        else if (resetTimer > 0.65f)
            m_light.enabled = false;

        else if (resetTimer > 0.55f)
        {
            m_light.enabled = true;
            m_light.intensity = startLightIntensity;
        }

        else if (resetTimer > 0.25f)
            m_light.enabled = false;

        else if (resetTimer > 0.15f)
        {
            m_light.enabled = true;
            m_light.intensity = startLightIntensity;
        }

        else if (resetTimer > 0.05f)
            m_light.enabled = false;
    }
    */
}
