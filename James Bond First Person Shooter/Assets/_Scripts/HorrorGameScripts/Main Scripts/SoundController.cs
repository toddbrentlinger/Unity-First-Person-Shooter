using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour {

    public AudioClip bounceClip;

    private AudioSource audioSource;

	// Use this for initialization
	void Start ()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 1.0f)
        {
            float t = Mathf.InverseLerp(2.0f, 5.0f, collision.relativeVelocity.magnitude);
            float volume = Mathf.Lerp(0.2f, 1.0f, t);

            audioSource.PlayOneShot(bounceClip);
            audioSource.volume = volume;

            // Debug.Log("t: " + t + " volume: " + volume);
        }
    }
}
