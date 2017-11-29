using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class InteractiveObject : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    
    //event system
    public void OnPointerDown(PointerEventData eventData)
    {
        this.GetComponent<Renderer>().material.color = Color.yellow;
        Debug.Log("OnPointerDown");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.GetComponent<Renderer>().material.color = Color.blue;
        Debug.Log("OnPointerEnter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.GetComponent<Renderer>().material.color = Color.red;
        Debug.Log("OnPointerExit");
    }
}
