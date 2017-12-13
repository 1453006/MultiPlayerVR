using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SceneObject : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{

    public static SceneObject instance;
    private void Awake()
    {
        instance = this;
    }

    #region common
    public enum Trigger
    {      
        HOVER = 1,
        EXIT,
        CLICK,
        TOUCH,
        START,
        BEGINDRAG,
        DRAGGING,
        ENDDRAG,
        DROP,
        SWIPE,
        NONE,
    }

    public enum Action
    {        
        RUNSCRIPT = 1,
        PLAYSOUND,
        STOPSOUND,
        PAUSESOUND,
        RESUMESOUND,
        SHOWTEXT,
        HIDETEXT,
        SHOWVIDEO,
        HIDEVIDEO,
        NONE
    }

    [System.Serializable]
    public struct SceneObjectEvent
    {
        public Trigger trigger1, trigger2, trigger3;
        public Action action1, action2, action3;
        public string param1, param2, param3;

        public SceneObjectEvent(Trigger trigger1, Action action1, string param1,
                                Trigger trigger2, Action action2, string param2,
                                Trigger trigger3, Action action3, string param3)
        {
            this.trigger1 = trigger1; this.trigger2 = trigger2; this.trigger3 = trigger3;
            this.action1 = action1; this.action2 = action2; this.action3 = action3;
            this.param1 = param1; this.param2 = param2; this.param3 = param3;
        }
    }

    public SceneObjectEvent sceneObjectEvent;

    public void addEvent(int trigger1, int trigger2, int trigger3,
                         int action1, int action2, int action3,
                         string param1,string param2, string param3)
    {
        sceneObjectEvent.trigger1 =(Trigger)trigger1;
        sceneObjectEvent.trigger2 = (Trigger)trigger2;
        sceneObjectEvent.trigger3 = (Trigger)trigger3;
        sceneObjectEvent.action1 = (Action)action1;
        sceneObjectEvent.action2 = (Action)action2; ;
        sceneObjectEvent.action3 = (Action)action3;
        sceneObjectEvent.param1 = param1;
        sceneObjectEvent.param2 = param2;
        sceneObjectEvent.param3 = param3;
    }
    #endregion

    #region elements

    public void OnPointerEnter(PointerEventData eventData)
    {          
        if (sceneObjectEvent.trigger1 == Trigger.HOVER)      
            implementAction(sceneObjectEvent.action1, sceneObjectEvent.param1);      
        if (sceneObjectEvent.trigger2 == Trigger.HOVER) 
            implementAction(sceneObjectEvent.action2, sceneObjectEvent.param2);    
        if (sceneObjectEvent.trigger3 == Trigger.HOVER)    
            implementAction(sceneObjectEvent.action3, sceneObjectEvent.param3);                
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (sceneObjectEvent.trigger1 == Trigger.EXIT)
            implementAction(sceneObjectEvent.action1, sceneObjectEvent.param1); 
        if (sceneObjectEvent.trigger2 == Trigger.EXIT) 
            implementAction(sceneObjectEvent.action2, sceneObjectEvent.param2);     
        if (sceneObjectEvent.trigger3 == Trigger.EXIT)   
            implementAction(sceneObjectEvent.action3, sceneObjectEvent.param3);    
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(sceneObjectEvent.trigger1 == Trigger.CLICK) 
            implementAction(sceneObjectEvent.action1, sceneObjectEvent.param1);
        if (sceneObjectEvent.trigger2 == Trigger.CLICK)
            implementAction(sceneObjectEvent.action2, sceneObjectEvent.param2);  
        if (sceneObjectEvent.trigger3 == Trigger.CLICK)    
            implementAction(sceneObjectEvent.action3, sceneObjectEvent.param3);      
    }

    void implementAction(Action action, string param)
    {
        if (action == Action.RUNSCRIPT) FBScriptManager.instance.runScript(param);
        if (action == Action.PLAYSOUND) SoundResonanceManager.instance.playSfx(this.gameObject, param);
        if (action == Action.PAUSESOUND) SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
        if (action == Action.RESUMESOUND) SoundResonanceManager.instance.resumeSound(this.gameObject);
        if (action == Action.STOPSOUND) SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);
        if (action == Action.SHOWTEXT) BaseUI.instance.ShowTextObject(sceneObjectEvent.param1, this.gameObject);
        if (action == Action.HIDETEXT) BaseUI.instance.HideTextObject(this.gameObject);
    }

    Vector2 touchDownPos;
    float swipeAmpToChangePage = 0.35f;


    #endregion


}
