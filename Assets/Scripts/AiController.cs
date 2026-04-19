using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MovementController))]
public class AiController : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private MovementController _movementController;
    private Vector3 _target;

    public Vector3 EyeLocation => transform.position;
    public Vector3 EyeDirection => transform.forward;

    public GameObject DetectedTarget;

    public float visionRadius = 10.0f;
    public float visionConeAngle = 25.0f;

    public float hearingRange = 15.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _movementController = GetComponent<MovementController>();
        
        _navMeshAgent.updatePosition = false;

        //while (!_navMeshAgent.hasPath)
        {
            _target = new Vector3(Random.Range(-10.0f, 10.0f), 0.0f,
                Random.Range(-10.0f, 10.0f));
            _navMeshAgent.destination = _target;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (DetectedTarget)
        {
            var direction = DetectedTarget.transform.position - transform.position;
            direction.y = 0.0f;

            transform.forward = direction;
            _movementController.Armed = true;
            _movementController.Velocity = Vector3.zero;
        }
        else
        {
            if (Vector3.Distance(transform.position, _target) < 2.0f)
            {
                _target = new Vector3(Random.Range(-10.0f, 10.0f), 0.0f,
                    Random.Range(-10.0f, 10.0f));
                _navMeshAgent.destination = _target;
            }
        
            var desiredVelocity = _navMeshAgent.desiredVelocity;
            _movementController.Velocity = new Vector3(0.0f, 0.0f, desiredVelocity.magnitude);   
        }
    }
    void Step()
    {

    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(AiController))]
public class AiControllerEditor : Editor
{
    public void OnSceneGUI()
    {
        var ai = target as AiController;

        // draw the detectopm range
        Handles.color = Color.green;
        Handles.DrawSolidDisc(ai.transform.position, Vector3.up, ai.visionRadius);
        
        // work out the start point of the vision cone
        Vector3 startPoint = Mathf.Cos(-ai.visionConeAngle * Mathf.Deg2Rad) * ai.transform.forward +
                             Mathf.Sin(-ai.visionConeAngle * Mathf.Deg2Rad) * ai.transform.right;

        // draw the vision cone
        Handles.color = Color.yellow;
        Handles.DrawSolidArc(ai.transform.position, Vector3.up, startPoint, ai.visionConeAngle * 2f, ai.visionRadius);


        Handles.color = Color.cyan;
        Handles.DrawSolidDisc(ai.transform.position + new Vector3(0, 5, 0), Vector3.up, ai.hearingRange);
    }
}
#endif // UNITY_EDITOR
