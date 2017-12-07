using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectInGame : Photon.MonoBehaviour {

    Rigidbody rigidBody;
    ObjectInGame instance;
    private Vector3 correctPos;

    public enum TYPE
    {
        Striker,
        Ball
    };
    public ObjectInGame.TYPE type;


    private void Awake()
    {
        instance = this;
        correctPos = this.transform.position;
    }
    // Use this for initialization
    void Start () {
        InitObject();
        rigidBody = this.GetComponent<Rigidbody>();

    }
	
	// Update is called once per frame
	void Update () {
        UpdateObject();
	}

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(transform.position);   

        }
        else
        {
            // Network player, receive data
            this.correctPos = (Vector3)stream.ReceiveNext();     
        }
    }


private void OnCollisionEnter(Collision collision)
    {
        switch (type)
        {
            case TYPE.Striker:
                {
                   
                    break;
                }
            case TYPE.Ball:
                {
                    OnCollisionEnterBall(collision);
                    break;
                }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called: ");
        switch (type)
        {
            
            case TYPE.Striker:
                {
                    OnTriggerEnterStriker(other);
                    break;
                }
            case TYPE.Ball:
                {
                   
                    break;
                }
        }
    }

    public void InitObject()
    {

    }

    public void UpdateObject()
    {
        switch(type)
        {
            case TYPE.Striker:
                {
                    UpdateStriker();
                    break;
                }
            case TYPE.Ball:
                {
                    UpdateBall();
                    break;
                }
        }
    }

#region common
    [PunRPC]
    public void SetParent(string parent)
    {
        GameObject parentGO = GameObject.Find("parent");
        if(parentGO)
        {
            transform.SetParent(parentGO.transform);
        }

        
    }
#endregion

#region Striker
    void UpdateStriker()
    {
        if (photonView.isMine)
        {
            Vector3 pos = GvrPointerInputModule.CurrentRaycastResult.worldPosition;
            this.transform.position = pos;

            Ray a = new Ray(transform.position, transform.forward);
            Ray b;
            RaycastHit hit;

            if (Deflect(a, out b, out hit))
            {
                Debug.DrawLine(a.origin, hit.point);
                Debug.DrawLine(b.origin, b.origin + 3 * b.direction);
            }
        }
        else
        {
            transform.DOMove(correctPos, 0.2f);
        }
    }
    void OnTriggerEnterStriker(Collider other)
    {
        ObjectInGame obj = other.gameObject.GetComponent<ObjectInGame>();
        if(obj && obj.type == TYPE.Ball)
        {
            if(other.gameObject.GetPhotonView().owner.ID != PhotonNetwork.player.ID)
                other.gameObject.GetPhotonView().TransferOwnership(PhotonNetwork.player.ID);
        }
    }

    bool Deflect(Ray ray, out Ray deflected, out RaycastHit hit)
    {

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 normal = hit.normal;
            Vector3 deflect = Vector3.Reflect(ray.direction, normal);

            deflected = new Ray(hit.point, deflect);
            return true;
        }

        deflected = new Ray(Vector3.zero, Vector3.zero);
        return false;
    }

    #endregion

#region Ball
   
    void OnCollisionEnterBall(Collision other)
    {
        Debug.Log("OnCollisionEnterBall With : " + other.gameObject.name);
        Vector3 forceVector = other.contacts[0].normal * 3f;
        rigidBody.AddForce(forceVector, ForceMode.Impulse);

    }

    void UpdateBall()
    {
        if (!photonView.isMine)
        {
            rigidBody.isKinematic = true;
            transform.DOMove(correctPos, 0.2f);
        }
        else
        {
            rigidBody.isKinematic = false;
           
        }
    }

    [PunRPC]
    public void AddForceOverNetwork(Vector3 force)
    {
        
        Debug.Log("AddForceOverNetwork called");
        rigidBody.AddForce(force, ForceMode.Impulse);
    }

#endregion
}
