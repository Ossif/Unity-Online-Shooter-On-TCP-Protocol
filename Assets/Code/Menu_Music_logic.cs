using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_Music_logic : MonoBehaviour
{

    private AudioSource AS;
    public AudioClip music1;
    public AudioClip music2;
    // Start is called before the first frame update
    void Start()
    {
        AS = transform.Find("Audio Source").GetComponent<AudioSource>();
        AS.PlayOneShot(music1);
        Invoke("PlayMusic", 2.0f);
    }

    public void PlayMusic() { 
        AS.clip = music2;
        AS.loop = true;
        AS.Play(); 
    }
}
