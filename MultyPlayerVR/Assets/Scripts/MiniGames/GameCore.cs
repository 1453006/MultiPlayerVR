using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;

public class GameCore : PunBehaviour
{
    public enum ResultType
    {
        None = 0,
        Draw,
        LocalWin,
        LocalLoss
    }

    // note that master position always 0, others are remote position
    public Transform[] playerPos;
    public State currentState;
    public GameType currentGame;

    public double startTime = 0;

    #region UI
    public Text countDown;
    public Text Result;
    public Text txtScore_master, txtScore_remote;
    #endregion

    #region 2 Players
    public int score_master = 0;
    public int score_remote = 0;
#endregion

    public enum GameType
    {
        Hockey = 0

    }

    public enum State
    {
        Waiting = 0,
        CountDown,
        Start,
        Playing,
        End
    };

    // Use this for initialization
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    public void OnUpdateGUI()
    {
        countDown.transform.faceToMainCamera();
        txtScore_master.transform.faceToMainCamera();
        txtScore_remote.transform.faceToMainCamera();
    }
    public void OnSetGUI()
    {

        countDown.text = 0.ToString();
        txtScore_master.text = score_master.ToString();
        txtScore_remote.text = score_remote.ToString();
    }

    public void SetState(State state)
    {
        switch (state)
        {
            case State.Waiting:{
                    OnSetGUI();
                    break;
                }
            case State.CountDown:{
                    OnCountDown();
                    break;
                }
            case State.Start:
                {
                    OnStartGame();
                    break;
                }
            case State.Playing:
                {
                    break;
                }
        }
        currentState = state;
    }

    public virtual void OnStartGame()
    {
      

       
    }

    public virtual void OnCountDown()
    {
        //set player Position to be correct
        Player.instance.SetState(Player.PlayerState.PlayingGame);
        this.photonView.RPC("SetPlayerPosition", PhotonTargets.AllViaServer);
        if (PhotonNetwork.isMasterClient)
        {
            this.photonView.RPC("SetStartTime", PhotonTargets.AllViaServer, PhotonNetwork.time);
        }

    }
    #region PUN Behaviour
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom()");
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.room.PlayerCount == 1)
        {
            Debug.Log("OnJoinedRoom: Player Count == 2");
            SetState(State.CountDown);
        }
        else
        {
            Debug.Log("Waiting for another player");
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("Other player arrived");

        if (PhotonNetwork.room.PlayerCount == 1)
        {
            Debug.Log("OnPhotonPlayerConnected: Player Count == 2");
            SetState(State.CountDown);

        }
    }


    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        Debug.Log("Other player disconnected! " + otherPlayer.ToStringFull());
    }


    public override void OnConnectionFail(DisconnectCause cause)
    {

    }
    #endregion

#region RPC FUNCTION

    [PunRPC]
    public void SetPlayerPosition()
    {
        if(PhotonNetwork.isMasterClient)
        {
            Player.instance.MoveTo(playerPos[0].position, playerPos[0].rotation);
        }
        else
        {
            Player.instance.MoveTo(playerPos[1].position, playerPos[1].rotation);
        }
    }

    [PunRPC]
    public void SetStartTime(double time)
    {
        startTime = time;
    }

    [PunRPC]
    public void AddScore2Players(int score, int index)
    {
        if (index == 0) //is master
        {
            score_master += 1;
            txtScore_master.text = score_master.ToString();
        }
        else
        {
            score_remote += 1;
            txtScore_remote.text = score_remote.ToString();
        }


    }

#endregion

}

