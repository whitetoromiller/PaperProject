using UnityEngine;

public class PlayerMoveH : MonoBehaviour
{
    public float speed = 5f;

    public Sprite upS;
    public Sprite downS;
    public Sprite leftS;
    public Sprite rightS;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRen;

    private Vector2 movement;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRen = GetComponent<SpriteRenderer>();

        spriteRen.sprite = downS;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.x > 0) spriteRen.sprite = rightS;
        else if (movement.x < 0) spriteRen.sprite = leftS;
        else if (movement.y > 0) spriteRen.sprite = upS;
        else if (movement.y < 0) spriteRen.sprite = downS;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }
}
