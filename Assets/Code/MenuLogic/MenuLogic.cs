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
    public GameObject Settings;
    public string NickName;

    public bool windowed = true;

    //notifications
    public GameObject notificationPrefab;
    public int maxNotifications = 3;
    public int currentNotifications = 0;

    private void Start()
    {
        //if(windowed) Screen.fullScreen = !Screen.fullScreen; 
        waitingMenu.SetActive(false);
        mainMenu.SetActive(true);
        FindMenu.SetActive(false);
        AddServerMenu.SetActive(false);
        Settings.SetActive(false);


        //Если есть ник-нейм то мы его загружаем
        if (PlayerPrefs.HasKey("PlayerNick"))
        {
            NickName = PlayerPrefs.GetString("PlayerNick");
        }
        else NickName = null;
    }

    public void OpenFindMenu()
    {
        if(!CheckNickNameToAvaible())
        {
            CreateNotify(NotificationType.Error, "Некорректный ник-нейм", "Вы не можете приступить к поиску игры пока не установите корректный ник-нейм");
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
        if (!CheckNickNameToAvaible())
        {
            CreateNotify(NotificationType.Error, "Некорректный ник-нейм", "Вы не можете приступить к созданию игры пока не установите корректный ник-нейм");
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
    public void OpenSettingMenu()
    {
        InputField Nick = Settings.transform.Find("NickField").GetComponent<InputField>();
        if(NickName != null)
        {
            Nick.text = NickName;
        }
        Settings.SetActive(true);
        return;
    }

    public void CloseSettingsMenu()
    {
        Settings.SetActive(false);
        return;
    }
    public void AcceptPlayerNick()
    {
        InputField Nick = Settings.transform.Find("NickField").GetComponent<InputField>();
        if(Nick.text.Length < 4)
        {
            CreateNotify(NotificationType.Error, "Ошибка ввода", "Ваш ник должен содержать как минимум 4 символа");
            return;
        }
        if(Nick.text.Length > 20)
        {
            CreateNotify(NotificationType.Error, "Ошибка ввода", "Ваш ник не должен быть длиннее 20 символов");
        }
        foreach (char c in Nick.text)
        {
            if (!Char.IsLetter(c) && c != '-' && c != '_')
            {
                CreateNotify(NotificationType.Error, "Ошибка ввода", "Ваш ник-нейм должен содержать только символы алфавита.");
                return;
            }
        }
        CreateNotify(NotificationType.Accept, "Ник-нейм", $"Теперь ваш ник-нейм - {Nick.text}.");
        NickName = Nick.text;
        PlayerPrefs.SetString("PlayerNick", NickName); //Сохраняем ник
        return;
    }
    public void ExitButton()
    {
        Application.Quit();
    }
    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public bool CheckNickNameToAvaible()
    {
        if (NickName == null) return false;
        if (NickName.Length < 4) return false;
        if (NickName.Length > 20) return false;
        foreach (char c in NickName)
            if (!Char.IsLetter(c) && c != '-' && c != '_')
                return false;

        return true;
    }
    public void CreateNotify(NotificationType type, string title, string description)
    {
        if (currentNotifications >= maxNotifications)
        {
            return;
        }

        GameObject notificationObject = Instantiate(notificationPrefab, Canvas.transform);
        Notification notification = notificationObject.GetComponent<Notification>();

        notification.Initialize(type, title, description, HideNotification);


        float x = Screen.width - notification.GetWidth() - 10f;
        float y = Screen.height - ((currentNotifications + 1) * notification.GetHeight()) - 10f;
        notification.SetPosition(new Vector2(x, y));

        currentNotifications++;

        //Invoke(nameof(HideNotification), 3f);
    }

    private void HideNotification()
    {
        currentNotifications--;

        if (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
    }
}