using System;
using System.Collections.Generic;

//战斗结算请求
[Serializable]
public class FightSettlementRequest
{
    public int gold;
}

//用户信息返回值
[Serializable]
public class TrainerInfoResult
{
    public int code;
    public string msg;
    public TrainerInfoData data;
}


//用户信息数据Data
[Serializable]
public class TrainerInfoData
{
    //用户id
    public long playerID;
    public string playerName;
    public int playerMoney;
    public int playerDiamonds;
    public int pokeDexMeet;
    public int pokeDexCatch;
    public string playerTime;
    public int playerHours;
    public int playerMinutes;
    public int playerSeconds;
    public int playerScore;
    public string fileCreationDate;
    public bool male;
    //TaskData信息
    public TrainerTaskData plot;
    private bool isFirstGetTaskData = true;

    public Dictionary<string, int> getTaskMap(int index)
    {
        if (plot != null && isFirstGetTaskData)
        {
            if (plot.task1 != null)
            {
                GlobalPreference.Instance.setPrefPlayerTaskConfig(0, plot.task1);
            }
            if (plot.task2 != null)
            {
                GlobalPreference.Instance.setPrefPlayerTaskConfig(1, plot.task2);
            }
            if (plot.task3 != null)
            {
                GlobalPreference.Instance.setPrefPlayerTaskConfig(2, plot.task3);
            }
            if (plot.task4 != null)
            {
                GlobalPreference.Instance.setPrefPlayerTaskConfig(3, plot.task4);
            }
            if (plot.task5 != null)
            {
                GlobalPreference.Instance.setPrefPlayerTaskConfig(4, plot.task5);
            }
            if (plot.task6 != null)
            {
                GlobalPreference.Instance.setPrefPlayerTaskConfig(5, plot.task6);
            }
            if (plot.task7 != null)
            {
                GlobalPreference.Instance.setPrefPlayerTaskConfig(6, plot.task7);
            }
            if (plot.task8 != null)
            {
                GlobalPreference.Instance.setPrefPlayerTaskConfig(7, plot.task8);
            }
            if (plot.task9 != null)
            {
                GlobalPreference.Instance.setPrefPlayerTaskConfig(8, plot.task9);
            }
            isFirstGetTaskData = false;
        }
        return GlobalPreference.Instance.getPrefPlayerTaskConfig(index);
    }

    public void setTaskMap(int index, Dictionary<string, int> config)
    {
        GlobalPreference.Instance.setPrefPlayerTaskConfig(index, config);
    }

    //理论上记录的TaskMap数
    public int getTaskMapNumber()
    {
        return 9;
    }
}

//任务返回结果
[Serializable]
public class TrainerTaskData
{
    public long aUpId;
    public long aRId;
    public Dictionary<string, int> task1;
    public Dictionary<string, int> task2;
    public Dictionary<string, int> task3;
    public Dictionary<string, int> task4;
    public Dictionary<string, int> task5;
    public Dictionary<string, int> task6;
    public Dictionary<string, int> task7;
    public Dictionary<string, int> task8;
    public Dictionary<string, int> task9;

    public void setTaskValue(int i, Dictionary<string, int> task)
    {
        if (i == 8)
        {
            task9 = task;
        }
        else if (i == 7)
        {
            task8 = task;
        }
        else if (i == 6)
        {
            task7 = task;
        }
        else if (i == 5)
        {
            task6 = task;
        }
        else if (i == 4)
        {
            task5 = task;
        }
        else if (i == 3)
        {
            task4 = task;
        }
        else if (i == 2)
        {
            task3 = task;
        }
        else if (i == 1)
        {
            task2 = task;
        }
        else
        {
            task1 = task;
        }
    }
}
