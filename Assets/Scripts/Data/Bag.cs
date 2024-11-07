using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

//物品信息承载对象
[System.Serializable]
public class BagItem
{
    public long holdId;
    public long basicGoodsId;
    public int tradable;
    public string itemName;
    public int amount;
}

[System.Serializable]
public class Bag
{
    List<BagItem> bagItemList = new List<BagItem>();

    public Bag() { }

    //通过holdId和物品图鉴id查找对应的item索引
    private int getIndexOf(long basicGoodsId, long holdId)
    {
        for (int i = 0; i < bagItemList.Count; i++)
        {
            if (bagItemList[i].basicGoodsId == basicGoodsId
                && bagItemList[i].holdId == holdId)
            {
                return i;
            }
        }
        return -1;
    }

    //获取背包物品种类数量
    [MethodImpl(MethodImplOptions.Synchronized)]
    public int getLength()
    {
        return bagItemList.Count;
    }


    //获取背包质量
    [MethodImpl(MethodImplOptions.Synchronized)]
    public int getQuantity(BagItem item)
    {
        int index = bagItemList.IndexOf(item);
        if (index != -1)
        {
            return bagItemList[index].amount;
        }
        return 0;
    }

    //添加物品
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool addItem(BagItem item)
    {
        try
        {
            int index = getIndexOf(item.basicGoodsId, item.holdId);
            if (index == -1)
            {
                bagItemList.Add(item);
                return true;
            }
            if (bagItemList[index].amount + item.amount > 999)
            {
                return false;
            }
            bagItemList[index].amount += item.amount;
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("addItem error:" + e);
            return false;
        }
    }

    //移除物品
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool removeItem(BagItem item)
    {
        try
        {
            int index = getIndexOf(item.basicGoodsId, item.holdId);
            if (index == -1)
            {
                return false;
            }
            if (bagItemList[index].amount - item.amount < 0)
            {
                return false;
            }
            bagItemList[index].amount -= item.amount;
            if (bagItemList[index].amount == 0)
            {
                bagItemList.RemoveAt(index);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("removeItem error:" + e);
            return false;
        }
    }

    //获取可卖物品
    [MethodImpl(MethodImplOptions.Synchronized)]
    public BagItem[] getSellableItemArray()
    {
        return getItemTypeArray(ItemData.ItemType.ITEM, true);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public BagItem[] getItemTypeArray(ItemData.ItemType itemType)
    {
        return getItemTypeArray(itemType, false);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private BagItem[] getItemTypeArray(ItemData.ItemType itemType, bool allSellables)
    {
        int length = getLength();
        BagItem[] result = new BagItem[length];
        int resultPos = 0;
        for (int i = 0; i < length; i++)
        {
            ItemData itemData = ItemDatabase.getItem(bagItemList[i].basicGoodsId);
            if (itemData == null)
            {
                continue;
            }
            if (!allSellables)
            {
                if (itemData.getItemType() == itemType)
                {
                    result[resultPos] = bagItemList[i];
                    resultPos += 1;
                }
            }
            else
            {
                if (itemData.getItemType() == ItemData.ItemType.ITEM ||
                    itemData.getItemType() == ItemData.ItemType.MEDICINE ||
                    itemData.getItemType() == ItemData.ItemType.BERRY)
                {
                    result[resultPos] = bagItemList[i];
                    resultPos += 1;
                }
            }
        }
        BagItem[] cleanedResult = new BagItem[resultPos];
        for (int i = 0; i < cleanedResult.Length; i++)
        {
            cleanedResult[i] = result[i];
        }
        return cleanedResult;
    }

    //获取战斗用物品
    [MethodImpl(MethodImplOptions.Synchronized)]
    public BagItem[] getBattleTypeArray(ItemData.BattleType battleType)
    {
        int length = getLength();
        BagItem[] result = new BagItem[length];
        int resultPos = 0;
        for (int i = 0; i < length; i++)
        {
            ItemData itemData = ItemDatabase.getItem(bagItemList[i].basicGoodsId);
            if (itemData == null)
            {
                continue;
            }
            if (itemData.getBattleType() == battleType)
            {
                result[resultPos] = bagItemList[i];
                resultPos += 1;
            }
        }
        BagItem[] cleanedResult = new BagItem[resultPos];
        for (int i = 0; i < cleanedResult.Length; i++)
        {
            cleanedResult[i] = result[i];
        }
        return cleanedResult;
    }
}