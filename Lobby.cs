class Lobby
{
    List<Player> players = new List<Player>();
    int maxPlayers = 2;

    public int id;

    public Lobby(int id){
        this.id = id;
        Log.info($"lobby {id} created");
    }

    public bool join(Player player)
    {
        if (players.Count >= maxPlayers)
            return false;
        players.Add(player);
        sendToAll(player, new Packet(Packet.Type.JOINED, player));
        for(int i = 0;i<players.Count;i++){
            player.send(new Packet(Packet.Type.JOINED, players[i]));
        }
        Log.info($"player {player.id} joined lobby {id}");
        return true;
    }

    public void leave(Player player)
    {
        players.Remove(player);
        sendToAll(player, new Packet(Packet.Type.LEFT, player));
        if(players.Count==0){
            Program.lobbies.Remove(id);
            Log.info($"lobby {id} deleted");
        }
        Log.info($"player {player.id} left lobby {id}");
    }

    public void sendToAll(Player player, Packet packet)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == player) continue;
            players[i].send(packet);
        }
    }
}
