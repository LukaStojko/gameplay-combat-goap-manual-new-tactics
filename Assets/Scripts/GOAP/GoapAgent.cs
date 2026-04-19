using System.Collections.Generic;
using System.Linq;
using DependencyInjection; // https://github.com/adammyhre/Unity-Dependency-Injection-Lite
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoapAgent : MonoBehaviour
{
    [Header("Sensors")]
    [SerializeField] Sensor playerSensor; //is SensorWithVisionCone
    [SerializeField] Sensor meleeAttackSensor;
    [SerializeField] Sensor rangedAttackSensor;

    [Header("Known Locations")]
    [SerializeField] Transform targetPosition;
    [SerializeField] List<Waypoint> waypoints;
    [SerializeField] List<TacticsWaypoint> tacticsWaypoints;

    [Header("Info")]
    public bool rangeAttack = true;
    public bool hasCoordinationPlan = false;
    [SerializeField] List<GoapAgent> allies;

    NavMeshAgent navMeshAgent;
    MovementController movementController;

    GameObject target;
    Vector3 destination;

    AgentGoal lastGoal;
    public AgentGoal currentGoal;
    public ActionPlan actionPlan;
    public AgentAction currentAction;

    public Dictionary<string, AgentBelief> beliefs;
    public HashSet<AgentAction> actions;
    public HashSet<AgentGoal> goals;

    [Inject] GoapFactory gFactory;
    IGoapPlanner gPlanner;

    void Awake()
    {
        gPlanner = new GoapPlanner();

        navMeshAgent = GetComponent<NavMeshAgent>();
        movementController = GetComponent<MovementController>();
    }

    void Start()
    {
        tacticsWaypoints = GameObject.FindObjectsOfType<TacticsWaypoint>().ToList();
        allies = GameObject.FindObjectsOfType<GoapAgent>().ToList();
        allies.Remove(this);

        SetupBeliefs();
        SetupActions();
        SetupGoals();
    }

    void SetupBeliefs()
    {
        beliefs = new Dictionary<string, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, beliefs);

        factory.AddBelief("Nothing", () => false);

        factory.AddBelief("AgentIdle", () => !navMeshAgent.hasPath);
        factory.AddBelief("AgentMoving", () => navMeshAgent.hasPath);

        //factory.AddLocationBelief("AgentAtTarget", 1f, targetPosition);
        //for(int i = 0; i < waypoints.Count; i++)
        //{
        //    factory.AddLocationBelief($"At Waypoint {i}", 0f, waypoints[i].transform);
        //}

        factory.AddBelief("ArcherArmed", () => movementController.Armed);
        factory.AddBelief("ArcherUnarmed", () => !movementController.Armed);
        factory.AddBelief("ArcherShootArrow", () => movementController.Armed);

        factory.AddBelief("ConsideredInCover", () => movementController.isInCover || !movementController.shouldGetToCoverLocation);

        factory.AddBelief("hasAllies", () => allies.Count > 0);
        factory.AddBelief("ConsideredCoordinationCompleted", () => allies.Count <= 0 || hasCoordinationPlan);

        factory.AddSensorBelief("PlayerDetected", playerSensor);
        factory.AddSensorBelief("PlayerInMeleeAttackRange", meleeAttackSensor);
        factory.AddBelief("ShouldRangeAttack", () => !beliefs["PlayerInMeleeAttackRange"].Evaluate() && rangeAttack); //makes sure to switch to melee if attacking in ranged
        factory.AddSensorBelief("PlayerInRangedAttackRange", rangedAttackSensor);
        factory.AddBelief("AttackingPlayer", () => false); // Player can always be attacked, this will never become true

        factory.AddBelief("CanPatrol", () => waypoints.Count > 0);
        factory.AddBelief("ShouldPatrol", () => !beliefs["PlayerDetected"].Evaluate());
        factory.AddBelief("Patrol", () => false); // Player can always be patrol, this will never become true
    }

    void SetupActions()
    {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(5))
            .AddEffect(beliefs["Nothing"])
            .Build());

        //actions.Add(new AgentAction.Builder("Move to Position")
        //    .WithStrategy(new MoveStrategy(navMeshAgent, () => targetPosition.position))
        //    .AddEffect(beliefs["AgentAtTarget"])
        //    .Build());

        //actions.Add(new AgentAction.Builder("Patrolling")
        //    .WithStrategy(new PatrolStrategy(navMeshAgent, movementController, waypoints))
        //    .AddPrecondition(beliefs["CanPatrol"])
        //    .AddEffect(beliefs["Patrol"])
        //    .WithCost(10)
        //    .Build());

        //actions.Add(new AgentAction.Builder("Patrol")
        //    .WithStrategy(new PatrolStrategy(navMeshAgent, movementController, waypoints))
        //    .AddPrecondition(beliefs["ArcherUnarmed"])
        //    .AddPrecondition(beliefs["ShouldPatrol"])
        //    .AddPrecondition(beliefs["CanPatrol"])
        //    .AddEffect(beliefs["Patrol"])
        //    .WithCost(10)
        //    .Build());

        //actions.Add(new AgentAction.Builder("Ranged Attack Player")
        //    .WithStrategy(new ArcherRangedAttackStrategy(navMeshAgent, movementController, () => beliefs["PlayerInRangedAttackRange"].Location))
        //    .AddPrecondition(beliefs["PlayerInRangedAttackRange"])
        //    .AddPrecondition(beliefs["ShouldRangeAttack"])
        //    .AddEffect(beliefs["AttackingPlayer"])
        //    .WithCost(10)
        //    .Build());

        actions.Add(new AgentAction.Builder("Form Attack Plan with Allies")
            .WithStrategy(new ArcherFormAttackPlan(this, allies))
            .AddPrecondition(beliefs["hasAllies"])
            .AddPrecondition(beliefs["PlayerDetected"])
            .AddEffect(beliefs["ConsideredCoordinationCompleted"])
            .WithCost(1)
            .Build());

        actions.Add(new AgentAction.Builder("Unequip Bow")
            .WithStrategy(new ArcherUnequipBowStrategy(navMeshAgent, movementController))
            .AddPrecondition(beliefs["ArcherArmed"])
            .AddEffect(beliefs["ArcherUnarmed"])
            .WithCost(1)
            .Build());

        actions.Add(new AgentAction.Builder("Equip Bow")
            .WithStrategy(new ArcherEquipBowStrategy(navMeshAgent, movementController, () => beliefs["PlayerInRangedAttackRange"].Location))
            .AddPrecondition(beliefs["ConsideredCoordinationCompleted"])
            .AddPrecondition(beliefs["PlayerInRangedAttackRange"])
            .AddPrecondition(beliefs["ShouldRangeAttack"])
            .AddPrecondition(beliefs["PlayerDetected"])
            .AddEffect(beliefs["ArcherArmed"])
            .WithCost(1)
            .Build());

        actions.Add(new AgentAction.Builder("Get to Cover")
            .WithStrategy(new ArcherGetToCover(navMeshAgent, movementController, tacticsWaypoints, playerSensor))
            //.AddPrecondition(beliefs["ArcherArmed"])
            .AddEffect(beliefs["ConsideredInCover"])
            .WithCost(1)
            .Build());

        actions.Add(new AgentAction.Builder("Aim Bow")
            .WithStrategy(new ArcherAimBowStrategy(navMeshAgent, movementController, () => beliefs["PlayerInRangedAttackRange"].Location))
            .AddPrecondition(beliefs["ConsideredInCover"])
            .AddPrecondition(beliefs["ArcherArmed"])
            .AddEffect(beliefs["ArcherShootArrow"])
            .WithCost(1)
            .Build());

        actions.Add(new AgentAction.Builder("Shoot Arrow")
            .WithStrategy(new ArcherShootArrowStrategy(navMeshAgent, movementController))
            .AddPrecondition(beliefs["ArcherShootArrow"])
            .AddEffect(beliefs["ArcherUnarmed"])
            .AddEffect(beliefs["AttackingPlayer"])
            //.AddEffect(beliefs["RangeAttackingPlayer"])
            .WithCost(1)
            .Build());

        actions.Add(new AgentAction.Builder("Chase Player")
            .WithStrategy(new ArcherMoveStrategy(navMeshAgent, movementController, () => beliefs["PlayerDetected"].Location))
            .AddPrecondition(beliefs["ConsideredCoordinationCompleted"])
            .AddPrecondition(beliefs["PlayerDetected"])
            .AddEffect(beliefs["PlayerInMeleeAttackRange"])
            .WithCost(5)
            .Build());

        actions.Add(new AgentAction.Builder("Melee Attack Player")
            .WithStrategy(new ArcherMeleeAttackStrategy(navMeshAgent, movementController, () => beliefs["PlayerInMeleeAttackRange"].Location))
            .AddPrecondition(beliefs["ArcherUnarmed"])
            .AddPrecondition(beliefs["PlayerInMeleeAttackRange"])
            .AddEffect(beliefs["AttackingPlayer"])
            //.AddEffect(beliefs["MeleeAttackingPlayer"])
            .WithCost(20)
            .Build());
    }

    void SetupGoals()
    {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("Chill Out")
            .WithPriority(1)
            .WithDesiredEffect(beliefs["Nothing"])
            .Build());

        //goals.Add(new AgentGoal.Builder("Move Goal")
        //    .WithPriority(2)
        //    .WithDesiredEffect(beliefs["AgentAtTarget"])
        //    .Build());

        goals.Add(new AgentGoal.Builder("Patrol Area")
            .WithPriority(3)
            .WithDesiredEffect(beliefs["Patrol"])
            .Build());

        goals.Add(new AgentGoal.Builder("Elimiate Player")
            .WithPriority(5)
            .WithDesiredEffect(beliefs["AttackingPlayer"])
            .Build());
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) < range;

    void OnEnable()
    {
        playerSensor.OnTargetChanged += HandleTargetChanged;
    }
    void OnDisable()
    {
        playerSensor.OnTargetChanged -= HandleTargetChanged;
    }
    void HandleTargetChanged()
    {
        Debug.Log($"Target changed, clearing current action and goal");
        // Force the planner to re-evaluate the plan
        currentAction = null;
        currentGoal = null;
    }

    void Update()
    {
        // Update the plan and current action if there is one
        if (currentAction == null)
        {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();

            if (actionPlan != null && actionPlan.Actions.Count > 0)
            {
                navMeshAgent.ResetPath();

                currentGoal = actionPlan.AgentGoal;
                Debug.Log($"Goal: {currentGoal.Name} with {actionPlan.Actions.Count} actions in plan");
                currentAction = actionPlan.Actions.Pop();
                Debug.Log($"Popped action: {currentAction.Name}");
                // Verify all precondition effects are true
                if (currentAction.Preconditions.All(b => b.Evaluate()))
                {
                    currentAction.Start();
                }
                else
                {
                    Debug.Log("Preconditions not met, clearing current action and goal");
                    currentAction = null;
                    currentGoal = null;
                }
            }
        }

        // If we have a current action, execute it
        if (actionPlan != null && currentAction != null)
        {
            currentAction.Update(Time.deltaTime);

            if (currentAction.Complete)
            {
                Debug.Log($"{currentAction.Name} complete");
                currentAction.Stop();
                currentAction = null;

                if (actionPlan.Actions.Count == 0)
                {
                    Debug.Log("Plan complete");
                    lastGoal = currentGoal;
                    currentGoal = null;
                }
            }
        }
    }

    void CalculatePlan()
    {
        var priorityLevel = currentGoal?.Priority ?? 0;

        HashSet<AgentGoal> goalsToCheck = goals;

        // If we have a current goal, we only want to check goals with higher priority
        if (currentGoal != null)
        {
            Debug.Log("Current goal exists, checking goals with higher priority");
            goalsToCheck = new HashSet<AgentGoal>(goals.Where(g => g.Priority > priorityLevel));
        }

        var potentialPlan = gPlanner.Plan(this, goalsToCheck, lastGoal);
        if (potentialPlan != null)
        {
            actionPlan = potentialPlan;
        }
    }
}