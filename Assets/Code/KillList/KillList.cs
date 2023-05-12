using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillList : MonoBehaviour
{
    public GameObject KillMessage;
    private Transform Content;
    private const int MAX_KILL_ARRAY = 20;
    private GameObject[] KillArray = new GameObject[MAX_KILL_ARRAY];
    public Sprite[] sprites;

    private void Start()
    {
        Content = this.transform.Find("Viewport").Find("Content");
        sprites = Resources.LoadAll<Sprite>("Icons");

        for (int i = 0; i < MAX_KILL_ARRAY; i++)
        {
            KillArray[i] = null;
        }
    }

    public void AddKillMessage(string KillerName, string DeathName, byte IconID)
    {
        for (int i = MAX_KILL_ARRAY - 1; i > 0; i--)
        {
            if (i == MAX_KILL_ARRAY - 1 && KillArray[i] != null)
            {
                Destroy(KillArray[i]);
            }
            KillArray[i] = KillArray[i - 1];
        }
        KillArray[0] = Instantiate(KillMessage, Content);
        KillArray[0].transform.Find("KillerName").GetComponent<TMP_Text>().text = KillerName;
        KillArray[0].transform.Find("DeathName").GetComponent<TMP_Text>().text = DeathName;
        KillArray[0].transform.Find("Panel").Find("Image").GetComponent<Image>().sprite = sprites[IconID];
        return;
    }

    public void ClearChat()
    {
        for (int i = 0; i < MAX_KILL_ARRAY; i++)
        {
            if (KillArray[i] != null)
            {
                Destroy(KillArray[i]);
                KillArray[i] = null;
            }
        }
    }
}
