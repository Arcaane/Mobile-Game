using Addressables;
using Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.AddressableAssets.Addressables;

namespace Service.SceneService
{
    public class SceneService : SwitchableService, ISceneService
    {
        [DependsOnService] private IInputService inputService;
        
        public GameObject LoadingCanvasGo => loadingCanvasGo != null ? loadingCanvasGo : LoadLoadingCanvas();
        private GameObject loadingCanvasGo;
        
        public SceneService(bool startState) : base(startState)
        {
        }
        
        private GameObject LoadLoadingCanvas()
        {
            var op = LoadAssetAsync<GameObject>("LoadingCanvas");
            var obj = op.WaitForCompletion().gameObject;
            loadingCanvasGo = Object.Instantiate(obj);
            Release(obj);
            loadingCanvasGo.SetActive(false);
            Object.DontDestroyOnLoad(loadingCanvasGo);
            return loadingCanvasGo;
        }
        
        public void LoadScene(int sceneIndex)
        {
            if(!enable) return;
            SceneManager.LoadScene(sceneIndex);
        }

        public void LoadSceneAsync(int sceneIndex)
        {
            if(!enable) return;
            inputService.Disable();
            LoadingCanvasGo.SetActive(true);
            SceneManager.LoadSceneAsync(sceneIndex);
            SceneManager.sceneLoaded += DeactivateLoadingCanvas;

            void DeactivateLoadingCanvas(Scene scene,LoadSceneMode mode)
            {
                if(scene.buildIndex != sceneIndex) return;
                LoadingCanvasGo.SetActive(false);
                inputService.Enable();
                SceneManager.sceneLoaded -= DeactivateLoadingCanvas;
            }
        }

        public void LoadScene(string sceneName)
        {
            if(!enable) return;
            SceneManager.LoadScene(sceneName);
        }
    }
}




