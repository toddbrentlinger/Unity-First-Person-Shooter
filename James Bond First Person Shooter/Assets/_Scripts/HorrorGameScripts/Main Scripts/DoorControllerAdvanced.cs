using UnityEngine;
using System.Collections;

public class DoorControllerAdvanced : MonoBehaviour {

    [SerializeField] private float openAngle = 90.0f;
    private float doorClickOpenAngle = 2.0f;
    private float slightDoorOpenAngle = 10.0f;
    private float maxDistance = 2.0f;

    [SerializeField] private AudioClip openDoorClip;
    [SerializeField] private AudioClip closeDoorClip;
    [SerializeField] private AudioClip slamDoorClip;

    [SerializeField] private AudioClip doorSqueekClip;
    [SerializeField] private AudioClip doorClickOpenClip;
    [SerializeField] private AudioClip doorClickCloseClip;
    [SerializeField] private AudioClip doorLockClip;

    private bool open;
    private bool enter;
    private bool isMoving;
    private bool isSlightOpen;
    private bool slamDoor;
    private bool slightlyOpenDoor;
    [SerializeField] private bool lockDoor = false;

    private Vector3 closedRotation;

    private BoxCollider doorCollider;
    private Rigidbody doorRB;

    // private Vector3 doorSize;
    // private Bounds doorBounds;

    private GameObject hinge;
    private AudioSource hingeAudioSource;
    private GameObject doorLock;
    private AudioSource doorLockAudioSource;

