using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using static CommonUtils;

public class PauseHandler : MonoBehaviour
{
    public enum PauseType
    {
        None,
        Pokedex,
        Party,
        Bag,
        Trainer,
        Save,
        Settings,
        Trade
    }

    private Transform pauseTop;
    private Transform pauseBottom;

    private Text selectedText;
    public Texture2D iconPokedexTex;
    public Texture2D iconPartyTex;
    public Texture2D iconBagTex;
    public Texture2D iconTrainerTex;
    public Texture2D iconSaveTex;
    public Texture2D iconSettingsTex;
    public Texture2D iconTradeTex;

    float halfWidth = UnityEngine.Screen.width / 2.0f;
    float halfHeight = UnityEngine.Screen.height / 2.0f;
    private float originAlpha = 0.65f;
    private float showAlpha = 1f;
    private Vector3 littleScale = new Vector3(0, 0, 0);
    private Vector3 originScale = new Vector3(1, 1, 1);
    private Vector3 showScale = new Vector3(1.2f, 1.2f, 1.2f);

    private Transform iconPokedex;
    private Transform iconParty;
    private Transform iconBag;
    private Transform iconTrainer;
    private Transform iconSave;
    private Transform iconSettings;
    private Transform iconTrade;

    //private AudioSource PauseAudio; 
    public AudioClip selectClip;
    public AudioClip pauseSelect;
    public AudioClip returnClip;
    public AudioClip openClip;

    private PauseType selectedIcon = PauseType.None;
    private Transform targetIcon;

    private bool running;

    void Awake()
    {
        //PauseAudio = transform.GetComponent<AudioSource>();
        pauseTop = transform.Find("PauseTop");
        pauseBottom = transform.Find("PauseBottom");
        selectedText = transform.Find("SelectedText").GetComponent<Text>();
        iconPokedex = transform.Find("IconPokedex");
        iconParty = transform.Find("IconParty");
        iconBag = transform.Find("IconBag");
        iconTrainer = transform.Find("IconTrainer");
        iconSave = transform.Find("IconSave");
        iconSettings = transform.Find("IconSettings");
        iconTrade = transform.Find("IconTrade");
    }

    void Start()
    {
        pauseTop.GetComponent<RectTransform>().rect.Set(0, 192, pauseTop.GetComponent<RectTransform>().rect.width,
            pauseTop.GetComponent<RectTransform>().rect.height);
        pauseBottom.GetComponent<RectTransform>().rect.Set(0, -96, pauseBottom.GetComponent<RectTransform>().rect.width,
            pauseBottom.GetComponent<RectTransform>().rect.height);

        setSelectedText("");
        iconPokedex.GetComponent<Image>().color = new Color(1f, 1f, 1f, originAlpha);
        iconParty.GetComponent<Image>().color = new Color(1f, 1f, 1f, originAlpha);
        iconBag.GetComponent<Image>().color = new Color(1f, 1f, 1f, originAlpha);
        iconTrainer.GetComponent<Image>().color = new Color(1f, 1f, 1f, originAlpha);
        iconSave.GetComponent<Image>().color = new Color(1f, 1f, 1f, originAlpha);
        iconSettings.GetComponent<Image>().color = new Color(1f, 1f, 1f, originAlpha);
        iconTrade.GetComponent<Image>().color = new Color(1f, 1f, 1f, originAlpha);

        selectedIcon = 0;
        this.gameObject.SetActive(false);
    }

    private void setSelectedText(string text)
    {
        selectedText.text = text;
    }

