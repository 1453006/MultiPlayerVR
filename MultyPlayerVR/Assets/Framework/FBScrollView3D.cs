
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class FBScrollView3D : FBBaseUI
{
	/// <summary>
	/// distance between items
	/// </summary>
	public float itemSpaceX = 50;

	/// <summary>
	/// distance between items
	/// </summary>
	public float itemSpaceY = 50;

	/// <summary>
	/// distance between items
	/// </summary>
	public float itemSpaceZ = 0;

	/// <summary>
	/// unified item sizes
	/// </summary>
	public float itemSize = 0;

	/// <summary>
	/// items layout direction
	/// </summary>
	public enum Direction
	{
		HORIZONTAL = 0,
		VERTICAL,
	}
	public Direction direction = Direction.HORIZONTAL;

	/// <summary>
	/// item container, will be set to self if not specified
	/// </summary>
	GameObject container = null;

	/// <summary>
	/// fill data list
	/// </summary>
	/// <param name="dataList">data list</param>
	/// <param name="dest">destination</param>
	/// <returns>true on success, false on failure</returns>
	public override bool fillDataList(List<FBClassObject> dataList, GameObject dest = null)
	{
		container = (dest == null ? gameObject : dest);
		bool res = base.fillDataList(dataList, container);
		updateLayout();
		initItemEvents();
		return res;
	}

	/// <summary>
	/// update layout
	/// </summary>
	void updateLayout()
	{
		if (!container)
			return;

		Transform containerTransform = container.transform;
		int count = filledDataList.Count;

		pagesCount = count / (rowsCount * columnsCount);
		if (count % (rowsCount * columnsCount) != 0)
			pagesCount++;
		if (currentPage < 0 || currentPage >= pagesCount)
			currentPage = 0;

		float x = 0, y = 0, z = 0;
		int col = 0, row = 0;
		int pageItemsCount = rowsCount * columnsCount;
		int si = currentPage * pageItemsCount;
		int ei = si + pageItemsCount;
		if (ei > count)
			ei = count;

		for (int i = 0; i < count; i++)
		{
			Transform child = containerTransform.GetChild(i);

			if (i < si || i >= ei)
			{
				// hide item in other pages
				child.gameObject.SetActive(false);
				continue;
			}

			child.gameObject.SetActive(true);
			child.localPosition = new Vector3(x, y, z);
			child.localRotation = Quaternion.identity;

			if (direction == Direction.HORIZONTAL)
			{
				col++;
				if (col >= columnsCount)
				{
					col = 0;
					x = 0;
					y += itemSpaceY;
				}
				else
					x += itemSpaceX;
			}
			else // if (direction == Direction.VERTICAL)
			{
				row++;
				if (row >= rowsCount)
				{
					row = 0;
					y = 0;
					x += itemSpaceX;
				}
				else
					y += itemSpaceY;
			}

			if(itemSize != 0)
			{
				// unify item size
				FBUIEx[] fbuiex = child.gameObject.GetComponentsInChildren<FBUIEx>(true);
				for (int j = 0; j < fbuiex.Length; j++)
					if (fbuiex[j].isModel3D)
						fbuiex[j].setModel3DSize(itemSize);
			}
		}

		updatePageIndicators();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		// update layout to reflect changes in editor
		updateLayout();
	}
#endif

	private void Start()
	{
		if(background)
		{
			BoxCollider boxCollider = background.addMissingComponent<BoxCollider>();
			boxCollider.isTrigger = true;

			EventTrigger eventTrigger = background.addMissingComponent<EventTrigger>();
			eventTrigger.triggers.Clear();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) => { onPointerEnter(); });
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) => { onPointerExit(); });
			eventTrigger.triggers.Add(entry);
		}
	}

	private void Update()
	{
		if (isHover)
		{
#if UNITY_HAS_GOOGLEVR
			// check for swipe action
			if (GvrController.TouchDown)
			{
				touchDownPos = GvrController.TouchPos;
			}
			if (GvrController.TouchUp)
			{
				float offset = 0;
				if (direction == Direction.HORIZONTAL)
					offset = GvrController.TouchPos.x - touchDownPos.x;
				else // if(direction == Direction.VERTICAL)
					offset = GvrController.TouchPos.y - touchDownPos.y;
				if (offset >= swipeAmpToChangePage)
					prevPage();
				if (offset <= -swipeAmpToChangePage)
					nextPage();
			}
#endif
		}
	}

	#region paging

	/// <summary>
	/// columns count
	/// </summary>
	public int columnsCount = 3;

	/// <summary>
	/// rows count
	/// </summary>
	public int rowsCount = 3;

	/// <summary>
	/// current page
	/// </summary>
	int currentPage = 0;

	/// <summary>
	/// pages count
	/// </summary>
	int pagesCount = 0;

	/// <summary>
	/// container of page indicators
	/// </summary>
	public GameObject pageIndicators;

	/// <summary>
	/// distance of page indicators
	/// </summary>
	public float pageIndicatorsDistance = 0.8f;

	/// <summary>
	/// next page
	/// </summary>
	void nextPage()
	{
		if (currentPage < pagesCount - 1)
		{
			currentPage++;
			updateLayout();
			onItemEvent0(EventTriggerType.Scroll, null, 1);
		}
	}

	/// <summary>
	/// prev page
	/// </summary>
	void prevPage()
	{
		if (currentPage > 0)
		{
			currentPage--;
			updateLayout();
			onItemEvent0(EventTriggerType.Scroll, null, -1);
		}
	}

	/// <summary>
	/// update display of page indicators
	/// </summary>
	void updatePageIndicators()
	{
		if (!pageIndicators)
			return;

		if(pagesCount <= 1)
		{
			pageIndicators.SetActive(false);
			return;
		}

		pageIndicators.SetActive(true);
		Transform piTransform = pageIndicators.transform;
		float w = pageIndicatorsDistance * (pagesCount - 1);
		float x = -w / 2;
		int i = 0;
		while(i < pagesCount)
		{
			GameObject obj = null;
			if (i >= piTransform.childCount)
				obj = GameObject.Instantiate(piTransform.GetChild(0).gameObject, piTransform);
			else
				obj = piTransform.GetChild(i).gameObject;
			obj.SetActive(true);

			Vector3 pos = obj.transform.localPosition;
			pos.x = x;
			obj.transform.localPosition = pos;
			obj.transform.DOScale(i == currentPage ? 1 : 0.5f, 0.2f);

			x += pageIndicatorsDistance;
			i++;
		}

		while (i < piTransform.childCount)
		{
			piTransform.GetChild(i).gameObject.SetActive(false);
			i++;
		}
	}

	/// <summary>
	/// background, used to check for hover
	/// </summary>
	public GameObject background;

	/// <summary>
	/// only scroll if hovered
	/// </summary>
	bool isHover = false;

	void onPointerEnter()
	{
		isHover = true;
	}

	void onPointerExit()
	{
		isHover = false;
	}

