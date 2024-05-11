using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Setting")]
    [SerializeField]private float walking = 4.5f;
    [Space(5)]

    
    [Header("Vertical(JUMP) Movement Setting")]
    [SerializeField]private float jumpforce = 25f;
    private int jumpBufferCounter = 0;
    [SerializeField]private int jumpBufferFrames;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter =0;
    [SerializeField] private int maxAirJumps;
    [Space(5)]

    
    [Header("Ground Check Setting")]
    [SerializeField]private Transform groundCheckPoint;
    [SerializeField]private float groundCheckX = 0.5f;
    [SerializeField]private float groundCheckY = 0.2f;
    [SerializeField]private LayerMask whatIsGround;
    [Space(5)]

    
    [Header("Dashing Check Setting")]
    [SerializeField]private float dashSpeed;
    [SerializeField]private float dashTime;
    [SerializeField]private float dashCooldown;
    [SerializeField]GameObject dashEffect;
    [Space(5)]

    [Header("Projectile Test Attack ")]
    public GameObject ProjectilePrefab;
    public Transform LaunchOffset;


    PlayerStateList pState;
    private Rigidbody2D rb;
    private float hAxis;
    private float gravity;
    Animator anims;
    private bool canDash = true;
    private bool dashed;


    public static PlayerController Instance;
    
    private void Awake() {
        if(Instance != null && Instance !=this )
        {
            // Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();
        anims = GetComponent<Animator>();
        gravity = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariable();
        if(pState.dashing) return;
        Flip();
        Move();
        Jump();
        startDash();
        Projectile();
    }

    void GetInputs()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
    }

    void Flip()
    {
        if(hAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
        else if(hAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
    
    }

    private void Move()
    {
        rb.velocity = new Vector2(walking * hAxis, rb.velocity.y);
        anims.SetBool("Walking", rb.velocity.x != 0 && Grounded());
    }

    void startDash()
    {
        if(Input.GetButtonDown("Dash")&& canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }
        if (Grounded())
        {
            dashed = false;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        anims.SetTrigger("Dashing");
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        if (Grounded()) Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    public bool Grounded()
    {
        if(Physics2D.Raycast(groundCheckPoint.position,Vector2.down, groundCheckY,whatIsGround)
        || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0),Vector2.down, groundCheckY,whatIsGround)
        || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0),Vector2.down, groundCheckY,whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Jump()
    {
        if(Input.GetButtonDown("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);

            pState.jumping = false;
        }

        if(!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
            rb.velocity = new Vector3(rb.velocity.x, jumpforce);
            
            pState.jumping = true;
            }
            else if(!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                pState.jumping = true;

                airJumpCounter++;

                rb.velocity = new Vector3(rb.velocity.x, jumpforce);

            }

        }
        anims.SetBool("Jumping", !Grounded());
    }

    void UpdateJumpVariable()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if(Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }

    void Projectile()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Instantiate(ProjectilePrefab, LaunchOffset.position, transform.rotation);
        }
    }

}
