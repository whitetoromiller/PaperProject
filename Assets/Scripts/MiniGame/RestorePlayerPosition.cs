
using UnityEngine;

public class RestorePlayerPosition : MonoBehaviour
{
    void Start()
    {
        if (MiniGameManager.shouldReturn)
        {
            transform.position = MiniGameManager.returnPosition;
            MiniGameManager.shouldReturn = false;
        }
    }
}
