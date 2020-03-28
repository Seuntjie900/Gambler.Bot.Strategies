using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using DoormatCore.Sites;
using DoormatCore.Helpers;
using DoormatCore.Games;

namespace DoormatControllers
{
    class Websockethandler
    {
        public const int BufferSize = 4096;

        WebSocket socket;

        Websockethandler(WebSocket socket)
        {
            this.socket = socket;
            if (Program.DiceBot != null)
            {
                Program.DiceBot.OnSiteBetFinished += DiceBot_OnSiteDiceBetFinished;
                Program.DiceBot.OnSiteLoginFinished += DiceBot_OnSiteLoginFinished;
            }
        }

        private void DiceBot_OnSiteLoginFinished(object sender, LoginFinishedEventArgs e)
        {
            //this.socket.SendAsync( Encoding.UTF8.GetBytes(e.Success? json.JsonSerializer<SiteStats>(e.Stats):e.Success.ToString()), WebSocketMessageType.Text, true, CancellationToken.None);
        }


        private void DiceBot_OnSiteDiceBetFinished(object sender, BetFinisedEventArgs e)
        {
            /*if (e.NewBet is DiceBet)
                this.socket.SendAsync(Encoding.UTF8.GetBytes(json.JsonSerializer<DiceBet>(e.NewBet as DiceBet)), WebSocketMessageType.Text, true, CancellationToken.None);
           */ 
        }

        async Task EchoLoop()
        {
            var buffer = new byte[BufferSize];
            var seg = new ArraySegment<byte>(buffer);

            while (this.socket.State == WebSocketState.Open)
            {
                var incoming = await this.socket.ReceiveAsync(seg, CancellationToken.None);
                var outgoing = new ArraySegment<byte>(buffer, 0, incoming.Count);
                await this.socket.SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        static async Task Acceptor(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;

            var socket = await hc.WebSockets.AcceptWebSocketAsync();
            var h = new Websockethandler(socket);
           
            await h.EchoLoop();
        }

        public static void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(Websockethandler.Acceptor);
        }
    }
}
