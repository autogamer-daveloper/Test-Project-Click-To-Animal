using UnityEngine;
using UnityEngine.UI;

public class ButtonPhysicObject : MonoBehaviour
{
    /* У этого объекта должен быть тег "Ball" */

    /* Не обращайте внимания на то, что фигурки в скриптах называются тупо ball, хотя по графике могут быть и кругом, и квадратом, и треугольником.
    Просто симуляция физики завязана на шарах. Так как до рисовки графики я видел только шары, я это так и оставлю, хотя по идее можно заменить
    имена полей и переинитиализировать всё в Inspectore, но это лишний гемор. */

    [Header ("___ Basic Settings ___")]
    [SerializeField] private GameObject button; // Тут должен быть префаб кнопки, заготовка
    [Header ("___ Ball type ___")]
    [SerializeField] private ButtonType[] types; // Типы кнопок с разными вариациями иконок и с уникальными id

    private GameObject _obj; // Объект, который по итогу станет плавающей кнопкой из префаба button
    private Transform _targetTransform; // Transform невидимого объекта, который будет симулировать физику
    private RectTransform _targetRect; // RectTransform, которому будет передаваться информация о местоположении
    private Canvas canvas; // Тут канвас, на который мы будем передавать позицию RectTransform для кнопки

    [Range (0, 3)]
    [HideInInspector]internal int type = 0; // 0 - обычный, 1 - тяжёлый, 2 - липкий, 3 - замороженный.

    private int id = -1; // Айди мяча, который мы возьмём с ButtonType

    private FixedJoint2D joint; // Для прикола с липучестью

#region Initialize when enabling

    // Понятная проверка на наличие айди
    private void InitializeId()
    {
        if(id != -1) {
            InitializeTransform();
        }
        else {
            Debug.LogError("Внимание! Мяч был не инитиализирован, я не знаю как, но это не есть хорошо!");
            Destroy(gameObject);
        }
    }

    // Инитиализация симуляции кнопки, которую мы призываем и связываем с Canvas
    // Если в игре по механике должно быть несколько Canvas, то будем искать Canvas уже по тегу, потому что может кнопку повести не туда
    private void InitializeTransform()
    {
        _targetTransform = gameObject.transform;
        var startPos = new Vector2(0, 0);

        var tempCanvas = GameObject.FindGameObjectWithTag("GameManagement");
        if(tempCanvas != null) canvas = tempCanvas.GetComponent<Canvas>();

        _obj = Instantiate(button, startPos, Quaternion.identity) as GameObject;
        _obj.transform.SetParent(canvas.transform, false);
        _targetRect = _obj.GetComponent<RectTransform>();
        var _syncButton = _obj.GetComponent<ButtonAnimation>();

        _syncButton.InitializePhysicObject(gameObject);

        InitializeImage();
    }

    // Инитиализация картинки на симулируемой кнопке, кстати если есть уникальные приколы, то появляется именно картинка с приколом
    private void InitializeImage()
    {
        int attempts = 0;

        while (attempts < 10)
        {
            if(id != -1)
            {
                switch(type)
                {
                    case 0:
                        SetImage(types[id].ButtonImage);
                    break;
                    case 1:
                        SetImage(types[id].ButtonImageHeavy);
                    break;
                    case 2:
                        SetImage(types[id].ButtonImageSticky);
                    break;
                    case 3:
                        SetImage(types[id].ButtonImageFrozen);
                    break;
                    default:
                        SetImage(types[id].ButtonImage);
                    break;
                }
                return;
            }
            else
            {
                attempts++;
                if(_obj != null) Destroy(_obj);
                Destroy(gameObject);
            }
        }
    }

    // Универсальный метод смены картинки
    private void SetImage(Sprite sprite)
    {
        var img = _obj.GetComponent<Image>();
        img.sprite = sprite;
    }

#endregion

#region Sync invisible physic object to button in canvas

