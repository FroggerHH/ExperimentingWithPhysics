namespace ExperimentingWithPhysics;

public class DragRigidbody : MonoBehaviour
{
    public float force = 600;
    public float damping = 6;
    private float dragDepth;

    private Transform jointTrans;

    private void OnMouseDown() { HandleInputBegin(Input.mousePosition); }

    private void OnMouseDrag() { HandleInput(Input.mousePosition); }

    private void OnMouseUp() { HandleInputEnd(Input.mousePosition); }

    public void HandleInputBegin(Vector3 screenPosition)
    {
        var ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            dragDepth = CameraPlane.CameraToPointDepth(Camera.main, hit.point);
            jointTrans = AttachJoint(hit.rigidbody, hit.point);
        }
    }

    public void HandleInput(Vector3 screenPosition)
    {
        if (jointTrans == null)
            return;
        var worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        jointTrans.position = CameraPlane.ScreenToWorldPlanePoint(Camera.main, dragDepth, screenPosition);
    }

    public void HandleInputEnd(Vector3 screenPosition)
    {
        if (jointTrans != null) Destroy(jointTrans.gameObject);
    }

    private Transform AttachJoint(Rigidbody rb, Vector3 attachmentPosition)
    {
        var go = new GameObject("Attachment Point");
        go.hideFlags = HideFlags.HideInHierarchy;
        go.transform.position = attachmentPosition;

        var newRb = go.AddComponent<Rigidbody>();
        newRb.isKinematic = true;

        var joint = go.AddComponent<ConfigurableJoint>();
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
        joint.connectedBody = rb;
        joint.configuredInWorldSpace = true;
        joint.xDrive = NewJointDrive(force, damping);
        joint.yDrive = NewJointDrive(force, damping);
        joint.zDrive = NewJointDrive(force, damping);
        //joint.slerpDrive = NewJointDrive(force, damping);
        //joint.rotationDriveMode = RotationDriveMode.Slerp;

        return go.transform;
    }

    private JointDrive NewJointDrive(float force, float damping)
    {
        var drive = new JointDrive();
        drive.mode = JointDriveMode.Position;
        drive.positionSpring = force;
        drive.positionDamper = damping;
        drive.maximumForce = Mathf.Infinity;
        return drive;
    }
}

public static class CameraPlane
{
    /// <summary>
    ///     Returns world space position at a given viewport coordinate for a given depth.
    /// </summary>
    public static Vector3 ViewportToWorldPlanePoint(Camera theCamera, float zDepth, Vector2 viewportCord)
    {
        var angles = ViewportPointToAngle(theCamera, viewportCord);
        var xOffset = Mathf.Tan(angles.x) * zDepth;
        var yOffset = Mathf.Tan(angles.y) * zDepth;
        var cameraPlanePosition = new Vector3(xOffset, yOffset, zDepth);
        cameraPlanePosition = theCamera.transform.TransformPoint(cameraPlanePosition);
        return cameraPlanePosition;
    }

    public static Vector3 ScreenToWorldPlanePoint(Camera camera, float zDepth, Vector3 screenCoord)
    {
        var point = Camera.main.ScreenToViewportPoint(screenCoord);
        return ViewportToWorldPlanePoint(camera, zDepth, point);
    }

    /// <summary>
    ///     Returns X and Y frustum angle for the given camera representing the given viewport space coordinate.
    /// </summary>
    public static Vector2 ViewportPointToAngle(Camera cam, Vector2 ViewportCord)
    {
        var adjustedAngle = AngleProportion(cam.fieldOfView / 2, cam.aspect) * 2;
        var xProportion = (ViewportCord.x - .5f) / .5f;
        var yProportion = (ViewportCord.y - .5f) / .5f;
        var xAngle = AngleProportion(adjustedAngle / 2, xProportion) * Mathf.Deg2Rad;
        var yAngle = AngleProportion(cam.fieldOfView / 2, yProportion) * Mathf.Deg2Rad;
        return new Vector2(xAngle, yAngle);
    }

    /// <summary>
    ///     Distance between the camera and a plane parallel to the viewport that passes through a given point.
    /// </summary>
    public static float CameraToPointDepth(Camera cam, Vector3 point)
    {
        var localPosition = cam.transform.InverseTransformPoint(point);
        return localPosition.z;
    }

    public static float AngleProportion(float angle, float proportion)
    {
        var oppisite = Mathf.Tan(angle * Mathf.Deg2Rad);
        var oppisiteProportion = oppisite * proportion;
        return Mathf.Atan(oppisiteProportion) * Mathf.Rad2Deg;
    }
}