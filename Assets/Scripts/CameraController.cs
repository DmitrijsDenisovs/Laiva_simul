using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{

    public GameObject robotBody;
    public GameObject mainCamera;

    public float distance = 5.0f;

    private float angleX = 30;
    private float angleY = 90;
    private float angleZ = 0;

    private const  float ANGLE_ROTATION_SPEED = 60.0f;
    private const float DISTANCE_CHANGE_SPEED = 35.0f;
    private const float MOUSE_SPEED_MULTIPLIER = 15.0f;
    private const float MAX_DISTANCE = 30.0f;
    private const float MIN_DISTANCE = 1.5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton((int) MouseButton.Left))
        {
            angleY -= Input.GetAxis("Mouse X") * ANGLE_ROTATION_SPEED * MOUSE_SPEED_MULTIPLIER * Time.deltaTime;
            angleX -= Input.GetAxis("Mouse Y") * ANGLE_ROTATION_SPEED * MOUSE_SPEED_MULTIPLIER * Time.deltaTime;
        }
        else
        {
            angleY -= Input.GetAxis("Horizontal") * ANGLE_ROTATION_SPEED * Time.deltaTime;
            angleX += Input.GetAxis("Vertical") * ANGLE_ROTATION_SPEED * Time.deltaTime;
        }

        distance -= Input.mouseScrollDelta.y * DISTANCE_CHANGE_SPEED * Time.deltaTime;

        if (distance < MIN_DISTANCE)
        {
            distance = MIN_DISTANCE;
        }
        else if (distance > MAX_DISTANCE)
        {
            distance = MAX_DISTANCE;
        }

        if (Input.GetMouseButtonDown((int)MouseButton.Right))
        {
            Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                RobotController.trajectory.Add(new Vector3(hit.point.x, 0, hit.point.z));
            }
        }
    }

    void LateUpdate()
    {
        Quaternion rotation = Quaternion.Euler(angleX, angleY, angleZ);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + robotBody.transform.position;
        mainCamera.transform.SetPositionAndRotation(position, rotation);
    }
}
