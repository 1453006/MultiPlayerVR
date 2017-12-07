using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObjectManager : MonoBehaviour {

    public static SceneObjectManager instance;

    private void Awake()
    {
        instance = this;
    }   

    #region common  
    FBClassData sceneObject;
    List< FBClassObject> interactiveObjects;

    public void initSceneInteractiveObjects(UnityEngine.SceneManagement.Scene scene, GameObject[] rootObjects = null)
    {
        if (rootObjects == null)
            rootObjects = scene.GetRootGameObjects();
        initInteractiveObjects(scene.name, rootObjects);        
    }

    public void initInteractiveObjects(string sceneName, GameObject[] rootObjects)
    {
        sceneObject = FBGameData.instance.getClassData("UIObject");
        List<FBClassObject> interactiveObjects = sceneObject.getObjects("SceneName", new FBValue(sceneName));

        if (interactiveObjects.Count <= 0)
            return;

        Dictionary<string, SceneObject> objectDict = new Dictionary<string, SceneObject>();

        for(int i = 0; i < interactiveObjects.Count; i++)
        {
            string objectName = interactiveObjects[i].getFieldValue("ObjectName").stringValue;

            SceneObject scnObj = null;
            objectDict.TryGetValue(objectName, out scnObj);
            if(scnObj == null)
            {
                GameObject interactiveObject = null;
                for (int j = 0; j < rootObjects.Length; j++)
                {
                    Transform transform;
                    if (rootObjects[j].name == objectName)
                        transform = rootObjects[j].transform;
                    else
                        transform = rootObjects[j].transform.findChildRecursively(objectName);
                    if (transform)
                    {
                        interactiveObject = transform.gameObject;
                        break;
                    }
                }
                if (interactiveObject)
                {
                    scnObj = interactiveObject.addMissingComponent<SceneObject>();
                    objectDict[objectName] = scnObj;
                }
            }
            if (scnObj)
            {
                // set trigger and action
                scnObj.sceneObjectEvent.triggerHover = scnObj.triggerTypeToID(interactiveObjects[i].getFieldValue("TriggerHover").stringValue);
                scnObj.sceneObjectEvent.triggerExit = scnObj.triggerTypeToID(interactiveObjects[i].getFieldValue("TriggerExit").stringValue);
                scnObj.sceneObjectEvent.triggerClick = scnObj.triggerTypeToID(interactiveObjects[i].getFieldValue("TriggerClick").stringValue);
                scnObj.sceneObjectEvent.actionHover = scnObj.actionTypeToID(interactiveObjects[i].getFieldValue("ActionHover").stringValue);
                scnObj.sceneObjectEvent.actionExit = scnObj.actionTypeToID(interactiveObjects[i].getFieldValue("ActionExit").stringValue);
                scnObj.sceneObjectEvent.actionClick = scnObj.actionTypeToID(interactiveObjects[i].getFieldValue("ActionClick").stringValue);
                scnObj.sceneObjectEvent.paramHover = interactiveObjects[i].getFieldValue("ParamHover").stringValue;
                scnObj.sceneObjectEvent.paramExit = interactiveObjects[i].getFieldValue("ParamExit").stringValue;
                scnObj.sceneObjectEvent.paramClick = interactiveObjects[i].getFieldValue("ParamClick").stringValue;

            }
        }
    }

    #endregion
}
