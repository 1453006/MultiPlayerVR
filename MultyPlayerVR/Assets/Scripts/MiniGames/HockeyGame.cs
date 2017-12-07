using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HockeyGame : GameCore {
 
    public Transform[] StrikerSpawnPoint;
    public static HockeyGame instance;
#region Objects In Game
    public GameObject strikerPrefab;
    public GameObject ball;
    #endregion

    private void Awake()
    {
        instance = this;
    }
    // Use this for initialization
    void Start () {
        ball.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    public override void OnStartGame()
    {
        base.OnStartGame();
        Vector3 pos;
        if(PhotonNetwork.isMasterClient)
            //if is master room
            pos = StrikerSpawnPoint[0].position;
        else
            pos = StrikerSpawnPoint[1].position;

        GameObject striker = PhotonNetwork.Instantiate(strikerPrefab.name, pos, Quaternion.identity, 0);
        striker.GetPhotonView().RPC("SetParent", PhotonTargets.AllViaServer, this.gameObject.name);

        //set active BALL
        ball.SetActive(true);
    }


}
