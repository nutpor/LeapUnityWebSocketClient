===========================
Leap Unity WebSocket Client
===========================

Using Leap with Unity standard via WebSocket.

WebSocket Client using code from : Nugget Web Socket Server
http://nugget.codeplex.com/

JSON parser using only parser from : light-weight-json-library
http://code.google.com/p/light-weight-json-library/

===========================
Notes
===========================
- This client has been testing and tuning for using with Leap's WebSocket Server.
- It seem that "id" is jumping from time to time(not coming in the perfect sequence).
I've tested with javascript version and the "id" has skipped as well.
- "version" is keep from JSON data but not check against anything :P
- No data frame caching yet.
- Currently what it does is getting JSON data from server like javascript get 
and missing the mapping data from JSON to Leap's data structures, feel free to expand it further :)

===========================
Guide to use in Unity
===========================
1. Add folder and all files in "WebSocketClient" into your Unity project.
2. You can use "WebSocketClientBehavior.cs" in Unity folder as a starting point, just attach to any GameObject in scene.
3. JSON string of current receieved data frame will print via Debug.Log();
