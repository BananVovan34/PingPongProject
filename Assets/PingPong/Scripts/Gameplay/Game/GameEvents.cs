using System;

namespace Gameplay.Game
{
    public static class GameEvents
    {
        public static event Action GameStart;
        public static event Action<byte> GameEnd;
        
        public static void OnGameStart() => GameStart?.Invoke();
        public static void OnGameEnd(byte winnerId) => GameEnd?.Invoke(winnerId);
    }
}