# ChatApplication
C# WPF Chat Application Over Asynchronous UDP Sockets with Catel MVVM Framework

The infrastructure of this chat application is a simple client-server infrastructure.
The server will be the central point which all clients connect to, and the communication between server and 
clients will be done using a custom data packet which will contain the data being sent over UDP.

##The Server
The server  allow clients to connect to it and then broadcast each message sent by the client. 
This means that every message a client sends will be received by all other connected clients. 
This chat application is like a chat room where everyone chats together. 
It does not support one to one chat.

##The Client
The server is listening for incoming connections. 
The client must know the IP address of the server and which port the server is listening on. 
The server is listening on port 30,000. 
The server’s IP address will have to be input manually from the user interface of the client. 

ScreenCast
http://screencast.com/t/uP94rTP6o