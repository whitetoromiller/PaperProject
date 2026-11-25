using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private AudioClip footstepsSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Получаем нажатие клавиш движения
        bool isMoving =
            Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0;

        // Если персонаж движется и звук не играет
        if (isMoving && !audioSource.isPlaying)
        {
            audioSource.clip = footstepsSound;
            audioSource.Play();
        }

        // Если персонаж остановился
        if (!isMoving && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}