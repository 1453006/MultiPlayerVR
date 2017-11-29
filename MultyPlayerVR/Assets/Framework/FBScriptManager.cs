using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBScriptManager : MonoBehaviour
{
	/// <summary>
	/// scripts files
	/// </summary>
	public List<TextAsset> scriptFiles;

	/// <summary>
	/// a prefab to instantiate new script object when neccessary
	/// </summary>
	public FBScript scriptPrefab;

	/// <summary>
	/// instance
	/// </summary>
	public static FBScriptManager instance;

	void Awake()
	{
		instance = this;
	}

	/// <summary>
	/// runs a script
	/// </summary>
	/// <param name="text">script content</param>
	/// <returns>FBScript object</returns>
	public FBScript runScript(string text)
	{
		GameObject obj = null;
		FBScript script = null;

		// try to reuse existing script object
		for(int i=0; i<transform.childCount; i++)
		{
			GameObject obj2 = transform.GetChild(i).gameObject;
			if (!obj2.activeInHierarchy)
			{
				script = obj2.GetComponent<FBScript>();
				if(script)
				{
					obj = obj2;
					break;
				}
			}
		}

		if(!obj)
		{
			// create new
			obj = GameObject.Instantiate(scriptPrefab.gameObject);
			obj.transform.SetParent(transform);
			script = obj.GetComponent<FBScript>();
		}

		script.manager = this;
		script.run(text);
		return script;
	}

	/// <summary>
	/// runs a script file
	/// </summary>
	/// <param name="name">file name</param>
	/// <returns>true on success, false on failure</returns>
	public FBScript runScriptFile(string name)
	{
		// try to run from loaded asset
		FBScript script = null;
		for (int i = 0; i < scriptFiles.Count; i++)
		{
			if (scriptFiles[i].name == name)
			{
				script = runScript(scriptFiles[i].text);
				script.gameObject.name = name;
				return script;
			}
		}

		// if not found, load from resources folder and run
		TextAsset textAsset = Resources.Load<TextAsset>(name);
		if(textAsset)
		{
			script = runScript(textAsset.text);
			script.gameObject.name = name;
			Resources.UnloadAsset(textAsset);
		}

		return script;
	}

	/// <summary>
	/// called when a script is finished
	/// </summary>
	/// <param name="script">script object</param>
	public void onScriptFinished(FBScript script)
	{
	
	} 
}
