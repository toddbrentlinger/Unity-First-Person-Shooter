using UnityEngine;

public class RigidbodyVelocityColorChange : MonoBehaviour {

    [SerializeField] private float m_maxSpeed = 5f;
    [SerializeField] private Color m_maxColor = Color.red;
    [SerializeField] private Color m_minColor = Color.blue;

    private Rigidbody m_rigidbody;
    private Renderer m_renderer;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_renderer = GetComponent<Renderer>();
    }

	// Use this for initialization
	void Start () {
        ChangeColor();
	}
	
	// Update is called once per frame
	private void Update ()
    {
        //Debug.Log("Velocity: " + m_rigidbody.velocity.magnitude + " - isSleeping: " + m_rigidbody.IsSleeping());

        if (!m_rigidbody.IsSleeping())
        {
            ChangeColor();
        }
	}

    private void ChangeColor()
    {
        float interpolationFactor = Mathf.InverseLerp(0f, m_maxSpeed * m_maxSpeed, m_rigidbody.velocity.sqrMagnitude);
        interpolationFactor = Mathf.Clamp01(interpolationFactor);
        m_renderer.material.color = Color.Lerp(m_minColor, m_maxColor, interpolationFactor);
    }
}
