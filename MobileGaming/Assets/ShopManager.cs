using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private ScriptableItemDatabase itemDatabase;
    
    [Header("Confirm Buy Section")]
    [SerializeField] private GameObject go;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    
    private CurrencyOwnByPlayer currencyToGive;
    private HowIPay currencyToTake;
    private int quantity;
    private float lastPrice;

    public void ShowItemToPurchase(float price, Sprite sprite, CurrencyOwnByPlayer a, int howMuch, HowIPay b)
    {
        quantity = howMuch;
        currencyToGive = a;
        currencyToTake = b;
        go.SetActive(true);
        image.sprite = sprite;
        text.text = $"{price}€";
        lastPrice = price;
    }

    public void Buy()
    {
        switch (currencyToTake)
        {
            case HowIPay.gold: itemDatabase.GoldCount -= (int)lastPrice; break;
            case HowIPay.stars: itemDatabase.StarCount -= (int)lastPrice; break;
            case HowIPay.money: Debug.Log($"Player stend {lastPrice}€"); break;
        }
        
        switch (currencyToGive)
        {
            case CurrencyOwnByPlayer.gold: itemDatabase.GoldCount += (int)quantity; break;
            case CurrencyOwnByPlayer.stars: itemDatabase.StarCount += (int)quantity; break;
        }
    }
}

public enum CurrencyOwnByPlayer { gold, stars, powerup }
public enum HowIPay { gold, stars, money }
