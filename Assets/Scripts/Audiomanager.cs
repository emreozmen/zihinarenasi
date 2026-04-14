using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Müzik")]
    public AudioClip menuSong;
    public AudioClip gameSong;

    [Header("Ses Efektleri")]
    public AudioClip clickSFX;
    public AudioClip correctSFX;
    public AudioClip incorrectSFX;
    public AudioClip levelCompleteSFX;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private const string MusicKey = "MusicEnabled";
    private const string SoundKey = "SoundEnabled";

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ApplySavedSettings();
        PlayMusicForCurrentScene();
    }

    private void SetupAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.5f;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.volume = 1f;
        sfxSource.playOnAwake = false;
    }

    private void ApplySavedSettings()
    {
        bool musicOn = IsMusicOn();
        bool soundOn = IsSoundOn();

        musicSource.mute = !musicOn;
        sfxSource.mute = !soundOn;
    }

    // ──────────────────────────────────────────
    // Sahneye Göre Müzik
    // ──────────────────────────────────────────

    public void PlayMusicForCurrentScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu" || sceneName == "LevelSelect" || sceneName == "Boot")
            PlayMusic(menuSong);
        else
            PlayMusic(gameSong);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // ──────────────────────────────────────────
    // Ses Efektleri
    // ──────────────────────────────────────────

    public void PlayClick() => PlaySFX(clickSFX);
    public void PlayCorrect() => PlaySFX(correctSFX);
    public void PlayIncorrect() => PlaySFX(incorrectSFX);
    public void PlayLevelComplete() => PlaySFX(levelCompleteSFX);

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // ──────────────────────────────────────────
    // Ses & Müzik Aç/Kapat — AYRI AYRI
    // ──────────────────────────────────────────

    /// <summary>Sadece SFX'i (tıklama, doğru/yanlış sesleri) etkiler.</summary>
    public void SetSound(bool isOn)
    {
        PlayerPrefs.SetInt(SoundKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
        sfxSource.mute = !isOn;
    }

    /// <summary>Sadece arka plan müziğini etkiler.</summary>
    public void SetMusic(bool isOn)
    {
        PlayerPrefs.SetInt(MusicKey, isOn ? 1 : 0);
        PlayerPrefs.Save();

        musicSource.mute = !isOn;

        if (isOn && !musicSource.isPlaying)
            PlayMusicForCurrentScene();
    }

    public bool IsMusicOn() => PlayerPrefs.GetInt(MusicKey, 1) == 1;
    public bool IsSoundOn() => PlayerPrefs.GetInt(SoundKey, 1) == 1;

    // ──────────────────────────────────────────
    // Sahne Değişince Müzik Güncelle
    // ──────────────────────────────────────────

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        ApplySavedSettings();
        PlayMusicForCurrentScene();
    }
}