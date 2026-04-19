using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AiController))]
[RequireComponent(typeof(VisionSensor))]
[RequireComponent(typeof(HearingSensor))]
public class AwarenessSystem : MonoBehaviour
{
    private AiController aiController;
    private VisionSensor visionSensor;
    private HearingSensor hearingSensor;

    // Start is called before the first frame update
    void Start()
    {
        aiController = GetComponent<AiController>();
        visionSensor = GetComponent<VisionSensor>();
        hearingSensor = GetComponent<HearingSensor>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject detectedTarget = null;

        // Check for detected target from the Vision Sensor
        if (visionSensor.active && visionSensor.DetectedTarget != null)
        {
            detectedTarget = visionSensor.DetectedTarget;

            aiController.DetectedTarget = detectedTarget;
            return;
        }

        // Check for detected target from the Hearing Sensor
        if (hearingSensor.active && hearingSensor.DetectedTarget != null)
        {
            detectedTarget = hearingSensor.DetectedTarget;
        }

        // Set the AiController's detected target based on what we found
        aiController.DetectedTarget = detectedTarget;
    }
}
