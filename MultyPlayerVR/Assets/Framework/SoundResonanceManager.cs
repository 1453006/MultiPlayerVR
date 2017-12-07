using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class SoundResonanceManager : MonoBehaviour {

    public static SoundResonanceManager instance;
    [HideInInspector]
    public Dictionary<string, AudioClip> soundBank;

    public List<AudioClip> listAudio;
    private void Awake()
    {
        instance = this;

        soundBank = new Dictionary<string, AudioClip>();
        for (int i = 0; i < listAudio.Count; i++)
            soundBank.Add(listAudio[i].name, listAudio[i]);

    }   

    void addResonanceComponent(GameObject obj)
    {
        obj.AddComponent<ResonanceAudioSource>();
        AudioSource audioSource = obj.GetComponent<ResonanceAudioSource>().audioSource;      
        AudioMixer mixer = Resources.Load("ResonanceAudioMixer") as AudioMixer;
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
        audioSource.spatialize = true;
        audioSource.spatializePostEffects = true;
    }
    public void playSfx(GameObject obj, string soundName)
    {
        if(!obj.GetComponent<ResonanceAudioSource>())
            addResonanceComponent(obj);
        if (soundBank.ContainsKey(soundName))
        {
            if (!obj.GetComponent<AudioSource>().isPlaying)
            {
                obj.GetComponent<AudioSource>().clip = soundBank[soundName];
                obj.GetComponent<AudioSource>().PlayOneShot(soundBank[soundName]);
            }
        }
    }
    public void playBgm(GameObject obj, string soundName, bool loop)
    {
        addResonanceComponent(obj);
        if (soundBank.ContainsKey(soundName))
        {
            obj.GetComponent<AudioSource>().clip = soundBank[soundName];
            obj.GetComponent<AudioSource>().Play();
            obj.GetComponent<AudioSource>().loop = loop;
        }
    }
    public void pauseAllSoundOnObj(GameObject obj)
    {
        if (obj.GetComponent<AudioSource>().isPlaying)
            obj.GetComponent<AudioSource>().Pause();        
    }
    public void stopAllSoundOnObj(GameObject obj)
    {
 
        if (obj.GetComponent<AudioSource>().isPlaying)
            obj.GetComponent<AudioSource>().Stop();
    }
    public void resumeSound(GameObject obj)
    {
        if (!obj.GetComponent<AudioSource>().isPlaying)
            obj.GetComponent<AudioSource>().UnPause();
    }
}
