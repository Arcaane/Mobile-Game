namespace Service
{
    public interface ILevelService : IService
    {
        public void InitLevel(Level level);

        public void StartLevel();

        public void EndLevel();
    }
}


