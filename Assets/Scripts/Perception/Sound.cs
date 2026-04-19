using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    AudioSource audioSource;
    public GameObject owner;

    // Start is called before the first frame update
    void Start()
    {
        if (audioSource == null)
        {
            Inititalize();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlaySound(float destroyAfter)
    {
        audioSource.Play();

        Destroy(gameObject, destroyAfter);
    }

    public void setAudio(GameObject owner, string ownerTag, AudioClip audioClip, float volume)
    {
        this.owner = owner;
        this.gameObject.tag = ownerTag;
        if (audioSource == null)
        {
            Inititalize();
        }
        audioSource.clip = audioClip;
        audioSource.volume = volume;
    }

    private void Inititalize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        SetAudioSourceProperties();
    }

    private void SetAudioSourceProperties()
    {
        audioSource.spatialBlend = 1.0f; // Make it 3D
        audioSource.minDistance = 1.0f;  // Minimum distance for sound to be heard at full volume
        audioSource.maxDistance = 500.0f; // Maximum distance for sound to be heard (attenuation starts)
        audioSource.rolloffMode = AudioRolloffMode.Linear; // Set rolloff mode (Custom or Logarithmic)
        audioSource.dopplerLevel = 0; // Set to 0 if you do not want doppler effects
        audioSource.spread = 0; // Set spread (0 = directly in front, 360 = surround)
    }
}
