using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class SettingsLogic : MonoBehaviour
{
    public GameObject Settings;
    public NotifyLogic NotifyLogic;
    public string NickName;
    public float currentSensitivity = 9.0f;

    void Start()
    {
        Settings.SetActive(false);
        //���� ���� ���-���� �� �� ��� ���������
        if (PlayerPrefs.HasKey("PlayerNick"))
        {
            NickName = PlayerPrefs.GetString("PlayerNick");
        }
        else NickName = null;

        if (PlayerPrefs.HasKey("Sens"))
        {
            currentSensitivity = PlayerPrefs.GetFloat("Sens");
        }
        else
        {
            currentSensitivity = 9.0f; // �������� �� ���������
            PlayerPrefs.SetFloat("Sens", currentSensitivity);
            PlayerPrefs.Save();
        }

        Button AcceptNickButton = Settings.transform.Find("AcceptNick").GetComponent<Button>();
        AcceptNickButton.onClick.RemoveAllListeners();
        AcceptNickButton.onClick.AddListener(AcceptPlayerNick);

        Button ExitButton = Settings.transform.Find("ExitButton").GetComponent<Button>();
        ExitButton.onClick.RemoveAllListeners();
        ExitButton.onClick.AddListener(CloseSettingsMenu);
    }

    // ���������� �������� ����������������
    private void SaveSensitivitySettings()
    {
        PlayerPrefs.SetFloat("Sens", currentSensitivity);
        PlayerPrefs.Save();
    }

    // ���������� ��������� ���������������� ����� UI
    public void OnSensitivityChanged(float value)
    {
        // ����������� �������� �������� (0-1) � ���������������� (1-20)
        float newSensitivity = value * 20f;

        currentSensitivity = newSensitivity + 1;
        SaveSensitivitySettings();
    }

    public void OpenSettingMenu()
    {
        Settings.SetActive(true);
        InputField Nick = Settings.transform.Find("NickField").GetComponent<InputField>();
        if (NickName != null)
        {
            Nick.text = NickName;
        }

        Scrollbar SensivityScrollbar = Settings.transform.Find("SensivityScrollbar").GetComponent<Scrollbar>();
        if (SensivityScrollbar != null)
        {
            // ������������� �������� �������� (0-1)
            SensivityScrollbar.value = (currentSensitivity - 1) / 20f;
            // ���������� ���������� �������
            SensivityScrollbar.onValueChanged.RemoveAllListeners();
            SensivityScrollbar.onValueChanged.AddListener(OnSensitivityChanged);
        }
        return;
    }

    public void CloseSettingsMenu()
    {
        // ��������� ��������� ��� �������� ����
        SaveSensitivitySettings();
        Settings.SetActive(false);
        return;
    }
    public void AcceptPlayerNick()
    {
        InputField Nick = Settings.transform.Find("NickField").GetComponent<InputField>();
        if (Nick.text.Length < 4)
        {
            NotifyLogic.CreateNotify(NotificationType.Error, "������ �����", "��� ��� ������ ��������� ��� ������� 4 �������");
            return;
        }
        if (Nick.text.Length > 20)
        {
            NotifyLogic.CreateNotify(NotificationType.Error, "������ �����", "��� ��� �� ������ ���� ������� 20 ��������");
        }
        foreach (char c in Nick.text)
        {
            if (!Char.IsLetter(c) && c != '-' && c != '_')
            {
                NotifyLogic.CreateNotify(NotificationType.Error, "������ �����", "��� ���-���� ������ ��������� ������ ������� ��������.");
                return;
            }
        }
        NotifyLogic.CreateNotify(NotificationType.Accept, "���-����", $"������ ��� ���-���� - {Nick.text}.");
        NickName = Nick.text;
        PlayerPrefs.SetString("PlayerNick", NickName); //��������� ���
        return;
    }

    public static bool CheckNickNameToAvaible()
    {
        string? NickName = null;
        if (PlayerPrefs.HasKey("PlayerNick"))
        {
            NickName = PlayerPrefs.GetString("PlayerNick");
        }
        if (NickName == null) return false;
        if (NickName.Length < 4) return false;
        if (NickName.Length > 20) return false;
        foreach (char c in NickName)
            if (!Char.IsLetter(c) && c != '-' && c != '_')
                return false;

        return true;
    }
}
