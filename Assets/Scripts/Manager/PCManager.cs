using System.Collections.Generic;
using System.Linq;
using Proyecto26;
using UnityEngine;

/**
 * 电脑管理单例
 */
public class PCManager
{
    //查询手上精灵
    private const string PC_GET_CARRY_POKEMON_URL = HttpsUtils.DNS_URL + "/pokemon/app/appHoldPokemon/carryPokemon";
    //查询仓库精灵
    private const string PC_GET_WARE_HOUSE_POKEMON_URL = HttpsUtils.DNS_URL + "/pokemon/app/appHoldPokemon/currentWarehouseList";
    //释放精灵
    private const string PC_POST_FREE_POKEMON_URL = HttpsUtils.DNS_URL + "/pokemon/app/appHoldPokemon/freePokemon";
    //增加精灵
    private const string PC_POST_ADD_POKEMON_URL = HttpsUtils.DNS_URL + "/pokemon/app/appHoldPokemon/addPokemon";
    //更新精灵
    private const string PC_POST_UPDATE_POKEMON_URL = HttpsUtils.DNS_URL + "/pokemon/app/appHoldPokemon/updatePokemon";
    //交换精灵
    private const string PC_POST_EXCHANGE_POKEMON_URL = HttpsUtils.DNS_URL + "/pokemon/app/appHoldPokemon/exchangePoke";

    public delegate void HttpCallback(bool result);
    private PC PC = new PC();

    private static PCManager _instance = null;
    private PCManager()
    {
    }

    public static PCManager Instance
    {
        get
        {
            return _instance ??= new PCManager();
        }
    }

