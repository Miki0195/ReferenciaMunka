namespace Bomberman.Models.SignalR
{
    public class LobbyDisplayViewModel
    {
        public string Map { get; set; } = "";
        public int CurrentPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int LobbyNum { get; set; }

    }
}
