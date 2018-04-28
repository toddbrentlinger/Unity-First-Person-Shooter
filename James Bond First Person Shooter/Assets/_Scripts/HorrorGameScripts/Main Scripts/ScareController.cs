using UnityEngine;
using System.Collections;

public class ScareController : MonoBehaviour {

    [SerializeField] private GameObject ghost;
    [SerializeField] private Light spotlight;

    private Camera mainCam;
    private bool hasScared;

    // Use this for initialization
    void Start ()
    {
        mainCam = Camera.main;

        // Turn ghost renderer  off
        ghost.GetComponent<Renderer>().enabled = false;

        hasScared = false;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RaycastHit hit;
            Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
            Debug.DrawRay(mainCam.transform.position, fwd);
            if (Physics.Raycast(mainCam.transform.position, fwd, out hit) && !hasScared)
            {
                if (hit.collider.tag == "Ghost")
                {
                    hasScared = true;
                    StartCoroutine("Scare01");
                }
            }
        }
    }

    // coroutine for scare01
    IEnumerator Scare01()
    {
        yield return new WaitForSeconds(0.5f);

        // turn light off
        float initialIntensity = spotlight.intensity;
        spotlight.intensity = 0;

        // enable ghost renderer
        ghost.GetComponent<Renderer>().enabled = true;

        yield return new WaitForSeconds(2.5f);

        // disable ghost renderer
        ghost.GetComponent<Renderer>().enabled = false;

        // turn light on
        spotlight.intensity = initialIntensity;

        yield return new WaitForSeconds(7.0f);

        hasScared = false;
    }

    void MoveWhenNotVisible()
    {
        if (!ghost.GetComponent<Renderer>().enabled)
        {
            ghost.transform.position = new Vector3(1f, 1f, 1f);
        }
    }
}
