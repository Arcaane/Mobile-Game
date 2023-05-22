using System.Collections.Generic;
using Attributes;
using Service;
using UnityEngine;

public class LevelService : ILevelService
{

    [DependsOnService] private IMagicLineService magicLineService;
    
    private Level currentLevel;

    private Queue<Client> clients = new ();
    
    public void InitLevel(Level level)
    {
        currentLevel = level;

        currentLevel.StartPanel.OnAnimationOver += StartLevel;

        ResetVariables();
        
        LinkStuff();
    }

    private void ResetVariables()
    {
        clients.Clear();    
    }

    private void LinkStuff()
    {
        currentLevel.StartPanel.UpdateValues(currentLevel);
        
        magicLineService.SetCamera(currentLevel.Camera);
    }
    
    public void StartLevel()
    {
        SorcererController.Instance.hudCanvasGO.SetActive(true);
        
        magicLineService.Enable();
    }

    public void EndLevel()
    {
        currentLevel = null;
    }
}