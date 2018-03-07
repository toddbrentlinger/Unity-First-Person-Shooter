using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour {

    [SerializeField] private float openAngle = 90.0f;
    private float doorClickOpenAngle = 2.0f;
    private float slightDoorOpenAngle = 10.0f;

    [SerializeField] private AudioClip openDoorClip;
    [SerializeField] private AudioClip closeDoorClip;
    [SerializeField] private AudioClip slamDoorClip;

    [SerializeField] private AudioClip doorSqueekClip;
    [SerializeField] private AudioClip doorClickOpenClip;
    [SerializeField] private AudioClip doorClickCloseClip;
    [SerializeField] private AudioClip doorLockClip;

    private AudioSource audioSource;

    private bool open;
    private bool enter;
    private bool isMoving;
    private bool isSlightOpen;
    // private bool slamDoor;
    private bool lockDoor;
    // private bool slightlyOpenDoor;

    private Vector3 closedRotation;
    private Vector3 doorSize;
    private Bounds doorBounds;

    // Use this for initialization
    void Start()
    {
        closedRotation = transform.rotation.eulerAngles;
        open = false;
        enter = false;
        isMoving = false;
        isSlightOpen = false;
        // slamDoor = false;
        lockDoor = false;
        // slightlyOpenDoor = false;

        if (!GetComponent<AudioSource>())
            audioSource = CreateAudioSource(gameObject);
        else
            audioSource = GetComponent<AudioSource>();

        // Get box collider on door to represent dimensions of door
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        BoxCollider doorCollider = colliders[0]; // first collider by default
        doorSize = doorCollider.bounds.size; // first collider size by defualt
        foreach (BoxCollider collider in colliders)
            if (!collider.isTrigger)
            {
                doorCollider = collider;
                doorBounds = doorCollider.bounds;
                doorSize = doorBounds.size;
            }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Locked: " + lockDoor);

        if (!enter || isMoving)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // open = !open;
            isMoving = true;
            if (open)
                StartCoroutine("CloseDoor");
            else
            {
                if (isSlightOpen)
                    StartCoroutine("OpenSlightOpenDoor");
                else
                    StartCoroutine("OpenDoor");
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
            // slamDoor = true;
            isMoving = true;
            StartCoroutine("SlamDoor");
        }
    }

    public void SetSlightOpenDoor()
    {
        if (!open)
        {
            // slightlyOpenDoor = true;
            isMoving = true;
            StartCoroutine("SlightOpenDoor");
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

    // Open Door Coroutine
    IEnumerator OpenDoor()
    {
        // Create gameObject with AudioSource at position of door lock
        GameObject doorLock = new GameObject("DoorLock");
        AudioSource doorLockAudioSource = CreateAudioSource(doorLock);
        // Vector3 localVecToDoorLock = new Vector3(-doorSize.x, doorSize.y / 2, -doorSize.z / 2);
        // doorLock.transform.position = transform.position + localVecToDoorLock;

        // Parent doorLock gameObject to door
        doorLock.transform.parent = gameObject.transform;

        // Set local position of doorLock
        doorLock.transform.localPosition = new Vector3(-doorSize.x, -0.5f * doorSize.z, 0.5f * doorSize.y);

        // Play doorClickOpenClip first before setting main audio clip for opening
        doorLockAudioSource.PlayOneShot(doorClickOpenClip, doorLockAudioSource.volume);

        // Door animation in time with audioClip
        yield return new WaitForSeconds(0.2f);

        if (lockDoor)
        {
            open = false;
            isMoving = false;
            StopCoroutine("OpenDoor");
        }

        float from = closedRotation.z;
        float to = closedRotation.z - doorClickOpenAngle;
        float startTime = Time.time;
        float duration = 0.4f;
        while (doorLockAudioSource.isPlaying)
        {
            float timeRatio = (Time.time - startTime) / duration;
            float rotation = Mathf.LerpAngle(from, to, timeRatio);

            transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));

            yield return null;
        }

        // Destroy doorLock gameObject containing AudioSource component
        Destroy(doorLock);

        // Play main clip; plays during rotation of door
        audioSource.PlayOneShot(openDoorClip, audioSource.volume);

        from = closedRotation.z - doorClickOpenAngle;
        to = closedRotation.z - openAngle;
        startTime = Time.time;
        duration = openDoorClip.length;
        while (audioSource.isPlaying)
        {
            float timeRatio = (Time.time - startTime) / duration;
            float rotation = Mathf.SmoothStep(from, to, timeRatio);

            transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));

            yield return null;
        }

        DestroyObject(doorLock);
        open = true;
        isMoving = false;
    }

    // Close Door Coroutine
    IEnumerator CloseDoor()
    {
        // Play main clip; plays during rotation of door
        audioSource.PlayOneShot(closeDoorClip, audioSource.volume);

        float from = closedRotation.z - openAngle;
        float to = closedRotation.z;
        float startTime = Time.time;
        float duration = closeDoorClip.length - 0.2f;
        while (audioSource.isPlaying)
        {
            float timeRatio = (Time.time - startTime) / duration;
            float rotation = Mathf.SmoothStep(from, to, timeRatio);

            transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));

            yield return null;
        }

        open = false;
        isMoving = false;
    }

    // Slam Door Coroutine
    IEnumerator SlamDoor()
    {
        // Play main clip; plays during rotation of door
        audioSource.PlayOneShot(slamDoorClip, audioSource.volume);

        float from = closedRotation.z - openAngle;
        float to = closedRotation.z;
        float startTime = Time.time;
        float duration = 0.6f;
        while (audioSource.isPlaying)
        {
            float timeRatio = (Time.time - startTime) / duration;
            float rotation = Mathf.SmoothStep(from, to, timeRatio);

            transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));

            yield return null;
        }

        open = false;
        isMoving = false;
        // slam = false;
    }

    // Slight Open Door Coroutine
    IEnumerator SlightOpenDoor()
    {
        // Create gameObject with AudioSource at position of door lock
        GameObject doorLock = new GameObject("DoorLock");
        AudioSource doorLockAudioSource = CreateAudioSource(doorLock);
        // Vector3 localVecToDoorLock = new Vector3(-doorSize.x, doorSize.y / 2, -doorSize.z / 2);
        // doorLock.transform.position = transform.position + localVecToDoorLock;

        // Parent doorLock gameObject to door
        doorLock.transform.parent = gameObject.transform;

        // Set local position of doorLock
        doorLock.transform.localPosition = new Vector3(-doorSize.x, -0.5f * doorSize.z, 0.5f * doorSize.y);

        // Play doorClickOpenClip first before setting main audio clip for opening
        doorLockAudioSource.PlayOneShot(doorClickOpenClip, doorLockAudioSource.volume);

        // Door animation in time with audioClip
        yield return new WaitForSeconds(0.2f);

        if (lockDoor)
        {
            DestroyObject(doorLock);
            // open = false;
            isMoving = false;
            StopCoroutine("SlightOpenDoor");
        }

        float from = closedRotation.z;
        float to = closedRotation.z - doorClickOpenAngle;
        float startTime = Time.time;
        float duration = 0.4f;
        while (doorLockAudioSource.isPlaying)
        {
            float timeRatio = (Time.time - startTime) / duration;
            float rotation = Mathf.SmoothStep(from, to, timeRatio);

            transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));

            yield return null;
        }

        // Destroy doorLock gameObject containing AudioSource component
        Destroy(doorLock);

        // Play main clip; plays during rotation of door
        audioSource.PlayOneShot(doorSqueekClip, audioSource.volume);

        from = closedRotation.z - doorClickOpenAngle;
        to = closedRotation.z - slightDoorOpenAngle;
        startTime = Time.time;
        duration = doorSqueekClip.length;
        while (audioSource.isPlaying)
        {
            float timeRatio = (Time.time - startTime) / duration;
            float rotation = Mathf.SmoothStep(from, to, timeRatio);

            transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));

            yield return null;
        }

        DestroyObject(doorLock);
        isSlightOpen = true;
        isMoving = false;
    }

    // Open Slightly Open Door Coroutine
    IEnumerator OpenSlightOpenDoor()
    {
        // Play main clip; plays during rotation of door
        audioSource.PlayOneShot(openDoorClip, audioSource.volume);

        float from = closedRotation.z - slightDoorOpenAngle;
        float to = closedRotation.z - openAngle;
        float startTime = Time.time;
        float duration = openDoorClip.length;
        while (audioSource.isPlaying)
        {
            float timeRatio = (Time.time - startTime) / duration;
            float rotation = Mathf.SmoothStep(from, to, timeRatio);

            transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));

            yield return null;
        }

        open = true;
        isMoving = false;
        isSlightOpen = false;
    }
}
