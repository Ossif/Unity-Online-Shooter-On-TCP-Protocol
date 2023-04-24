using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponEnumIds;
using UnityEngine.UIElements;

public class WeaponSystem : MonoBehaviour
{
    public GameObject AK_cartridge;
    private GameObject weaponCartridge = null;

    private WeaponEnum we;

    private WeaponId[] weaponSlots = new WeaponId[3];
    private int currentSlot = 0;

    private bool isReloading = false;

    private GameObject weaponObject;
    private Animator handsAnimator;
    // Start is called before the first frame update
    void Start()
    {
        handsAnimator = GameObject.Find("hands").GetComponent<Animator>();
        we = gameObject.GetComponent<WeaponEnum>();
        we.InitializeAllWeapon();

        weaponSlots[0] = WeaponId.AK;
        weaponSlots[1] = WeaponId.PISTOL;
        weaponSlots[2] = WeaponId.SAWNED_OFF;

        foreach(Weapon w in we.weaponList)
        {
            if(weaponSlots[0] == w.weaponId)
            {
                weaponObject = Instantiate(w.weaponObject, new Vector3(0, 0, 0), Quaternion.identity);
                weaponObject.transform.parent = GameObject.Find("FPSAnimationsObject").GetComponent<Transform>();
                weaponObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                weaponObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
                weaponObject.transform.localPosition = new Vector3(0, 0, 0);
                break;
            }
        }
    }

    public void ChangeWeapon(int slotId)
    {
        currentSlot = slotId;
        Destroy(weaponObject);

        string handAnim = "H_";

        foreach(Weapon w in we.weaponList)
        {
            if(weaponSlots[currentSlot] == w.weaponId)
            {
                weaponObject = Instantiate(w.weaponObject, new Vector3(0, 0, 0), Quaternion.identity);
                weaponObject.transform.parent = GameObject.Find("FPSAnimationsObject").GetComponent<Transform>();
                weaponObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                weaponObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
                weaponObject.transform.localPosition = new Vector3(0, 0, 0);

                handAnim += w.takeAnim;
                break;
            }
        }
        handsAnimator.Play(handAnim);
        FinishReload();
    }

    public void FinishReload(){
        isReloading = false;
        if(weaponCartridge != null) 
        {
            Destroy(weaponCartridge);
            weaponCartridge = null;
        }
    }

    void Update()
    {    
        if(Input.GetKeyDown("1") && currentSlot != 0){ 
            ChangeWeapon(0);
        }

        if(Input.GetKeyDown("2") && currentSlot != 1){ 
            ChangeWeapon(1);
        }

        if(Input.GetKeyDown("3") && currentSlot != 2){ 
            ChangeWeapon(2);
        }

        if(Input.GetKeyDown(KeyCode.R) && isReloading == false){
            Debug.Log("RRR");
            isReloading = true;
            string handAnim = "H_";

            foreach(Weapon w in we.weaponList)
            {
                if(weaponSlots[currentSlot] == w.weaponId)
                {
                    if(w.name == "AK"){
                        weaponCartridge = Instantiate(AK_cartridge, new Vector3(0, 0, 0), Quaternion.identity);
                        weaponCartridge.transform.parent = GameObject.Find("FPSAnimationsObject").GetComponent<Transform>();
                        weaponCartridge.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                        weaponCartridge.transform.localRotation = new Quaternion(0, 0, 0, 0);
                        weaponCartridge.transform.localPosition = new Vector3(0, 0, 0);
                    }
                    handAnim += w.reloadAnim;
                    weaponObject.transform.GetChild(0).GetComponent<Animator>().Play(w.reloadAnim);
                    break;
                }
            }
            handsAnimator.Play(handAnim);
        }
    }

    void OnKeyDown(KeyDownEvent ev)
    {
        /*switch (ev.keyCode) {
            
            case (KeyCode.Keypad2):
            {
                if(currentSlot == 1) break;
                currentSlot = 1;
                Destroy(weaponObject);

                string handAnim = "H_";

                foreach(Weapon w in we.weaponList)
                {
                    if(weaponSlots[0] == w.weaponId)
                    {
                        Debug.Log(w.name);
                        weaponObject = Instantiate(w.weaponObject, new Vector3(0, 0, 0), Quaternion.identity);
                        weaponObject.transform.parent = GameObject.Find("FPSAnimationsObject").GetComponent<Transform>();
                        weaponObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                        weaponObject.transform.localPosition = new Vector3(0, 0, 0);

                        handAnim += w.takeAnim;
                        break;
                    }
                }
                handsAnimator.Play(handAnim);
                break; 
            }
            case (KeyCode.Keypad3):
            {
                if(currentSlot == 2) break;
                currentSlot = 2;
                Destroy(weaponObject);

                string handAnim = "H_";

                foreach(Weapon w in we.weaponList)
                {
                    if(weaponSlots[0] == w.weaponId)
                    {
                        Debug.Log(w.name);
                        weaponObject = Instantiate(w.weaponObject, new Vector3(0, 0, 0), Quaternion.identity);
                        weaponObject.transform.parent = GameObject.Find("FPSAnimationsObject").GetComponent<Transform>();
                        weaponObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                        weaponObject.transform.localPosition = new Vector3(0, 0, 0);

                        handAnim += w.takeAnim;
                        break;
                    }
                }

                handsAnimator.Play(handAnim);
                break; 
            }




            case (KeyCode.R):
            {
                break;
            }
        }*/
    }
}
