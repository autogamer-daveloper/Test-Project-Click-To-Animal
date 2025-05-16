using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header ("___ Settings - gameplay ___")]
    [SerializeField] private ButtonsLibrary ball; // Сборник объектов
    [SerializeField] private IDsLibrary index; // Сборник айди
    [Header ("___ Settings - UI ___")]
    [SerializeField] private GameObject win; // Префаб победной панели
    [SerializeField] private GameObject lose; // Префаб панели поражения
    [SerializeField] private float loseTime = 0.5f; // Задержка вывода панели победы
    [Header ("___ Settings - Audio ___")]
    [SerializeField] private AudioClip sfxLose; // Аудио-эффект поражения

    [Range (0, 6)]
    [HideInInspector] internal int closestPointId = 0; // Ближайшее свободное место

    private bool _canInvokeResult = true; // Изначально тру, сделано для того, чтобы не вызывать повторно результат игровой сессии
    private AudioSource src; // Источник звука

    private void Start()
    {
        var blockButtons = GameObject.FindFirstObjectByType<BlockButtons>();
        blockButtons.InitializeButtons();

        var srcObj = GameObject.FindWithTag("AudioSourceDefault");
        if(srcObj != null) src = srcObj.GetComponent<AudioSource>();
    }

    internal void AddBallToLibrary(int id, GameObject obj) // По названию метода тут всё понятно. Добавление мяча в библиотеку мячей (экшен-бар)
    {
        var blockButtons = GameObject.FindFirstObjectByType<BlockButtons>();
        blockButtons.DeactivateButtons();

        var anim = obj.GetComponent<ButtonAnimation>();

        for (int i = 0; i < ball.buttons.Length; i++)
        {
            if (ball.buttons[i] == null && index.id[i] == -1)
            {
                ball.buttons[i] = anim;
                index.id[i] = id;
                Initializing();
                return;
            }
        }
    }

    private void Initializing() // Инитиализация экшен-бара и проверка на свободный слот
    {
        for (int x = 0; x < index.counting.Length; x++)
        {
            index.counting[x] = 0;
        }

        closestPointId = -1;

        for (int i = 0; i < index.id.Length; i++)
        {
            if (index.id[i] != -1 && index.id[i] < index.counting.Length)
            {
                index.counting[index.id[i]]++;
            }

            if (closestPointId == -1 && index.id[i] == -1)
            {
                closestPointId = i;
            }
        }

        Checking();
    }

    private void Checking() // Проверка на то, есть ли 3 одинаковых фигурки в экшен-баре
    {
        bool isFull = true;

        for (int x = 0; x < index.counting.Length; x++)
        {
            if (index.counting[x] >= 3)
            {
                index.counting[x] = 0;

                for (int y = 0; y < index.id.Length; y++)
                {
                    if (index.id[y] == x)
                    {
                        index.id[y] = -1;
                        ball.buttons[y].DestroyingObject();
                        ball.buttons[y] = null;
                        Initializing();
                    }
                }
            }
        }

        for (int i = 0; i < index.id.Length; i++)
        {
            if (index.id[i] == -1)
            {
                isFull = false;
                break;
            }
        }

        if (isFull)
        {
            Invoke(nameof(YouLose), loseTime);
            if(src != null && sfxLose != null) src.PlayOneShot(sfxLose);
        }
    }

// Думаю тут будет супер-очевидные методы, так что даже комменты не напишу
#region Actions

    /* Проверка количества шаров находиться в другом скрипте, не вижу смысла копировать логику или объединять 2 скрипта под разные задачи.
    Поэтому и модификатор доступа internal - альтернатива public, но с ограничениями. Об этом в документации от Microsoft */
    internal void YouWin()
    {
        Result(win);
    }

    private void YouLose()
    {
        Result(lose);
    }

    private void Result(GameObject objWithAnim)
    {
        if(!_canInvokeResult) return;
        var anim = objWithAnim.GetComponent<PanelAnimation>();
        anim.ChangeAnimState();
        _canInvokeResult = false;
    }

#endregion
}


// Просто создал ещё классы как хранилища данных

//Этот класс отвечает за кнопки в экшен-баре, чтобы мы знали, с чем взаимодействовать визуально
[System.Serializable]
internal class ButtonsLibrary
{
    public ButtonAnimation[] buttons = new ButtonAnimation[7];
}

//Этот класс уже отвечает за внутреннюю обработку фигурок из экшен-бара. Тут храняться их айди короче.
[System.Serializable]
internal class IDsLibrary
{
    public int[] id = { -1, -1, -1, -1, -1, -1, -1 };

    public int[] counting = new int[5];
}
