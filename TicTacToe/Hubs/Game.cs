using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TicTacToe.Hubs
{
    public class Game : Hub
    {
        private static List<Client> clients = new List<Client>();
        private static List<TicTacToeGame> games = new List<TicTacToeGame>();
        private static object syncRoot = new object();
        private static Random random = new Random();

        public void RegisterClient(string data)
        {
            lock (syncRoot)
            {
                var client = clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                if (client == null)
                {
                    client = new Client { ConnectionId = Context.ConnectionId, Name = data };
                    clients.Add(client);
                }

                client.IsPlaying = false;
            }
        }
        public void FindOpponent()
        {
            var player = clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (player == null)
            { 
                return; 
            }
            player.LookingForOpponent = true;

            //Szukanie losowego przeciwnika, jeśli jest więcej niż jeden szukający gry
            var opponent = clients.Where(x => x.ConnectionId != Context.ConnectionId && x.LookingForOpponent && !x.IsPlaying).FirstOrDefault();
            if (opponent == null)
            {
                Clients.Client(Context.ConnectionId).SendAsync("noOpponents");
                return;
            }

            player.IsPlaying = true;
            player.LookingForOpponent = false;
            opponent.IsPlaying = true;
            opponent.LookingForOpponent = false;

            player.Opponent = opponent;
            opponent.Opponent = player;

            // Informacja dla graczy i przeciwnikach
            Clients.Client(Context.ConnectionId).SendAsync("foundOpponent", opponent.Name, player.Name);
            Clients.Client(opponent.ConnectionId).SendAsync("foundOpponent", player.Name, opponent.Name);

            if (random.Next(0, 1000) % 3 == 0)
            {
                player.WaitingForMove = false;
                opponent.WaitingForMove = true;

                Clients.Client(player.ConnectionId).SendAsync("waitingForMarkerPlacement", opponent.Name);
                Clients.Client(opponent.ConnectionId).SendAsync("waitingForOpponent", opponent.Name);
            }
            else
            {
                player.WaitingForMove = true;
                opponent.WaitingForMove = false;

                Clients.Client(opponent.ConnectionId).SendAsync("waitingForMarkerPlacement", opponent.Name);
                Clients.Client(player.ConnectionId).SendAsync("waitingForOpponent", opponent.Name);
            }

            lock (syncRoot)
            {
                games.Add(new TicTacToeGame { Player1 = player, Player2 = opponent });
            }
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var game = games.FirstOrDefault(x => x.Player1.ConnectionId == Context.ConnectionId || x.Player2.ConnectionId == Context.ConnectionId);
            // Klient nie był uczestnikiem gry
            if (game == null)
            {
                var clientWithoutGame = clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                if (clientWithoutGame != null)
                {
                    clients.Remove(clientWithoutGame);
                }
                return base.OnDisconnectedAsync(exception);
            }

            // Klient był uczestnikiem gry
            if (game != null)
            {
                games.Remove(game);
            }

            //Informacja o rozłączającym się graczu
            var client = game.Player1.ConnectionId == Context.ConnectionId ? game.Player1 : game.Player2;
            //Nie można odnaleźć gracza
            if (client == null) return base.OnDisconnectedAsync(exception);

            clients.Remove(client);
            if (client.Opponent != null)
            {
                return Clients.Client(client.Opponent.ConnectionId).SendAsync("opponentDisconnected", client.Name);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public void Play(int position)
        {
            var game = games.FirstOrDefault(x => x.Player1.ConnectionId == Context.ConnectionId || x.Player2.ConnectionId == Context.ConnectionId);

            if (game == null || game.IsGameOver)
            {
                return;
            }

            int marker = 0;

            if (game.Player2.ConnectionId == Context.ConnectionId)
            {
                marker = 1;
            }
            var player = marker == 0 ? game.Player1 : game.Player2;

            if (player.WaitingForMove)
            {
                return;
            }

            Clients.Client(game.Player1.ConnectionId).SendAsync("addMarkerPlacement", new GameInformation { OpponentName = player.Name, MarkerPosition = position });
            Clients.Client(game.Player2.ConnectionId).SendAsync("addMarkerPlacement", new GameInformation { OpponentName = player.Name, MarkerPosition = position });

            if (game.Play(marker, position))
            {
                games.Remove(game);
                Clients.Client(game.Player1.ConnectionId).SendAsync("gameOver", player.Name);
                Clients.Client(game.Player2.ConnectionId).SendAsync("gameOver", player.Name);
            }

            if (game.IsGameOver && game.IsDraw)
            {
                games.Remove(game);
                Clients.Client(game.Player1.ConnectionId).SendAsync("gameOver", "Remis!");
                Clients.Client(game.Player2.ConnectionId).SendAsync("gameOver", "Remis!");
            }

            if (!game.IsGameOver)
            {
                //Zamiana zmiennych waitingForMove, które są definiowane na początku gry
                player.WaitingForMove = !player.WaitingForMove;
                player.Opponent.WaitingForMove = !player.Opponent.WaitingForMove;

                Clients.Client(player.Opponent.ConnectionId).SendAsync("waitingForMarkerPlacement", player.Name);
            }
        }
    }
}
