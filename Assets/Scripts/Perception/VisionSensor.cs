using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[RequireComponent(typeof(AiController))]
public class VisionSensor : MonoBehaviour
{
    private AiController aiController;

    public bool active = true;
    public GameObject DetectedTarget { get; private set; } // Public getter to allow read-only access

    // Start is called before the first frame update
    void Start()
    {
        aiController = GetComponent<AiController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            var gameObjects = GameObject.FindGameObjectsWithTag("Player");

            foreach (var gameObject in gameObjects)
            {
                //check if in range
                //check if in code
                //check raycast
                //if visible report to AiController

                Vector3 directionToPlayer = (gameObject.transform.position - aiController.EyeLocation).normalized;
                float angleToPlayer = Vector3.Angle(aiController.EyeDirection, directionToPlayer);

                // Check if within vision radius and cone angle
                if (Vector3.Distance(aiController.EyeLocation, gameObject.transform.position) < aiController.visionRadius
                    && angleToPlayer < aiController.visionConeAngle)
                {
                    // Check for obstacles (Raycast)
                    if (!Physics.Linecast(aiController.EyeLocation, gameObject.transform.position))
                    {
                        DetectedTarget = gameObject;
                        Debug.Log("Seen Player");
                        return;
                    }
                }
            }

            DetectedTarget = null;
        }
    }
}
