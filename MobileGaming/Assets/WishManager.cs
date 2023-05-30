using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class WishManager : MonoBehaviour
{
    public MainMenuManager menuManager;
    public PlayableDirector timeline;
    public Button wishButton;

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
    }

    private IEnumerator ButtonCd(float duration)
    {
        yield return new WaitForSeconds(duration);
        wishButton.interactable = true;
    } 
}
