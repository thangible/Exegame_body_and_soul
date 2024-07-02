using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Based on https://www.youtube.com/watch?v=oxiPWg8cdRM&ab_channel=Chris%27Tutorials
public class TouchingDirections : MonoBehaviour
{
    public ContactFilter2D castFilter;

    // detection lengths
    public float groundDistance = 0.05f;
    public float approachingGroundDistance = 1f;
    public float wallDistance = 0.2f;
    public float ceilingDistance = 0.05f;

    // Handle slopes (https://www.youtube.com/watch?v=B2BCnIIV1WE)
    [SerializeField] private Transform rayCastOrigin;
    //[SerializeField] private Transform playerFeet;
    [SerializeField] private LayerMask layerMask; // for the player col layer --> ground

    RaycastHit2D[] groundHits = new RaycastHit2D[5];
    private RaycastHit2D _groundHit; // feet on ground
    RaycastHit2D[] approachingGroundHits = new RaycastHit2D[5];
    private RaycastHit2D _approachingGroundHit;
    RaycastHit2D[] ceilingHits = new RaycastHit2D[5];
    private RaycastHit2D _ceilingHit; // hitting head on ceiling
    RaycastHit2D[] wallHits = new RaycastHit2D[5];

    CapsuleCollider2D touchingCol;
    [SerializeField] private BoxCollider2D feetCol;
    [Range(0f, 1f)] public float headWidth = 1f;


    private Vector2 wallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left; // => means the value automatically gets updated like in the update() method

    [SerializeField]
    private bool _isGrounded = false;
    public bool IsGrounded { get 
        { 
            return _isGrounded;
        } 
        private set
        {
            _isGrounded = value;
            animator.SetBool(AnimationStrings.isGrounded, value);
        }  
    }

    [SerializeField]
    private bool _isApproachingGround = false;
    public bool IsApproachingGround
    {
        get
        {
            return _isApproachingGround;
        }
        private set
        {
            _isApproachingGround = value;
        }
    }
    
    [SerializeField]
    private bool _isOnWall = false;
    public bool IsOnWall
    {
        get
        {
            return _isOnWall;
        }
        private set
        {
            _isOnWall = value;
            animator.SetBool(AnimationStrings.isOnWall, value);
        }
    }

    [SerializeField]
    private bool _isOnCeiling = false;
    public bool IsOnCeiling
    {
        get
        {
            return _isOnCeiling;
        }
        private set
        {
            _isOnCeiling = value;
            animator.SetBool(AnimationStrings.isOnCeiling, value);
        }
    }

    Rigidbody2D rb;
    Animator animator;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingCol = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {


        //GroundCheckMethod();
    }

    void FixedUpdate()
    {
        /*
        Vector2 boxCastOrigin = new Vector2(feetCol.bounds.center.x, feetCol.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetCol.bounds.size.x, groundDistance);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, groundDistance, layerMask);
        if (_groundHit.collider != null)
        {
            IsGrounded = true;
        } 
        else
        {
            IsGrounded = false;
        }
        */
        IsGrounded = touchingCol.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;
        IsApproachingGround = touchingCol.Cast(Vector2.down, castFilter, approachingGroundHits, approachingGroundDistance) > 0;

        IsOnWall = touchingCol.Cast(wallCheckDirection, castFilter, wallHits, wallDistance) > 0;
        IsOnCeiling = touchingCol.Cast(Vector2.up, castFilter, ceilingHits, ceilingDistance) > 0 && !IsGrounded; // added && !IsGrounded
    }


    // method uses a raycast to check below the player. Use this ray hit info to update player position (outdated)
    private void GroundCheckMethod()
    {
        /*
        GroundHit2 = Physics2D.Raycast(rayCastOrigin.position, -Vector2.up, 100f, layerMask);

        //Performant check to see if raycast hit has any data
        if (GroundHit2D != false)
        {
            //Vector2 temp = playerFeet.position;
            //temp.y = GroundHit2D.point.y;
            //playerFeet.position = temp;
        }
        */
    }

}
