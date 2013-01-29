using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace WebSocketClient
{
	class ClientSocket
	{
		public Socket Socket { get; set; }
		public ExtendedClientHandshake Handshake { get; set; }

		public DataFrame VersionDataFrame { get; set; }

		public ClientSocket(ExtendedClientHandshake hs)
		{
			Handshake = hs;
			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
		}

		public bool Connect()
		{
			var tmp = Handshake.Host.Split(':');
			var port = 81;
			var host = Handshake.Host;
			if (tmp.Length > 1)
			{
				port = Int32.Parse(tmp[1]);
				host = tmp[0];
			}

			Socket.ReceiveBufferSize = 1024 * 64;
			Socket.Connect(host, port);
			SendHandshake();
			VersionDataFrame = null;

			if (!ReadHandshake())
			{
				Socket.Close();
				return false;
			}


			if (VersionDataFrame == null)
				Receive(VersionDataFrame);

			return true;
		}

		public void Disconnect()
		{
			if (Connected())
				Socket.Close();
		}

		public bool Connected()
		{
			return Socket.Connected;
		}

		public bool DataAvailiable()
		{
			if (!Connected())
				return false;

			return Socket.Available > 0;
		}

		public string Receive(DataFrame frame = null)
		{
			if (frame == null)
				frame = new DataFrame();

			var buffer = new byte[1024 * 8];

			int sizeOfReceivedData =  Socket.Receive(buffer);
			frame.Append(buffer);
			if (frame.IsComplete)
				return frame.ToString();
			else
				return Receive(frame);
		}

		public void Send(string data)
		{
			Socket.Send(DataFrame.Wrap(data));
		}

		private bool ReadHandshake()
		{
			var hs = new byte[1024];
			int count = Socket.Receive(hs);

			// check that "version" DataFrame has included in this handshake data (DataFrame start with 0 and end with 255)
			int start = 0;
			int end = count - 1;
			int dataFrameLength = 0;
			if (hs[end] == 255)
			{
				// find "version" DataFrame length
				for (int i = end; i >= start; --i)
				{
					if (hs[i] == 0)
					{
						dataFrameLength = end - i + 1;
						break;
					}
				}
			}


			// make version DataFrame
			if (dataFrameLength != 0)
			{
				var versionByte = new byte[dataFrameLength];
				Array.Copy(hs, count - dataFrameLength, versionByte, 0, dataFrameLength);
				VersionDataFrame = new DataFrame();
				VersionDataFrame.Append(versionByte);
			}

			var bytes = new ArraySegment<byte>(hs, 0, count - dataFrameLength);

			ServerHandshake handshake = null;
			if (bytes.Count > 0)
			{
				handshake = ParseServerHandshake(bytes);
			}


			var isValid = (handshake != null) &&
						(handshake.Origin == Handshake.Origin) &&
						(handshake.Location == "ws://" + Handshake.Host + Handshake.ResourcePath);

			// skip checking handshake.AnswerBytes against Handshake.ExpectedAnswer
			/*if (isValid)
			{
				for (int i = 0; i < 16; i++)
				{
					if (handshake.AnswerBytes[i] != Handshake.ExpectedAnswer[i])
					{
						isValid = false;
						break;
					}

				}
			}*/

			return isValid;
		}

		private void SendHandshake()
		{
			// generate a byte array representation of the handshake including the challenge
			byte[] hsBytes = Encoding.UTF8.GetBytes(Handshake.ToString());
			var challenge = Handshake.ChallengeBytes.Array;
			int hsBytesLength = hsBytes.Length;
			Array.Resize(ref hsBytes, hsBytesLength + challenge.Length);
			Array.Copy(challenge, 0, hsBytes, hsBytesLength, challenge.Length);

			Socket.Send(hsBytes);
		}

		private ServerHandshake ParseServerHandshake(ArraySegment<byte> byteShake)
		{
			var pattern = @"^HTTP\/1\.1 101 Switching Protocols\r\n" +
						  @"((?<field_name>[^:\r\n]+):\s(?<field_value>[^\r\n]+)\r\n)+"; // unordered set of fields (name-chars colon space any-chars cr lf)

			// subtract the challenge bytes from the handshake
			const int answerByteCount = 16;
			var handshake = new ServerHandshake();
			ArraySegment<byte> challenge = new ArraySegment<byte>(byteShake.Array, byteShake.Count - answerByteCount, answerByteCount);
			handshake.AnswerBytes = new byte[answerByteCount];
			Array.Copy(challenge.Array, challenge.Offset, handshake.AnswerBytes, 0, answerByteCount);

			// get the rest of the handshake
			var utf8_handshake = Encoding.UTF8.GetString(byteShake.Array, 0, byteShake.Count - answerByteCount);

			// match the handshake against the "grammar"
			var regex = new Regex(pattern, RegexOptions.IgnoreCase);
			var match = regex.Match(utf8_handshake);
			var fields = match.Groups;

			// run through every match and save them in the handshake object
			for (int i = 0; i < fields["field_name"].Captures.Count; i++)
			{
				var name = fields["field_name"].Captures[i].ToString();
				var value = fields["field_value"].Captures[i].ToString();

				switch (name.ToLower())
				{
					case "sec-websocket-origin":
						handshake.Origin = value;
						break;
					case "sec-websocket-location":
						handshake.Location = value;
						break;
					case "sec-websocket-protocol":
						handshake.SubProtocol = value;
						break;
				}
			}
			return handshake;
		}

	}
}
