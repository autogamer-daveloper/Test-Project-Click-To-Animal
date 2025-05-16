using UnityEngine;

[CreateAssetMenu(fileName = "New Button Type", menuName = "Buttons/New Button Type")]
public class ButtonType : ScriptableObject
{
    /* Это scriptable object, просто контейнер, который где-то затерялся.
    Пока у остальных кипишь, у него тишина...
     
    Короче, тупо хранит картинки. Например для собаки в разных вариантах, когда он обычный, заморженный, липкий или тяжёлый. */

    public Sprite ButtonImage;
    public Sprite ButtonImageHeavy;
    public Sprite ButtonImageSticky;
    public Sprite ButtonImageFrozen;
}
