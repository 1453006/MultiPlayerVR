// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace DaydreamElements.ObjectManipulation
{

    /// Used for responding to pointer events, and implementing a movable object.
    public abstract class BaseInteractiveObject : Photon.MonoBehaviour,
    IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
    {

        public enum MotionMode { Transform, Rigidbody }
        public enum ObjectState { None, Selected, Released }
        public enum Axis { X, Y, Z }

        /// This controls whether the Gvr pointer is automatically
        /// assigned as the control transform. True by default.
        [Tooltip("Should we automatically assign the control transform to the pointer.")]
        public bool automaticallyAssignPointer = true;

        /// The control transform that manipulated objects will follow.
        [Tooltip("The transform objects should follow.")]
        public Transform controlTransform;

        private ObjectState state = ObjectState.None;

        private int lastStateChangeFrame = -1;

        private Vector2 lastTouchpadPosition;

        /// The total touchpad motion since startup.
        private Vector2 totalTouchpadMotionSinceSelection;

        private Quaternion initialControllerOrientation;
        private Quaternion inverseControllerOrientation;

        protected const float NORMALIZATION_EPSILON = 0.00001f;

        //datld: Photon
        #region PUN
        public bool PickupIsMine;
        public bool SentPickup;
        public int currentOwnerID = -1;


        [PunRPC]
        public void PunPickup(PhotonMessageInfo msgInfo)
        {

            // when this client's RPC gets executed, this client no longer waits for a sent pickup and can try again
            if (msgInfo.sender.IsLocal) this.SentPickup = false;

            this.PickupIsMine = msgInfo.sender.IsLocal;


            if (this.PickupIsMine)
            {
                //picked up this object
                photonView.TransferOwnership(PhotonNetwork.player.ID);
                currentOwnerID = PhotonNetwork.player.ID;
                //Select();
            }


        }

        [PunRPC]
        public void PunDrop(PhotonMessageInfo msgInfo)
        {

            this.PickupIsMine = false;
            currentOwnerID = -1;
            //Deselect();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(currentOwnerID);
            }
            else
            {
                currentOwnerID = (int)stream.ReceiveNext();

            }

        }

        #endregion



        /// The current state of the object (selected / released).
        public BaseInteractiveObject.ObjectState State
        {
            get
            {
                return state;
            }
        }

        /// Check to see if the pointer is hovering over this object.
        public bool Hover { get; private set; }

        /// The sum total of touchpad deltas since we began selecting an object.
        public Vector2 TotalTouchpadMotionSinceSelection
        {
            get
            {
                return totalTouchpadMotionSinceSelection;
            }
        }

        /// The controller transform orientation in world space at time of selection.
        public Quaternion InitialControllerOrientation
        {
            get
            {
                return initialControllerOrientation;
            }
        }

        /// The inverse controller transform orientation in world space at time of selection.
        public Quaternion InverseControllerOrientation
        {
            get
            {
                return inverseControllerOrientation;
            }
        }

        /// The current world space position of the controller transform.
        public Vector3 ControlPosition
        {
            get
            {
                return controlTransform.position;
            }
        }

        /// The current world space rotation of the controller transform.
        public Quaternion ControlRotation
        {
            get
            {
                return controlTransform.rotation;
            }
        }

        /// The current world space forward vector of the controller transform.
        public Vector3 ControlForward
        {
            get
            {
                return controlTransform.forward;
            }
        }

        /// The current change in controller world space rotation from start of selection.
        public Quaternion GetDeltaRotation()
        {
            return controlTransform.rotation * inverseControllerOrientation;
        }

        void OnEnable()
        {
            
            Reset();
        }

        void OnDisable()
        {
            Reset();
        }

        // We must check in update because it's possible to miss events if we don't click on the object.
        void Update()
        {
            if (state == ObjectState.Selected && GvrPointerInputModule.Pointer != null)
            {
                if (GvrPointerInputModule.Pointer.TriggerDown)
                {
                    Deselect();
                }
                else
                {
                    Drag();
                }
            }
            else if (state == ObjectState.Released)
            {
                Reset();
            }
        }

        // GvrEventSystem may not throw click events in the correct frame (unverified...), so we respond on Down.
        public void OnPointerDown(PointerEventData data)
        {
            /// if main player is not ready then return 
            if (Player.instance.currentState != Player.PlayerState.Selecting)
                return;

            // If the state is ready for selection, select the object.
            if (state == ObjectState.None)
            {
                if (currentOwnerID != -1)
                    return;
                Select();
                // Otherwise, try to deselect it.
            }
            else if (state == ObjectState.Selected)
            {
                if (currentOwnerID != -1)
                    return;
                Deselect();

            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //Player.instance.SetState(Player.PlayerState.None);
        }

        public void OnPointerEnter(PointerEventData data)
        {

            if (Player.instance.currentState != Player.PlayerState.None)
                return;

            Hover = true;
            Player.instance.currentState = Player.PlayerState.Selecting;
        }

        public void OnPointerExit(PointerEventData data)
        {
            if (Player.instance.currentState != Player.PlayerState.Selecting)
                return;
            Hover = false;
            Player.instance.SetState(Player.PlayerState.None);
        }

        protected virtual void OnSelect()
        {
            Player.instance.currentState = Player.PlayerState.Selecting;

            photonView.TransferOwnership(PhotonNetwork.player.ID);
            currentOwnerID = PhotonNetwork.player.ID;

            totalTouchpadMotionSinceSelection = Vector2.zero;
            initialControllerOrientation = controlTransform.rotation;

            // Store inverse initial controller quaternion, for performance.
            inverseControllerOrientation = Quaternion.Inverse(initialControllerOrientation);

            // Mark state change, and frame of state change.
            state = ObjectState.Selected;
            lastStateChangeFrame = Time.frameCount;
        }

        protected virtual void OnDeselect()
        {

            state = ObjectState.Released;
            lastStateChangeFrame = Time.frameCount;

            currentOwnerID = -1;


        }

        protected virtual void OnDrag()
        {
        }

        protected virtual void OnTouchDown()
        {
        }

        protected virtual void OnTouchUpdate(Vector2 deltaPosition)
        {
        }



        protected virtual void OnReset()
        {
        }

        // Try to select this object.
        private void Select()
        {


            if (ObjectManipulationPointer.IsObjectSelected())
            {
                return;
            }
            // This can be null in start, so let's just check here.
            if (automaticallyAssignPointer)
            {
                if (GvrPointerInputModule.Pointer != null)
                {
                    controlTransform = GvrPointerInputModule.Pointer.PointerTransform;
                }
            }

            if (controlTransform == null)
            {
                return;
            }

            // Only select the object if it's in the correct state, and the state didn't change this frame.
            if (state != ObjectState.None || lastStateChangeFrame == Time.frameCount)
            {
                return;
            }

            OnSelect();
        }

        // If there is a selected object, deselect it.
        protected void Deselect()
        {

            // Only deselect the object if it's selected, and we didn't select it this frame.
            if (state != ObjectState.Selected || lastStateChangeFrame == Time.frameCount)
            {
                return;
            }


            OnDeselect();
        }

        // Clear all state and mark the object as ready for selection.
        private void Reset()
        {
            state = ObjectState.None;
            lastStateChangeFrame = -1;
            currentOwnerID = -1;
            OnReset();
        }

        private void Drag()
        {
            if (controlTransform == null)
            {
                Reset();
                return;
            }

            // On a new touch, record the start position.
            if (GvrControllerInput.TouchDown)
            {
                lastTouchpadPosition = GvrControllerInput.TouchPosCentered;
                OnTouchDown();
                // Otherwise, calculate the touchpad drag distance.
            }
            else if (GvrControllerInput.IsTouching)
            {
                Vector2 currentPosition = GvrControllerInput.TouchPosCentered;

                // Compute delta position since last frame.
                Vector2 deltaPosition = currentPosition - lastTouchpadPosition;
                totalTouchpadMotionSinceSelection += deltaPosition;
                lastTouchpadPosition = currentPosition;
                OnTouchUpdate(deltaPosition);
            }

            OnDrag();
        }
    }
}