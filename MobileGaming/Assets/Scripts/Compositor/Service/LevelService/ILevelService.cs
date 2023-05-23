using System;
using TMPro;
using UnityEngine.UI;

namespace Service
{
    public interface ILevelService : IService
    {
        public void InitLevel(Level level,Slider newScore,TextMeshProUGUI newTimeText);

        public void StartLevel();

        public void EndLevel();
        
        public event Action<int> OnEndLevel;
    }
}


