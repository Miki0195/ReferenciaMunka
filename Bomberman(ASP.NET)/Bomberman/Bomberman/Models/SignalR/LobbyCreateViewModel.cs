using System.ComponentModel.DataAnnotations;

namespace Bomberman.Models.SignalR
{
    public class LobbyCreateViewModel
    {
        public string Map { get; set; } = "";

        [Required]
        public int MaxPlayers { get; set; }
        public List<string> Maps { get; set; } = new List<string>();
        public string ErrorMessage { get; set; } = "";
    }
}
