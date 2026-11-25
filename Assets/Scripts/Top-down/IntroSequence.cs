
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroSequence : MonoBehaviour
{
    [Header("Фон")]
    public Image backgroundImage;

    [Header("Картинки истории")]
    public Image introImage;
    public Sprite[] pictures = new Sprite[9];

    [Header("Настройки")]
    public float showTime = 3f;
    public float fadeTime = 1f;

    [Header("Переход")]
    public string nextSceneName;

    private void Start()
    {
        backgroundImage.gameObject.SetActive(true);
        introImage.gameObject.SetActive(true);

        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        for (int i = 0; i < pictures.Length; i++)
        {
            introImage.sprite = pictures[i];

            Color color = introImage.color;
            color.a = 0;
            introImage.color = color;

            yield return StartCoroutine(Fade(0, 1));

            yield return new WaitForSeconds(showTime);

            yield return StartCoroutine(Fade(1, 0));
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator Fade(float start, float end)
    {
        float timer = 0;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(start, end, timer / fadeTime);

            Color color = introImage.color;
            color.a = alpha;
            introImage.color = color;

            yield return null;
        }

        Color finalColor = introImage.color;
        finalColor.a = end;
        introImage.color = finalColor;
    }
}
