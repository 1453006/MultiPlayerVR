using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MathGame : GameCore, IPunTurnManagerCallbacks
{
    private PunTurnManager turnManager;
    short countDownDuration = 5;
    // keep track of when we show the results to handle game logic.
    private bool IsShowingResults;


    // Use this for initialization
    void Start () {
        this.turnManager = this.gameObject.AddComponent<PunTurnManager>();
        this.turnManager.TurnManagerListener = this;
        this.turnManager.TurnDuration = 5f;
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
                //countDown.gameObject.SetActive(false);
            }
        }

        if (this.turnManager.IsOver)
        {
            return;
        }

        /*
		// check if we ran out of time, in which case we loose
		if (turnEnd<0f && !IsShowingResults)
		{
				Debug.Log("Calling OnTurnCompleted with turnEnd ="+turnEnd);
				OnTurnCompleted(-1);
				return;
		}
	*/
       
        if (this.turnManager.Turn > 0  && !IsShowingResults)
        {
            this.countDown.text = this.turnManager.RemainingSecondsInTurn.ToString("F1") + " SECONDS";
        }

    }

    public override void OnStartGame()
    {
        base.OnStartGame();
        if (this.turnManager.Turn == 0)
        {
            // when the room has two players, start the first turn (later on, joining players won't trigger a turn)
            this.StartTurn();
        }

    }

    #region TurnManager Callbacks

    /// <summary>Called when a turn begins (Master Client set a new Turn number).</summary>
    public void OnTurnBegins(int turn)
    {
        Debug.Log("OnTurnBegins() turn: " + turn);
        IsShowingResults = false;
     
    }

    public void OnTurnTimeEnds(int obj)
    {
        if (!IsShowingResults)
        {
            Debug.Log("OnTurnTimeEnds: Calling OnTurnCompleted");
            OnTurnCompleted(-1);
        }
    }


    public void OnTurnCompleted(int obj)
    {
        Debug.Log("OnTurnCompleted: " + obj);
        this.OnEndTurn();
    }


    // when a player moved (but did not finish the turn)
    public void OnPlayerMove(PhotonPlayer photonPlayer, int turn, object move)
    {
        
    }


    // when a player made the last/final move in a turn
    public void OnPlayerFinished(PhotonPlayer photonPlayer, int turn, object move)
    {
        Debug.Log("OnTurnFinished: " + photonPlayer + " turn: " + turn + " action: " + move);

        
    }


    private void UpdateScores()
    {
        //if (this.result == ResultType.LocalWin)
        //{
        //    PhotonNetwork.player.AddScore(1);   // this is an extension method for PhotonPlayer. you can see it's implementation
        //}
    }

    #endregion


    #region Core Gameplay Methods


    /// <summary>Call to start the turn (only the Master Client will send this).</summary>
    public void StartTurn()
    {
        if (PhotonNetwork.isMasterClient)
        {
            this.turnManager.BeginTurn();
        }
    }

    public void MakeTurn()
    {
        
    }

    public void OnEndTurn()
    {
        this.StartCoroutine("ShowResultsBeginNextTurnCoroutine");
    }

    public IEnumerator ShowResultsBeginNextTurnCoroutine()
    {

        IsShowingResults = true;
        yield return new WaitForSeconds(2.0f);

        //DATLD
        this.StartTurn();
    }


    public void EndGame()
    {
        Debug.Log("EndGame");
    }

    private void CalculateWinAndLoss()
    {
       
    }
    #endregion


}
