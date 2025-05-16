using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SimpleAudioSFX : MonoBehaviour
{
    // Ну этот код настолько лёгкий, что его поймёт не программист
    [Header ("___ VERY Simple 'one shot' sfx audio ___")]
    [SerializeField] private AudioClip sfx;
    [SerializeField] private AudioSource src;
    
    private Button button;

    private void Start()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(Click);
    }

    private void Click()
    {
        src.PlayOneShot(sfx);
    }
}
