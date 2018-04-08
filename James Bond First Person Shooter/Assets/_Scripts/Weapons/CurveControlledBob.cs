using System;
using UnityEngine;

[Serializable]
public class CurveControlledBob {

    [SerializeField] private float m_horizontalBobRange = 0.33f;
    [SerializeField] private float m_verticalBobRange = 0.33f;
    [SerializeField] private AnimationCurve m_bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                        new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                        new Keyframe(2f, 0f)); // sin curve for head bob
    [SerializeField] private float m_verticaltoHorizontalRatio = 1f;

    private float m_CyclePositionX;
    private float m_CyclePositionY;
    private float m_BobBaseInterval;
    private Vector3 m_OriginalPosition;
    private float m_Time;

    public void Setup (Transform objectToBob, float bobBaseInterval)
    {
        m_BobBaseInterval = bobBaseInterval;
        m_OriginalPosition = objectToBob.localPosition;

        // get the length of the curve in time
        m_Time = m_bobcurve[m_bobcurve.length - 1].time;
    }

    public Vector3 DoObjectBob(float speed)
    {
        float xPos = m_OriginalPosition.x + (m_bobcurve.Evaluate(m_CyclePositionX) * m_horizontalBobRange);
        float yPos = m_OriginalPosition.y + (m_bobcurve.Evaluate(m_CyclePositionY) * m_verticalBobRange);

        m_CyclePositionX += (speed * Time.deltaTime) / m_BobBaseInterval;
        m_CyclePositionY += ((speed * Time.deltaTime) / m_BobBaseInterval) * m_verticaltoHorizontalRatio;

        if (m_CyclePositionX > m_Time)
        {
            m_CyclePositionX = m_CyclePositionX - m_Time;
        }
        if (m_CyclePositionY > m_Time)
        {
            m_CyclePositionY = m_CyclePositionY - m_Time;
        }

        return new Vector3(xPos, yPos, 0f);
    }
}
