using System;
using UnityEngine;

public class SensorWithVisionCone : Sensor
{
    public Vector3 EyeLocation => transform.position;
    public Vector3 EyeDirection => transform.forward;

    [SerializeField] float visionConeAngle = 25.0f; // Vision cone angle in degrees
    [SerializeField] float targetMemoryDuration = 7f; // Time to remember target after losing it

    private CountdownTimer memoryTimer;

    protected override void Start()
    {
        base.Start();
        memoryTimer = new CountdownTimer(targetMemoryDuration);
        memoryTimer.OnTimerStop += ForgetTarget;
    }

    private bool IsInVisionCone(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - EyeLocation).normalized;
        float angleToTarget = Vector3.Angle(EyeDirection, directionToTarget);

        return angleToTarget <= visionConeAngle * 0.5f;
    }

    private bool HasLineOfSight(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - EyeLocation).normalized;
        float distanceToTarget = Vector3.Distance(EyeLocation, targetPosition);

        if (Physics.Raycast(EyeLocation, directionToTarget, out RaycastHit hit, distanceToTarget, ~ignoredLayers))
        {
            return hit.collider.gameObject == target;
        }
        return true;
    }

    protected override void UpdateTargetPosition(GameObject newTarget = null)
    {
        if (newTarget != null)
        {
            if (IsInVisionCone(newTarget.transform.position))
            {
                if (HasLineOfSight(newTarget.transform.position))
                {
                    base.UpdateTargetPosition(newTarget);
                    memoryTimer.Start();
                }
                else
                {
                    // If target is in vision cone but not in line of sight, forget it immediately
                    ForgetTarget();
                }
            }
        }
        else
        {
            if (target != null && memoryTimer.IsRunning)
            {
                // Continue to remember the last known position
                lastKnownPosition = target.transform.position;
            }
            else
            {
                // If timer expires, forget the target
                ForgetTarget();
            }
        }
    }

    private void ForgetTarget()
    {
        lastKnownPosition = Vector3.zero;
        target = null;
        //NotifyTargetChanged();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (IsInVisionCone(other.transform.position) && HasLineOfSight(other.transform.position))
        {
            UpdateTargetPosition(other.gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (IsInVisionCone(other.transform.position))
        {
            if (HasLineOfSight(other.transform.position))
            {
                UpdateTargetPosition(other.gameObject);
            }
            else
            {
                // Forget the target if it is visible in the cone but occluded
                ForgetTarget();
            }
        }
        else
        {
            UpdateTargetPosition();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        UpdateTargetPosition();
    }

    protected override void Update()
    {
        base.Update();
        memoryTimer.Tick(Time.deltaTime);

        // Check if the currently tracked target is occluded during update
        if (target != null)
        {
            if (!HasLineOfSight(target.transform.position))
            {
                ForgetTarget();
            }
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = UnityEngine.Color.blue;
        Vector3 forward = transform.forward * detectionRadius;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-visionConeAngle * 0.5f, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(visionConeAngle * 0.5f, Vector3.up);

        Vector3 leftRay = leftRayRotation * forward;
        Vector3 rightRay = rightRayRotation * forward;

        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);

        if (lastKnownPosition != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(lastKnownPosition, 0.3f); // Show last known position
        }

        if (target != null)
        {
            Debug.DrawLine(EyeLocation, target.transform.position, Color.cyan);
        }
    }
}