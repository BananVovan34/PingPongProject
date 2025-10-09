using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PingPong.Scripts.Multiplayer.Lobby
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        /// <summary>
        /// Host lobby, if you hosted
        /// </summary>
        private Unity.Services.Lobbies.Models.Lobby _hostLobby;
        /// <summary>
        /// Joined lobby, if you joined
        /// </summary>
        private Unity.Services.Lobbies.Models.Lobby _joinedLobby;
        /// <summary>
        /// Send heartbeat to UGS to tell lobby is alive
        /// </summary>
        private float _heartbeatTimer;

        private const string RelayCodeKey = "RelayCode";
        private const int MaxPlayers = 2;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            await InitializeUnityAuthentication();
        }

        /// <summary>
        /// UGS authentication
        /// </summary>
        private async Task InitializeUnityAuthentication()
        {
            await UnityServices.InitializeAsync();
            Debug.Log($"Unity Services initialized, environment: {UnityServices.ExternalUserId}");

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Signed in as player: {AuthenticationService.Instance.PlayerId}");
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private void Update()
        {
            HandlerLobbyHeartbeat();
        }

        /// <summary>
        /// Lobby heartbeat logic to send packets lobby is alive
        /// </summary>
        private async void HandlerLobbyHeartbeat()
        {
            if (_hostLobby == null) return;

            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer <= 0f)
            {
                _heartbeatTimer = 15f;
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
                    Debug.Log("Heartbeat sent");
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        public void CreateLobby()
        {
            _ = CreateLobby("New Lobby", false);
        }

        /// <summary>
        /// Create lobby with two params
        /// </summary>
        /// <param name="lobbyName"></param>
        /// <param name="isPrivate"></param>
        public async Task CreateLobby(string lobbyName = "New Lobby", bool isPrivate = false) {
            try
            {
                Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                    lobbyName,
                    MaxPlayers,
                    new CreateLobbyOptions
                    {
                        IsPrivate = isPrivate
                    }
                    );
                
                _hostLobby = lobby;
                
                Debug.Log($"Lobby created: {lobby.Name}, id: {lobby.Id}");

                await SetupRelayForHost();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        /// <summary>
        /// Create Peer-2-peer connection with players using UGS
        /// </summary>
        private async Task SetupRelayForHost()
        {
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { RelayCodeKey, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                    }
                };
                
                await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, updateLobbyOptions);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
                
                NetworkManager.Singleton.StartHost();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async Task DeleteLobby()
        {
            if (_hostLobby == null) return;
            
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_hostLobby.Id);
                Debug.Log("Lobby deleted");
                _hostLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async Task QuickJoin()
        {
            try
            {
                _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                Debug.Log("Quick joined lobby: " + _joinedLobby.Name);

                await JoinRelayFromLobby(_joinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async Task JoinRelayFromLobby(Unity.Services.Lobbies.Models.Lobby joinedLobby)
        {
            try
            {
                string relayCode = joinedLobby.Data[RelayCodeKey].Value;
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

                NetworkManager.Singleton.StartHost();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async void ListLobbies()
        {
            try
            {
                var queryLobbies = await LobbyService.Instance.QueryLobbiesAsync();
                Debug.Log($"Lobbies found: {queryLobbies.Results.Count}");
                
                foreach (var lobby in queryLobbies.Results)
                    Debug.Log($"Lobby: {lobby.Name}, id: {lobby.Id}");
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public void QuickJoinLobby()
        {
            _ = QuickJoin();
        }

        public void StartGame()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
    }
}
