using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Loxodon.Framework.Views;
using Loxodon.Framework.Views.Variables;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Binding.Builder;
using System.Collections.Specialized;

public class SettingsHandler : UIView
{
    private AudioSource SettingsAudio;
    public AudioClip selectClip;
    public VariableArray variables;
    private SettingViewModel settingViewModel;
    public SettingConfig tempSettingConfig = new SettingConfig();

    protected override void Awake()
    {
        settingViewModel = new SettingViewModel(this);
        SettingsAudio = transform.GetComponent<AudioSource>();
    }

    protected override void Start()
    {
        IBindingContext bindingContext = this.BindingContext();
        bindingContext.DataContext = settingViewModel;
        BindingSet<SettingsHandler, SettingViewModel> settingBindingSet = this.CreateBindingSet<SettingsHandler, SettingViewModel>();
        settingBindingSet.Bind(variables.Get<Button>("returnButton"))
               .For(v => v.onClick).To(vm => vm.OnReturn);
        settingBindingSet.Build();
        gameObject.SetActive(false);
    }

    public IEnumerator control()
    {
        settingViewModel.reset();
        updateSettingConfig();
        Scene.main.SetMapButtonVisible(false);
        yield return StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
        while (settingViewModel.running)
        {
            yield return null;
        }
        yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
        GlobalVariables.global.resetFollower();
        gameObject.SetActive(false);
    }

    private void updateSettingConfig()
    {
        tempSettingConfig = new SettingConfig();
        SettingConfig settingConfig = GlobalPreference.Instance.getPrefSettingConfig();
        //设置视角
        variables.Get<Toggle>("visualAngleToggle").onValueChanged.AddListener(delegate (bool result)
        {
            tempSettingConfig.setOrthographic(result);
        });
        tempSettingConfig.setOrthographic(settingConfig.getOrthographic());
        variables.Get<Toggle>("visualAngleToggle").isOn = tempSettingConfig.getOrthographic();
        //设置Sfx速度
        variables.Get<Slider>("sfxVolumeSlider").onValueChanged.AddListener(delegate (float result)
        {
            tempSettingConfig.setSfxVolume(result);
            variables.Get<Text>("sfxVolume").text = "SFX音效" + result.ToString("0.0");
            GlobalPreference.Instance.getPrefSettingConfig().setSfxVolume(result);
        });
        tempSettingConfig.setSfxVolume(settingConfig.getSfxVolume());
        variables.Get<Slider>("sfxVolumeSlider").value = tempSettingConfig.getSfxVolume();
        variables.Get<Text>("sfxVolume").text = "SFX音效" + tempSettingConfig.getSfxVolume().ToString("0.0");
        //设置Bgm速度
        variables.Get<Slider>("musicVolumeSlider").onValueChanged.AddListener(delegate (float result)
        {
            tempSettingConfig.setMusicVolume(result);
            variables.Get<Text>("musicVolume").text = "BGM音量" + result.ToString("0.0");
            GlobalPreference.Instance.getPrefSettingConfig().setMusicVolume(result);
        });
        tempSettingConfig.setMusicVolume(settingConfig.getMusicVolume());
        variables.Get<Slider>("musicVolumeSlider").value = tempSettingConfig.getMusicVolume();
        variables.Get<Text>("musicVolume").text = "SFX音效" + tempSettingConfig.getMusicVolume().ToString("0.0");
        //设置文案速度
        variables.Get<Slider>("textSpeedSlider").onValueChanged.AddListener(delegate (float result)
        {
            tempSettingConfig.setTextSpeed((int)result);
            variables.Get<Text>("textSpeed").text = "文字速度 " + (int)result;
        });
        tempSettingConfig.setTextSpeed(settingConfig.getTextSpeed());
        variables.Get<Slider>("textSpeedSlider").value = tempSettingConfig.getTextSpeed();
        variables.Get<Text>("textSpeed").text = "文字速度 " + tempSettingConfig.getTextSpeed();
        //设置是否跟随
        variables.Get<Toggle>("canFollowToggle").onValueChanged.AddListener(delegate (bool result)
        {
            tempSettingConfig.setCanFollow(result);
        });
        tempSettingConfig.setCanFollow(settingConfig.getCanFollow());
        variables.Get<Toggle>("canFollowToggle").isOn = tempSettingConfig.getCanFollow();
        //设置视角大小
        variables.Get<Slider>("visualSizeSlider").onValueChanged.AddListener(delegate (float result)
        {
            setVisualSize(result);
        });
        tempSettingConfig.setOtherVisualSize(settingConfig.getOtherVisualSize());
        tempSettingConfig.setOrthVisualSize(settingConfig.getOrthVisualSize());
        variables.Get<Slider>("visualSizeSlider").value = tempSettingConfig.getOtherVisualSize();
        variables.Get<Text>("visualSize").text = "视角大小 " + tempSettingConfig.getOtherVisualSize().ToString("0.0");
    }

    private void setVisualSize(float result)
    {
        tempSettingConfig.setOrthVisualSize(result * 7.5f / 25.0f);
        tempSettingConfig.setOtherVisualSize(result);
        variables.Get<Slider>("visualSizeSlider").value = result;
        variables.Get<Text>("visualSize").text = "视角大小 " + result.ToString("0.0");
    }
}