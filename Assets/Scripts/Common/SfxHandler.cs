﻿using UnityEngine;
using System.Collections;

public class SfxHandler : MonoBehaviour
{
    public static SfxHandler sfxHandler;
    public AudioSource[] sources;

    void Awake()
    {
        if (sfxHandler == null)
        {
            sfxHandler = this;
        }
        else if (sfxHandler != this)
        {
            Destroy(gameObject);
        }

        sources = this.gameObject.GetComponents<AudioSource>();
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i].loop = false;
        }
    }

    public static AudioSource Play(AudioClip clip)
    {
        return Play(clip, 1f);
    }

    public static AudioSource Play(AudioClip clip, float pitch)
    {
        AudioSource source = null;
        for (int i = 0; i < sfxHandler.sources.Length; i++)
        {
            if (!sfxHandler.sources[i].isPlaying)
            {
                source = sfxHandler.sources[i];
                i = sfxHandler.sources.Length;
            }
        }
        if (source == null)
        {
            float mostFinished = 0;
            int mostFinishedIndex = 0;
            for (int i = 0; i < sfxHandler.sources.Length; i++)
            {
                if (sfxHandler.sources[i].clip != null)
                {
                    if (sfxHandler.sources[i].clip.length != 0)
                    {
                        if ((sfxHandler.sources[i].timeSamples / sfxHandler.sources[i].clip.length) > mostFinished)
                        {
                            mostFinished = sfxHandler.sources[i].timeSamples / sfxHandler.sources[i].clip.length;
                            mostFinishedIndex = i;
                        }
                    }
                }
            }
            source = sfxHandler.sources[mostFinishedIndex];
        }
        SettingConfig settingConfig = GlobalPreference.Instance.getPrefSettingConfig();
        source.volume = settingConfig.getSfxVolume();
        source.clip = clip;    
        source.pitch = pitch;
        source.Play();

        return source;
    }

    public static void FadeSource(AudioSource source, float time)
    {
        sfxHandler.StartCoroutine(sfxHandler.FadeIE(source, time));
    }

    private IEnumerator FadeIE(AudioSource source, float time)
    {
        float initialVolume = source.volume;
        float increment = 0f;
        while (increment < 1)
        {
            increment += (1 / time) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1f;
            }

            if (!source.isPlaying)
            {
                increment = 1f;
            }

            source.volume = initialVolume * (1 - increment);
            if (source.isPlaying)
            {
                yield return null;
            }
        }
        source.volume = initialVolume;
        source.Stop();
    }
}