    // Ну что тут скажешь, просто симулируем саму слежку симулируемой кнопки за физическим шаром
    private void Update()
    {
        if (_targetTransform == null || _targetRect == null || canvas == null)
            return;

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(_targetTransform.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPosition
        );

        _targetRect.localPosition = localPosition;
    }

#endregion

#region Set custom settings

    // Вставляем свой айди с другого скрипта, точнее просто передаём, а тут уже всатвляем. Мы же дожны соблюдать ООП.
    internal void SetId(int index)
    {
        id = index;
        InitializeId();
    }

#endregion

#region Extra options

    /* Немного про взрывающуюся фигурку. Так как у меня генерируется заранее просчитанное количество фигурок, уничтожение
    этой же фигурки уже в экшен-баре приведёт к невозможности закончить игру, так как не будет собрано нужное количество
    одинаковых фигурок из-за уничтожения одной из них. Реализовать можно, но с геймплейной дырой, вам такое явно не нужно.
    Конечно, можно после взрыва добавить недостающую фигурку в поле, но тогда какой смысл от взрывающейся фигурки? */

    // Делаем фигуру тяжёлой

    internal void SetThisAsHeavy()
    {
        var rb = gameObject.GetComponent<Rigidbody2D>();
        if(rb != null) {
            rb.mass = 50; // Лучше фиксированное значение
            rb.gravityScale = 5; // Лучше фиксированное значение
        }
        type = 1;
        SetImage(types[id].ButtonImageHeavy);
    }

    // Делаем фигуру липкой

    internal void SetThisAsSticky()
    {
        var rb = gameObject.GetComponent<Rigidbody2D>();
        if(rb != null) {
            rb.mass = 25; // Лучше фиксированное значение
            rb.gravityScale = 1; // Лучше фиксированное значение
        }
        type = 2;
        SetImage(types[id].ButtonImageSticky);
    }

    // Шайтан-машина по прилипанию одного объекта к другому, или хотя-бы что то подобное

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Ball")) return;

        var ball = other.gameObject.GetComponent<ButtonPhysicObject>();

        if (ball == null) return;

        if (ball.type == 2 || type == 2)
        {
            if (joint == null)
            {
                joint = gameObject.AddComponent<FixedJoint2D>();
                joint.connectedBody = ball.GetComponent<Rigidbody2D>();
                joint.enableCollision = false;
            }
        }
    }

    // А потом и отлипания

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Ball")) return;

        var ball = other.gameObject.GetComponent<ButtonPhysicObject>();

        if (ball == null) return;

        if ((ball.type == 2 || type == 2) && joint != null)
        {
            Destroy(joint);
        }
    }

    // Делаем фигуру замороженной

    internal void SetThisAsFrozen()
    {
        var rb = gameObject.GetComponent<Rigidbody2D>();
        if(rb != null) {
            rb.mass = 25; // Лучше фиксированное значение
            rb.gravityScale = 1; // Лучше фиксированное значение
        }
        type = 3;
        SetImage(types[id].ButtonImageFrozen);
        _obj.tag = "Frozen";
    }

    internal void SetItAsDefault()
    {
        var rb = gameObject.GetComponent<Rigidbody2D>();
        if(rb != null) {
            rb.mass = 25; // Лучше фиксированное значение
            rb.gravityScale = 1; // Лучше фиксированное значение
        }
        type = 0;
        SetImage(types[id].ButtonImage);
    }

#endregion

#region Last data transfer

    // Что делать при уничтожении физического шара?

    private void OnDestroy()
    {
        GetInfo();
    }

    internal void GetInfo()
    {
        var _syncButton = _obj.GetComponent<ButtonAnimation>();
        if(_syncButton != null) {
        _syncButton.GetBallId(id, type);
        } 
        /* Передавать в "наследство" айди к кнопке, которая отвечает в основном за анимации, но также связана с экшен-баром */
    }

#endregion
}