using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float prevLeftX = 0f;
    private float prevLeftY = 0f;
    private float prevRightX = 0f;
    private float prevRightY = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private bool isDead;
    [HideInInspector] public bool playerUnlocked;
    [HideInInspector] public bool extraLife;

    [Header("VFX")]
    [SerializeField] private ParticleSystem dustFx;
    [SerializeField] private ParticleSystem bloodFx;

    [Header("Knockback info")]
    [SerializeField] private Vector2 knockbackDir;
    private bool isKnocked;
    private bool canBeKnocked = true;

    [Header("Move info")]
    [SerializeField] private float speedToSurvive = 18;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedMultiplier;
    private float defaultSpeed;
    [Space]
    [SerializeField] private float milestoneIncreaser;
    private float defaultMilestoneIncrease;
    private float speedMilestone;

    private bool readyToLand;

    [Header("Jump info")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private bool canDoubleJump;
    


    [Header("Slide info")]
    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideTime;
    [SerializeField] private float slideCooldown;
    [HideInInspector] public float slideCooldownCounter;
    private float slideTimeCounter;
    private bool isSliding;

    [Header("Collision info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float ceillingCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize;
    private bool isGrounded;
    private bool wallDetected;
    private bool ceillingDetected;
    [HideInInspector] public bool ledgeDetected;

    [Header("Ledge info")]
    [SerializeField] private Vector2 offset1; // offset for position before climb
    [SerializeField] private Vector2 offset2; // offset for position AFTER climb

    private Vector2 climbBegunPosition;
    private Vector2 climbOverPosition;

    private bool canGrabLedge = true;
    private bool canClimb;

    private float prevDpadX = 0f;
    private float prevDpadY = 0f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        

        speedMilestone = milestoneIncreaser;
        defaultSpeed = moveSpeed;
        defaultMilestoneIncrease = milestoneIncreaser;
    }

    void Update()
    {
        CheckCollision();
        AnimatorControllers();

        slideTimeCounter -= Time.deltaTime;
        slideCooldownCounter -= Time.deltaTime;

        extraLife = moveSpeed >= speedToSurvive;
        //if (Input.GetKeyDown(KeyCode.K))
        //    Knockback();

        //if (Input.GetKeyDown(KeyCode.O) && !isDead)
        //    StartCoroutine(Die());


        if (isDead)
            return;

        if (isKnocked)
            return;

        if (playerUnlocked)
            SetupMovement();

        if (isGrounded)
        {
            Time.timeScale = 1;
            canDoubleJump = true;
        }

        SpeedController();

        CheckForLanding();
        CheckForLedge();
        CheckForSlideCancel();
        CheckInput();

        if (transform.position.y < -10f) // Adjust -10f to your fall-off point
        {
            this.gameObject.SetActive(false);
        }
    }

    private void CheckForLanding()
    {
        if (rb.velocity.y < -5 && !isGrounded)
            readyToLand = true;

        if (readyToLand && isGrounded)
        {
            dustFx.Play();
            readyToLand = false;
        }
    }

    public void Damage()
    {
        //bloodFx.Play();

        if (extraLife)
            Knockback();
        else
            StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        AudioManager.instance.PlaySFX(3);
        isDead = true;
        canBeKnocked = false;
        rb.velocity = knockbackDir;
        anim.SetBool("isDead", true);

        Time.timeScale = .6f;

        yield return new WaitForSeconds(1f);

        Time.timeScale = 1f;
        rb.velocity = new Vector2(0, 0);
        GameManager.instance.GameEnded();
        
    }


    #region Knockback
    private IEnumerator Invincibility()
    {
        Color originalColor = sr.color;
        Color darkenColor = new Color(sr.color.r, sr.color.g, sr.color.b, .5f);

        canBeKnocked = false;
        sr.color = darkenColor;
        yield return new WaitForSeconds(.1f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.1f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.15f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.15f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.25f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.25f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.3f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.35f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.4f);

        sr.color = originalColor;
        canBeKnocked = true;
    }

    private void Knockback()
    {
        if (!canBeKnocked)
            return;

        SpeedReset();
        StartCoroutine(Invincibility());
        isKnocked = true;
        rb.velocity = knockbackDir;
        //this.gameObject.SetActive(false);
    }

    private void CancelKnockback() => isKnocked = false;

    #endregion

    #region SpeedControll
    private void SpeedReset()
    {
        if (isSliding)
            return;

        moveSpeed = defaultSpeed;
        milestoneIncreaser = defaultMilestoneIncrease;
    }

    private void SpeedController()
    {
        if (moveSpeed == maxSpeed)
            return;


        if (transform.position.x > speedMilestone)
        {
            speedMilestone = speedMilestone + milestoneIncreaser;

            moveSpeed = moveSpeed * speedMultiplier;
            milestoneIncreaser = milestoneIncreaser * speedMultiplier;

            if (moveSpeed > maxSpeed)
                moveSpeed = maxSpeed;
        }
    }
    #endregion

    #region Ledge Climb Region

    private void CheckForLedge()
    {
        if (ledgeDetected && canGrabLedge)
        {
            canGrabLedge = false;
            rb.gravityScale = 0;

            Vector2 ledgePosition = GetComponentInChildren<LedgeDetection>().transform.position;

            climbBegunPosition = ledgePosition + offset1;
            climbOverPosition = ledgePosition + offset2;

            canClimb = true;
        }

        if (canClimb)
            transform.position = climbBegunPosition;
    }

    private void LedgeClimbOver()
    {

        canClimb = false;
        rb.gravityScale = 5;
        transform.position = climbOverPosition;
        Invoke("AllowLedgeGrab", .1f);
    }

    private void AllowLedgeGrab() => canGrabLedge = true;


    #endregion

    private void CheckForSlideCancel()
    {
        if (slideTimeCounter < 0 && !ceillingDetected)
            isSliding = false;
    }
    private void SetupMovement()
    {

        if (wallDetected)
        {
            SpeedReset();
            return;
        }

        if (isSliding)
            rb.velocity = new Vector2(slideSpeed, rb.velocity.y);
        else
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    }


    #region Inputs
    public void SlideButton()
    {
        if (isDead)
            return;

        if (rb.velocity.x != 0 && slideCooldownCounter < 0)
        {
            dustFx.Play();
            isSliding = true;
            slideTimeCounter = slideTime;
            slideCooldownCounter = slideCooldown;
        }
    }

    public void JumpButton()
    {
        if (isSliding || isDead)
            return;

        RollAnimFinished();

        if (isGrounded)
        {
            Jump(jumpForce);
        }
        else if (canDoubleJump)
        {
            canDoubleJump = false;
            Jump(doubleJumpForce);
        }
    }

    private void Jump(float force)
    {
        dustFx.Play();
        AudioManager.instance.PlaySFX(Random.Range(1, 2));
        rb.velocity = new Vector2(rb.velocity.x, force);
    }

    private void CheckInput()
    {
        // D-pad
        float dpadX = Input.GetAxisRaw("DPad_XAxis");
        float dpadY = Input.GetAxisRaw("DPad_YAxis");
        bool dpadPressed = (prevDpadX == 0 && dpadX != 0) || (prevDpadY == 0 && dpadY != 0);

        // Left stick
        float leftX = Input.GetAxisRaw("Horizontal"); // default "Horizontal"
        float leftY = Input.GetAxisRaw("Vertical");   // default "Vertical"
        bool leftStickPressed = (prevLeftX == 0 && leftX != 0) || (prevLeftY == 0 && leftY != 0);

        // Right stick
        float rightX = Input.GetAxisRaw("RightStick_X");
        float rightY = Input.GetAxisRaw("RightStick_Y");
        bool rightStickPressed = (prevRightX == 0 && rightX != 0) || (prevRightY == 0 && rightY != 0);

        if (Input.GetButtonDown("Jump") ||
            Input.GetKeyDown(KeyCode.JoystickButton0) || // A
            Input.GetKeyDown(KeyCode.JoystickButton1) || // B
            Input.GetKeyDown(KeyCode.JoystickButton2) || // X
            Input.GetKeyDown(KeyCode.JoystickButton3) || // Y
            dpadPressed || leftStickPressed || rightStickPressed)
        {
            JumpButton();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
            SlideButton();

        // Save previous values
        prevDpadX = dpadX;
        prevDpadY = dpadY;
        prevLeftX = leftX;
        prevLeftY = leftY;
        prevRightX = rightX;
        prevRightY = rightY;
    }

    bool IsDpadPressed()
    {
        float dpadX = Input.GetAxisRaw("DPad_XAxis"); // D-pad left/right
        float dpadY = Input.GetAxisRaw("DPad_YAxis"); // D-pad up/down

        return dpadX != 0 || dpadY != 0;
    }
    #endregion
    #region Animations
    private void AnimatorControllers()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);

        anim.SetBool("canDoubleJump", canDoubleJump);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("canClimb", canClimb);
        anim.SetBool("isKnocked", isKnocked);

        if (rb.velocity.y < -20)
            anim.SetBool("canRoll", true);

    }

    private void RollAnimFinished() => anim.SetBool("canRoll", false);

    #endregion

    private void CheckCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        ceillingDetected = Physics2D.Raycast(transform.position, Vector2.up, ceillingCheckDistance, whatIsGround);
        wallDetected = Physics2D.BoxCast(wallCheck.position, wallCheckSize, 0, Vector2.zero, 0, whatIsGround);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y + ceillingCheckDistance));
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}
