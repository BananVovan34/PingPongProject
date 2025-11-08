using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PingPong.Scripts.Core.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PingPong.Scripts.Core.Lobby
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        /// <summary>
        /// Current lobby (host/joined)
        /// </summary>
        private Unity.Services.Lobbies.Models.Lobby _currentLobby;
        private bool _isCreatingLobby;
        private string _relayJoinCode;
        
        private Player _player;
        private string _playerId;

        public string RelayJoinCode
        {
            get
            {
                if (_currentLobby?.Data != null && _currentLobby.Data.ContainsKey(RelayCodeKey))
                    return _currentLobby.Data[RelayCodeKey].Value;
                return _relayJoinCode;
            }
        }
        public string PlayerId => _playerId;
        public bool IsPlayerHost => _currentLobby != null && _currentLobby.HostId == PlayerId;
        
        private float _heartbeatTimer = 15f;
        private ILobbyEvents _lobbyEvents;

        private const string BasePlayerName = "Player";
        private const string RelayCodeKey = "RelayCode";
        private string PlayerNameKey => "PlayerName";
        private const int MaxPlayers = 2;
        
        public Action<List<string>> LobbyPlayersChanged;
        public Action<string> RelayJoinCodeChanged;
        public event Action OnLobbyCreatedSuccessfully;
        public event Action OnLobbyJoinedSuccessfully;
        public event Action OnLobbyLeftSuccessfully;
        public event Action<string> OnLobbyJoinFailed;

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
            MenuUIManager.Instance.JoinWithCodeButtonPressed += TryJoinWithCode;
            
            await InitializeUnityAuthentication();
            CreatePlayer();
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
                _playerId = AuthenticationService.Instance.PlayerId;
                Debug.Log($"Signed in as player: {PlayerId}");
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        
        private void CreatePlayer()
        {
            var playerName = BasePlayerName + Random.Range(1, 101);
            
            _player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>()
                {
                    { PlayerNameKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
                }
            };
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
            if (_currentLobby == null) return;
            if (!IsPlayerHost) return;
            if (!AuthenticationService.Instance.IsSignedIn) return;

            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer <= 0f)
            {
                _heartbeatTimer = 15f;
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
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
            if (!UnityServices.State.Equals(ServicesInitializationState.Initialized) || 
                !AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("Unity Services not initialized or player not signed in.");
                return;
            }
            if (_isCreatingLobby) return;
            if (_currentLobby != null)
            {
                Debug.LogWarning($"Lobby already exists: {_currentLobby.Id}");
                return;
            }

            try
            {
                _isCreatingLobby = true;
                Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                    lobbyName,
                    MaxPlayers,
                    new CreateLobbyOptions
                    {
                        IsPrivate = isPrivate,
                        Player = _player
                    }
                );

                _currentLobby = lobby;

                Debug.Log($"Lobby created: {lobby.Name}, id: {lobby.Id}");
                UpdatePlayersList();

                await SetupRelayForHost();

                var callbacks = new LobbyEventCallbacks();
                callbacks.PlayerJoined += PlayerJoinedHandler;
                callbacks.PlayerLeft += PlayerLeftHandler;
                callbacks.LobbyDeleted += OnLobbyDeleted;

                try
                {
                    await SubscribeToLobbyEvents(_currentLobby);
                }
                catch (LobbyServiceException e)
                {
                    switch (e.Reason)
                    {
                        case LobbyExceptionReason.AlreadySubscribedToLobby:
                            Debug.LogWarning(
                                $"Already subscribed to lobby[{lobby.Id}]. We did not need to try and subscribe again. Exception Message: {e.Message}");
                            break;
                        case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
                            Debug.LogError(
                                $"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {e.Message}");
                            throw;
                        case LobbyExceptionReason.LobbyEventServiceConnectionError:
                            Debug.LogError($"Failed to connect to lobby events. Exception Message: {e.Message}");
                            throw;
                        default: throw;
                    }
                }
                
                OnLobbyCreatedSuccessfully?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
            finally
            {
                _isCreatingLobby = false;
            }
        }

        private void OnLobbyDeleted()
        {
            Debug.Log("Lobby deleted by host. Disconnecting...");

            _currentLobby = null;
            _relayJoinCode = null;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            _ = UnsubscribeFromLobbyEvents();
        }

        private async void PlayerJoinedHandler(List<LobbyPlayerJoined> joinedPlayers)
        {
            Debug.Log($"{joinedPlayers.Count} players joined.");
    
            try
            {
                _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
                UpdatePlayersList();
            }
            catch (Exception e)
            {
                Debug.Log($"Error updating lobby after player join: {e}");
            }
        }

        private async void PlayerLeftHandler(List<int> leftPlayers)
        {
            Debug.Log($"{leftPlayers.Count} players left.");
            
            try
            {
                _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
                UpdatePlayersList();
            }
            catch (Exception e)
            {
                Debug.Log($"Error updating lobby after player left: {e}");
            }
        }

        private void UpdatePlayersList()
        {
            Debug.Log("UpdatePlayersList called");
            if (_currentLobby == null)
            {
                Debug.LogWarning("currentLobby == null");
                return;
            }

            if (_currentLobby.Players == null || _currentLobby.Players.Count == 0)
            {
                Debug.LogWarning("no players in current lobby");
            }

            var players = new List<string>();
            foreach (var p in _currentLobby.Players)
            {
                string name = p.Profile?.Name;
                if (string.IsNullOrEmpty(name) && p.Data != null && p.Data.ContainsKey("PlayerName"))
                    name = p.Data["PlayerName"].Value;
                players.Add(string.IsNullOrEmpty(name) ? p.Id : name);
            }

            Debug.Log("Players: " + string.Join(", ", players));
            LobbyPlayersChanged?.Invoke(players);
        }

        /// <summary>
        /// Create Peer-2-peer connection with players using UGS
        /// </summary>
        private async Task SetupRelayForHost()
        {
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
                _relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                Debug.Log($"Allocation created. Join code: {_relayJoinCode}");

                UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { RelayCodeKey, new DataObject(DataObject.VisibilityOptions.Public, _relayJoinCode, DataObject.IndexOptions.S1) }
                    }
                };
                
                _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, updateLobbyOptions);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
                
                NetworkManager.Singleton.StartHost();
                
                RelayJoinCodeChanged?.Invoke(RelayJoinCode);
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }
        }
        
        private async Task SubscribeToLobbyEvents(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            try
            {
                var callbacks = new LobbyEventCallbacks();
                callbacks.PlayerJoined += PlayerJoinedHandler;
                callbacks.PlayerLeft += PlayerLeftHandler;
                callbacks.LobbyDeleted += OnLobbyDeleted;

                _lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
                Debug.Log("Subscribed to lobby events");
            }
            catch (LobbyServiceException e)
            {
                switch (e.Reason)
                {
                    case LobbyExceptionReason.AlreadySubscribedToLobby:
                        Debug.LogWarning(
                            $"Already subscribed to lobby[{lobby.Id}]. We did not need to try and subscribe again. Exception Message: {e.Message}");
                        break;
                    case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
                        Debug.LogError(
                            $"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {e.Message}");
                        throw;
                    case LobbyExceptionReason.LobbyEventServiceConnectionError:
                        Debug.LogError($"Failed to connect to lobby events. Exception Message: {e.Message}");
                        throw;
                    default: throw;
                }
            }
        }
        
        private async Task UnsubscribeFromLobbyEvents()
        {
            if (_lobbyEvents == null) return;
            
            try
            {
                await _lobbyEvents.UnsubscribeAsync();
                Debug.Log("Unsubscribed from lobby events");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Unsubscribe failed: {e}");
            }
            finally
            {
                _lobbyEvents = null;
            }
        }

        public void DeleteLobby()
        {
            _ = DeleteLobbyTask();
        }

        public async Task DeleteLobbyTask()
        {
            if (_currentLobby == null) return;
            if (!IsPlayerHost) return;

            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                Debug.Log("Lobby deleted");
                _currentLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
            finally
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        public async Task QuickJoin()
        {
            if (_currentLobby != null) return;
            if (IsPlayerHost) return;

            try
            {
                _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(new QuickJoinLobbyOptions
                {
                    Player = _player
                });
                Debug.Log($"Quick joined lobby: {_currentLobby.Name},  {_currentLobby.HostId}");

                await JoinRelayFromLobby(_currentLobby);
                
                try
                {
                    await SubscribeToLobbyEvents(_currentLobby);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Client could not subscribe to lobby events, using polling instead: {e}");
                }
                
                UpdatePlayersList();
            }
            catch (LobbyServiceException e)
            {
                OnLobbyJoinFailed?.Invoke("Unexpected error.");
                if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                    Debug.LogWarning("No available lobbies found for quick join.");
                else
                    Debug.LogError($"Quick join failed: {e}");
            }
        }

        public void TryJoinWithCode(string relayJoinCode)
        {
            _ = TryJoinWithCodeTask(relayJoinCode);
        }
        
        public async Task TryJoinWithCodeTask(string relayJoinCode)
        {
            if (string.IsNullOrWhiteSpace(relayJoinCode))
            {
                Debug.LogWarning("No relay join code provided.");
                OnLobbyJoinFailed?.Invoke("No relay join code provided.");
                return;
            }
            
            if (_currentLobby != null)
            {
                Debug.LogWarning("Already in lobby.");
                OnLobbyJoinFailed?.Invoke("Already in lobby.");
                return;
            }

            try
            {
                Debug.Log("Trying to join with code: " + relayJoinCode);

                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
                {
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.S1, relayJoinCode, QueryFilter.OpOptions.EQ)
                    }
                });

                if (queryResponse.Results.Count > 0)
                {
                    _currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id, new JoinLobbyByIdOptions
                    {
                        Player = _player
                    });
                    Debug.Log("Joined lobby: " + _currentLobby.Name);
                    
                    await JoinRelayFromLobby(_currentLobby);
                    
                    UpdatePlayersList();
                    
                    try
                    {
                        await SubscribeToLobbyEvents(_currentLobby);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Could not subscribe to lobby events: {e}");
                    }
                    
                    OnLobbyJoinedSuccessfully?.Invoke();
                }
                else
                {
                    Debug.Log("No lobbies found.");
                    OnLobbyJoinFailed?.Invoke("Lobby not found");
                }
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                    Debug.LogWarning("No available lobbies found for quick join.");
                else
                    Debug.LogError($"Quick join failed: {e}");
            }
        }

        private async Task JoinRelayFromLobby(Unity.Services.Lobbies.Models.Lobby joinedLobby)
        {
            try
            {
                if (!joinedLobby.Data.ContainsKey(RelayCodeKey))
                {
                    Debug.LogWarning("Relay join code not found in lobby data!");
                    return;
                }
                
                string relayCode = joinedLobby.Data[RelayCodeKey].Value;
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

                NetworkManager.Singleton.StartClient();
                
                Debug.Log($"Client joined relay with code: {relayCode}");
                
                RelayJoinCodeChanged?.Invoke(RelayJoinCode);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void LeaveLobby()
        {
            _ = LeaveLobbyTask();
        }

        private async Task LeaveLobbyTask()
        {
            if (_currentLobby == null) return;

            try
            {
                string playerId = AuthenticationService.Instance.PlayerId;
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, playerId);
                _currentLobby = null;
                
                await UnsubscribeFromLobbyEvents();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
            finally
            {
                NetworkManager.Singleton.Shutdown();
                OnLobbyLeftSuccessfully?.Invoke();
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
            if (_currentLobby == null) return;
            if (!IsPlayerHost) return;
            if (!NetworkManager.Singleton.IsHost) return;
            if (_currentLobby.Players.Count != 2) return;
            
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }

        public void EndGame()
        {
            if (_currentLobby == null) return;
            
            if (!IsPlayerHost) LeaveLobby();
            if (IsPlayerHost) DeleteLobby();
            NetworkManager.Singleton.Shutdown();
        }
        
        private void OnDestroy()
        {
            _ = UnsubscribeFromLobbyEvents();
        }
    }
}
