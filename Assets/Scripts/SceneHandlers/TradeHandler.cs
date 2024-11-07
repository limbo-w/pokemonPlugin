using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Proyecto26;
using System;
using Loxodon.Framework.Views.Variables;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Binding.Builder;
using System.Collections.Specialized;

public class TradeHandler : MonoBehaviour
{
    private Transform itemList;
    private RawImage[] itemSlot = new RawImage[visableSlots];
    private Text[] itemName = new Text[visableSlots];
    private Text[] itemLv = new Text[visableSlots];
    private RawImage[] pokeIcon = new RawImage[visableSlots];
    private RawImage[] itemIcon = new RawImage[visableSlots];
    private RawImage[] itemPricePic = new RawImage[visableSlots];
    private Text[] itemX = new Text[visableSlots];
    private Text[] itemQuantity = new Text[visableSlots];
    private RawImage search;
    private RawImage tradeChoiceGood;
    private Text tradeChoiceGoodText;
    private RawImage tradeChoicePoke;
    private Text tradeChoicePokeText;
    private RawImage tradeChoiceMine;
    private Text tradeChoiceMineText;
    private RawImage tradeSale;
    private RawImage tradeThings;

    private Button doSellButton;
    private Transform sellItemList;
    private Transform tradeBuyBack;
    private RawImage tradeBuyBackTex;
    private Text tradeBuyBackText;
    private Transform tradeInfoBack;
    private RawImage tradeInfoBackTex;
    private Text tradeInfoBackText;
    public InputField sellMoneyInputField;
    public InputField sellDiamondInputField;
    private Text pageInfo;

    //是否选中物品
    private bool isSelected = false;
    //可视化卡槽
    public static int visableSlots = 14;

    public VariableArray variables;
    private TradeViewModel tradeViewModel;

    public AudioClip selectClip;

    void Awake()
    {
        search = transform.Find("Search").GetComponent<RawImage>();
        tradeChoiceGood = transform.Find("TradeChoiceGood").GetComponent<RawImage>();
        tradeChoiceGoodText = transform.Find("TradeChoiceGood/TradeChoiceGoodText").GetComponent<Text>();
        tradeChoicePoke = transform.Find("TradeChoicePoke").GetComponent<RawImage>();
        tradeChoicePokeText = transform.Find("TradeChoicePoke/TradePokemonBallText").GetComponent<Text>();
        tradeChoiceMine = transform.Find("TradeChoiceMy").GetComponent<RawImage>();
        tradeChoiceMineText = transform.Find("TradeChoiceMy/TradeChoiceMyText").GetComponent<Text>();
        tradeSale = transform.Find("TradeSale").GetComponent<RawImage>();
        tradeThings = transform.Find("TradeThings").GetComponent<RawImage>();
        itemList = transform.Find("ItemList");
        pageInfo = itemList.Find("TradeChangeBack/PageInfo").GetComponent<Text>();
        for (int i = 0; i < visableSlots; i++)
        {
            itemSlot[i] = itemList.Find("Item" + i).GetComponent<RawImage>();
            itemName[i] = itemSlot[i].transform.Find("Name").GetComponent<Text>();
            pokeIcon[i] = itemSlot[i].transform.Find("PokeIcon").GetComponent<RawImage>();
            itemIcon[i] = itemSlot[i].transform.Find("ItemIcon").GetComponent<RawImage>();
            itemLv[i] = itemSlot[i].transform.Find("Lv").GetComponent<Text>();
            itemPricePic[i] = itemSlot[i].transform.Find("Price/PriceCoin").GetComponent<RawImage>();
            itemX[i] = itemSlot[i].transform.Find("X").GetComponent<Text>();
            itemQuantity[i] = itemSlot[i].transform.Find("Quantity").GetComponent<Text>();
        }
        sellItemList = transform.Find("SellItemList");
        tradeBuyBack = transform.Find("TradeBuyBack");
        tradeBuyBackText = tradeBuyBack.Find("TradeBuyBackText").GetComponent<Text>();
        tradeBuyBackTex = tradeBuyBack.GetComponent<RawImage>();
        tradeInfoBack = transform.Find("TradeInfoBack");
        tradeInfoBackText = tradeInfoBack.Find("TradeInfoBackText").GetComponent<Text>();
        tradeInfoBackTex = tradeInfoBack.GetComponent<RawImage>();
        sellDiamondInputField = sellItemList.Find("SellDiamondInputField").GetComponent<InputField>();
        sellDiamondInputField.onValueChanged.AddListener(sellDiamondChangedValue);
        sellMoneyInputField = sellItemList.Find("SellMoneyInputField").GetComponent<InputField>();
        sellMoneyInputField.onValueChanged.AddListener(sellMoneyChangedValue);


        TradeViewModel.NotifyCollectionChangedObject changedObject = new TradeViewModel.NotifyCollectionChangedObject();
        changedObject.buyOrSellHandler = OnBuyOrSellChanged;
        changedObject.itemClickHandler = OnItemClicked;
        changedObject.screenHandler = OnScreenChanged;
        changedObject.itemListHandler = OnItemListChanged;
        changedObject.pokemonListHandler = OnPokemonListChanged;
        tradeViewModel = new TradeViewModel(this, changedObject);
    }

