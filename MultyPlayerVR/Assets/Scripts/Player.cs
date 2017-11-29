using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public GameObject defaultLaser;
    public GameObject visualPlayer;
    public DaydreamElements.Teleport.TeleportController teleportController;
    public static Player instance;


    public enum PlayerState
    {
        None, Teleporting,Selecting
    };

    private void Awake()
    {
        instance = this;
    }
  
    public PlayerState currentState;

	// Use this for initialization
	void Start () {
	
	}

    public void SetState(PlayerState state)
    {
        currentState = state;
        defaultLaser.SetActive(true);
        if (state == PlayerState.None)
        {
            Invoke("enableTeleport", 1f);
        }
        else if (state == PlayerState.Teleporting)
        {
            defaultLaser.SetActive(false);
        }
        else if (state == PlayerState.Selecting)
        {
            teleportController.gameObject.SetActive(false);
        }

    }

    public void enableTeleport()
    {
        teleportController.gameObject.SetActive(true);
    }

    void UpdateState()
    {
        if (currentState == PlayerState.None)
        {

        }
        else if (currentState == PlayerState.Teleporting)
        {
           
        }
    }

    


    // Update is called once per frame
    void Update () {

       
        UpdateState();

    }
}
