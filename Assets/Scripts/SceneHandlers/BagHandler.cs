using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Loxodon.Framework.Views;
using Loxodon.Framework.Views.Variables;
using System.Collections.Specialized;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding.Builder;
using Loxodon.Framework.Binding;

public class BagHandler : UIView
{
    public VariableArray variables;
    private BagItemViewModel itemViewModel;
    private BagPartyViewModel partyViewModel;
    private BagShopViewModel shopViewModel;
    public Texture partySlotTex, partySlotSelectedTex;
    public Texture itemListTex, itemListHighlightTex;

    private AudioSource bagAudio;
    public AudioClip selectClip;
    public AudioClip healClip;
    public AudioClip tmBootupClip;
    public AudioClip forgetMoveClip;
    public AudioClip saleClip;

    protected override void Awake()
    {
        itemViewModel = new BagItemViewModel(this,OnItemIconInfoChanged);
        partyViewModel = new BagPartyViewModel(this, OnPokeIconInfoChanged);
        partyViewModel.itemViewModel = itemViewModel;
        shopViewModel = new BagShopViewModel(this);
        shopViewModel.itemViewModel = itemViewModel;
        itemViewModel.shopViewModel = shopViewModel;
        itemViewModel.partyViewModel = partyViewModel;

        bagAudio = transform.GetComponent<AudioSource>();
    }

    protected override void Start()
    {
        IBindingContext bindingContext = this.BindingContext();
        bindingContext.DataContext = itemViewModel;
        BindingSet<BagHandler, BagItemViewModel> itemBindingSet = this.CreateBindingSet<BagHandler, BagItemViewModel>();
        itemBindingSet.Bind(variables.Get<GameObject>("party"))
                 .For(v => v.activeSelf).To(vm => vm.partyViewModel.PartyVisible);
        itemBindingSet.Bind(variables.Get<GameObject>("shop"))
                 .For(v => v.activeSelf).To(vm => vm.shopViewModel.ShopVisible);
        for (int i = 0; i < 6; i++)
        {
            itemBindingSet.Bind(variables.Get<Button>("slot" + i))
                 .For(v => v.onClick).To(vm => vm.partyViewModel.OnParty).CommandParameter(i);
        }
        for (int i = 0; i < 7; i++)
        {
            itemBindingSet.Bind(variables.Get<Button>("item" + i))
                 .For(v => v.onClick).To(vm => vm.OnItem).CommandParameter(i);
        }
        for (int i = 0; i < 5; i++)
        {
            itemBindingSet.Bind(variables.Get<Button>("bag" + (i + 1)))
                 .For(v => v.onClick).To(vm => vm.OnBag).CommandParameter(i);
        }
        itemBindingSet.Bind(variables.Get<Button>("up"))
                 .For(v => v.onClick).To(vm => vm.OnUp);
        itemBindingSet.Bind(variables.Get<Button>("down"))
                 .For(v => v.onClick).To(vm => vm.OnDown);
        itemBindingSet.Bind(variables.Get<Button>("left"))
                 .For(v => v.onClick).To(vm => vm.OnLeft);
        itemBindingSet.Bind(variables.Get<Button>("right"))
                 .For(v => v.onClick).To(vm => vm.OnRight);
        itemBindingSet.Bind(variables.Get<Button>("return"))
                 .For(v => v.onClick).To(vm => vm.OnReturn);
        itemBindingSet.Bind(variables.Get<Text>("itemDescription"))
                 .For(v => v.text).To(vm => vm.ItemDescription).TwoWay();
        itemBindingSet.Bind(variables.Get<GameObject>("itemDescriptionObject"))
                 .For(v => v.activeSelf).To(vm => vm.ItemDescriptionShow);
        itemBindingSet.Bind(variables.Get<GameObject>("numbersBox"))
                 .For(v => v.activeSelf).To(vm => vm.shopViewModel.NumbersBoxShow);
        itemBindingSet.Bind(variables.Get<GameObject>("moneyBox"))
                 .For(v => v.activeSelf).To(vm => vm.shopViewModel.MoneyBoxShow);
        itemBindingSet.Bind(variables.Get<GameObject>("dataBox"))
                 .For(v => v.activeSelf).To(vm => vm.shopViewModel.DataBoxShow);
        itemBindingSet.Bind(variables.Get<Text>("numberText"))
                 .For(v => v.text).To(vm => vm.shopViewModel.NumberText);
        itemBindingSet.Bind(variables.Get<Text>("moneyText"))
                 .For(v => v.text).To(vm => vm.shopViewModel.MoneyText);
        itemBindingSet.Build();
        this.gameObject.SetActive(false);
    }

