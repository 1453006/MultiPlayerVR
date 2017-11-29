using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FBTextManager : MonoBehaviour
{
	private Dictionary<string, string> vars = new Dictionary<string, string>();

	public string[] languageFileNames;
	public static FBTextManager instance;

	void Awake()
	{
		instance = this;
		setCurrentLanguage(languageFileNames[0]);
	}

	/// <summary>
	/// Sets the current language.
	/// </summary>
	/// <param name="fileName">language file name</param>
	public void setCurrentLanguage(string fileName)
	{
		TextAsset txtAsset = Resources.Load<TextAsset>(fileName);
		StringReader reader = new StringReader(txtAsset.text);
		string line;
		while ((line = reader.ReadLine()) != null)
		{
			string[] test = line.Split('\t');
			vars.Add(test[0], test[1]);
		}
	}

	/// <summary>
	/// Gets the text.
	/// </summary>
	/// <returns>The text.</returns>
	/// <param name="id">text id</param>
	public string getText(string id)
	{
		string value = null;
		vars.TryGetValue(id, out value);
		return value;
	}
}
