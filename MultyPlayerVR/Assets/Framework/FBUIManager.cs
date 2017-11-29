using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FBUIManager : MonoBehaviour
{
	/// <summary>
	/// list of all uis
	/// </summary>
	public Dictionary<string, FBBaseUI> uiList = new Dictionary<string, FBBaseUI>();

	/// <summary>
	/// ui canvas
	/// </summary>
	public Canvas canvas;

	/// <summary>
	/// instance
	/// </summary>
	public static FBUIManager instance;

    void Awake()
	{
		instance = this;

		// keep ui for other scenes
		GameObject.DontDestroyOnLoad(canvas.gameObject);

		// add all ui to list
		FBBaseUI[] childUIs = canvas.GetComponentsInChildren<FBBaseUI>();
		for (int i = 0; i < childUIs.Length; i++)
			this.uiList[childUIs[i].gameObject.name] = childUIs[i];
	}

	#region ui stack

	/// <summary>
	/// ui display stack
	/// </summary>
	List<FBBaseUI> uiStack = new List<FBBaseUI>(5);

	/// <summary>
	/// returns ui with specified name
	/// </summary>
	/// <param name="name"></param>
	/// <returns>ui with specified name</returns>
	public FBBaseUI getUIByName(string name)
	{
		FBBaseUI ui = null;
		uiList.TryGetValue(name, out ui);
		return ui;
	}

	/// <summary>
	/// shows ui
	/// </summary>
	/// <param name="ui">ui to show</param>
	public void showUI(FBBaseUI ui)
	{
		if(!uiStack.Contains(ui))
			uiStack.Add(ui);
		ui.show();
	}

	/// <summary>
	/// shows ui by name
	/// </summary>
	/// <param name="name">ui name to show</param>
	public void showUI(string name)
	{
		FBBaseUI ui = getUIByName(name);
		showUI(ui);
	}

	/// <summary>
	/// hides ui
	/// </summary>
	/// <param name="ui">ui to hide</param>
	public void hideUI(FBBaseUI ui)
	{
		uiStack.Remove(ui);
		ui.hide();
	}

	/// <summary>
	/// hides ui by name
	/// </summary>
	/// <param name="name">ui name to hide or null to hide all</param>
	public void hideUI(string name = null)
	{
		if(name == null)
		{
			for (int i = 0; i < uiStack.Count; i++)
				uiStack[i].hide();
			uiStack.Clear();
			return;
		}

		FBBaseUI ui = getUIByName(name);
		hideUI(ui);
	}

	/// <summary>
	/// replaces current ui with new ui
	/// </summary>
	/// <param name="ui">new ui</param>
	public void replaceUI(FBBaseUI ui)
	{
		hideUI();
		showUI(ui);
	}

	/// <summary>
	/// replaces current ui with new ui
	/// </summary>
	/// <param name="name">new ui name</param>
	public void replaceUI(string name)
	{
		hideUI();
		showUI(name);
	}

	#endregion

	#region scene objects

	/// <summary>
	/// find objects defined in SceneObject and attach ui components so they can be interactive
	/// </summary>
	/// <param name="scene">current scene</param>
	/// <param name="root">root object to search, if not specified this function will search all objects, which may be slow</param>
	public void initSceneInteractiveObjects(Scene scene, GameObject[] rootObjects = null)
	{
		List<FBClassObject> interactiveObjects = FBGameData.instance.getClassData("UIObject").getObjects("sceneName", new FBValue(scene.name));
		if (interactiveObjects.Count <= 0)
			return;

		if (rootObjects == null)
			rootObjects = scene.GetRootGameObjects();

		// use a dictionary to cache items for faster searching
		Dictionary<string, FBUIEx> objectDict = new Dictionary<string, FBUIEx>();

		for (int i = 0; i < interactiveObjects.Count; i++)
		{
			string objectName = interactiveObjects[i].getFieldValue("objectName").stringValue;

			// find
			FBUIEx fbuiex = null;
			objectDict.TryGetValue(objectName, out fbuiex);
			if (fbuiex == null)
			{
				GameObject interactiveObject = null;
				for (int j = 0; j < rootObjects.Length; j++)
				{
					Transform transform = rootObjects[j].transform.findChildRecursively(objectName);
					if (transform)
					{
						interactiveObject = transform.gameObject;
						break;
					}
				}
				if (interactiveObject)
				{
					fbuiex = interactiveObject.addMissingComponent<FBUIEx>();
					objectDict[objectName] = fbuiex;
				}
			}

			if (fbuiex)
			{
				// set trigger and action
				fbuiex.addEvent(interactiveObjects[i].getFieldValue("trigger").intValue, interactiveObjects[i].getFieldValue("action").intValue, interactiveObjects[i].getFieldValue("param").stringValue);
				fbuiex.isModel3D = true;
			}
			else
				Debug.Log("FBUIManager::initSceneInteractiveObjects: object not found: " + objectName);
		}
	}

    #endregion

    //datld
    #region level Info

    //public void initLevel()
    //{
    //    Dictionary<int, FBClassObject> dicLevel = FBGameData.instance.getClassData("LevelInfo").objects;

    //    if (dicLevel.Count <= 0)
    //        return;

    //    if (!uiLevelSelect)
    //        return;

    //    //get level pattern
    //    GameObject levelpattern = uiLevelSelect.transform.findChildRecursively("level").gameObject;
    //    GameObject locklevelpattern = uiLevelSelect.transform.findChildRecursively("locklevel").gameObject;

    //    levelpattern.SetActive(false);
    //    locklevelpattern.SetActive(false);
    //    Debug.Log(dicLevel.Count);
    //    foreach(int key in dicLevel.Keys)
    //    {
    //        GameObject lv;
    //        if (dicLevel[key].getFieldValue("status").intValue == 1)
    //        {
    //            lv = Instantiate(levelpattern);
    //            GameObject stars = lv.transform.findChildRecursively("stars").gameObject;
    //            if(stars)
    //            {
    //                int count = 0;
    //                stars.gameObject.SetActive(true);
    //                foreach(Transform littlestar in stars.transform)
    //                    if(count < dicLevel[key].getFieldValue("stars").intValue)
    //                    {
    //                        littlestar.gameObject.SetActive(true);
    //                        count++;
    //                    }
                        
    //            }

    //        }
    //        else
    //        {
    //            lv = Instantiate(locklevelpattern);
    //            GameObject stars = lv.transform.findChildRecursively("stars").gameObject;
    //            stars.gameObject.SetActive(false);
    //        }

    //        lv.transform.findChildRecursively("levelName").GetComponent<Text>().text = dicLevel[key].getFieldValue("levelName").stringValue;
    //        //call level function here !!!
    //        lv.transform.GetComponentInChildren<Button>().onClick.AddListener(() => { FBScriptManager.instance.runScript("startBattle"); } ) ;
    //        lv.SetActive(true);
    //        lv.transform.parent = levelpattern.transform.parent;
    //        lv.transform.localScale = new Vector3(1, 1, 1);
    //        lv.transform.localPosition = Vector3.zero;
    //        lv.transform.localRotation = Quaternion.Euler(Vector3.zero);

    //    }

    //}
	#endregion

	#region ui settings

	//public Canvas uiLevelSelect;

	//public GameObject uiSettings;
	//public Canvas uiStartBattle;

	//GameObject settingPanel;
	//GameObject languagePanel;
	//Slider music, sound;
	//public void initSetting()
	//{
	//    settingPanel = uiSettings.transform.findChildRecursively("Setting").gameObject;
	//    languagePanel = uiSettings.transform.findChildRecursively("Languages").gameObject;
	//    getLanguage();
	//    music = uiSettings.transform.findChildRecursively("Music").gameObject.GetComponent<Slider>();
	//    sound = uiSettings.transform.findChildRecursively("Sound").gameObject.GetComponent<Slider>();        
	//    settingPanel.transform.findChildRecursively("Languages").GetComponent<Button>().onClick.AddListener(showLanguage);
	//    settingPanel.transform.findChildRecursively("Close").GetComponent<Button>().onClick.AddListener(hideSetting);
	//    languagePanel.transform.findChildRecursively("Close").GetComponent<Button>().onClick.AddListener(hideLanguage);
	//}

	//void getLanguage()
	//{
	//    for (int i = 0; i < FBTextManager.instance.languageFileNames.Length; i++)
	//    {           
	//        GameObject btns = new GameObject();          
	//        btns.transform.rotation = Quaternion.identity;

	//        string[] tmp = FBTextManager.instance.languageFileNames[i].Split('/');           
	//        btns.name = FBTextManager.instance.languageFileNames[i];
	//        btns.AddComponent<Image>().sprite = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/UI/Settings/" + tmp[1] + ".png", typeof(Sprite));

	//        btns.AddComponent<Button>();
	//        btns.GetComponent<Button>().onClick.AddListener(() => FBTextManager.instance.setCurrentLanguage(FBTextManager.instance.languageFileNames[i]));

	//        btns.transform.SetParent(languagePanel.transform.findChildRecursively("Content").gameObject.transform);
	//        btns.transform.localScale = languagePanel.transform.findChildRecursively("Content").gameObject.transform.localScale;
	//        btns.transform.rotation = languagePanel.transform.findChildRecursively("Content").gameObject.transform.rotation;

	//    }
	//}

	//void updateVolume()
	//{
	//    FBSoundManager.instance.bgmVolume *= music.value;
	//    FBSoundManager.instance.sfxVolume *= music.value;
	//}

	//public void showUISetting()
	//{
	//    settingPanel.SetActive(true);
	//}

	//void showLanguage()
	//{
	//    settingPanel.SetActive(false);
	//    languagePanel.SetActive(true);
	//}

	//void hideSetting()
	//{
	//    settingPanel.SetActive(false);
	//    languagePanel.SetActive(false);
	//}

	//void hideLanguage()
	//{
	//    languagePanel.SetActive(false);
	//    settingPanel.SetActive(true);
	//}
	#endregion

}
