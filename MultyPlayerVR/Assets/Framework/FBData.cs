
// enable: game will mix saved data with constants exported from FFTool
// disable: game will use saved data and ignore constants
//#define MIX_SAVED_DATA_WITH_CONSTANT_DATA

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;

#region FBDataType
/// <summary>
/// data types exported from tool
/// </summary>
public enum FBDataType
{
	None = 0,
	String = -1,
	Float = -2,
	Int = -3,
	Image = -4,
	Class1 = 1,
	Class2 = 2,
	// ...
}
#endregion

#region FBValue
/// <summary>
/// represents a value of an FBClassField
/// </summary>
public class FBValue
{
	/// <summary>
	/// int value
	/// </summary>
	public int intValue = 0;

	/// <summary>
	/// float value
	/// </summary>
	public float floatValue = 0;

	/// <summary>
	/// string value
	/// </summary>
	public string stringValue = null;

	/// <summary>
	/// data type
	/// </summary>
	public FBDataType dataType;

	/// <summary>
	/// empty ctor
	/// </summary>
	public FBValue() { }

	/// <summary>
	/// creates an in
	/// </summary>
	/// <param name="i">int</param>
	public FBValue(int i)
	{
		intValue = i;
		dataType = FBDataType.Int;
	}

	/// <summary>
	/// creates a float
	/// </summary>
	/// <param name="f">float</param>
	public FBValue(float f)
	{
		floatValue = f;
		dataType = FBDataType.Float;
	}

	/// <summary>
	/// creates a string
	/// if parse is true, this function automatically detects correct value type and parse to int, float...
	/// </summary>
	/// <param name="s">input string</param>
	/// <param name="parse">automatically detect correct value type and parse</param>
	public FBValue(string s, bool parse = false)
	{
		if (parse)
		{
			dataType = FBDataType.None;

			// try parsing to int
			try
			{
				intValue = int.Parse(s);
				dataType = FBDataType.Int;
			}
			catch
			{
				intValue = 0;
			}

			// try parsing to float
			if (s.Contains("."))
			{
				try
				{
					floatValue = float.Parse(s);
					dataType = FBDataType.Float;
				}
				catch
				{
					floatValue = intValue;
				}
			}
			else
				floatValue = intValue;

			// cannot parse
			if(dataType == FBDataType.None)
			{
				stringValue = s;
				dataType = FBDataType.String;
			}
		}
		else
		{
			stringValue = s;
			dataType = FBDataType.String;
		}
	}

	/// <summary>
	/// creates a class object id
	/// </summary>
	/// <param name="classId">class id</param>
	/// <param name="objId">object id</param>
	public FBValue(FBDataType classId, int objId)
	{
		intValue = objId;
		dataType = classId;
	}

	/// <summary>
	/// creates from raw json data
	/// </summary>
	/// <param name="dataType">data type</param>
	/// <param name="rawData">raw json data</param>
	public FBValue(FBDataType dataType, string rawData)
	{
		this.dataType = dataType;
		if (dataType == FBDataType.Float)
			floatValue = float.Parse(rawData);
		else if (dataType == FBDataType.Int || dataType >= FBDataType.Class1)
			intValue = int.Parse(rawData);
		else if (dataType == FBDataType.String || dataType == FBDataType.Image)
			stringValue = rawData;
	}

	/// <summary>
	/// returns a string to display on ui
	/// </summary>
	/// /// <param name="id2name">convert class object id to name if possible</param>
	/// <returns>string</returns>
	public string getDisplayString(bool id2name = false)
	{
		if (dataType >= FBDataType.Class1)
		{
			if(id2name)
			{
				// try to return object name instead of id
				FBClassData classData = FBGameData.instance.getClassData((int)dataType);
				FBClassObject classObject = classData.getObject(intValue);
				if (classObject == null)
					return intValue.ToString();
				FBValue val = classObject.getFieldValue("Name");
				if (val != null)
					return val.stringValue;
				val = classObject.getFieldValue("name");
				if (val != null)
					return val.stringValue;
			}
			return intValue.ToString();
		}
		if (dataType == FBDataType.Int)
			return intValue.ToString();
		if (dataType == FBDataType.Float)
			return floatValue.ToString();
		return stringValue;
	}

