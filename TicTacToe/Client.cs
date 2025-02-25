using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class Client
    {
        public string Name { get; set; }
        public Client Opponent { get; set; }
        public string ConnectionId { get; set; }
        public bool LookingForOpponent { get; set; }
        public bool IsPlaying { get; set; }
        public bool WaitingForMove { get; set; }
    }
}
