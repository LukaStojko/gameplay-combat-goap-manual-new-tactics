using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class TacticsWaypoint : MonoBehaviour
{
    private Renderer objectRenderer;

    [Tooltip("The cover object associated with this waypoint")]
    public Cover relatedCover; // Assume Cover is another script or GameObject

    [Tooltip("Cover value influenced by waypoint positioning.")]
    public float coverValue = 0f;

    //[Tooltip("The field of view angle for grading the waypoint, in degrees")]
    //private float nullifierAngleOpposite = 45f;

    private GameObject player; // Reference to the player

    private GUIStyle textStyle; // For displaying debug score in the scene

    private float maxDistance = 15f; //Max distance at which the cover is still considered viable

    public TacticsWaypoint waypoint_left;
    public TacticsWaypoint waypoint_right;

    // Start is called before the first frame update
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        // Initialize text style for debugging
        textStyle = new GUIStyle();
        textStyle.normal.textColor = Color.black;
        textStyle.alignment = TextAnchor.MiddleCenter;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
            if (player == null)
            {
                Debug.LogWarning("Player not found in the scene!");
                return;
            }
        }

        gameObject.transform.LookAt(relatedCover.transform.position);

        // Calculate and debug score
        CalculateScore();

        // Debug Draw visuals
        DebugDraw();
    }

    private void CalculateScore()
    {
        if (player == null || relatedCover == null)
        {
            Debug.LogWarning("Cannot calculate score: player or related cover is null.");
            coverValue = 0;
            return;
        }

        // Perform a line of sight check using a raycast
        RaycastHit hit;
        if (Physics.Linecast(transform.position, player.transform.position, out hit))
        {
            // If the hit object isn't the player and not the ground
            if (hit.transform != player.transform && hit.transform.CompareTag("Ground") == false)
            {
                coverValue = 0; // Score goes to zero if cover is blocking the line of sight
                return;
            }
        }

        // Ensure neighboring waypoints are defined for angle calculations
        if (waypoint_left == null || waypoint_right == null)
        {
            Debug.LogWarning("Neighboring waypoints are not set.");
            coverValue = 0;
            return;
        }
        Vector3 toWaypointLeft = (waypoint_left.transform.position - transform.position).normalized;
        Vector3 toWaypointRight = (waypoint_right.transform.position - transform.position).normalized;
        Vector3 toPlayer = (player.transform.position - transform.position).normalized;
        float angleToLeft = Vector3.Angle(toPlayer, toWaypointLeft);
        float angleToRight = Vector3.Angle(toPlayer, toWaypointRight);
        float angleWaypoints = Vector3.Angle(toWaypointLeft, toWaypointRight);

        // Validate if the player's position falls within the waypoint angle range
        if (angleToLeft > angleWaypoints || angleToRight > angleWaypoints)
        {
            coverValue = 0; // Invalid position for this waypoint
            return;
        }

        // Ensure the player is in the forward-facing 180° arc of the waypoint
        Vector3 toCover = (relatedCover.transform.position - transform.position).normalized;
        float playerAngleToCoverDirection = Vector3.Angle(toPlayer, toCover);
        if (playerAngleToCoverDirection > 120f) //90f // 180° total, so exclude angles >90°
        {
            coverValue = 0; // Player is behind the waypoint; not valid
            return;
        }

        // Distance score: we want a lower score when the waypoint is near the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float distanceScore = Mathf.Clamp01(distanceToPlayer / maxDistance);

        //// Angle score: we penalize larger angles between the waypoint->player and waypoint->cover vectors
        //Vector3 toPlayer = (player.transform.position - transform.position).normalized;
        //Vector3 toCover = (relatedCover.transform.position - transform.position).normalized;

        //float angle = Vector3.Angle(toPlayer, toCover);

        //// Calculate angle penalty: normalize angle to a 0-1 range (smaller is better)
        //float maxAnglePenalty = 90f; // We consider angles beyond 90 degrees to be highly penalizing
        //float angleScore = Mathf.Clamp01(1f - (angle / maxAnglePenalty));

        //// Final score combines distance score and angle score
        //float score = distanceScore * angleScore;


        float score = distanceScore;

        coverValue = score * 100f; // Scale score to be between 0-100
    }

    public void ChangeMaterialColor(Color newColor)
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = newColor;
        }
        else
        {
            Debug.LogWarning("Renderer not found on this GameObject!");
        }
    }

    private void DebugDraw()
    {
        if (player == null || relatedCover == null) return;

        // Line to player
        Debug.DrawLine(transform.position, player.transform.position, Color.green);

        // Line to cover
        if(relatedCover != null)
        {
            Debug.DrawLine(transform.position, relatedCover.transform.position, Color.blue);
        }

        // Lines to neighborung waypoints
        if(waypoint_left != null)
        {
            Debug.DrawLine(transform.position, waypoint_left.transform.position, Color.magenta);
        }
        if(waypoint_right != null)
        {
            Debug.DrawLine(transform.position, waypoint_right.transform.position, Color.magenta);
        }

        // Field of view (cones)
        //DebugDrawCone(transform.position, transform.forward, nullifierAngleOpposite, 5f, Color.red);
    }

    private void DebugDrawCone(Vector3 position, Vector3 direction, float angle, float length, Color color)
    {
        Vector3 leftBoundary = Quaternion.Euler(0, -angle / 2, 0) * direction;
        Vector3 rightBoundary = Quaternion.Euler(0, angle / 2, 0) * direction;

        leftBoundary = leftBoundary.normalized * length;
        rightBoundary = rightBoundary.normalized * length;

        Debug.DrawLine(position, position + leftBoundary, color);
        Debug.DrawLine(position, position + rightBoundary, color);

        // Optionally draw lines connecting cone boundaries for better visualization
        Vector3 midPoint = position + direction.normalized * length;
        Debug.DrawLine(position + leftBoundary, midPoint, color);
        Debug.DrawLine(midPoint, position + rightBoundary, color);
    }


    // Draw the score above the waypoint in the scene
    private void OnGUI()
    {
        if (player != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.0f);
            if (screenPosition.z > 0) // Ensure it is in front of the camera
            {
                string scoreText = $"Score: {coverValue:F1}";
                GUI.Label(new Rect(screenPosition.x - 50, Screen.height - screenPosition.y, 100, 20), scoreText, textStyle);
            }
        }
    }
}
