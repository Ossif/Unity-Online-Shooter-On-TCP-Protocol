using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Node : MonoBehaviour
{
    public List<neighbour> neighbours = new List<neighbour>();
    public objects parentObject;
    public Vector3 position = new Vector3(0f, 0f, 0f);
    public void addNodeNeighbour(neighbour neigh)
    {
        neighbours.Add(neigh);
    }

    public void deleteNodeNeighbor(neighbour neigh)
    {
        neighbours.Remove(neigh);
    }
}
