using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DoozyUI;
using DG.Tweening;

public class FBScript : MonoBehaviour
{
	#region misc

	/// <summary>
	/// script file to run
	/// if script file is not specified, you must call run() manually
	/// </summary>
	public TextAsset scriptFile;

	/// <summary>
	/// manager of this script, can be null
	/// </summary>
	public FBScriptManager manager;

	/// <summary>
	/// all commands and their parameters
	/// </summary>
	FBValue[][] data;

	/// <summary>
	/// current command and its parameters
	/// </summary>
	FBValue[] cmdData;

	/// <summary>
	/// current command index
	/// </summary>
	int cmdIdx;

	/// <summary>
	/// commands count
	/// </summary>
	int cmdCount;

	/// <summary>
	/// can proceed to next command
	/// </summary>
	bool nextCmd;

	/// <summary>
	/// command timer
	/// </summary>
	float timer;

	/// <summary>
	/// state parameters
	/// </summary>
	float paramf00;

	void Start()
	{
		if (scriptFile)
		{
			gameObject.name = scriptFile.name;
			run(scriptFile.text);
		}
		else
			gameObject.SetActive(false);
	}

	void Update()
	{
		if (nextCmd)
		{
			nextCmd = false;
			nextCommand();
		}
		if(gameObject.activeInHierarchy)
		{
			runCommand(data[cmdIdx]);
			timer += Time.deltaTime;
		}
	}

	/// <summary>
	/// executes a script
	/// </summary>
	/// <param name="text">script content to run</param>
	/// <returns>true on success, false on failure</returns>
	public bool run(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			onFinished();
			return false;
		}

		cmdIdx = cmdCount = 0;
		timer = 0;
		nextCmd = false;
		cmdData = null;

		string[] lines = text.Split('\n');
		int linesCount = lines.Length;
		data = new FBValue[linesCount][];

		for (int i = 0; i < linesCount; i++)
		{
			string[] paramTexts = lines[i].Trim().Split(' ');
			if (paramTexts == null || paramTexts.Length <= 0)
				continue; // empty line

			int paramsCount = 0;
			while (paramsCount < paramTexts.Length
				&& paramTexts[paramsCount].Length > 0
				&& !paramTexts[paramsCount].StartsWith("//"))
				paramsCount++;
			if (paramsCount <= 0)
				continue; // line commented out

			// parse line content
			data[cmdCount] = new FBValue[paramsCount];
			for (int j = 0; j < paramsCount; j++)
				data[cmdCount][j] = new FBValue(paramTexts[j], true);

			cmdCount++;
		}

		if (cmdCount <= 0)
		{
			onFinished();
			return false;
		}

