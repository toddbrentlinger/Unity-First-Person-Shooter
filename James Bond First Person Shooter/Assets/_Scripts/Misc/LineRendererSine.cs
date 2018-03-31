using UnityEngine;

public class LineRendererSine : MonoBehaviour {

    // Creates a line renderer that follows a Sin() function
    // and animates it.

    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    public int lengthOfLineRenderer = 20;

    private LineRenderer m_lineRenderer;

    void Start()
    {
        m_lineRenderer = gameObject.AddComponent<LineRenderer>();
        m_lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        m_lineRenderer.widthMultiplier = 0.2f;
        m_lineRenderer.positionCount = lengthOfLineRenderer;

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        m_lineRenderer.colorGradient = gradient;
    }

    void Update()
    {
        float t = Time.time;
        for (int i = 0; i < lengthOfLineRenderer; i++)
        {
            Vector3 newPosition = new Vector3(i * 0.5f, Mathf.Sin(i + t), 0.0f);

            m_lineRenderer.SetPosition(i, newPosition + transform.position);
        }
    }
}
