using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowCollectionItemHolder : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI chapterText;
    public TextMeshProUGUI raretyText;
    public GameObject[] GO;

    public void FillAndShowItemCollectionDescription(Sprite _sprite, string _title, string _description, string _chapter, string _rarety)
    {
        itemImage.sprite = _sprite;
        titleText.text = _title;
        descriptionText.text = _description;
        chapterText.text = _chapter;
        raretyText.text = _rarety;

        foreach (var t in GO)
        {
            t.SetActive(true);
        }
    }
}
