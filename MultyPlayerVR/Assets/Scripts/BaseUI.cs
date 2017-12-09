using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DoozyUI;
using UnityEngine.Video;
public class BaseUI : MonoBehaviour {
    public static BaseUI instance;
    string textPosName = "TextPos";
    string clipPosName = "ClipPos";
    private void Awake()
    {
        instance = this;
    }
    public void ShowTextObject(string content, GameObject obj)
    {
        obj.transform.Find(textPosName).GetComponentInChildren<Text>().text = content;
        obj.transform.Find(textPosName).GetComponentInChildren<UIElement>().Show(false);
    }
    public void HideTextObject(GameObject obj) {
        obj.transform.Find(textPosName).GetComponentInChildren<UIElement>().Hide(false);
    }
    public void PlayVideo(VideoClip clip,GameObject obj)
    {
        obj.transform.Find(clipPosName).GetComponentInChildren<UIElement>().Show(false);
        StartCoroutine(PlayProcess(clip, obj));
    }
    public void PauseVideo(GameObject obj) {
        obj.transform.Find(clipPosName).GetComponentInChildren<VideoPlayer>().Pause();
        obj.transform.Find(clipPosName).GetComponentInChildren<UIElement>().Hide(false);
    }
    public void ShowTextOnClick(string content, Vector3 pos, float delaytime = 0)
    {
        GameObject temp =Resources.Load<GameObject>("Prefabs/RemoteText");
        temp.GetComponentInChildren<UIElement>().GetInAnimations.moveIn.delay = delaytime;
        temp.GetComponentInChildren<Text>().text =content;
        GameObject textShow = Instantiate(temp);
        textShow.transform.position = pos;
        GameObject camera = GameObject.Find("Main Camera");
        textShow.transform.LookAt(camera.transform);
        StartCoroutine(DeleteText(textShow, 2));
    }
    public IEnumerator DeleteText(GameObject obj,float waitTime) {
        yield return new WaitForSeconds(waitTime);
        DestroyObject(obj.gameObject);
    }
    IEnumerator PlayProcess(VideoClip clip, GameObject obj)
    {
        RawImage image=obj.transform.Find(clipPosName).GetComponentInChildren<RawImage>();
        VideoPlayer videoPlayer= obj.transform.Find(clipPosName).GetComponentInChildren<VideoPlayer>();
        videoPlayer.clip = clip;
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }
        image.texture = videoPlayer.texture;
        videoPlayer.Play();

    }
}
