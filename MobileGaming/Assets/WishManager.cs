using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class WishManager : MonoBehaviour
{
    [SerializeField] private ScriptableItemDatabase itemDatabase;
    [SerializeField] private ParticleSystem wishImageParticleSystem;
    [SerializeField] private ParticleSystem wishItemFlareParticleSystem;
    [SerializeField] private ParticleSystem wishHatFlareParticleSystem;
    public MainMenuManager menuManager;
    public PlayableDirector timeline;
    public Button wishButton;

    public Color rareItemColor;
    public Color epicItemColor;
    public Color legendaryItemColor;
    
    public void MakeAWish(int wishCost)
    {
        if (menuManager.StarCount - wishCost < 0) // Pas assez d'étoiles pour faire un voeu
        {
            // Animation Broke
            return;
        }

        // Animation Button Sucess
        menuManager.StarCount -= wishCost;
        //StartCoroutine(ButtonCd((float)timeline.duration));
        
        // A la fin de l'anim, bouton disparait & la timeline se lance
        if (timeline.state == PlayState.Playing)
            timeline.Stop();
        
        timeline.Play();
        EventManager.AddListener<WishEvent>(ChangeWishAnimation);
        
        itemDatabase.Wish();
    }

    private void ChangeWishAnimation(WishEvent wishEvent)
    {
        EventManager.RemoveListener<WishEvent>(ChangeWishAnimation);
        var item = wishEvent.Item;
        if (item != null)
        {
            wishImageParticleSystem.textureSheetAnimation.SetSprite(0,item.itemSprite);
            var color = item.Rarity switch
            {
                ItemRarity.Rare => rareItemColor,
                ItemRarity.Epic => epicItemColor,
                ItemRarity.Legendary => legendaryItemColor,
                _ => Color.white
            };
            wishItemFlareParticleSystem.GetComponent<Renderer>().material.color = color;
            wishHatFlareParticleSystem.GetComponent<Renderer>().material.color = color;
            /*
            switch (item.Rarity)
            {
                case ItemRarity.Rare: wishImageParticleSystem.GetComponent<Renderer>().material.color = rareItemColor; break;
                case ItemRarity.Epic: wishImageParticleSystem.GetComponent<Renderer>().material.color = epicItemColor; break;
                case ItemRarity.Legendary: wishImageParticleSystem.GetComponent<Renderer>().material.color = legendaryItemColor; break;
            } 
            */
        }
    }

    private IEnumerator ButtonCd(float duration)
    {
        yield return new WaitForSeconds(duration);
        wishButton.interactable = true;
    } 
}
