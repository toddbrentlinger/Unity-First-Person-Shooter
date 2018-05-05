using UnityEngine;

public class PerlinNoiseHeightChange : MonoBehaviour
{
    public float heightScale = 2f;
    public float xScale = 1f;

    private float m_startHeight;
    private float m_minHeight;
    private float m_maxHeight;

    private void Start()
    {
        m_startHeight = transform.position.y;
        m_minHeight = m_maxHeight = .5f * heightScale;
    }

    private void Update()
    {
        SetHeight();
    }

    private void SetHeight()
    {
        float height = heightScale * Mathf.PerlinNoise(Time.time * xScale, 0f);
        Vector3 pos = transform.position;
        pos.y = m_startHeight + height;
        transform.position = pos;

        //CheckMinMax(height);

        //Debug.Log("Height: " + height + " - Min(" + m_minHeight + ") - Max(" + m_maxHeight + ")");
    }

    private void CheckMinMax(float height)
    {
        if (height < m_minHeight)
            m_minHeight = height;
        else if (height > m_maxHeight)
            m_maxHeight = height;
    }
}
