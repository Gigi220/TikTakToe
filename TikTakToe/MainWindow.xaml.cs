using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Server;

namespace TikTakToe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static FieldState[] GameField;
		static IPEndPoint ipPoint;
		static Socket socket;
		static int port = 64100;
		static string address = "127.0.0.1";

		class Aswer
		{
			byte Pos { get; set; }
			PlayerTeam Flag { get; set; }
			public Aswer(byte pos, PlayerTeam flag)
			{
				Pos = pos;
				Flag = flag;
			}
		}

		public MainWindow()
        {
            InitializeComponent();

			//((Button)GameFieldGrid.Children[1]).Content = FieldState.X.ToString();

			GameField = new FieldState[9];
			for (int i = 0; i < GameField.Length; ++i)
			{
				GameField[i] = FieldState.None;
			}

			try
			{
				ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				socket.Connect(ipPoint);

				MessageBox.Show("Connected");

				AsyncGet();

			}
			catch (Exception ex)
			{ }

		}

		public async void AsyncGet()
		{
			await Task.Run(() => GetFromServer());
		}

		public void GetFromServer()
		{
			try
			{
				while(true)
				{
					if(socket.Connected)
					{
						byte[] data = new byte[3];
						socket.Receive(data);

						byte pos = data[0];
						PlayerTeam who = (PlayerTeam)data[1];
						GameState state = (GameState)data[2];

						

						this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate { ((Button)this.GameFieldGrid.Children[pos]).Content = who.ToString(); });
						//if (state != 0)
						//{
						//	socket.Close();
						//	Check(state);
						//}

						//Check(state);
						//this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate { Check(state); });

					}
				}
			}
			catch(Exception e)
			{ }
		}

		public void Check(GameState state)
		{
			if (state == GameState.Drow)
			{
				MessageBox.Show("Drow");
			}
			else if (state == GameState.X)
			{
				MessageBox.Show("Win X");
			}
			else if (state == GameState.O)
			{
				MessageBox.Show("Win O");
			}
		}

		public void SendPos(byte pos)
		{
			socket.Send(new byte[3] { pos, 0, 0 });
		}

		public void SetField(byte pos, PlayerTeam who)
		{
			GameField[pos] = (FieldState)who;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string buf = (string)((Button)sender).Tag;
			SendPos(Byte.Parse(buf));
		}
	}
}
