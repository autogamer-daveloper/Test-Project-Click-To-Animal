using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonAnimation : MonoBehaviour
{
    [Header ("___ Button animation with DOTween ___")]
    [SerializeField] private Vector2[] positionsInBar; // Заранее подготовленные позиции в экшен-баре
    [Range (0.1f, 5f)]
    [SerializeField] private float speed = 0.5f; // Скорость проигрывание анимации перекидывания шарика с поля на экшен-бар
    [Range (1f, 2f)]
    [SerializeField] private float scaler = 1.25f; // Увеличение фигурки на время перетаскивания (множитель)
    [Header ("___ Settings - Audio ___")]
    [SerializeField] private AudioClip sfx; // Аудио-эффект
    [Header ("___ Bonus settings - Frozen ___")]
    [Range (2, 14)]
    [SerializeField] private int NeedToUnfreeze = 5; // Сколько шаров нужно поймать перед разморозкой

    private AudioSource src; // Источник звука
    private const string PlayingTag = "Playing"; // Тег кнопок, которые будут выключены на время
    private GameManager manager; // игровой менеджер
    private GameObject physicSimulatedBall; // физическая симуляция мяча
    private int ballId = -1; // айди мяча
    [Range (0, 3)]
    private int ballType = 0; // тип мяча

    private UnityAction<BaseEventData> cachedCallback; // Сохранённый слушатель, которого мы будет удалять/добавлять

#region Initializing game manager

    private void Start()
    {
        var management = GameObject.FindWithTag("GameManagement");
        if(management != null) manager = management.GetComponent<GameManager>();

        var srcObj = GameObject.FindWithTag("AudioSourceDefault");
        if(srcObj != null) src = srcObj.GetComponent<AudioSource>();

        AddGamePlayListener();
    }

    public void AddGamePlayListener()
    {
        var eventTrigger = gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            return;
        }

        if (cachedCallback == null)
        {
            cachedCallback = (data) => { AnimateMovingToActionBar(); };
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

    public void InitializePhysicObject(GameObject obj)
    {
        physicSimulatedBall = obj;
    }

#endregion

#region Animations

    public void AnimateMovingToActionBar() // Вызывается, когда мы нажимаем на кнопку непосредственно на канвасе
    {
        var button = GetComponent<Button>();

        if(button.interactable == false) {
            return;
        }

        var _ButtonPhysicObject = physicSimulatedBall.GetComponent<ButtonPhysicObject>();
        if(_ButtonPhysicObject != null) _ButtonPhysicObject.GetInfo();

        if(ballType == 3) {
            var blockButtons = GameObject.FindFirstObjectByType<BlockButtons>();
            if(blockButtons.DeactivatedButtons < NeedToUnfreeze) return;
        }

        Destroy(physicSimulatedBall);

        gameObject.tag = "DontTouch";
        button.interactable = false;

        if(src != null && sfx != null) src.PlayOneShot(sfx);

        var rTransform = gameObject.GetComponent<RectTransform>();

        var finalBall = new Vector2(rTransform.sizeDelta.x * 0.75f, rTransform.sizeDelta.y * 0.75f);
        var scaledBall = new Vector2(rTransform.sizeDelta.x * scaler, rTransform.sizeDelta.y * scaler);
        rTransform.DOSizeDelta(scaledBall, speed / 2).OnComplete(()=> {
            rTransform.DOSizeDelta(finalBall, speed / 2);
        });

        rTransform.DOAnchorPos(positionsInBar[manager.closestPointId], speed).OnComplete(()=> {
            manager.AddBallToLibrary(ballId, gameObject);
        });
    }

    public void DestroyingObject() // уничтожение шарика из экшен-бара, анимация с помощью DOTween
    {
        var rTransform = gameObject.GetComponent<RectTransform>();
        rTransform.DOSizeDelta(new Vector2(0, 0), speed).OnComplete(()=> {
            Destroy(gameObject);
        });
    }

#endregion

#region Data receiver

    public void GetBallId(int id, int type) // Ну думаю тут очевидно, просто принимаем айди, для дальнейшей обработки
    {
        if(id != -1) ballId = id;
        ballType = type;
    }

#endregion

#region If ball frozen

    public void Unfreeze()
    {
        var _ButtonPhysicObject = physicSimulatedBall.GetComponent<ButtonPhysicObject>();
        if(_ButtonPhysicObject != null) _ButtonPhysicObject.SetItAsDefault();
    }

#endregion
}