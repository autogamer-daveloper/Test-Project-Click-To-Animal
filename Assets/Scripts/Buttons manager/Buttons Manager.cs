using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsManager : MonoBehaviour
{
    [Header ("___ Balls management ___")]
    [SerializeField] private BallsLibrary[] library; /* Библиотеки типов шаров, в каждой библиотеке должны содержаться шары одного вида,
    иначе это можно считать за баг! Но у меня всё окей, так что багов нету. */
    [SerializeField] private int idCount = 5; // Количество типов животных в шариках
    [SerializeField] private GameObject ballPrefab; // Префаб шара
    [SerializeField] private Button regenerateButton; // Кнопка регенерации уровня (перестройка с учётом нынешних шаров)
    [Range (0f, 10f)] // Ограничение x рандомизации
    [SerializeField] private float startX = 5f; // Стартовая позиция X начала алготритмической генерации
    [SerializeField] private float startY = 10f; // Стартовая высота Y начала алгоритмической генерации
    [Range (1f, 15f)]
    [SerializeField] private float timeToActivateRegeneration = 5f; // Через сколько можно разрешить повторную перегенерацию уровня после перегенерации

    private Randomize randomize; // Класс
    private GameObject tempBall; // Создаваемый мяч
    private const int count = 3;

    // Просто очередное начало рабочего дня скрипта
    private void Awake()
    {
        library = new BallsLibrary[idCount];
        for (int i = 0; i < idCount; i++)
        {
            library[i] = new BallsLibrary();
        }
        randomize = gameObject.AddComponent<Randomize>();
        StartSpawnBalls();
        regenerateButton.onClick.AddListener(Regenerate);
    }

#region Checking empty balls id's

    // Регистрируем в библиотеку шаров ещё один шар
    private void Registrate(int id, int queue, GameObject obj)
    {
        library[id].balls[queue] = obj;
    }

#endregion

#region Spawn balls and regenerate levels

/* Перегенерация уровня. Мы тупо переставляем оставшиеся шары в новые позиции. Не вижу смысла в перегенерации, так как в моём понимании
 нужно, чтобы они просто переигрывали анимацию падения, баловство. Мало данных об этой механике в ТЗ, поэтому реализовал так, как понял.
 Это имеет смысл, только если хочешь перезапустить уровень, но строчка в ТЗ опровергает это:
 "Сверху насыпается столько же фигурок, сколько было на момент нажатия"*/
    private void Regenerate()
    {
        DeactivateRegenerate();

        float cordBalls = startX;
        float heightScaler = 2.5f;
        bool goingRight = false;

        for(int i = 0; i < library.Length; i++)
        {
            for(int x = 0; x < count; x++)
            {
                if(library[i].balls[x] != null) {
                float cordX = 0;
                float cordY = 0;

                if(goingRight) {
                    cordX = cordBalls;
                    cordY = (cordBalls * heightScaler) + startY;
                    cordBalls += startX / 5;
                }
                else {
                    cordX = cordBalls;
                    cordY = (cordBalls * heightScaler) + startY;
                    cordBalls -= startX / 5;
                }
                var startPos = new Vector3(cordX, cordY, 0);

                if(cordBalls <= -startX)
                {
                    goingRight = true;
                }
                if(cordBalls >= startX)
                {
                    goingRight = false;
                }

                GameObject temp = library[i].balls[x];
                temp.transform.position = startPos;

                var script = temp.GetComponent<ButtonPhysicObject>();
                }

            }
        }
    }

    // Активация перегенерации
    private void ActivateRegenerate()
    {
        regenerateButton.interactable = true;
    }

    // Деактивация перегенерации
    private void DeactivateRegenerate()
    {
        regenerateButton.interactable = false;
        Invoke(nameof(ActivateRegenerate), timeToActivateRegeneration);
    }

#endregion

#region Start spawn balls

/* В отличии от прошлой механики перегенерации, создание шаров всех типов, которые ты указал, очень даже полезно, ведь без
этой механики ты в игру то и не поиграешь. Как было понятно из прошлого предложения, создаём все типы шаров по MaximumBalls.count штук */
    private void StartSpawnBalls()
    {
        DeactivateRegenerate();

        int countBall = 0;
        float cordBalls = startX;
        float heightScaler = 2f;
        bool goingRight = false;

        int heavy = RandomToMaxBalls(new int[] {0});
        int sticky = RandomToMaxBalls(new int[] {0, heavy});
        int frozen = RandomToMaxBalls(new int[] {0, heavy, sticky});

        for(int i = 0; i < library.Length; i++)
        {
            for(int x = 0; x < count; x++)
            {
                float cordX = 0;
                float cordY = 0;

                if(goingRight) {
                    cordX = cordBalls;
                    cordY = (cordBalls * heightScaler) + startY;
                    cordBalls += startX / 10;
                }
                else {
                    cordX = cordBalls;
                    cordY = (cordBalls * heightScaler) + startY;
                    cordBalls -= startX / 10;
                }
                var startPos = new Vector3(cordX, cordY, 0);

                if(cordBalls <= -startX)
                {
                    goingRight = true;
                }
                if(cordBalls >= startX)
                {
                    goingRight = false;
                }

                countBall++;

                tempBall = Instantiate(ballPrefab, startPos, Quaternion.Euler(0, 0, 0));
                var script = tempBall.GetComponent<ButtonPhysicObject>();
                script.SetId(i);

                if(countBall == heavy) script.SetThisAsHeavy();
                else if(countBall == sticky) script.SetThisAsSticky();
                else if(countBall == frozen) script.SetThisAsFrozen();
                else script.SetItAsDefault();

                Registrate(i, x, tempBall);
            }
        }
    }

#endregion

#region Extra options

    // Тут мы играем в лотерею и гадаем, какой из шаров станет легендарным скалой джонсоном (тяжёлым), липким как слаймик и замороженным, как... лёд...
    // Также создал отдельный класс рандома
    private int RandomToMaxBalls(int[] exceptions)
    {
        float maxBalls = count * library.Length;
        int number;

        HashSet<int> exceptionSet = new HashSet<int>(exceptions);

        do
        {
            float tmp = randomize.Randomizing(maxBalls);
            number = Mathf.RoundToInt(tmp);
        } while (exceptionSet.Contains(number));

        return number;
    }


#endregion
}

/* Тут находятся классы, которые обеспечивают работу главного класса из этого скрипта. */

//Тоже очевидно
[System.Serializable]
public class BallsLibrary
{
    public GameObject[] balls = new GameObject[3];
}

//Даже не программист поймёт, что это тоже очевидно
public class Randomize : MonoBehaviour
{
    public float Randomizing(float number)
    {
        float result = Random.Range(0, number);
        return result;
    }
}