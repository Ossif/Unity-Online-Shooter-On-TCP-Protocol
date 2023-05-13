using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CanvasLogic : MonoBehaviour
{

    public TextMeshProUGUI  ammoLeft;
    public TextMeshProUGUI  ammoRight;
    public TextMeshProUGUI  ammoTotal;

    public TextMeshProUGUI  health;

    public void SetAmmoLeft(int var){
        ammoLeft.text = var.ToString();
    }
    
    public void SetAmmoRight(int var){
        ammoRight.text = var.ToString();
    }

    public void SetAmmoTotal(int var){
        ammoTotal.text = var.ToString();
    }

    public void SetHealth(int var){
        health.text = var.ToString();
    }
    public void HideHUD(){
        transform.Find("Ammo").Find("AmmoLeft").gameObject.SetActive(false);
        transform.Find("Ammo").Find("AmmoRight").gameObject.SetActive(false);
        transform.Find("Ammo").Find("AmmoTotal").gameObject.SetActive(false);
        transform.Find("Ammo").Find("AmmoStr").gameObject.SetActive(false);
        transform.Find("Cross").gameObject.SetActive(false);
        transform.Find("Health").gameObject.SetActive(false);
    }
    public void ShowHUD(){
        transform.Find("Ammo").Find("AmmoLeft").gameObject.SetActive(true);
        transform.Find("Ammo").Find("AmmoRight").gameObject.SetActive(true);
        transform.Find("Ammo").Find("AmmoTotal").gameObject.SetActive(true);
        transform.Find("Ammo").Find("AmmoStr").gameObject.SetActive(true);
        transform.Find("Cross").gameObject.SetActive(true);
        transform.Find("Health").gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
