using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInfo : MonoBehaviour
{
    public string playerId;
    public string PlayerName;
    public int playerAnimId = 0;
    private void Start()
    {
        RaggDollOff();
    }

    public void RaggDollOff()
    {

        Rigidbody[] rb = this.transform.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = this.transform.GetComponentsInChildren<Collider>();
        foreach(Rigidbody rigidbody in rb)
        {
            rigidbody.isKinematic = true;
        }        
        foreach(Collider col in colliders)
        {
            col.enabled = false;
        }
        this.transform.GetComponent<Rigidbody>().isKinematic = false;
        this.transform.GetComponent<CapsuleCollider>().enabled = true;
        this.transform.GetComponent<Animator>().enabled = true;
    }
    public void RaggDollOn()
    {

        Rigidbody[] rb = this.transform.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = this.transform.GetComponentsInChildren<Collider>();
        foreach(Rigidbody rigidbody in rb)
        {
            rigidbody.isKinematic = false;
        }        
        foreach(Collider col in colliders)
        {
            col.enabled = true;
        }
        this.transform.GetComponent<Rigidbody>().isKinematic = true;
        this.transform.GetComponent<CapsuleCollider>().enabled = false;
        this.transform.GetComponent<Animator>().enabled = false;
        
    }
}
