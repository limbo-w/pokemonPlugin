using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Execution;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class TradeViewModel : ViewModelBase
{
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
    public bool running = false;
    //物品上架
    private const string ON_THE_SHELF_URL = HttpsUtils.DNS_URL + "/pokemon/transaction/onTheShelf";
    //物品下架
    private const string OFF_THE_SHELF_URL = HttpsUtils.DNS_URL + "/pokemon/transaction/offTheShelf";
    //购买商品
    private const string TRANSACTION_BUY = HttpsUtils.DNS_URL + "/pokemon/transaction/buy";
    //搜索物品
    private const string TRANSACTION_GOOD_LIST = HttpsUtils.DNS_URL + "/pokemon/transaction/transactionGoods/list";
    //搜索精灵
    private const string TRANSACTION_POKEMON_LIST = HttpsUtils.DNS_URL + "/pokemon/transaction/transactionPokemon/list";
    //获取当前用户的交易是否完成信息
    private const string SELECT_CUR_TRANSACTION = HttpsUtils.DNS_URL + "/pokemon/transaction/selectCurTransaction";

    private Screen.ScreenType currentScreen;
    private TradeHandler tradeHandler;
    private BuyOrSell.BuyOrSellType currentBuyOrSell;

    public int[] currentPosition = new int[]
    {
      0, 0, 0,
     };

    //正在交易的精灵或者物品
    private List<TransactionDetailInfo> transactionPokemonDetailInfos = new List<TransactionDetailInfo>();
    private int pokemonPageNum = 1;
    private List<TransactionDetailInfo> transactionGoodDetailInfos = new List<TransactionDetailInfo>();
    private int goodPageNum = 1;
    //我的精灵或者物品
    private List<TransactionDetailInfo> myPokemonDetailInfos = new List<TransactionDetailInfo>();
    private int myPokemonPageNum = 1;
    private List<TransactionDetailInfo> myGoodDetailInfos = new List<TransactionDetailInfo>();
    private int myGoodPageNum = 1;
    //完成交易或者交易中的信息
    private List<TransactionDetailInfo> shelfPokemonDetailInfos = new List<TransactionDetailInfo>();
    private int shelfPokemonPageNum = 1;
    private List<TransactionDetailInfo> shelfGoodDetailInfos = new List<TransactionDetailInfo>();
    private int shelfGoodPageNum = 1;

    private ObservableList<BuyOrSell> buyOrSell;
    private ObservableList<Screen> screen;
    private ObservableList<ItemList> itemList;
    private ObservableList<PokemonList> pokemonList;
    private ObservableList<ItemPositionClick> itemPositionClicks;

    public TradeViewModel(TradeHandler tradeHandler, NotifyCollectionChangedObject changedObject)
    {
        this.tradeHandler = tradeHandler;
        buyOrSell = new ObservableList<BuyOrSell>();
        buyOrSell.CollectionChanged += changedObject.buyOrSellHandler;
        buyOrSell.Add(new BuyOrSell());

        screen = new ObservableList<Screen>();
        screen.CollectionChanged += changedObject.screenHandler;
        screen.Add(new Screen());

        itemList = new ObservableList<ItemList>();
        itemList.CollectionChanged += changedObject.itemListHandler;
        itemList.Add(new ItemList());

        pokemonList = new ObservableList<PokemonList>();
        pokemonList.CollectionChanged += changedObject.pokemonListHandler;
        pokemonList.Add(new PokemonList());

        itemPositionClicks = new ObservableList<ItemPositionClick>();
        itemPositionClicks.CollectionChanged += changedObject.itemClickHandler;
        itemPositionClicks.Add(new ItemPositionClick());

        this.onItemShot = new SimpleCommand<int>(OnItemShotClick);
    }

    //卖出按钮点击
    public void TransactionSell()
    {
        long holdId = 0;
        int transactionType = 1;
        TransactionDetailInfo transactionDetailInfo;
        if (currentScreen == Screen.ScreenType.Poke)
        {
            transactionDetailInfo = myPokemonDetailInfos[currentPosition[(int)currentScreen]];
            holdId = transactionDetailInfo.holdPokemonId;
            transactionType = 1;
        }
        else
        {
            transactionDetailInfo = myGoodDetailInfos[currentPosition[(int)currentScreen]];
            holdId = transactionDetailInfo.holdGoodsId;
            transactionType = 2;
        }
        int payType = 1;
        int sellMoney = 0;
        int sellDiamond = 0;
        if (!string.IsNullOrEmpty(tradeHandler.sellMoneyInputField.text) && int.Parse(tradeHandler.sellMoneyInputField.text) > 0)
        {
            sellMoney = int.Parse(tradeHandler.sellMoneyInputField.text);
            payType = 1;
        }
        else if (!string.IsNullOrEmpty(tradeHandler.sellDiamondInputField.text) && int.Parse(tradeHandler.sellDiamondInputField.text) > 0)
        {
            sellDiamond = int.Parse(tradeHandler.sellDiamondInputField.text);
            payType = 2;
        }
        Dictionary<string, string> headers = new Dictionary<string, string>();
        Dictionary<string, string> paramz = new Dictionary<string, string>();

        HttpsUtils.SetGeneralHeaders(headers);
        HttpsUtils.SetGeneralParams(paramz);
        try
        {
            OnTheShelfBodyRequest body = new OnTheShelfBodyRequest
            {
                payType = payType,
                transactionType = transactionType,
                holdId = holdId,
                goldCoinsPrice = sellMoney,
                diamondsPrice = sellDiamond,
                number = 1
            };
            RequestHelper request = new RequestHelper
            {
                Uri = ON_THE_SHELF_URL,
                Headers = headers,
                Params = paramz,
                Body = body,
                Method = "POST",
            };
            RestClient.Request(request).Then(result =>
            {
                GetPokemonTransactionListResult response = JsonUtility.FromJson<GetPokemonTransactionListResult>(result.Text);
                if (response == null || response.code != 200)
                {
                    updateParty();
                    StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("上架失败:OH MY GOD"));
                    return;
                }
                if (currentScreen == Screen.ScreenType.Poke)
                {
                    Pokemon[] pokemons = PCManager.Instance.getPCBox(myPokemonPageNum);
                    foreach (Pokemon pokemon in pokemons)
                    {
                        if (pokemon != null && pokemon.getHoldId() == holdId)
                        {
                            if (PCManager.Instance.removePokemon(pokemon))
                            {
                                StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("上架精灵成功"));
                            }
                            else
                            {
                                StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("上架精灵失败"));
                            }
                        }
                    }
                }
                else
                {
                    BagItem bagItem = new BagItem
                    {
                        holdId = transactionDetailInfo.holdGoodsId,
                        basicGoodsId = transactionDetailInfo.goodsId,
                        amount = 1,
                        tradable = 1
                    };
                    if (BagManager.Instance.removeItem(bagItem))
                    {
                        StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("上架物品成功"));
                    }
                    else
                    {
                        StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("上架物品失败"));
                    }
                }
                updateParty();
            });
        }
        catch (Exception err)
        {
            updateParty();
            StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("上架失败:" + err));
        }
    }

    //购买按钮点击
    public void TransactionBuy()
    {
        long transactionId = 0;
        TransactionDetailInfo transactionDetailInfo;
        int transactionType = 1;
        if (currentScreen == Screen.ScreenType.Poke)
        {
            transactionDetailInfo = transactionPokemonDetailInfos[currentPosition[(int)currentScreen]];
            transactionId = transactionDetailInfo.id;
            transactionType = 1;
        }
        else
        {
            transactionDetailInfo = transactionGoodDetailInfos[currentPosition[(int)currentScreen]];
            transactionId = transactionDetailInfo.id;
            transactionType = 2;
        }
        Dictionary<string, string> headers = new Dictionary<string, string>();
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        HttpsUtils.SetGeneralParams(paramz);
        TransactionBuyBodyRequest body = new TransactionBuyBodyRequest
        {
            transactionType = transactionType,
            transactionId = transactionId,
            number = 1
        };
        RequestHelper request = new RequestHelper
        {
            Uri = TRANSACTION_BUY,
            Headers = headers,
            Params = paramz,
            Body = body,
            Method = "POST",
        };
        RestClient.Request(request).Then(result =>
        {
            GetPokemonTransactionListResult response = JsonUtility.FromJson<GetPokemonTransactionListResult>(result.Text);
            if (response == null || response.code != 200)
            {
                updateParty();
                StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("购买失败:OH MY GOD"));
                return;
            }
            if (currentScreen == Screen.ScreenType.Poke)
            {
                //对应的宝可梦直接添加
                AppDetailedPokemon appDetailedPokemon = transactionDetailInfo.appDetailedPokemon;
                BasicPokemon basicPokemon = transactionDetailInfo.basicPokemon;
                SkillInfo[] skillIds = transactionDetailInfo.skillIdsObj;
                Pokemon pokemon = new Pokemon(transactionDetailInfo.holdPokemonId, transactionDetailInfo.pokemonId, basicPokemon.pokemonName, Pokemon.Gender.CALCULATE, transactionDetailInfo.level, 0, new BagItem(), "",
                    appDetailedPokemon.individualHp, appDetailedPokemon.individualAtk, appDetailedPokemon.individualDef, appDetailedPokemon.individualSpA, appDetailedPokemon.individualSpD, appDetailedPokemon.individualSpe,
                    appDetailedPokemon.effortHp, appDetailedPokemon.effortAtk, appDetailedPokemon.effortDef, appDetailedPokemon.effortSpA, appDetailedPokemon.effortSpD, appDetailedPokemon.effortSpe,
                    "", skillIds != null ? skillIds.ToList() : new List<SkillInfo>());
                if (PCManager.Instance.addPokemon(pokemon))
                {
                    transactionPokemonDetailInfos.Remove(transactionDetailInfo);
                    StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("恭喜购买成功对应宝可梦"));
                }
                else
                {
                    StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("购买对应宝可梦失败"));
                }
            }
            else
            {
                BagItem bagItem = new BagItem
                {
                    holdId = transactionDetailInfo.holdGoodsId,
                    basicGoodsId = transactionDetailInfo.goodsId,
                    amount = 1,
                    tradable = 1
                };
                if (BagManager.Instance.addItem(bagItem))
                {
                    transactionGoodDetailInfos.Remove(transactionDetailInfo);
                    StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("恭喜购买成功对应物品"));
                }
                else
                {
                    StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("购买对应物品失败"));
                }
            }
            updateParty();
        }).Catch(err =>
        {
            updateParty();
            StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("购买失败:" + err));
        });
    }

    //reset初始化
    public void reset()
    {
        myGoodPageNum = 1;
        myPokemonPageNum = 1;
        pokemonPageNum = 1;
        goodPageNum = 1;
        shelfGoodPageNum = 1;
        shelfPokemonPageNum = 1;
        SellItemListVisible = false;
        dialog = DialogBoxUIHandler.main;
        running = true;
        currentScreen = Screen.ScreenType.Poke;
        currentBuyOrSell = BuyOrSell.BuyOrSellType.Buy;
        currentPosition = new int[]
        {
            -1, 0, 0,
        };
        updateScreen(currentScreen, currentBuyOrSell);
    }

    //获取当前交易结果
    private void getCurTransactionResult()
    {
        shelfPokemonDetailInfos.Clear();
        shelfGoodDetailInfos.Clear();
        updateParty();
        Dictionary<string, string> headers = new Dictionary<string, string>();
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = SELECT_CUR_TRANSACTION,
            Headers = headers,
            Params = paramz,
            Method = "POST",
        };
        RestClient.Request(request).Then(result =>
        {
            GetCurTransactionInfoResult response = JsonUtility.FromJson<GetCurTransactionInfoResult>(result.Text);
            if (response == null || response.data == null)
            {
                StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("获取我的交易情况失败"));
                return;
            }
            if (response.data.toBeTraded != null)
            {
                List<TransactionDetailInfo> toBeTraded = response.data.toBeTraded;
                foreach (TransactionDetailInfo info in toBeTraded)
                {
                    info.tradeState = 1;
                    if (info.pokemonId > 0)
                    {
                        shelfPokemonDetailInfos.Add(info);
                    }
                    else if (info.goodsId > 0)
                    {
                        shelfGoodDetailInfos.Add(info);
                    }
                }
            }
            if (response.data.completed != null)
            {
                List<TransactionDetailInfo> completed = response.data.completed;
                foreach (TransactionDetailInfo info in completed)
                {
                    info.tradeState = 2;
                    if (info.pokemonId > 0)
                    {
                        shelfPokemonDetailInfos.Add(info);
                    }
                    else if (info.goodsId > 0)
                    {
                        shelfGoodDetailInfos.Add(info);
                    }
                }
            }
            updateParty();
        }).Catch(err =>
        {
            updateParty();
            StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("获取我的交易情况失败:" + err));
        });
    }

    //获取交易物品的数据
    private void getTransactionGoodListRequest()
    {
        transactionGoodDetailInfos.Clear();
        updateParty();
        Dictionary<string, string> headers = new Dictionary<string, string>();
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        HttpsUtils.SetGeneralParams(paramz);
        paramz.Add("pageNum", "" + goodPageNum);
        paramz.Add("pageSize", "" + TradeHandler.visableSlots);
        RequestHelper request = new RequestHelper
        {
            Uri = TRANSACTION_GOOD_LIST,
            Headers = headers,
            Params = paramz,
            Method = "POST",
            Body = new CommonTradeBodyRequest()
        };
        RestClient.Request(request).Then(result =>
        {
            GetGoodTransactionListResult response = JsonUtility.FromJson<GetGoodTransactionListResult>(result.Text);
            if (response == null || response.code != 200)
            {
                updateParty();
                return;
            }
            goodPageNum = response.pageNum;
            List<TransactionDetailInfo> rows = response.rows;
            if (rows != null)
            {
                foreach (TransactionDetailInfo detailInfo in rows)
                {
                    if (detailInfo.id > 0)
                    {
                        transactionGoodDetailInfos.Add(detailInfo);
                    }
                }
            }
            updateParty();
        }).Catch(err =>
        {
            updateParty();
            StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("获取交易列表失败:" + err));
        });
    }

    //获取交易精灵的数据
    private void getTransactionPokemonListRequest()
    {
        transactionPokemonDetailInfos.Clear();
        updateParty();
        Dictionary<string, string> headers = new Dictionary<string, string>();
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        HttpsUtils.SetGeneralParams(paramz);
        paramz.Add("pageNum", "" + pokemonPageNum);
        paramz.Add("pageSize", "" + TradeHandler.visableSlots);
        RequestHelper request = new RequestHelper
        {
            Uri = TRANSACTION_POKEMON_LIST,
            Headers = headers,
            Params = paramz,
            Method = "POST",
            Body = new CommonTradeBodyRequest()
        };
        RestClient.Request(request).Then(result =>
        {
            GetPokemonTransactionListResult response = JsonUtility.FromJson<GetPokemonTransactionListResult>(result.Text);
            if (response == null || response.code != 200)
            {
                updateParty();
                return;
            }
            pokemonPageNum = response.pageNum;
            List<TransactionDetailInfo> rows = response.rows;
            if (rows != null)
            {
                foreach (TransactionDetailInfo detailInfo in rows)
                {
                    if (detailInfo.id > 0)
                    {
                        transactionPokemonDetailInfos.Add(detailInfo);
                    }
                }
            }
            updateParty();
        }).Catch(err =>
        {
            updateParty();
            StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("获取交易列表失败:" + err));
        });
    }

    //精灵和物品屏幕切换
    private void updateScreen(Screen.ScreenType screenType, BuyOrSell.BuyOrSellType buyOrSellType)
    {
        bool buyOrSellTypeChange = false;
        if (currentBuyOrSell != buyOrSellType)
        {
            buyOrSellTypeChange = true;
            currentBuyOrSell = buyOrSellType;
        }
        if (currentScreen != screenType)
        {
            currentScreen = screenType;
        }
        if (buyOrSellType == BuyOrSell.BuyOrSellType.Buy)
        {
            if (screenType == Screen.ScreenType.Goods && (transactionGoodDetailInfos.Count == 0 || buyOrSellTypeChange))
            {
                getTransactionGoodListRequest();
            }
            else if (screenType == Screen.ScreenType.Poke && (transactionPokemonDetailInfos.Count == 0 || buyOrSellTypeChange))
            {
                getTransactionPokemonListRequest();
            }
            else
            {
                updateParty();
            }
        }
        else if (buyOrSellType == BuyOrSell.BuyOrSellType.Mine && buyOrSellTypeChange)
        {
            getCurTransactionResult();
        }
        else
        {
            updateParty();
        }
    }

    //更新展示界面
    private void updateParty()
    {
        if (currentScreen == Screen.ScreenType.Goods)
        {
            updateItemList();
        }
        else if (currentScreen == Screen.ScreenType.Poke
            || currentScreen == Screen.ScreenType.Search)
        {
            updatePokemonList();
        }
        buyOrSell[0] = new BuyOrSell()
        {
            CurrentBuyOrSell = currentBuyOrSell
        };
    }

    //更新物品
    private void updateItemList()
    {
        itemList[0] = new ItemList()
        {
            GoodDetailInfos = new List<TransactionDetailInfo>()
        };
        if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Buy)
        {
            itemList[0] = new ItemList()
            {
                PageNumber = goodPageNum,
                GoodDetailInfos = transactionGoodDetailInfos
            };
        }
        else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Sell)
        {
            myGoodDetailInfos.Clear();
            BagManager.Instance.fetchCurrentGoodList(delegate (bool result)
            {
                if (result)
                {
                    BagItem[] items = BagManager.Instance.getItemTypeArray(ItemData.ItemType.ITEM);
                    if ((myGoodPageNum - 1) * TradeHandler.visableSlots >= items.Length)
                    {
                        return;
                    }
                    for (int i = (myGoodPageNum - 1) * TradeHandler.visableSlots; i < items.Length; i++)
                    {
                        myGoodDetailInfos.Add(new TransactionDetailInfo
                        {
                            holdGoodsId = items[i].holdId,
                            goodsId = items[i].basicGoodsId,
                            number = items[i].amount
                        });
                    }
                }
                itemList[0] = new ItemList()
                {
                    PageNumber = myGoodPageNum,
                    GoodDetailInfos = myGoodDetailInfos
                };
            });
        }
        else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Mine)
        {
            List<TransactionDetailInfo> tmp = new List<TransactionDetailInfo>();
            for (int i = (shelfGoodPageNum - 1) * TradeHandler.visableSlots; i < shelfGoodDetailInfos.Count; i++)
            {
                tmp.Add(shelfGoodDetailInfos[i]);
            }
            itemList[0] = new ItemList()
            {
                PageNumber = shelfGoodPageNum,
                GoodDetailInfos = tmp
            };
        }
    }

    //更新精灵
    private void updatePokemonList()
    {
        pokemonList[0] = new PokemonList()
        {
            PokemonDetailInfos = new List<TransactionDetailInfo>()
        };
        if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Buy)
        {
            pokemonList[0] = new PokemonList()
            {
                PageNumber = pokemonPageNum,
                PokemonDetailInfos = transactionPokemonDetailInfos
            };
        }
        else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Sell)
        {
            //想要卖出
            myPokemonDetailInfos.Clear();
            PCManager.Instance.fetchBoxPokemonList(delegate (bool result)
            {
                if (result)
                {
                    if (myPokemonPageNum >= PCManager.Instance.getBoxLength()
                    || PCManager.Instance.getSubBoxPokemonLength(myPokemonPageNum) <= 0)
                    {
                        return;
                    }
                    Pokemon[] pokemons = PCManager.Instance.getPCBox(myPokemonPageNum);
                    foreach (Pokemon pokemon in pokemons)
                    {
                        if (pokemon != null)
                        {
                            myPokemonDetailInfos.Add(new TransactionDetailInfo
                            {
                                holdPokemonId = pokemon.getHoldId(),
                                pokemonId = pokemon.getID(),
                                level = pokemon.getLevel(),
                            });
                        }
                    }
                    pokemonList[0] = new PokemonList()
                    {
                        PageNumber = myPokemonPageNum,
                        PokemonDetailInfos = myPokemonDetailInfos
                    };
                }
            });
        }
        else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Mine)
        {
            List<TransactionDetailInfo> tmp = new List<TransactionDetailInfo>();
            for (int i = (shelfPokemonPageNum - 1) * TradeHandler.visableSlots; i < shelfPokemonDetailInfos.Count; i++)
            {
                tmp.Add(shelfPokemonDetailInfos[i]);
            }
            pokemonList[0] = new PokemonList()
            {
                PageNumber = shelfPokemonPageNum,
                PokemonDetailInfos = tmp
            };
        }
    }

    //点击物品
    private IEnumerator DoItemClick(int i)
    {
        SfxHandler.Play(tradeHandler.selectClip);
        currentPosition[(int)currentScreen] = i;
        itemPositionClicks[0] = new ItemPositionClick()
        if (currentScreen == Screen.ScreenType.Search)
        screen[0] = new Screen()
        {
            CurrentScreen = currentScreen
        };
        if (currentScreen == Screen.ScreenType.Search)
        {
            ItemPosition = i
        };
        dialog.drawTextInstant("你想干什么?");
        string[] choices = new string[] { "概述", "购买", "取消" };
        if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Sell)
        {
            choices = new string[] { "卖出", "取消" };
        }
        else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Mine)
        {
            choices = new string[] { "概述", "取消" };
        }
        yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
        yield return new WaitForSeconds(0.3f);
        int chosenIndex = dialog.buttonIndex;
        string chosenChoice = choices[choices.Length - chosenIndex - 1];
        if (chosenChoice == "概述")
        {
            SfxHandler.Play(tradeHandler.selectClip);
            if (currentScreen == Screen.ScreenType.Poke)
            {
                TransactionDetailInfo curTransactionDetailInfo = null;
                if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Buy)
                {
                    curTransactionDetailInfo = transactionPokemonDetailInfos[i];
                }
                else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Mine)
                {
                    curTransactionDetailInfo = shelfPokemonDetailInfos[i];
                }
                if (curTransactionDetailInfo != null)
                {
                    //概述对应的宝可梦
                    AppDetailedPokemon appDetailedPokemon = curTransactionDetailInfo.appDetailedPokemon;
                    BasicPokemon basicPokemon = curTransactionDetailInfo.basicPokemon;
                    SkillInfo[] skillIds = curTransactionDetailInfo.skillIdsObj;
                    Pokemon pokemon = new Pokemon(curTransactionDetailInfo.holdPokemonId, curTransactionDetailInfo.pokemonId, basicPokemon.pokemonName, Pokemon.Gender.CALCULATE, curTransactionDetailInfo.level, 0, new BagItem(), "",
                        appDetailedPokemon.individualHp, appDetailedPokemon.individualAtk, appDetailedPokemon.individualDef, appDetailedPokemon.individualSpA, appDetailedPokemon.individualSpD, appDetailedPokemon.individualSpe,
                        appDetailedPokemon.effortHp, appDetailedPokemon.effortAtk, appDetailedPokemon.effortDef, appDetailedPokemon.effortSpA, appDetailedPokemon.effortSpD, appDetailedPokemon.effortSpe,
                        "", skillIds != null ? skillIds.ToList() : new List<SkillInfo>());
                    Scene.main.Summary.gameObject.SetActive(true);
                    StaticCoroutine.StartCoroutine(Scene.main.Summary.control(new Pokemon[] { pokemon }, 0));
                    while (Scene.main.Summary.gameObject.activeSelf)
                    {
                        yield return null;
                    }
                    yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
                }
            }
            else
            {
                yield return CommonUtils.showDialogText("暂时只有交易的精灵能显示概述");
            }
        }
        else if (chosenChoice == "购买")
        {
            SfxHandler.Play(tradeHandler.selectClip);
            dialog.drawTextInstant("你确定要购买吗?");
            choices = new string[] { "是", "否" };
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
            chosenIndex = dialog.buttonIndex;
            if (chosenIndex == 1)
            {
                TransactionBuy();
            }
        }
        else if (chosenChoice == "卖出")
        {
            SfxHandler.Play(tradeHandler.selectClip);
            SellItemListVisible = true;
        }
        else
        {
            SfxHandler.Play(tradeHandler.selectClip);
        }
        updateParty();
    }

    //====================================================================================
    //返回按钮点击
    public void OnReturnButton()
    {
        running = false;
    }

    //精灵按钮点击
    public void OnPokeButton()
    {
        SfxHandler.Play(tradeHandler.selectClip);
        if (running)
        {
            updateScreen(Screen.ScreenType.Poke, currentBuyOrSell);
        }
    }

    //物品按钮点击
    public void OnGoodsButton()
    {
        SfxHandler.Play(tradeHandler.selectClip);
        if (running)
        {
            updateScreen(Screen.ScreenType.Goods, currentBuyOrSell);
        }
    }

    //购买按钮点击
    public void OnBuyButton()
    {
        SfxHandler.Play(tradeHandler.selectClip);
        if (running)
        {
            updateScreen(currentScreen, BuyOrSell.BuyOrSellType.Buy);
        }
    }

    //卖出按钮点击
    public void OnSellButton()
    {
        SfxHandler.Play(tradeHandler.selectClip);
        if (running)
        {
            updateScreen(currentScreen, BuyOrSell.BuyOrSellType.Sell);
        }
    }

    //我的按钮点击
    public void OnMineButton()
    {
        SfxHandler.Play(tradeHandler.selectClip);
        if (running)
        {
            updateScreen(currentScreen, BuyOrSell.BuyOrSellType.Mine);
        }
    }


    public class NotifyCollectionChangedObject
    {
        public NotifyCollectionChangedEventHandler buyOrSellHandler;
        public NotifyCollectionChangedEventHandler itemClickHandler;
        public NotifyCollectionChangedEventHandler screenHandler;
        public NotifyCollectionChangedEventHandler itemListHandler;
        public NotifyCollectionChangedEventHandler pokemonListHandler;
    }

    private bool sellItemListVisible;
    public bool SellItemListVisible
    {
        get { return this.sellItemListVisible; }
        set { this.Set<bool>(ref this.sellItemListVisible, value, "SellItemListVisible"); }
    }

    public class ItemPositionClick : ObservableObject
    {
        private int itemPosition;
        public int ItemPosition
        {
            get { return this.itemPosition; }
            set { this.Set<int>(ref this.itemPosition, value, "ItemPosition"); }
        }
    }

    public class BuyOrSell : ObservableObject
    {
        public enum BuyOrSellType
        {
            Buy,
            Sell,
            Mine
        }

        private BuyOrSellType currentBuyOrSell;

        public BuyOrSellType CurrentBuyOrSell
        {
            get { return this.currentBuyOrSell; }
            set { this.Set<BuyOrSellType>(ref this.currentBuyOrSell, value, "CurrentBuyOrSell"); }
        }

    }

    public class Screen : ObservableObject
    {
        public enum ScreenType
        {
            Search,
            Poke,
            Goods
        }
        private ScreenType currentScreen;
        public ScreenType CurrentScreen
        {
            get { return this.currentScreen; }
            set { this.Set<ScreenType>(ref this.currentScreen, value, "CurrentScreen"); }
        }
    }

    public class ItemList : ObservableObject
    {
        private List<TransactionDetailInfo> goodDetailInfos;

        public List<TransactionDetailInfo> GoodDetailInfos
        {
            get { return this.goodDetailInfos; }
            set { this.Set<List<TransactionDetailInfo>>(ref this.goodDetailInfos, value, "GoodDetailInfos"); }
        }

        private int pageNumber;
        public int PageNumber
        {
            get { return this.pageNumber; }
            set { this.Set<int>(ref this.pageNumber, value, "PageNumber"); }
        }
    }

    public class PokemonList : ObservableObject
    {
        private List<TransactionDetailInfo> pokemonDetailInfos;

        public List<TransactionDetailInfo> PokemonDetailInfos
        {
            get { return this.pokemonDetailInfos; }
            set { this.Set<List<TransactionDetailInfo>>(ref this.pokemonDetailInfos, value, "PokemonDetailInfos"); }
        }

        private int pageNumber;
        public int PageNumber
        {
            get { return this.pageNumber; }
            set { this.Set<int>(ref this.pageNumber, value, "PageNumber"); }
        }
    }

    private readonly SimpleCommand<int> onItemShot;
    public ICommand OnItemShot
    {
        get { return this.onItemShot; }
    }

    public void OnItemShotClick(int index)
    {
        Executors.RunOnCoroutineNoReturn(DoItemClick(index));
    }

    public void OnDoSellClick()
    {
        SfxHandler.Play(tradeHandler.selectClip);
        SellItemListVisible = false;
        TransactionSell();
    }

    public void OnCancelSellClick()
    {
        SfxHandler.Play(tradeHandler.selectClip);
        SellItemListVisible = false;
        updateParty();
    }

    public void OnNext()
    {
        SfxHandler.Play(tradeHandler.selectClip);
        if (currentScreen == Screen.ScreenType.Goods)
        {
            if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Buy)
            {
                goodPageNum++;
                getTransactionGoodListRequest();
            }
            else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Sell)
            {
                myGoodPageNum++;
                updateParty();
            }
            else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Mine)
            {
                shelfGoodPageNum++;
                updateParty();
            }
        }
        else if (currentScreen == Screen.ScreenType.Poke
            || currentScreen == Screen.ScreenType.Search)
        {
            if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Buy)
            {
                pokemonPageNum++;
                getTransactionPokemonListRequest();
            }
            else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Sell)
            {
                myPokemonPageNum++;
                updateParty();
            }
            else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Mine)
            {
                shelfPokemonPageNum++;
                updateParty();
            }
        }
    }

    public void OnPre()
    {
        SfxHandler.Play(tradeHandler.selectClip);
        if (currentScreen == Screen.ScreenType.Goods)
        {
            if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Buy)
            {
                if (goodPageNum > 1)
                {
                    goodPageNum--;
                    updateParty();
                }
            }
            else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Sell)
            {
                if (myGoodPageNum > 1)
                {
                    myGoodPageNum--;
                    updateParty();
                }
            }
            else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Mine)
            {
                if (shelfGoodPageNum > 1)
                {
                    shelfGoodPageNum--;
                    updateParty();
                }
            }
        }
        else if (currentScreen == Screen.ScreenType.Poke
            || currentScreen == Screen.ScreenType.Search)
        {
            if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Buy)
            {
                if (pokemonPageNum > 1)
                {
                    pokemonPageNum--;
                    getTransactionPokemonListRequest();
                }
            }
            else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Sell)
            {
                if (myPokemonPageNum > 1)
                {
                    myPokemonPageNum--;
                    updateParty();
                }
            }
            else if (currentBuyOrSell == BuyOrSell.BuyOrSellType.Mine)
            {
                if (shelfPokemonPageNum > 1)
                {
                    shelfPokemonPageNum--;
                    updateParty();
                }
            }
        }
    }
}