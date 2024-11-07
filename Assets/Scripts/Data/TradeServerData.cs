using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

//交易购买请求
[Serializable]
public class TransactionBuyBodyRequest
{
    //交易种类1代表宝可梦，2代表物品
    public int transactionType;
    public long transactionId;
    //购买的数量 todo 
    public int number;
}

//确认交易请求
[Serializable]
public class ConfirmTransactionBodyRequest
{
    public long transactionId;
}

//商品上架请求
[Serializable]
public class OnTheShelfBodyRequest
{
    public string AppAuthorization;
    //交易货币类型1表示金币，2表示钻石
    public int payType;
    //交易种类1代表宝可梦，2代表物品
    public int transactionType;
    public long holdId;
    public int goldCoinsPrice;
    public int diamondsPrice;
    public int number;
}

//商品下架请求
[Serializable]
public class OffTheShelfBodyRequest
{
    //交易种类1代表宝可梦，2代表物品
    public int transactionType;
    public long transactionId;
    public long holdId;
}

//通用空请求
[Serializable]
public class CommonTradeBodyRequest
{

}

//==========================================================================

[Serializable]
public class TransactionDetailInfo
{
    //精灵交易id，传入为transactionId
    public long id;
    public long transactionPokemonDetailedId;
    public long pokemonId;
    public long holdPokemonId;
    public int level;
    public int sex;
    public int isFlashing;
    public int characterId;
    public string eggId;
    //物品
    public long transactionGoodsDetailedId;
    public long goodsId;
    public long holdGoodsId;
    public int number;
    //公共部分
    public long sellerRoleId;
    public int payType;
    public int goldCoinsPrice;
    public int diamondsPrice;
    public string createTime;
    //交易状态0无状态，1表示交易中，2表示交易完成
    public int tradeState;
    //技能列表
    public SkillInfo[] skillIdsObj;
    public BasicPokemon basicPokemon;
    public AppDetailedPokemon appDetailedPokemon;
}

//获取用户交易信息
[Serializable]
public class GetCurTransactionInfoResult
{
    public int code;
    public string msg;
    public TransactionInfoData data;
}

[Serializable]
public class TransactionInfoData
{
    public List<TransactionDetailInfo> completed;
    public List<TransactionDetailInfo> toBeTraded;
}

//获取可交易精灵信息
[Serializable]
public class GetPokemonTransactionListResult
{
    public int total;
    public int code;
    public string msg;
    public int pageNum;
    public List<TransactionDetailInfo> rows;
}

//获取可交易物品信息
[Serializable]
public class GetGoodTransactionListResult
{
    public int total;
    public int code;
    public string msg;
    public int pageNum;
    public List<TransactionDetailInfo> rows;
}