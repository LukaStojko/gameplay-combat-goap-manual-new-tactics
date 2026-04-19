using UnityEngine;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour
{
    public List<Vector3> positions = new List<Vector3>();
    protected int obstacleIdx;

    // Use this for initialization
    void Start()
    {
        var x = transform.localScale.x * 0.5f;
        var z = transform.localScale.z * 0.5f;
        
        if (transform.rotation.y != 0.0f)
        {
            Debug.LogError("Rotation of obstacles not supported");
        }
        
        var position = transform.position;
        
        // Calculating obstacle polygon from object bounds.
        // TODO include rotation
        positions = new List<Vector3>()
        {
            position + new Vector3(-x, 0.0f, -z),
            position + new Vector3( x, 0.0f, -z),
            position + new Vector3( x, 0.0f,  z),
            position + new Vector3(-x, 0.0f,  z)
        };
        
        obstacleIdx = CrowdManager.Instance.AddObstacleToSimulator(this);
    }

    void Update()
    {
        if (positions.Count == 0)
        {
            return;
        }

        for (int i = 1; i < positions.Count; i++)
        {
            Debug.DrawLine(positions[i-1], positions[i], Color.green);
        }

        Debug.DrawLine(positions[positions.Count-1], positions[0], Color.green);
    }
}