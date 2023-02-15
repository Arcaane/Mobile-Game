using UnityEngine;

namespace Service
{
    public interface IInputService : IService
    {
        public PlayerControls controls { get; }
        public static Vector2 movement { get; }
        public static Vector2 cursorPosition { get; }
        public static Vector2 deltaPosition { get; }
    }
}


