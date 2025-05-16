using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

// Нужен, просто нужен.
[RequireComponent(typeof(GameManager))]
public class BlockButtons : MonoBehaviour
{
    [Range (0.2f, 5.1f)]
    [SerializeField] private float speedInit = 0.75f; // Кд на включение
    private const string PlayingTag = "Playing"; // Тег кнопок, которые будут выключены на время
    [Range (2, 14)]
    [SerializeField] private int NeedToUnfreeze = 5; // Сколько нужно шаров поймать перед разморозкой
    [SerializeField] private GameObject[] _buttons; // Объекты кнопок
    [SerializeField] private EventTrigger[] _buttonsUI; // Сами кнопки
    [SerializeField] private float winTime = 0.5f; // Задержка вывода панели победы
    [Header ("___ Settings - Audio ___")]
    [SerializeField] private AudioClip sfxWin; // Аудио-эффект победы

    [HideInInspector] internal int DeactivatedButtons = 0;

    private UnityAction<BaseEventData> cachedCallback; // Сохранённый слушатель, которого мы будет удалять/добавлять
    private bool isDeactivated = false; // Защита от двойного срабатывания метода
    private AudioSource src; // Источник звука

    // Ничего особенного, инитиализация источника звука
    private void Start()
    {
        var srcObj = GameObject.FindWithTag("AudioSourceDefault");
        if(srcObj != null) src = srcObj.GetComponent<AudioSource>();
    }

/* Достаточно интересный метод. Так вот, тут мы ищем именно игровые кнопки, у которых ещё остался тег PlayingTag. После нажатия на любую из
этих кнопок, её тег в другом скрипте меняется, НО, все кнопки деактивируются, и только оставшиеся будут включены обратно после кд. с EventSystem
запарился конкретно... Вот так вот...*/
    internal void InitializeButtons()
    {
        _buttons = GameObject.FindGameObjectsWithTag(PlayingTag);
        _buttonsUI = new EventTrigger[_buttons.Length];

        for(int i = 0; i < _buttons.Length; i++)
        {
            var tempButton = _buttons[i].GetComponent<EventTrigger>();
            _buttonsUI[i] = tempButton;
            _buttonsUI[i].enabled = true;

            var eventTrigger = _buttons[i].GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                return;
            }

            if (cachedCallback == null)
            {
                cachedCallback = (data) => { DeactivateButtons(); };
            }

            EventTrigger.Entry existingEntry = eventTrigger.triggers.Find(e => e.eventID == EventTriggerType.PointerDown);

            if (existingEntry != null)
            {
                existingEntry.callback.RemoveListener(cachedCallback);
            }
            else
            {
                existingEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerDown
                };
                eventTrigger.triggers.Add(existingEntry);
            }

            existingEntry.callback.AddListener(cachedCallback);
        }

        if(_buttons.Length <= 0) {
            Invoke(nameof(CheckWin), winTime);
            if(src != null && sfxWin != null) src.PlayOneShot(sfxWin);
        }
    }

// Это и есть метод, который вырубает все кнопки с 1 удара, на время естественно
    internal void DeactivateButtons()
    {
        if(isDeactivated) return;

        isDeactivated = true;

        foreach(EventTrigger but in _buttonsUI)
        {
            if (but != null)
            {
                but.enabled = false;
            }
        }

        DeactivatedButtons += 1;

        Invoke(nameof(ActivateButtons), speedInit);
    }

// Этот метод восстанавливает все кнопки, у которых остался тег PlayingTag, короче просто все переинитиализирует
// Также дополнительно с инитиализацией размораживает замоорженный шарик
    private void ActivateButtons()
    {
        isDeactivated = false;

        Debug.Log($"{DeactivatedButtons} >= {NeedToUnfreeze}");

        if(DeactivatedButtons >= NeedToUnfreeze) {
        var frozenButton = GameObject.FindGameObjectWithTag("Frozen");
        if(frozenButton != null) {
                var frozen = frozenButton.GetComponent<ButtonAnimation>();
                frozen.Unfreeze();
            }
        }

        InitializeButtons();
    }

#region Check win

    private void CheckWin()
    {
        var manager = gameObject.GetComponent<GameManager>();
        manager.YouWin();
    }

#endregion
}