using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Animator _animator;
    private CharacterController _characterController;
    public GameObject ArrowMesh;
    public GameObject ArrowPrefab;
    public Transform ArrowPosition;

    private Vector3 _velocity;
    private bool _armed = false;
    private bool _aiming = false;
    private bool _draw = false;
    private bool _shouldDraw = false;
    private bool _shouldKick = false;

    public bool Armed
    {
        get => _armed;
        set => _armed = value;
    }

    public bool Draw
    {
        get => _draw;
        set => _draw = value;
    }

    public bool ShouldDraw
    {
        get => _shouldDraw;
        set => _shouldDraw = value;
    }
    
    public bool ShouldKick
    {
        get => _shouldKick;
        set => _shouldKick = value;
    }
    
    public Vector3 Velocity
    {
        get => _velocity;
        set => _velocity = value;
    }

    public TacticsWaypoint coverWaypoint;
    public bool isInCover = false;
    public bool shouldGetToCoverLocation = true;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        ArrowMesh.GetComponent<Renderer>().enabled = _armed;
    }

    void Update()
    {
        //START: Goap Stuff
        if(coverWaypoint != null)
        {
            if (Vector3.Distance(transform.position, coverWaypoint.transform.position) < 0.5f)
                isInCover = true;
            else
                isInCover = false;

            if(coverWaypoint.coverValue <= 0f)
            {
                shouldGetToCoverLocation = false;
                isInCover = false;
            }
            else
            {
                shouldGetToCoverLocation = true;
            }
        }
        else
        {
            isInCover = false;
            shouldGetToCoverLocation = true;
        }
        //END: Goap Stuff

        _animator.SetFloat("Velocity", _velocity.magnitude);
        _animator.SetBool("Armed", _armed);
        
        if (_draw)
        {
            if (_shouldDraw == false)
            {
                if (_aiming && ArrowPrefab)
                {
                    var arrow = Instantiate(ArrowPrefab, ArrowPosition.position, transform.rotation);
                    arrow.GetComponent<Rigidbody>().velocity = transform.forward * 15.0f;
                    _aiming = false;
                }
            }

            _draw = _shouldDraw;
        }
        else
        {
            _draw = _shouldDraw;
            _aiming = false;
        }

        _animator.SetBool("Shoot", _draw);

        if (_shouldKick)
        {
            _animator.SetBool("Kick", _shouldKick);
            _shouldKick = false;
        }
        else
        {
            _animator.SetBool("Kick", false);
        }
    }

    private void OnAnimatorMove()
    {
        var movement = _animator.deltaPosition;
        movement.y = 0.0f;
        _characterController.Move(movement);
    }

    public void Equip()
    {
        ArrowMesh.GetComponent<Renderer>().enabled = true;
    }

    public void Disarm()
    {
        ArrowMesh.GetComponent<Renderer>().enabled = false;
    }
    
    // the function to be called as an event
    public void Shoot()
    {
        _aiming = true;
    }

    void Step()
    {

    }
}
