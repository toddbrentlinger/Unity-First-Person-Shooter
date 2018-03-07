using UnityEngine;
using System.Collections;

public class HoldObject : MonoBehaviour {

    private float maxDistance = 3.0f;
    private float distFromPlayer = 0.8f;

    private float spring = 150.0f; // 50
    private float damper = 5.0f; // 5
    private float distance = 0.0f;

    private float drag = 100.0f; // 10
    private float angularDrag = 5.0f;

    private float throwForce = 0.8f;
    // private float throwRange = 10.0f;

    private bool attachCenterOfMass = true;
    private SpringJoint springJoint;
    private Camera mainCamera;
    private Collider playerCollider;
    private Collider grabObjectCollider;

    private Ray ray;
    private RaycastHit hit;

    // Use this for initialization
    void Start () {
        mainCamera = Camera.main;
        playerCollider = gameObject.GetComponent<Collider>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (!Physics.Raycast(ray, out hit, maxDistance))
            return;

        if (!hit.rigidbody || hit.rigidbody.isKinematic)
            return;

        if (!hit.collider.gameObject.GetComponent<Rigidbody>()
            || hit.collider.gameObject.GetComponent<Rigidbody>().isKinematic)
            return;

        // Set up SpringJoint
        if (!springJoint)
        {
            GameObject go = new GameObject("Rigidbody dragger") as GameObject;
            Rigidbody body = go.AddComponent<Rigidbody>() as Rigidbody;
            body.isKinematic = true;
            springJoint = go.AddComponent<SpringJoint>() as SpringJoint;
            springJoint.transform.SetParent(mainCamera.transform);

            springJoint.spring = spring;
            springJoint.damper = damper;
            springJoint.maxDistance = distance;
        }

        // Move springJoint to front of mainCamera
        springJoint.transform.position = mainCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 0.3f, distFromPlayer));

        // Connect rigidbody to springJoint
        hit.rigidbody.transform.position = springJoint.transform.position;
        springJoint.connectedBody = hit.collider.gameObject.GetComponent<Rigidbody>();
        springJoint.connectedBody.transform.LookAt(mainCamera.transform);

        if (attachCenterOfMass)
        {
            Vector3 anchor = transform.TransformDirection(hit.rigidbody.centerOfMass)
                + hit.rigidbody.transform.position;
            anchor = springJoint.transform.InverseTransformPoint(anchor);
            springJoint.anchor = anchor;
        }
        else
            springJoint.anchor = Vector3.zero;

        grabObjectCollider = hit.collider.GetComponent<Collider>();

        StartCoroutine("DragObject");
	}

    IEnumerator DragObject ()
    {
        // Ignore collisions between object grabbed and player
        Physics.IgnoreCollision(grabObjectCollider, playerCollider);

        float oldDrag = springJoint.connectedBody.drag;
        float oldAngularDrag = springJoint.connectedBody.angularDrag;
        springJoint.connectedBody.GetComponent<Rigidbody>().useGravity = false;
        springJoint.connectedBody.drag = drag;
        springJoint.connectedBody.angularDrag = angularDrag;

        while (Input.GetMouseButton(0) && springJoint.connectedBody)
        {
            springJoint.transform.position = mainCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 0.3f, distFromPlayer));

            // Restrict connectedBody distance from camera to distFromPlayer

            // springJoint.transform.LookAt(mainCamera.transform.position);
            // springJoint.connectedBody.transform.LookAt(mainCamera.transform);

            yield return null;

            if (Input.GetMouseButton (1))
            {
                ThrowObject();
                springJoint.connectedBody.GetComponent<Rigidbody>().useGravity = true;
                springJoint.connectedBody.drag = oldDrag;
                springJoint.connectedBody.angularDrag = oldAngularDrag;
                springJoint.connectedBody = null;
            }
        }

        if (springJoint.connectedBody != null)
        {
            springJoint.connectedBody.GetComponent<Rigidbody>().velocity = Vector3.zero;
            springJoint.connectedBody.GetComponent<Rigidbody>().useGravity = true;
            springJoint.connectedBody.drag = oldDrag;
            springJoint.connectedBody.angularDrag = oldAngularDrag;
            springJoint.connectedBody = null;
        }

        // Allow collisions between object grabbed and player
        Physics.IgnoreCollision(grabObjectCollider, playerCollider, false);
    }

    void ThrowObject()
    {
        Vector3 connectedBodyTransform = springJoint.connectedBody.transform.position;
        Vector3 mainCamTransform = Camera.main.transform.position;
        Vector3 directionVector = (connectedBodyTransform - mainCamTransform).normalized;
        springJoint.connectedBody.AddForce(directionVector * throwForce, ForceMode.Impulse);
    }
}
