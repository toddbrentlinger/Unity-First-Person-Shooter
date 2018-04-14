using UnityEngine;

public class AimSway : MonoBehaviour {

    [SerializeField] private float m_maxRotationX = 5f;
    [SerializeField] private float m_maxRotationY = 5f;
    [SerializeField] private float m_maxVirtualAxis = 3f;
    [SerializeField] private float m_minVirtualAxis = 0f;
    [SerializeField] private float m_smoothTime = 5f;

    //private Vector3 m_originalWeaponPosition;
    //private Quaternion m_originalWeaponRotation;
    private Quaternion m_originalWeaponLocalRotation;
    private Quaternion m_targetRotation;
    //private Vector3 m_originalRotationEuler;
    //private Vector3 m_originalLocalRotationEuler;
    //private Quaternion m_currentWeaponRotation;
    //private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
       // m_originalWeaponPosition = transform.position;
        //m_originalWeaponRotation = transform.rotation;
        m_originalWeaponLocalRotation = transform.localRotation;

        //m_originalRotationEuler = transform.rotation.eulerAngles;
        //m_originalLocalRotationEuler = transform.localRotation.eulerAngles;

        //m_currentWeaponRotation = m_originalWeaponRotation;
    }
	
	// Update is called once per frame
	private void Update ()
    {
        SmoothAimSway();
        //QuickAimSway();
	}

    private void SmoothAimSway()
    {
        float horizontal = Input.GetAxis("Mouse X");
        float vertical = -Input.GetAxis("Mouse Y");

        float horizontalFactor = 0f;
        float verticalFactor = 0f;
        if (horizontal > 0)
            horizontalFactor = Mathf.InverseLerp(m_minVirtualAxis, m_maxVirtualAxis, horizontal);
        else if (horizontal < 0)
            horizontalFactor = -Mathf.InverseLerp(m_minVirtualAxis, m_maxVirtualAxis, -horizontal);
        if (vertical > 0)
            verticalFactor = Mathf.InverseLerp(m_minVirtualAxis, m_maxVirtualAxis, vertical);
        else if (vertical < 0)
            verticalFactor = -Mathf.InverseLerp(m_minVirtualAxis, m_maxVirtualAxis, -vertical);

        m_targetRotation = m_originalWeaponLocalRotation * Quaternion.Euler(verticalFactor * m_maxRotationY, horizontalFactor * m_maxRotationX, 0);

        //transform.localRotation = m_originalWeaponRotation * Quaternion.Euler(verticalFactor * m_maxRotationY, horizontalFactor * m_maxRotationX, 0);
        //transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * m_smoothFactor);
        //transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, Time.deltaTime * m_smoothFactor);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, m_targetRotation, Time.deltaTime * m_smoothTime);

        //Debug.Log("Horiz: " + horizontal + "(" + horizontalFactor + ") - Vert: " + vertical + "(" + verticalFactor + ") - LocalRotation: " + m_originalWeaponLocalRotation.eulerAngles + " - TargetRotation: " + targetRotation.eulerAngles);
        //Debug.Log("SlerpT01: " + Time.deltaTime * m_smoothFactor + " - Horizontal: " + (horizontalFactor * m_maxRotationX) + " - Vertical: "+ (verticalFactor * m_maxRotationY));
    }

    private void QuickAimSway()
    {
        float horizontal = Input.GetAxis("Mouse X");
        float vertical = -Input.GetAxis("Mouse Y");
        float returnSmoothFactor = 5f;

        float horizontalFactor, verticalFactor, horizontalSmooth, verticalSmooth;

        horizontalSmooth = (horizontal == 0) ? returnSmoothFactor : m_smoothTime;
        if (horizontal > 0)
            horizontalFactor = Mathf.InverseLerp(m_minVirtualAxis, m_maxVirtualAxis, horizontal);
        else if (horizontal < 0)
            horizontalFactor = -Mathf.InverseLerp(m_minVirtualAxis, m_maxVirtualAxis, -horizontal);
        else
            horizontalFactor = 0;

        verticalSmooth = (vertical == 0) ? returnSmoothFactor : m_smoothTime;
        if (vertical > 0)
            verticalFactor = Mathf.InverseLerp(m_minVirtualAxis, m_maxVirtualAxis, vertical);
        else if (vertical < 0)
            verticalFactor = -Mathf.InverseLerp(m_minVirtualAxis, m_maxVirtualAxis, -vertical);
        else
            verticalFactor = 0;

        Quaternion targetRotation = m_originalWeaponLocalRotation * Quaternion.Euler(verticalFactor, horizontalFactor, 0);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * m_smoothTime);

        //Debug.Log("Time.deltaTime * m_smoothFactor = " + Time.deltaTime * m_smoothTime);
    }
}
