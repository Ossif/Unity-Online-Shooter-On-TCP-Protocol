using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuLogic : MonoBehaviour
{
    public GameObject PauseMenu; // Основной объект меню паузы
    
    // Статическое поле для отслеживания состояния паузы (доступно из других скриптов)
    public static bool IsGamePaused = false;
    
    // Ссылки на скрипты игрока для блокировки управления
    private Movement playerMovement;


    void Start()
    {
        // Деактивируем меню паузы при старте игры
        if (PauseMenu != null)
        {
            PauseMenu.SetActive(false);
        }
        
        // Находим скрипты управления игроком
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<Movement>();
        }
        
        IsGamePaused = false;
    }

    void Update()
    {
        // Проверяем нажатие клавиши ESC для переключения меню паузы
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (PauseMenu != null)
        {
            // Переключаем состояние активности меню
            bool isActive = PauseMenu.activeSelf;
            PauseMenu.SetActive(!isActive);
            
            // Управляем временем игры и состоянием паузы
            if (!isActive)
            {
                // Активируем меню - ставим игру на паузу
                PauseGame();
            }
            else
            {
                // Деактивируем меню - возобновляем игру
                ResumeGame();
            }
        }
    }
    
    void PauseGame()
    {
        IsGamePaused = true;

        // Разблокируем курсор для взаимодействия с меню
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Блокируем управление игроком
        if (playerMovement != null)
            playerMovement.EnabledMovement = false;
    }
    
    void ResumeGame()
    {
        IsGamePaused = false;

        // Блокируем курсор для игры
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Возобновляем управление игроком
        if (playerMovement != null)
            playerMovement.EnabledMovement = true;
    }
    public void ExitToMenu()
    {
        // Находим скрипты управления игроком
        GameObject Client = GameObject.FindWithTag("Client");
        if (Client != null)
        {
            Client clientScript = Client.GetComponent<Client>();
            clientScript.CloseSocket();
            Destroy(Client);
        }

        GameObject Server = GameObject.FindWithTag("Server");
        if (Server != null)
        {
            Destroy(Server);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}
