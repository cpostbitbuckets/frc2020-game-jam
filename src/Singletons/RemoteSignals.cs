using Godot;

using System;

/// <summary>
/// RemoteSignals are used to communicate with the game when a remote action takes place, like a player
/// update or a new asteroid launch.
/// </summary>
public class RemoteSignals
{

    public delegate void SendPlayerUpdate(PlayerData player);
    public static event SendPlayerUpdate SendPlayerUpdateEvent;

    #region Event Publishers

    /// <summary>
    /// Publish this event when you want a player update to be sent to peers
    /// </summary>
    /// <param name="player"></param>
    public static void PublishSendPlayerUpdateEvent(PlayerData player)
    {
        SendPlayerUpdateEvent?.Invoke(player);
    }


    #endregion
}
