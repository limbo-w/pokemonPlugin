using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class MyTransactionDetailInfo
{
    public int id;
    public int transactionGoodsId;
    public int goodsId;
    public int holdGoodsId;
    public int sellerRoleId;
    public int buyerRoleId;
    public int status;
    public int payType;
    public int goldCoinsPrice;
    public int diamondsPrice;
    public string createTime;
    public string endTime;
    public int number;
    public int pokemonId;
    public int holdPokemonId;
    public int level;
    public int sex;
    public int isFlashing;
    public string characterId;
    public string eggId;
}

[Serializable]
public class GetMyTransactionInfo
{
    public int code;
    public string msg;
    public List<MyTransactionDetailInfo> completed;
    public List<MyTransactionDetailInfo> toBeTraded;
}
