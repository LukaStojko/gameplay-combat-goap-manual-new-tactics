using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(MovementController))]
public class HumanController : MonoBehaviour
{
    private CharacterController _characterController;
    private MovementController _movementController;

    private float _velocity = 0.0f;
    public float rotateSpeed = 3.0F;

    public AudioClip walkAudioClip;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _movementController = GetComponent<MovementController>();
    }

    // Update is called once per frame
    void Update()
    {
        var forward = Mathf.Max(0, Input.GetAxis("Vertical"));
        forward *= Input.GetKey(KeyCode.LeftShift) ? 2.0f : 1.0f;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            _movementController.Armed = !_movementController.Armed;
        }
        
        transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime, 0);

        _velocity += Math.Clamp(forward - _velocity, -Time.deltaTime * 2.0f, Time.deltaTime * 2.0f);
        _movementController.Velocity = new Vector3(0,0, _velocity);
        _movementController.ShouldDraw = Input.GetKey(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _movementController.ShouldKick = true;
        }
    }
    void Step()
    {
        GameObject soundObject = new GameObject();
        soundObject.name = "PlayerStepSound " + soundObject.GetInstanceID(); 
        soundObject.transform.position = transform.position;
        Sound stepSound = soundObject.AddComponent<Sound>();
        stepSound.setAudio(this.gameObject, "PlayerSound", walkAudioClip, 0.2f);
        stepSound.PlaySound(walkAudioClip.length * 3);
    }
}
