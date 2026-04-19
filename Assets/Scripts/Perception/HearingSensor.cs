using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[RequireComponent(typeof(AiController))]
public class HearingSensor : MonoBehaviour
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
            var gameObjects = GameObject.FindGameObjectsWithTag("PlayerSound");

            //Hear sensor
            //Distance based

            foreach (var gameObject in gameObjects)
            {
                Sound soundComponent = gameObject.GetComponent<Sound>();
                if (soundComponent != null)
                {
                    // Calculate the distance between the sensor and the player
                    float distanceToPlayer = Vector3.Distance(transform.position, gameObject.transform.position);

                    // Check if the player is within hearing range
                    if (distanceToPlayer <= aiController.hearingRange)
                    {
                        // Set the detected target in AiController to the player
                        DetectedTarget = soundComponent.owner;
                        Debug.Log("Head Player");
                        return;
                    }
                }
            }

            DetectedTarget = null;
        }
    }
}
