
// optimize:
// if isModel3D is true, FBUIEx.fillData and FBBaseUI.fillDataList wil perform some redundant tasks:
// - FBBaseUI.fillDataList instantiates the first child which already has a 3d model
// - FBUIEx.fillData destroys that model and creates its own model
// - FBUIEx.fillData clones a pooled object instead of directly using it
// => need a solution to remove redundant tasks

// - events are not implemented for 2d
// - input: gvr only, need wrapper for mouse, oculus

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class FBUIEx : MonoBehaviour
{
	#region basic

	/// <summary>
	/// name of FBDataField to fill
	/// </summary>
	public string fieldName;

	/// <summary>
	/// text id used for translation
	/// </summary>
	public string textId;

	/// <summary>
	/// text object
	/// </summary>
	Text text;

	/// <summary>
	/// image object
	/// </summary>
	Image image;

	/// <summary>
	/// animator
	/// </summary>
	Animator animator;

	/// <summary>
	/// data to fill
	/// </summary>
	public FBClassObject data;

	void Awake()
	{
		text = GetComponent<Text>();
		image = GetComponent<Image>();
		animator = GetComponent<Animator>();
	}

	private void Start()
	{
        initEvents();
		onEvent1(Trigger.START);
	}

	/// <summary>
	/// fills data from an FBClassObject
	/// </summary>
	/// <param name="data">source data</param>
	/// <returns>true on success, false on failure</returns>
	public bool fillData(FBClassObject data)
	{
		this.data = data;

		if (string.IsNullOrEmpty(fieldName))
			return false; // not a field

		FBValue val = data.getFieldValue(fieldName);
		if (val == null)
			return false; // not this field

		if (text)
			text.text = val.getDisplayString(true);

		if (image && val.dataType == FBDataType.Image)
		{
			string imageName = FBGameData.instance.pathToDatabaseResources + val.stringValue;
			imageName = imageName.Substring(0, imageName.LastIndexOf("."));
			image.sprite = Resources.Load<Sprite>(imageName);
		}

		if (isModel3D)
		{
			// TODO: clean up and optimize
			// clean up
			transform.destroyChildren();

			// since we are going to modify the model, we must clone a pooled object instead of directly using it
			GameObject poolObject = FBPoolManager.instance.getPoolObject(val.stringValue);
			model3D = GameObject.Instantiate<GameObject>(poolObject, transform);
			FBPoolManager.instance.returnObjectToPool(poolObject);
			model3D.transform.localPosition = Vector3.zero;
            //model3D.transform.localRotation = Quaternion.identity;
            //model3D.transform.localScale = Vector3.one;
            model3D.transform.localRotation = Quaternion.Euler(model3D.transform.localRotation.eulerAngles.x, model3D.transform.localRotation.eulerAngles.y - 90, model3D.transform.localRotation.eulerAngles.z);

			// cannot activate 3d model here
			// script components will run and mess up
			Invoke("activateModel3D", 0.1f); // model3D.SetActive(true);

			// only keep ui components
			string[] componentsToKeep = new string[] { "Transform", "MeshRenderer", "MeshFilter" };
			Component[] components = model3D.GetComponentsInChildren<Component>(true);
			for (int i = 0; i < components.Length; i++)
			{
				string typeName = components[i].GetType().Name;
				bool remove = true;
				for (int j = 0; j < componentsToKeep.Length; j++)
					if (typeName == componentsToKeep[j])
					{
						remove = false;
						break;
					}
				if (remove)
					GameObject.Destroy(components[i] as UnityEngine.Object);
			}

			model3DSize = 0;
		}

		return true;
	}

	/// <summary>
	/// set component text according to current language
	/// </summary>
	/// <returns>true on success, false on failure</returns>
	public bool translateText()
	{
		if(!string.IsNullOrEmpty(textId))
		{
			string translatedText = FBTextManager.instance.getText(textId);

			if (text)
				text.text = translatedText;

			return true;
		}
		return false;
	}

	#endregion

	#region 3d model

	/// <summary>
	/// whether or not this game object is a 3d model
	/// </summary>
	public bool isModel3D;

	/// <summary>
	/// reference to the 3d model
	/// </summary>
	GameObject model3D;

	/// <summary>
	/// size of 3d model, used for resizing
	/// </summary>
	float model3DSize = 0;

	/// <summary>
	/// original scale, used for resizing
	/// </summary>
	float model3DScale;

	/// <summary>
	/// set the size of 3d model, useful for unifying object sizes in a list
	/// </summary>
	/// <param name="size">size</param>
	public void setModel3DSize(float size)
	{
		if(model3DSize <= 0)
		{
			MeshRenderer[] meshRenderers = model3D.GetComponentsInChildren<MeshRenderer>(true);
			Vector3 center = Vector3.zero;
			Vector3 size2 = Vector3.zero;
			for (int i = 0; i < meshRenderers.Length; i++)
			{
				MeshRenderer m = meshRenderers[i];

				BoxCollider boxCollider = m.gameObject.addMissingComponent<BoxCollider>();
				boxCollider.isTrigger = true;

				center += m.bounds.center;
				size2 = Vector3.Max(size2, m.bounds.size);
			}
			center /= meshRenderers.Length;
			model3D.transform.position += model3D.transform.position - center;
			model3DSize = Mathf.Max(size2.x, size2.y, size2.z);
			model3DScale = transform.localScale.x;
			model3D.transform.parent.gameObject.layer = 0;
		}

		float scale = size * model3DScale / model3DSize;
		transform.localScale = new Vector3(scale, scale, scale);
	}

	void activateModel3D()
	{
		model3D.SetActive(true);
	}

	#endregion

	#region event0

	/// <summary>
	/// event delegate
	/// </summary>
	/// <param name="type">type</param>
	/// <param name="obj">source object</param>
	/// <param name="param">params</param>
	public delegate void eventAction(EventTriggerType type, FBUIEx obj, params object[] param);

	/// <summary>
	/// event
	/// </summary>
	public event eventAction onEvent;

	/// <summary>
	/// status
	/// </summary>
	bool isDragging = false;

	/// <summary>
	/// store the position before dragging to revert in case user cancels dragging
	/// </summary>
	Vector3 positionBeforeDragging;

	/// <summary>
	/// store the scale before dragging to revert in case user cancels dragging
	/// </summary>
	float scaleBeforeDragging;

	/// <summary>
	/// store the rotation before dragging to revert in case user cancels dragging
	/// </summary>
	Quaternion rotationBeforeDragging;

	/// <summary>
	/// drag tag
	/// </summary>
	public string[] dragTag;

	/// <summary>
	/// drop tag
	/// </summary>
	public string[] dropTag;

	/// <summary>
	/// can drag
	/// </summary>
	public bool canDrag { get { return (dragTag != null && dragTag.Length > 0); } }

	/// <summary>
	/// can drop
	/// </summary>
	public bool canDrop { get { return (dropTag != null && dropTag.Length > 0); } }

	/// <summary>
	/// object being dragged
	/// </summary>
	static FBUIEx dragObject;

	/// <summary>
	/// object that can be dropped on
	/// </summary>
	static FBUIEx dropObject;

	/// <summary>
	/// create collider and triggers
	/// </summary>
	void initEvents()
	{
		if (isModel3D || canDrop)
		{
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
			if(meshRenderer || canDrop)
			{
				BoxCollider boxCollider = GetComponentInChildren<BoxCollider>();
				if (!boxCollider)
					boxCollider = gameObject.addMissingComponent<BoxCollider>();
				boxCollider.isTrigger = true;
				boxCollider.gameObject.layer = 0;

				// feedback jira#249
				if (canDrop && dropTag[0] == "marker_body")
					boxCollider.size = Vector3.one * 2.5f;
			}

            EventTrigger eventTrigger = gameObject.addMissingComponent<EventTrigger>();
			eventTrigger.triggers.Clear();

			EventTriggerType[] types = { EventTriggerType.PointerClick, EventTriggerType.PointerDown, EventTriggerType.PointerUp, EventTriggerType.PointerEnter, EventTriggerType.PointerExit };
			for (int i = 0; i < types.Length; i++)
			{
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = types[i];
				entry.callback.AddListener((eventData) => { onEvent0(entry.eventID); });
				eventTrigger.triggers.Add(entry);
			}
		}
		else
		{
			Button button = GetComponent<Button>();
			if (button && button.onClick.GetPersistentEventCount() <= 0)
				button.onClick.AddListener(() => { onEvent0(EventTriggerType.PointerClick); });
		}

   
    }

	/// <summary>
	/// handle events internally before broadcasting
	/// </summary>
	/// <param name="type">type</param>
	void onEvent0(EventTriggerType type)
	{
		if (onEvent != null)
			onEvent(type, this);

		onEvent1(type);

		// drag
		if (canDrag)
		{
          
            if (type == EventTriggerType.PointerDown)
			{
				isDragging = true;
				dragObject = this;
				dropObject = null;
				positionBeforeDragging = transform.position;
				scaleBeforeDragging = transform.localScale.x;
				rotationBeforeDragging = transform.localRotation;
               
				onEvent(EventTriggerType.BeginDrag, this);
				onEvent1(EventTriggerType.BeginDrag);
			}

			if (type == EventTriggerType.PointerUp)
			{
				isDragging = false;
				dragObject = null;
				if (dropObject)
				{
					transform.DOKill();
					transform.position = positionBeforeDragging;
					transform.localScale = Vector3.one * scaleBeforeDragging;
					transform.localRotation = rotationBeforeDragging;
					onEvent(EventTriggerType.Drop, this, dropObject);
					onEvent1(EventTriggerType.Drop);
					dropObject = null;
				}
				else
				{
					transform.DOMove(positionBeforeDragging, 0.25f);
					transform.DOScale(scaleBeforeDragging, 0.25f);
					transform.localRotation = rotationBeforeDragging;
					onEvent(EventTriggerType.EndDrag, this);
					onEvent1(EventTriggerType.EndDrag);
				}
			}
		}

		// drop
		if (canDrop)
		{
			if (type == EventTriggerType.PointerEnter && dragObject != null)
			{
				if(dragObject.canDropOn(this))
				{
					dropObject = this;

					// gd feedback: should drop asap
					dragObject.onEvent0(EventTriggerType.PointerUp);
				}
			}
			if (type == EventTriggerType.PointerExit && dropObject == this)
				dropObject = null;
		}
	}

	/// <summary>
	/// check if we can drop current object on obj
	/// </summary>
	/// <param name="obj">obj to drop</param>
	/// <returns>true if droppable, false otherwise</returns>
	public bool canDropOn(FBUIEx obj)
	{
		if (obj.dropTag == null || obj.dropTag.Length <= 0)
			return false;

		for (int i = 0; i < obj.dropTag.Length; i++)
			for (int j = 0; j < dragTag.Length; j++)
				if (obj.dropTag[i] == dragTag[j])
					return true;
		return false;
	}

	void Update()
	{
		if (isDragging)
		{
			//// follow pointer
			//GvrLaserPointer laserPointer = GvrLaserPointer.instance;

			//// feedback jira#250
			////float dist = Vector3.Distance(positionBeforeDragging, laserPointer.LineStartPoint) - 2;
			//float dist = Vector3.Distance(HomeScene.instance.truck.transform.position, laserPointer.LineStartPoint) + 1;

			//Vector3 pos = laserPointer.LineStartPoint + (laserPointer.LineEndPoint - laserPointer.LineStartPoint).normalized * dist;
			//transform.DOMove(pos, 0.15f);
			//transform.DOScale(1, 0.15f);
			//transform.localRotation = GvrController.Orientation;
		}
	}

	#endregion

	#region event1

	/// <summary>
	/// trigger enumeration, MUST match UITrigger id in FFData
	/// </summary>
	public enum Trigger
	{
		START = 1,
		POINTER_ENTER,
		POINTER_EXIT,
		POINTER_DOWN,
		POINTER_UP,
		BEGIN_DRAG,
		DROP,
		POINTER_CLICK,
		END_DRAG,
	}

	/// <summary>
	/// action enumeration, MUST match UIAction id in FFData
	/// </summary>
	public enum Action
	{
		PLAY_SFX = 1,
		STOP_SFX,
		PLAY_ANIM,
		STOP_ANIM,
		RUN_SCRIPT,
	}

	[System.Serializable]
	public struct FBUIExEvent
	{
		public Trigger trigger;
		public Action action;
		public string param;

		public FBUIExEvent(Trigger trigger, Action action, string param)
		{
			this.trigger = trigger;
			this.action = action;
			this.param = param;
		}
	}

	/// <summary>
	/// trigger mapping
	/// </summary>
	static Dictionary<EventTriggerType, Trigger> systemTrigger2customTrigger = new Dictionary<EventTriggerType, Trigger>()
	{
		{EventTriggerType.PointerEnter, Trigger.POINTER_ENTER},
		{EventTriggerType.PointerExit, Trigger.POINTER_EXIT},
		{EventTriggerType.PointerDown, Trigger.POINTER_DOWN},
		{EventTriggerType.PointerUp, Trigger.POINTER_UP},
		{EventTriggerType.BeginDrag, Trigger.BEGIN_DRAG},
		{EventTriggerType.Drop, Trigger.DROP},
		{EventTriggerType.PointerClick, Trigger.POINTER_CLICK},
		{EventTriggerType.EndDrag, Trigger.END_DRAG},
	};

	/// <summary>
	/// event list
	/// </summary>
	public List<FBUIExEvent> eventList = new List<FBUIExEvent>();

	/// <summary>
	/// add an event handler
	/// </summary>
	/// <param name="trigger">trigger</param>
	/// <param name="action">action</param>
	/// <param name="param">param</param>
	public void addEvent(Trigger trigger, Action action, string param)
	{
		eventList.Add(new FBUIExEvent(trigger, action, param));
	}

	/// <summary>
	/// add an event handler
	/// </summary>
	/// <param name="trigger">trigger</param>
	/// <param name="action">action</param>
	/// <param name="param">param</param>
	public void addEvent(int trigger, int action, string param)
	{
		eventList.Add(new FBUIExEvent((Trigger)trigger, (Action)action, param));
	}

	/// <summary>
	/// handle an event
	/// </summary>
	/// <param name="type">type</param>
	void onEvent1(EventTriggerType type)
	{
		onEvent1(systemTrigger2customTrigger[type]);
	}

	/// <summary>
	/// handle an event
	/// </summary>
	/// <param name="type">type</param>
	void onEvent1(Trigger trigger)
	{
		int count = eventList.Count;
		for (int i = 0; i < count; i++)
		{
			FBUIExEvent eventData = eventList[i];
			if(eventData.trigger == trigger)
			{
				if(eventData.action == Action.PLAY_SFX)
				{
                    //FBSoundManagerWwise.instance.playEvent(eventData.param, this.gameObject);
				}
				else if (eventData.action == Action.STOP_SFX)
				{
                    //FBSoundManagerWwise.instance.stopEvent(eventData.param, this.gameObject, 1);
				}
				else if (eventData.action == Action.PLAY_ANIM)
				{
					animator.Play(eventData.param);
				}
				else if (eventData.action == Action.STOP_ANIM)
				{
					animator.StopPlayback();
				}
				else if (eventData.action == Action.RUN_SCRIPT)
				{
					FBScriptManager.instance.runScript(eventData.param);
				}
			}
		}
	}

	#endregion
}
