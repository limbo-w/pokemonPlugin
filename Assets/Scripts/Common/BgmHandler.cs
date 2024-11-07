using UnityEngine;
using System.Collections;

public class BgmHandler : MonoBehaviour
{
    public static BgmHandler main;
    public bool muted;
    private float baseVolume;
    private int samplesEndBuffer;
    private float defaultFadeSpeed = 1.8f;
    private AudioSource source;

    public enum Track
    {
        Main,
        Overlay,
        MFX
    }

    private Track currentTrack;

    public AudioTrack
        mainTrack = new AudioTrack(),
        mainTrackNext = new AudioTrack(),
        overlayTrack = new AudioTrack(),
        mfxTrack = new AudioTrack();

    private bool loop = true;
    private Coroutine fading;


    void Awake()
    {
        if (main == null)
        {
            main = this;
        }
        else if (main != this)
        {
            Destroy(gameObject);
        }
        source = this.GetComponent<AudioSource>();
        source.loop = false;
    }

    void Update()
    {
        SettingConfig settingConfig = GlobalPreference.Instance.getPrefSettingConfig();
        baseVolume = settingConfig.getMusicVolume();
        if (fading == null)
        {
            source.volume = baseVolume;
        }

        if (source.clip != null)
        {
            if (loop)
            {
                if (source.timeSamples >= source.clip.samples - samplesEndBuffer)
                {
                    int loopStartSamples = (currentTrack == Track.Main) ? mainTrack.loopStartSamples : overlayTrack.loopStartSamples;
                    source.timeSamples -= source.clip.samples - samplesEndBuffer - loopStartSamples;
                    source.Play();
                }
            }
        }
    }

    //音量降低
    private IEnumerator Fade(float time)
    {
        float increment = 0f;
        while (increment < 1)
        {
            increment += (1 / time) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1f;
            }
            source.volume = (1f - increment * 1.2f) * baseVolume;
            yield return null;
        }
        fading = null;
    }

    private void Play(Track trackType)
    {
        Play(trackType, 5000);
    }

    //播放对应的Track音频
    private void Play(Track trackType, int buffer)
    {
        AudioTrack track = mainTrack;
        if (trackType == Track.Main)
        {
            mainTrack = mainTrackNext;
            track = mainTrack;
            samplesEndBuffer = buffer;
        }
        else if (trackType == Track.Overlay)
        {
            track = overlayTrack;
        }
        else if (trackType == Track.MFX)
        {
            track = mfxTrack;
        }
        currentTrack = trackType;
        source.clip = track.clip;
        source.timeSamples = track.samplesPosition;
        source.volume = baseVolume;
        source.Play();
    }

    public void Pause()
    {
        if (currentTrack == Track.Main)
        {
            mainTrack.samplesPosition = source.timeSamples;
        }
        else if (currentTrack == Track.Overlay)
        {
            overlayTrack.samplesPosition = source.timeSamples;
        }
        source.Pause();
    }

    public void PlayMain(AudioClip bgm, int loopStartSamples, bool instant)
    {
        PlayMain(bgm, loopStartSamples, instant, 5000);
    }

    public void PlayMain(AudioClip bgm, int loopStartSamples, bool instant, int buffer)
    {
        loop = true;
        StartCoroutine(PlayMainIE(bgm, loopStartSamples, instant, buffer));
    }

    private IEnumerator PlayMainIE(AudioClip bgm, int loopStartSamples, bool instant, int buffer)
    {
        mainTrackNext = new AudioTrack(bgm, loopStartSamples);
        if (mainTrack.clip == null || instant)
        {
            Play(Track.Main, buffer);
        }
        else if (fading == null)
        {
            yield return fading = StartCoroutine(Fade(defaultFadeSpeed));
            Play(Track.Main, buffer);
        }
    }
    private void StopMainFade(bool backwardsCompat)
    {
        /*if(fading != null)*/
        StopCoroutine(fading);
        StopCoroutine("PlayMainIE");
        mainTrack = mainTrackNext;
        //if(!backwardsCompat) Play(Track.Main);
    }

    public void PlayOverlay(AudioClip bgm, int loopStartSamples)
    {
        PlayOverlay(bgm, loopStartSamples, 0.1f);
    }

    public void PlayOverlay(AudioClip bgm, int loopStartSamples, float fadeTime)
    {
        StartCoroutine(PlayOverlayIE(bgm, loopStartSamples, fadeTime));
    }

    private IEnumerator PlayOverlayIE(AudioClip bgm, int loopStartSamples, float fadeTime)
    {
        loop = true;
        if (currentTrack == Track.Main)
        {
            if (fading != null)
            {
                StopMainFade(true);
            }
            else
            {
                yield return fading = StartCoroutine(Fade(fadeTime));
                Pause();
            }
        }
        else if (currentTrack == Track.Overlay)
        {
            yield return fading = StartCoroutine(Fade(fadeTime));
        }
        overlayTrack = new AudioTrack(bgm, loopStartSamples);
        Play(Track.Overlay);
    }

    public void PlayMFX(AudioClip mfx)
    {
        StartCoroutine(PlayMfxIE(mfx));
    }

    public void PlayMFXConsecutive(AudioClip mfx)
    {
        StartCoroutine(PlayMfxConsecutiveIE(mfx));
    }

    private IEnumerator PlayMfxConsecutiveIE(AudioClip mfx)
    {
        yield return StartCoroutine(MuteIE(0.2f));
        StartCoroutine(PlayMfxIE(mfx));
    }

    private IEnumerator PlayMfxIE(AudioClip mfx)
    {
        if (fading != null)
        {
            StopCoroutine(fading);
            fading = null;
        }
        Track previousTrackType = currentTrack;
        Pause();

        loop = false;
        mfxTrack = new AudioTrack(mfx, 0);
        Play(Track.MFX);
        while (source.isPlaying)
        {
            yield return null;
        }

        loop = true;
        Play(previousTrackType);
    }

    public void ResumeMain()
    {
        ResumeMain(defaultFadeSpeed, mainTrack);
    }

    public void ResumeMain(float time)
    {
        ResumeMain(time, mainTrack);
    }

    public void ResumeMain(AudioClip clip, int loopStartSamples)
    {
        ResumeMain(defaultFadeSpeed, new AudioTrack(clip, loopStartSamples));
    }

    public void ResumeMain(float time, AudioClip clip, int loopStartSamples)
    {
        ResumeMain(time, new AudioTrack(clip, loopStartSamples));
    }

    private void ResumeMain(float time, AudioTrack track)
    {
        loop = true;
        if (currentTrack == Track.Overlay)
        {
            StartCoroutine(ResumeMainIE(track, time));
        }
    }

    private IEnumerator ResumeMainIE(AudioTrack resumedTrack, float time)
    {
        mainTrackNext = resumedTrack;
        if (fading == null)
        {
            yield return fading = StartCoroutine(Fade(time));
            Play(Track.Main);
        }
    }

    public void Mute(float time)
    {
        StartCoroutine(MuteIE(time));
    }

    private IEnumerator MuteIE(float time)
    {
        source.mute = true;
        yield return new WaitForSeconds(time);
        source.mute = false;
    }
}

public class AudioTrack
{
    public AudioClip clip;
    public int loopStartSamples;
    public int samplesPosition;

    public AudioTrack()
    {
        this.clip = null;
        this.loopStartSamples = 0;
        this.samplesPosition = 0;
    }

    public AudioTrack(AudioClip clip, int loopStartSamples)
    {
        this.clip = clip;
        this.loopStartSamples = loopStartSamples;
        this.samplesPosition = 0;
    }
}