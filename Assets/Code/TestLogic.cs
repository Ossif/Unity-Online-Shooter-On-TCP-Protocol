using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class TestLogic : MonoBehaviour
{

    public GameObject server;
    public GameObject client;

    // Start is called before the first frame update
    void Start()
    {
        try{
            Server s = Instantiate(server).GetComponent<Server>();
            s.Init();
            Instantiate(client);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
