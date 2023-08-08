using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class HostManager : MonoBehaviour
    {
        public static HostManager Instance { get; private set; }

        private bool gameHasStarted;
        # region Scene-Names
        [SerializeField] private string gameplaySceneName="Gameplay";
        [SerializeField] private string characterSelectionSceneName="CharacterSelection";
        #endregion
        private const int MaxPlayers = 4;
        
        public Dictionary<ulong,ClientData> ClientData { get; private set; }
        public string JoinCode { get; private set; }
        private string lobbyId;
      [SerializeField]  private float heartbeatLobbyPingTime=15f;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void StartServer()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
            ClientData = new Dictionary<ulong, ClientData>();
            NetworkManager.Singleton.StartServer();
        }
        public async void StartHost()
        {
            // setup relay service for hosting
            Allocation allocation;
            try
            {
                //First we will call relay service to create a connection for us
                allocation= await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay create allocation request failed{e.Message}");
                throw;
            }

            Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
            Debug.Log($"server: {allocation.AllocationId}");
            
            try
            {
                JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            }
            catch 
            {
                Debug.LogError("Relay get join code request failed");
                throw;
            }
            
            // now make the relay server data with our allocation and set it in Network Manager
            var relayServerData = new RelayServerData(allocation, "dtls"); // 'dtls' stand for Datagram Transport Layer Security
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            
            // make unity lobby after connecting to relay service;
            try
            {
                var createLobbyOptions = new CreateLobbyOptions();
                createLobbyOptions.IsPrivate = false;
                createLobbyOptions.Data = new Dictionary<string, DataObject>()
                {
                    {
                        "JoinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Member,
                            value: JoinCode
                        )
                    }
                };
                Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("My Lobby", MaxPlayers, createLobbyOptions);
                lobbyId = lobby.Id;
                StartCoroutine(nameof(HeartbeatLobbyCoroutine),heartbeatLobbyPingTime);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                throw;
            }

            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
            
            ClientData = new Dictionary<ulong, ClientData>();
            
            NetworkManager.Singleton.StartHost();
        }
        private IEnumerator HeartbeatLobbyCoroutine(float waitTimeSeconds)
        {
            var delay = new WaitForSeconds(waitTimeSeconds);
            while (true)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return delay;
            }
        }
        public void SetCharacter(ulong clientId, int characterId)
        {
            if (ClientData.TryGetValue(clientId, out ClientData data))
            {
                data.characterId = characterId;
            }
        }

        public void StartGame()
        {
            gameHasStarted = true;
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }

        #region Callbacks

        
        private void OnNetworkReady()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.LoadScene(characterSelectionSceneName,LoadSceneMode.Single);
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (ClientData.ContainsKey(clientId))
            {
                if (ClientData.Remove(clientId))
                {
                    Debug.Log($"Removed client {clientId}");
                }
            }
        }
        
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (ClientData.Count >= MaxPlayers || gameHasStarted)
            {
                response.Approved = false;
                return;
            }

            response.Approved = true;
            response.CreatePlayerObject = false;
            response.Pending = false;

            ClientData[request.ClientNetworkId] = new ClientData(request.ClientNetworkId);
            Debug.Log($"Added client {request.ClientNetworkId}");
        }


        #endregion
    }
}
