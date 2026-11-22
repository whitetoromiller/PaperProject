// using UnityEngine;
// using System;
// using System.Collections;
// using TMPro;

// namespace Assets.Welcome {
//     public class Huh_text: MonoBehaviour
//     {
//         private int[] nums = { 0, 0, 0 };

//         public TMP_Text mytext;

//         void update_text()
//         {
//             mytext.text = nums[0].ToString() + " " +  nums[1].ToString() + " " + nums[2].ToString();
//         }

//         public void upup(int num)
//         {
//             nums[num] = (nums[num] + 1) % 10;
//             update_text();
//         }

//         public void dodo(int num)
//         {
//             nums[num] = (nums[num] - 1) % 10;
//             if (nums[num] < 0) nums[num] += 10;
//             update_text();
//         }
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Assets.Welcome
{
    public class Huh_text : MonoBehaviour
    {
        private int[] nums = { 0, 0, 0 };

        [Header("Текст комбинации")]
        public TMP_Text mytext;

        [Header("Кнопки увеличения")]
        public Button[] upButtons = new Button[3];

        [Header("Кнопки уменьшения")]
        public Button[] downButtons = new Button[3];

        [Header("Кнопка проверки")]
        public Button checkButton;

        [Header("Правильная комбинация")]
        public int[] correctCode = { 4, 6, 7 };

        [Header("Сцена для перехода")]
        public string sceneName = "Stone";

        [Header("Эффект неправильного ответа")]
        public SpriteRenderer backgroundImage;
        public Color flashColor = Color.red;
        public float flashDuration = 0.2f;

        private Color originalColor;
        private Coroutine flashCoroutine;

        void Start()
        {
            for (int i = 0; i < 3; i++)
            {
                int index = i;

                upButtons[index].onClick.AddListener(() => upup(index));
                downButtons[index].onClick.AddListener(() => dodo(index));
            }

            checkButton.onClick.AddListener(CheckCode);

            if (backgroundImage != null)
                originalColor = backgroundImage.color;

            update_text();
        }

        void update_text()
        {
            mytext.text = nums[0] + " " +
                          nums[1] + " " +
                          nums[2];
        }

        void upup(int num)
        {
            nums[num] = (nums[num] + 1) % 10;
            update_text();
        }

        void dodo(int num)
        {
            nums[num] = (nums[num] - 1 + 10) % 10;
            update_text();
        }

        void CheckCode()
        {
            if (nums[0] == correctCode[0] &&
                nums[1] == correctCode[1] &&
                nums[2] == correctCode[2])
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.Log("Неверная комбинация!");

                if (backgroundImage != null)
                {
                    if (flashCoroutine != null)
                        StopCoroutine(flashCoroutine);

                    flashCoroutine = StartCoroutine(FlashBackground());
                }
            }
        }

        IEnumerator FlashBackground()
        {
            backgroundImage.color = flashColor;

            yield return new WaitForSeconds(flashDuration);

            backgroundImage.color = originalColor;

            flashCoroutine = null;
        }
    }
}
