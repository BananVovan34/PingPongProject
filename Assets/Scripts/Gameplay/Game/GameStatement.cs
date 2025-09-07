namespace Gameplay.Game
{
    public class GameStatement
    {
        private static GameStatement _instance;
        public static GameStatement Instance => _instance ??= new GameStatement();

		public Gamemode CurrentGamemode { get; set; }

        private GameStatement() { }
    }
}