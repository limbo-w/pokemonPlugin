using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerConfig
{
    public string respawnSceneName;
    public int playerDirection;
    public Vector3 respawnScenePosition;
    public Vector3 lastPokemonCenterPosition;
    public long[] registeredItems = new long[4];

    public PlayerConfig setRegisteredItems(long[] registeredItems)
    {
        this.registeredItems = registeredItems;
        return this;
    }

    public long[] getRegisteredItems()
    {
        return registeredItems;
    }

    public PlayerConfig setLastPokemonCenterPosition(Vector3 lastPokemonCenterPosition)
    {
        this.lastPokemonCenterPosition = lastPokemonCenterPosition;
        return this;
    }

    public Vector3 getLastPokemonCenterPosition()
    {
        return lastPokemonCenterPosition;
    }

    public PlayerConfig setRespawnSceneName(string respawnSceneName)
    {
        this.respawnSceneName = respawnSceneName;
        return this;
    }

    public string getRespawnSceneName()
    {
        return respawnSceneName;
    }

    public PlayerConfig setPlayerDirection(int playerDirection)
    {
        this.playerDirection = playerDirection;
        return this;
    }

    public int getPlayerDirection()
    {
        return playerDirection;
    }

    public PlayerConfig setRespawnScenePosition(Vector3 respawnScenePosition)
    {
        this.respawnScenePosition = respawnScenePosition;
        return this;
    }

    public Vector3 getRespawnScenePosition()
    {
        return respawnScenePosition;
    }
}
