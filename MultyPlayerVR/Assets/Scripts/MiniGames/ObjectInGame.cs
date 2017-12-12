using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectInGame : Photon.MonoBehaviour {

    public GameObject fakePhysicObject;

    Rigidbody rigidBody;
    ObjectInGame instance;

    private Vector3 correctPos;
    private Quaternion correctRot;

    private MeshRenderer meshRenderer;

    private Vector3 lastPos;

    private Vector3 direct;
    private Vector3 correctDirect;
    private float speed;
    private float correctSpeed;

 

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
        correctRot = this.transform.rotation;
    }
    // Use this for initialization
    void Start() {

        InitObject();
        rigidBody = this.GetComponent<Rigidbody>();
        meshRenderer = this.GetComponent<MeshRenderer>();

        direct = new Vector3(1, 0, 0);

        if (fakePhysicObject)
            fakePhysicObject.transform.DOMove(HockeyGame.instance.playerPos[1].transform.position,10f);

    }

    // Update is called once per frame
    void Update() {
        UpdateObject();
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            OnStreamWrite(stream, info);
        }
        else
        { 
            OnStreamReceive(stream, info);
        }
    }

    void OnStreamWrite(PhotonStream stream, PhotonMessageInfo info)
    {
        switch (type)
        {
            case TYPE.Striker:
                {
                    // We own this player: send the others our data
                    stream.SendNext(transform.position);
                    break;
                }
            case TYPE.Ball:
                {
                    // We own this player: send the others our data
                    stream.SendNext(transform.position);
                    stream.SendNext(transform.rotation);
                    break;
                }
        }
    }

    void OnStreamReceive(PhotonStream stream, PhotonMessageInfo info)
    {
        switch (type)
        {
            case TYPE.Striker:
                {
                    // Network player, receive data
                    this.correctPos = (Vector3)stream.ReceiveNext();
                    break;
                }
            case TYPE.Ball:
                {
                    // Network player, receive data
                    this.correctPos = (Vector3)stream.ReceiveNext();
                    this.correctRot = (Quaternion)stream.ReceiveNext();
                    break;
                }
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
                    OnTriggerEnterBall(other);
                    break;
                }
        }
    }



    public void InitObject()
    {

    }

    public void UpdateObject()
    {
        switch (type)
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
        if (parentGO)
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
            BoxCollider box = PhotonNetwork.isMasterClient ? HockeyGame.instance.validArea[0] : HockeyGame.instance.validArea[1];

            if (!FBUtils.PointInOABB(pos, box))
                return;

            //this.transform.position = pos;
            //pos.y = transform.position.y;
            lastPos = pos;
            rigidBody.MovePosition(pos);

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
    Tweener tweenDoMove = null;

    void OnTriggerEnterBall(Collider other)
    {
       //check is goal
        if(other.gameObject == HockeyGame.instance.goals[0].gameObject)
        {
            HockeyGame.instance.photonView.RPC("AddScore2Players", PhotonTargets.AllViaServer, 1, 1);
        }
        else if (other.gameObject == HockeyGame.instance.goals[1].gameObject)
        {
            HockeyGame.instance.photonView.RPC("AddScore2Players", PhotonTargets.AllViaServer, 1, 0);
        }

        Vector3 contact = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position).normalized;
        direct = Vector3.Reflect(other.transform.position,contact);
        

        ObjectInGame objClass = other.GetComponent<ObjectInGame>();
        speed = 2f;
        direct.y = 0;
        Debug.Log("direct: "+direct);
        RaycastHit hit;
        if (objClass)
        {
            if (objClass.instance.type == TYPE.Striker)
                speed = 4f;
        }

        float timeToEnd = 0f;
        Vector3 targetPos = Vector3.zero;
        if (Physics.Raycast(transform.position, transform.TransformDirection(direct), out hit))
        {
            Debug.Log("is raycast");
            Debug.DrawLine(transform.position, hit.point);
            float dist = Vector3.Distance(transform.position, hit.point);
            //Debug.Log(" time is :" + speed / dist);
            timeToEnd = speed / dist;
            targetPos = hit.point;
            tweenDoMove = transform.DOMove(hit.point, timeToEnd);
            
        }

        Vector3 fakepos = CalcNextPosition(0.3f, transform.position, speed, direct);
        Debug.Log("Next position is: " + fakepos);
        if (fakePhysicObject)
            fakePhysicObject.transform.position = fakepos;
       
        if(PhotonNetwork.isMasterClient)
        photonView.RPC("AddForceOverNetwork", PhotonTargets.AllViaServer 
            ,targetPos , timeToEnd,PhotonNetwork.ServerTimestamp);
    }

    float time = 0f;
    void UpdateBall()
    {
        //transform.Translate(direct * speed * Time.smoothDeltaTime);
        //time += Time.smoothDeltaTime;
        //if (time >= 0.2999f)
        //{
        //    time = 0;
        //    Debug.Log("my position is +" + transform.position);
        //}


    }

    [PunRPC]
    public void AddForceOverNetwork(Vector3 targetPos,float timeToEnd, int timestamp)
    {
        float delay = 1.0f / (PhotonNetwork.ServerTimestamp - timestamp);
        Debug.Log("AddForceOverNetwork called, delay:" + delay);
        
        if (delay > timeToEnd)
            return;
        //0.1f is time DOmove called
        //Vector3 nextPos = CalcNextPosition(delay, posWhenSend, correctSpeed, correctDirect);
        if (tweenDoMove != null)
            tweenDoMove.ChangeValues(transform.position,targetPos,timeToEnd - delay);

        //tweenDoMove.timeScale *= (tweenDoMove.Duration() - delay) / tweenDoMove.Duration();
      
        //direct = correctDirect;
        //speed = correctSpeed;
    }

    
    Vector3 CalcNextPosition(float timestamp, Vector3 pos,float speed, Vector3 direct)
    {
        float dist = speed * timestamp;
        Ray a = new Ray(pos, transform.TransformDirection(direct));
        return a.GetPoint(dist);   
    }
#endregion
}
