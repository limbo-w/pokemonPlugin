using System;
using System.Collections;
using System.Collections.Generic;
using Proyecto26;
using UnityEngine;

public class BagManager
{
    //网络请求
    private const string BAG_GET_HANDLER_URL = HttpsUtils.DNS_URL + "/pokemon/app/appHoldGoods/currentUserList";
    private const string BAG_REMOVE_HANDLER_URL = HttpsUtils.DNS_URL + "/pokemon/app/appHoldGoods/removeGoods";
    private const string BAG_ADD_HANDLER_URL = HttpsUtils.DNS_URL + "/pokemon/app/appHoldGoods/getGoods";

    private static BagManager _instance = null;

    public delegate void HttpCallback(bool result);
    private Bag Bag = new Bag();

    private BagManager()
    {

    }

    public static BagManager Instance
    {
        get
        {
            return _instance ??= new BagManager();
        }
    }

    //获取当前用户物品信息
    public void fetchCurrentGoodList(HttpCallback callback)
    {
        if (getItemTypeArray(ItemData.ItemType.ITEM).Length > 0)
        {
            callback(true);
            return;
        }
        Dictionary<string, string> headers = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = BAG_GET_HANDLER_URL,
            Headers = headers,
            Params = paramz,
            Method = "GET",
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            BagGetResult result = JsonUtility.FromJson<BagGetResult>(netPokedexDatas.Text);
            //缓存数据
            if (result == null || result.rows == null || result.rows.Length == 0)
            {
                callback(false);
                return;
            }
            for (int i = 0; i < result.rows.Length; i++)
            {
                BagGetData data = result.rows[i];
                ItemData itemData = ItemDatabase.getItem(data.basicGoodsId);
                if (itemData != null)
                {
                    Bag.addItem(new BagItem()
                    {
                        amount = data.number,
                        itemName = itemData.getName(),
                        holdId = data.id,
                        basicGoodsId = data.basicGoodsId,
                        tradable = data.tradable
                    });
                }
            }
            callback(true);
        }).Catch(err =>
        {
            Debug.Log("GetCurrentUserList请求失败:" + err);
            callback(false);
        });
    }

    //删除物品并传回回调
    public void removeItem(BagItem item, HttpCallback callback)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = BAG_REMOVE_HANDLER_URL,
            Headers = headers,
            Params = paramz,
            Method = "POST",
            Body = new BagRemoveReq()
            {
                id = item.holdId,
                number = item.amount
            }
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            BagRemoveResult result = JsonUtility.FromJson<BagRemoveResult>(netPokedexDatas.Text);
            //缓存数据
            if (result == null || result.code != 200)
            {
                callback(false);
                return;
            }
            callback(Bag.removeItem(item));
        }).Catch(err =>
        {
            Debug.Log("GetCurrentUserList请求失败:" + err);
            callback(false);
        });
    }

    public bool removeItem(BagItem item)
    {
        return Bag.removeItem(item);
    }

    //添加物品并传回回调
    public void addItem(BagItem item, HttpCallback callback)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = BAG_ADD_HANDLER_URL,
            Headers = headers,
            Params = paramz,
            Method = "POST",
            Body = new BagAddReq()
            {
                basicGoodsId = item.basicGoodsId,
                number = item.amount,
                tradable = item.tradable
            }
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            BagAddResult result = JsonUtility.FromJson<BagAddResult>(netPokedexDatas.Text);
            //缓存数据
            if (result == null || result.code != 200)
            {
                callback(false);
                return;
            }
            item.holdId = result.data;
            callback(Bag.addItem(item));
        }).Catch(err =>
        {
            Debug.Log("GetCurrentUserList请求失败:" + err);
            callback(false);
        });
    }

    public bool addItem(BagItem item)
    {
        return Bag.addItem(item);
    }

    public BagItem[] getBattleTypeArray(ItemData.BattleType battleType)
    {
        return Bag.getBattleTypeArray(battleType);
    }

    //或者一种类型的物品
    public BagItem[] getItemTypeArray(ItemData.ItemType itemType)
    {
        return Bag.getItemTypeArray(itemType);
    }

    public int getQuantity(BagItem item)
    {
        return Bag.getQuantity(item);
    }
}
