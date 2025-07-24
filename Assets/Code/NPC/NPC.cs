using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int npcID;
    public string npcName;
    //public int playerAnimId = 0;
    //public WeaponId weaponId = 0;
    public GameObject WeaponParentBone;
    //public GameObject WeaponObject = null;
    public bool isMoving = false;
    public Vector3 pointA;
    public Vector3 pointB;
    public float speed = 1.0f;
    private float startTime;
    private float journeyLength;
    public bool start = false;

    private void Start()
    {
        WeaponParentBone = transform.Find("Armature").Find("mixamorig:Hips").Find("mixamorig:Spine").Find("mixamorig:Spine1").Find("mixamorig:Spine2").Find("mixamorig:RightShoulder").Find("mixamorig:RightArm").Find("mixamorig:RightForeArm").Find("mixamorig:RightHand").Find("mixamorig:RightHand_end").gameObject;
        RaggDollOff();
    }

    public void RaggDollOff()
    {

        Rigidbody[] rb = this.transform.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = this.transform.GetComponentsInChildren<Collider>();
        foreach (Rigidbody rigidbody in rb)
        {
            rigidbody.isKinematic = true;
        }
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        this.transform.GetComponent<Rigidbody>().isKinematic = false;
        this.transform.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        this.transform.GetComponent<CapsuleCollider>().enabled = true;
        this.transform.GetComponent<Animator>().enabled = true;
    }
    public void RaggDollOn()
    {

        Rigidbody[] rb = this.transform.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = this.transform.GetComponentsInChildren<Collider>();
        foreach (Rigidbody rigidbody in rb)
        {
            rigidbody.isKinematic = false;
        }
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
        this.transform.GetComponent<Rigidbody>().isKinematic = true;
        this.transform.GetComponent<CapsuleCollider>().enabled = false;
        this.transform.GetComponent<Animator>().enabled = false;
    }

    public void StartMovePath(Vector3 startPos, Vector3 endPos, float Speed)
    {
        Debug.Log($"StartMovePath {startPos}, {endPos}, {Speed}");
        pointA = startPos;
        pointB = endPos;
        this.speed = Speed;
        startTime = Time.time;
        journeyLength = Vector3.Distance(pointA, pointB);
        isMoving = true;
    }
    void FixedUpdate()
    {
        if (isMoving == true)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;
            Vector3 newPosition = new Vector3(
                Mathf.Lerp(pointA.x, pointB.x, fracJourney),
                transform.position.y,
                Mathf.Lerp(pointA.z, pointB.z, fracJourney)
            );
            transform.position = newPosition;
            //transform.LookAt(pointB);
            float targetHeight = CalculateTargetHeight(newPosition);
            if (Mathf.Abs(transform.position.y - targetHeight) > 5f)
            {
                Debug.Log($"Текущая высота: {transform.position.y}, ожидаемая высота: {targetHeight}");
                newPosition.y = targetHeight;
            }

            transform.position = newPosition;
        }
    }
    
    float CalculateTargetHeight(Vector3 position)
    {
        float journeyLength = Vector3.Distance(pointA, pointB);
        float distCovered = (Time.time - startTime) * speed;
        float fracJourney = distCovered / journeyLength;

        Vector3 currentPosition = Vector3.Lerp(pointA, pointB, fracJourney);

        /*float distanceToTarget = Vector3.Distance(position, pointB);
        float heightDifference = pointB.y - pointA.y;
        
        Vector3 XYZ = (pointB - pointA);
        float AB = Mathf.Sqrt(Mathf.Pow(XYZ.x, 2) + Mathf.Pow(XYZ.z, 2));
        Vector3 V2 = position - pointA; 
        float AC = Mathf.Sqrt(Mathf.Pow(V2.x, 2) + Mathf.Pow(V2.z, 2));
        float y = Mathf.Abs(XYZ.y) * AC / AB;
        float finalY = */

        return currentPosition.y;
    }
}
