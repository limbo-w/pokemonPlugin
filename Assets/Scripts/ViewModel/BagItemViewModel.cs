using System;
using System.Collections;
using System.Collections.Specialized;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Execution;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class BagItemViewModel : ViewModelBase
{
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
    public bool switching = false;
    public bool running;
    private BagHandler handler;
    private ObservableList<ItemIconInfo> itemInfoList;
    public BagShopViewModel shopViewModel;
    public BagPartyViewModel partyViewModel;
    //战斗对应的ViewModel
    public BattleViewModel battleViewModel;
    private int visableSlots = 7;
    //当前的展示页号
    public int currentScreen = 1;
    //当前选中的item位置index+1
    public int selectedPosition;
    //顶部物品位置
    public int[] currentTopPosition = new int[]
    {
        0, 0, 0, 0, 0, 0
    };
    public BagItem[] currentItemList;
    public BagItem chosenItem;
    public string chosenChoice;

    private string itemDescription;
    private bool itemDescriptionShow = true;
    public string ItemDescription
    {
        get { return this.itemDescription; }
        set { this.Set<string>(ref this.itemDescription, value, "ItemDescription"); }
    }

    public bool ItemDescriptionShow
    {
        get { return this.itemDescriptionShow; }
        set { this.Set<bool>(ref this.itemDescriptionShow, value, "ItemDescriptionShow"); }
    }

    public class ItemIconInfo : ObservableObject
    {
        private int index;
        private Color color = Color.white;
        private bool isActive = true;
        private string name = "";
        private Texture itemSlotTex;
        private Texture iconTex;
        private string itemQuantity = "";

        public Texture ItemSlotTex
        {
            get { return this.itemSlotTex; }
            set { this.Set<Texture>(ref this.itemSlotTex, value, "ItemSlotTex"); }
        }

        public string ItemQuantity
        {
            get { return this.itemQuantity; }
            set { this.Set<string>(ref this.itemQuantity, value, "ItemQuantity"); }
        }

        public Texture IconTex
        {
            get { return this.iconTex; }
            set { this.Set<Texture>(ref this.iconTex, value, "IconTex"); }
        }

        public string Name
        {
            get { return this.name; }
            set { this.Set<string>(ref this.name, value, "Name"); }
        }

        public bool IsActive
        {
            get { return this.isActive; }
            set { this.Set<bool>(ref this.isActive, value, "IsActive"); }
        }

        public int Index
        {
            get { return this.index; }
            set { this.Set<int>(ref this.index, value, "Index"); }
        }

        public Color Color
        {
            get { return this.color; }
            set { this.Set<Color>(ref this.color, value, "Color"); }
        }
    }

    public BagItemViewModel(BagHandler bagHandler,
        NotifyCollectionChangedEventHandler itemInfoHandler)
    {
        handler = bagHandler;
        this.onItem = new SimpleCommand<int>(OnItemClick);
        this.onBag = new SimpleCommand<int>(OnBagClick);
        this.itemInfoList = new ObservableList<ItemIconInfo>();
        itemInfoList.CollectionChanged += itemInfoHandler;
        for (int i = 0; i < 7; i++)
        {
            itemInfoList.Add(new ItemIconInfo()
            {
                IsActive = false,
                Index = i
            });
        }
    }

    public void clear()
    {
        currentTopPosition = new int[] { 0, 0, 0, 0, 0, 0 };
        currentScreen = 1;
        updateItemInfo();
    }

    public void updateItemInfo()
    {
        dialog = DialogBoxUIHandler.main;
        switching = false;
        selectedPosition = 0;
        updateScreen();
        if (currentItemList != null)
        {
            updateItemList();
            updateSelectedItem(-1);
        }
    }

    //==========================================================================
    private readonly SimpleCommand<int> onItem;

    public ICommand OnItem
    {
        get { return this.onItem; }
    }

    private void OnItemClick(int index)
    {
        StaticCoroutine.StartCoroutine(onItemClick(index));
    }

    //物品点击
    private IEnumerator onItemClick(int position)
    {
        SfxHandler.Play(handler.selectClip);
        selectedPosition = position;
        int selected = selectedPosition + currentTopPosition[currentScreen];
        updateSelectedItem(position);
        updateDescription();
        if (shopViewModel.shopMode)
        {
            yield return StaticCoroutine.StartCoroutine(shopViewModel.onItemClick(selected));
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            chosenItem = currentItemList[selected];
            ItemData selectedItem = ItemDatabase.getItem(chosenItem.basicGoodsId);

            dialog.drawTextInstant("你想对" + selectedItem.getName() + "干什么?");
            string[] choices = new string[] { "取消" };
            if (battleViewModel != null)
            {
                if (selectedItem.getItemEffect() != ItemData.ItemEffect.NONE &&
                     selectedItem.getItemEffect() != ItemData.ItemEffect.FLEE &&
                     selectedItem.getItemEffect() != ItemData.ItemEffect.STATBOOST &&
                     selectedItem.getItemEffect() != ItemData.ItemEffect.UNIQUE &&
                     selectedItem.getItemEffect() != ItemData.ItemEffect.TM)
                {
                    choices = new string[] { "使用", "取消", };
                }
            }
            else
            {
                if (selectedItem.getItemType() == ItemData.ItemType.KEY)
                {
                    if (selectedItem.getItemEffect() == ItemData.ItemEffect.UNIQUE)
                    {
                        choices = new string[] { "使用", "登记", "取消", };
                    }
                }
                else if (selectedItem.getItemEffect() != ItemData.ItemEffect.NONE &&
                     selectedItem.getItemEffect() != ItemData.ItemEffect.FLEE &&
                     selectedItem.getItemEffect() != ItemData.ItemEffect.BALL &&
                     selectedItem.getItemEffect() != ItemData.ItemEffect.STATBOOST)
                {
                    choices = new string[] { "使用", "取消", };
                }
            }
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
            int chosenIndex = dialog.buttonIndex;
            chosenChoice = choices[choices.Length - chosenIndex - 1];
            if (chosenChoice == "使用")
            {
                SfxHandler.Play(handler.selectClip);
                //特殊处理战斗中使用精灵球的情况
                if (battleViewModel != null
                    && selectedItem.getItemEffect() == ItemData.ItemEffect.BALL)
                {
                    BagManager.Instance.removeItem(new BagItem
                    {
                        holdId = chosenItem.holdId,
                        basicGoodsId = chosenItem.basicGoodsId,
                        amount = 1
                    }, delegate (bool result)
                    {
                        if (result)
                        {
                            running = false;
                            battleViewModel.DoBagItemButtonClickDown(selectedItem, 0);
                        }
                        else
                        {
                            StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("使用精灵球失败"));
                            switching = false;
                            selectedPosition = 0;
                            updateItemInfo();
                        }
                    });
                }
                else
                {
                    int partyLength = PCManager.Instance.getSubBoxPokemonLength(0);
                    if (partyLength > 0)
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("对哪个精灵使用" + selectedItem.getName() + "?"));
                    }
                    else
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("没有精灵可以使用!"));
                    }
                    switching = true;
                }
            }
            else if (chosenChoice == "登记")
            {
                SfxHandler.Play(handler.selectClip);
                if (selectedItem.getItemType() == ItemData.ItemType.KEY)
                {
                    yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilent("注册" + selectedItem.getName() + "到哪个卡槽呢?"));
                    choices = new string[]
                    {
                       "卡槽1", "卡槽2", "卡槽3", "卡槽4", "取消"
                    };
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
                    chosenIndex = dialog.buttonIndex;
                    if (chosenIndex == 4)
                    {
                        GlobalVariables.global.setRegisterItem(0, selectedItem.getBasicGoodsId());
                    }
                    else if (chosenIndex == 3)
                    {
                        GlobalVariables.global.setRegisterItem(1, selectedItem.getBasicGoodsId());
                    }
                    else if (chosenIndex == 2)
                    {
                        GlobalVariables.global.setRegisterItem(2, selectedItem.getBasicGoodsId());
                    }
                    else if (chosenIndex == 1)
                    {
                        GlobalVariables.global.setRegisterItem(3, selectedItem.getBasicGoodsId());
                    }
                }
                else
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("该物品没法进行登记!"));
                }
                switching = true;
            }
            else if (chosenChoice == "给予")
            {
                SfxHandler.Play(handler.selectClip);
                int partyLength = PCManager.Instance.getSubBoxPokemonLength(0);
                if (partyLength > 0)
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("给予哪个精灵" + selectedItem.getName() + "?"));
                }
                else
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("没有精灵可以给予!"));
                }
                switching = true;
            }
            else if (chosenChoice == "取消")
            {
                SfxHandler.Play(handler.selectClip);
                switching = false;
                selectedPosition = 0;
                updateItemInfo();
            }
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.2f);
    }

    //物品列表刷新
    private void updateItemList()
    {
        BagItem[] items = new BagItem[7];
        for (int i = 0; i < 7; i++)
        {
            //算出对应的Index
            int index = i + currentTopPosition[currentScreen];
            if (index < 0 || index >= currentItemList.Length)
            {
                items[i] = null;
            }
            else
            {
                items[i] = currentItemList[index];
            }
        }
        for (int i = 0; i < 7; i++)
        {
            if (items[i] == null)
            {
                itemInfoList[i] = new ItemIconInfo()
                {
                    Index = i,
                    IsActive = false
                };
            }
            else
            {
                ItemData item = ItemDatabase.getItem(items[i].basicGoodsId);
                Texture icon = Resources.Load<Texture>("null");
                if (item.getItemType() == ItemData.ItemType.TM)
                {
                    MoveData moveData = MoveDatabase.getMove(item.getBasicGoodsId());
                    if (moveData != null)
                    {
                        icon = Resources.Load<Texture>("Items/tm" + moveData.getType().ToString());
                    }
                }
                else
                {
                    icon = Resources.Load<Texture>("Items/" + CommonUtils.convertLongID(item.getBasicGoodsId()));
                }
                string itemQuantity = "";
                if (item.getItemType() == ItemData.ItemType.TM)
                {
                    itemQuantity = "No. " + item.getTMNo();
                }
                else
                {
                    if (shopViewModel.shopMode)
                    {
                        itemQuantity = "$" + item.getPrice();
                    }
                    else
                    {
                        itemQuantity = "   x" + BagManager.Instance.getQuantity(items[i]);
                    }
                }

                itemInfoList[i] = new ItemIconInfo()
                {
                    IsActive = true,
                    Index = i,
                    Name = items[i].itemName,
                    Color = Color.black,
                    IconTex = icon,
                    ItemQuantity = itemQuantity
                };
            }
        }
    }

    //更新选中的物品
    private void updateSelectedItem(int position)
    {
        if (position >= 0)
        {
            selectedPosition = position;
        }
        for (int i = 0; i < 7; i++)
        {
            if (position == i)
            {
                itemInfoList[i] = new ItemIconInfo()
                {
                    IsActive = true,
                    Index = i,
                    ItemSlotTex = handler.itemListHighlightTex,
                    Color = Color.white
                };
            }
            else
            {
                itemInfoList[i] = new ItemIconInfo()
                {
                    IsActive = true,
                    Index = i,
                    ItemSlotTex = handler.itemListTex,
                    Color = Color.black
                };
            }
        }
    }

    //更新描述语
    private void updateDescription()
    {
        int index = selectedPosition + currentTopPosition[currentScreen];
        if (index < currentItemList.Length)
        {
            BagItem selectedItem = currentItemList[index];
            if (currentScreen != 4)
            {
                //物品信息内容描述
                ItemDescription = ItemDatabase.getItem(selectedItem.basicGoodsId).getDescription();
            }
        }
        else
        {
            ItemDescription = "";
        }
    }

    //更新屏幕
    private void updateScreen()
    {
        //物品置顶的位置
        if (currentScreen == 4 && !shopViewModel.shopMode)
        {
            currentItemList = BagManager.Instance.getItemTypeArray(ItemData.ItemType.TM);
            ItemDescriptionShow = false;
        }
        else
        {
            if (!shopViewModel.shopMode)
            {
                if (currentScreen == 1)
                {
                    currentItemList = BagManager.Instance.getItemTypeArray(ItemData.ItemType.ITEM);
                }
                else if (currentScreen == 2)
                {
                    currentItemList = BagManager.Instance.getItemTypeArray(ItemData.ItemType.MEDICINE);
                }
                else if (currentScreen == 3)
                {
                    currentItemList = BagManager.Instance.getItemTypeArray(ItemData.ItemType.BERRY);
                }
                else if (currentScreen == 5)
                {
                    currentItemList = BagManager.Instance.getItemTypeArray(ItemData.ItemType.KEY);
                }
            }
            else
            {
                currentItemList = shopViewModel.shopItemList;
            }
            ItemDescriptionShow = true;
        }
    }

    //添加物品
    public void addItem(BagItem bagItem)
    {
        BagManager.Instance.addItem(bagItem, delegate (bool result)
        {
            if (result)
            {
                updateItemInfo();
            }
        });
    }

    //移除物品
    public void removeItem(BagItem bagItem)
    {
        BagManager.Instance.removeItem(bagItem, delegate (bool result)
        {
            if (result)
            {
                updateItemInfo();
            }
        });
    }

    //=========================================================================
    private readonly SimpleCommand<int> onBag;

    public ICommand OnBag
    {
        get { return this.onBag; }
    }

    private void OnBagClick(int index)
    {
        SfxHandler.Play(handler.selectClip);
        if (!switching && !shopViewModel.shopMode)
        {
            currentScreen = index + 1;
            updateItemInfo();
            partyViewModel.updatePartyInfo();
        }
    }

    public void OnReturn()
    {
        if (battleViewModel != null)
        {
            battleViewModel.OnTaskReturn();
        }
        running = false;
    }

    public void OnUp()
    {
        SfxHandler.Play(handler.selectClip);
        if (currentTopPosition[currentScreen] >= visableSlots)
        {
            currentTopPosition[currentScreen] -= visableSlots;
        }
        else
        {
            currentTopPosition[currentScreen] = 0;
        }
        updateItemInfo();
        partyViewModel.updatePartyInfo();
    }

    public void OnDown()
    {
        SfxHandler.Play(handler.selectClip);
        if (currentTopPosition[currentScreen] < currentItemList.Length - visableSlots)
        {
            currentTopPosition[currentScreen] += visableSlots;
        }
        updateItemInfo();
        partyViewModel.updatePartyInfo();
    }

    public void OnLeft()
    {
        SfxHandler.Play(handler.selectClip);
        if (!switching)
        {
            if (currentScreen > 1)
            {
                if (!shopViewModel.shopMode)
                {
                    currentScreen -= 1;
                }
                else
                {
                    currentScreen = 1;
                }
                updateItemInfo();
                partyViewModel.updatePartyInfo();
            }
        }
    }

    public void OnRight()
    {
        SfxHandler.Play(handler.selectClip);
        if (!switching)
        {
            if (currentScreen < 5)
            {
                if (!shopViewModel.shopMode)
                {
                    currentScreen += 1;
                }
                else
                {
                    currentScreen = 1;
                }
                updateItemInfo();
                partyViewModel.updatePartyInfo();
            }
        }
    }
}
