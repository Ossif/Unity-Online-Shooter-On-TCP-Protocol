using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TramplineScript : MonoBehaviour
{
    public AudioClip TramplinClip;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            this.GetComponentInParent<Animator>().SetTrigger("Open");
            this.GetComponentInParent<AudioSource>().PlayOneShot(TramplinClip);
        }
        else if(other.CompareTag("Enemy"))
        {
            this.GetComponentInParent<Animator>().SetTrigger("Open");
        }
    }
}
