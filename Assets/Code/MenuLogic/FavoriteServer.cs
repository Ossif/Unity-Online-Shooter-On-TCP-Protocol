using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FavoriteServer
{
    [SerializeField] private string ip;
    [SerializeField] private int port;
    [SerializeField] private string name;
    [SerializeField] private int numPlayers;
    [SerializeField] private string gameMode;
    public GameObject block;

    public string IP { get { return ip; } set { ip = value; } }
    public int Port { get { return port; } set { port = value; } }
    public string Name { get { return name; } set { name = value; } }
    public int NumPlayers { get { return numPlayers; } set { numPlayers = value; } }
    public string GameMode { get { return gameMode; } set { gameMode = value; } }

    //public GameObject Menublock;
    public FavoriteServer(string IP, int Port)
    {
        this.IP = IP;
        this.Port = Port;

        this.Name = "DIA dedicated server";
        this.NumPlayers = 0;
        this.GameMode = "TDM";
        this.block = null;
        //this.Menublock = null;
    }
}

[System.Serializable]
public class FS
{
    [SerializeField] private List<FavoriteServer>serv;
    public List<FavoriteServer> Servers { get { return serv; } set { serv = value; } }
    public FS()
    {
        serv = new List<FavoriteServer>();
    }
    public void addToArray(FavoriteServer serv)
    {
        this.serv.Add(serv);
    }
}
