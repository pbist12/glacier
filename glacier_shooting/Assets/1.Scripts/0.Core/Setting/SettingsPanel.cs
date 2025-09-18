using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : UIPanelBase
{
    [Header("Audio")]
    [SerializeField] private Slider masterVol;
    [SerializeField] private Slider bgmVol;
    [SerializeField] private Slider sfxVol;

    private void OnEnable()
    {
        // 현재 값 반영
        var s = SettingsService.Instance.Current;
        if (masterVol) masterVol.value = s.masterVolume;
        if (bgmVol) bgmVol.value = s.bgmVolume;
        if (sfxVol) sfxVol.value = s.sfxVolume;
    }

    public void OnChangeMaster(float v) => SettingsService.Instance.SetMasterVolume(v, apply: true);
    public void OnChangeBgm(float v) => SettingsService.Instance.SetBgmVolume(v, apply: true);
    public void OnChangeSfx(float v) => SettingsService.Instance.SetSfxVolume(v, apply: true);

    public void OnClickApply() => SettingsService.Instance.Save();
    public void OnClickDefault()
    {
        SettingsService.Instance.LoadDefaults();
        OnEnable(); // UI 갱신
    }

    public void OnClickBack()
    {
        PauseFlowController.Instance.BackToPauseMenu();
    }
}
