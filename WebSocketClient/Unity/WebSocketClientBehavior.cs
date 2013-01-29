using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;

using WebSocketClient;

public class WebSocketClientBehavior : MonoBehaviour
{
	ClientSocket client;

	// Use this for initialization
	void Start()
	{
		client = new ClientSocket(new ExtendedClientHandshake()
			{
				Origin = "leap",
				Host = "localhost:6437",
				ResourcePath = "/get"
			});

		client.Connect();
	}

	void OnDestroy()
	{
		client.Disconnect();
	}

	// Update is called once per frame
	void Update()
	{
		var answer = client.Receive();
		Debug.Log(answer);
	}
}
