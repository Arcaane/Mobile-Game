using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class WishManager : MonoBehaviour
{
    [SerializeField] private ScriptableItemDatabase itemDatabase;
    [SerializeField] private ParticleSystem wishImageParticleSystem;
    [SerializeField] private ParticleSystem wishItemFlareParticleSystem;
    private Renderer wishItemFlareRenderer;
    [SerializeField] private ParticleSystem wishHatFlareParticleSystem;
    private Renderer wishHatFlareRenderer;
    public PlayableDirector timeline;
    public Button wishButton;

    public Color rareItemColor;
    public Color epicItemColor;
    public Color legendaryItemColor;

    public void OnEnable()
    {
        wishItemFlareRenderer = wishItemFlareParticleSystem.GetComponent<Renderer>();
        wishHatFlareRenderer = wishHatFlareParticleSystem.GetComponent<Renderer>();
    }

    public void MakeAWish(int wishCost)
    {
        EventManager.AddListener<WishEvent>(PlayWishAnimation);

        itemDatabase.Wish();
    }

    private void PlayWishAnimation(WishEvent wishEvent)
    {
        EventManager.RemoveListener<WishEvent>(PlayWishAnimation);

        var item = wishEvent.Item;
        var gold = wishEvent.Gold;
        
        ChangeWishAnimation(item);
        ChangeWishAnimation(gold);
        
        if(item == null && gold == 0) return;
        
        if (timeline.state == PlayState.Playing)
            timeline.Stop();
        
        timeline.Play();
    }

    private void ChangeWishAnimation(CollectionItem item)
    {
        if(item == null) return;
        wishImageParticleSystem.textureSheetAnimation.SetSprite(0,item.itemSprite);
        var color = item.Rarity switch
        {
            ItemRarity.Rare => rareItemColor,
            ItemRarity.Epic => epicItemColor,
            ItemRarity.Legendary => legendaryItemColor,
            _ => Color.white
        };
        wishItemFlareRenderer.material.color = color;
        wishHatFlareRenderer.material.color = color;
    }

    private void ChangeWishAnimation(int gold)
    {
        if(gold == 0) return;
    }

    private IEnumerator ButtonCd(float duration)
    {
        yield return new WaitForSeconds(duration);
        wishButton.interactable = true;
    } 
}
