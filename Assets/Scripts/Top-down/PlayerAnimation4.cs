using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class PlayerAnimation4 : MonoBehaviour
{
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    public Sprite idleSprite;

    public float moveThreshold = 0.1f; 
    public float idleDelay = 0.08f;   

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Sprite current;
    private Sprite lastDirectionSprite;
    private float idleTimer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        lastDirectionSprite = downSprite; 
        current = idleSprite != null ? idleSprite : lastDirectionSprite;
        if (current != null) sr.sprite = current;
    }

    void LateUpdate()
    {
        Vector2 v = rb.linearVelocity; 
        Sprite target = current;

        if (v.magnitude > moveThreshold)
        {
            idleTimer = 0f;


            if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
                lastDirectionSprite = v.x > 0 ? rightSprite : leftSprite;
            else
                lastDirectionSprite = v.y > 0 ? upSprite : downSprite;

            target = lastDirectionSprite;
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > idleDelay)
                target = idleSprite != null ? idleSprite : lastDirectionSprite;
        }

        if (target != null && target != current)
        {
            current = target;
            sr.sprite = current;
        }
    }
}