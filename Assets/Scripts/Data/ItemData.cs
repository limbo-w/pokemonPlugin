using UnityEngine;
using System.Collections;

public class ItemData
{
    public enum ItemType
    {
        ITEM,
        MEDICINE,
        BERRY,
        TM,
        KEY
    }

    public enum BattleType
    {
        NONE,
        HPPPRESTORE,
        STATUSHEALER,
        POKEBALLS,
        BATTLEITEMS
    }

    private long basicGoodsId;
    private string name;
    private ItemType itemType;
    private BattleType battleType;
    private string description;
    private int price;
    private long tmNo;

    public enum ItemEffect
    {
        NONE,
        UNIQUE,
        HP,
        PP,
        STATUS,
        EV,
        EVOLVE,
        FLEE,
        BALL,
        STATBOOST,
        TM
    }

    private ItemEffect itemEffect;
    private string stringParameter;
    private float floatParameter;

    public ItemData(long id, string name, ItemType itemType, BattleType battleType, string description, int price)
    {
        this.basicGoodsId = id;
        this.name = name;
        this.itemType = itemType;
        this.battleType = battleType;
        this.description = description;
        this.price = price;
        this.itemEffect = ItemEffect.NONE;
    }

    public ItemData(long id, string name, ItemType itemType, BattleType battleType, string description, int price,
        ItemEffect itemEffect)
    {
        this.basicGoodsId = id;
        this.name = name;
        this.itemType = itemType;
        this.battleType = battleType;
        this.description = description;
        this.price = price;
        this.itemEffect = itemEffect;
    }

    public ItemData(long id, string name, ItemType itemType, BattleType battleType, string description, int price,
        ItemEffect itemEffect, string stringParameter)
    {
        this.basicGoodsId = id;
        this.name = name;
        this.itemType = itemType;
        this.battleType = battleType;
        this.description = description;
        this.price = price;
        this.itemEffect = itemEffect;
        this.stringParameter = stringParameter;
    }

    public ItemData(long id, string name, ItemType itemType, BattleType battleType, string description, int price,
        ItemEffect itemEffect, float floatParameter)
    {
        this.basicGoodsId = id;
        this.name = name;
        this.itemType = itemType;
        this.battleType = battleType;
        this.description = description;
        this.price = price;
        this.itemEffect = itemEffect;
        this.floatParameter = floatParameter;
    }

    public ItemData(long id, string name, ItemType itemType, BattleType battleType, string description, int price,
        ItemEffect itemEffect, string stringParameter, float floatParameter)
    {
        this.basicGoodsId = id;
        this.name = name;
        this.itemType = itemType;
        this.battleType = battleType;
        this.description = description;
        this.price = price;
        this.itemEffect = itemEffect;
        this.stringParameter = stringParameter;
        this.floatParameter = floatParameter;
    }

    //TMs
    public ItemData(long id, long tmNo, string name, ItemType itemType, BattleType battleType, string description, int price)
    {
        this.basicGoodsId = id;
        this.tmNo = tmNo;
        this.name = name;
        this.itemType = itemType;
        this.battleType = battleType;
        this.description = description;
        this.price = price;
        this.itemEffect = ItemEffect.TM;
    }

    public long getBasicGoodsId()
    {
        return basicGoodsId;
    }

    public string getName()
    {
        return name;
    }

    public ItemType getItemType()
    {
        return itemType;
    }

    public BattleType getBattleType()
    {
        return battleType;
    }

    public string getDescription()
    {
        return description;
    }

    public int getPrice()
    {
        return price;
    }

    public long getTMNo()
    {
        return tmNo;
    }

    public ItemEffect getItemEffect()
    {
        return itemEffect;
    }

    public string getStringParameter()
    {
        return stringParameter;
    }

    public float getFloatParameter()
    {
        return floatParameter;
    }
}