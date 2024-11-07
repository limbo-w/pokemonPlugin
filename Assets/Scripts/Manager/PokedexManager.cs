using System;
using System.Collections.Generic;
using Proyecto26;
using UnityEngine;

public class PokedexManager
{
    //获取图鉴信息
    private const string POKEDEX_HANDLER_URL = HttpsUtils.DNS_URL + "/pokemon/basic/basicPokemon/getHandbookInfoByPage";
    //更新遭遇的精灵
    private const string POKEDEX_UPDATE_ATLAS_URL = HttpsUtils.DNS_URL + "/pokemon/system/atlas/updateAtlas";
    private Dictionary<long, int> pokedexStatusDic = new Dictionary<long, int>();

    public delegate void HttpCallback(PokedexData[] result);
    private static PokedexManager _instance = null;
    private PokedexManager()
    {

    }

    public static PokedexManager Instance
    {
        get
        {
            return _instance ??= new PokedexManager();
        }
    }

    //更新遭遇的精灵后台接口
    public void updateAtlas(long bPId, int status)
    {
        //过滤些无用遭遇状态
        if (pokedexStatusDic.ContainsKey(bPId) && pokedexStatusDic[bPId] > 0 && status == 1)
        {
            return;
        }
        pokedexStatusDic[bPId] = status;
        //请求图鉴分页信息
        Dictionary<string, string> headers = new Dictionary<string, string>();
        //设置请求头
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        //设置请求参数
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = POKEDEX_UPDATE_ATLAS_URL,
            Headers = headers,
            Params = paramz,
            Method = "GET",
            Body = new UpdateAtlasRequest()
            {
                bPId = "" + bPId,
                status = "" + status
            }
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            UpdateAtlasResult result = JsonUtility.FromJson<UpdateAtlasResult>(netPokedexDatas.Text);
        }).Catch(err =>
        {
            Debug.Log("update atlas请求失败:" + err);
        });
    }

    public void getHandbookInfoByPage(int boxNum, int boxMaxNum, HttpCallback callback)
    {
        //请求图鉴分页信息
        Dictionary<string, string> headers = new Dictionary<string, string>();
        //设置请求头
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        //设置请求参数
        HttpsUtils.SetGeneralParams(paramz);
        paramz.Add("currentPage", "" + (boxNum + 1));
        paramz.Add("pageSize", "" + boxMaxNum);
        RequestHelper request = new RequestHelper
        {
            Uri = POKEDEX_HANDLER_URL,
            Headers = headers,
            Params = paramz,
            Method = "GET",
        };

        RestClient.Request(request).Then(netPokedexDatas =>
        {
            PokedexResult result = JsonUtility.FromJson<PokedexResult>(netPokedexDatas.Text);
            //缓存数据
            if (result == null || result.rows == null || result.rows.Length == 0)
            {
                return;
            }
            for (int i = 0; i < result.rows.Length; i++)
            {
                PokedexData data = result.rows[i];
                pokedexStatusDic[data.id] = data.status;
            }
            callback(result.rows);
        }).Catch(err =>
        {
            Debug.Log("Pokedex请求失败:" + err);
        });
    }
}
