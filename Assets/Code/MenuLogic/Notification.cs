using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class Notification : MonoBehaviour
{
    public Text titleText; // Текст заголовка уведомления
    public Text descriptionText; // Текст описания уведомления
    public Color errorColor; // Цвет уведомления об ошибке
    public Color infoColor; // Цвет уведомления с информацией
    public Color AcceptColor; // Цвет уведомления с информацией
    public Button closeButton; // Кнопка закрытия уведомления

    private Action onHideNotification; // Метод, вызываемый при скрытии уведомления

    // Метод инициализации уведомления
    public void Initialize(NotificationType type, string title, string description, Action onHide)
    {
        // Установка текста заголовка и описания уведомления
        titleText.text = title;
        descriptionText.text = description;

        // Установка цвета иконки и фона уведомления в зависимости от типа
        switch (type)
        {
            case NotificationType.Error:
                GetComponent<Image>().color = errorColor; 
                break;
            case NotificationType.Information:
                GetComponent<Image>().color = infoColor;
                break;
            case NotificationType.Accept:
                GetComponent<Image>().color = AcceptColor;
                break;
        }
        // Сохранение метода, который будет вызван при скрытии уведомления
        onHideNotification = onHide;

        // Подписка на событие нажатия на кнопку закрытия уведомления
        closeButton.onClick.AddListener(HideNotification);

        // Запуск таймера скрытия уведомления через 3 секунды
        Invoke("HideNotification", 3f);
    }

    // Метод скрытия уведомления
    public void HideNotification()
    {
        // Отписка от события нажатия на кнопку закрытия уведомления
        closeButton.onClick.RemoveAllListeners();

        // Удаление объекта уведомления из сцены
        Destroy(gameObject);

        // Вызов метода, который был сохранен во время инициализации уведомления
        onHideNotification?.Invoke();
        
    }
    // Метод получения ширины уведомления
    public float GetWidth()
    {
        return GetComponent<RectTransform>().rect.width;
    }

    public float GetHeight()
    {
        return GetComponent<RectTransform>().rect.height;
    }

    // Метод установки позиции уведомления
    public void SetPosition(Vector2 position)
    {
        GetComponent<RectTransform>().anchoredPosition = position;
    }
}
