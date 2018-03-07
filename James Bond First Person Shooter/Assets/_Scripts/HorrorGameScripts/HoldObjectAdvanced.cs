using UnityEngine;
using System.Collections;

public class HoldObjectAdvanced : MonoBehaviour {

    private float maxDistance = 3.0f;
    private float distFromPlayer = 1.0f;

    private float spring = 10.0f; // 50
    private float damper = 1.0f; // 5
    private float distance = 0.2f;

    private float drag = 1.0f; // 10
    private float angularDrag = 5.0f;

    private float throwForce = 1.0f; // 50

    private bool attachCenterOfMass = true;
    private ConfigurableJoint configJoint;
    private Vector3 relVecFromCamToJoint;

    private Camera mainCamera;
    private Collider playerCollider;
    private Collider grabObjectCollider;

    private Ray ray;
    private RaycastHit hit;

    void Start()
    {
        mainCamera = Camera.main;

        //if (gameObject.transform.parent.GetComponentInChildren(typeof(ConfigurableJoint)))
        //    configJoint = gameObject.transform.parent.GetComponentInChildren(typeof(ConfigurableJoint)) as ConfigurableJoint;

        relVecFromCamToJoint = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.3f, distFromPlayer))
            - mainCamera.transform.position;

        playerCollider = gameObject.GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
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

        // Set up ConfigurableJoint

        if (!configJoint)
        {
            GameObject go = new GameObject("Rigidbody dragger") as GameObject;
            Rigidbody body = go.AddComponent<Rigidbody>() as Rigidbody;
            body.isKinematic = true;
            configJoint = go.AddComponent<ConfigurableJoint>() as ConfigurableJoint;
            go.transform.SetParent(mainCamera.transform);

            // ConfigurableJoint parameters
            // configJoint.axis = new Vector3(1.0f, 1.0f, 1.0f);

            configJoint.xMotion = ConfigurableJointMotion.Limited;
            configJoint.yMotion = ConfigurableJointMotion.Limited;
            configJoint.zMotion = ConfigurableJointMotion.Locked;

            configJoint.angularXMotion = ConfigurableJointMotion.Free;
            configJoint.angularYMotion = ConfigurableJointMotion.Free;
            configJoint.angularZMotion = ConfigurableJointMotion.Free;

            /*
            SoftJointLimit jointLimit = new SoftJointLimit();
            jointLimit.limit = distance;
            configJoint.linearLimit = jointLimit;
            */

            SoftJointLimitSpring limitSpring = new SoftJointLimitSpring();
            limitSpring.spring = spring;
            limitSpring.damper = damper;
            configJoint.linearLimitSpring = limitSpring;

            /*
            JointDrive xDrive = new JointDrive();
            xDrive.positionSpring = spring;
            xDrive.positionDamper = damper;
            configJoint.xDrive = xDrive;

            JointDrive yDrive = new JointDrive();
            yDrive.positionSpring = spring;
            yDrive.positionDamper = damper;
            configJoint.yDrive = yDrive;
            */
        }

        configJoint.transform.position = mainCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 0.3f, distFromPlayer));

        configJoint.transform.LookAt(mainCamera.transform.position);

        hit.rigidbody.transform.position = configJoint.transform.position;
        configJoint.connectedBody = hit.collider.gameObject.GetComponent<Rigidbody>();

        if (attachCenterOfMass)
        {
            Vector3 anchor = transform.TransformDirection(hit.rigidbody.centerOfMass)
                + hit.rigidbody.transform.position;
            anchor = configJoint.transform.InverseTransformPoint(anchor);
            configJoint.anchor = anchor;
        }
        else
            configJoint.anchor = Vector3.zero;

        grabObjectCollider = hit.collider.GetComponent<Collider>();

        StartCoroutine("DragObject");
    }

    IEnumerator DragObject()
    {
        // Ignore collisions between object grabbed and player
        Physics.IgnoreCollision(grabObjectCollider, playerCollider);

        float oldDrag = configJoint.connectedBody.drag;
        float oldAngularDrag = configJoint.connectedBody.angularDrag;
        configJoint.connectedBody.GetComponent<Rigidbody>().useGravity = false;
        configJoint.connectedBody.drag = drag;
        configJoint.connectedBody.angularDrag = angularDrag;
        Camera mainCamera = Camera.main;

        while (Input.GetMouseButton(0) && configJoint.connectedBody)
        {
            configJoint.transform.position = mainCamera.ViewportToWorldPoint(
                new Vector3(0.5f, 0.3f, distFromPlayer));

            configJoint.transform.LookAt(mainCamera.transform);

            yield return null;

            if (Input.GetMouseButton(1))
            {
                ThrowObject();
                configJoint.connectedBody.GetComponent<Rigidbody>().useGravity = true;
                configJoint.connectedBody.drag = oldDrag;
                configJoint.connectedBody.angularDrag = oldAngularDrag;
                configJoint.connectedBody = null;
            }
        }

        if (configJoint.connectedBody != null)
        {
            configJoint.connectedBody.GetComponent<Rigidbody>().velocity = Vector3.zero;
            configJoint.connectedBody.GetComponent<Rigidbody>().useGravity = true;
            configJoint.connectedBody.drag = oldDrag;
            configJoint.connectedBody.angularDrag = oldAngularDrag;
            configJoint.connectedBody = null;
        }

        // Allow collisions between object grabbed and player
        Physics.IgnoreCollision(grabObjectCollider, playerCollider, false);
    }

    void ThrowObject()
    {
        Vector3 connectedBodyTransform = configJoint.connectedBody.transform.position;
        Vector3 mainCamTransform = Camera.main.transform.position;
        Vector3 directionVector = (connectedBodyTransform - mainCamTransform).normalized;
        configJoint.connectedBody.AddForce(directionVector * throwForce, ForceMode.Impulse);
    }
}