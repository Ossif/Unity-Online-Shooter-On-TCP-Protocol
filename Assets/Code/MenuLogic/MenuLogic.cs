using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

using UnityEngine.UI;
using TMPro;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!НОВЫЙ ФАЙЛ НОВЫЙ ФАЙЛ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


public class MenuLogic : MonoBehaviour
{

    public GameObject serverPrefab;
    public GameObject clientPrefab;

    public TMP_InputField IpInput;
    public TMP_InputField nameInput;
    public GameObject mainMenu;
    public GameObject waitingMenu;

    public bool windowed = true;

    private void Start()
    {
        if(windowed) Screen.fullScreen = !Screen.fullScreen; 
        waitingMenu.SetActive(false);
        mainMenu.SetActive(true);
    }


    public void FindButton(){
        string IpString = IpInput.text.ToString();

        if(IpString == "")
            IpString = "127.0.0.1";

        try
        {
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.ClientName = nameInput.text;
            if(c.ClientName == "") c.ClientName = "Client";
            c.IsHost = false;
            c.ConnectToServer(IpString, 6321);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void HostButton()
    {
        try
        {
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.Init();

            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.ClientName = nameInput.text;
            if(c.ClientName == "") c.ClientName = "Host";
            c.IsHost = true;
            //c.ConnectToServer("127.0.0.1", 6321);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        mainMenu.SetActive(false);
        waitingMenu.SetActive(true);
    }
    public void BackButton()
    {
        Server s = FindObjectOfType<Server>();
        if(s != null) Destroy(s.gameObject);

        Client c = FindObjectOfType<Client>();
        if(c != null) Destroy(c.gameObject);

        waitingMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
