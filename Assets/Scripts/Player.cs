using System.Collections;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb2d;
    const float speed = 5f;
    const float jumpForce = 5f;
    const float dashForce = 30f;
    int jumpCount = 0;
    const int maxJumpCount = 2;
    const float bulletOffset = 1f;
    int bulletShot = 0;
    const int bulletMag = 30;

    const float coolDownTime = 0.1f;
    float _nextFireTime;
    private bool IsCoolDown => Time.time < _nextFireTime;
    private void StartCoolDown() => _nextFireTime = Time.time + coolDownTime;

    const float reloadTime = 3f;

    [SerializeField]
    GameObject prefabBullet;
    [SerializeField]
    GameObject prefabWeapon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        _nextFireTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Shoot();
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
        Vector3 position = transform.position;
        float horizontalInput = Input.GetAxis("Horizontal");
        bool jumped = Input.GetKeyDown(KeyCode.Space);
        bool dashed = Input.GetKeyDown(KeyCode.LeftShift);
        rb2d.linearVelocity = new Vector2(horizontalInput * speed, rb2d.linearVelocityY);

        if (jumped && jumpCount < maxJumpCount)
        {
            rb2d.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;
        }

        if (dashed && horizontalInput != 0)
        {
            rb2d.AddForce(new Vector2(Mathf.Sign(horizontalInput), 0) * dashForce 
                , ForceMode2D.Impulse);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
        }
    }
}
