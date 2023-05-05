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
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        client = FindObjectOfType<Client>().GetComponent<Client>();

        player = Instantiate(playerPrefab, new Vector3(0, 2, 0), Quaternion.identity);

        Packet packet = new Packet((int) PacketHeaders.WorldCommand.CMSG_PLAYER_LOGIN);
        packet.Write((float)player.transform.position.x);
        packet.Write((float)player.transform.position.y);
        packet.Write((float)player.transform.position.z);

        packet.Write((float)player.transform.rotation.z);

        client.Send(packet);
        //enemy = Instantiate(enemyPrefab, new Vector3(0, 2, 0), Quaternion.identity);
    }
}
