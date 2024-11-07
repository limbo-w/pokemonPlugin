using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DialogBoxUIHandler : MonoBehaviour
{
    public static DialogBoxUIHandler main;
    public const int CHOICE_RUNNING = 0;
    public const int BUTTON_CLICK_RUNNING = 1;
    public int defaultDialogLines = 2;

    private GameObject DialogBox;
    private Button DialogBoxButton;
    private Text DialogBoxText;
    private InputField DialogBoxInputField;

    private GameObject ChoiceBox;
    private Button[] ChoiceButtons = new Button[6];
    private Text[] ChoiceTexts = new Text[6];
    public bool hideDialogOnStart = true;
    public bool hideChoiceOnStart = true;
    public int buttonIndex;
    private bool[] running = new bool[2];
    private SettingConfig settingConfig;

    private void Awake()
    {
        if (main == null)
        {
            main = this;
            DialogBox = this.gameObject;
            DialogBox.SetActive(true);
            Button DialogBoxBack = transform.Find("DialogBoxBack").GetComponent<Button>();
            DialogBoxBack.onClick.AddListener(delegate
            {
                stopRunning(BUTTON_CLICK_RUNNING);
            });
            DialogBoxButton = transform.Find("DialogBoxButton").GetComponent<Button>();
            DialogBoxButton.onClick.AddListener(delegate
            {
                stopRunning(BUTTON_CLICK_RUNNING);
            });
            DialogBoxText = transform.Find("DialogBoxBack/BoxText").GetComponent<Text>();
            ChoiceBox = transform.Find("ChoiceBox").gameObject;
            ChoiceBox.SetActive(true);
            DialogBoxInputField = transform.Find("DialogBoxBack/NumberInputField").GetComponent<InputField>();
            DialogBoxInputField.gameObject.SetActive(false);
            for (int i = 0; i < 6; i++)
            {
                string index = i.ToString();
                ChoiceButtons[i] = ChoiceBox.transform.Find("ChoiceButton" + i).GetComponent<Button>();
                ChoiceButtons[i].onClick.AddListener(delegate
                {
                    buttonIndex = int.Parse(index);
                    stopRunning(CHOICE_RUNNING);
                });
                ChoiceButtons[i].gameObject.SetActive(false);
                ChoiceTexts[i] = ChoiceBox.transform.Find("ChoiceButton" + i + "/Text").GetComponent<Text>();
            }

        }
    }

    void Start()
    {
        settingConfig = GlobalPreference.Instance.getPrefSettingConfig();
        if (hideDialogOnStart)
        {
            DialogBox.SetActive(false);
        }
        if (hideChoiceOnStart)
        {
            ChoiceBox.SetActive(false);
        }
    }

    public void stopRunning(int type)
    {
        running[type] = false;
    }

    public bool isRunning(int type)
    {
        return running[type];
    }

    public void drawChoiceButtons(string[] choices)
    {
        running[CHOICE_RUNNING] = true;
        DialogBox.SetActive(true);
        DialogBoxButton.gameObject.SetActive(true);
        ChoiceBox.SetActive(true);
        for (int i = 0; i < 6 && i < choices.Length; i++)
        {
            ChoiceButtons[i].gameObject.SetActive(true);
            int stringIndex = choices.Length - 1 > 5 ? 5 : choices.Length - 1;
            ChoiceTexts[i].text = choices[stringIndex - i];
        }
    }

    public void undrawChoiceButtons()
    {
        DialogBoxInputField.gameObject.SetActive(false);
        running[CHOICE_RUNNING] = false;
        DialogBox.SetActive(false);
        ChoiceBox.SetActive(false);
        for (int i = 0; i < 6 && i < ChoiceTexts.Length; i++)
        {
            ChoiceButtons[i].gameObject.SetActive(false);
        }
    }

    public IEnumerator drawTextSilent(string textLine)
    {
        yield return drawTextSilentNoEnd(textLine);
        running[BUTTON_CLICK_RUNNING] = false;
    }

    public IEnumerator drawTextSilentNoEnd(string textLine)
    {
        running[BUTTON_CLICK_RUNNING] = true;
        DialogBox.SetActive(true);
        DialogBoxButton.gameObject.SetActive(true);
        DialogBoxText.text = "";
        int textSpeed = settingConfig.getTextSpeed() + 1;
        float charPerSec = textSpeed * textSpeed;
        float secPerChar = 1 / charPerSec;
        char[] chars = textLine.ToCharArray();
        for (int i = 0; i < textLine.Length; i++)
        {
            if (chars[i].Equals('\\'))
            {
                DialogBoxText.text += "\n";
            }
            else
            {
                DialogBoxText.text += chars[i].ToString();
            }
            yield return new WaitForSeconds(secPerChar);
        }
    }

    public IEnumerator drawTextSilentWithInputNoEnd(string textLine)
    {
        DialogBoxInputField.gameObject.SetActive(true);
        DialogBoxInputField.text = "1";
        yield return drawTextSilentNoEnd(textLine);
    }

    public int getInputNumber()
    {
        if (string.IsNullOrEmpty(DialogBoxInputField.text))
        {
            return 0;
        }
        return int.Parse(DialogBoxInputField.text);
    }

    public void drawTextInstant(string textLine)
    {
        running[BUTTON_CLICK_RUNNING] = true;
        DialogBox.SetActive(true);
        DialogBoxButton.gameObject.SetActive(true);
        DialogBoxText.text = "";
        char[] chars = textLine.ToCharArray();
        for (int i = 0; i < textLine.Length; i++)
        {
            if (chars[i].Equals('\\'))
            {
                DialogBoxText.text += "\n";
            }
            else
            {
                DialogBoxText.text += chars[i].ToString();
            }
        }
        running[BUTTON_CLICK_RUNNING] = false;
    }

    public void drawDialogBox()
    {
        drawDialogBox(defaultDialogLines);
    }

    public void drawDialogBox(int lines)
    {
        running[BUTTON_CLICK_RUNNING] = true;
        DialogBox.SetActive(true);
        DialogBoxButton.gameObject.SetActive(true);
        DialogBoxText.text = "";
    }

    public void undrawDialogBox()
    {
        DialogBoxInputField.gameObject.SetActive(false);
        running[BUTTON_CLICK_RUNNING] = false;
        DialogBox.SetActive(false);
    }
}
