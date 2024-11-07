using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition gameScene;

    public float fadeSpeed = 1.2f;
    public bool fading = false;
    private bool fadingOut = false;
    private float increment = 0.5f;

    private RawImage screenFader;
    public Texture defaultFadeTex;

    private bool RotatableGUI;

    void Awake()
    {
        if (transform.name == "GUI")
        {
            gameScene = this;
        }


        if (screenFader != null)
        {
            RotatableGUI = false;
        }
        else
        {
            RotatableGUI = true;
        }
        if (!RotatableGUI)
        {
            screenFader.GetComponent<RectTransform>().rect.Set(0, 0, 342, 192);
        }
    }


    void Start()
    {
        if (!GlobalVariables.global.fadeIn)
        {
            if (!fading)
            {
                if (!RotatableGUI)
                {
                    screenFader.enabled = false;
                }
                
            }
        }
    }

    private IEnumerator fade(float speed)
    {
        if (!RotatableGUI)
        {
            if (screenFader.texture == null)
            {
                screenFader.texture = defaultFadeTex;
            }
        }

        if (speed == 0)
        {
            increment = 1;
            if (fadingOut)
            {
                if (!RotatableGUI)
                {
                    screenFader.color = new Color(screenFader.color.r, screenFader.color.g, screenFader.color.b,
                        (0f + increment) / 2);
                }
            }
            else
            {
                if (!RotatableGUI)
                {
                    screenFader.color = new Color(screenFader.color.r, screenFader.color.g, screenFader.color.b,
                        (1f - increment) / 2);
                }
            }
        }
        while (increment < 1)
        {
            increment += (1 / speed * 1.2f) * Time.deltaTime;
            if (fadingOut)
            {
                if (!RotatableGUI)
                {
                    screenFader.color = new Color(screenFader.color.r, screenFader.color.g, screenFader.color.b,
                        (0f + increment) / 2);
                }
            }
            else
            {
                if (!RotatableGUI)
                {
                    screenFader.color = new Color(screenFader.color.r, screenFader.color.g, screenFader.color.b,
                        (1f - increment) / 2);
                }
            }
            yield return null;
        }
        if (!fadingOut)
        {
            GlobalVariables.global.fadeTex = defaultFadeTex;
            if (!RotatableGUI)
            {
                screenFader.enabled = false;
            }
        }
        fading = false;
    }

    public float FadeIn()
    {
        return FadeIn(fadeSpeed);
    }

    public float FadeIn(float speed)
    {
        if (!RotatableGUI)
        {
            screenFader.enabled = true;
            screenFader.texture = GlobalVariables.global.fadeTex;
        }
        increment = 0;
        fadingOut = false;
        fading = true;
        StartCoroutine("fade", speed);
        GlobalVariables.global.fadeIn = false;
        return speed;
    }

    public float FadeOut()
    {
        if (!RotatableGUI)
        {
            screenFader.enabled = true;
            screenFader.texture = GlobalVariables.global.fadeTex;
        }
        increment = 0;
        fadingOut = true;
        fading = true;
        StartCoroutine("fade", fadeSpeed);
        GlobalVariables.global.fadeIn = true;
        return fadeSpeed;
    }

    public float FadeOut(float speed)
    {
        if (!RotatableGUI)
        {
            screenFader.enabled = true;
            screenFader.texture = GlobalVariables.global.fadeTex;
        }
        increment = 0;
        fadingOut = true;
        fading = true;
        StartCoroutine("fade", speed);
        GlobalVariables.global.fadeIn = true;
        return speed;
    }
}