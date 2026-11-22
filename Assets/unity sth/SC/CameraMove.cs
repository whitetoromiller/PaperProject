using System;
using System.Collections;
using UnityEngine;

namespace Assets.Welcome
{
    public class CameraMove : MonoBehaviour
    {
        [SerializeField] private Vector3 pointA;
        [SerializeField] private Vector3 pointB;
        [SerializeField] private float duration = 5f;
        // Use this for initialization
        private void Start(){
            StartCoroutine(MoveCamera());
        }
        
        private IEnumerator MoveCamera()
        {
            float elapsed = 0f;
            transform.position = pointA;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(pointA, pointB, t);
                yield return null;
            }
            transform.position = pointB;
        }
    }
}