    // Use this for initialization
    void Start()
    {
        closedRotation = transform.rotation.eulerAngles;
        open = false;
        enter = false;
        isMoving = false;
        isSlightOpen = false;
        slamDoor = false;
        slightlyOpenDoor = false;

        doorCollider = gameObject.GetComponent<BoxCollider>();
        if (gameObject.GetComponent<Rigidbody>() != null)
            doorRB = gameObject.GetComponent<Rigidbody>();

        // Add BoxCollider component as trigger
        CreateBoxColliderTrigger(gameObject);

        // Check y-scale to correct direction of door swing
        if (gameObject.transform.localScale.y < 0)
        {
            openAngle *= -1;
            doorClickOpenAngle *= -1;
            slightDoorOpenAngle *= -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!enter || isMoving)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (doorCollider.Raycast(ray, out hit, maxDistance))
            {
                isMoving = true;
                StartCoroutine("MoveDoor");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            enter = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            enter = false;
    }

    public void SetSlamDoor()
    {
        if (open)
        {
            slamDoor = true;
            isMoving = true;
            StartCoroutine("MoveDoor");
        }
    }

    public void SetSlightOpenDoor()
    {
        if (!open)
        {
            slightlyOpenDoor = true;
            isMoving = true;
            StartCoroutine("MoveDoor");
        }
    }

    public void SetLockDoor(bool choice)
    {
        lockDoor = choice;
    }

    private AudioSource CreateAudioSource(GameObject targetGameObject)
    {
        AudioSource audio = targetGameObject.AddComponent<AudioSource>();

        audio.volume = 1.0f;
        audio.spatialBlend = 1.0f;
        audio.minDistance = 0.5f; // 1.0
        audio.maxDistance = 30.0f; // 50

        return audio;
    }

    private void CreateBoxColliderTrigger(GameObject targetGameObject)
    {
        BoxCollider collider = targetGameObject.AddComponent<BoxCollider>();

        collider.center = gameObject.transform.InverseTransformPoint(doorCollider.bounds.center);
        collider.size = new Vector3(2.0f, 2.5f, 2.3f);
        collider.isTrigger = true;
    }

    IEnumerator MoveDoor()
    {
        // Door rotation parameters
        float from, to, startTime, duration;

        // Create hinge GameObject with AudioSource
        CreateHingeObject();

        // if door is closed
        if (!open)
        {
            // if door is slightly open
            if (isSlightOpen)
            {
                hingeAudioSource.clip = openDoorClip;
                from = closedRotation.z - slightDoorOpenAngle;
                to = closedRotation.z - openAngle;

                // Reset bool values
                isSlightOpen = false;
            }

            // if door is not slightly open (completely closed)
            else
            {
                // Create doorLock GameObject with AudioSource
                CreateDoorLockObject();

                // Play audio of door clicking open
                doorLockAudioSource.PlayOneShot(doorClickOpenClip);

                // Door animation in time with audioClip
                yield return new WaitForSeconds(0.3f);

                // Check if door is locked
                if (lockDoor)
                {
                    isMoving = false;
                    DestroyDoorObjects();
                    yield break;
                }

                // Set door rotation parameters for door moving slightly from clicking open
                from = closedRotation.z;
                to = closedRotation.z - doorClickOpenAngle;
                startTime = Time.time;
                duration = 0.4f;

                // Rotate door while audioClip is playing
                while (doorLockAudioSource.isPlaying)
                {
                    RotateDoor(from, to, startTime, duration);
                    /*
                    float timeRatio = (Time.time - startTime) / duration;
                    float rotation = Mathf.LerpAngle(from, to, timeRatio);

                    if (doorRB != null)
                        doorRB.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                            closedRotation.y, rotation));
                    else
                        transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                            closedRotation.y, rotation));
                    */
                    yield return null;
                }

                // Destroy doorLock gameObject containing AudioSource component
                Destroy(doorLock);

                // Set door rotation parameters for door opening
                from = closedRotation.z - doorClickOpenAngle;

                // if door is to be opened slightly
                if (slightlyOpenDoor)
                {
                    hingeAudioSource.clip = doorSqueekClip;
                    to = closedRotation.z - slightDoorOpenAngle;

                    // Reset bool values
                    isSlightOpen = true;
                    slightlyOpenDoor = false;
                }
                // else door is to be opened fully
                else
                {
                    hingeAudioSource.clip = openDoorClip;
                    to = closedRotation.z - openAngle;
                }
            }

            // Play audio of door opening
            hingeAudioSource.Play();
            startTime = Time.time;
            duration = hingeAudioSource.clip.length;

            // Rotate door while audioClip is playing
            while (hingeAudioSource.isPlaying)
            {
                RotateDoor(from, to, startTime, duration);
                /*
                float timeRatio = (Time.time - startTime) / duration;
                float rotation = Mathf.SmoothStep(from, to, timeRatio);

                if (doorRB != null)
                    doorRB.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                        closedRotation.y, rotation));
                else
                    transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                        closedRotation.y, rotation));
                */
                yield return null;
            }

            // Set bool values
            if (isSlightOpen)
                open = false;
            else
                open = true;
        }

        // if door is open
        else if (open)
        {
            // Set door rotation parameters for closing door
            from = closedRotation.z - openAngle;
            to = closedRotation.z;

            // if door is to be slammed
            if (slamDoor)
            {
                hingeAudioSource.PlayOneShot(slamDoorClip);
                duration = 0.6f;

                // Reset bool values
                slamDoor = false;
            }
            // else door is closed normally
            else
            {
                hingeAudioSource.PlayOneShot(closeDoorClip);
                duration = closeDoorClip.length - 0.2f;
            }

            startTime = Time.time;

            // Rotate door while audioClip is playing
            while (hingeAudioSource.isPlaying)
            {
                RotateDoor(from, to, startTime, duration);
                /*
                float timeRatio = (Time.time - startTime) / duration;
                float rotation = Mathf.SmoothStep(from, to, timeRatio);

                if (doorRB != null)
                    doorRB.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                        closedRotation.y, rotation));
                else
                    transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                        closedRotation.y, rotation));
                */
                yield return null;
            }

            // Set bool values
            open = false;
        }

        // Destroy hinge GameObject containing audioSource component
        Destroy(hinge);
        DestroyDoorObjects();

        // Set bool values
        isMoving = false;
    }

    private void RotateDoor(float from, float to, float startTime, float duration)
    {
        float timeRatio = (Time.time - startTime) / duration;
        float rotation = Mathf.SmoothStep(from, to, timeRatio);

        if (doorRB != null)
            doorRB.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));
        else
            transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));
    }

    void CreateHingeObject()
    {
        // Create gameObject with AudioSource at position of hinge
        hinge = new GameObject("Hinge");
        hingeAudioSource = CreateAudioSource(hinge);

        // Parent hinge gameObject to door
        hinge.transform.parent = gameObject.transform;

        // Set local position of hinge
        hinge.transform.localPosition = new Vector3(0.0f, -0.5f * doorCollider.size.y, 0.5f * doorCollider.size.z);
    }

    void CreateDoorLockObject()
    {
        // Create gameObject with AudioSource at position of door lock
        doorLock = new GameObject("DoorLock");
        doorLockAudioSource = CreateAudioSource(doorLock);

        // Parent doorLock gameObject to door
        doorLock.transform.parent = gameObject.transform;

        // Set local position of doorLock
        doorLock.transform.localPosition = new Vector3(-doorCollider.size.x, -0.5f * doorCollider.size.y, 
            0.5f * doorCollider.size.z);
    }

    void DestroyDoorObjects()
    {
        if (hinge != null)
            Destroy(hinge);
        if (doorLock != null)
            Destroy(doorLock);
    }

    private float pushPower = 2.0f;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
            return;

        if (hit.moveDirection.y < -0.3f)
            return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.velocity = pushDir * pushPower;
    }
    /*
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Colliding");
        }
        else
            Debug.Log("Not Colliding");
    }
    */
}