using UnityEngine;

public class CameraFlow : MonoBehaviour
{
    public Transform target;
    public float smooth = 5f;

    void LateUpdate()
    {
        Vector3 pos = new Vector3(target.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, pos, smooth * Time.deltaTime);
    }
}
