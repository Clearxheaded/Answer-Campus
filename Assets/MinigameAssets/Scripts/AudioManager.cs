using UnityEngine;

public class AudioManager : MonoBehaviour {
    // Singleton instance
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake() {
        // Ensure only one instance of the AudioManager
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        // Make sure there's an AudioSource on this same GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Plays a single sound effect (OneShot).
    /// </summary>
    /// <param name="clip">Clip to play</param>
    public void PlaySFX(AudioClip clip) {
        if (clip != null) {
            audioSource.PlayOneShot(clip);
        }
    }
}
