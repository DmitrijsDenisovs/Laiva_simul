using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RouteContoller : MonoBehaviour
{
    public GameObject robot;

    private GameObject[] _yellowBuoys;
    private GameObject[] _redBuoys;
    private GameObject[] _greenBuoys;

    private const float _evadeRadius = 2.5f;
    // Start is called before the first frame update
    void Start()
    {
        _yellowBuoys = GameObject.FindGameObjectsWithTag("Yellow Buoy");
        _redBuoys = GameObject.FindGameObjectsWithTag("Red Buoy");
        _greenBuoys = GameObject.FindGameObjectsWithTag("Green Buoy");

        var greenRedPairs = new Tuple<GameObject, GameObject>[_greenBuoys.Length];
        int k = 0;
        foreach (var greenBuoy in _greenBuoys)
        {

            float oldDistance = float.PositiveInfinity;

            GameObject nearestRedBuoy = _redBuoys[0];

            foreach (var redBuoy in _redBuoys)
            {
                float dist = Vector3.Distance(greenBuoy.transform.position, redBuoy.transform.position);
                if (dist < oldDistance)
                {
                    nearestRedBuoy = redBuoy;
                    oldDistance = dist;
                }
            }

            greenRedPairs[k++] = new Tuple<GameObject, GameObject>(greenBuoy, nearestRedBuoy);
        }

        if (RobotController.trajectory == null)
        {
            RobotController.trajectory = new List<Vector3>();
        }

        List<Vector3> points = new List<Vector3>();

        foreach (var greenRedPair in greenRedPairs)
        {
            Debug.DrawLine(greenRedPair.Item1.transform.position, greenRedPair.Item2.transform.position, UnityEngine.Color.magenta);
            Vector3 mean = (greenRedPair.Item1.transform.position + greenRedPair.Item2.transform.position) / 2;
            points.Add(new Vector3(mean.x, 0, mean.z));
        }

        List<Vector3> evadingPoints = new List<Vector3>();
        points = points.OrderBy(point => Vector3.Distance(robot.transform.position, point)).ToList();

        for (int i = 0; i < points.Count; ++i)
        {
            evadingPoints.Add(points.ElementAt(i));
            if (i + 1 < points.Count)
            {
                foreach (GameObject yellowBuoy in _yellowBuoys)
                {
                    //https://mathworld.wolfram.com/Circle-LineIntersection.html
                    //Vector2 normalizedA = new Vector2(points.ElementAt(i).x - yellowBuoy.transform.position.x, points.ElementAt(i).z - yellowBuoy.transform.position.z);
                    //Vector2 normalizedB = new Vector2(points.ElementAt(i + 1).x - yellowBuoy.transform.position.x, points.ElementAt(i + 1).z - yellowBuoy.transform.position.z);
                    //float distanceR = Mathf.Sqrt(Mathf.Pow(normalizedB.x - normalizedA.x, 2) + Mathf.Pow(normalizedB.y - normalizedA.y, 2));
                    //float determinant = normalizedA.x * normalizedB.y - normalizedB.x * normalizedA.y;

                    //float intersectionDiscriminant = Mathf.Pow(_evadeRadius, 2) * Mathf.Pow(distanceR, 2) - determinant;

                    if ((yellowBuoy.transform.position.x > points.ElementAt(i).x && yellowBuoy.transform.position.x < points.ElementAt(i + 1).x
                        && yellowBuoy.transform.position.z > points.ElementAt(i).z && yellowBuoy.transform.position.z < points.ElementAt(i + 1).z)
                        ||
                        (yellowBuoy.transform.position.x < points.ElementAt(i).x && yellowBuoy.transform.position.x > points.ElementAt(i + 1).x
                        && yellowBuoy.transform.position.z < points.ElementAt(i).z && yellowBuoy.transform.position.z > points.ElementAt(i + 1).z)
                        ||
                        (yellowBuoy.transform.position.x > points.ElementAt(i).x && yellowBuoy.transform.position.x < points.ElementAt(i + 1).x
                        && yellowBuoy.transform.position.z < points.ElementAt(i).z && yellowBuoy.transform.position.z > points.ElementAt(i + 1).z)
                        ||
                        (yellowBuoy.transform.position.x < points.ElementAt(i).x && yellowBuoy.transform.position.x > points.ElementAt(i + 1).x
                        && yellowBuoy.transform.position.z > points.ElementAt(i).z && yellowBuoy.transform.position.z < points.ElementAt(i + 1).z))
                    //if (intersectionDiscriminant >= 0)
                    {
                        Vector3 robotToBuoy = yellowBuoy.transform.position - robot.transform.position;
                        robotToBuoy.y = 0;
                        robotToBuoy = robotToBuoy.normalized;
                        Vector3 evadePointRight = yellowBuoy.transform.position + (Quaternion.Euler(0, 90, 0) * robotToBuoy * _evadeRadius);
                        Vector3 evadePointLeft = yellowBuoy.transform.position + (Quaternion.Euler(0, -90, 0) * robotToBuoy * _evadeRadius);

                        evadePointRight.y = 0;
                        evadePointLeft.y = 0;

                        var evadePoint = evadePointLeft;
                        if (Vector3.Distance(points.ElementAt(i + 1), evadePointLeft) > Vector3.Distance(points.ElementAt(i + 1), evadePointLeft))
                        {
                            evadePoint = evadePointRight;
                        }
                        evadingPoints.Add(evadePoint);
                    }
                }
            }
        }
        foreach (var point in evadingPoints)
        {
            RobotController.trajectory.Add(point);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