	/// <summary>
	/// checks if current value is equal to another value
	/// </summary>
	/// <param name="val">value to check</param>
	/// <returns>true if two values are equal, false otherwise</returns>
	public bool equals(FBValue val)
	{
		if (val.dataType != dataType)
			return false;

		if (dataType == FBDataType.Int || dataType >= FBDataType.Class1)
			return (val.intValue == intValue);

		if (dataType == FBDataType.Float)
			return (val.floatValue == floatValue);

		if (dataType == FBDataType.String)
			return (val.stringValue == stringValue);

		return false;
	}

	/// <summary>
	/// compares current value to another value
	/// </summary>
	/// <param name="val">value to compare</param>
	/// <returns>int.MinValue if two values cannot be compared, negative number if current value is smaller, zero if two values are equal, negative number if current value is larger</returns>
	public int compareTo(FBValue val)
	{
		if (val.dataType != dataType)
			return int.MinValue;

		if (dataType == FBDataType.Int || dataType >= FBDataType.Class1)
			return (intValue == val.intValue ? 0 : (intValue > val.intValue ? 1 : -1));

		if (dataType == FBDataType.Float)
			return (floatValue == val.floatValue ? 0 : (floatValue > val.floatValue ? 1 : -1));

		if (dataType == FBDataType.String)
			return stringValue.CompareTo(val.stringValue);

		return int.MinValue;
	}

	/// <summary>
	/// check whether an FBValue is null or emtpy
	/// </summary>
	/// <param name="val">value to check</param>
	/// <returns>true or false</returns>
	public static bool isNullOrEmpty(FBValue val)
	{
		return (val == null || val.dataType == FBDataType.None);
	}

	/// <summary>
	/// create a clone of current value
	/// </summary>
	/// <returns>cloned FBValue</returns>
	public FBValue clone()
	{
		return (FBValue)this.MemberwiseClone();
	}
}
#endregion

#region FBClassField
/// <summary>
/// represents a class field
/// </summary>
public class FBClassField
{
	/// <summary>
	/// field name
	/// </summary>
	public string name;

	/// <summary>
	/// data type
	/// </summary>
	public FBDataType dataType;

	/// <summary>
	/// ctor
	/// </summary>
	/// <param name="name">name</param>
	/// <param name="dataType">dataType</param>
	public FBClassField(string name, FBDataType dataType)
	{
		this.name = name;
		this.dataType = dataType;
	}
}
#endregion

#region FBClassDefinition
/// <summary>
/// definition of a class
/// </summary>
public class FBClassDefinition
{
	/// <summary>
	/// class id
	/// </summary>
	public int id;

	/// <summary>
	/// class name
	/// </summary>
	public string name;

	/// <summary>
	/// class fields, mapped by name, READONLY, DO NOT MODIFY!
	/// </summary>
	public Dictionary<string, FBClassField> fieldsByName = new Dictionary<string, FBClassField>();

	/// <summary>
	/// ctor
	/// </summary>
	/// <param name="id">id</param>
	/// <param name="name">name</param>
	public FBClassDefinition(int id, string name)
	{
		this.id = id;
		this.name = name;
	}

	/// <summary>
	/// adds a field
	/// </summary>
	/// <param name="field">field to add</param>
	/// <returns></returns>
	public bool setField(FBClassField field)
	{
		fieldsByName[field.name] = field;
		return true;
	}
	
	/// <summary>
	/// returns field with specified name
	/// </summary>
	/// <param name="name">field name</param>
	/// <returns>FBClassField object with specified name or null if not found</returns>
	public FBClassField getField(string name)
	{
		return fieldsByName[name];
	}
}
#endregion

#region FBClassObject
/// <summary>
/// manages data for a class object
/// </summary>
public class FBClassObject
{
	/// <summary>
	/// object id
	/// </summary>
	public int id;

	/// <summary>
	/// class id
	/// </summary>
	public int classId;

	/// <summary>
	/// list of field values, READONLY, DO NOT MODIFY!
	/// </summary>
	public Dictionary<string, FBValue> fieldValues = new Dictionary<string, FBValue>();

	///// <summary>
	///// ctor
	///// </summary>
	///// <param name="id">object id</param>
	///// <param name="classDef">parent class definition</param>
	//public FBClassObject(int id, int classId)
	//{
	//	this.id = id;
	//	this.classId = classId;
	//}

