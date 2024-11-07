using System;
using System.Collections.Generic;

[Serializable]
public class PokedexData
{
    //图鉴id
    public long id;
    //精灵名字
    public string pokemonName;
    //精灵官方编号
    public string pokemonNumber;
    //初始亲密度
    public int initialIntimacy;
    //基础经验值
    public int initialEmpiricalValues;
    public string eggGroupId;
    public int initialHp;
    public int initialAtk;
    public int initialDef;
    public int initialSpA;
    public int initialSpD;
    public int initialSpe;
    //0是未遭遇，1是遭遇，2是捕捉
    public int status;
    public string captureLoc;
    public string description;
}

[Serializable]
public class PokedexResult
{
    public int total;
    public PokedexData[] rows;
    public int code;
    public string msg;
    public int pageNum;
}

[Serializable]
public class UpdateAtlasRequest
{
    public string bPId;
    public string status;
}

[Serializable]
public class UpdateAtlasResult
{
    public int code;
    public string msg;
}