using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatsButtons : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private ScriptableItemDatabase itemDb;
    [SerializeField] private GameObject panelGo;
    [SerializeField] private Button closePanelButton;
    
    [Header("Set In Scene")]
    [SerializeField] private Button openPanelButton;

    [Header("Buttons")]
    [SerializeField] private Button clearPlayerPrefsButton;
    [SerializeField] private Button giveStarsButton;
    [SerializeField] private Button resetStarsButton;
    [SerializeField] private Button unlockAllLevelsButton;
    [SerializeField] private Button resetLevelsButton;
    [SerializeField] private Button giveGoldButton;
    [SerializeField] private Button resetGoldButton;
    [SerializeField] private Button getAllItemButton;
    [SerializeField] private Button resetAllItemsButton;
    [SerializeField] private Button unlockSlot1Button;
    [SerializeField] private Button unlockSlot2Button;
    [SerializeField] private Button unlockSlot3Button;
    
    private ResetPlayerPrefsEvent resetPlayerPrefsEvent = new ();
    private ResetLevelsEvent resetLevelsEvent = new ();

    private void Start()
    {
        if(openPanelButton == null) Debug.LogWarning("openPanelButton is null",this);
        
        ClosePanel();
        
        closePanelButton.onClick.AddListener(ClosePanel);
        openPanelButton.onClick.AddListener(OpenPanel);
        
        clearPlayerPrefsButton.onClick.AddListener(ResetPlayerPrefs);
        
        giveStarsButton.onClick.AddListener(()=>itemDb.StarCount += 100);
        resetStarsButton.onClick.AddListener(()=>itemDb.StarCount = 0);

        unlockAllLevelsButton.onClick.AddListener(UnlockAllLevels);
        resetLevelsButton.onClick.AddListener(ResetLevels);

        giveGoldButton.onClick.AddListener(()=>itemDb.GoldCount += 1000);
        resetGoldButton.onClick.AddListener(()=>itemDb.GoldCount = 0);
        
        unlockSlot1Button.onClick.AddListener(()=>UnlockSlot(1));
        unlockSlot2Button.onClick.AddListener(()=>UnlockSlot(2));
        unlockSlot3Button.onClick.AddListener(()=>UnlockSlot(3));
        
        getAllItemButton.onClick.AddListener(itemDb.UnlockedAllItems);
        resetAllItemsButton.onClick.AddListener(itemDb.LockAllItems);

        void OpenPanel()
        {
            panelGo.SetActive(true);
        }
        
        void ClosePanel()
        {
            panelGo.SetActive(false);
        }

        void UnlockAllLevels()
        {
            Debug.Log("Unlocking all levels");
            itemDb.SetLevelUnlocked(22);
            EventManager.Trigger(new UpdateLevelsEvent());
        }

        void ResetLevels()
        {
            itemDb.SetLevelUnlocked(0,true);
            EventManager.Trigger(resetLevelsEvent);
        }

        void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            EventManager.Trigger(resetPlayerPrefsEvent);
            Application.Quit();
        }
        
        void UnlockSlot(int i)
        {
            ScriptableItemDatabase.CollectionLevel = i;
            itemDb.ResetEquippedItems();
        }
    }
}

public class ResetPlayerPrefsEvent { }
public class ResetLevelsEvent { }

