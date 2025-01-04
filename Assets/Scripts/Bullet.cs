using UnityEngine;

public class Bullet : MonoBehaviour
{
    const float bulletSpeed = 20f;
    Rigidbody2D rb2d;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        Vector3 bulletPos = transform.position;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        float deltaX = mouseWorldPos.x - bulletPos.x;
        float deltaY = mouseWorldPos.y - bulletPos.y;
        float angle = Mathf.Atan2(deltaY, deltaX);
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        rb2d.linearVelocity = direction * bulletSpeed;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null)
        {
            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);    
    }
}
