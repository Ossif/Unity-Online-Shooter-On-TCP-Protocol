using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponEnumIds;

public class EnemyInfo : MonoBehaviour
{
    public string playerId;
    public string PlayerName;
    public int playerAnimId = 0;
    public WeaponId weaponId = 0;
    public GameObject WeaponParentBone;
    public GameObject WeaponObject = null;
    private void Start()
    {
        WeaponParentBone = transform.Find("Armature").Find("mixamorig:Hips").Find("mixamorig:Spine").Find("mixamorig:Spine1").Find("mixamorig:Spine2").Find("mixamorig:RightShoulder").Find("mixamorig:RightArm").Find("mixamorig:RightForeArm").Find("mixamorig:RightHand").Find("mixamorig:RightHand_end").gameObject;
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
