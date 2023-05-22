using System.Collections.Generic;
using Attributes;
using Service;

public class LevelService : ILevelService
{
    [DependsOnService] private IInputService inputService;
    [DependsOnService] private IMagicLineService magicLineService;
    
    private Level currentLevel;

    private Queue<Client> clients = new ();
    
    
    public void InitLevel(Level level)
    {
        currentLevel = level;
     
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
        magicLineService.Enable();
        
        inputService.Enable();
    }

    public void EndLevel()
    {
        currentLevel = null;
    }
}