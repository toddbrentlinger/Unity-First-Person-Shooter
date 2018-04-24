using UnityEngine;

public class WeaponBobOld : MonoBehaviour {

    private float timer = 0f;
    [SerializeField]
    private float bobbingMaxSpeed = .18f;
    [SerializeField]
    private float bobbingAmount = .2f;
    private float midpoint;

    private void Awake()
    {
        midpoint = transform.localPosition.y;
    }

    private void Update()
    {
        float waveslice = 0f;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
            timer = 0f;
        else
        {
            waveslice = Mathf.Sin(timer);
            timer = timer + bobbingMaxSpeed;
            if (timer > Mathf.PI * 2)
                timer = timer - (Mathf.PI * 2);
        }
        if (waveslice != 0)
        {
            float translateChange = waveslice * bobbingAmount;
            float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            totalAxes = Mathf.Clamp(totalAxes, 0f, 1f);
            translateChange = totalAxes * translateChange;
            transform.localPosition = new Vector3(transform.localPosition.x,
                midpoint + translateChange,
                transform.localPosition.z);
        }
        else
        {
            transform.localPosition = new Vector3(transform.localPosition.x,
                midpoint,
                transform.localPosition.z);
        }
    }
}
