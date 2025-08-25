using System;

namespace Gameplay.Game
{
    public static class GameEvents
    {
        public static event Action OnGameStart;
        public static event Action OnGameEnd;
    }
}