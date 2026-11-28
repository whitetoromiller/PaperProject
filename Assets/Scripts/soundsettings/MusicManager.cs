
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip music;
    }

    [Header("Музыка для сцен")]
    [SerializeField] private SceneMusic[] sceneMusics;

    [Header("Настройки")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    private void Start()
    {
        PlayMusicForCurrentScene();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        AudioClip targetMusic = null;

        foreach (SceneMusic sceneMusic in sceneMusics)
        {
            if (sceneMusic.sceneName == currentScene)
            {
                targetMusic = sceneMusic.music;
                break;
            }
        }

        if (targetMusic == audioSource.clip)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(ChangeMusic(targetMusic));
    }

    private IEnumerator ChangeMusic(AudioClip newMusic)
    {
        float startVolume = audioSource.volume;

        // Плавно уменьшаем громкость старой музыки
        while (audioSource.volume > 0.01f)
        {
            audioSource.volume = Mathf.MoveTowards(
                audioSource.volume,
                0f,
                Time.unscaledDeltaTime * volume / fadeDuration
            );

            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newMusic;

        if (newMusic == null)
        {
            audioSource.volume = 0f;
            yield break;
        }

        audioSource.Play();

        // Плавно увеличиваем громкость новой музыки
        while (audioSource.volume < volume)
        {
            audioSource.volume = Mathf.MoveTowards(
                audioSource.volume,
                volume,
                Time.unscaledDeltaTime * volume / fadeDuration
            );

            yield return null;
        }

        audioSource.volume = volume;
        fadeCoroutine = null;
    }
}
