using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;

// Обязательно на объекте иметь и его!
[RequireComponent (typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    /* Это музыкальный контроллер, который меняет треки после того, как трек либо закончился, либо
    сменилась сцена. Объект с этим скриптом не удалиться после смены сцены. Поэтому лучше не ссылаться
    на объект с этим скриптом, поскольку объекты из DontDestroyOnLoad не могут быть связаны с объектами
    в обычной сцене */

    /* Этот скрипт я взял из моего другого OpenSource проекта - Save the candy, так как является почти универсальным решением (я немного
    обрезал функционал этого скрипта тут) */

    [Header ("Settings - music")]
    [SerializeField] private AudioClip[] audioClips; // Гибкая настройка треков
    [SerializeField] private float speed = 0.5f; // Скорость перехода
    [SerializeField] private float volume = 0.5f; // Стартовое значение громкости

    private AudioSource audioSrc; // Ссылка на аудио
    private int _lastPlayedIndex = -1; // Ссылка на последний проигранный трек

    private static MusicController _instance; // В начале указываем ссылку на этот объект, помечая его в DontDestroyOnLoad

    private Tween _musicVolume; // Ссылка на твин из DOTween, не обязательно, но чтобы можно было удалить, если отключено автоудаление в DOTween

#region While spawn music object

    // Начало инициализации
    private void Awake()
    {
        Initialize();
    }

    // Инициализация
    private void Initialize()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        audioSrc = GetComponent<AudioSource>();
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

#endregion

#region Registration to scene changing

    // Отписка от событий и уничтожение твина
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if(_musicVolume != null) _musicVolume.Kill();
    }

    // Создание события
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Loaded_Scene();
    }

#endregion

#region Music changing

    // Что делать, если событие произошло? Менять музыку!
    private void Loaded_Scene()
    {
        Change_Music();
    }

    // Смена музыки
    private void Change_Music()
    {
        CancelInvoke(nameof(Change_Music));

        _musicVolume = audioSrc.DOFade(0, speed).OnComplete(() => {
            int rand = Select_Clip();
            audioSrc.clip = audioClips[rand];
            audioSrc.Play();

            _musicVolume = audioSrc.DOFade(volume, speed).OnComplete(() => {
                Invoke(nameof(Change_Music), audioClips[rand].length);
            });
        });
    }

    // Выбор клипа
    private int Select_Clip()
    {
        if (audioClips.Length <= 1)
        return 0;

        int rand;
        do
        {
            rand = Random.Range(0, audioClips.Length);
        }
        while (rand == _lastPlayedIndex);

        _lastPlayedIndex = rand;
        return rand;
    }

#endregion
}
