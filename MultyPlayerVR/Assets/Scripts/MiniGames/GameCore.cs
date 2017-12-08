using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class GameCore : PunBehaviour
{

    // note that master position always 0, others are remote position
    public Transform[] playerPos;
    public State currentState;
    public GameType currentGame;

    public enum GameType
    {
        Hockey = 0

    }

    public enum State
    {
        Waiting = 0,
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

    void SetState(State state)
    {
        switch (state)
        {
            case State.Waiting:{
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
        //set player Position to be correct
        this.photonView.RPC("SetPlayerPosition", PhotonTargets.AllViaServer);
    }

    
    #region PUN Behaviour
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom()");
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.room.PlayerCount == 2)
        {
            Debug.Log("OnJoinedRoom: Player Count == 2");
            SetState(State.Start);
        }
        else
        {
            Debug.Log("Waiting for another player");
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("Other player arrived");

        if (PhotonNetwork.room.PlayerCount == 2)
        {
            Debug.Log("OnPhotonPlayerConnected: Player Count == 2");
            SetState(State.Start);

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


#endregion

}

