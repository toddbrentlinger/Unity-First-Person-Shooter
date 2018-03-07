using UnityEngine;
using System.Collections;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] private float openAngle = 90.0f;
    // [SerializeField] private float duration = 3.0f;

    [SerializeField] private AudioClip openDoorClip;
    [SerializeField] private AudioClip closeDoorClip;
    [SerializeField] private AudioClip slamDoorClip;
    [SerializeField] private AudioClip slightOpenDoorClip;
    [SerializeField] private AudioClip doorClickOpenClip;
    [SerializeField] private AudioClip doorLockClip;

    private AudioSource audioSource;

    private bool open;
    private bool enter;
    private bool slam;
    private bool lockDoor;
    private bool isMoving;
    private Vector3 closedRotation;
    private float startTime;

    // Use this for initialization
    void Start()
    {
        closedRotation = transform.rotation.eulerAngles;
        open = false;
        enter = false;
        slam = false;
        lockDoor = false;
        isMoving = false;

        if (!GetComponent<AudioSource>())
            audioSource = CreateAudioSource(gameObject);
        else
            audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && enter && !isMoving && !lockDoor)
        {
            open = !open;
            isMoving = true;
            // StartCoroutine("MoveDoor");
            StartCoroutine("MoveDoorAdvanced");
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

    IEnumerator MoveDoor()
    {
        PlaySound();

        float duration, from, to;
        duration = audioSource.clip.length;

        if (!open)
        {
            from = closedRotation.z - openAngle;
            to = closedRotation.z;
            if (slam)
                duration = 0.6f;
            else
                duration -= 1.5f;
        }
        else
        {
            from = closedRotation.z;
            to = closedRotation.z - openAngle;
        }
        
        startTime = Time.time;
        while (isMoving)
        {
            float timeRatio = (Time.time - startTime) / duration;
            float rotation = Mathf.SmoothStep(from, to, timeRatio);

            transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));

            if (timeRatio > 1.0f)
            {
                isMoving = false;
            }

            yield return null;
        }

        slam = false;
    }

    public void SlamDoor()
    {
        if (open)
        {
            slam = true;
            open = false;
            isMoving = true;
            StartCoroutine("MoveDoor");
        }
    }

    public void LockDoor(bool choice)
    {
        if (!open)
            lockDoor = choice;
    }

    void PlaySound()
    {
        if (!open)
        {
            if (slam)
                audioSource.clip = slamDoorClip;
            else
                audioSource.clip = closeDoorClip;
        }
        else
            audioSource.clip = openDoorClip;

        audioSource.Play();
    }

    AudioSource CreateAudioSource(GameObject targetGameObject)
    {
        AudioSource audio = targetGameObject.AddComponent<AudioSource>();

        audio.volume = 0.5f;
        audio.spatialBlend = 1.0f;
        audio.minDistance = 1.0f;
        audio.maxDistance = 50.0f;

        return audio;
    }

    IEnumerator MoveDoorAdvanced()
    {
        // Get box collider on door to represent dimensions of door
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        BoxCollider doorCollider = colliders[0]; // first collider by default
        Vector3 doorSize = doorCollider.bounds.size; // first collider size by defualt
        foreach (BoxCollider collider in colliders)
            if (!collider.isTrigger)
            {
                doorCollider = collider;
                Bounds doorBounds = doorCollider.bounds;
                doorSize = doorBounds.size;
            }

        /*
        // if door is being opened, create gameObject with AudioSource at position of door lock
        GameObject doorLock = new GameObject("DoorLock");
        AudioSource doorLockAudioSource = CreateAudioSource(doorLock);
        Vector3 localVecToDoorLock = new Vector3(-doorSize.x, doorSize.y/2, -doorSize.z/2);
        doorLock.transform.position = transform.position + localVecToDoorLock;

        // Parent doorLock gameObject to door
        doorLock.transform.parent = gameObject.transform;
        */

        // Set correct audio clip and duration depending on bool parameters
        float duration;
        if (!open)
        {
            if (slam)
            {
                audioSource.clip = slamDoorClip;
                duration = 0.6f;
            }
            else
            {
                audioSource.clip = closeDoorClip;
                duration = closeDoorClip.length - 1.5f;
            }
        }
        else
        {
            // if door is being opened, create gameObject with AudioSource at position of door lock
            GameObject doorLock = new GameObject("DoorLock");
            AudioSource doorLockAudioSource = CreateAudioSource(doorLock);
            Vector3 localVecToDoorLock = new Vector3(-doorSize.x, doorSize.y / 2, -doorSize.z / 2);
            doorLock.transform.position = transform.position + localVecToDoorLock;

            // Parent doorLock gameObject to door
            doorLock.transform.parent = gameObject.transform;

            // play doorClickOpenClip first before setting main audio clip for opening
            doorLockAudioSource.PlayOneShot(doorClickOpenClip, doorLockAudioSource.volume);

            while (doorLockAudioSource.isPlaying)
                yield return null;

            Destroy(doorLock);

            audioSource.clip = openDoorClip;
            duration = openDoorClip.length;
        }

        // Play main clip; plays during rotation of door
        audioSource.Play();

        float from, to;
        if (!open)
        {
            from = closedRotation.z - openAngle;
            to = closedRotation.z;
        }
        else
        {
            from = closedRotation.z;
            to = closedRotation.z - openAngle;
        }

        startTime = Time.time;
        while (isMoving)
        {
            float timeRatio = (Time.time - startTime) / duration;
            float rotation = Mathf.SmoothStep(from, to, timeRatio);

            transform.rotation = Quaternion.Euler(new Vector3(closedRotation.x,
                closedRotation.y, rotation));

            if (timeRatio > 1.0f)
            {
                isMoving = false;
            }

            yield return null;
        }

        slam = false;
    }
}
