using System.Collections;
using UnityEngine;

public class GhostAppear : MonoBehaviour
{
    public float appearDuration = 3f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(Appear());
    }

    IEnumerator Appear()
    {
        Color color = spriteRenderer.color;

        // Полная прозрачность
        color.a = 0f;
        spriteRenderer.color = color;

        float timer = 0f;

        while (timer < appearDuration)
        {
            timer += Time.deltaTime;

            color.a = Mathf.Lerp(0f, 1f, timer / appearDuration);
            spriteRenderer.color = color;

            yield return null;
        }

        color.a = 1f;
        spriteRenderer.color = color;
    }
}

