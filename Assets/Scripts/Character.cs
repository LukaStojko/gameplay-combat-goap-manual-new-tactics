using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour {
	public Vector3 target;
	public Color color;

	protected int agentIdx;

	// returns the real velocity of the agent
	public Vector3 GetAgentVelocity()
	{
		var v = RVO.Simulator.Instance.getAgentVelocity(agentIdx);
		return new Vector3(v.x(), 0.0f, v.y());
	}

	public void SetAgentPrefVelocity(Vector3 velocity)
	{
		RVO.Simulator.Instance.setAgentPrefVelocity(agentIdx, new RVO.Vector2(velocity.x, velocity.z));
	}
	
	public Vector3 GetAgentPrefVelocity()
	{
		var v = RVO.Simulator.Instance.getAgentPrefVelocity(agentIdx);
		return new Vector3(v.x(), 0.0f, v.y());
	}
	
	public Vector3 GetAgentPosition()
	{
		var p = RVO.Simulator.Instance.getAgentPosition(agentIdx);
		return new Vector3(p.x(), transform.position.y, p.y());
	}
		
	protected virtual void Start () {
		agentIdx = CrowdManager.Instance.AddCharacterToSimulator(this);

        // clone diffuse material to change color
        var materialColored = new Material(Shader.Find("Diffuse"));
		materialColored.color = color;

		var renderer = this.GetComponent<Renderer>();

		if (renderer != null)
		{
			renderer.material = materialColored;
		}
	}
		
	protected virtual void FixedUpdate () {
    	// TODO uncomment
		// calc prefered velocity
		//var prefVelocity = Vector3.ClampMagnitude(target - transform.position, CrowdManager.Instance.MaxSpeed);
		
        // set prefered velocity
        //SetAgentPrefVelocity(prefVelocity);

		// update position
		//transform.position = GetAgentPosition();
	 }

    // debugdraw
    void OnDrawGizmosSelected() {
		if (!Application.isPlaying)
		{
			return;
		}

		var lines = RVO.Simulator.Instance.getAgentOrcaLines(agentIdx);
		var position = transform.position;

		foreach (var line in lines)
		{
			var otherId = line.agentIdx;

			var c = Color.black;

			if (otherId >= 0)
			{
				Character otherAgent = CrowdManager.Instance.GetCharacter(otherId);
				c = otherAgent.color;
			}

			var p = new Vector3(line.point.x(), transform.position.y, line.point.y());
			var d = new Vector3(line.direction.x(), 0.0f, line.direction.y());

			var r = Vector3.Cross(d, Vector3.up);
			Debug.DrawRay(position + p - d * 5.0f, d * 10.0f, new Color(c.r, c.g, c.b));
			Debug.DrawRay(position + p - d * 5.0f - r * 0.1f, d * 10.0f, new Color(c.r, c.g, c.b, 0.66f));
			Debug.DrawRay(position + p - d * 5.0f - r * 0.2f, d * 10.0f, new Color(c.r, c.g, c.b, 0.33f));
		}

		Gizmos.color = Color.black;
		var prefVelocity = GetAgentPrefVelocity();
		Gizmos.DrawRay(position, prefVelocity);
		Gizmos.DrawSphere(position + prefVelocity, 0.1f);

		Gizmos.color = Color.green;
		var velocity = GetAgentVelocity();
		Gizmos.DrawRay(position, velocity);
		Gizmos.DrawSphere(position + velocity, 0.1f);
    }
}
