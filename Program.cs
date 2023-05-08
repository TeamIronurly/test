using System.Net;
using System.Net.Sockets;

class Program
{

    static int port = 42069;

    public static List<Player> clients = new List<Player>();
    public static Dictionary<int,Lobby> lobbies = new Dictionary<int,Lobby>();
    static Random random = new Random(Environment.TickCount);

    static void Main()
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
        UdpClient udpListener = new UdpClient();
        udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udpListener.Client.Bind(new IPEndPoint(IPAddress.Any, port));
        tcpListener.Start();


        Log.info("server started");

        while (true)
        {
            TcpClient tcp = tcpListener.AcceptTcpClient();

            //TODO: check client id before connecting to UDP
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            udpListener.Receive(ref remoteEP);
            UdpClient udp = new UdpClient();
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            udp.Connect(remoteEP);

            Player client = new Player(tcp, udp, random.Next());
        }
    }

    public static Lobby createLobby(){
        int id = random.Next()%10000;
        while(lobbies.ContainsKey(id)){
            id = random.Next()%10000;
        }
        Lobby lobby = new Lobby(id);
        lobbies.Add(id,lobby);
        return lobby;
    }
}