		gameObject.SetActive(true);
		return true;
	}

	/// <summary>
	/// runs specified command
	/// </summary>
	/// <param name="cmdData">command to run and its parameters</param>
	void runCommand(FBValue[] cmdData)
	{
		this.cmdData = cmdData;

		string cmd = cmdData[0].stringValue;
		if (cmd.EndsWith(":"))
			nextCmd = true; // skip label
		else
		{
			// consider using try/catch to detect missing impementation
			Invoke(cmd, 0);
		}
	}

	/// <summary>
	/// proceeds to next command, reset temporary fields
	/// </summary>
	void nextCommand()
	{
		timer = 0;
		cmdIdx++;
		if (cmdIdx >= cmdCount)
			onFinished();
	}

	/// <summary>
	/// handles execution error
	/// </summary>
	/// <param name="message">error message</param>
	void onError(string message)
	{
		onFinished();
	}

	/// <summary>
	/// handles finish event
	/// </summary>
	void onFinished()
	{
		gameObject.SetActive(false);
		if (manager)
			manager.onScriptFinished(this);
	}

	/// <summary>
	/// checks if a parameter exists
	/// </summary>
	/// <param name="name">parameter name</param>
	/// <returns>true if exist, false if not</returns>
	bool hasParam(string name)
	{
		string name2 = name + "=";
		for (int i = 1; i < cmdData.Length; i++)
			if (cmdData[i].stringValue == name || cmdData[i].stringValue.StartsWith(name2))
				return true;
		return false;
	}

	/// <summary>
	/// returns a string parameter with specified name
	/// </summary>
	/// <param name="name">parameter name</param>
	/// <returns>parameter value or null if not found</returns>
	string getStringParam(string name)
	{
		name += "=";
		for (int i = 1; i < cmdData.Length; i++)
		{
			if (cmdData[i].stringValue.StartsWith(name))
				return cmdData[i].stringValue.Substring(name.Length);
		}
		return null;
	}

	/// <summary>
	/// returns an int parameter with specified name
	/// </summary>
	/// <param name="name">parameter name</param>
	/// <returns>parameter value or int.MinValue if not found</returns>
	int getIntParam(string name)
	{
		string s = getStringParam(name);
		if (s == null)
			return int.MinValue;
		try { return int.Parse(s); }
		catch { }
		return int.MinValue;
	}

	/// <summary>
	/// returns a float parameter with specified name
	/// </summary>
	/// <param name="name">parameter name</param>
	/// <returns>parameter value or float.MinValue if not found</returns>
	float getFloatParam(string name)
	{
		string s = getStringParam(name);
		if (s == null)
			return float.MinValue;
		try { return float.Parse(s); }
		catch { }
		return float.MinValue;
	}

	#endregion

	#region script command implementation

	/// <summary>
	/// jumps to the label with specified name
	/// </summary>
	/// <returns>true on success, false if label not found</returns>
	bool jumpTo()
	{
		string label = cmdData[1].stringValue + ":";
		for (int i = 0; i < cmdCount; i++)
		{
			if (data[i][0].stringValue == label)
			{
				cmdIdx = i;
				return true;
			}
		}
		onError("label not found: " + label);
		return false;
	}

	/// <summary>
	/// shows ui
	/// </summary>
	/// <returns>true on success, false on failure</returns>
	bool showUI()
	{
		FBUIManager uiMgr = FBUIManager.instance;
		FBBaseUI ui = uiMgr.getUIByName(cmdData[1].stringValue);
		if(ui)
		{
			if(!ui.isVisible)
				uiMgr.showUI(ui);
			else if(!ui.isAnimating)
				nextCmd = true;
			return true;
		}
		onError("ui not found: " + cmdData[1].stringValue);
		return false;
	}

	/// <summary>
	/// hides ui
	/// </summary>
	/// <returns>true on success, false on failure</returns>
	bool hideUI()
	{
		FBUIManager uiMgr = FBUIManager.instance;
		if (cmdData.Length < 2)
		{
			// ui name not specified => hide all
			uiMgr.hideUI();
			return true;
		}

		FBBaseUI ui = uiMgr.getUIByName(cmdData[1].stringValue);
		if (ui)
		{
			if (!ui.isVisible)
				nextCmd = true;
			else if (!ui.isAnimating)
				uiMgr.hideUI(ui);
			return true;
		}
		onError("ui not found: " + cmdData[1].stringValue);
		return false;
	}

	/// <summary>
	/// replaces current ui with new ui
	/// </summary>
	/// <returns>true on success, false on failure</returns>
	bool replaceUI()
	{
		FBUIManager uiMgr = FBUIManager.instance;
		FBBaseUI ui = uiMgr.getUIByName(cmdData[1].stringValue);
		if (ui)
		{
			if (!ui.isVisible)
				uiMgr.replaceUI(ui);
			else if (!ui.isAnimating)
				nextCmd = true;
			return true;
		}
		onError("ui not found: " + cmdData[1].stringValue);
		return false;
	}

	/// <summary>
	/// waits for an amount of time
	/// </summary>
	void delay()
	{
		if (timer >= cmdData[1].floatValue)
			nextCmd = true;
	}

	float startWatch_time;
	void startWatch()
	{
		startWatch_time = Time.time;
		nextCmd = true;
	}

	/// <summary>
	/// waits for an amount of time elapsed since startWatch
	/// </summary>
	void stopWatch()
	{
		if (Time.time - startWatch_time >= cmdData[1].floatValue)
			nextCmd = true;
	}

	void filterItems()
	{
		//HomeScene.instance.onFilterItemTouched(cmdData[1].stringValue);
		nextCmd = true;
	}

	void removeTruckItems()
	{
		//HomeScene.instance.onRemoveItems();
		nextCmd = true;
	}

    void openDrawer()
    {
        //HomeScene.instance.global_openDrawer();
        nextCmd = true;
    }
    
    void closeDrawer()
    {
        //HomeScene.instance.global_closeDrawer();
        nextCmd = true;
    }

    void test()
    {
        Debug.Log(cmdData[1].stringValue);
        nextCmd = true;
    }
    #endregion

}
