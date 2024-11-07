using System;
using System.Collections;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class BagShopViewModel : ViewModelBase
{
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
    private BagHandler handler;
    public BagItemViewModel itemViewModel;
    private int shopSelectedQuantity;
    public bool shopMode;
    public BagItem[] shopItemList;

    public BagShopViewModel(BagHandler bagHandler)
    {
        handler = bagHandler;
    }

    //重置
    public void reset(bool mode, BagItem[] shopStock)
    {
        dialog = DialogBoxUIHandler.main;
        shopMode = mode;
        shopItemList = shopStock;
        updateMoneyBox();
        updateDataBox();
    }

    public IEnumerator onItemClick(int selected)
    {
        SfxHandler.Play(handler.selectClip);
        TrainerInfoData trainerInfoData = TrainerManager.Instance.getTrainerInfoData();
        ItemData selectedItem = ItemDatabase.getItem(shopItemList[selected].basicGoodsId);
        itemViewModel.switching = true;
        itemViewModel.chosenItem = shopItemList[selected];
        if (itemViewModel.currentScreen == 1)
        {
            if (trainerInfoData.playerMoney >= selectedItem.getPrice())
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceTextWithInput(selectedItem.getName() + "? 你想买多少呢?"));
                int chosenIndex = dialog.buttonIndex;
                if (chosenIndex == 1)
                {
                    shopSelectedQuantity = dialog.getInputNumber();
                    //最大数量
                    int maxQuantity = Mathf.FloorToInt((float)trainerInfoData.playerMoney / (float)selectedItem.getPrice());
                    if (maxQuantity < shopSelectedQuantity)
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("你没有足够的钱."));
                    }
                    else
                    {
                        yield return StaticCoroutine.StartCoroutine(updateNumbersBox(selectedItem.getPrice(), maxQuantity));
                        if (shopSelectedQuantity > 0)
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("你想要" + shopSelectedQuantity + "个" + selectedItem.getName() + ",这将花费"
                            + (shopSelectedQuantity * selectedItem.getPrice()) + ",可以吗?", 0.5f));

                            string[] choiceInfos = new string[] { "同意", "反对" };
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choiceInfos));
                            chosenIndex = dialog.buttonIndex;
                            if (chosenIndex == 1)
                            {
                                SfxHandler.Play(handler.selectClip);
                                itemViewModel.addItem(new BagItem()
                                {
                                    basicGoodsId = selectedItem.getBasicGoodsId(),
                                    tradable = 1,
                                    itemName = selectedItem.getName(),
                                    amount = shopSelectedQuantity,
                                });
                                trainerInfoData.playerMoney -= (shopSelectedQuantity * selectedItem.getPrice());
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("给你,非常感谢."));
                            }
                            else
                            {
                                SfxHandler.Play(handler.selectClip);
                            }
                        }
                    }
                }
                else
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("你放弃了购买物品."));
                }
            }
            else
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("你没有足够的钱."));
            }
        }
        else
        {
            //卖东西
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceTextWithInput(selectedItem.getName() + "你想卖多少呢？"));
            int chosenIndex = dialog.buttonIndex;
            if (chosenIndex == 1)
            {
                shopSelectedQuantity = dialog.getInputNumber();
                int maxQuantity = BagManager.Instance.getQuantity(itemViewModel.currentItemList[selected]);
                if (maxQuantity < shopSelectedQuantity)
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("你没有足够的物品数量."));
                }
                else
                {
                    yield return StaticCoroutine.StartCoroutine(updateNumbersBox(Mathf.FloorToInt((float)selectedItem.getPrice() / 2f), maxQuantity));
                    if (shopSelectedQuantity > 0)
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("我可以支付" + (shopSelectedQuantity *
                            Mathf.FloorToInt((float)selectedItem.getPrice() / 2f)) + ",可以吗？", 0.5f));

                        string[] choiceInfos = new string[] { "同意", "反对" };
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choiceInfos));
                        chosenIndex = dialog.buttonIndex;
                        if (chosenIndex == 1)
                        {
                            itemViewModel.removeItem(itemViewModel.currentItemList[selected]);
                            trainerInfoData.playerMoney += shopSelectedQuantity * Mathf.FloorToInt((float)selectedItem.getPrice() / 2f);
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("交出" + selectedItem.getName()
                                   + "获得" + (shopSelectedQuantity * Mathf.FloorToInt((float)selectedItem.getPrice() / 2f))));
                        }
                    }
                }
            }
            else
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("你放弃了卖出物品."));
            }
        }
        updateMoneyBox();
        updateDataBox();

        itemViewModel.switching = false;
        itemViewModel.updateItemInfo();
    }

    private IEnumerator updateNumbersBox(int price, int max)
    {
        string numberString = "" + shopSelectedQuantity;
        if (shopSelectedQuantity < 10)
        {
            numberString = "0" + shopSelectedQuantity;
        }

        NumberText = "x " + numberString + " 金额:" + (price * shopSelectedQuantity);
        yield return null;
    }

    private void updateMoneyBox()
    {
        if (shopMode)
        {
            MoneyBoxShow = true;
            TrainerInfoData trainerInfoData = TrainerManager.Instance.getTrainerInfoData();
            string playerMoney = "" + trainerInfoData.playerMoney;
            char[] playerMoneyChars = playerMoney.ToCharArray();
            playerMoney = "";
            for (int i = 0; i < playerMoneyChars.Length; i++)
            {
                playerMoney = playerMoneyChars[playerMoneyChars.Length - 1 - i] + playerMoney;
                if ((i + 1) % 3 == 0 && i != playerMoneyChars.Length - 1)
                {
                    playerMoney = "," + playerMoney;
                }
            }
            MoneyText = "$" + playerMoney;
        }
        else
        {
            MoneyBoxShow = false;
        }
    }

    private void updateDataBox()
    {
        if (shopMode)
        {
            DataBoxShow = true;
        }
        else
        {
            DataBoxShow = false;
        }
    }

    //==========================================================================
    private bool shopVisible = false;
    public bool ShopVisible
    {
        get { return this.shopVisible; }
        set { this.Set<bool>(ref this.shopVisible, value, "ShopVisible"); }
    }

    private bool numbersBoxShow = true;
    private bool moneyBoxShow = true;
    private bool dataBoxShow = true;
    private string numberText = "";
    private string moneyText = "";

    public string NumberText
    {
        get { return this.numberText; }
        set { this.Set<string>(ref this.numberText, value, "NumberText"); }
    }

    public string MoneyText
    {
        get { return this.moneyText; }
        set { this.Set<string>(ref this.moneyText, value, "MoneyText"); }
    }

    public bool NumbersBoxShow
    {
        get { return this.numbersBoxShow; }
        set { this.Set<bool>(ref this.numbersBoxShow, value, "NumbersBoxShow"); }
    }

    public bool MoneyBoxShow
    {
        get { return this.moneyBoxShow; }
        set { this.Set<bool>(ref this.moneyBoxShow, value, "MoneyBoxShow"); }
    }

    public bool DataBoxShow
    {
        get { return this.dataBoxShow; }
        set { this.Set<bool>(ref this.dataBoxShow, value, "DataBoxShow"); }
    }
}