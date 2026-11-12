using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Sprite rightSprite;
    public Sprite leftSprite;
    public Sprite idleSprite;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private float lastDirection = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        spriteRenderer.sprite = idleSprite;
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");

        if (moveX > 0)
        {
            lastDirection = 1;
            spriteRenderer.sprite = rightSprite;
        }
        else if (moveX < 0)
        {
            lastDirection = -1;
            spriteRenderer.sprite = leftSprite;
        }
        else
        {
            spriteRenderer.sprite = idleSprite;
        }
    }
}
