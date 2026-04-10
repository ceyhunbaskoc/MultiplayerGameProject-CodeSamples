using System;
using Unity.Netcode;
using Managers.UI;
using Managers.Inspection;
using Managers.CoreLogic;
using Managers.Newspaper;
using UnityEngine;

namespace Managers.Network
{
    public static class CoopEvents
    {
        public const string DangerButtonPressed = "DangerButtonPressed";
        public const string BombButtonPressed   = "BombButtonPressed";
        public const string BombExplode         = "BombExplode";
        public const string DangerEnded         = "DangerEnded";
        public const string AllPlayersReady     = "AllPlayersReady";
        public const string DayEnded            = "DayEnded";
    }
    public class CoopSyncManager : NetworkBehaviour
{
    public static CoopSyncManager Instance { get; private set; }

    public static Action<string> OnBroadcastReceived;

    public NetworkVariable<int> SyncedDayIndex = new(1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    public NetworkVariable<float> SyncedBombExplosionDuration = new(10f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    public NetworkVariable<bool> IsBombActive = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<double> SyncedBombEndTime = new(0.0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsEntryDoorOpen = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsExitDoorOpen = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private int _readyPlayersCount = 0;

    public override void OnNetworkSpawn()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        NewspaperUI.OnNewspaperClosed  += HandleLocalPlayerReady;
        EndDayUI.OnEndDayButtonPressed += HandleLocalPlayerReady;
    }

    public override void OnNetworkDespawn()
    {
        NewspaperUI.OnNewspaperClosed  -= HandleLocalPlayerReady;
        EndDayUI.OnEndDayButtonPressed -= HandleLocalPlayerReady;
    }

    // ← tek broadcast metodu, her yerden çağrılır
    public void Broadcast(string eventName)
    {
        if (IsSpawned) _broadcastServerRpc(eventName);
        else OnBroadcastReceived?.Invoke(eventName);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _broadcastServerRpc(string eventName)
    {
        _broadcastClientRpc(eventName);
    }

    [ClientRpc]
    private void _broadcastClientRpc(string eventName)
    {
        OnBroadcastReceived?.Invoke(eventName);
    }

    #region Ready Players

    public void HandleLocalPlayerReady()
    {
        if (IsSpawned) _playerReadyServerRpc();
        else OnBroadcastReceived?.Invoke(CoopEvents.AllPlayersReady);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _playerReadyServerRpc()
    {
        _readyPlayersCount++;
        int totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;

        if (_readyPlayersCount >= totalPlayers)
        {
            _readyPlayersCount = 0;
            _broadcastClientRpc(CoopEvents.AllPlayersReady);
        }
    }

    #endregion

    #region Newspaper

    public void BroadcastStartDay()
    {
        GameState.ChangeState(AppState.Newspaper);
        if (!IsServer) return;
        _showNewspaperClientRpc(SyncedDayIndex.Value);
    }

    [ClientRpc]
    private void _showNewspaperClientRpc(int dayIndex)
    {
        string news = NewspaperGenerator.GenerateTodaysNews(dayIndex);
        FindFirstObjectByType<NewspaperUI>(FindObjectsInactive.Include)
            ?.ShowNewspaper(dayIndex, news);
    }

    #endregion

    #region EndDay

    public void BroadcastEndDay()
    {
        if (!IsServer) return;
        _endDayClientRpc(SyncedDayIndex.Value);
    }

    [ClientRpc]
    private void _endDayClientRpc(int dayIndex)
    {
        OnBroadcastReceived?.Invoke(CoopEvents.DayEnded);
        FindFirstObjectByType<EndDayUI>(FindObjectsInactive.Include)
            ?.ShowEndDayScreen(dayIndex);
    }

    #endregion
}
}