namespace Service
{
    public interface ISceneService : ISwitchableService
    {
        public void LoadScene(int sceneIndex);
        public void LoadSceneAsync(int sceneIndex);
        public void LoadScene(string sceneName);
    }
}


