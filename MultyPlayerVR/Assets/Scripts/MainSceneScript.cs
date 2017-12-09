using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneScript : MonoBehaviour {

    private void Awake()
    {
        FBGameData.instance.loadGameDatabase("Data");
    }
    // Use this for initialization
    void Start () {
        SceneObjectManager.instance.initSceneInteractiveObjects(this.gameObject.scene);

	}
	
	// Update is called once per frame

}
