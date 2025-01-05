using System.Collections;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    bool isFacingRight = true;
    float horizontalInput;
    const float speed = 5f;
    const float jumpForce = 7f;
    const float doubleJumpForce = 8f;
    bool doubleJump;
    const float bulletOffset = 1f;
    int bulletShot = 0;
    const int bulletMag = 30;

    const float coolDownTime = 0.1f;
    float _nextFireTime;
    private bool IsCoolDown => Time.time < _nextFireTime;
    private void StartCoolDown() => _nextFireTime = Time.time + coolDownTime;
    const float reloadTime = 3f;

    bool canDash = true;
    bool isDashing;
    float dashingForce = 24f;
    float dashingTime = 0.2f;
    float dashingCooldown = 0.5f;

    bool isWallJumping;
    float wallJumpingDirection;
    float wallJumpingTime = 0.2f;
    float wallJumpingCounter;
    float wallJumpingDuration = 0.4f;
    Vector2 wallJumpingPower = new Vector2(8f, 16f);

    bool isWallSliding;
    const float wallSlidingSpeed = 2f;

    int hookShot = 0;

    Rigidbody2D rb2d;
    [SerializeField] GameObject prefabBullet;
    [SerializeField] Hook prefabHook;
    [SerializeField] TrailRenderer tr;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform wallCheck;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] Camera mainCamera;
    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] DistanceJoint2D _distanceJoint;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _distanceJoint.enabled = false;
        rb2d = GetComponent<Rigidbody2D>();
        _nextFireTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Shoot();
        if (!isWallJumping)
        {
            Flip();
        }
        Hook();

    }

    void Hook()
    {
        if (Input.GetButtonDown("Hook"))
        {
            Vector2 mousePos = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _lineRenderer.SetPosition(0, mousePos);
            _lineRenderer.SetPosition(1, transform.position);
            _distanceJoint.connectedAnchor = mousePos;
            _distanceJoint.enabled = true;
            _lineRenderer.enabled = true;
        }
        else if (Input.GetButtonUp("Hook"))
        {
            _distanceJoint.enabled = false;
            _lineRenderer.enabled = false;
        }
        if (_distanceJoint.enabled)
        {
            _lineRenderer.SetPosition(1, transform.position);
        }
    }


    void Shoot()
    {
        if (!IsCoolDown && bulletShot < bulletMag)
        {
            float shoot = Input.GetAxis("Shoot");
            if (shoot > 0)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = -Camera.main.transform.position.z;
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
                Vector3 playerPos = transform.position;
                Vector3 direction = (mouseWorldPos - playerPos).normalized;
                Instantiate<GameObject>(prefabBullet, playerPos + direction * bulletOffset, Quaternion.identity);
                bulletShot++;
                StartCoolDown();
            }
        }else if (bulletShot >= bulletMag)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);
        bulletShot = 0;
        Debug.Log("Reload complete!");
    }

    void Move()
    {
        if (isDashing)
        {
            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        
        if (!isWallJumping) 
        {
            rb2d.linearVelocity = new Vector2(horizontalInput * speed, rb2d.linearVelocityY);
        }

        if (Input.GetButtonDown("Dash") && canDash)
        {
            StartCoroutine(Dash());
        }

        Jump();
        WallSlide();
        WallJump();
    }


    void Jump()
    {
        if (isGrounded() && !Input.GetButton("Jump"))
        {
            doubleJump = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded() || doubleJump)
            {
                rb2d.linearVelocity = new Vector2(rb2d.linearVelocityX, doubleJump ? doubleJumpForce : jumpForce);
                doubleJump = !doubleJump;
            }
        }

        if (Input.GetButtonUp("Jump") && !isGrounded())
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocityX, rb2d.linearVelocityY * 0.5f);
        }
    }

    void WallSlide()
    {
        if (isWalled() && !isGrounded() && horizontalInput != 0f)
        {
            isWallSliding = true;
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocityX,
                Mathf.Clamp(rb2d.linearVelocityY, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0)
        {
            isWallJumping = true;
            rb2d.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0;
            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localscale = transform.localScale;
                localscale.x *= -1f;
                transform.localScale = localscale;
            }
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    void StopWallJumping()
    {
        isWallJumping = false;
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb2d.gravityScale;
        rb2d.gravityScale = 0;
        float dashDirection = Mathf.Sign(Input.GetAxis("Horizontal"));
        rb2d.linearVelocityX = dashDirection * dashingForce;
        
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        
        tr.emitting = false;
        rb2d.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        
        canDash = true;
    }

    bool isGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    bool isWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    void Flip()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localscale = transform.localScale;
            localscale.x *= -1f;
            transform.localScale = localscale;
        }
    }
}
