using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Proyecto26;
using System.IO;
using System;
using ThreeDISevenZeroR.UnityGifDecoder;
using UnityEngine.Networking;

//常用工具类
public class CommonUtils
{
    public enum DpadDiction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    private static long mLastClickTime;

    public static bool isQuickClick()
    {
        long currentClickTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        long elapsedTime = currentClickTime - mLastClickTime;
        mLastClickTime = currentClickTime;
        if (elapsedTime <= 1000)
        {

            return true;
        }
        return false;
    }

    public static void onDestoryApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static string convertLongID(long ID)
    {
        string result = ID.ToString();
        while (result.Length < 3)
        {
            result = "0" + result;
        }
        return result;
    }


    public static DpadDiction getDpadDiction(Vector2 movement)
    {
        if (movement.x < 0 && movement.y < 0)
        {
            if (Math.Abs(movement.x) > Math.Abs(movement.y))
            {
                return DpadDiction.Left;
            }
            else
            {
                return DpadDiction.Down;
            }
        }
        else if (movement.x > 0 && movement.y < 0)
        {
            if (Math.Abs(movement.x) > Math.Abs(movement.y))
            {
                return DpadDiction.Right;
            }
            else
            {
                return DpadDiction.Down;
            }
        }
        else if (movement.x < 0 && movement.y > 0)
        {
            if (Math.Abs(movement.x) > Math.Abs(movement.y))
            {
                return DpadDiction.Left;
            }
            else
            {
                return DpadDiction.Up;
            }
        }
        else if (movement.x > 0 && movement.y > 0)
        {
            if (Math.Abs(movement.x) > Math.Abs(movement.y))
            {
                return DpadDiction.Right;
            }
            else
            {
                return DpadDiction.Up;
            }
        }
        return DpadDiction.None;
    }


    public static void changeAnimateImageScale(List<Texture> selectedSpriteAnimation, RawImage selectedSprite, float size)
    {
        if (selectedSpriteAnimation == null || selectedSpriteAnimation.Count <= 0)
        {
            return;
        }
        selectedSprite.texture = selectedSpriteAnimation[0];
        if (selectedSpriteAnimation[0].height > 0)
        {
            selectedSprite.GetComponent<RectTransform>().localScale = new Vector3(
                selectedSpriteAnimation[0].width / size, selectedSpriteAnimation[0].height / size, 1);
        }
    }

    public static Sprite toSprite(Texture2D self)
    {
        var rect = new Rect(0, 0, self.width, self.height);
        var pivot = Vector2.one * 0.5f;
        var newSprite = Sprite.Create(self, rect, pivot);
        return newSprite;
    }

    public static IEnumerator animateImage(List<Texture> selectedSpriteAnimation, RawImage selectedSprite)
    {
        int frame = 0;
        while (selectedSpriteAnimation != null)
        {
            if (selectedSpriteAnimation.Count > 0)
            {
                if (frame < selectedSpriteAnimation.Count - 1)
                {
                    frame += 1;
                }
                else
                {
                    frame = 0;

                }
                selectedSprite.texture = selectedSpriteAnimation[frame];
            }
            yield return new WaitForSeconds(0.075f);
        }
    }

    public static IEnumerator animateImage(List<Texture> selectedSpriteAnimation, SpriteRenderer selectedSprite)
    {
        int frame = 0;
        while (selectedSpriteAnimation != null)
        {
            if (selectedSpriteAnimation.Count > 0)
            {
                if (frame < selectedSpriteAnimation.Count - 1)
                {
                    frame += 1;
                }
                else
                {
                    frame = 0;

                }
                selectedSprite.sprite = toSprite(selectedSpriteAnimation[frame] as Texture2D);
            }
            yield return new WaitForSeconds(0.075f);
        }
    }

    public static IEnumerator StartUVAnimation(RawImage[] icons, float interTime)
    {
        foreach (RawImage icon in icons)
        {
            icon.uvRect = new Rect(0, 0, icon.uvRect.width, icon.uvRect.height);
        }
        yield return new WaitForSeconds(interTime);
        foreach (RawImage icon in icons)
        {
            icon.uvRect = new Rect(0.5f, 0, icon.uvRect.width, icon.uvRect.height);
        }
        yield return new WaitForSeconds(interTime);
    }