    void Start()
    {
        tradeBuyBack.gameObject.SetActive(false);
        tradeInfoBack.gameObject.SetActive(false);
        setSellItemListVisible(false);

        IBindingContext bindingContext = this.BindingContext();
        bindingContext.DataContext = tradeViewModel;
        BindingSet<TradeHandler, TradeViewModel> bindingSet = this.CreateBindingSet<TradeHandler, TradeViewModel>();
        bindingSet.Bind(variables.Get<Button>("return")).For(v => v.onClick).To(vm => vm.OnReturnButton);
        bindingSet.Bind(variables.Get<Button>("buy")).For(v => v.onClick).To(vm => vm.OnBuyButton);
        bindingSet.Bind(variables.Get<Button>("sell")).For(v => v.onClick).To(vm => vm.OnSellButton);
        bindingSet.Bind(variables.Get<Button>("mine")).For(v => v.onClick).To(vm => vm.OnMineButton);
        bindingSet.Bind(variables.Get<Button>("poke")).For(v => v.onClick).To(vm => vm.OnPokeButton);
        bindingSet.Bind(variables.Get<Button>("goods")).For(v => v.onClick).To(vm => vm.OnGoodsButton);

        bindingSet.Bind(variables.Get<GameObject>("sellItemList")).For(v => v.activeSelf).To(vm => vm.SellItemListVisible);

        bindingSet.Bind(variables.Get<Button>("doSell")).For(v => v.onClick).To(vm => vm.OnDoSellClick);
        bindingSet.Bind(variables.Get<Button>("cancelSell")).For(v => v.onClick).To(vm => vm.OnCancelSellClick);
        bindingSet.Bind(variables.Get<Button>("next")).For(v => v.onClick).To(vm => vm.OnNext);
        bindingSet.Bind(variables.Get<Button>("pre")).For(v => v.onClick).To(vm => vm.OnPre);

        for (int i = 0; i < visableSlots; i++)
        {
            bindingSet.Bind(variables.Get<Button>("item" + i)).For(v => v.onClick)
                            .To(vm => vm.OnItemShot).CommandParameter(i); ;
        }
        bindingSet.Build();
        this.gameObject.SetActive(false);
    }

    //点击某个Iten的变化
    protected void OnItemClicked(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Replace:
                foreach (TradeViewModel.ItemPositionClick item in eventArgs.NewItems)
                {
                    int i = item.ItemPosition;
                    itemSlot[i].gameObject.SetActive(true);
                    itemSlot[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradeinchoice");
                    itemName[i].color = Color.white;
                    itemQuantity[i].color = Color.white;
                    itemX[i].color = Color.white;
                    itemLv[i].color = Color.white;
                }
                break;
        }
    }

