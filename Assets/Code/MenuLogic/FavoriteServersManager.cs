using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;


    public class FavoriteServersManager : MonoBehaviour
    {
        public Transform AddServer;
        public GameObject FavoriteServerObject;
        public Transform ContentView;
        public MenuLogic MenuLogic;

        private FS favoriteServers = new FS();
        private string saveKey = "FavoriteServers";
        private InputField IPstring = null;
        private FavoriteServer selectedServer = null;

        public void Start()
        {
            IPstring = AddServer.Find("AddField").GetComponent<InputField>();
            //ContentView = this.transform.Find("ServerView").Find("Viewport").Find("Content");
        }
        public void AddFavoriteServer()
        {
            string serverAdress = IPstring.text;

            string[] parts = serverAdress.Split(':');
            if (parts.Length != 2)
            {
                Debug.LogError("Invalid server address format: " + serverAdress);
                return;
            }

            string ip = parts[0];
            int port;
            if (!int.TryParse(parts[1], out port))
            {
                Debug.LogError("Invalid server address format: " + serverAdress);
                return;
            }
            IPstring.text = ""; //Очищаем строку ввода

            FavoriteServer serv = new FavoriteServer(ip, port);

            if (favoriteServers.Servers.Count > 0)
            {
                // Check if the server already exists in the list
                foreach (FavoriteServer server in favoriteServers.Servers)
                {
                    if (server.IP == ip && server.Port == port)
                    {
                        Debug.LogWarning("Server already exists in favorites: " + serverAdress);
                        return;
                    }
                }
            }
            favoriteServers.Servers.Add(serv);

            Debug.Log($"Сервер успешно добавлен {favoriteServers.Servers.Count}");
            SaveFavoriteServers(); //Сохраняем список
            LoadFavoriteServers(); //Обновляем список
        }
        public void RemoveFavoriteServer()
        {
            if (selectedServer == null)
                return;
            Destroy(selectedServer.block);
            favoriteServers.Servers.Remove(selectedServer);

            SaveFavoriteServers();
        }
        public void SaveFavoriteServers()
        {
            PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(favoriteServers));
            Debug.Log($"Список успешно сохранён {JsonUtility.ToJson(favoriteServers)}");
        }

        public void LoadFavoriteServers()
        {
            if (favoriteServers.Servers.Count > 0)
            {
                foreach (FavoriteServer serv in favoriteServers.Servers)
                {
                    if (serv.block != null)
                    {
                        Destroy(serv.block);
                    }
                }
                favoriteServers.Servers.Clear();
                selectedServer = null;
            }
            Debug.Log("Попытка загрузить список серверов");
            if (PlayerPrefs.HasKey(saveKey))
            {
                string json = PlayerPrefs.GetString(saveKey);
                favoriteServers = JsonUtility.FromJson<FS>(json);
                Debug.Log($"Список успешно загружен {favoriteServers.Servers.Count}\n{json}");
                foreach (FavoriteServer serv in favoriteServers.Servers)
                {
                    Debug.Log($"{serv.IP}:{serv.Port}");
                    GameObject block = Instantiate(FavoriteServerObject);
                    block.transform.SetParent(ContentView);
                    block.transform.Find("NameText").GetComponent<Text>().text = $"{serv.IP}:{serv.Port}";
                    serv.block = block;
                    block.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        SelectFavorite(serv, block);
                    });
                }
            }

        }

        public void SelectFavorite(FavoriteServer serv, GameObject Block)
        {
            Debug.Log($"Выбран сервер: {serv.IP}:{serv.Port}");
            foreach (FavoriteServer server in favoriteServers.Servers)
            {
                if (Equals(server, serv))
                {
                    server.block.transform.GetComponent<Image>().color = new Color32(113, 107, 99, 255);
                    selectedServer = server;
                    Debug.Log("Тот самый объект");
                }
                else
                    server.block.transform.GetComponent<Image>().color = new Color32(65, 58, 54, 255);
            }
        }

        public void Connect()
        {
            if (selectedServer == null)
                return;
            MenuLogic.ConnectedToServer(selectedServer);
        }
    }
