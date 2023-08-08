using JetBrains.Annotations;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace UI
{
    public class LobbyItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyNamText;
        [SerializeField] private TMP_Text lobbyStatusText;

        private LobbiesList lobbiesList;
        private Lobby lobby;

        public void Initialize(LobbiesList lobbiesList, Lobby lobby)
        {
            this.lobbiesList = lobbiesList;
            this.lobby = lobby;

            lobbyNamText.text = lobby.Name;
            lobbyStatusText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        }

        public void Join()
        {
            lobbiesList.JoinAsync(lobby);
        }
    }
}
