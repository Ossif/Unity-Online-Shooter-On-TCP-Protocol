using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;


//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!НОВЫЙ ФАЙЛ НОВЫЙ ФАЙЛ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

public class MenuLogic : MonoBehaviour
{

    public GameObject serverPrefab;
    public GameObject clientPrefab;

    public TMP_InputField IpInput;
    public TMP_InputField nameInput;
    public GameObject mainMenu;
    public GameObject waitingMenu;
    public GameObject FindMenu;
    public GameObject AddServerMenu;
    public GameObject Canvas;
    public NotifyLogic NotifyLogic;


    public bool windowed = true;

    private void Start()
    {
        //if(windowed) Screen.fullScreen = !Screen.fullScreen; 
        waitingMenu.SetActive(false);
        mainMenu.SetActive(true);
        FindMenu.SetActive(false);
        AddServerMenu.SetActive(false);
    }

    public void OpenFindMenu()
    {   
        if(!SettingsLogic.CheckNickNameToAvaible())
        {
            NotifyLogic.CreateNotify(NotificationType.Error, "Некорректный ник-нейм", "Вы не можете приступить к поиску игры пока не установите корректный ник-нейм");
            return;
        }
        FindMenu.SetActive(true); //Открываем меню
        FindMenu.GetComponent<FavoriteServersManager>().LoadFavoriteServers(); //Загружаем все сервера
    }
    public void ConnectedToServer(FavoriteServer server)
    {
        try
        {
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.ClientName = nameInput.text;
            if (c.ClientName == "") c.ClientName = "Client";
            c.IsHost = false;
            c.ConnectToServer(server.IP, server.Port);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
    public void CloseFindMenu()
    {
        if (FindMenu.activeInHierarchy == true)
            FindMenu.SetActive(false);
        return;
    }
    public void OpenAddServerMenu()
    {
        AddServerMenu.SetActive(true);
        return;
    }
    public void CloseAddServerMenu()
    {
        if (AddServerMenu.activeInHierarchy == true)
            AddServerMenu.SetActive(false);
        return;
    }
    public void HostButton()
    {
        if (!SettingsLogic.CheckNickNameToAvaible())
        {
            NotifyLogic.CreateNotify(NotificationType.Error, "Некорректный ник-нейм", "Вы не можете приступить к созданию игры пока не установите корректный ник-нейм");
            return;
        }
        try
        {
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.Init();

            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.ClientName = nameInput.text;
            if (c.ClientName == "") c.ClientName = "Host";
            c.IsHost = true;
            c.ConnectToServer("127.0.0.1", 6321);
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
        if (s != null) Destroy(s.gameObject);

        Client c = FindObjectOfType<Client>();
        if (c != null) Destroy(c.gameObject);

        waitingMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
  
    public void ExitButton()
    {
        Application.Quit();
    }
    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}