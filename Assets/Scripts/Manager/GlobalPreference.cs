using UnityEngine;
using System.Collections;
using Loxodon.Framework.Prefs;
using System;
using System.Collections.Generic;

public class GlobalPreference : MonoBehaviour
{
    private static GlobalPreference _instance = null;
    //设置配置相
    private SettingConfig settingConfig;
    //玩家本地信息
    private PlayerConfig playerConfig;
    private Dictionary<int, Dictionary<string, int>> playerTaskConfigDic = new Dictionary<int, Dictionary<string, int>>();


    public static GlobalPreference Instance
    {
        get
        {
            return _instance ??= new GlobalPreference();
        }
    }

    private GlobalPreference()
    {
        getPrefSettingConfig();
        getPrefPlayerConfig();

    }

    public void setPrefPlayerTaskConfig(int index, Dictionary<string, int> config)
    {
        playerTaskConfigDic[index] = config;
        PlayerPrefs.SetString("PlayerTaskConfig" + index, JsonUtility.ToJson(config));
        PlayerPrefs.Save();
    }

    public Dictionary<string, int> getPrefPlayerTaskConfig(int index)
    {
        if (playerTaskConfigDic.ContainsKey(index) && playerTaskConfigDic[index] != null)
        {
            return playerTaskConfigDic[index];
        }
        playerTaskConfigDic[index] = JsonUtility.FromJson<Dictionary<string, int>>(PlayerPrefs.GetString("PlayerTaskConfig" + index, ""));
        if (playerTaskConfigDic[index] == null)
        {
            playerTaskConfigDic[index] = new Dictionary<string, int>();
        }
        return playerTaskConfigDic[index];
    }


    public void setPrefPlayerConfig(PlayerConfig config)
    {
        playerConfig = config;
        PlayerPrefs.SetString("PlayerConfig", JsonUtility.ToJson(config));
        PlayerPrefs.Save();
    }

    public PlayerConfig getPrefPlayerConfig()
    {
        if (playerConfig != null)
        {
            return playerConfig;
        }
        playerConfig = JsonUtility.FromJson<PlayerConfig>(PlayerPrefs.GetString("PlayerConfig", ""));
        if (playerConfig == null)
        {
            playerConfig = new PlayerConfig();
        }
        return playerConfig;
    }

    public void setPrefSettingConfig(SettingConfig config)
    {
        settingConfig = config;
        //todo 要反注释这行
        //PlayerMovement.player.mainCamera.orthographic = settingConfig.getOrthographic();
        //if (settingConfig.getOrthographic())
        //{
        //    PlayerMovement.player.mainCamera.orthographicSize = settingConfig.getOrthVisualSize();
        //}
        //else
        //{
        //    PlayerMovement.player.mainCamera.fieldOfView = settingConfig.getOtherVisualSize();
        //}
        PlayerPrefs.SetString("SettingConfig", JsonUtility.ToJson(settingConfig));
        PlayerPrefs.Save();
    }

    public SettingConfig getPrefSettingConfig()
    {
        if (settingConfig != null)
        {
            return settingConfig;
        }
        settingConfig = JsonUtility.FromJson<SettingConfig>(PlayerPrefs.GetString("SettingConfig", ""));
        if (settingConfig == null || settingConfig.getTextSpeed() == 0)
        {
            settingConfig = new SettingConfig();
            settingConfig.setTextSpeed(10);
            settingConfig.setSfxVolume(1f);
            settingConfig.setMusicVolume(1f);
            settingConfig.setOtherVisualSize(25f);
            settingConfig.setOrthVisualSize(7.5f);
            setPrefSettingConfig(settingConfig);
        }
        return settingConfig;
    }

    //==========================================================================
    public struct CustomData
    {
        public CustomData(string name, string description)
        {
            this.name = name;
            this.description = description;
        }

        public string name;
        public string description;
    }

    public class CustomDataTypeEncoder : ITypeEncoder
    {
        private int priority = 0;

        public int Priority
        {
            get { return this.priority; }
            set { this.priority = value; }
        }

        public bool IsSupport(Type type)
        {
            return typeof(CustomData).Equals(type);
        }

        public object Decode(Type type, string value)
        {
            return JsonUtility.FromJson(value, type);
        }

        public string Encode(object value)
        {
            return JsonUtility.ToJson(value);
        }
    }
}
