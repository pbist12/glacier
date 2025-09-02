using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float speed;
    public float detailSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 currentPos = transform.position;
        Vector3 nextPos = new Vector3(h, v, 0) * speed * Time.deltaTime;

        if(Input.GetKey(KeyCode.LeftShift) )
        {
            nextPos = new Vector3(h, v, 0) * detailSpeed * Time.deltaTime;
        }

        transform.position = currentPos + nextPos;
    }
}
