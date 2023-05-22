using NaughtyAttributes;
using UnityEngine;

public class CollectionItem : ScriptableObject
{
    public Sprite itemSprite;
    public string objectTitle;
    public string chapterNumber;
    public string rarety;
    [ResizableTextArea] public string descriptionText;
    [ResizableTextArea] public string powerUpText;
}