    private IEnumerator openAnim()
    {
        float increment = 1;
        iTween.MoveTo(iconPokedex.gameObject, new Vector3(halfWidth - 60, halfHeight + 85, iconPokedex.position.z), 0.3f);
        iTween.ScaleTo(iconPokedex.gameObject, originScale, 0.01f);
        iTween.MoveTo(iconTrainer.gameObject, new Vector3(halfWidth + 60, halfHeight + 85, iconTrainer.position.z), 0.3f);
        iTween.ScaleTo(iconTrainer.gameObject, originScale, 0.01f);
        iTween.MoveTo(iconSettings.gameObject, new Vector3(halfWidth - 110, halfHeight + 20, iconSettings.position.z), 0.3f);
        iTween.ScaleTo(iconSettings.gameObject, originScale, 0.01f);
        iTween.MoveTo(iconBag.gameObject, new Vector3(halfWidth + 110, halfHeight + 20, iconBag.position.z), 0.3f);
        iTween.ScaleTo(iconBag.gameObject, originScale, 0.01f);
        iTween.MoveTo(iconParty.gameObject, new Vector3(halfWidth + 100, halfHeight - 60, iconParty.position.z), 0.3f);
        iTween.ScaleTo(iconParty.gameObject, originScale, 0.01f);
        iTween.MoveTo(iconSave.gameObject, new Vector3(halfWidth - 100, halfHeight - 60, iconSave.position.z), 0.3f);
        iTween.ScaleTo(iconSave.gameObject, originScale, 0.01f);
        iTween.MoveTo(iconTrade.gameObject, new Vector3(halfWidth, halfHeight - 100, iconSettings.position.z), 0.3f);
        iTween.ScaleTo(iconTrade.gameObject, originScale, 0.01f);
        while (increment < 1)
        {
            increment += (1 / 0.4f) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            pauseBottom.GetComponent<RectTransform>().rect.Set(pauseBottom.GetComponent<RectTransform>().rect.x, -96 + (increment * 96),
                pauseBottom.GetComponent<RectTransform>().rect.width, pauseBottom.GetComponent<RectTransform>().rect.height);
            pauseTop.GetComponent<RectTransform>().rect.Set(pauseTop.GetComponent<RectTransform>().rect.x, 192 - (increment * 96), pauseTop.GetComponent<RectTransform>().rect.width,
                pauseTop.GetComponent<RectTransform>().rect.height);

            yield return null;
        }
    }

    private IEnumerator closeAnim()
    {
        SfxHandler.Play(returnClip);
        setSelectedText("");
        float increment = 1;
        iTween.MoveTo(iconPokedex.gameObject, new Vector3(halfWidth, halfHeight, iconPokedex.position.z), 0.4f);
        iTween.ScaleTo(iconPokedex.gameObject, littleScale, 0.3f);
        iTween.MoveTo(iconParty.gameObject, new Vector3(halfWidth, halfHeight, iconParty.position.z), 0.4f);
        iTween.ScaleTo(iconParty.gameObject, littleScale, 0.3f);
        iTween.MoveTo(iconBag.gameObject, new Vector3(halfWidth, halfHeight, iconBag.position.z), 0.4f);
        iTween.ScaleTo(iconBag.gameObject, littleScale, 0.3f);
        iTween.MoveTo(iconTrainer.gameObject, new Vector3(halfWidth, halfHeight, iconTrainer.position.z), 0.4f);
        iTween.ScaleTo(iconTrainer.gameObject, littleScale, 0.3f);
        iTween.MoveTo(iconSave.gameObject, new Vector3(halfWidth, halfHeight, iconSave.position.z), 0.4f);
        iTween.ScaleTo(iconSave.gameObject, littleScale, 0.3f);
        iTween.MoveTo(iconSettings.gameObject, new Vector3(halfWidth, halfHeight, iconSettings.position.z), 0.4f);
        iTween.ScaleTo(iconSettings.gameObject, littleScale, 0.3f);
        iTween.MoveTo(iconTrade.gameObject, new Vector3(halfWidth, halfHeight, iconTrade.position.z), 0.4f);
        iTween.ScaleTo(iconTrade.gameObject, littleScale, 0.3f);
        while (increment < 1)
        {
            increment += (1 / 0.4f) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            pauseBottom.GetComponent<RectTransform>().rect.Set(pauseBottom.GetComponent<RectTransform>().rect.x, 0 - (increment * 96),
                pauseBottom.GetComponent<RectTransform>().rect.width, pauseBottom.GetComponent<RectTransform>().rect.height);
            pauseTop.GetComponent<RectTransform>().rect.Set(pauseTop.GetComponent<RectTransform>().rect.x, 96 + (increment * 96),
                pauseTop.GetComponent<RectTransform>().rect.width,
                pauseTop.GetComponent<RectTransform>().rect.height);
            yield return null;
        }
    }