	/// ctor
	public FBClassObject() { }

	/// <summary>
	/// sets a field value by its name
	/// </summary>
	/// <param name="fieldName">field name</param>
	/// <param name="value">value</param>
	/// <returns>true on success, false on failure</returns>
	public bool setFieldValue(string fieldName, FBValue value)
	{
		fieldValues[fieldName] = value;

		FBClassData classData = FBGameData.instance.getClassData(classId);
		if (classData != null)
			classData.modified = true;

		return true;
	}

	/// <summary>
	/// returns a field value by its name
	/// </summary>
	/// <param name="fieldName">field name</param>
	/// <returns>value of specified field</returns>
	public FBValue getFieldValue(string fieldName)
	{
		FBValue fieldValue = null;
		fieldValues.TryGetValue(fieldName, out fieldValue);
		return fieldValue;
	}

	/// <summary>
	/// create a clone of current object
	/// </summary>
	/// <returns>cloned object</returns>
	public FBClassObject clone()
	{
		FBClassObject res = new FBClassObject();
		res.id = id;
		res.classId = classId;
		foreach(KeyValuePair<string, FBValue> p in fieldValues)
			res.fieldValues[p.Key] = p.Value.clone();
		return res;
	}
}
#endregion

#region FBClassData
/// <summary>
/// manages objects of a class
/// </summary>
public class FBClassData
{
	/// <summary>
	/// class definition
	/// </summary>
	[JsonIgnore]
	public FBClassDefinition classDef;

	/// <summary>
	/// object list, READONLY, DO NOT MODIFY!
	/// </summary>
	public Dictionary<int, FBClassObject> objects = new Dictionary<int, FBClassObject>();

	/// <summary>
	/// modified and needs to be saved
	/// </summary>
	[JsonIgnore]
	public bool modified = false;

	/// <summary>
	/// next object id
	/// </summary>
	public int nextObjectId = 1024 * 1024;

	/// <summary>
	/// ctor
	/// </summary>
	public FBClassData(FBClassDefinition classDef)
	{
		this.classDef = classDef;
	}

	/// <summary>
	/// create a new object of this class and add it
	/// </summary>
	/// <returns>added object</returns>
	public FBClassObject addObject()
	{
		FBClassObject obj = new FBClassObject();
		obj.id = nextObjectId;
		obj.classId = classDef.id;
		addObject(obj);
		return obj;
	}

	/// <summary>
	/// adds object
	/// </summary>
	/// <param name="obj">object</param>
	/// <returns>true on success, false on failure</returns>
	public bool addObject(FBClassObject obj)
	{
		objects[obj.id] = obj;
		if (obj.id >= nextObjectId)
			nextObjectId = obj.id + 1;
		modified = true;
		return true;
	}

	/// <summary>
	/// removes object
	/// </summary>
	/// <param name="id">id</param>
	/// <returns>true if object exists, false otherwise</returns>
	public bool removeObject(int id)
	{
		if(objects.Remove(id))
		{
			modified = true;
			return true;
		}
		return false;
	}

	/// <summary>
	/// gets object by id
	/// </summary>
	/// <param name="id">id</param>
	/// <returns>object with specified id or null if not found</returns>
	public FBClassObject getObject(int id)
	{
		FBClassObject res = null;
		objects.TryGetValue(id, out res);
		return res;
	}

	/// <summary>
	/// returns an object which matches the condition
	/// </summary>
	/// <param name="fieldName">field name</param>
	/// <param name="value">value to compare</param>
	/// <returns>object which matches the condition</returns>
	public FBClassObject getObject(string fieldName, FBValue value)
	{
		foreach (KeyValuePair<int, FBClassObject> p in objects)
		{
			if (p.Value.getFieldValue(fieldName).equals(value))
				return p.Value;
		}
		return null;
	}


	/// <summary>
	/// returns a list of objects which match the condition
	/// </summary>
	/// <param name="fieldName">field name</param>
	/// <param name="value">value to compare</param>
	/// <returns>list of objects which match the condition</returns>
	public List<FBClassObject> getObjects(string fieldName, FBValue value)
	{
		List<FBClassObject> list = new List<FBClassObject>();
		foreach(KeyValuePair<int, FBClassObject> p in objects)
		{
			if (p.Value.getFieldValue(fieldName).equals(value))
				list.Add(p.Value);
		}
		return list;
	}

