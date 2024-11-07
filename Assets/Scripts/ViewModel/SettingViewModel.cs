using System;
using System.Collections;
using System.Collections.Specialized;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class SettingViewModel : ViewModelBase
{
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
    private SettingsHandler settingsHandler;
    public bool running;

    public SettingViewModel(SettingsHandler handler)
    {
        this.settingsHandler = handler;
    }

    public void reset()
    {
        dialog = DialogBoxUIHandler.main;
        running = true;
    }
    //==========================================================================
    public void OnReturn()
    {
        StaticCoroutine.StartCoroutine(doReturn());
    }

    public IEnumerator doReturn()
    {
        yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilent("是否保存当前选项设置?"));
        string[] choices = { "是", "否", };
        yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
        int chosenIndex = dialog.buttonIndex;
        if (chosenIndex == 1)
        {
            SfxHandler.Play(settingsHandler.selectClip);
            GlobalPreference.Instance.setPrefSettingConfig(settingsHandler.tempSettingConfig);
        }
        else
        {
            SfxHandler.Play(settingsHandler.selectClip);
        }
        running = false;
    }
}
