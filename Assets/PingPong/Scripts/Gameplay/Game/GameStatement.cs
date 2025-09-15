using Unity.Netcode;

namespace Gameplay.Game
{
    public class GameStatement
    {
        private static GameStatement _instance;
        public static GameStatement Instance => _instance ??= new GameStatement();
        
        private NetworkManager _networkManager;

		public Status CurrentStatus { get; set; }

        private GameStatement() { }
    }
}