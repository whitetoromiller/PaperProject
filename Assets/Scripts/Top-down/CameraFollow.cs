
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Chapter;      
    public Transform Background;   

    public float smooth = 5f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

    }

    void LateUpdate()
    {
        if (Chapter == null || Background == null)
            return;

        SpriteRenderer background = Background.GetComponent<SpriteRenderer>();



        Vector3 pos = new Vector3(
            Chapter.position.x,
            Chapter.position.y,
            transform.position.z
        );


        Bounds bounds = background.bounds;


        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;


        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;

        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;


        if (minX <= maxX)
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
        else
            pos.x = bounds.center.x;

        if (minY <= maxY)
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        else
            pos.y = bounds.center.y;


        transform.position = Vector3.Lerp(
            transform.position,
            pos,
            smooth * Time.deltaTime
        );
    }
}