	/// <summary>
	/// returns a list of objects which match the condition
	/// </summary>
	/// <param name="fieldNames">field names</param>
	/// <param name="values">field values</param>
	/// <returns>list of objects which match the condition</returns>
	public List<FBClassObject> getObjects(string[] fieldNames, FBValue[] values)
	{
		List<FBClassObject> list = new List<FBClassObject>();
		foreach (KeyValuePair<int, FBClassObject> p in objects)
		{
			FBClassObject obj = p.Value;
			bool equal = true;
			for(int i=0; i<fieldNames.Length; i++)
			{
				if (!obj.getFieldValue(fieldNames[i]).equals(values[i]))
				{
					equal = false;
					break;
				}
			}
			if(equal)
				list.Add(p.Value);
		}
		return list;
	}
}
#endregion

#region FBGameData
/// <summary>
/// manages all data
/// </summary>
public class FBGameData
{
	/// <summary>
	/// instance
	/// </summary>
	public static FBGameData instance = new FBGameData();

	/// <summary>
	/// all data, mapped by id
	/// </summary>
	Dictionary<int, FBClassData> classDataById = new Dictionary<int, FBClassData>();

	/// <summary>
	/// all data, mapped by name
	/// </summary>
	Dictionary<string, FBClassData> classDataByName = new Dictionary<string, FBClassData>();

	/// <summary>
	/// path to FFTool's resources folder
	/// </summary>
	public string pathToDatabaseResources;

	/// <summary>
	/// path to saved user data
	/// </summary>
	public string pathToSavedData = Application.persistentDataPath + "/";

	/// <summary>
	/// get all data of a class by its id
	/// </summary>
	/// <param name="id">class id</param>
	/// <returns>data of class with specified id</returns>
	public FBClassData getClassData(int id)
	{
		return classDataById[id];
	}

	/// <summary>
	/// get all data of a class by its name
	/// </summary>
	/// <param name="name">class name</param>
	/// <returns>data of class with specified name</returns>
	public FBClassData getClassData(string name)
	{
		return classDataByName[name];
	}

	/// <summary>
	/// add a new class data
	/// </summary>
	/// <param name="classData">class data to add</param>
	/// <returns>true on success, false on failure</returns>
	bool addClassData(FBClassData classData)
	{
		classDataById[classData.classDef.id] = classData;
		classDataByName[classData.classDef.name] = classData;
		return true;
	}

	/// <summary>
	/// loads all class definition
	/// </summary>
	/// <param name="json">json string</param>
	/// <returns>true on success, false on failure</returns>
	bool loadAllClassDefinition(string json)
	{
		JArray classDefinesJson = JsonConvert.DeserializeObject(json) as JArray;
		for (int i = 0; i < classDefinesJson.Count; i++)
		{
			JObject classDefineJson = classDefinesJson[i] as JObject;
			int classId = int.Parse((string)classDefineJson["id"]);
			string className = ((string)classDefineJson["name"]);
			JArray classFields = classDefineJson["data"] as JArray;
			FBClassDefinition classDefine = new FBClassDefinition(classId, className);
			for (int j = 0; j < classFields.Count; j++)
			{
				JObject classFieldJson = classFields[j] as JObject;
				string fieldName = ((string)classFieldJson["fieldName"]);
				FBDataType fieldType = (FBDataType)((int)classFieldJson["dataType"]);
				FBClassField classField = new FBClassField(fieldName, fieldType);
				classDefine.setField(classField);
			}
			FBClassData classData = new FBClassData(classDefine);
			addClassData(classData);
		}
		return true;
	}

