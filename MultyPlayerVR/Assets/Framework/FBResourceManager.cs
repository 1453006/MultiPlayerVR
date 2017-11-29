using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBResourceManager : MonoBehaviour
{
	public static FBResourceManager instance;

	void Awake()
	{
		instance = this;

		// inactive, call startLoading() to activate
		gameObject.SetActive(false);
	}

	void Update()
	{
		updateLoading();
	}

	#region loading machine

	int loadingIdx = 0;
	List<string> loadings = new List<string>(100);
	string loadingParams = null;

	/// <summary>
	/// adds a loading command
	/// </summary>
	/// <param name="loadingCommand">command</param>
	/// <param name="loadingParams">params</param>
	public void addLoading(string loadingCommand, string loadingParams)
	{
		loadings.Add(loadingCommand);
		loadings.Add(loadingParams);
		if (!gameObject.activeInHierarchy)
			startLoading();
	}

	/// <summary>
	/// starts or resumes loading
	/// </summary>
	public void startLoading()
	{
		gameObject.SetActive(true);
	}

	/// <summary>
	/// cancel loading, remove all steps
	/// </summary>
	public void cancelLoading()
	{
		gameObject.SetActive(false);
		loadings.Clear();
		loadingIdx = 0;
		loadingParams = null;
		Resources.UnloadUnusedAssets();
	}

	/// <summary>
	/// pauses loading
	/// </summary>
	public void pauseLoading()
	{
		gameObject.SetActive(false);
	}

	/// <summary>
	/// resumes loading
	/// </summary>
	public void resumeLoading()
	{
		gameObject.SetActive(true);
	}

	/// <summary>
	/// returns current loading progress in percent
	/// </summary>
	/// <returns>current loading progress in percent</returns>
	public int getLoadingProgress()
	{
		if (loadings.Count <= 0)
			return 0;
		return (loadingIdx * 100 / loadings.Count);
	}

	/// <summary>
	/// returns current loading state
	/// </summary>
	/// <returns>true if loading is finished, false otherwise</returns>
	public bool isLoadingFinished()
	{
		return (loadingIdx >= loadings.Count);
	}

	/// <summary>
	/// update loading process
	/// </summary>
	void updateLoading()
	{
		if (isLoadingFinished())
		{
			cancelLoading();
			return;
		}

		string loadingCommand = loadings[loadingIdx++];
		loadingParams = loadings[loadingIdx++];
		Invoke(loadingCommand, 0);
	}

	#endregion

	#region loading commands

	#endregion
}
