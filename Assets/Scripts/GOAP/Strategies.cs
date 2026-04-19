using System;
using System.Collections.Generic;
using System.Linq;
using RVO;
using UnityEngine;
using UnityEngine.AI;

// TODO Migrate Strategies, Beliefs, Actions and Goals to Scriptable Objects and create Node Editor for them
public interface IActionStrategy {
    bool CanPerform { get; }
    bool Complete { get; }
    
    void Start() {
        // noop
    }
    
    void Update(float deltaTime) {
        // noop
    }
    
    void Stop() {
        // noop
    }
}

public class IdleStrategy : IActionStrategy
{
    public bool CanPerform => true; // Agent can always Idle
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public IdleStrategy(float duration)
    {
        timer = new CountdownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start()
    {
        timer.Start();
    }
    public void Update(float deltaTime)
    {
        timer.Tick(deltaTime);
    }
}

public class MoveStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly Func<Vector3> destination;
    
    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;
    
    public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination)
    {
        this.agent = agent;
        this.destination = destination;
    }

    public void Start()
    {
        agent.SetDestination(destination());
    }
    public void Stop()
    {
        agent.ResetPath();
    }
}

public class ArcherMoveStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly MovementController movementController;
    readonly Func<Vector3> destination;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

    public ArcherMoveStrategy(NavMeshAgent agent, MovementController movementController, Func<Vector3> destination)
    {
        this.agent = agent;
        this.movementController = movementController;
        this.destination = destination;
    }

    public void Start()
    {
        agent.SetDestination(destination());
    }

    public void Update(float deltaTime)
    {
        var desiredVelocity = agent.desiredVelocity;
        movementController.Velocity = new Vector3(0.0f, 0.0f, desiredVelocity.magnitude);
    }

    public void Stop()
    {
        agent.ResetPath();
        movementController.Velocity = Vector3.zero;
    }
}

public class WanderStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly float wanderRadius;
    
    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;
    
    public WanderStrategy(NavMeshAgent agent, float wanderRadius)
    {
        this.agent = agent;
        this.wanderRadius = wanderRadius;
    }

    public void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * wanderRadius).With(y: 0);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, wanderRadius, 1)) {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
}

public class PatrolStrategy : IActionStrategy
{
    private readonly NavMeshAgent navMeshAgent;
    private readonly MovementController movementController;
    private readonly List<Waypoint> waypoints;
    private int currentIndex;
    private float waitTimer;
    private bool isWaiting;

    public bool CanPerform => waypoints.Count > 0; // Can always patrol as long as waypoints exist
    public bool Complete { get; private set; }
    public float WaitTime { get; set; } = 1.0f; // Default wait time at each waypoint

    public PatrolStrategy(NavMeshAgent agent, MovementController movementController, List<Waypoint> waypoints)
    {
        this.navMeshAgent = agent;
        this.movementController = movementController;
        this.waypoints = waypoints;
        currentIndex = -1; // Initialize to an invalid index
        waitTimer = 0f;
        isWaiting = false;
    }

    public void Start()
    {
        // Find the nearest waypoint to start
        float nearestDistance = float.MaxValue;
        int nearestIndex = -1;

        for (int i = 0; i < waypoints.Count; i++)
        {
            float distance = Vector3.Distance(navMeshAgent.transform.position, waypoints[i].transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        if (nearestIndex != -1)
        {
            currentIndex = nearestIndex;
            navMeshAgent.SetDestination(waypoints[currentIndex].transform.position);
        }
    }

    public void Update(float deltaTime)
    {
        if (isWaiting)
        {
            waitTimer += deltaTime;
            if (waitTimer >= WaitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                MoveToNextWaypoint();
            }
        }
        else
        {
            // Check if the agent has reached the current waypoint
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                isWaiting = true;
                movementController.Velocity = Vector3.zero; // Stop movement during wait
            }
            else
            {
                // Update movement controller velocity
                var desiredVelocity = navMeshAgent.desiredVelocity;
                movementController.Velocity = new Vector3(0.0f, 0.0f, desiredVelocity.magnitude);
            }
        }
    }

    private void MoveToNextWaypoint()
    {
        // Move to the next waypoint
        currentIndex = (currentIndex + 1) % waypoints.Count;
        navMeshAgent.SetDestination(waypoints[currentIndex].transform.position);
    }

    public void Stop()
    {
        navMeshAgent.ResetPath();
        movementController.Velocity = Vector3.zero;
        isWaiting = false;
        waitTimer = 0f;
    }
}

public class AttackStrategy : IActionStrategy
{
    public bool CanPerform => true; // Agent can always attack
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public AttackStrategy()
    {
        //this.animations = animations;
        //timer = new CountdownTimer(animations.GetAnimationLength(animations.attackClip));
        //timer.OnTimerStart += () => Complete = false;
        //timer.OnTimerStop += () => Complete = true;
    }

    public void Start()
    {
        //timer.Start();
        //animations.Attack();
    }