    //购买和卖出的切换
    protected void OnBuyOrSellChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (TradeViewModel.BuyOrSell item in eventArgs.NewItems)
                {
                    search.texture = Resources.Load<Texture>("Sprites/GUI/Trade/searchback");
                    tradeChoiceGood.texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradeunchoicepoke");
                    tradeChoiceGoodText.color = Color.black;
                    tradeChoicePoke.texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradeunchoicepoke");
                    tradeChoicePokeText.color = Color.black;
                    tradeChoiceMine.texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradeunchoicepoke");
                    tradeChoiceMineText.color = Color.black;
                    if (item.CurrentBuyOrSell == TradeViewModel.BuyOrSell.BuyOrSellType.Buy)
                    {
                        tradeChoicePoke.texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradechoicepoke");
                        tradeChoicePokeText.color = Color.white;
                        tradeBuyBackText.text = "购买";
                    }
                    else if (item.CurrentBuyOrSell == TradeViewModel.BuyOrSell.BuyOrSellType.Sell)
                    {
                        tradeChoiceGood.texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradechoicepoke");
                        tradeChoiceGoodText.color = Color.white;
                        tradeBuyBackText.text = "上架";
                    }
                    else if (item.CurrentBuyOrSell == TradeViewModel.BuyOrSell.BuyOrSellType.Mine)
                    {
                        tradeChoiceMine.texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradechoicepoke");
                        tradeChoiceMineText.color = Color.white;
                        tradeChoiceMineText.text = "我的";

                    }
                }
                break;
        }
    }

    //道具界面
    protected void OnScreenChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {

    }
    protected void OnItemListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                tradeSale.texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradethings2");
                tradeThings.texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradesale2");
                foreach (TradeViewModel.ItemList item in eventArgs.NewItems)
                {
                    if (item.PageNumber > 0)
                    {
                        pageInfo.text = "Page:" + item.PageNumber;
                    }
                    for (int i = 0; i < visableSlots; i++)
                    {
                        itemSlot[i].gameObject.SetActive(false);
                    }
                    List<TransactionDetailInfo> tmp = item.GoodDetailInfos;
                    if (tmp == null || tmp.Count == 0)
                    {
                        return;
                    }
                    for (int i = 0; i < visableSlots; i++)
                    {
                        if (i >= 0 && i < tmp.Count)
                        {
                            TransactionDetailInfo info = tmp[i];
                            ItemData itemData = ItemDatabase.getItem(info.goodsId);
                            if (itemData == null)
                            {
                                continue;
                            }
                            itemLv[i].gameObject.SetActive(false);
                            itemSlot[i].gameObject.SetActive(true);
                            itemSlot[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradeunchoice");
                            itemName[i].text = itemData.getName();
                            itemName[i].color = Color.black;
                            //物品图片修改
                            pokeIcon[i].gameObject.SetActive(false);
                            itemIcon[i].gameObject.SetActive(true);
                            itemIcon[i].texture = Resources.Load<Texture>("Items/" + CommonUtils.convertLongID(itemData.getBasicGoodsId()));
                            itemX[i].gameObject.SetActive(true);
                            itemX[i].color = Color.black;
                            itemQuantity[i].gameObject.SetActive(true);
                            itemQuantity[i].color = Color.black;
                            if (info.diamondsPrice > 0)
                            {
                                itemPricePic[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradediamond");
                                itemQuantity[i].text = "" + info.diamondsPrice;
                            }
                            else if (info.goldCoinsPrice > 0)
                            {
                                itemPricePic[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/trademoney");
                                itemQuantity[i].text = "" + info.goldCoinsPrice;
                            }
                            else
                            {
                                itemPricePic[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/trademoney");
                                itemQuantity[i].text = "" + info.number;
                            }
                            if (info.tradeState == 1)
                            {
                                itemPricePic[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/trademoney");
                                itemQuantity[i].text = "卖出中";
                            }
                            else if (info.tradeState == 2)
                            {
                                itemPricePic[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/trademoney");
                                itemQuantity[i].text = "已完成";
                            }
                        }
                    }
                }
                break;
        }
    }

    //精灵界面
    protected void OnPokemonListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                tradeSale.texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradesale");
                tradeThings.texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradethings");
                foreach (TradeViewModel.PokemonList item in eventArgs.NewItems)
                {
                    if (item.PageNumber > 0)
                    {
                        pageInfo.text = "Page:" + item.PageNumber;
                    }
                    for (int i = 0; i < visableSlots; i++)
                    {
                        itemSlot[i].gameObject.SetActive(false);
                    }
                    List<TransactionDetailInfo> tmp = item.PokemonDetailInfos;
                    if (tmp == null || tmp.Count == 0)
                    {
                        return;
                    }
                    for (int i = 0; i < visableSlots; i++)
                    {
                        if (i >= 0 && i < tmp.Count)
                        {
                            TransactionDetailInfo info = tmp[i];
                            if (info == null)
                            {
                                continue;
                            }
                            PokemonData pokemonData = PokemonDatabase.getPokemon(info.pokemonId);
                            itemSlot[i].gameObject.SetActive(true);
                            itemSlot[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradeunchoice");
                            itemName[i].text = pokemonData.getName();
                            itemName[i].color = Color.black;
                            itemLv[i].gameObject.SetActive(true);
                            itemLv[i].text = "Lv:" + info.level;
                            itemLv[i].color = Color.black;
                            //精灵图片修改
                            itemIcon[i].gameObject.SetActive(false);
                            pokeIcon[i].gameObject.SetActive(true);
                            pokeIcon[i].texture = Resources.Load<Texture>("PokemonIcons/icon" + CommonUtils.convertLongID(info.pokemonId));
                            itemX[i].gameObject.SetActive(true);
                            itemX[i].color = Color.black;
                            itemQuantity[i].gameObject.SetActive(true);
                            itemQuantity[i].color = Color.black;
                            if (info.diamondsPrice > 0)
                            {
                                itemPricePic[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/tradediamond");
                                itemQuantity[i].text = "" + info.diamondsPrice;
                            }
                            else if (info.goldCoinsPrice > 0)
                            {
                                itemPricePic[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/trademoney");
                                itemQuantity[i].text = "" + info.goldCoinsPrice;
                            }
                            else
                            {
                                itemPricePic[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/trademoney");
                                itemQuantity[i].text = "待定";
                            }
                            if (info.tradeState == 1)
                            {
                                itemPricePic[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/trademoney");
                                itemQuantity[i].text = "卖出中";
                            }
                            else if (info.tradeState == 2)
                            {
                                itemPricePic[i].texture = Resources.Load<Texture>("Sprites/GUI/Trade/trademoney");
                                itemQuantity[i].text = "已完成";
                            }
                        }
                    }
                }
                break;
        }
    }

    private void sellDiamondChangedValue(string value)
    {
        sellDiamondInputField.text = value;
    }

    private void sellMoneyChangedValue(string value)
    {
        sellMoneyInputField.text = value;
    }

    private void setSellItemListVisible(bool visible)
    {
        if (visible)
        {
            itemList.gameObject.SetActive(false);
            sellItemList.gameObject.SetActive(true);
        }
        else
        {
            itemList.gameObject.SetActive(true);
            sellItemList.gameObject.SetActive(false);
        }
    }

    public IEnumerator control()
    {
        Scene.main.SetMapButtonVisible(false);
        yield return StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
        tradeViewModel.reset();
        while (tradeViewModel.running)
        {
            yield return null;
        }
        yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
        GlobalVariables.global.resetFollower();
        this.gameObject.SetActive(false);
    }
}

