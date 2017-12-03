using UnityEngine;
using System.Collections;


public class NetworkPlayer : Photon.MonoBehaviour {
    GameObject playerGO;
    Transform cameraTransform;
    Transform controllerTransform;
    public Transform visualHead;
    public Transform visualHandTransform;
    public Transform visualLowerJaw;
    public float range = 100f;
    public float health;

    private Vector3 lowerJawInitPos;
    

#region Voice Regconition
    public AudioSource audioSource;
    public int numSample = 1024;
    public float[] samples;

    float GetAveragedVolume()
    {
        float result = 0f;
        if(audioSource.isPlaying)
        {
            for(int chanel = 0;chanel < 2;chanel++)
            {
                audioSource.GetOutputData(samples, chanel);
                for(int i = 0; i< numSample;i++)
                {
                    result += Mathf.Abs(samples[i]);
                }
            }
        }
        return result / numSample;
    }

    #endregion

    // Use this for initialization
    void Start () {
        playerGO = GameObject.Find("Player");
        lowerJawInitPos = visualLowerJaw.transform.localPosition;
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


            //samples = new float[numSample];
            //audioSource = transform.GetComponent< AudioSource > ();
            //audioSource.clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
            //while (!(Microphone.GetPosition(null) > 0)) { }
            //audioSource.Play();

        }

    }
	
	// Update is called once per frame
	void Update () {

      
       
        //if(!photonView.isMine)
        //{
        //    float volume = GetAveragedVolume();
        //    visualLowerJaw.position = lowerJawInitPos - transform.up * volume;
        //}
        if (photonView.isMine)
        {
            visualLowerJaw.localPosition = new Vector3(lowerJawInitPos.x, lowerJawInitPos.y - MicInput.instance.MicLoudness/20f, lowerJawInitPos.z);
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