    public void Update(float deltaTime)
    {
        //timer.Tick(deltaTime);
    }
}

public class ArcherMeleeAttackStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly MovementController movementController;
    readonly Func<Vector3> destination;
    public bool CanPerform => true; // Agent can always attack
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public ArcherMeleeAttackStrategy(NavMeshAgent agent, MovementController movementController, Func<Vector3> destination)
    {
        //timer = new CountdownTimer(animations.GetAnimationLength(animations.attackClip));
        //timer.OnTimerStart += () => Complete = false;
        //timer.OnTimerStop += () => Complete = true;

        this.agent = agent;
        this.movementController = movementController;
        this.destination = destination;
        Complete = false;

        timer = new CountdownTimer(1f); //Animation Time
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start()
    {
        timer.Start();
    }

    public void Update(float deltaTime)
    {
        agent.transform.LookAt(destination());
        movementController.ShouldKick = true; //make the associated mesh move, bug
        timer.Tick(deltaTime);
    }

    public void Stop()
    {
        timer.Reset();
        movementController.ShouldKick = false;
    }
}

public class ArcherRangedAttackStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly MovementController movementController;
    readonly Func<Vector3> destination;

    public bool CanPerform => destination() != Vector3.zero;
    //public bool Complete => { get; private set; }
    public bool Complete => !CanPerform;

    private float drawTimer;
    private const float DrawDuration = 1.5f;

    private float rearmTimer;
    private const float RearmDuration = 1f;

    public ArcherRangedAttackStrategy(NavMeshAgent agent, MovementController movementController, Func<Vector3> destination)
    {
        this.agent = agent;
        this.movementController = movementController;
        this.destination = destination;
        //Complete = false;

        drawTimer = DrawDuration;
        rearmTimer = RearmDuration;
    }

    public void Start()
    {
        movementController.Velocity = Vector3.zero;
    }

    public void Update(float deltaTime)
    {
        agent.transform.LookAt(destination());

        if (drawTimer > 0f)
        {
            drawTimer -= deltaTime;
            movementController.Armed = true;
            movementController.ShouldDraw = true;
        }
        else
        {
            rearmTimer -= deltaTime;
            movementController.ShouldDraw = false;
            if (rearmTimer < 0f)
            {
                drawTimer = DrawDuration;
                rearmTimer = RearmDuration;
            }
        }
    }

    public void Stop()
    {
        movementController.ShouldDraw = false;
        movementController.Armed = false;
    }
}

public class ArcherFormAttackPlan : IActionStrategy
{
    GoapAgent agent;
    List<GoapAgent> allies;
    public bool CanPerform => !Complete; //destination() != Vector3.zero
    public bool Complete { get; private set; }

    public ArcherFormAttackPlan(GoapAgent agent, List<GoapAgent> allies)
    {
        this.agent = agent;
        this.allies = allies;
        Complete = false;
    }

    public void Start()
    {
        allies.Add(agent);

        var randomIndex = UnityEngine.Random.Range(0, allies.Count); // Randomly selects an index within the allies list
        var selectedAlly = allies[randomIndex];
        selectedAlly.rangeAttack = false;


        foreach (GoapAgent agent in allies)
        {
            agent.hasCoordinationPlan = true;
        }

        Complete = true;
    }

    public void Update(float deltaTime)
    {

    }

    public void Stop()
    {

    }
}

public class ArcherUnequipBowStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly MovementController movementController;

    public bool CanPerform => !Complete;
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public ArcherUnequipBowStrategy(NavMeshAgent agent, MovementController movementController)
    {
        this.agent = agent;
        this.movementController = movementController;
        Complete = false;

        timer = new CountdownTimer(1.5f); //Animation Time
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start()
    {
        timer.Start();
        movementController.Velocity = Vector3.zero;
        movementController.Armed = false;
    }

    public void Update(float deltaTime)
    {
        timer.Tick(deltaTime);
    }

    public void Stop()
    {
        timer.Reset();
    }
}

public class ArcherEquipBowStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly MovementController movementController;
    readonly Func<Vector3> destination;

    public bool CanPerform => !Complete; //destination() != Vector3.zero
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public ArcherEquipBowStrategy(NavMeshAgent agent, MovementController movementController, Func<Vector3> destination)
    {
        this.agent = agent;
        this.movementController = movementController;
        this.destination = destination;
        Complete = false;

        timer = new CountdownTimer(1.5f); //Animation Time
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start()
    {
        timer.Start();
        movementController.Velocity = Vector3.zero;
        movementController.Armed = true;
    }

    public void Update(float deltaTime)
    {
        timer.Tick(deltaTime);
    }

    public void Stop()
    {
        timer.Reset();
    }
}

