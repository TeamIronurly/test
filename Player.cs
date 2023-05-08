using System.Net;
using System.Net.Sockets;

class Player
{
    Lobby? lobby;
    public int id = 0;
    TcpClient tcpClient;
    UdpClient udp;
    NetworkStream tcp;
    public Player(TcpClient tcpClient, UdpClient udpClinet, int id)
    {
        this.id = id;
        this.tcpClient = tcpClient;
        this.tcp = tcpClient.GetStream();
        this.udp = udpClinet;
        startReceiveLoops();
        Log.info($"player {id} connected");
    }

    public void send(Packet packet)
    {
        if (packet.protocol == Packet.Protocol.TCP)
        {
            tcp.Write(packet.bytes);
        }
        else
        {
            udp.Send(packet.bytes);
        }
    }

    void receive(Packet packet)
    {
        if (packet.type == Packet.Type.PING)
        {
            send(packet);
        }
        else if (packet.type == Packet.Type.GET_ID)
        {
            send(new Packet(Packet.Type.GET_ID, this));
        }
        else if (packet.type == Packet.Type.CREATE_LOBBY)
        {
            lobby = Program.createLobby();
            lobby.join(this);
            send(new Packet(Packet.Type.CREATED_LOBBY, this, lobby.id));
            send(new Packet(Packet.Type.JOINED, this));
        }
        else if (packet.type == Packet.Type.JOIN)
        {
            int lobby_id = BitConverter.ToInt32(packet.bytes, 8);
            if (Program.lobbies.ContainsKey(lobby_id) && Program.lobbies[lobby_id].join(this))
            {
                send(new Packet(Packet.Type.JOINED, this));
                lobby = Program.lobbies[lobby_id];
            }
            else
            {
                send(new Packet(Packet.Type.JOIN_FAILED, this));
            }

        }
        else if (packet.type == Packet.Type.LEFT)
        {
            lobby?.leave(this);
            lobby = null;
        }
        else
        {
            lobby?.sendToAll(this, packet);
        }
    }

    void startReceiveLoops()
    {
        new Thread(TcpLoop).Start();
        new Thread(UdpLoop).Start();
    }

    void TcpLoop()
    {
        try
        {
            while (tcpClient.Connected)
            {
                byte[] typeBuffer = new byte[4];
                tcp.Read(typeBuffer, 0, 4);
                Packet.Type type = (Packet.Type)BitConverter.ToInt32(typeBuffer);
                byte[] bytes = new byte[Packet.Lengths[type]];
                typeBuffer.CopyTo(bytes, 0);
                int read = tcp.Read(bytes, 4, Packet.Lengths[type] - 4);
                if (read != Packet.Lengths[type] - 4)
                {
                    tcpClient.Close();
                    break;
                }
                receive(new Packet(Packet.Protocol.TCP, bytes));
            }
        }
        catch (Exception)
        {
            tcpClient.Close();
        }
        tcpClient.Close();
        udp.Close();
        lobby?.leave(this);
        Log.info($"player {id} disconnected");
    }
    void UdpLoop()
    {
        try
        {
            while (tcpClient.Connected)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udp.Receive(ref ip);
                receive(new Packet(Packet.Protocol.UDP, bytes));
            }
        }
        catch (Exception)
        {
            udp.Close();
        }
    }
}
