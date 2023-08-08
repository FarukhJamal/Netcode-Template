using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Managers
{
    public class CharacterSpawner: NetworkBehaviour
    {
        [SerializeField] private CharacterDatabase characterDatabase;
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            foreach (var client in HostManager.Instance.ClientData)
            {
                var character = characterDatabase.GetCharacterById(client.Value.characterId);
                if (character != null)
                {
                    var spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                    var characterInstance=Instantiate(character.GameplayPrefab,spawnPos,Quaternion.identity);
                    characterInstance.SpawnAsPlayerObject(client.Value.clientId);
                }
            }
        }
    }
}