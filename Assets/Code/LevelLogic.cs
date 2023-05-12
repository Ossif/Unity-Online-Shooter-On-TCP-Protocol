using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLogic : MonoBehaviour
{
    public GameObject playerPrefab;

    private Client client = null;

    private GameObject player;
    public GameObject[] pickupsPrefab;
    public List<GameObject> Pickups = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pickupsPrefab = Resources.LoadAll<GameObject>("Pickups");
        client = FindObjectOfType<Client>().GetComponent<Client>();

        player = Instantiate(playerPrefab, client.SpawnPos, Quaternion.identity);

        Packet packet = new Packet((int) PacketHeaders.WorldCommand.CMSG_PLAYER_LOGIN);
        packet.Write((float)player.transform.position.x);
        packet.Write((float)player.transform.position.y);
        packet.Write((float)player.transform.position.z);

        packet.Write((float)player.transform.rotation.z);

        client.Send(packet);
        //enemy = Instantiate(enemyPrefab, new Vector3(0, 2, 0), Quaternion.identity);
    }
    public void CreatePicup(int id, byte type, string ModelName, Vector3 pos)
    {
        foreach(GameObject go in pickupsPrefab)
        {
            if(go.name == ModelName)
            {
                GameObject pickup = Instantiate(go, pos, Quaternion.identity);
                pickups pic = pickup.AddComponent<pickups>();
                pic.id = id;
                pic.type = type;
                Pickups.Add(pickup);
            }
        }
    }
    public void CreatePicup(int id, byte type, string ModelName, Vector3 pos, Vector3 rot)
    {
        foreach(GameObject go in pickupsPrefab)
        {
            if(go.name == ModelName)
            {
                Debug.Log(rot);
                GameObject pickup = Instantiate(go, pos, Quaternion.Euler(rot));
                pickups pic = pickup.AddComponent<pickups>();
                //pickup.AddComponent<Rigidbody>().isKinematic = true;
                pic.id = id;
                pic.type = type;
                Pickups.Add(pickup);
            }
        }
    }

    public bool DestroyPickup(int pickupid)
    {
        foreach(GameObject pic in Pickups)
        {
            if (pic.transform.GetComponent<pickups>().id == pickupid)
            {
                Pickups.Remove(pic);
                Destroy(pic);
                return true;
            }
        }
        return false;
    }    
}