    public IEnumerator animActiveIcon()
    {
        iTween.ScaleTo(targetIcon.gameObject, showScale, 0.25f);
        targetIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, showAlpha);
        yield return null;
    }

    private IEnumerator updateIcon()
    {
        if (targetIcon != null)
        {
            iTween.ScaleTo(targetIcon.gameObject, originScale, 0.25f);
            targetIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, originAlpha);
        }
        if (selectedIcon == PauseType.Pokedex)
        {
            targetIcon = iconPokedex;
            setSelectedText("Pokédex");
        }
        else if (selectedIcon == PauseType.Party)
        {
            targetIcon = iconParty;
            setSelectedText("Pokémon Party");
        }
        else if (selectedIcon == PauseType.Bag)
        {
            targetIcon = iconBag;
            setSelectedText("Bag");
        }
        else if (selectedIcon == PauseType.Trainer)
        {
            targetIcon = iconTrainer;
            setSelectedText(TrainerManager.Instance.getTrainerInfoData().playerName);
        }
        else if (selectedIcon == PauseType.Save)
        {
            targetIcon = iconSave;
            setSelectedText("Save Game");
        }
        else if (selectedIcon == PauseType.Settings)
        {
            targetIcon = iconSettings;
            setSelectedText("Settings");
        }
        else if (selectedIcon == PauseType.Trade)
        {
            targetIcon = iconTrade;
            setSelectedText("Trade");
        }
        else
        {
            targetIcon = null;
            setSelectedText("");
        }
        if (targetIcon != null)
        {
            StopCoroutine("animActiveIcon");
            StartCoroutine("animActiveIcon");
        }
        yield return null;
    }

    private IEnumerator fadeIcons(float speed)
    {
        float increment = 0f;
        while (increment < 1)
        {
            increment += (1 / speed * 1.2f) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            iconPokedex.GetComponent<Image>().color = new Color(1f - increment, 1f - increment, 1f - increment, 1f);
            iconParty.GetComponent<Image>().color = new Color(1f - increment, 1f - increment, 1f - increment, 1f);
            iconBag.GetComponent<Image>().color = new Color(1f - increment, 1f - increment, 1f - increment, 1f);
            iconTrainer.GetComponent<Image>().color = new Color(1f - increment, 1f - increment, 1f - increment, 1f);
            iconSave.GetComponent<Image>().color = new Color(1f - increment, 1f - increment, 1f - increment, 1f);
            iconSettings.GetComponent<Image>().color = new Color(1f - increment, 1f - increment, 1f - increment, 1f);
            iconTrade.GetComponent<Image>().color = new Color(1f - increment, 1f - increment, 1f - increment, 1f);
            yield return null;
        }
    }

    private IEnumerator unfadeIcons(float speed)
    {
        float increment = 0f;
        while (increment < 1)
        {
            increment += (1 / speed * 1.2f) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            iconPokedex.GetComponent<Image>().color = new Color(increment, increment, increment, 1f);
            iconParty.GetComponent<Image>().color = new Color(increment, increment, increment, 1f);
            iconBag.GetComponent<Image>().color = new Color(increment, increment, increment, 1f);
            iconTrainer.GetComponent<Image>().color = new Color(increment, increment, increment, 1f);
            iconSave.GetComponent<Image>().color = new Color(increment, increment, increment, 1f);
            iconSettings.GetComponent<Image>().color = new Color(increment, increment, increment, 1f);
            iconTrade.GetComponent<Image>().color = new Color(increment, increment, increment, 1f);
            yield return null;
        }
    }

    public IEnumerator control()
    {
        StartCoroutine("updateIcon", selectedIcon);
        SfxHandler.Play(openClip);
        yield return StartCoroutine("openAnim");
        running = true;
        while (running)
        {
            Vector2 movement = Scene.main.playerInput.Player.Move.ReadValue<Vector2>();
            DpadDiction dpadDiction = CommonUtils.getDpadDiction(movement);
            if (selectedIcon == PauseType.None)
            {
                if (dpadDiction == DpadDiction.Up)
                {
                    selectedIcon = PauseType.Pokedex;
                    StartCoroutine("updateIcon");
                    SfxHandler.Play(pauseSelect);
                }
                else if (dpadDiction == DpadDiction.Left)
                {
                    selectedIcon = PauseType.Settings;
                    StartCoroutine("updateIcon");
                    SfxHandler.Play(pauseSelect);
                }
                else if (dpadDiction == DpadDiction.Down)
                {
                    selectedIcon = PauseType.Trade;
                    StartCoroutine("updateIcon");
                    SfxHandler.Play(pauseSelect);
                }
                else if (dpadDiction == DpadDiction.Right)
                {
                    selectedIcon = PauseType.Bag;
                    StartCoroutine("updateIcon");
                    SfxHandler.Play(pauseSelect);
                }
            }
            else
            {
                if (dpadDiction == DpadDiction.Up)
                {
                    if (selectedIcon == PauseType.Save)
                    {
                        selectedIcon = PauseType.Settings;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Party)
                    {
                        selectedIcon = PauseType.Bag;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Bag)
                    {
                        selectedIcon = PauseType.Trainer;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Settings)
                    {
                        selectedIcon = PauseType.Pokedex;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                }
                else if (dpadDiction == DpadDiction.Right)
                {
                    if (selectedIcon == PauseType.Save)
                    {
                        selectedIcon = PauseType.Trade;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Trade)
                    {
                        selectedIcon = PauseType.Party;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Pokedex)
                    {
                        selectedIcon = PauseType.Trainer;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Settings)
                    {
                        selectedIcon = PauseType.Pokedex;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Trainer)
                    {
                        selectedIcon = PauseType.Bag;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                }
                else if (dpadDiction == DpadDiction.Down)
                {
                    if (selectedIcon == PauseType.Settings)
                    {
                        selectedIcon = PauseType.Save;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Bag)
                    {
                        selectedIcon = PauseType.Party;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Trainer)
                    {
                        selectedIcon = PauseType.Bag;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Pokedex)
                    {
                        selectedIcon = PauseType.Settings;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Save)
                    {
                        selectedIcon = PauseType.Trade;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Party)
                    {
                        selectedIcon = PauseType.Trade;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                }
                else if (dpadDiction == DpadDiction.Left)
                {
                    if (selectedIcon == PauseType.Trade)
                    {
                        selectedIcon = PauseType.Save;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Party)
                    {
                        selectedIcon = PauseType.Trade;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Trainer)
                    {
                        selectedIcon = PauseType.Pokedex;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Pokedex)
                    {
                        selectedIcon = PauseType.Settings;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                    else if (selectedIcon == PauseType.Bag)
                    {
                        selectedIcon = PauseType.Trainer;
                        StartCoroutine("updateIcon");
                        SfxHandler.Play(pauseSelect);
                    }
                }
            }
            bool AButtonPressed = Scene.main.playerInput.Player.AButton.IsPressed();
            bool BButtonPressed = Scene.main.playerInput.Player.BButton.IsPressed();
            if (AButtonPressed)
            {
                if (selectedIcon == PauseType.Pokedex)
                {
                    //已经拿到图鉴
                    if (TrainerManager.Instance.getTaskValue("getPokedex") > 0)
                    {
                        //Pokedex
                        SfxHandler.Play(selectClip);
                        yield return StartCoroutine(ScreenFade.main.Fade(false, 0.4f));

                        GameObject pokedex = Scene.main.Pokedex.gameObject;
                        pokedex.SetActive(true);
                        yield return StartCoroutine(Scene.main.Pokedex.control());
                        while (pokedex.activeSelf)
                        {
                            yield return null;
                        }
                        Scene.main.SetMapButtonVisible(true);
                        yield return StartCoroutine(ScreenFade.main.Fade(true, 0.4f));
                    }
                    else
                    {
                        yield return StartCoroutine(CommonUtils.showDialogText("你还未获得图鉴"));
                    }
                }
                else if (selectedIcon == PauseType.Party)
                {
                    //Party
                    SfxHandler.Play(selectClip);
                    yield return StartCoroutine(ScreenFade.main.Fade(false, 0.4f));

                    GameObject party = Scene.main.Party.gameObject;
                    party.SetActive(true);
                    yield return StartCoroutine(Scene.main.Party.control());
                    while (party.activeSelf)
                    {
                        yield return null;
                    }
                    Scene.main.SetMapButtonVisible(true);
                    yield return StartCoroutine(ScreenFade.main.Fade(true, 0.4f));
                }
                else if (selectedIcon == PauseType.Bag)
                {
                    //Bag
                    SfxHandler.Play(selectClip);
                    yield return StartCoroutine(ScreenFade.main.Fade(false, 0.4f));

                    GameObject bag = Scene.main.Bag.gameObject;
                    bag.SetActive(true);
                    yield return StartCoroutine(Scene.main.Bag.control());
                    while (bag.activeSelf)
                    {
                        yield return null;
                    }
                    Scene.main.SetMapButtonVisible(true);
                    yield return StartCoroutine(ScreenFade.main.Fade(true, 0.4f));
                }
                else if (selectedIcon == PauseType.Trainer)
                {
                    //TrainerCard
                    SfxHandler.Play(selectClip);
                    yield return StartCoroutine(ScreenFade.main.Fade(false, 0.4f));

                    GameObject trainer = Scene.main.Trainer.gameObject;
                    trainer.SetActive(true);
                    yield return StartCoroutine(Scene.main.Trainer.control());
                    while (trainer.activeSelf)
                    {
                        yield return null;
                    }
                    Scene.main.SetMapButtonVisible(true);
                    yield return StartCoroutine(ScreenFade.main.Fade(true, 0.4f));
                }
                else if (selectedIcon == PauseType.Save)
                {
                    //Save
                    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
                    yield return StartCoroutine(dialog.drawTextSilent("要保存当前游戏进度吗?"));
                    string[] choices = { "是", "否", };
                    yield return StartCoroutine(CommonUtils.showChoiceText(choices));
                    int chosenIndex = dialog.buttonIndex;
                    if (chosenIndex == 1)
                    {
                        //PlayerMovement.player.pauseInput();
                        bool recordCompleted = false;
                        yield return StartCoroutine(dialog.drawTextSilent("记录开始"));
                        yield return new WaitForSeconds(0.5f);
                        yield return StartCoroutine(dialog.drawTextSilent(".............."));
                        yield return new WaitForSeconds(0.5f);
                        yield return StartCoroutine(dialog.drawTextSilent("...................."));
                        yield return new WaitForSeconds(0.5f);
                        yield return StartCoroutine(dialog.drawTextSilent(".........................."));
                        yield return new WaitForSeconds(0.5f);
                        TrainerManager.Instance.recordPlayerConfig(GlobalVariables.global.savePlayerConfig(), delegate (bool result) {
                            if (result)
                            {
                                StartCoroutine(CommonUtils.showDialogText("游戏进度记录完成.........."));
                            }
                            else
                            {
                                StartCoroutine(CommonUtils.showDialogText("游戏进度记录失败.........."));
                            }
                            recordCompleted = true;
                        });
                        while (!recordCompleted)
                        {
                            yield return null;
                        }
                        //PlayerMovement.player.unpauseInput();

                    }
                    else
                    {

                    }
                }
                else if (selectedIcon == PauseType.Settings)
                {
                    //Settings
                    SfxHandler.Play(selectClip);
                    yield return StartCoroutine(ScreenFade.main.Fade(false, 0.4f));

                    GameObject settings = Scene.main.Settings.gameObject;
                    settings.SetActive(true);
                    yield return StartCoroutine(Scene.main.Settings.control());
                    while (settings.activeSelf)
                    {
                        yield return null;
                    }
                    Scene.main.SetMapButtonVisible(true);
                    yield return StartCoroutine(ScreenFade.main.Fade(true, 0.4f));
                }
                else if (selectedIcon == PauseType.Trade)
                {
                    //Trade
                    SfxHandler.Play(selectClip);
                    yield return StartCoroutine(ScreenFade.main.Fade(false, 0.4f));

                    GameObject trade = Scene.main.Trade.gameObject;
                    trade.SetActive(true);
                    yield return StartCoroutine(Scene.main.Trade.control());
                    while (trade.activeSelf)
                    {
                        yield return null;
                    }
                    Scene.main.SetMapButtonVisible(true);
                    yield return StartCoroutine(ScreenFade.main.Fade(true, 0.4f));

                }
            }
            else if (BButtonPressed)
            {
                running = false;
            }
            yield return null;
        }

        yield return StartCoroutine("closeAnim");
        yield return new WaitForSeconds(0.15f);
        this.gameObject.SetActive(false);
    }
}