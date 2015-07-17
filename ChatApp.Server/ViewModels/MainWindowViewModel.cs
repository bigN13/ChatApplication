using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using Catel.Data;
using Catel.IoC;
using Catel.Services;
using ChatApp.Library;

namespace ChatApp.ViewModels
{
    using Catel.MVVM;

    /// <summary>
    /// MainWindow view model.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string StatusText
        {
            get { return GetValue<string>(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        /// <summary>
        /// Register the StatusText property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StatusTextProperty = RegisterProperty("StatusText", typeof(string), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string StatusServer
        {
            get { return GetValue<string>(StatusServerProperty); }
            set { SetValue(StatusServerProperty, value); }
        }

        /// <summary>
        /// Register the StatusServer property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StatusServerProperty = RegisterProperty("StatusServer", typeof(string), null);



        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ArrayList ClientList
        {
            get { return GetValue<ArrayList>(ClientListProperty); }
            set { SetValue(ClientListProperty, value); }
        }

        /// <summary>
        /// Register the name property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ClientListProperty = RegisterProperty("name", typeof(ArrayList), null);



        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Socket ServerSocket
        {
            get { return GetValue<Socket>(ServerSocketProperty); }
            set { SetValue(ServerSocketProperty, value); }
        }

        /// <summary>
        /// Register the ServerSocket property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ServerSocketProperty = RegisterProperty("ServerSocket", typeof(Socket), null);



        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public byte[] DataStream
        {
            get { return GetValue<byte[]>(DataStreamProperty); }
            set { SetValue(DataStreamProperty, value); }
        }

        /// <summary>
        /// Register the DataStream property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DataStreamProperty = RegisterProperty("DataStream", typeof(byte[]), null);


        // Structure to store the client information
        private struct Client
        {
            public EndPoint endPoint;
            public string name;
        }

        // Status delegate
        private delegate void UpdateStatusDelegate(string status);
        private UpdateStatusDelegate updateStatusDelegate = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
            : base()
        {
            StartServer = new Command(OnStartServerExecute, OnStartServerCanExecute);
            ExitServer = new Command(OnExitServerExecute, OnExitServerCanExecute);
            DataStream = new byte[1024];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "View model title"; } }

        
        // TODO: Register models with the vmpropmodel codesnippet
        // TODO: Register view model properties with the vmprop or vmpropviewmodeltomodel codesnippets
        #endregion

        #region Commands
        // TODO: Register commands with the vmcommand or vmcommandwithcanexecute codesnippets
       
        /// <summary>
        /// Gets the StartServer command.
        /// </summary>
        public Command StartServer { get; private set; }

        /// <summary>
        /// Method to check whether the StartServer command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnStartServerCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the StartServer command is executed.
        /// </summary>
        private void OnStartServerExecute()
        {
            try
            {
                // Initialise the ArrayList of connected clients
                ClientList = new ArrayList();

                // Initialise the socket
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // Initialise the IPEndPoint for the server and listen on port 30000
                IPEndPoint server = new IPEndPoint(IPAddress.Any, 30000);

                // Associate the socket with this IP address and port
                ServerSocket.Bind(server);

                // Initialise the IPEndPoint for the clients
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);

                // Initialise the EndPoint for the clients
                EndPoint epSender = (EndPoint)clients;

                // Start listening for incoming data
                ServerSocket.BeginReceiveFrom(DataStream, 0, DataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);

                StatusServer = "Listening";
            }
            catch (Exception ex)
            {
                StatusServer = "Error";

                var dependencyResolver = this.GetDependencyResolver();
                var messageService = dependencyResolver.Resolve<IMessageService>();
                messageService.ShowError("Load Error: " + ex.Message, "UDP Server");
            }
        }



        /// <summary>
        /// Gets the ExitServer command.
        /// </summary>
        public Command ExitServer { get; private set; }


        /// <summary>
        /// Method to check whether the ExitServer command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnExitServerCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ExitServer command is executed.
        /// </summary>
        private void OnExitServerExecute()
        {
            CloseViewModel(true);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Send data to Clients
        /// </summary>
        /// <param name="asyncResult"></param>
        public void SendData(IAsyncResult asyncResult)
        {
            try
            {
                ServerSocket.EndSend(asyncResult);
            }
            catch (Exception ex)
            {
                var dependencyResolver = this.GetDependencyResolver();
                var messageService = dependencyResolver.Resolve<IMessageService>();
                messageService.ShowError("SendData Error: " + ex.Message, "UDP Server");
            }
        }

        /// <summary>
        /// Receive data from clients
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                byte[] data;

                // Initialise a packet object to store the received data
                Packet receivedData = new Packet(DataStream);

                // Initialise a packet object to store the data to be sent
                Packet sendData = new Packet();

                // Initialise the IPEndPoint for the clients
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);

                // Initialise the EndPoint for the clients
                EndPoint epSender = (EndPoint)clients;

                // Receive all data
                ServerSocket.EndReceiveFrom(asyncResult, ref epSender);

                // Start populating the packet to be sent
                sendData.ChatDataIdentifier = receivedData.ChatDataIdentifier;
                sendData.ChatName = receivedData.ChatName;

                switch (receivedData.ChatDataIdentifier)
                {
                    case DataIdentifier.Message:
                        sendData.ChatMessage = string.Format("{0}: {1}", receivedData.ChatName, receivedData.ChatMessage);
                        break;

                    case DataIdentifier.LogIn:
                        // Populate client object
                        Client client = new Client();
                        client.endPoint = epSender;
                        client.name = receivedData.ChatName;

                        // Add client to list
                        ClientList.Add(client);

                        sendData.ChatMessage = string.Format("-- {0} is online --", receivedData.ChatName);
                        break;

                    case DataIdentifier.LogOut:
                        // Remove current client from list
                        foreach (Client c in ClientList)
                        {
                            if (c.endPoint.Equals(epSender))
                            {
                                ClientList.Remove(c);
                                break;
                            }
                        }

                        sendData.ChatMessage = string.Format("-- {0} has gone offline --", receivedData.ChatName);
                        break;
                }

                // Get packet as byte array
                data = sendData.GetDataStream();

                foreach (Client client in ClientList)
                {
                    if (client.endPoint != epSender || sendData.ChatDataIdentifier != DataIdentifier.LogIn)
                    {
                        // Broadcast to all logged on users
                        ServerSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, client.endPoint, new AsyncCallback(SendData), client.endPoint);
                    }
                }

                // Listen for more connections again...
                ServerSocket.BeginReceiveFrom(DataStream, 0, DataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);

                // Update status through a delegate

                if (!string.IsNullOrEmpty(sendData.ChatMessage))
                {
                    UpdateStatus(sendData.ChatMessage);
                }
            }
            catch (Exception ex)
            {
                var dependencyResolver = this.GetDependencyResolver();
                var messageService = dependencyResolver.Resolve<IMessageService>();
                messageService.ShowError("ReceiveData Error: " + ex.Message, "UDP Server");
            }
        }



        private void UpdateStatus(string status)
        {
            StatusText += status + Environment.NewLine;
        }
        #endregion
    }
}
