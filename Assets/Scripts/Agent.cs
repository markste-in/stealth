using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class Agent : MonoBehaviour
{
    public static event Action OnGuardHasSpottedPlayer;
    public Transform Pathholder;
    private Transform LookingFor;
    public Light spotlight;

    [Range(0.1f, 50.0f)] public float viewDistance;

    [Range(0.1f, 50.0f)] public float speed = 5f;

    [Range(0.1f, 50.0f)] public float RotateToTargetSpeed = 5f;

    [Range(0f, 5f)] public float waitOnWayPoint = 0.1f;

    public float TimeUntilSpotted = 1f;
    private float spotAngle;
    private Vector3[] waypoints;
    private Color originalSpotlightColor;
    private float timeSpotted;

    private void Start()
    {
        
        LookingFor = FindObjectOfType<Player>().transform;
        spotAngle = spotlight.spotAngle;
        originalSpotlightColor = spotlight.color;
        waypoints = new Vector3[Pathholder.childCount];
        for (int i = 0; i < Pathholder.childCount; i++)
        {
            waypoints[i] = new Vector3(Pathholder.GetChild(i).position.x, transform.position.y,
                Pathholder.GetChild(i).position.z);
        }

        transform.position = waypoints[0];

        StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        yield return new WaitForSeconds(1f);
        int nextTarget = 1;
        while (true)
        {
            yield return RotateTowardsTarget(waypoints[nextTarget]);
            yield return Move(waypoints[nextTarget]);
            yield return new WaitForSeconds(waitOnWayPoint);

            nextTarget++;
            nextTarget %= waypoints.Length;
        }
    }

    IEnumerator Move(Vector3 position)
    {
        while ((transform.position - position).magnitude > 0.1)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, speed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator RotateTowardsTarget(Vector3 target)
    {
        float targetAngle;


        do
        {
            Vector3 targetDirection = (target - transform.position).normalized;
            Vector3 RotDir = Vector3.Cross(transform.forward, targetDirection).normalized;
            float TargetAngleRad = Mathf.Clamp(Vector3.Dot(transform.forward, targetDirection), -1f, 1f);
            targetAngle = Mathf.Acos(TargetAngleRad) * Mathf.Rad2Deg;
            float reqAngle = Mathf.Clamp(RotateToTargetSpeed * targetAngle, 15, 5000);
            transform.Rotate(reqAngle * Time.deltaTime * RotDir);
            yield return null;
        } while (Mathf.Abs(targetAngle) > 2f);
    }

    private void OnDrawGizmos()
    {
        // Waypoints
        Vector3 startPosition = Pathholder.GetChild(0).position;
        Vector3 prevPosition = startPosition;
        foreach (Transform waypoint in Pathholder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(prevPosition, waypoint.position);
            prevPosition = waypoint.position;
        }

        Gizmos.DrawLine(prevPosition, startPosition);

        // Forward Direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * viewDistance);

    }

    private void Update()
    {
        if (CheckIfPlayerVisible())
        {
            timeSpotted += Time.deltaTime;
            Debug.DrawRay(transform.position, LookingFor.position - transform.position, Color.red);
            spotlight.color = Color.red;
            if (timeSpotted > TimeUntilSpotted)
            {
                Debug.Log("SPOTTED");
                if (OnGuardHasSpottedPlayer != null)
                    OnGuardHasSpottedPlayer();
            }        
        }
        else

        {
            timeSpotted = 0;
            spotlight.color = originalSpotlightColor;
            Debug.DrawRay(transform.position, LookingFor.position - transform.position, Color.green);
        }
    }

    private bool CheckIfPlayerVisible()
    {
        Vector3 directionToPlayer = LookingFor.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if ((angleToPlayer <= (spotAngle / 2f)) && (distanceToPlayer <= viewDistance))
        {
            Ray ray = new Ray(transform.position, directionToPlayer);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, viewDistance))
                if (hitInfo.collider.tag == "Player")
                    return true;
        }

        return false;
    }
    
}
