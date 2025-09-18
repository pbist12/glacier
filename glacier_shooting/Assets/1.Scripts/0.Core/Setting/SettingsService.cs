using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SettingsService : MonoBehaviour
{
    public static SettingsService Instance { get; private set; }

    [Serializable]
    public class Model
    {
        public float masterVolume = 1f;
        public float bgmVolume = 0.8f;
        public float sfxVolume = 0.8f;
        // 필요하면 여기에 해상도/품질/VSync 등도 추가
    }

    public Model Current { get; private set; } = new Model();

    // ES3 키 모음
    const string KEY_SETTINGS = "settings_model_v1";

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();       // ES3 로드
        ApplyAll();   // 게임에 즉시 반영
    }

    // -------- 저장/로드 --------
    public void Load()
    {
        if (ES3.KeyExists(KEY_SETTINGS))
        {
            Current = ES3.Load(KEY_SETTINGS, new Model());
        }
        else
        {
            Current.masterVolume = 1f;
            Current.bgmVolume = 0.8f;
            Current.sfxVolume = 0.8f;

            Save();
        }
    }

    public void Save()
    {
        ES3.Save(KEY_SETTINGS, Current);
        // ES3.Save(key, value, "myfile.es3"); // 특정 파일에 저장하고 싶으면 이렇게
        ApplyAll();
    }

    public void LoadDefaults()
    {
        Current.masterVolume = 1f;
        Current.bgmVolume = 0.8f;
        Current.sfxVolume = 0.8f;
        Save();
    }

    // -------- Setter + 적용 --------
    public void SetMasterVolume(float v, bool apply = false)
    {
        Current.masterVolume = Mathf.Clamp01(v);
        if (apply) ApplyMaster();
    }

    public void SetBgmVolume(float v, bool apply = false)
    {
        Current.bgmVolume = Mathf.Clamp01(v);
        if (apply) ApplyBgm();
    }

    public void SetSfxVolume(float v, bool apply = false)
    {
        Current.sfxVolume = Mathf.Clamp01(v);
        if (apply) ApplySfx();
    }

    public void ApplyAll() { ApplyMaster(); ApplyBgm(); ApplySfx(); }

    void ApplyMaster() { /* 오디오 믹서/전역 볼륨 연동 */ }
    void ApplyBgm() { /* BGM AudioMixer 파라미터 연동 */ }
    void ApplySfx() { /* SFX AudioMixer 파라미터 연동 */ }

    void OnApplicationQuit() { Save(); }  // 안전하게 종료 시 저장
}
