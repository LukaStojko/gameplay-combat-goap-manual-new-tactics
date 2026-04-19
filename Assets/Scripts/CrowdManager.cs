using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CrowdManager : Singleton<CrowdManager> {

	private RVO.Simulator simulator;
	public float NeighborDist = 15.0f;
	public int MaxNeighbors = 10;
	public float TimeHorizon = 10.0f;
	public float TimeHorizonObst = 10.0f;
	public float Radius = 1.5f;
	public float MaxSpeed = 2.0f;
	public RVO.Vector2 Velocity = new RVO.Vector2(0, 0);

	public Dictionary<int, Character> characters = new Dictionary<int, Character>();
    public Dictionary<int, Obstacle> obstacles = new Dictionary<int, Obstacle>();

    bool processObstacles = false;

	void Awake () {
		// Initialize simulator
		simulator = RVO.Simulator.Instance;
		simulator.setTimeStep(Time.fixedDeltaTime);
		simulator.setAgentDefaults(
			NeighborDist, MaxNeighbors, TimeHorizon, TimeHorizonObst, Radius, MaxSpeed, Velocity);
	}

    public int AddCharacterToSimulator(Character agent)
	{
		int agentIdx = simulator.addAgent(new RVO.Vector2(agent.transform.position.x, agent.transform.position.z));
		characters.Add(agentIdx, agent);
		// make sure that we recreate workers in next frame otherwise new agent wont be simulated.
		// 0 means using min thread count for current platform.
		simulator.SetNumWorkers(0);
		return agentIdx;
	}

    public int AddObstacleToSimulator(Obstacle obstacle)
    {
        var list = new List<RVO.Vector2>();
        foreach (var position in obstacle.positions)
        {
            list.Add(new RVO.Vector2(position.x, position.z));
        }

        int obstacleIdx = simulator.addObstacle(list);
       
        obstacles.Add(obstacleIdx, obstacle);
        processObstacles = true;

        return obstacleIdx;
    }

    public Character GetCharacter(int agentIdx)
	{
        characters.TryGetValue(agentIdx, out Character character);
        return character;
	}

    void FixedUpdate () 
	{
		// if obstacles where added processObstacle needs to be called
        if (processObstacles)
        {
            simulator.processObstacles();
            processObstacles = false;
        }

		simulator.doStep();
	}
}
