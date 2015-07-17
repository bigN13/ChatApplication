using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using Catel.Data;
using Catel.IoC;
using Catel.Services;
using ChatApp.Library;

namespace ChatApp.Client.ViewModels
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
        public string UserName
        {
            get { return GetValue<string>(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        /// <summary>
        /// Register the UserName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UserNameProperty = RegisterProperty("UserName", typeof(string), null);


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ServerIP
        {
            get { return GetValue<string>(ServerIPProperty); }
            set { SetValue(ServerIPProperty, value); }
        }

        /// <summary>
        /// Register the ServerIP property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ServerIPProperty = RegisterProperty("ServerIP", typeof(string), null);



        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ConversationText
        {
            get { return GetValue<string>(ConversationTextProperty); }
            set { SetValue(ConversationTextProperty, value); }
        }

        /// <summary>
        /// Register the ConversationText property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ConversationTextProperty = RegisterProperty("ConversationText", typeof(string), null);


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string MessageText
        {
            get { return GetValue<string>(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }

        /// <summary>
        /// Register the MessageText property so it is known in the class.
        /// </summary>
        public static readonly PropertyData MessageTextProperty = RegisterProperty("MessageText", typeof(string), null);


        // Server End Point
        private EndPoint epServer;

        // Data stream
        private byte[] dataStream = new byte[1024];

        // Display message delegate
        private delegate void DisplayMessageDelegate(string message);
        private DisplayMessageDelegate displayMessageDelegate = null;



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
        public string ClientName
        {
            get { return GetValue<string>(ClientNameProperty); }
            set { SetValue(ClientNameProperty, value); }
        }

        /// <summary>
        /// Register the ClientName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ClientNameProperty = RegisterProperty("ClientName", typeof(string), null);


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
        public Socket ClientSocket
        {
            get { return GetValue<Socket>(ClientSocketProperty); }
            set { SetValue(ClientSocketProperty, value); }
        }

        /// <summary>
        /// Register the ClientSocket property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ClientSocketProperty = RegisterProperty("ClientSocket", typeof(Socket), null);


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

            ConnectServer = new Command(OnConnectServerExecute, OnConnectServerCanExecute);
            SendMessage = new Command(OnSendMessageExecute, OnSendMessageCanExecute);
            ExitApplication = new Command(OnExitApplicationExecute, OnExitApplicationCanExecute);

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
        /// Gets the ConnectServer command.
        /// </summary>
        public Command ConnectServer { get; private set; }

        /// <summary>
        /// Method to check whether the ConnectServer command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnConnectServerCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ConnectServer command is executed.
        /// </summary>
        private void OnConnectServerExecute()
        {
            try
            {
                // Initialise a packet object to store the data to be sent
                Packet sendData = new Packet();
                sendData.ChatName = UserName.Trim();
                sendData.ChatMessage = null;
                sendData.ChatDataIdentifier = DataIdentifier.LogIn;

                // Initialise socket
                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (string.IsNullOrEmpty(ServerIP))
                {
                    var dependencyResolver = this.GetDependencyResolver();
                    var messageService = dependencyResolver.Resolve<IMessageService>();
                    messageService.ShowError("Server IP is empty!", "Error");
                    return;
                }

                if (string.IsNullOrEmpty(UserName))
                {
                    var dependencyResolver = this.GetDependencyResolver();
                    var messageService = dependencyResolver.Resolve<IMessageService>();
                    messageService.ShowError("UserName is empty!", "Error");
                    return;
                }

                // Initialise server IP
                IPAddress serverIP = IPAddress.Parse(ServerIP.Trim());

                // Initialise the IPEndPoint for the server and use port 30000
                IPEndPoint server = new IPEndPoint(serverIP, 30000);

                // Initialise the EndPoint for the server
                epServer = (EndPoint)server;

                // Get packet as byte array
                byte[] data = sendData.GetDataStream();

                // Send data to server
                ClientSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, epServer, new AsyncCallback(SendData), null);

                // Initialise data stream
                this.dataStream = new byte[1024];

                // Begin listening for broadcasts
                ClientSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);
            }
            catch (Exception ex)
            {
                var dependencyResolver = this.GetDependencyResolver();
                var messageService = dependencyResolver.Resolve<IMessageService>();
                messageService.ShowError("Connection Error: " + ex.Message, "UDP Client");
            }
        }



        /// <summary>
        /// Gets the SendMessage command.
        /// </summary>
        public Command SendMessage { get; private set; }


        /// <summary>
        /// Method to check whether the SendMessage command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSendMessageCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SendMessage command is executed.
        /// </summary>
        private void OnSendMessageExecute()
        {
            try
            {
                // Initialise a packet object to store the data to be sent
                Packet sendData = new Packet();
                sendData.ChatName = UserName;
                sendData.ChatMessage = MessageText.Trim();
                sendData.ChatDataIdentifier = DataIdentifier.Message;


                if (string.IsNullOrEmpty(MessageText))
                {
                    var dependencyResolver = this.GetDependencyResolver();
                    var messageService = dependencyResolver.Resolve<IMessageService>();
                    messageService.ShowError("Message can not be empty!", "Error");
                    return;
                }

                // Get packet as byte array
                byte[] byteData = sendData.GetDataStream();

                // Send packet to the server
                ClientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(SendData), null);

                MessageText = string.Empty;
            }
            catch (Exception ex)
            {
                var dependencyResolver = this.GetDependencyResolver();
                var messageService = dependencyResolver.Resolve<IMessageService>();
                messageService.ShowError("Send Error: " + ex.Message, "UDP Client");
            }
        }


        /// <summary>
        /// Gets the ExitApplication command.
        /// </summary>
        public Command ExitApplication { get; private set; }

        /// <summary>
        /// Method to check whether the ExitApplication command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnExitApplicationCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ExitApplication command is executed.
        /// </summary>
        private void OnExitApplicationExecute()
        {
            CloseViewModel(true);
        }
        #endregion

        #region Methods
        private void SendData(IAsyncResult ar)
        {
            try
            {
                ClientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                var dependencyResolver = this.GetDependencyResolver();
                var messageService = dependencyResolver.Resolve<IMessageService>();
                messageService.ShowError("Send Data: " + ex.Message, "UDP Client");
            }
        }

        private void ReceiveData(IAsyncResult ar)
        {
            try
            {
                // Receive all data
                ClientSocket.EndReceive(ar);

                // Initialise a packet object to store the received data
                Packet receivedData = new Packet(this.dataStream);

                // Update display through a delegate
                if (receivedData.ChatMessage != null)
                {
                    DisplayMessage(receivedData.ChatMessage);
                }
                    //this.Invoke(this.displayMessageDelegate, new object[] { receivedData.ChatMessage });

                // Reset data stream
                this.dataStream = new byte[1024];

                // Continue listening for broadcasts
                ClientSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                var dependencyResolver = this.GetDependencyResolver();
                var messageService = dependencyResolver.Resolve<IMessageService>();
                messageService.ShowError("Receive Data: " + ex.Message, "UDP Client");
            }
        }

        private void DisplayMessage(string messge)
        {
            ConversationText += messge + Environment.NewLine;
        }
        #endregion
    }
}
