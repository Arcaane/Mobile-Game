using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCollectionManager : MonoBehaviour
{
    public MainMenuManager menuManager;
    public ItemSlot[] slots;
}

[Serializable]
public class ItemSlot
{
    public Image lockImage;
    public Image itemImage;
    public Sprite itemInSlotSprite;
}
