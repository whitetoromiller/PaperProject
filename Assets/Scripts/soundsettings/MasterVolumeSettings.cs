using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MasterVolumeSettings : MonoBehaviour
{
    [Header("Volume Slider")]
    [SerializeField] private Slider masterVolumeSlider;

    [Header("Volume Text")]
    [SerializeField] private TMP_Text masterVolumeText;

    private const string VolumeKey = "MasterVolume";

    private void Start()
    {
        // Проверяем, назначен ли Slider
        if (masterVolumeSlider == null)
        {
            Debug.LogError("Master Volume Slider не назначен!");
            return;
        }

        // Настраиваем диапазон Slider
        masterVolumeSlider.minValue = 0f;
        masterVolumeSlider.maxValue = 1f;
        masterVolumeSlider.wholeNumbers = false;

        // Загружаем сохранённую громкость
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        savedVolume = Mathf.Clamp01(savedVolume);

        // Устанавливаем значение ползунка
        masterVolumeSlider.SetValueWithoutNotify(savedVolume);

        // Применяем громкость
        SetVolume(savedVolume);

        // Обновляем текст
        UpdateVolumeText(savedVolume);

        // Подписываемся на изменение Slider
        masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    // Вызывается при изменении ползунка
    private void OnVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);

        // Изменяем общую громкость игры
        SetVolume(value);

        // Обновляем проценты
        UpdateVolumeText(value);

        // Сохраняем значение
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    // Устанавливаем общую громкость
    private void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    // Обновляем отображение процентов
    private void UpdateVolumeText(float value)
    {
        if (masterVolumeText == null)
            return;

        int percent = Mathf.RoundToInt(value * 100f);

        masterVolumeText.text = percent + "%";
    }

    // Возвращаем громкость к 100%
    public void ResetVolume()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = 1f;
        }
    }

    // Сохраняем настройки при выходе из игры
    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    // Сохраняем настройки при сворачивании игры
    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            PlayerPrefs.Save();
        }
    }

    // Сохраняем настройки при отключении объекта
    private void OnDisable()
    {
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}
