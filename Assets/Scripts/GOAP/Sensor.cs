using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Sensor : MonoBehaviour
{
    [SerializeField] protected LayerMask ignoredLayers;
    [SerializeField] public float detectionRadius = 5f;
    [SerializeField] float timerInterval = 1f;
    
    SphereCollider detectionRange;
    
    public event Action OnTargetChanged = delegate { };
    
    public Vector3 TargetPosition => target ? target.transform.position : Vector3.zero;
    public bool IsTargetInRange => TargetPosition != Vector3.zero;
    
    public GameObject target;
    protected Vector3 lastKnownPosition;
    protected CountdownTimer timer;

    void Awake()
    {
        detectionRange = GetComponent<SphereCollider>();
        detectionRange.isTrigger = true;
        detectionRange.radius = detectionRadius;
    }

    protected virtual void Start()
    {
        timer = new CountdownTimer(timerInterval);
        timer.OnTimerStop += () => {
            UpdateTargetPosition(target.OrNull());
            timer.Start();
        };
        timer.Start();
    }

    protected virtual void Update()
    {
        timer.Tick(Time.deltaTime);
    }

    /// <summary>
    /// Triggers the OnTargetChanged event.
    /// </summary>
    protected void NotifyTargetChanged()
    {
        OnTargetChanged?.Invoke();
    }

    protected virtual void UpdateTargetPosition(GameObject target = null)
    {
        //START: WILL ALWAYS CAUSE TARGET CHANGE
        //this.target = target;
        //if (IsTargetInRange && (lastKnownPosition != TargetPosition || lastKnownPosition != Vector3.zero))
        //{
        //    lastKnownPosition = TargetPosition;
        //    OnTargetChanged.Invoke();
        //}
        //END: WILL ALWAYS CAUSE TARGET CHANGE

        //START: WILL ALWAYS CAUSE TARGET CHANGE WHEN TARGET MOVING
        //// Avoid unnecessary updates if the target and position haven't actually changed
        //if (this.target == target && lastKnownPosition == TargetPosition)
        //    return;

        //this.target = target;
        //Vector3 newPosition = TargetPosition;

        //// Invoke only when there's a meaningful change in target or position
        //if (newPosition != lastKnownPosition)
        //{
        //    lastKnownPosition = newPosition;
        //    OnTargetChanged.Invoke();
        //}
        //END: WILL ALWAYS CAUSE TARGET CHANGE WHEN TARGET MOVING

        //START: WILL CAUSE TARGET CHANGE WHEN TARGET ENTERS/LEAVES
        if(IsTargetInRange && (lastKnownPosition != TargetPosition || lastKnownPosition != Vector3.zero))
        {
            lastKnownPosition = TargetPosition;
        }
        if (this.target != target)
        {
            this.target = target;
            OnTargetChanged.Invoke();
        }
        //END: WILL CAUSE TARGET CHANGE WHEN TARGET ENTERS/LEAVES
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        UpdateTargetPosition(other.gameObject);
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        UpdateTargetPosition();
    }
    
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = IsTargetInRange ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}