    public void fetchPartyPokemonList(HttpCallback callback)
    {
        if (getSubBoxPokemonLength(0) > 0)
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
            Uri = PC_GET_CARRY_POKEMON_URL,
            Headers = headers,
            Params = paramz,
            Method = "GET",
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            PokemonGetResult result = JsonUtility.FromJson<PokemonGetResult>(netPokedexDatas.Text);
            if (result == null || result.rows == null || result.rows.Length == 0)
            {
                callback(false);
                return;
            }
            for (int i = 0; i < result.rows.Length; i++)
            {
                PokemonGetData data = result.rows[i];
                AppDetailedPokemon appDetailedPokemon = data.appDetailedPokemon;
                Pokemon pokemon = new Pokemon(data.id, data.pokemonId, data.nickName, Pokemon.Gender.CALCULATE, data.level, 0, new BagItem(), "",
                    appDetailedPokemon.individualHp, appDetailedPokemon.individualAtk, appDetailedPokemon.individualDef, appDetailedPokemon.individualSpA, appDetailedPokemon.individualSpD, appDetailedPokemon.individualSpe,
                    appDetailedPokemon.effortHp, appDetailedPokemon.effortAtk, appDetailedPokemon.effortDef, appDetailedPokemon.effortSpA, appDetailedPokemon.effortSpD, appDetailedPokemon.effortSpe,
                    "", data.skillIdsObj != null ? data.skillIdsObj.ToList() : new List<SkillInfo>());
                pokemon.setCurrentHp(data.chp);
                PC.addPokemon(pokemon, 0);
            }
            callback(true);
        }).Catch(err =>
        {
            Debug.Log("fetchPartyPokemonList请求失败:" + err);
            callback(false);
        });
    }

    public void fetchBoxPokemonList(HttpCallback callback)
    {
        if (getWarehousePokemonLength() > 0)
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
            Uri = PC_GET_WARE_HOUSE_POKEMON_URL,
            Headers = headers,
            Params = paramz,
            Method = "GET",
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            PokemonGetResult result = JsonUtility.FromJson<PokemonGetResult>(netPokedexDatas.Text);
            if (result == null || result.rows == null || result.rows.Length == 0)
            {
                callback(false);
                return;
            }
            for (int i = 0; i < result.rows.Length; i++)
            {
                PokemonGetData data = result.rows[i];
                AppDetailedPokemon appDetailedPokemon = data.appDetailedPokemon;
                Pokemon pokemon = new Pokemon(data.id, data.pokemonId, data.nickName, Pokemon.Gender.CALCULATE, data.level, 0, new BagItem(), "",
                    appDetailedPokemon.individualHp, appDetailedPokemon.individualAtk, appDetailedPokemon.individualDef, appDetailedPokemon.individualSpA, appDetailedPokemon.individualSpD, appDetailedPokemon.individualSpe,
                    appDetailedPokemon.effortHp, appDetailedPokemon.effortAtk, appDetailedPokemon.effortDef, appDetailedPokemon.effortSpA, appDetailedPokemon.effortSpD, appDetailedPokemon.effortSpe,
                    "", data.skillIdsObj != null ? data.skillIdsObj.ToList() : new List<SkillInfo>());
                pokemon.setCurrentHp(data.chp);
                PC.addPokemon(pokemon, 1);
            }
            callback(true);
        }).Catch(err =>
        {
            Debug.Log("fetchBoxPokemonList请求失败:" + err);
            callback(false);
        });
    }
    //===========================================================================
    //更新宝可梦
    public void updatePokemons(List<Pokemon> pokemonList, HttpCallback callback)
    {
        List<PokemonGetData> pokemonGetDataList = new List<PokemonGetData>();
        foreach (Pokemon pokemon in pokemonList)
        {
            pokemonGetDataList.Add(new PokemonGetData()
            {
                id = pokemon.getHoldId(),
                pokemonId = pokemon.getID(),
                level = pokemon.getLevel(),
                skillIdsObj = pokemon.getSkillInfos().ToArray(),
                appDetailedPokemon = new AppDetailedPokemon
                {
                    id = pokemon.getID(),
                    holdPokemonId = pokemon.getHoldId(),
                    individualAtk = pokemon.getIV_ATK(),
                    individualDef = pokemon.getIV_DEF(),
                    individualHp = pokemon.getIV_HP(),
                    individualSpA = pokemon.getIV_SPA(),
                    individualSpD = pokemon.getIV_SPD(),
                    individualSpe = pokemon.getIV_SPE(),
                }
            });
        }
        Dictionary<string, string> headers = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = PC_POST_UPDATE_POKEMON_URL,
            Headers = headers,
            Params = paramz,
            Method = "POST",
            Body = pokemonGetDataList
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            PokemonRemoveResult result = JsonUtility.FromJson<PokemonRemoveResult>(netPokedexDatas.Text);
            if (result == null || result.code != 200)
            {
                callback(false);
                return;
            }
            callback(true);
        }).Catch(err =>
        {
            Debug.Log("updatePokemon请求失败:" + err);
            callback(false);
        });
    }

    //释放宝可梦
    public void releasePokemon(Pokemon pokemon, HttpCallback callback)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = PC_POST_FREE_POKEMON_URL,
            Headers = headers,
            Params = paramz,
            Method = "POST",
            Body = new PokemonRemoveReq()
            {
                id = pokemon.getHoldId(),
            }
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            PokemonRemoveResult result = JsonUtility.FromJson<PokemonRemoveResult>(netPokedexDatas.Text);
            if (result == null || result.code != 200)
            {
                callback(false);
                return;
            }
            callback(removePokemon(pokemon));
        }).Catch(err =>
        {
            Debug.Log("fetchBoxPokemonList请求失败:" + err);
            callback(false);
        });
    }

    public bool removePokemon(Pokemon pokemon)
    {
        return PC.removePokemon(pokemon);
    }

    public bool addPokemon(Pokemon pokemon)
    {
        return PC.addPokemon(pokemon, 0);
    }

    //添加宝可梦
    public void addPokemon(Pokemon pokemon, HttpCallback callback)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = PC_POST_ADD_POKEMON_URL,
            Headers = headers,
            Params = paramz,
            Method = "POST",
            Body = new PokemonAddReq()
            {
                pokemonId = pokemon.getID(),
                level = pokemon.getLevel(),
                sex = pokemon.getGender() == Pokemon.Gender.MALE ? 0 : 1,
                chp = pokemon.getCurrentHP(),
                skillIdsObj = pokemon.getSkillInfos().ToArray()
            }
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            PokemonAddResult result = JsonUtility.FromJson<PokemonAddResult>(netPokedexDatas.Text);
            if (result == null || result.data == null || result.code != 200)
            {
                callback(false);
                return;
            }
            PokemonGetData data = result.data;
            AppDetailedPokemon appDetailedPokemon = data.appDetailedPokemon;
            Pokemon pokemon = new Pokemon(data.id, data.pokemonId, data.nickName, Pokemon.Gender.CALCULATE, data.level, 0, null, "",
                appDetailedPokemon.individualHp, appDetailedPokemon.individualAtk, appDetailedPokemon.individualDef, appDetailedPokemon.individualSpA, appDetailedPokemon.individualSpD, appDetailedPokemon.individualSpe,
                appDetailedPokemon.effortHp, appDetailedPokemon.effortAtk, appDetailedPokemon.effortDef, appDetailedPokemon.effortSpA, appDetailedPokemon.effortSpD, appDetailedPokemon.effortSpe,
                "", data.skillIdsObj != null ? data.skillIdsObj.ToList() : new List<SkillInfo>());
            pokemon.setCurrentHp(data.chp);
            PC.addPokemon(pokemon, 0);
            callback(true);
            //更新后台遭遇表
            PokedexManager.Instance.updateAtlas(pokemon.getID(), 2);
        }).Catch(err =>
        {
            Debug.Log("fetchBoxPokemonList请求失败:" + err);
            callback(false);
        });
    }

    //交换精灵位置
    public void swapPokemon(long exchangeHoldId1, long exchangeHoldId2, HttpCallback callback)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = PC_POST_EXCHANGE_POKEMON_URL,
            Headers = headers,
            Params = paramz,
            Method = "POST",
            Body = new PokemonSwapReq
            {
                exchange1 = exchangeHoldId1,
                exchange2 = exchangeHoldId2
            }
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            PokemonRemoveResult result = JsonUtility.FromJson<PokemonRemoveResult>(netPokedexDatas.Text);
            if (result == null || result.code != 200)
            {
                callback(false);
                return;
            }
            callback(true);
        }).Catch(err =>
        {
            Debug.Log("swapPokemon请求失败:" + err);
            callback(false);
        });
    }
    //===========================================================================
    public Pokemon[] getPartyBox()
    {
        return PC.boxes[0];
    }

    public Pokemon[] getPCBox(int index)
    {
        return PC.boxes[index];
    }

    //获取box数量
    public int getBoxLength()
    {
        return PC.boxes.Length;
    }

    //获取一个子盒子精灵数量
    public int getSubBoxPokemonLength(int index)
    {
        return PC.getBoxPokemonLength(index);
    }

    //获取所有电脑子盒子精灵数量
    public int getWarehousePokemonLength()
    {
        int length = 0;
        for (int i = 1; i < getBoxLength(); i++)
        {
            length += getSubBoxPokemonLength(i);
        }
        return length;
    }

    public void packParty()
    {
        PC.pack(0);
    }

    public void swapPokemon(int box1, int pos1, int box2, int pos2, HttpCallback callback)
    {
        Pokemon pokemon1 = getPCBox(box1)[pos1];
        Pokemon pokemon2 = getPCBox(box2)[pos2];
        long pokemon1HoldId = pokemon1 != null ? pokemon1.getHoldId() : -1;
        long pokemon2HoldId = pokemon2 != null ? pokemon2.getHoldId() : -1;
        swapPokemon(pokemon1HoldId, pokemon2HoldId, delegate (bool result)
        {
            if (result)
            {
                PC.swapPokemon(box1, pos1, box2, pos2);
                callback(true);
            }
            else
            {
                callback(false);
            }
        });
    }
}
