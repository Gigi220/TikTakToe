using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Player
    {
        public Socket Socket_ { get; set; }
        public PlayerTeam Team;

        public Player()
        {
            Socket_ = null;
            Team = PlayerTeam.None;
        }
        public Player(Socket socket, PlayerTeam playerTeam)
        {
            Socket_ = socket;
            Team = playerTeam;
        }
    }
    class Program
    {
        static int port = 64100;
        static string ip = "127.0.0.1";
        static Socket ListenSocket;
		static List<Player> players;
		static byte team = 1;
		static FieldState[] GameField;

        static void Main(string[] args)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			players = new List<Player>();
			GameField = new FieldState[9];
			for(int i = 0; i < GameField.Length; ++i)
			{
				GameField[i] = FieldState.None;
			}

			try
            {
                ListenSocket.Bind(ipPoint);
                ListenSocket.Listen(10);

                Console.WriteLine("Start Server ...");

                while(true)
                {
					if(players.Count < 3)
					{
						Player newPlayer = new Player();
						newPlayer.Socket_ = ListenSocket.Accept();
						if (team == 1)
						{
							newPlayer.Team = PlayerTeam.X;
							team = 0;
						}
						else
						{
							newPlayer.Team = PlayerTeam.O;
							team = 1;
						}
						players.Add(newPlayer);
						Console.WriteLine("Player {0} connected", newPlayer.Team.ToString());

						AsyncGet(newPlayer);
					}
				}
                
            }
            catch (Exception e) { }
        }
		private async static void AsyncGet(Player player)
		{
			Console.WriteLine("Async start {0}", player.Team.ToString());
			await Task.Run(() => GetPos(player));
		}
		private static void Send(byte pos, Player CurrPlayer)
		{
			try
			{
				if(CurrPlayer.Socket_.Connected)
				{
					for (int i = 0; i < players.Count; ++i)
					{
						byte[] data = new byte[3];
						data[0] = pos;
						data[1] = (byte)CurrPlayer.Team;
						data[2] = 0;//CheckGameState();
						players[i].Socket_.Send(data);
						Console.WriteLine("Sended to {0}", players[i].Team.ToString());
						Console.WriteLine(data.ToString());
					}
				}
			}
			catch(Exception e)
			{ }
		}
		private static void GetPos(Player player)
		{
			while(true)
			{
				if (player.Socket_.Connected)
				{
					if(players.Count == 2)
					{
						byte pos = 0;
						byte[] data = new byte[3];

						player.Socket_.Receive(data);
						pos = data[0];

						if (CheckTurn(pos, player) == 1)
						{
							GameField[pos] = (FieldState)player.Team;
							Send(pos, player);
							Console.WriteLine(player.Team.ToString() + " has turned");
							if (team == 1)
								team = 0;
							else
								team = 1;
						}
						else
						{
							Console.WriteLine(player.Team.ToString() + " hasn't turned");
						}
					}
				}
			}
		}
		private static byte CheckTurn(byte pos, Player player)
		{
			if(GameField[pos] == FieldState.None && (byte)player.Team == team)
			{
				return 1;
			}
			return 0;
		}

		private static bool CheckX()
		{
			bool flag = true;
			FieldState[,] arr = new FieldState[3, 3];
			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					arr[i, j] = GameField[i + j * 2];
				}
			}

			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					if (arr[j, i] != FieldState.X)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag == true)
				return true;

			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					if (arr[0, j] != FieldState.X)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag == true)
				return true;

			for (int i = 0; i < 3; ++i)
			{
				if (arr[i, i] != FieldState.X)
				{
					flag = false;
					break;
				}
			}
			if (flag == true)
				return true;
			for (int i = 0; i < 3; ++i)
			{
				for (int j = 2; j >= 0; ++j)
				{
					if (arr[i, j] != FieldState.X)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag == true)
				return true;

			return flag;
		}

		private static bool CheckO()
		{
			bool flag = true;
			FieldState[,] arr = new FieldState[3, 3];
			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					arr[i, j] = GameField[i + j * 2];
				}
			}

			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					if (arr[j, i] != FieldState.O)
					{
						flag = false;
						break;
					}
				}
			}
			if(flag == true)
			{
				return true;
			}

			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					if (arr[0, j] != FieldState.O)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag == true)
			{
				return true;
			}

			for (int i = 0; i < 3; ++i)
			{
				if (arr[i, i] != FieldState.O)
				{
					flag = false;
					break;
				}
			}
			if (flag == true)
			{
				return true;
			}
			for (int i = 0; i < 3; ++i)
			{
				for (int j = 2; j >= 0; ++j)
				{
					if (arr[i, j] != FieldState.O)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag == true)
			{
				return true;
			}

			return flag;
		}

		private static bool CheckFull()
		{
			foreach(FieldState x in GameField)
			{
				if (x == FieldState.None)
					return false;
			}
			return true;
		}

		private static byte CheckGameState()
		{
			if(CheckX() == false && CheckO() == false && CheckFull() == true)
			{
				return 1;
			}
			else if(CheckX())
			{
				return 3;
			}
			else if(CheckO())
			{
				return 2;
			}
			
			return 0;
		}
    }
}
