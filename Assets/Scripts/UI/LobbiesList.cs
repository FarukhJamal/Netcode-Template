using System;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class LobbiesList : MonoBehaviour
    {
        [SerializeField] private Transform lobbyItemParent;
        [SerializeField] private LobbyItem lobbyItemPrefab;
        [SerializeField] private int totalLobbyCount=25;
        
        private bool isRefreshing;
        private bool isJoining;
        private void OnEnable()
        {
            RefreshList();
        }

        public async void RefreshList()
        {
            if(isRefreshing) return;
            
            isRefreshing = true;

            try
            {
                var options = new QueryLobbiesOptions();
                options.Count = totalLobbyCount;

                options.Filters = new List<QueryFilter>()
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"
                    ),
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.IsLocked,
                        op: QueryFilter.OpOptions.EQ,
                        value: "0"
                    )
                };
                var lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
                PopulateLobbiesUI(lobbies);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                isRefreshing = false;
                throw;
            }
            isRefreshing = false;
        }

        public async void JoinAsync(Lobby lobby)
        {
            if (isJoining) return;

            isJoining = true;

            try
            {
                var joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
                string joinCode = joiningLobby.Data["JoinCode"].Value;

                await ClientManager.Instance.StartClient(joinCode);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                isJoining = false;
                throw;
            }
            
            isJoining = false;
        }

        void PopulateLobbiesUI(QueryResponse lobbies)
        {
            foreach (Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach (Lobby lobby in lobbies.Results)
            {
                var lobbyInstance = Instantiate(lobbyItemPrefab, lobbyItemParent);
                if (lobbyInstance != null)
                {
                    LobbyItem lobbyItem = lobbyInstance.GetComponent<LobbyItem>();
                    
                    if(lobbyItem!=null)
                        lobbyItem.Initialize(this, lobby);
                }
            }
        }
    }
}
