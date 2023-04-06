using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLogic : MonoBehaviour
{
    public GameObject playerPrefab;

    private Client client = null;

    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<Client>().GetComponent<Client>();

        player = Instantiate(playerPrefab, new Vector3(0, 2, 0), Quaternion.identity);

        Packet packet = new Packet((int) PacketHeaders.WorldCommand.CMSG_PLAYER_LOGIN);
        packet.Write((float) gameObject.transform.position.x);
        packet.Write((float) gameObject.transform.position.y);
        packet.Write((float) gameObject.transform.position.z);

        packet.Write((float) gameObject.transform.rotation.z);

        client.Send(packet);
        //enemy = Instantiate(enemyPrefab, new Vector3(0, 2, 0), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
