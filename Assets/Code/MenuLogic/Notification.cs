using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class Notification : MonoBehaviour
{
    public Text titleText; // ����� ��������� �����������
    public Text descriptionText; // ����� �������� �����������
    public Color errorColor; // ���� ����������� �� ������
    public Color infoColor; // ���� ����������� � �����������
    public Color AcceptColor; // ���� ����������� � �����������
    public Button closeButton; // ������ �������� �����������

    private Action onHideNotification; // �����, ���������� ��� ������� �����������

    // ����� ������������� �����������
    public void Initialize(NotificationType type, string title, string description, Action onHide)
    {
        // ��������� ������ ��������� � �������� �����������
        titleText.text = title;
        descriptionText.text = description;

        // ��������� ����� ������ � ���� ����������� � ����������� �� ����
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
        // ���������� ������, ������� ����� ������ ��� ������� �����������
        onHideNotification = onHide;

        // �������� �� ������� ������� �� ������ �������� �����������
        closeButton.onClick.AddListener(HideNotification);

        // ������ ������� ������� ����������� ����� 3 �������
        Invoke("HideNotification", 3f);
    }

    // ����� ������� �����������
    public void HideNotification()
    {
        // ������� �� ������� ������� �� ������ �������� �����������
        closeButton.onClick.RemoveAllListeners();

        // �������� ������� ����������� �� �����
        Destroy(gameObject);

        // ����� ������, ������� ��� �������� �� ����� ������������� �����������
        onHideNotification?.Invoke();
        
    }
    // ����� ��������� ������ �����������
    public float GetWidth()
    {
        return GetComponent<RectTransform>().rect.width;
    }

    public float GetHeight()
    {
        return GetComponent<RectTransform>().rect.height;
    }

    // ����� ��������� ������� �����������
    public void SetPosition(Vector2 position)
    {
        GetComponent<RectTransform>().anchoredPosition = position;
    }
}
