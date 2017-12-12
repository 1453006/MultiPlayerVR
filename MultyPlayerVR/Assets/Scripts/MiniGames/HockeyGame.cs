using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HockeyGame : GameCore {
 
    public Transform[] StrikerSpawnPoint;
    public static HockeyGame instance;

    short countDownDuration = 5;

#region Objects In Game
    public GameObject strikerPrefab;
    public GameObject ball;
    public BoxCollider[] validArea;
    public BoxCollider[] goals;
    #endregion


    private void Awake()
    {
        currentGame = GameType.Hockey;
        instance = this;
        
    }
    // Use this for initialization
    void Start () {
        ball.SetActive(false);

        base.OnSetGUI();
    }
	
	// Update is called once per frame
	void Update () {

        int displayTime = 10000;
        base.OnUpdateGUI();
        if (currentState == State.CountDown && base.startTime != 0)
        {
            double timetick = PhotonNetwork.time - base.startTime;
            displayTime = Mathf.RoundToInt((float)(countDownDuration - timetick));
            countDown.text = displayTime.ToString();
            if (displayTime <= 0)
            {
                base.SetState(State.Start);
                countDown.gameObject.SetActive(false);
            }
        }
        

    }

  
    public override void OnStartGame()
    {
        base.OnStartGame();
        Vector3 pos;
        if (PhotonNetwork.isMasterClient)
        {    //if is master room
            pos = StrikerSpawnPoint[0].position;
            validArea[1].gameObject.SetActive(false);
        }
        else
        {
            pos = StrikerSpawnPoint[1].position;
            validArea[0].gameObject.SetActive(false);
        }

        GameObject striker = PhotonNetwork.Instantiate(strikerPrefab.name, pos, Quaternion.identity, 0);
        striker.GetPhotonView().RPC("SetParent", PhotonTargets.AllViaServer, this.gameObject.name);

        //set active BALL
        ball.SetActive(true);

        
    }

    
   
}
