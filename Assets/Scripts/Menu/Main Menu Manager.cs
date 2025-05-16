using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header ("___ Settings - Main menu ___")]
    [SerializeField] private Button[] startGame; // Кнопки для перемещения на игровую сцену
    [SerializeField] private int sceneGamePlayId = 1; // Айди игровой сцены
    [Header ("___ Settings - Game ___")]
    [SerializeField] private Button[] exit; // Кнопки для перемещения в главное меню
    [SerializeField] private int sceneMenuId = 0; // Айди меню
    [Header ("___ Settings - Other ___")]
    [Range (0.1f, 5f)]
    [SerializeField] private float timer = 0.5f; // Задержка
    [SerializeField] private GameObject CanvasLoader; // Плавный переход (В самой сцене узнаете, как это реализовано, или в видео-записи)
    [Header ("___ Settings - Audio ___")]
    [SerializeField] private AudioClip sfx; // Аудио-эффект
    [SerializeField] private AudioSource src; // Источник звука

    // Инитиализация
    private void Start()
    {
        foreach(Button button in startGame)
        {
            button.onClick.AddListener(LoadGameScene);
        }
        foreach(Button button in exit)
        {
            button.onClick.AddListener(LoadMenuScene);
        }
    }

#region Actions

    // Плавная загрузка игровой сцены
    private void LoadGameScene()
    {
        src.PlayOneShot(sfx);
        Instantiate(CanvasLoader);
        Invoke(nameof(_LoadGameScene), timer);
    }

    // Плавная загрузка меню
    private void LoadMenuScene()
    {
        src.PlayOneShot(sfx);
        Instantiate(CanvasLoader);
        Invoke(nameof(_LoadMenuScene), timer);
    }

#endregion

#region SceneLoader

    // Загрузка игровой сцены
    private void _LoadGameScene()
    {
        SceneManager.LoadSceneAsync(sceneGamePlayId);
    }

    // Загрузка меню
    private void _LoadMenuScene()
    {
        SceneManager.LoadSceneAsync(sceneMenuId);
    }

#endregion
}
