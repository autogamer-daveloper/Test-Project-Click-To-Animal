using UnityEngine;
using UnityEngine.UI;

// Этого кстати ещё не было в этом проекте, решил добавить
[RequireComponent(typeof(Button))]
public class OpenLink : MonoBehaviour
{
    // Я уже задолбался писать скрипты, сделаю максимально просто, Всё равно в ТЗ не входит, просто для галочки.
    [Header ("___ Open link ___")]
    [SerializeField] private string URL = "https://www.google.com"; // Просто пример

    // Берём кнопку
    private void Start()
    {
        var button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(OpenCustomLink);
    }

    // Пихаем туда слушатель с этим методом
    private void OpenCustomLink()
    {
        Application.OpenURL(URL);
    }
}
