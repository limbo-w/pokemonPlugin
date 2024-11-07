using System;

[Serializable]
public class SettingConfig
{
    //对话文字速度
    public int textSpeed;
    //sfx音量
    public float sfxVolume;
    //music音量
    public float musicVolume;
    //视角正交
    public bool orthographic;
    //正交视角大小
    public float orthVisualSize;
    //其他视角大小
    public float otherVisualSize;
    //是否精灵跟随
    public bool canFollow;

    public void setCanFollow(bool canFollow)
    {
        this.canFollow = canFollow;
    }

    public bool getCanFollow()
    {
        return canFollow;
    }

    public void setOrthographic(bool orthographic)
    {
        this.orthographic = orthographic;
    }

    public bool getOrthographic()
    {
        return this.orthographic;
    }

    public void setOrthVisualSize(float visualSize)
    {
        this.orthVisualSize = visualSize;
    }

    public float getOrthVisualSize()
    {
        return orthVisualSize;
    }

    public void setOtherVisualSize(float visualSize)
    {
        this.otherVisualSize = visualSize;
    }

    public float getOtherVisualSize()
    {
        return otherVisualSize;
    }

    public void setTextSpeed(int textSpeed)
    {
        this.textSpeed = textSpeed;
    }

    public int getTextSpeed()
    {
        return this.textSpeed;
    }

    public void setSfxVolume(float sfxVolume)
    {
        this.sfxVolume = sfxVolume;
    }

    public float getSfxVolume()
    {
        return this.sfxVolume;
    }

    public void setMusicVolume(float musicVolume)
    {
        this.musicVolume = musicVolume;
    }

    public float getMusicVolume()
    {
        return musicVolume;
    }
}
