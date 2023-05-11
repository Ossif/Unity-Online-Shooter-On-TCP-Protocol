using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatUI : MonoBehaviour
{
    public GameObject ChatMessage;
    private Transform Content;
    private Transform ChatField;
    private Client client;
    private const int MAX_CHAT_ARRAY = 20;
    private GameObject[] ChatArray = new GameObject[MAX_CHAT_ARRAY];

    public bool ChatIsOpen = false;
    private string NickName;
    private void Start()
    {
        client = FindObjectOfType<Client>().GetComponent<Client>();
        Content = this.transform.Find("Viewport").Find("Content");
        ChatField = this.transform.Find("ChatField");
        ChatField.gameObject.SetActive(false);
        NickName = PlayerPrefs.GetString("PlayerNick");

        for (int i = 0; i < MAX_CHAT_ARRAY; i++)
        {
            ChatArray[i] = null;
        }
    }

    public void AddChatMessage(string message)
    {
        for(int i = MAX_CHAT_ARRAY-1; i > 0; i--)
        {
            if (i == MAX_CHAT_ARRAY - 1 && ChatArray[i] != null)
            {
                Destroy(ChatArray[i]);
            }
            ChatArray[i] = ChatArray[i - 1];
        }
        ChatArray[0] = Instantiate(ChatMessage, Content);
        ChatArray[0].transform.GetComponent<TMP_Text>().text = message;
        return;
    }

    public void ClearChat()
    {
        for (int i = 0; i < MAX_CHAT_ARRAY; i++)
        {
            if (ChatArray[i] != null)
            {
                Destroy(ChatArray[i]);
                ChatArray[i] = null;
            }
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if (ChatIsOpen == false)
            {
                ChatIsOpen = true;
                ChatField.gameObject.SetActive(true);
                ChatField.GetComponent<TMP_InputField>().ActivateInputField();
            }
            else
            {
                if (ChatField.GetComponent<TMP_InputField>().text.Length != 0)
                {
                    Packet packet = new Packet((int)PacketHeaders.WorldCommand.CMSG_SEND_MESSAGE);
                    packet.Write(ChatField.GetComponent<TMP_InputField>().text);
                    client.Send(packet);
                    ChatField.GetComponent<TMP_InputField>().text = "";
                }
                ChatIsOpen = false;
                ChatField.GetComponent<TMP_InputField>().DeactivateInputField();
                ChatField.gameObject.SetActive(false);
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (ChatIsOpen == true)
            {
                ChatIsOpen = false;
                ChatField.GetComponent<TMP_InputField>().DeactivateInputField();
                ChatField.gameObject.SetActive(false);
            }
        }
    }
}
