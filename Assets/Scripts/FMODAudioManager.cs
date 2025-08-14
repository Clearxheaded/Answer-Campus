using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
//using Debug = FMOD.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class FMODAudioManager : MonoBehaviour
{
    public static FMODAudioManager Instance { get; private set; }
    private string currentEventPath = "";
    private AudioSource audioSource;
    public FMOD.Studio.EventInstance currentMusic;
    public FMOD.Studio.EventInstance currentAmbient;

    private Coroutine currentFadeOut;
    private EventInstance nextMusicToPlay;

    private Coroutine currentMusicCoroutine;
    private List<EventInstance> allCreatedInstances = new();


    public void PrintActiveMusicInstances()
    {
        Debug.Log("----- Active FMOD Music Instances -----");
        foreach (var instance in allCreatedInstances)
        {
            if (instance.isValid())
            {
                instance.getDescription(out var desc);
                desc.getPath(out string path);
                instance.getPlaybackState(out var state);
                Debug.Log($"Playing: {path}, State: {state}");
            }
        }
    }

    public void PlayMusic(string eventName, float fadeDuration = 1f)
    {
        if (currentMusicCoroutine != null)
            StopCoroutine(currentMusicCoroutine);

        currentMusicCoroutine = StartCoroutine(TransitionToNewMusic(eventName, fadeDuration));
    }

    private IEnumerator TransitionToNewMusic(string newEventName, float fadeDuration)
    {
        // Fade out current music
        if (currentMusic.isValid())
        {
            currentMusic.getVolume(out float startVolume);
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
                currentMusic.setVolume(volume);
                yield return null;
            }

            currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentMusic.release();
        }

        // Now start new music
        currentMusic = FMODUnity.RuntimeManager.CreateInstance(newEventName);
        allCreatedInstances.Add(currentMusic);
        currentMusic.setVolume(1f);
        currentMusic.start();
    }


    private IEnumerator FadeOutAndReplace(EventInstance oldMusic, EventInstance newMusic, float duration)
    {
        float timer = 0f;
        oldMusic.getVolume(out float startVolume);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newVol = Mathf.Lerp(startVolume, 0, timer / duration);
            oldMusic.setVolume(newVol);
            yield return null;
        }

        oldMusic.stop(STOP_MODE.ALLOWFADEOUT);
        oldMusic.release();

        currentMusic = newMusic;
        currentMusic.start();

        currentFadeOut = null;
        nextMusicToPlay.clearHandle();
    }

    public void PlayAmbient(string eventName)
    {
        if (currentAmbient.isValid())
        {
            currentAmbient.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentAmbient.release();
        }
        currentAmbient = FMODUnity.RuntimeManager.CreateInstance(eventName);
        currentAmbient.start();
        allCreatedInstances.Add(currentAmbient);
    }

    public void FadeOutMusic(float duration)
    {
        if (currentMusic.isValid())
        {
            StartCoroutine(FadeOutAndStop(currentMusic, duration));
        }
    }

    public void FadeOutAmbient(float duration)
    {
        if (currentAmbient.isValid())
        {
            StartCoroutine(FadeOutAndStop(currentAmbient, duration));
        }
    }
    private IEnumerator FadeOutAndStop(FMOD.Studio.EventInstance instance, float duration)
    {
        float timer = 0f;
        instance.getVolume(out float startVolume);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newVol = Mathf.Lerp(startVolume, 0, timer / duration);
            instance.setVolume(newVol);
            yield return null;
        }

        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        Debug.Log($"Releasing instance {instance}");
        instance.release();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public AudioSource GetAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = Camera.main.gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                UnityEngine.Debug.LogError("No audio source found!");
            }
        }
        return audioSource;
    }

    public void SetDrums(int drumSet)
    {
        currentMusic.setParameterByName("CharacterDrumSelection", drumSet);

    }
    /// <summary>
    /// Plays a one-shot event, optionally setting a parameter before playback.
    /// </summary>
    /// <param name="eventPath">FMOD event path (e.g., "SFX/Cheer")</param>
    /// <param name="paramName">Name of the parameter to set (optional)</param>
    /// <param name="paramValue">Value to set the parameter to (ignored if paramName is null or empty)</param>
    public void PlayOneShot(string eventPath, string paramName = null, float paramValue = 0f)
    {
        if (string.IsNullOrEmpty(eventPath)) return;

        var instance = RuntimeManager.CreateInstance($"event:/{eventPath}");

        if (!string.IsNullOrEmpty(paramName))
        {
            instance.setParameterByName(paramName, paramValue);
        }

        instance.start();
        instance.release(); // Automatically cleans up once finished
    }
    public IEnumerator PlayOneShotAndWaitPrecise(string eventPath, string paramName = null, float paramValue = 0f)
    {
        if (string.IsNullOrEmpty(eventPath)) yield break;

        var instance = RuntimeManager.CreateInstance($"event:/{eventPath}");
        if (!string.IsNullOrEmpty(paramName))
            instance.setParameterByName(paramName, paramValue);

        instance.start();
        instance.release();

        FMOD.Studio.PLAYBACK_STATE state;
        do
        {
            yield return null;
            instance.getPlaybackState(out state);
        }
        while (state != FMOD.Studio.PLAYBACK_STATE.STOPPED);
    }
    
    public void StopMusic(bool allowFadeOut = true)
    {
        if (currentMusic.isValid())
        {
            currentMusic.stop(allowFadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            currentMusic.release();
        }

        currentEventPath = "";
    }



}

