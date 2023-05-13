using UnityEngine.SceneManagement;

namespace Service.SceneService
{
    public class SceneService : SwitchableService, ISceneService
    {
        public void LoadScene(int sceneIndex)
        {
            if(!enable) return;
            SceneManager.LoadScene(sceneIndex);
        }

        public void LoadSceneAsync(int sceneIndex)
        {
            if(!enable) return;
            SceneManager.LoadSceneAsync(sceneIndex);
        }

        public void LoadScene(string sceneName)
        {
            if(!enable) return;
            SceneManager.LoadScene(sceneName);
        }
    }
}




