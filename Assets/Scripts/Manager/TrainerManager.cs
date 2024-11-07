using System;
using System.Collections.Generic;
using System.Threading;
using Proyecto26;
using UnityEngine;

/**
 * 玩家训练师信息单例
 */
public class TrainerManager
{
    //获取用户信息
    private const string POKEDEX_GET_INFO = HttpsUtils.DNS_URL + "/pokemon/app/user/getInfo";
    //更新任务情况
    private const string POKEDEX_PLOT_UPDATE = HttpsUtils.DNS_URL + "/pokemon/system/plot/update";
    //战斗结算
    private const string FIGHT_SETTLEMENT = HttpsUtils.DNS_URL + "/pokemon/app/appFight/settlement";

    private static TrainerManager _instance = null;
    private TrainerManager()
    {
    }

    public static TrainerManager Instance
    {
        get
        {
            return _instance ??= new TrainerManager();
        }
    }

    public string mapName;
    public bool[] gymsEncountered = new bool[12];
    public bool[] gymsBeaten = new bool[12];
    public string levelName;

    private string playerSpriteName = "m_hgss_front";
    private TrainerInfoData trainerInfoData;

    public TrainerInfoData getTrainerInfoData()
    {
        return trainerInfoData;
    }

    public void setPlayerSpriteName(string spriteName)
    {
        this.playerSpriteName = spriteName;
    }

    public string getPlayerSpriteName()
    {
        return playerSpriteName;
    }

    //更新任务记录
    public void setTaskValue(string key, int value)
    {
        for (int i = 0; i < trainerInfoData.getTaskMapNumber(); i++)
        {
            Dictionary<string, int> tasks = trainerInfoData.getTaskMap(i);
            if (tasks.Count < 100)
            {
                if (tasks.ContainsKey(key))
                {
                    tasks[key] = value;
                }
                else
                {
                    tasks.Add(key, value);
                }
                Dictionary<string, string> headers = new Dictionary<string, string>();
                //设置请求头
                HttpsUtils.SetGeneralHeaders(headers);
                Dictionary<string, string> paramz = new Dictionary<string, string>();
                //设置请求参数
                HttpsUtils.SetGeneralParams(paramz);
                TrainerTaskData data = new TrainerTaskData();
                //设置修改的TaskMap参数
                data.setTaskValue(i, tasks);
                RequestHelper request = new RequestHelper
                {
                    Uri = POKEDEX_PLOT_UPDATE,
                    Headers = headers,
                    Params = paramz,
                    Method = "POST",
                    Body = data
                };
                RestClient.Request(request).Then(netPokedexDatas =>
                {
                    TrainerInfoResult result = JsonUtility.FromJson<TrainerInfoResult>(netPokedexDatas.Text);
                    if (result == null || result.code != 200)
                    {
                        CommonUtils.showDialogText("设置TaskValue失败 result" + result);
                        return;
                    }
                    //本地持久化
                    trainerInfoData.setTaskMap(i, tasks);
                }).Catch(err =>
                {
                    CommonUtils.showDialogText("设置TaskValue失败");
                });
            }
        }
    }

    public int getTaskValue(string key)
    {
        if (trainerInfoData == null)
        {
            return -1;
        }
        for (int i = 0; i < trainerInfoData.getTaskMapNumber(); i++)
        {
            Dictionary<string, int> taskMap = trainerInfoData.getTaskMap(i);
            if (taskMap.ContainsKey(key))
            {
                return taskMap[key];
            }
        }
        return -1;
    }

    public delegate void HttpCallback(bool result);

    //获取用户信息
    public void updateData(HttpCallback callback)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        //设置请求头
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        //设置请求参数
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = POKEDEX_GET_INFO,
            Headers = headers,
            Params = paramz,
            Method = "GET",
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            TrainerInfoResult result = JsonUtility.FromJson<TrainerInfoResult>(netPokedexDatas.Text);
            if (result == null || result.data == null)
            {
                callback(false);
                return;
            }
            trainerInfoData = result.data;
            callback(true);
        }).Catch(err =>
        {
            Debug.Log("Pokedex请求失败:" + err);
            callback(false);
        });
    }

    //战斗结算方法
    public void fightSettlement(int goldDif, HttpCallback callback)
    {
        //如果扣的钱不够
        if (trainerInfoData.playerMoney + goldDif < 0)
        {
            goldDif = -trainerInfoData.playerMoney;
        }
        Dictionary<string, string> headers = new Dictionary<string, string>();
        //设置请求头
        HttpsUtils.SetGeneralHeaders(headers);
        Dictionary<string, string> paramz = new Dictionary<string, string>();
        //设置请求参数
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = FIGHT_SETTLEMENT,
            Headers = headers,
            Params = paramz,
            Method = "POST",
            Body = new FightSettlementRequest
            {
                gold = goldDif
            }
        };
        RestClient.Request(request).Then(netPokedexDatas =>
        {
            TrainerInfoResult result = JsonUtility.FromJson<TrainerInfoResult>(netPokedexDatas.Text);
            if (result == null || result.code != 200)
            {
                callback(false);
                CommonUtils.showDialogText("战斗结算失败失败 result" + result);
                return;
            }
            trainerInfoData.playerMoney += goldDif;
            callback(true);
        }).Catch(err =>
        {
            Debug.Log("战斗结算失败失败:" + err);
            callback(false);
        });
    }

    //记录用户配置信息
    public void recordPlayerConfig(PlayerConfig config, HttpCallback callback)
    {
        callback(true);
    }
}
