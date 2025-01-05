using Unity.VisualScripting;
using UnityEngine;

public class Hook : MonoBehaviour
{
    [SerializeField] Transform wallCheck;
    [SerializeField] LayerMask wallLayer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        while (!isWalled())
        {
            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;
            pos.x += 1;
            pos.y += 1;
            scale.x += 2;
            transform.localScale = scale;
            transform.position = pos;
        }
    }

    public void hookShoot(Vector3 direction)
    {

    }

    private bool isWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }
}
