using System;
[Serializable]
public class BagRemoveReq
{
    //道具持有id
    public long id;
    public int number;
}

public class BagAddReq
{
    //道具基础图鉴id
    public long basicGoodsId;
    public int number;
    public int tradable;
}


[Serializable]
public class BagGetData
{
    //道具持有id
    public long id;
    //道具基础图鉴id
    public long basicGoodsId;
    public long roleId;
    public int tradable;
    public int number;
    public BasicGoods basicGoods;

}

[Serializable]
public class BasicGoods
{
    //道具基础图鉴id，本身无用
    public long id;
    public string name;
    public int type;
    public int money;
    public int maxSize;
    public string describe;
}


[Serializable]
public class BagGetResult
{
    public int total;
    public BagGetData[] rows;
    public int code;
    public string msg;
    public int pageNum;
}

[Serializable]
public class BagAddResult
{
    public int code;
    public string msg;
    //holdId号
    public long data;
}

[Serializable]
public class BagRemoveResult
{
    public int code;
    public string msg;  
}