using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;
using Utils;

public class RobotController : MonoBehaviour
{
    //data classes/models
    protected class Speeds
    {
        public float leftThrust;
        public float rightThrust;
    }

    public const float MAX_SPEED = 5;
    public const float LOCATION_PRECISION = 0.5f;
    public const float THRUST_SENSITIVITY = 5.5f;
    public const float ANGLE_PRECISION_RAD = 0.02f;
    public const float DISTANCE_SENSITIVITY = 1;

    public GameObject leftThrust;
    public GameObject rightThrust;

    public GameObject body;

    public static List<Vector3> trajectory = new List<Vector3>();
    public static List<Vector3> historicalTrajectory = new List<Vector3>();
    public static List<Vector3> actualTrajectory = new List<Vector3>();

    private PidController _leftAnglePid = new PidController
    {
        OutputMax = MAX_SPEED,
        OutputMin = -MAX_SPEED,
        ProportionalGain = 10f,
        DifferentialGain = 0.0001f,
        IntegralGain = 0.5f,
        Target = 0,
    };

    private PidController _leftDistancePid = new PidController
    {
        OutputMax = MAX_SPEED,
        OutputMin = -MAX_SPEED,
        ProportionalGain = 10f,
        DifferentialGain = 0.0001f,
        IntegralGain = 0.5f,
        Target = 0,
    };

    private PidController _rightAnglePid = new PidController
    {
        OutputMax = MAX_SPEED,
        OutputMin = -MAX_SPEED,
        ProportionalGain = 10f,
        DifferentialGain = 0.1f,
        IntegralGain = 0.5f,
        Target = 0,
    };

    private PidController _rightDistancePid = new PidController
    {
        OutputMax = MAX_SPEED,
        OutputMin = -MAX_SPEED,
        ProportionalGain = 10f,
        DifferentialGain = 0.1f,
        IntegralGain = 0.5f,
        Target = 0,
    };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3? target = trajectory.Any() ? trajectory.First() - body.transform.position : null;
        actualTrajectory.Add(body.transform.position);
        if (target != null)
        {
            TraceTrajectory(trajectory, Color.cyan);
            TraceTrajectory(historicalTrajectory, Color.cyan);

            Debug.DrawRay(body.transform.position, body.transform.forward, Color.green);
            Debug.DrawLine(body.transform.position, trajectory.First(), Color.red);

            float angle = Mathf.Deg2Rad * Vector3.Angle(body.transform.forward, target.Value) 
                * Mathf.Sign(target.Value.x * body.transform.forward.z - target.Value.z * body.transform.forward.x);

            float distance = target.Value.magnitude;

            Speeds speeds = new Speeds { 
                leftThrust = -_leftDistancePid.GetValue(distance, Time.time) - _leftAnglePid.GetValue(angle, Time.time), 
                rightThrust = -_rightDistancePid.GetValue(distance, Time.time) + _rightAnglePid.GetValue(angle, Time.time) };

            Debug.Log("Angle to target: " + angle.ToString());
            //if (Math.Abs(angle) > ANGLE_PRECISION_RAD)
            //{       

            //    speeds.leftThrust += angle * THRUST_SENSITIVITY;
            //    speeds.rightThrust -= angle * THRUST_SENSITIVITY;
                

            //    speeds.rightThrust = speeds.rightThrust > MAX_SPEED ? MAX_SPEED : speeds.rightThrust;
            //    speeds.leftThrust = speeds.leftThrust > MAX_SPEED ? MAX_SPEED : speeds.leftThrust;

            //    //speeds.rightThrust = speeds.rightThrust < 0 ? 0 : speeds.rightThrust;
            //    //speeds.leftThrust = speeds.leftThrust < 0 ? 0 : speeds.leftThrust;
            //}
            //else
            //{
            //    Debug.Log("Going straight");
            //}

            Debug.Log("Applying speeds: L:" + speeds.leftThrust.ToString() + "; R:" + speeds.rightThrust.ToString());
            if ((trajectory.First() - body.transform.position).magnitude < LOCATION_PRECISION)
            {
                Debug.Log("Target reached");
                historicalTrajectory.Add(trajectory.ElementAt(0));
                trajectory.RemoveAt(0);

                _leftAnglePid.WindupIntegral();
                _rightAnglePid.WindupIntegral();

                _rightDistancePid.WindupIntegral();
                _leftDistancePid.WindupIntegral();

                ApplySpeeds(new Speeds { leftThrust = 0, rightThrust = 0 });
            }
            ApplySpeeds(speeds);
        }

        TraceTrajectory(trajectory, Color.cyan);
        TraceTrajectory(historicalTrajectory, Color.cyan);
        TraceTrajectory(actualTrajectory, Color.black);
    }

    protected void ApplySpeeds(Speeds speeds)
    {
        leftThrust.GetComponent<Rigidbody>().velocity = new Vector3(leftThrust.transform.forward.x, 0, leftThrust.transform.forward.z) * speeds.leftThrust * leftThrust.transform.forward.magnitude;
        rightThrust.GetComponent<Rigidbody>().velocity = new Vector3(rightThrust.transform.forward.x, 0, rightThrust.transform.forward.z) * speeds.rightThrust * rightThrust.transform.forward.magnitude;
        //leftThrust.GetComponent<Rigidbody>().AddForce(0.1f * leftThrust.transform.forward * speeds.leftThrust * leftThrust.transform.forward.magnitude, ForceMode.Impulse);
        //leftThrust.GetComponent<Rigidbody>().AddForce(0.1f * rightThrust.transform.forward * speeds.rightThrust * rightThrust.transform.forward.magnitude, ForceMode.Impulse);
    }

    protected void TraceTrajectory(List<Vector3> trajectory, Color color)
    {
        for (int i = 0; i < trajectory.Count - 1; ++i)
        {
            Debug.DrawLine(trajectory.ElementAt(i), trajectory.ElementAt(i + 1), color);
        }
    }
}

