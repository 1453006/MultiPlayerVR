using UnityEngine;
using System.Collections;


public class NetworkPlayer : Photon.MonoBehaviour {
    GameObject playerGO;
    Transform cameraTransform;
    Transform controllerTransform;
    public Transform visualHead;
    public Transform visualHandTransform;
    public float range = 100f;
    public float health;
	// Use this for initialization
	void Start () {
        playerGO = GameObject.Find("Player");
        
        health = 100f;
        foreach (Transform child in playerGO.transform)
        {
            if(child.name == "Main Camera")
            {
                cameraTransform = child;
            }
            else if(child.name == "GvrControllerPointer")
            {
                controllerTransform = child.GetChild(0); // is controller 
            }
        }

        //enable voice recorder only if it is mine;
        if(photonView.isMine)
        {
            this.transform.GetComponent<PhotonVoiceRecorder>().enabled = true;
            //save this avatar to player prefab
            Player.instance.visualPlayer = gameObject;
            //if (playerGO)
            //    transform.SetParent(playerGO.transform);
            //transform.localPosition = Vector3.zero;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (photonView.isMine)
        {
            GvrBasePointer laserPointerImpl = (GvrBasePointer)GvrPointerInputModule.Pointer;

            if (GvrControllerInput.TouchDown)
            {
                Debug.Log("hitting point:" + GvrPointerInputModule.CurrentRaycastResult.worldPosition);
                Vector3 endPoint = GvrPointerInputModule.CurrentRaycastResult.worldPosition;
                endPoint.y = playerGO.transform.position.y;
                
                //gameObject.transform.position = new Vector3(laserPointerImpl.PointerIntersection.x, gameObject.transform.position.y, laserPointerImpl.PointerIntersection.z);
            }

            // Lerping smooths the movement
            if (cameraTransform)
            {
              
                visualHead.rotation = cameraTransform.rotation;
            }
            if(controllerTransform)
            {
                visualHandTransform.position = controllerTransform.position;
                visualHandTransform.rotation = controllerTransform.rotation;
               // visualHandTransform.position = controllerTransform.position;
            }
            //test shooting
            if (GvrController.AppButtonDown)
            {
                Debug.Log("shoot");
                RaycastHit hit;
                if (Physics.Raycast(visualHandTransform.transform.position, visualHandTransform.transform.forward, out hit, range))
                {
                    switch (hit.transform.gameObject.tag)
                    {
                        case "Player":
                            {
                                Debug.Log("hit other");
                                NetworkPlayer script = hit.transform.gameObject.GetComponent<NetworkPlayer>();
                                script.GetComponent<PhotonView>().RPC("TakeDamge", PhotonTargets.All, (float)-10f);
                                break;
                            }
                    }
                }
            }
        }


 
    }

  

    [PunRPC]
    public void TakeDamge(float atk)
    {
        health += atk;
        if (health <= 0)
        {
            this.gameObject.SetActive(false);
            Debug.Log("die");
        }

    }
    
    
}
