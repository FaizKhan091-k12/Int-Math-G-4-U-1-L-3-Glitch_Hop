using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip wrongClip;
    [SerializeField] private AudioClip clickClip;

    [Header("Mute Settings")]
    [SerializeField] private bool isMuted = false;
    [SerializeField] private Image muteButtonImage;
    [SerializeField] private Sprite muteSprite;
    [SerializeField] private Sprite unmuteSprite;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Strictly eliminate dial audio clip - generate a soft cyber click
        if (clickClip == null || (clickClip != null && clickClip.name.ToLower().Contains("dial")))
        {
            clickClip = CreateSoftCyberClick();
        }

        if (correctClip == null)
        {
            correctClip = CreateCyberCorrectChime();
        }

        if (wrongClip == null)
        {
            wrongClip = CreateCyberGlitchBuzz();
        }
    }

    private AudioClip CreateSoftCyberClick()
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.05f); // 50ms pleasant soft sci-fi blip
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(1200f, 650f, t);
            float envelope = Mathf.Pow(1f - t, 2.5f);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * envelope * 0.25f;
        }
        AudioClip clip = AudioClip.Create("CyberClick", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateCyberCorrectChime()
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.5f); // 500ms lush electronic chord
        float[] data = new float[samples];
        float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Pow(1f - t, 1.8f);
            float sampleSum = 0f;
            for (int f = 0; f < freqs.Length; f++)
            {
                sampleSum += Mathf.Sin(2f * Mathf.PI * freqs[f] * i / sampleRate);
            }
            data[i] = (sampleSum / freqs.Length) * envelope * 0.45f;
        }
        AudioClip clip = AudioClip.Create("CyberCorrectChime", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateCyberGlitchBuzz()
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.35f); // 350ms glitch buzz
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(220f, 85f, t);
            float envelope = Mathf.Pow(1f - t, 2.0f);
            float tone = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate);
            float noise = (Random.value * 2f - 1f) * 0.25f;
            data[i] = Mathf.Clamp(tone + noise, -1f, 1f) * envelope * 0.35f;
        }
        AudioClip clip = AudioClip.Create("CyberGlitchBuzz", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void Start()
    {
        PlayBGM();
        UpdateMuteVisuals();
    }

    public void PlayBGM()
    {
        if (musicSource != null && bgmClip != null)
        {
            musicSource.clip = bgmClip;
            musicSource.loop = true;
            musicSource.volume = isMuted ? 0f : 0.45f;
            musicSource.Play();
        }
    }

    public void PlayCorrect()
    {
        if (isMuted) return;
        if (sfxSource != null && correctClip != null)
        {
            sfxSource.PlayOneShot(correctClip, 0.9f);
        }
    }

    public void PlayWrong()
    {
        if (isMuted) return;
        if (sfxSource != null && wrongClip != null)
        {
            sfxSource.PlayOneShot(wrongClip, 0.9f);
        }
    }

    public void PlayClick()
    {
        if (isMuted) return;
        if (sfxSource != null && clickClip != null)
        {
            sfxSource.PlayOneShot(clickClip, 0.8f);
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        if (musicSource != null)
        {
            musicSource.volume = isMuted ? 0f : 0.45f;
        }
        if (sfxSource != null)
        {
            sfxSource.volume = isMuted ? 0f : 1f;
        }
        UpdateMuteVisuals();
    }

    private void UpdateMuteVisuals()
    {
        if (muteButtonImage != null)
        {
            muteButtonImage.sprite = isMuted ? muteSprite : unmuteSprite;
        }
    }

    public bool IsMuted => isMuted;
}
