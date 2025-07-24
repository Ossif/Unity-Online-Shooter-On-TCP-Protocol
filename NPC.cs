using UnityEngine;


public class NPC
{
    public int NPCid;
    public string NPCname;
    public Vector3 SpawnPos;
    public float SpawnRot;
    public Vector3 Position;
    public static int NPCIDcounter = 0;

    public NPC(string Name, Vector3 SpawnPos)
    {
        this.NPCname = Name;
        this.NPCid = NPCIDcounter;
        this.SpawnPos = SpawnPos;
        this.Position = SpawnPos;
        NPCIDcounter++;
    }
}

