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
        return Action.NONE;]
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
        {
            if (sceneObjectEvent.action1 == Action.RUNSCRIPT)                FBScriptManager.instance.runScript(sceneObjectEvent.param1);
            if (sceneObjectEvent.action1 == Action.PLAYSOUND)                SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.param1);
            if (sceneObjectEvent.action1 == Action.PAUSESOUND)                SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.action1 == Action.RESUMESOUND)                SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.action1 == Action.STOPSOUND)                SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);
        }
        if (sceneObjectEvent.trigger2 == Trigger.HOVER)
        {
            if (sceneObjectEvent.action2 == Action.RUNSCRIPT)                FBScriptManager.instance.runScript(sceneObjectEvent.param2);
            if (sceneObjectEvent.action2 == Action.PLAYSOUND)                SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.param2);
            if (sceneObjectEvent.action2 == Action.PAUSESOUND)                SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.action2 == Action.RESUMESOUND)                SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.action2 == Action.STOPSOUND)                SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);
        }
        if (sceneObjectEvent.trigger3 == Trigger.HOVER)
        {
            if (sceneObjectEvent.action3 == Action.RUNSCRIPT)                FBScriptManager.instance.runScript(sceneObjectEvent.param3);
            if (sceneObjectEvent.action3 == Action.PLAYSOUND)                SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.param3);
            if (sceneObjectEvent.action3 == Action.PAUSESOUND)                SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.action3 == Action.RESUMESOUND)                SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.action3 == Action.STOPSOUND)                SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (sceneObjectEvent.trigger1 == Trigger.EXIT)
        {
            if (sceneObjectEvent.action1 == Action.RUNSCRIPT)                FBScriptManager.instance.runScript(sceneObjectEvent.param1);            
            if (sceneObjectEvent.action1 == Action.PLAYSOUND)                SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.param1);
            if (sceneObjectEvent.action1 == Action.PAUSESOUND)                SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.action1 == Action.RESUMESOUND)                SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.action1 == Action.STOPSOUND)                SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);
        }
        if (sceneObjectEvent.trigger2 == Trigger.EXIT)
        {
            if (sceneObjectEvent.action2 == Action.RUNSCRIPT) FBScriptManager.instance.runScript(sceneObjectEvent.param2);
            if (sceneObjectEvent.action2 == Action.PLAYSOUND) SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.param2);
            if (sceneObjectEvent.action2 == Action.PAUSESOUND) SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.action2 == Action.RESUMESOUND) SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.action2 == Action.STOPSOUND) SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);
        }
        if (sceneObjectEvent.trigger3 == Trigger.EXIT)
        {
            if (sceneObjectEvent.action3 == Action.RUNSCRIPT) FBScriptManager.instance.runScript(sceneObjectEvent.param3);
            if (sceneObjectEvent.action3 == Action.PLAYSOUND) SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.param3);
            if (sceneObjectEvent.action3 == Action.PAUSESOUND) SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.action3 == Action.RESUMESOUND) SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.action3 == Action.STOPSOUND) SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(sceneObjectEvent.trigger1 == Trigger.CLICK)
        {
            if(sceneObjectEvent.action1 == Action.RUNSCRIPT)                FBScriptManager.instance.runScript(sceneObjectEvent.param1);
            if (sceneObjectEvent.action1 == Action.PLAYSOUND)                SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.param1);
            if (sceneObjectEvent.action1 == Action.PAUSESOUND)                SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.action1 == Action.RESUMESOUND)                SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.action1 == Action.STOPSOUND)                SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);

        }
        if (sceneObjectEvent.trigger2 == Trigger.CLICK)
        {
            if (sceneObjectEvent.action2 == Action.RUNSCRIPT)                FBScriptManager.instance.runScript(sceneObjectEvent.param2);
            if (sceneObjectEvent.action2 == Action.PLAYSOUND)                SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.param2);
            if (sceneObjectEvent.action2 == Action.PAUSESOUND)                SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.action2 == Action.RESUMESOUND)                SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.action2 == Action.STOPSOUND)                SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);

        }
        if (sceneObjectEvent.trigger3 == Trigger.CLICK)
        {
            if (sceneObjectEvent.action3 == Action.RUNSCRIPT)                FBScriptManager.instance.runScript(sceneObjectEvent.param3);
            if (sceneObjectEvent.action3 == Action.PLAYSOUND)                SoundResonanceManager.instance.playSfx(this.gameObject, sceneObjectEvent.param3);
            if (sceneObjectEvent.action3 == Action.PAUSESOUND)                SoundResonanceManager.instance.pauseAllSoundOnObj(this.gameObject);
            if (sceneObjectEvent.action3 == Action.RESUMESOUND)                SoundResonanceManager.instance.resumeSound(this.gameObject);
            if (sceneObjectEvent.action3 == Action.STOPSOUND)                SoundResonanceManager.instance.stopAllSoundOnObj(this.gameObject);

        }
    }
    
    //Begin Drag
    void beginDrag()
    {
        
    }
   
    #endregion
}
