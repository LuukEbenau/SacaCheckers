using FunctionalLayer;

namespace FunctionalLayer_Test.Mocks
{
	internal class GameMock : Game
	{
		public override IPlayer Player1 { get; set; }
		public override IPlayer Player2 { get; set; }

		public GameMock(ISettings settings) : base(settings)
		{
			//IPlayer p1 = new Player(PlayerNumber.One, this, "testspeler 1", PlayerColor.Light);
			//IPlayer p2 = new Player(PlayerNumber.Two, this, "testspeler 2", PlayerColor.Dark);

			//var startLocation = new TileCoordinate(5, 2);
			//Dictionary<Player, List<TileCoordinate>> multiJumpTestSetup = new Dictionary<Player, List<TileCoordinate>>
			//{
			//    {
			//        p1,
			//        new List<TileCoordinate>{
			//            startLocation
			//        }
			//    },
			//    {
			//        p2,
			//        new List<TileCoordinate>{
			//            new TileCoordinate(6,3),//ends 7,4
			//            new TileCoordinate(8,5),//blocked
			//            new TileCoordinate(9,6),//blocks previous
			//            new TileCoordinate(6,5),//ends 5,6
			//            new TileCoordinate(4,5)
			//        }
			//    },
			//};
			//Board.InitializePlayers(p1, p2, multiJumpTestSetup);
		}
	}
}