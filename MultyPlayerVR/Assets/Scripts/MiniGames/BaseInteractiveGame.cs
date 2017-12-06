using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseInteractiveGame : Photon.MonoBehaviour, IPointerDownHandler
{
    public Transform[] spawnPlayerPos;
    public bool[] validPos;
    bool SentPickup, PickupIsMine;

    // Use this for initialization
    void Start()
    {
        //init pos
        validPos = new bool[spawnPlayerPos.Length];
        for (int i = 0; i < validPos.Length; i++)
            validPos[i] = false;
    }

    bool getValidSpawnPos(ref int index)
    {
        for (int i = 0; i < validPos.Length; i++)
        {
            if (validPos[i] == false)
            {
                index = i;
                return true;
            }
        }
        return false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Player.instance.currentState != Player.PlayerState.None)
            return;

        if (this.SentPickup)
        {
            // skip sending more pickups until the original pickup-RPC got back to this client
            return;
        }

        this.SentPickup = true;
        this.photonView.RPC("PunPickup", PhotonTargets.AllViaServer);




    }

    [PunRPC]
    public void PunPickup(PhotonMessageInfo msgInfo)
    {
        // when this client's RPC gets executed, this client no longer waits for a sent pickup and can try again
        if (msgInfo.sender.IsLocal) this.SentPickup = false;

        int index = 0;
        if (!getValidSpawnPos(ref index))
        {

            return;     // makes this RPC being ignored
        }


        // if the RPC isn't ignored by now, this is a successful pickup. this might be "my" pickup and we should do a callback
        this.PickupIsMine = msgInfo.sender.IsLocal;

        // call the method OnPickedUp(PickupItem item) if a GameObject was defined as callback target
        validPos[index] = true;

        if(PickupIsMine)
        {
            Player.instance.SetState(Player.PlayerState.PlayingGame);
            Player.instance.MoveTo(spawnPlayerPos[index].position, spawnPlayerPos[index].rotation);
        }
    }
}