    public static IEnumerator StartUVAnimation(RawImage icon, float interTime)
    {
        icon.uvRect = new Rect(0, 0, icon.uvRect.width, icon.uvRect.height);
        yield return new WaitForSeconds(interTime);
        icon.uvRect = new Rect(0.5f, 0, icon.uvRect.width, icon.uvRect.height);
        yield return new WaitForSeconds(interTime);
    }

    //展示对话内容
    public static IEnumerator showDialogTextDelay(string text, float waitTime)
    {
        DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
        yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilent(text));
        yield return new WaitForSeconds(waitTime);
        dialog.undrawDialogBox();
    }

    //展示对话内容并不隐藏
    public static IEnumerator showDialogTextDelayNoEnd(string text, float waitTime)
    {
        DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
        yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilent(text));
        yield return new WaitForSeconds(waitTime);
    }

    //展示对话内容
    public static IEnumerator showDialogText(string text)
    {
        DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
        yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilentNoEnd(text));
        while (dialog.isRunning(DialogBoxUIHandler.BUTTON_CLICK_RUNNING))
        {
            yield return new WaitForSeconds(0.2f);
        }
        dialog.undrawDialogBox();
    }

    //展示对话内容
    public static IEnumerator showChoiceTextWithInput(string text)
    {
        DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
        yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilentWithInputNoEnd(text));
        string[] choices = new string[] { "确定", "取消" };
        dialog.drawChoiceButtons(choices);
        while (dialog.isRunning(DialogBoxUIHandler.CHOICE_RUNNING))
        {
            yield return new WaitForSeconds(0.2f);
        }
        dialog.undrawChoiceButtons();
    }

    //展示选择按钮内容
    public static IEnumerator showChoiceText(string[] choices)
    {
        DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
        dialog.drawChoiceButtons(choices);
        while (dialog.isRunning(DialogBoxUIHandler.CHOICE_RUNNING))
        {
            yield return new WaitForSeconds(0.2f);
        }
        dialog.undrawChoiceButtons();
    }

    public static List<Texture> loadTexturesFromGif(string texturePath)
    {
        List<Texture> animation = new List<Texture>();
        try
        {
            GifStream gifStream = null;
            string path = Application.streamingAssetsPath + "/" + texturePath;
            if (Application.platform == RuntimePlatform.Android)
            {
                WWW www = new WWW(path);
                while (!www.isDone) { }
                gifStream = new GifStream(www.bytes);
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor
                || Application.platform == RuntimePlatform.OSXEditor)
            {
                gifStream = new GifStream(path);
            }
            if (gifStream == null)
            {
                return animation;
            }
            using (gifStream)
            {
                while (gifStream.HasMoreData)
                {
                    switch (gifStream.CurrentToken)
                    {
                        case GifStream.Token.Image:
                            var image = gifStream.ReadImage();
                            var frame = new Texture2D(
                                gifStream.Header.width,
                                gifStream.Header.height,
                                TextureFormat.ARGB32, false);

                            frame.SetPixels32(image.colors);
                            frame.Apply();

                            animation.Add(frame);
                            //frameDelays.Add(image.SafeDelaySeconds); // More about SafeDelay below
                            break;

                        case GifStream.Token.Comment:
                            var commentText = gifStream.ReadComment();
                            Debug.Log(commentText);
                            break;

                        default:
                            gifStream.SkipToken(); // Other tokens
                            break;
                    }
                }
            }
            //byte[] data = File.ReadAllBytes(path);
            //using (var decoder = new MG.GIF.Decoder(data))
            //{
            //    var img = decoder.NextImage();
            //    while (img != null)
            //    {
            //        animationDic[side].Add(img.CreateTexture());
            //        int delay = img.Delay;
            //        img = decoder.NextImage();
            //    }
            //}
        }
        catch (Exception e)
        {
            Debug.Log("loadTexturesFromGif error:" + e.StackTrace);
        }
        return animation;
    }
}
