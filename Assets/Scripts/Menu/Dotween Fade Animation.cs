using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DotweenFadeanimation : MonoBehaviour
{
    /* Эта анимация настолько проста, что даже думал не писать сюда комментарии. Так вот, тут просто изменяется прозрачность лаудера (экрана загрузки)*/

    private enum FadeMode{
        FadeIn,
        FadeOut
    }

    [Header ("___ Simple DOTween fade animation settings ___")]
    [SerializeField] private bool playAutomatically = true; // Автоматически?
    [SerializeField] private FadeMode fadeMode = FadeMode.FadeIn; // Режим анимации
    [SerializeField] private float speed = 0.5f; // Скорость (очевидно)
    [SerializeField] private bool autoDestroy = false; // Само-уничтожение

    private Image img; // Скрытое поле, сюда мы вставим ссылку на изображение, которому будем изменять прозрачность

    // Тупо инитиализация
    private void Start()
    {
        if(playAutomatically) {
            Animate();
        }
    }

    // Начать анимацию, если хотим из другого скрипта
    internal void StartAnimation()
    {
        Animate();
    }

    // Само начало анимации
    private void Animate()
    {
        img = gameObject.GetComponent<Image>();

        float transparency = 0;

        if(fadeMode == FadeMode.FadeIn) {
            transparency = 1;
        }
        if(fadeMode == FadeMode.FadeOut) {
            transparency = 0;
        }

        img.DOFade(transparency, speed).SetAutoKill(true).OnComplete(()=> {
            if(autoDestroy) Destroy(gameObject);
        });
    }
}
