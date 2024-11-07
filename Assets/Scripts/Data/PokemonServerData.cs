using System;

//交换精灵
[Serializable]
public class PokemonSwapReq
{
    public long exchange1;
    public long exchange2;
}

//删除精灵
[Serializable]
public class PokemonRemoveReq
{
    //精灵持有id
    public long id;
}

[Serializable]
public class PokemonRemoveResult
{
    public int code;
    public string msg;
}

//增加精灵
[Serializable]
public class PokemonAddReq
{
    //精灵图鉴id
    public long pokemonId;
    public int level;
    //性别sex=0表示男，1表示女，2表示未知
    public int sex;
    public int chp;
    //技能列表
    public SkillInfo[] skillIdsObj;
}

[Serializable]
public class PokemonAddResult
{
    public PokemonGetData data;
    public int code;
    public string msg;
}

[Serializable]
public class PokemonGetResult
{
    public int total;
    public PokemonGetData[] rows;
    public int code;
    public string msg;
    public int pageNum;
}

[Serializable]
public class PokemonGetData
{
    //精灵持有id
    public long id;
    //精灵基础图鉴id
    public long pokemonId;
    //用户玩家角色id
    public long roleId;
    //昵称
    public string nickName;
    //最初的使用者
    public string originalOwner;
    public int level;
    public int sex;
    //性格
    public int characterId;
    //亲密度
    public int intimacy;
    //是否是闪光
    public int isFlashing;
    //经验值
    public int empiricalValues;
    //当前血量
    public int chp;
    //物品持有Id
    public long goodsId;
    //位置index
    public int index;
    //存储状态[1:手里携带; 2:存放电脑; 3:寄养商店; 4:用户交换; 5:商城寄卖; 6:自然放生]
    public int storageStatus;
    //技能列表
    public SkillInfo[] skillIdsObj;

    //最大血量
    public int hp;
    public int atk;
    public int def;
    public int spA;
    public int spD;
    public int spe;
    public BasicPokemon basicPokemon;
    public AppDetailedPokemon appDetailedPokemon;
}

[Serializable]
public class BasicPokemon
{
    //精灵基础图鉴id
    public long id;
    public string pokemonName;
    public int pokemonNumber;
    public int initialIntimacy;
    public int initialEmpiricalValues;
    public string eggGroupId;
    public int initialHp;
    public int initialAtk;
    public int initialDef;
    public int initialSpA;
    public int initialSpD;
    public int initialSpe;
    //捕捉位置
    public string captureLoc;
    //描述
    public string description;
}

[Serializable]
public class SkillInfo
{
    public long skillId;
    //当前pp值
    public int curPp;
    //自有pp上限
    public int pp;
    //最终最大pp上限
    public int maxPp;
}

//努力信息表
[Serializable]
public class AppDetailedPokemon
{
    //精灵基础图鉴id
    public long id;
    //精灵持有id
    public long holdPokemonId;
    public int effortHp;
    public int effortAtk;
    public int effortDef;
    public int effortSpA;
    public int effortSpD;
    public int effortSpe;
    public int individualHp;
    public int individualAtk;
    public int individualDef;
    public int individualSpA;
    public int individualSpD;
    public int individualSpe;
}
