using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class CutsceneScript : MonoBehaviour
{
    public GameObject cutScene;
    public TextMeshProUGUI display;
    public string[] sentences;
    public float timeBetweenSentences;

    public GameObject exterior;
    public GameObject dialogueManager;
    public GameObject story;

    private float nextSentenceTime;
    private int sentenceIndex = 0;
    private int loadSequence = 0;

    // Start is called before the first frame update
    void Start()
    {
        loadSequence = 0;
        if(sentences.Length > 0)
        {
            Debug.Log("Setting first text");
            display.text = sentences[sentenceIndex];
        }

        nextSentenceTime = Time.time + timeBetweenSentences;
        sentenceIndex++;
    }

    // Update is called once per frame
    void Update()
    {
        if (nextSentenceTime <= Time.time && loadSequence == 0)
        {

            nextSentenceTime = Time.time + timeBetweenSentences;
            if(sentenceIndex < sentences.Length)
            {
                display.text = sentences[sentenceIndex];
            }

            sentenceIndex++;
        }
        //go over by one to gain another timer on the last item
        if (sentences.Length < sentenceIndex)
        {
            loadSequence = 1;
        }

        if(loadSequence == 1)
        {
            display.gameObject.SetActive(false);
            exterior.SetActive(true);
            loadSequence = 2;
        }

        if(loadSequence == 2)
        {
            if(exterior.GetComponent<Image>())
            {
                if (exterior.GetComponent<Image>().color == Color.black)
                {
                    loadSequence = 3;
                }

            }
            else
            {
                Debug.Log("No image found, moving to next sequence...");
                loadSequence = 3;
            }

        }

        if(loadSequence == 3)
        {
            if(cutScene.GetComponent<FadeOutAudioSource>())
            {
                AudioSource cameraAudio = cutScene.GetComponent<FadeOutAudioSource>().GetAudioSource();
                cutScene.SetActive(false);
                //Get camera's MonoBehaviour
                MonoBehaviour camMono = Camera.main.GetComponent<MonoBehaviour>();
                //Use it to start your coroutine function
                camMono.StartCoroutine(FadeAudioSource.StartFade(cameraAudio, 1f, 0));

            }
            dialogueManager.SetActive(true);
            story.SetActive(true);
        }

    }


}

public static class FadeAudioSource
{
    public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }
}