#if UNITY_HAS_GOOGLEVR
	/// <summary>
	/// touched down position, used to detect swipe action
	/// </summary>
	Vector2 touchDownPos;

	/// <summary>
	/// swipe amplitude to change page
	/// </summary>
	public float swipeAmpToChangePage = 0.35f;
#endif

	#endregion

	#region event

	/// <summary>
	/// event delegate
	/// </summary>
	/// <param name="type">type</param>
	/// <param name="obj">source object</param>
	/// <param name="param">params</param>
	public delegate void itemEventAction(EventTriggerType type, FBUIEx obj, params object[] param);

	/// <summary>
	/// event
	/// </summary>
	public event itemEventAction onItemEvent;

	/// <summary>
	/// handle item events
	/// </summary>
	void initItemEvents()
	{
		FBUIEx[] fbuiex = container.GetComponentsInChildren<FBUIEx>(true);
		for (int i = 0; i < fbuiex.Length; i++)
		{
			fbuiex[i].onEvent -= onItemEvent0;
			fbuiex[i].onEvent += onItemEvent0;
		}
	}

	/// <summary>
	/// handle item events internally before broadcasting
	/// </summary>
	/// <param name="type">type</param>
	/// <param name="obj">item</param>
	/// <param name="param">param</param>
	private void onItemEvent0(EventTriggerType type, FBUIEx obj, params object[] param)
	{
		if (onItemEvent != null)
			onItemEvent(type, obj, param);
	}

	#endregion
}
