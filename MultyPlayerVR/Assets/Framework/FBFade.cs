using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FBFade : MonoBehaviour
{
	/// <summary>
	/// shared instance
	/// </summary>
	public static FBFade instance = null;

	/// <summary>
	/// fade image
	/// </summary>
	public Image image;

	/// <summary>
	/// fade canvas
	/// </summary>
	public Canvas canvas;

	private void Awake()
	{
		instance = this;
		canvas.gameObject.SetActive(true);
		GameObject.DontDestroyOnLoad(this);
	}

	/// <summary>
	/// fade in
	/// </summary>
	/// <param name="duration">duration</param>
	/// <returns>IEnumerator</returns>
	public IEnumerator fadeInRoutine(float duration)
	{
		enableInput(false);
		canvas.gameObject.SetActive(true);
		setFadeAlpha(1);

		float timer = duration;
		while(timer > 0)
		{
			setFadeAlpha(Mathf.Lerp(0, 1, timer / duration));
			timer -= Time.deltaTime;
			yield return null;
		}

		setFadeAlpha(0);
		canvas.gameObject.SetActive(false);
		enableInput(true);
	}

	/// <summary>
	/// fade in
	/// </summary>
	/// <param name="duration">duration</param>
	public void fadeIn(float duration)
	{
		StartCoroutine(fadeInRoutine(duration));
	}

	/// <summary>
	/// fade out
	/// </summary>
	/// <param name="duration">duration</param>
	/// <param name="nextSceneName">scene to load after fading</param>
	/// <returns>IEnumerator</returns>
	public IEnumerator fadeOutRoutine(float duration, string nextSceneName = null)
	{
		enableInput(false);
		canvas.gameObject.SetActive(true);
		setFadeAlpha(0);

		float timer = 0;
		while (timer < duration)
		{
			setFadeAlpha(Mathf.Lerp(0, 1, timer / duration));
			timer += Time.deltaTime;
			yield return null;
		}

		setFadeAlpha(1);
		if (nextSceneName != null)
			SceneManager.LoadScene(nextSceneName);
	}

	/// <summary>
	/// fade out
	/// </summary>
	/// <param name="duration">duration</param>
	/// <param name="nextSceneName">scene to load after fading</param>
	public void fadeOut(float duration, string nextSceneName = null)
	{
		StartCoroutine(fadeOutRoutine(duration, nextSceneName));
	}

	/// <summary>
	/// load scene async and fade out when finished
	/// </summary>
	/// <param name="fadeDuration">fade duration</param>
	/// <param name="nextSceneName">scene to load</param>
	/// <param name="delay">delay before changing scene</param>
	/// <returns>IEnumerator</returns>
	public IEnumerator loadSceneAsyncAndFadeOutRoutine(string nextSceneName, float fadeDuration, float delay = 0)
	{
		enableInput(false);
		float startTime = Time.time;

		AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
		op.allowSceneActivation = false;
		while (op.progress < 0.9f)
			yield return new WaitForSeconds(0.5f);

		delay = startTime + delay - fadeDuration - Time.time;
		if (delay > 0)
			yield return new WaitForSeconds(delay);

		yield return fadeOutRoutine(fadeDuration);
		op.allowSceneActivation = true;
	}

	/// <summary>
	/// load scene async and fade out when finished
	/// </summary>
	/// <param name="fadeDuration">fade duration</param>
	/// <param name="nextSceneName">scene to load</param>
	/// <param name="delay">delay before changing scene</param>
	public void loadSceneAsyncAndFadeOut(string nextSceneName, float fadeDuration, float delay = 0)
	{
		StartCoroutine(loadSceneAsyncAndFadeOutRoutine(nextSceneName, fadeDuration, delay));
	}

	void setFadeAlpha(float alpha)
	{
		image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
	}

	void enableInput(bool enable)
	{
		GameObject gvr = GameObject.Find("Gvr");
		if(gvr)
		{
			GameObject eventSystem = gvr.GetComponentInChildren<EventSystem>(true).gameObject;
			if (eventSystem)
				eventSystem.SetActive(enable);
		}
	}
}