public class ArcherGetToCover : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly MovementController movementController;
    readonly Sensor playerSensor;

    List<TacticsWaypoint> allWaypoints;
    List<TacticsWaypoint> waypoints;
    TacticsWaypoint bestWaypoint;

    public bool CanPerform => !Complete; //destination() != Vector3.zero
    public bool Complete { get; private set; }

    //readonly CountdownTimer timer;

    public ArcherGetToCover(NavMeshAgent agent, MovementController movementController, List<TacticsWaypoint> waypoints, Sensor playerSensor)
    {
        this.agent = agent;
        this.movementController = movementController;
        this.allWaypoints = waypoints;
        this.waypoints = new List<TacticsWaypoint>();
        this.playerSensor = playerSensor;

        //timer = new CountdownTimer(1.5f); //Animation Time
        //timer.OnTimerStart += () => Complete = false;
        //timer.OnTimerStop += () => Complete = true;
    }

    public void Start()
    {
        //timer.Start();
        Complete = false;

        float detectionRadius = playerSensor.detectionRadius;
        Vector3 sensorPosition = playerSensor.transform.position;
        waypoints = allWaypoints.Where(waypoint => Vector3.Distance(sensorPosition, waypoint.transform.position) <= detectionRadius).ToList();
    }

    public void Update(float deltaTime)
    {
        //timer.Tick(deltaTime);

        if (waypoints.Count <= 0)
        {
            Complete = true;
        }

        bestWaypoint = SelectBestWaypoint();

        if (bestWaypoint != null)
        {
            if (bestWaypoint.coverValue <= 0f)
            {
                Complete = true;
            }

            movementController.coverWaypoint = bestWaypoint;
            agent.destination = movementController.coverWaypoint.transform.position;

            var desiredVelocity = agent.desiredVelocity;
            movementController.Velocity = new Vector3(0.0f, 0.0f, desiredVelocity.magnitude);

            //if (agent.remainingDistance <= 0.25f) //threshold
            //{
            //    Complete = true;
            //}
            if(Vector3.Distance(agent.transform.position, agent.destination) <= 0.25f)
            {
                Complete = true;
            }
        }
    }

    public void Stop()
    {
        //timer.Reset();
        movementController.Velocity = Vector3.zero;
        agent.ResetPath();
        //bestWaypoint.ChangeMaterialColor(Color.blue);
        //bestWaypoint = null;
    }

    public TacticsWaypoint SelectBestWaypoint()
    {
        // Ensure there are waypoints to choose from
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("No waypoints available to select from.");
            return null;
        }

        // Initialize bestWaypoint to null and track the highest score
        TacticsWaypoint bestWaypoint = null;
        float highestScore = float.MinValue;

        // Iterate through all valid waypoints to find the one with the highest cover value
        foreach (TacticsWaypoint waypoint in waypoints)
        {
            if (waypoint == null)
                continue;

            if (waypoint.coverValue > highestScore)
            {
                bestWaypoint = waypoint;
                highestScore = waypoint.coverValue;
            }
        }

        if (bestWaypoint != null)
        {
            Debug.Log($"Best waypoint selected with score: {highestScore}");
        }
        else
        {
            Debug.LogWarning("Could not determine a best waypoint.");
        }

        return bestWaypoint;
    }
}

public class ArcherAimBowStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly MovementController movementController;
    readonly Func<Vector3> destination;

    public bool CanPerform => !Complete; //destination() != Vector3.zero
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public ArcherAimBowStrategy(NavMeshAgent agent, MovementController movementController, Func<Vector3> destination)
    {
        this.agent = agent;
        this.movementController = movementController;
        this.destination = destination;
        Complete = false;

        timer = new CountdownTimer(2f); //Aim Simulation
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start()
    {
        timer.Start();
    }

    public void Update(float deltaTime)
    {
        timer.Tick(deltaTime);
        agent.transform.LookAt(destination());
        movementController.ShouldDraw = true;
    }

    public void Stop()
    {
        timer.Reset();
    }
}

public class ArcherDrawBowStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly MovementController movementController;

    public bool CanPerform => !Complete; //destination() != Vector3.zero
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public ArcherDrawBowStrategy(NavMeshAgent agent, MovementController movementController)
    {
        this.agent = agent;
        this.movementController = movementController;
        Complete = false;

        timer = new CountdownTimer(2f); //Animation Time
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start()
    {
        timer.Start();
    }

    public void Update(float deltaTime)
    {
        timer.Tick(deltaTime);
        movementController.ShouldDraw = true;
    }

    public void Stop()
    {
        timer.Reset();
    }
}

public class ArcherShootArrowStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly MovementController movementController;

    public bool CanPerform => !Complete; //destination() != Vector3.zero
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public ArcherShootArrowStrategy(NavMeshAgent agent, MovementController movementController)
    {
        this.agent = agent;
        this.movementController = movementController;
        Complete = false;

        timer = new CountdownTimer(0.1f); //Animation Time
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start()
    {
        timer.Start();
    }

    public void Update(float deltaTime)
    {
        timer.Tick(deltaTime);
        movementController.ShouldDraw = false;
    }

    public void Stop()
    {
        timer.Reset();
        movementController.Armed = false;
    }
}