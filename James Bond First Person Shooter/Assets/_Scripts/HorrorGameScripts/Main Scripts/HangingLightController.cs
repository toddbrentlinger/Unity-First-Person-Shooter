using UnityEngine;
using System.Collections;

public class HangingLightController : MonoBehaviour {

    [SerializeField] private float maxDistance = 2.0f;
    [SerializeField] private Light lightSource;
    [SerializeField] private AudioClip chainPullOnClip;
    [SerializeField] private AudioClip chainPullOffClip;
    [SerializeField] private AudioClip lightBulbShatterClip;

    [SerializeField] private GameObject m_lightBulb;
    [SerializeField] private Rigidbody m_pullStringRigidbody;
    private AudioSource audioSource;

    private Material m_lightMaterial;
    private Color m_initialLightShaderEmission;
    private bool m_enter;
    private bool lightOn = true;

    void Awake()
    {

    }

	// Use this for initialization
	void Start ()
    {
        m_lightMaterial = m_lightBulb.GetComponent<Renderer>().material;
        m_initialLightShaderEmission = m_lightMaterial.GetColor("_EmissionColor");
        audioSource = gameObject.GetComponent<AudioSource>();
        ToggleLight();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!m_enter)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                if (hit.collider.GetComponent<Rigidbody>() == m_pullStringRigidbody)
                {
                    lightOn = !lightOn;
                    StartCoroutine("ToggleLight");
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            m_enter = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            m_enter = false;
    }

    IEnumerator ToggleLight()
    {
        if (lightOn)
            audioSource.PlayOneShot(chainPullOnClip);
        else
            audioSource.PlayOneShot(chainPullOffClip);

        // Wait to time audioClip with the animation of light toggling on/off 
        yield return new WaitForSeconds(0.25f);

        if (lightOn)
        {
            m_lightMaterial.SetColor("_EmissionColor", m_initialLightShaderEmission);
            lightSource.enabled = true;
        }
        else
        {
            m_lightMaterial.SetColor("_EmissionColor", Color.black);
            lightSource.enabled = false;
        }
    }

    public void SetLightOn(bool choice)
    {
        lightOn = choice;
        StartCoroutine("ToggleLight");
    }

    public bool GetLightOn()
    {
        return lightOn;
    }

    public void ShatterLight()
    {
        lightOn = false;
        audioSource.PlayOneShot(lightBulbShatterClip);
        if (lightOn)
        {
            m_lightMaterial.SetColor("_EmissionColor", m_initialLightShaderEmission);
            lightSource.enabled = true;
        }
        else
        {
            m_lightMaterial.SetColor("_EmissionColor", Color.black);
            lightSource.enabled = false;
        }
    }

}