	/// <summary>
	/// load all objects of a class
	/// </summary>
	/// <param name="json">json string</param>
	/// <returns>true on success, false on failure</returns>
	bool loadClassObjects(string json)
	{
		JObject classDataJson = JsonConvert.DeserializeObject(json) as JObject;
		FBClassData classData = getClassData((string)classDataJson["name"]);
		FBClassDefinition classDef = classData.classDef;
		JArray classObjectsJson = classDataJson["data"] as JArray;
		for (int i = 0; i < classObjectsJson.Count; i++)
		{
			JObject classObjectJson = classObjectsJson[i] as JObject;
			int id = int.Parse((string)classObjectJson["id"]);
			FBClassObject classObject = new FBClassObject();
			classObject.id = id;
			classObject.classId = classDef.id;
			foreach (KeyValuePair<string, FBClassField> p in classDef.fieldsByName)
			{
				FBValue val = new FBValue(p.Value.dataType, (string)classObjectJson[p.Value.name]);
				classObject.setFieldValue(p.Value.name, val);
			}
			classData.addObject(classObject);
		}
		classData.modified = false;
		return true;
	}

	/// <summary>
	/// load saved class objects
	/// </summary>
	/// <param name="classId">class id</param>
	/// <param name="json">json string</param>
	/// <returns>true on success, false on failure</returns>
	bool loadSavedClassObjects(int classId, string json)
	{
		FBClassData loadedClassData = JsonConvert.DeserializeObject<FBClassData>(json);
		if (loadedClassData == null)
			return false;

		FBClassData classData = FBGameData.instance.getClassData(classId);
#if MIX_SAVED_DATA_WITH_CONSTANT_DATA
		foreach(KeyValuePair<int, FBClassObject> p in loadedClassData.objects)
			classData.addObject(p.Value);
#else
		classData.objects = loadedClassData.objects;
#endif

		classData.nextObjectId = loadedClassData.nextObjectId;
		classData.modified = false;
		return true;
	}

	/// <summary>
	/// load game database generated by FFTool
	/// </summary>
	/// <param name="path">path to FFTool project</param>
	/// <returns>true on success, false on failure</returns>
	public bool loadGameDatabase(string path)
	{
		// clean up
		classDataById.Clear();
		classDataByName.Clear();

		pathToDatabaseResources = path + "/Resources/";
		string pathToDatabaseImplement = path + "/Implement/";
		
		// load all class definition
		TextAsset textAsset = Resources.Load<TextAsset>(path + "/define");
		loadAllClassDefinition(textAsset.text);
		Resources.UnloadAsset(textAsset);

		// load class objects
		foreach (KeyValuePair<string, FBClassData> p in classDataByName)
		{
#if MIX_SAVED_DATA_WITH_CONSTANT_DATA
			// constant
			textAsset = Resources.Load<TextAsset>(pathToDatabaseImplement + p.Key);
			if (textAsset)
			{
				loadClassObjects(textAsset.text);
				Resources.UnloadAsset(textAsset);
			}

			// saved data
			string json = File.ReadAllText(pathToSavedData + p.Key);
			if (!string.IsNullOrEmpty(json))
			{
				loadSavedClassObjects(p.Value.classDef.id, json);
			}
#else // MIX_SAVED_DATA_WITH_CONSTANT_DATA
			// load saved data
			bool loaded = false;
			string filePath = pathToSavedData + p.Key;
			if(File.Exists(filePath))
			{
				string json = File.ReadAllText(filePath);
				if (!string.IsNullOrEmpty(json))
				{
					loaded = loadSavedClassObjects(p.Value.classDef.id, json);
				}
			}
			if (!loaded)
			{
				// load constants if saved data does not exist
				textAsset = Resources.Load<TextAsset>(pathToDatabaseImplement + p.Key);
				if (textAsset)
				{
					loadClassObjects(textAsset.text);
					Resources.UnloadAsset(textAsset);
				}
			}
#endif // MIX_SAVED_DATA_WITH_CONSTANT_DATA
		}

		return true;
	}

	/// <summary>
	/// saves game database
	/// </summary>
	/// <param name="modifiedOnly">only save modified classes</param>
	/// <returns>true on success, false on failure</returns>
	public bool saveGameDatabase(bool modifiedOnly = true)
	{
		foreach(KeyValuePair<string, FBClassData> p in classDataByName)
		{
			if(p.Value.modified || !modifiedOnly)
			{
				string json = JsonConvert.SerializeObject(p.Value);
				File.WriteAllText(pathToSavedData + p.Key, json);
				p.Value.modified = false;
			}
		}
		return true;
	}
}
#endregion
