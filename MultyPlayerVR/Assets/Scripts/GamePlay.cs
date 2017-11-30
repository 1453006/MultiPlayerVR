using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour {

    public GameObject VRplayer;
    public Vector3 spawnPoint;
    public Text status;

    float lastClickTime = 0f;
    float catchTime = .25f;

    public static GamePlay instance;
    private static List<GvrPermissionsRequester.PermissionStatus> permissionList =
new List<GvrPermissionsRequester.PermissionStatus>();

    private void Awake()
    {
        spawnPoint = new Vector3(Random.RandomRange(0,3), 2, Random.RandomRange(0, 3));
        instance = this;
    }
    // Use this for initialization
    void Start () {
        //get audio permission
        string[]  permissionNames = { "android.permission.RECORD_AUDIO" };
        /// request permisson only call when build on devices
#if !UNITY_EDITOR
        RequestPermissions(permissionNames, status);
#endif
        //spawn VR player to screen
        VRplayer.transform.position = spawnPoint;
	}
	
	// Update is called once per frame
	void Update () {
        

        if (GvrControllerInput.ClickButtonDown)
        {
            if (Time.time - lastClickTime < catchTime)
            {
                Debug.Log("double click");
            }
            else
            {
                //normal click
                Debug.Log("single click");
            }
            lastClickTime = Time.time;
        }
      
    }


    public void RequestPermissions(string[] permissionNames,Text statusText)
    {
        if (statusText != null)
        {
            statusText.text = "Requesting permission....";
        }
        GvrPermissionsRequester permissionRequester = GvrPermissionsRequester.Instance;
        if (permissionRequester == null)
        {
            statusText.text = "Permission requester cannot be initialized.";
            return;
        }
        Debug.Log("Permissions.RequestPermisions: Check if permission has been granted");
        if (!permissionRequester.IsPermissionGranted(permissionNames[0]))
        {
            Debug.Log("Permissions.RequestPermisions: Permission has not been previously granted");
            if (permissionRequester.ShouldShowRational(permissionNames[0]))
            {
                statusText.text = "This game needs to access external storage.  Please grant permission when prompted.";
                statusText.color = Color.red;
            }
            permissionRequester.RequestPermissions(permissionNames,
                (GvrPermissionsRequester.PermissionStatus[] permissionResults) =>
                {
                    statusText.color = Color.cyan;
                    permissionList.Clear();
                    permissionList.AddRange(permissionResults);
                    string msg = "";
                    foreach (GvrPermissionsRequester.PermissionStatus p in permissionList)
                    {
                        msg += p.Name + ": " + (p.Granted ? "Granted" : "Denied") + "\n";
                    }
                    statusText.text = msg;
                });
        }
        else
        {
            statusText.text = "ExternalStorage permission already granted!";
        }
    }


    //datld test
    public void HideObjectIsNotMine()
    {
        GameObject[] listobj = GameObject.FindGameObjectsWithTag("InteractiveObject");
        foreach(GameObject obj in listobj)
        {
            PhotonView photonView = obj.GetComponent<PhotonView>();
            if(photonView && !photonView.isMine)
            {
                obj.SetActive(false);
            }
                
        }
       
           
    }
}
