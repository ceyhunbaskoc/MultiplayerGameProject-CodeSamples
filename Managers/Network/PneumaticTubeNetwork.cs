using System;
using Unity.Netcode;
using UnityEngine;

namespace Managers.Network
{
    public class PneumaticTubeNetwork : NetworkBehaviour
    {
        public static PneumaticTubeNetwork Instance { get; private set; }

        public static event Action<string, PlayerRole> OnMessageReceived;

        public override void OnNetworkSpawn()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("Dont destroy çalıştı!");
        }

        public void SendMessage(string message, PlayerRole senderRole)
        {
            SendMessageServerRpc(message, senderRole);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendMessageServerRpc(string message, PlayerRole senderRole)
        {
            // karşı tarafa ilet
            PlayerRole receiverRole = senderRole == PlayerRole.Reception
                ? PlayerRole.Archive
                : PlayerRole.Reception;

            DeliverMessageClientRpc(message, receiverRole);
        }

        [ClientRpc]
        private void DeliverMessageClientRpc(string message, PlayerRole targetRole)
        {
            if (PlayerRoleManager.LocalRole != targetRole) return;

            OnMessageReceived?.Invoke(message, targetRole);
        }
    }
}