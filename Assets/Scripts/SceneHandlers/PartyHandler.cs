using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Loxodon.Framework.Views;
using Loxodon.Framework.Views.Variables;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Binding;
using System.Globalization;
using Loxodon.Framework.Localizations;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding.Builder;
using Loxodon.Framework.Observables;
using System.Collections.Specialized;

public class PartyHandler : UIView
{
    public VariableArray variables;
    private PartyViewModel viewModel;

    public Texture selectBallOpen;
    public Texture selectBallClosed;
    public Texture unSlotCheck;
    public Texture slotCheck;
    public Texture unItemCheck;
    public Texture itemCheck;

    //private AudioSource PartyAudio;
    public AudioClip selectClip;

    protected override void Awake()
    {
        //PartyAudio = transform.GetComponent<AudioSource>();
        viewModel = new PartyViewModel(this,OnTechShotInfoChanged, OnSlotShotInfoChanged);
    }

    protected override void Start()
    {
        IBindingContext bindingContext = this.BindingContext();
        bindingContext.DataContext = viewModel;
        BindingSet<PartyHandler, PartyViewModel> bindingSet = this.CreateBindingSet<PartyHandler, PartyViewModel>();
        for (int i = 0; i < 4; i++)
        {
            bindingSet.Bind(variables.Get<Button>("techSlot" + i)).For(v => v.onClick)
                .To(vm => vm.OnTechShot).CommandParameter(i); ;
        }
        for (int i = 0; i < 6; i++)
        {
            bindingSet.Bind(variables.Get<Button>("slot" + i)).For(v => v.onClick)
                .To(vm => vm.OnSlotShot).CommandParameter(i); ;
        }
        bindingSet.Bind(this.variables.Get<Button>("return")).For(v => v.onClick).To(vm => vm.OnReturn);
        bindingSet.Bind(this.variables.Get<Text>("characterInfo")).For(v => v.text).To(vm => vm.CharcterInfo);
        bindingSet.Build();
        this.gameObject.SetActive(false);
    }

    protected void OnTechShotInfoChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (PartyViewModel.TechShotInfo item in eventArgs.NewItems)
                {
                    RawImage rawImage = variables.Get<RawImage>("techSlotImage" + item.Index);
                    if (item.Texture != null)
                    {
                        rawImage.texture = item.Texture;
                    }
                    if (item.Scale != null)
                    {
                        iTween.ScaleTo(rawImage.gameObject, item.Scale, 0.1f);
                    }
                    Button button = variables.Get<Button>("techSlot" + item.Index);
                    button.gameObject.SetActive(item.IsObjectActive);
                    if (item.TechShotName != null)
                    {
                        variables.Get<Text>("techShotName" + item.Index).text = item.TechShotName;
                    }
                    if (item.TechShotNum != null)
                    {
                        variables.Get<Text>("techShotNum" + item.Index).text = item.TechShotNum;
                    }
                }
                break;
        }
    }

    protected void OnSlotShotInfoChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (PartyViewModel.SlotInfo item in eventArgs.NewItems)
                {
                    RawImage rawImage = variables.Get<RawImage>("slotImage" + item.Index);
                    if (item.Texture != null)
                    {
                        rawImage.texture = item.Texture;
                    }
                    iTween.ScaleTo(rawImage.gameObject, item.Scale, 0.1f);
                    Button button = variables.Get<Button>("slot" + item.Index);
                    button.gameObject.SetActive(true);
                    if (item.Postion != Vector3.zero)
                    {
                        button.transform.position = item.Postion;
                    }
                    RawImage selectBallImage = variables.Get<RawImage>("selectBall" + item.Index);
                    if (item.SelectBallTexture != null)
                    {
                        selectBallImage.texture = item.SelectBallTexture;
                    }
                    variables.Get<Text>("name" + item.Index).color = item.Color;
                    variables.Get<Text>("maxHP" + item.Index).color = item.Color;
                    variables.Get<Text>("currentHP" + item.Index).color = item.Color;
                    variables.Get<Text>("slash" + item.Index).color = item.Color;
                    variables.Get<Text>("level" + item.Index).color = item.Color;
                    if (item.PokemonName != null)
                    {
                        variables.Get<Text>("name" + item.Index).text = item.PokemonName;
                    }
                    if (item.MaxHp != null)
                    {
                        variables.Get<Text>("maxHP" + item.Index).text = item.MaxHp;
                    }
                    if (item.CurrentHp != null)
                    {
                        variables.Get<Text>("currentHP" + item.Index).text = item.CurrentHp;
                    }
                    if (item.Level != null)
                    {
                        variables.Get<Text>("level" + item.Index).text = item.Level;
                    }
                    if (item.IconTexture != null)
                    {
                        variables.Get<RawImage>("icon" + item.Index).texture = item.IconTexture;
                    }
                    RawImage statusImage = variables.Get<RawImage>("status" + item.Index);
                    if (item.StatusTexture != null)
                    {
                        statusImage.gameObject.SetActive(true);
                        statusImage.texture = item.StatusTexture;
                    }
                    else
                    {
                        statusImage.gameObject.SetActive(false);
                    }
                    if (item.Gender != null)
                    {
                        variables.Get<Text>("gender" + item.Index).text = item.Gender;
                    }
                    Image hpBar = variables.Get<Image>("hPBar" + item.Index);
                    hpBar.GetComponent<RectTransform>().localScale = item.HpScale;
                    if (item.HpScale.x < 0.35f)
                    {
                        hpBar.color = Color.red;
                    }
                    else if (item.HpScale.x < 0.65f)
                    {
                        hpBar.color = Color.yellow;
                    }
                    else
                    {
                        hpBar.color = Color.green;
                    }
                    variables.Get<Image>("item" + item.Index).enabled = item.ItemEnable;
                }
                break;
        }
    }

    //图标的动画
    private IEnumerator animateIcons()
    {
        float interTime = 0.2f;
        while (viewModel.running)
        {
            if (viewModel.partyPosition >= 0)
            {
                RawImage icon = variables.Get<RawImage>("icon" + viewModel.partyPosition);
                StartCoroutine(CommonUtils.StartUVAnimation(icon, interTime));
            }
            yield return new WaitForSeconds(interTime * 2);
        }
        yield return null;
    }

    public IEnumerator control()
    {
        Scene.main.SetMapButtonVisible(false);
        StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
        viewModel.running = true;
        //更新item
        yield return StartCoroutine(viewModel.SetItemPosition(-1));
        //更新slot
        yield return StartCoroutine(viewModel.SetSlotPosition(-1));
        StartCoroutine("animateIcons");
        PCManager.Instance.packParty();
        while (viewModel.running)
        {
            yield return null;
        }
        StopCoroutine("animateIcons");
        yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
        //todo增加一个复位操作
        GlobalVariables.global.resetFollower();
        this.gameObject.SetActive(false);
    }
}