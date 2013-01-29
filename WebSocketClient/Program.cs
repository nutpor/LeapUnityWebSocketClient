using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Threading;

using Engine.JSON;

namespace WebSocketClient
{
	public class Logger
	{
		static StreamWriter stream;
		static FileStream streamByte;

		public static void Log(string str)
		{
			stream.WriteLine("<<<" + str + ">>>");
		}

		public static void Write(byte[] data)
		{
			streamByte.Write(data, 0, data.Length);
		}

		public static void StartLogger()
		{
			stream = File.CreateText("output.txt");

			streamByte = File.Create("output.bin");
		}

		public static void StopLogger()
		{
			stream.Close();

			streamByte.Close();
		}
	}


	class Program
	{

		static void WorkerThread()
		{
			Logger.StartLogger();
			// ws://localhost:6437
			var client = new ClientSocket(new ExtendedClientHandshake()
			{
				Origin = "testproject",
				Host = "localhost:6437",
				ResourcePath = "/get"
			});

			if (!client.Connect())
				return;

			int i = 0;
			while (i < 30)
			{
				++i;
				var answer = client.Receive();
				Logger.Log(answer);
				//Thread.Sleep(1);

			}

			/*Parser p = new Parser(client.VersionDataFrame.ToString());
			Dictionary<string, object> dict = p.Parse();
			Console.WriteLine(dict["version"]);*/

			client.Disconnect();
			Logger.StopLogger();
		}

		static void Main(string[] args)
		{
			//Console.WriteLine("Ok");

			

			Thread t = new Thread(new ThreadStart(WorkerThread));
			t.Start();

			while (t.IsAlive)
				Thread.Sleep(1);



			Console.WriteLine("END");
			Console.ReadKey();
		}
	}
}
