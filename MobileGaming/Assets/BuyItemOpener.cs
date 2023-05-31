using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyItemOpener : MonoBehaviour
{
    [SerializeField] private ShopManager _shopManager;

    [Header("PURCHASE")]
    public float itemPrice;
    public HowIPay purchasingCurrency;
    
    [Space(10)]
    [Header("FOR PLAYER")]
    public int currencyQuantity;
    public CurrencyOwnByPlayer currencyType;
    public Sprite currencySprite;
    
    public void SendToPurchase()
    {
        _shopManager.ShowItemToPurchase(itemPrice, currencySprite, currencyType, currencyQuantity, purchasingCurrency);
    }
}
