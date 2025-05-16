using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PanelAnimation : MonoBehaviour
{
    [Header ("___ Simple DOTween show/hide animations ___")]
    [SerializeField] private Vector2 targetPosition; // Финальная позиция, к которой будет целевое передвижение
    [Range (0.1f, 5f)]
    [SerializeField] private float speed = 0.5f; // Скорость (очевидно)
    [SerializeField] private Button[] changeState; // Сюда вставить все кнопки

    private bool needShow = true; // Булевое поле, которое будет отвечать за то, какую анимацию нам врубить
    private Vector2 initialPosition; // Сюда будет записана стартовая позиция
    private RectTransform rect; // Ссылка на RectTransform

    // Простая инитиализация
    private void Start()
    {
        rect = gameObject.GetComponent<RectTransform>();
        if(rect != null) initialPosition = rect.anchoredPosition;

        foreach(Button button in changeState)
        {
            button.onClick.AddListener(ChangeAnimState);
        }
    }

#region Button state

    // Изменение возможности клика по кнопкам
    private void ButtonsState(bool State)
    {
        foreach(Button button in changeState)
        {
            button.interactable = State;
        }
    }

#endregion

#region Animate panel

    // Показать панель
    private void Show()
    {
        ButtonsState(false);
        rect.DOAnchorPos(targetPosition, speed).OnComplete(()=> {
            ButtonsState(true);
        });
    }

    // Спрятать панель
    private void Hide()
    {
        ButtonsState(false);
        rect.DOAnchorPos(initialPosition, speed).OnComplete(()=> {
            ButtonsState(true);
        });
    }

#endregion

#region Request to animation

    // Изменить свойство (Что-то похожее на переключатель)
    public void ChangeAnimState()
    {
        if(needShow) {
            Show();
        }
        else {
            Hide();
        }

        needShow = !needShow;
    }

#endregion
}
