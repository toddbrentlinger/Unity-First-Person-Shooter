using UnityEngine;
using System.Collections;

public class NoisePlayRandom : MonoBehaviour {

    [SerializeField] private float maxTriggerTime = 15.0f;

    private AudioSource audioSource;
    private float timeInTrigger;

	// Use this for initialization
	void Start () {
        audioSource = GetComponent<AudioSource>();
        timeInTrigger = 0;
	}
	
	// Update is called once per frame
	void Update () {
	    if (timeInTrigger > maxTriggerTime)
        {
            audioSource.Play();
            timeInTrigger = 0;
        }
	}

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !audioSource.isPlaying)
        {
            timeInTrigger += Time.deltaTime;
        }
    }
}
