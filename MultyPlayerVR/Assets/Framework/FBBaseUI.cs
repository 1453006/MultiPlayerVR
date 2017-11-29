using UnityEngine;
using DoozyUI;
using System.Collections.Generic;

public class FBBaseUI : MonoBehaviour
{
	/// <summary>
	/// DoozyUI component for fx, can be null
	/// </summary>
	public UIElement uiElement;

	/// <summary>
	/// (DoozyUI only) whether is ui is animating
	/// </summary>
	[HideInInspector]
	public bool isAnimating = false;

	/// <summary>
	/// reference to filled data
	/// </summary>
	protected List<FBClassObject> filledDataList;

	/// <summary>
	/// is visible
	/// </summary>
	public bool isVisible { get { if (uiElement) return uiElement.isVisible; else return gameObject.activeInHierarchy; } }

	void Start()
	{
		if (uiElement)
		{
			// register events to help detect animation state
			uiElement.useInAnimationsFinishEvents = true;
			uiElement.useOutAnimationsFinishEvents = true;
		}
		translateTexts();
	}

	#region ui

	/// <summary>
	/// show
	/// </summary>
	public virtual void show()
	{
		if (uiElement)
		{
			UIManager.ShowUiElement(uiElement.elementName);
			if(uiElement.AreInAnimationsEnabled)
			{
				isAnimating = true;
				uiElement.onInAnimationsFinish.AddListener(() => { isAnimating = false; });
			}
		}
		else
			gameObject.SetActive(true);
	}

	/// <summary>
	/// hide
	/// </summary>
	public virtual void hide()
	{
		if (uiElement)
		{
			UIManager.HideUiElement(uiElement.elementName);
			if(uiElement.AreOutAnimationsEnabled)
			{
				isAnimating = true;
				uiElement.onOutAnimationsFinish.AddListener(() => { isAnimating = false; });
			}
		}
		else
			gameObject.SetActive(false);
	}

	/// <summary>
	/// translate texts
	/// </summary>
	public void translateTexts()
	{
		FBUIEx[] listUIex = GetComponentsInChildren<FBUIEx>(true);
		for (int i = 0; i < listUIex.Length; i++)
			listUIex[i].translateText();
	}

	#endregion

	#region data

	/// <summary>
	/// fills data from an FBClassObject
	/// </summary>
	/// <param name="data">source data</param>
	/// <param name="destName">name of destination child object</param>
	/// <returns>true on success, false on failure</returns>
	public bool fillData(FBClassObject data, string destName)
	{
		GameObject dest = transform.Find(destName).gameObject;
		if (dest)
			return fillData(data, dest);
		return false;
	}

	/// <summary>
	/// fills data from an FBClassObject
	/// </summary>
	/// <param name="data">source data</param>
	/// <param name="dest">destination child object</param>
	/// <returns>true on success, false on failure</returns>
	public bool fillData(FBClassObject data, GameObject dest = null)
	{
		// if dest is not specified, fill data to self
		if (dest == null)
			dest = gameObject;

		FBUIEx[] list = dest.GetComponentsInChildren<FBUIEx>(true);
		if (list == null || list.Length <= 0)
			return false;

		// call fillData() on each element
		for (int i = 0; i < list.Length; i++)
			list[i].fillData(data);

		return true;
	}

	/// <summary>
	/// fills data from an FBClassData
	/// </summary>
	/// <param name="data">source data</param>
	/// <param name="destName">name of destination child object</param>
	/// <returns>true on success, false on failure</returns>
	public bool fillDataList(FBClassData data, string destName)
	{
		List<FBClassObject> list = new List<FBClassObject>(data.objects.Count);
		foreach(KeyValuePair<int, FBClassObject> p in data.objects)
			list.Add(p.Value);
		return fillDataList(list, destName);
	}

	/// <summary>
	/// fills data from an FBClassData
	/// </summary>
	/// <param name="data">source data</param>
	/// <param name="dest">destination child object</param>
	/// <returns>true on success, false on failure</returns>
	public bool fillDataList(FBClassData data, GameObject dest = null)
	{
		List<FBClassObject> list = new List<FBClassObject>(data.objects.Count);
		foreach (KeyValuePair<int, FBClassObject> p in data.objects)
			list.Add(p.Value);
		return fillDataList(list, dest);
	}

	/// <summary>
	/// fills data from a list of FBClassObject
	/// </summary>
	/// <param name="dataList">source data</param>
	/// <param name="destName">name of destination child object</param>
	/// <returns>true on success, false on failure</returns>
	public bool fillDataList(List<FBClassObject> dataList, string destName)
	{
		return fillDataList(dataList, destName == null ? null : transform.Find(destName).gameObject);
	}

	/// <summary>
	/// fills data from a list of FBClassObject
	/// </summary>
	/// <param name="dataList">source data</param>
	/// <param name="dest">destination child object</param>
	/// <returns>true on success, false on failure</returns>
	public virtual bool fillDataList(List<FBClassObject> dataList, GameObject dest = null)
	{
		// if dest is not specified, fill data to self
		if (dest == null)
			dest = gameObject;

		Transform destTransform = dest.transform;
		GameObject itemPrefab = destTransform.GetChild(0).gameObject;

		// fill
		int i = 0;
		while (i < dataList.Count)
		{
			GameObject item = null;
			if (i < destTransform.childCount)
				item = destTransform.GetChild(i).gameObject;
			else
			{
				item = GameObject.Instantiate(itemPrefab);
				item.transform.SetParent(destTransform);
				item.transform.localScale = Vector3.one;
			}

			item.transform.position = Vector3.zero;
			item.SetActive(true);
			fillData(dataList[i], item);
			i++;
		}

		// hide remaining items
		while (i < destTransform.childCount)
		{
			dest.transform.GetChild(i).gameObject.SetActive(false);
			i++;
		}

		filledDataList = dataList;
		return true;
	}

	#endregion
}
