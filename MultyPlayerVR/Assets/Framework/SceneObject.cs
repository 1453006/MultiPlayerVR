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
        NONE,
    }

    public enum Action
    {        
        RUNSCRIPT = 1,
        PLAYSOUND,
        STOPSOUND,
        PAUSESOUND,
        RESUMESOUND,
        NONE
    }
      
   public Trigger triggerTypeToID(string name)
    {
        if (name == "Hover") return Trigger.HOVER;
        if (name == "Exit") return Trigger.EXIT;
        if (name == "Click") return Trigger.CLICK;
        return Trigger.NONE;
    }
   public Action actionTypeToID(string name)
    {
        if (name == "runScript") return Action.RUNSCRIPT;
        if (name == "playSound") return Action.PLAYSOUND;
        if (name == "pauseSound") return Action.PAUSESOUND;
        if (name == "resumeSound") return Action.RESUMESOUND;
        if (name == "stopSound") return Action.STOPSOUND;
        return Action.NONE;
    }

    [System.Serializable]
    public struct SceneObjectEvent
    {
        public Trigger triggerHover, triggerExit, triggerClick;
        public Action actionHover, actionExit, actionClick;
        public string paramHover, paramExit, paramClick;

        public SceneObjectEvent(Trigger trigger1, Action action1, string param1,
                                Trigger trigger2, Action action2, string param2,
                                Trigger trigger3, Action action3, string param3)
        {
            this.triggerHover = trigger1; this.triggerExit = trigger2; this.triggerClick = trigger3;
            this.actionHover = action1; this.actionExit = action2; this.actionClick = action3;
            this.paramHover = param1; this.paramExit = param2; this.paramClick = param3;
        }
    }

    public SceneObjectEvent sceneObjectEvent;

    public void addEvent(int trigger1, int trigger2, int trigger3,
                         int action1, int action2, int action3,
                         string param1,string param2, string param3)
    {
        sceneObjectEvent.triggerHover =(Trigger)trigger1;
        sceneObjectEvent.triggerExit = (Trigger)trigger2;
        sceneObjectEvent.triggerClick = (Trigger)trigger3;
        sceneObjectEvent.actionHover = (Action)action1;
        sceneObjectEvent.actionExit = (Action)action2; ;
        sceneObjectEvent.actionClick = (Action)action3;
        sceneObjectEvent.paramHover = param1;
        sceneObjectEvent.paramExit = param2;
        sceneObjectEvent.paramClick = param3;
    }
    #endregion

    #region elements
    public void OnPointerEnter(PointerEventData eventData)
    {    
        
        if (sceneObjectEvent.triggerHover == Trigger.HOVER)
        {
            if (sceneObjectEvent.actionHover == Action.RUNSCRIPT)            
                FBScriptManager.instance.runScript(sceneObjectEvent.paramHover);
            if (sceneObjectEvent.actionHover == Action.PLAYSOUND)
                SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.paramHover);
            if (sceneObjectEvent.actionHover == Action.PAUSESOUND)
                SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.actionHover == Action.RESUMESOUND)
                SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.actionHover == Action.STOPSOUND)
                SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (sceneObjectEvent.triggerExit == Trigger.EXIT)
        {
            if (sceneObjectEvent.actionExit == Action.RUNSCRIPT)            
                FBScriptManager.instance.runScript(sceneObjectEvent.paramExit);            
            if (sceneObjectEvent.actionExit == Action.PLAYSOUND)
                SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.paramHover);
            if (sceneObjectEvent.actionExit == Action.PAUSESOUND)
                SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.actionExit == Action.RESUMESOUND)
                SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.actionExit == Action.STOPSOUND)
                SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(sceneObjectEvent.triggerClick == Trigger.CLICK)
        {
            if(sceneObjectEvent.actionClick == Action.RUNSCRIPT)            
                FBScriptManager.instance.runScript(sceneObjectEvent.paramClick);
            if (sceneObjectEvent.actionClick == Action.PLAYSOUND)
                SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.paramHover);
            if (sceneObjectEvent.actionClick == Action.PAUSESOUND)
                SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.actionClick == Action.RESUMESOUND)
                SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.actionClick == Action.STOPSOUND)
                SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);

        }
    }
   
    #endregion
}
