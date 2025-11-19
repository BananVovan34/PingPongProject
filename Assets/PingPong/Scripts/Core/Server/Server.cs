using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Relay.Models;

namespace PingPong.Scripts.Core.Server
{
    public class Server : MonoBehaviour
    {
        public const ushort DefaultPort = 7777;
        public const int MaxPlayers = 2;

        // private async void Awake()
        // {
        //     Debug.Log("[Server] Initializing dedicated server...");
        //     try
        //     {
        //         await UnityServices.InitializeAsync();
        //         Debug.Log("[Server] Services initialized");
        //
        //         await AuthenticateServerAsync();
        //         await StartRelayServerAsync();
        //         await RegisterMatchmakerTicketAsync();
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"[Server] Failed: {e}");
        //     }
        // }
        //
        // private async Task AuthenticateServerAsync()
        // {
        //     string serverKey = Environment.GetEnvironmentVariable("UGS_SERVER_KEY");
        //     if (string.IsNullOrEmpty(serverKey))
        //     {
        //         throw new Exception("UGS_SERVER_KEY not set");
        //     }
        //     await AuthenticationService.Instance.SignInWithServerAsync(serverKey);
        //     Debug.Log($"[Server] Authenticated as {AuthenticationService.Instance.PlayerId}");
        // }
        //
        // private async Task StartRelayServerAsync()
        // {
        //     var allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        //     Debug.Log("[Server] Relay allocation created");
        //
        //     var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //     if (transport == null) throw new Exception("UnityTransport missing");
        //
        //     var relayServerData = AllocationUtils.ToRelayServerData(allocation, "udp");
        //     transport.SetRelayServerData(
        //         relayServerData.IpAddress,
        //         relayServerData.Port,
        //         relayServerData.AllocationIdBytes,
        //         relayServerData.Key,
        //         relayServerData.ConnectionData,
        //         relayServerData.HostConnectionData,
        //         relayServerData.IsSecure
        //     );
        //
        //     NetworkManager.Singleton.StartServer();
        //     Debug.Log("[Server] Relay server started");
        // }
        //
        // private async Task RegisterMatchmakerTicketAsync()
        // {
        //     var options = new CreateTicketOptions
        //     {
        //         QueueName = "default",
        //     };
        //
        //     var ticket = await MatchmakerService.Instance.CreateTicketAsync(options);
        //     Debug.Log($"[Server] Matchmaker ticket created: {ticket.TicketId}");
        // }
    }
}