    protected void OnPokeIconInfoChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BagPartyViewModel.PokeIconInfo item in eventArgs.NewItems)
                {
                    RawImage slot = transform.Find("Party/Slot" + item.Index).GetComponent<RawImage>();
                    slot.gameObject.SetActive(item.IsActive);
                    if (item.PartySlotTex != null)
                    {
                        slot.texture = item.PartySlotTex;
                    }
                    iTween.ScaleTo(slot.gameObject, item.Scale, 0.1f);

                    RawImage icon = transform.Find("Party/Slot" + item.Index + "/Icon").GetComponent<RawImage>();
                    if (item.IconTex != null)
                    {
                        icon.texture = item.IconTex;
                    }

                    Text name = transform.Find("Party/Slot" + item.Index + "/Name").GetComponent<Text>();
                    name.color = item.Color;
                    if (!string.IsNullOrEmpty(item.PokemonName))
                    {
                        name.text = item.PokemonName;
                    }         

                    Text gender = transform.Find("Party/Slot" + item.Index + "/Gender").GetComponent<Text>();
                    gender.color = item.Color;
                    if (!string.IsNullOrEmpty(item.Gender))
                    {
                        gender.text = item.Gender;
                    }

                    if (item.HpScale != Vector3.zero)
                    {
                        Image partyHPBarImage = transform.Find("Party/Slot" + item.Index + "/StandardDisplay/HPBar").GetComponent<Image>();
                        partyHPBarImage.GetComponent<RectTransform>().localScale = item.HpScale;
                        if (item.HpScale.x < 0.35f)
                        {
                            partyHPBarImage.color = Color.red;
                        }
                        else if (item.HpScale.x < 0.65f)
                        {
                            partyHPBarImage.color = Color.yellow;
                        }
                        else
                        {
                            partyHPBarImage.color = Color.green;
                        }
                    }

                    Image lvImage = transform.Find("Party/Slot" + item.Index + "/StandardDisplay/Lv").GetComponent<Image>();
                    Text level = transform.Find("Party/Slot" + item.Index + "/StandardDisplay/Level").GetComponent<Text>();
                    level.color = item.Color;
                    if (!string.IsNullOrEmpty(item.Level))
                    {
                        level.text = item.Level;
                    }

                    Text textDisplay = transform.Find("Party/Slot" + item.Index + "/TextDisplay").GetComponent<Text>();
                    textDisplay.color = item.PartyTextDisplayColor;
                    if (!string.IsNullOrEmpty(item.PartyTextDisplay))
                    {
                        textDisplay.text = item.PartyTextDisplay;
                    }

                    RawImage status = transform.Find("Party/Slot" + item.Index + "/Status").GetComponent<RawImage>();
                    if (item.StatusTexture != null)
                    {
                        status.gameObject.SetActive(true);
                        status.texture = item.StatusTexture;
                    }
                    else
                    {
                        status.gameObject.SetActive(false);
                    }
                    Image itemImage = transform.Find("Party/Slot" + item.Index + "/Item").GetComponent<Image>();
                    itemImage.enabled = item.ItemEnable;
                }
                break;
        }
    }

    protected void OnItemIconInfoChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BagItemViewModel.ItemIconInfo item in eventArgs.NewItems)
                {
                    RawImage slot = transform.Find("ItemList/Item" + item.Index).GetComponent<RawImage>();
                    Text itemName = transform.Find("ItemList/Item" + item.Index + "/Name").GetComponent<Text>();
                    Text quantity = transform.Find("ItemList/Item" + item.Index + "/Quantity").GetComponent<Text>();
                    itemName.color = item.Color;
                    quantity.color = item.Color;
                    //如果有texture背景直接返回
                    if (item.ItemSlotTex != null)
                    {
                        slot.texture = item.ItemSlotTex;
                        return;
                    }
                    slot.gameObject.SetActive(item.IsActive);
                    itemName.text = item.Name;
                    RawImage icon = transform.Find("ItemList/Item" + item.Index + "/Icon").GetComponent<RawImage>();
                    if (item.IconTex)
                    {
                        icon.texture = item.IconTex;
                    }
                    quantity.text = item.ItemQuantity;
                }
                break;
        }
    }

    public IEnumerator control()
    {
        itemViewModel.partyViewModel.PartyVisible = true;
        itemViewModel.shopViewModel.ShopVisible = false;
        itemViewModel.battleViewModel = null;
        partyViewModel.battleViewModel = null;
        yield return StartCoroutine(control(true, false, false, null));
    }

    public IEnumerator control(BattleViewModel battleViewModel)
    {
        itemViewModel.partyViewModel.PartyVisible = true;
        itemViewModel.shopViewModel.ShopVisible = false;
        itemViewModel.battleViewModel = battleViewModel;
        partyViewModel.battleViewModel = battleViewModel;
        yield return StartCoroutine(control(true, false, false, null));
    }

    //商店专用
    public IEnumerator control(BagItem[] shopStock)
    {
        itemViewModel.partyViewModel.PartyVisible = false;
        itemViewModel.shopViewModel.ShopVisible = true;
        itemViewModel.battleViewModel = null;
        partyViewModel.battleViewModel = null;
        yield return StartCoroutine(control(false, false, true, shopStock));
    }

    private IEnumerator control(bool partyAccessible, bool getItem, bool setShopMode, BagItem[] shopStock)
    {
        Scene.main.SetMapButtonVisible(false);
        itemViewModel.running = true;
        StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
        BagManager.Instance.fetchCurrentGoodList(delegate (bool result)
        {
            if (result)
            {
                itemViewModel.shopViewModel.reset(setShopMode, shopStock);
                StartCoroutine(ResetControl());
            }
        });
        yield return StartCoroutine("animateIcons");
        while (itemViewModel.running)
        {
            yield return null;
        }
        StopCoroutine("animateIcons");
        yield return StartCoroutine(EndControl());
        if (itemViewModel.battleViewModel == null)
        {
            yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
        }
        GlobalVariables.global.resetFollower();
        gameObject.SetActive(false);
    }

    public IEnumerator EndControl()
    {
        itemViewModel.clear();
        partyViewModel.clear();
        yield return new WaitForSeconds(0.2f);
    }

    //结束控制
    public IEnumerator ResetControl()
    {
        itemViewModel.updateItemInfo();
        partyViewModel.updatePartyInfo();
        yield return null;
    }

    public BagItem getChosenItem()
    {
        return itemViewModel.chosenItem;
    }

    //图标的动画
    private IEnumerator animateIcons()
    {
        float interTime = 0.2f;
        while (itemViewModel.running)
        {
            if (partyViewModel.partyPosition >= 0)
            {
                RawImage icon = transform.Find("Party/Slot" + partyViewModel.partyPosition + "/Icon").GetComponent<RawImage>();
                StartCoroutine(CommonUtils.StartUVAnimation(icon, interTime));
            }
            yield return new WaitForSeconds(interTime * 2);
        }
        yield return null;
    }
}