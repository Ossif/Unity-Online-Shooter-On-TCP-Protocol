using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCommands
{
    public List<ServerClient> CommandPlayers;
    public Vector3 spawnPoint;
    public ServerCommands(Vector3 spawnInfo)
    {
        CommandPlayers = new List<ServerClient>();
        spawnPoint = spawnInfo;
    }
}
