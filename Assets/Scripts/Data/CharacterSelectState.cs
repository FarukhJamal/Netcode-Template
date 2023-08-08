using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct CharacterSelectState : INetworkSerializable,IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public bool IsLockedIn;

    public CharacterSelectState(ulong _clientId, int _characterId=-1,bool _isLockedIn=false)
    {
        ClientId = _clientId;
        CharacterId = _characterId;
        IsLockedIn = _isLockedIn;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref IsLockedIn);
    }

    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId && CharacterId == other.CharacterId && IsLockedIn==other.IsLockedIn;
    }
}
