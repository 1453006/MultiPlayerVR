using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInGame : MonoBehaviour {

    Rigidbody rigidBody;
    ObjectInGame instance;
    public enum TYPE
    {
        Striker,
        Ball
    };
    public ObjectInGame.TYPE type;


    private void Awake()
    {
        instance = this;
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
                    break;
                }
        }
    }

#region Striker
    void UpdateStriker()
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

       
        rigidBody.AddForce(other.contacts[0].normal * 3f, ForceMode.Impulse);
       

    }
#endregion
}
