using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyBundle : MonoBehaviour
{
    [SerializeField] private ScriptableItemDatabase itemDatabase;
    
    public CurrencyOwnByPlayer[] currencyOwnByP;
    public int[] quantity;

    public void Buy()
    {
        for (int i = 0; i < currencyOwnByP.Length; i++)
        {
            switch (currencyOwnByP[i])
            {
                case CurrencyOwnByPlayer.gold: itemDatabase.GoldCount += quantity[i]; break;
                case CurrencyOwnByPlayer.stars: itemDatabase.StarCount += quantity[i]; break;
            }
        }
    }
}
