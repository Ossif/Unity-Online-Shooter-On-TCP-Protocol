using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CanvasLogic : MonoBehaviour
{

    public TextMeshProUGUI  ammoLeft;
    public TextMeshProUGUI  ammoRight;
    public TextMeshProUGUI  ammoTotal;

    public void SetAmmoLeft(int var){
        ammoLeft.text = var.ToString();
    }
    
    public void SetAmmoRight(int var){
        ammoRight.text = var.ToString();
    }

    public void SetAmmoTotal(int var){
        ammoTotal.text = var.ToString();
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
