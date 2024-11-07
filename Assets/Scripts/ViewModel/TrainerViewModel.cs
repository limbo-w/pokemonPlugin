using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Numerics;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class TrainerViewModel : ViewModelBase
{
    private bool running;
    private string playerID;
    private string playerName;
    private string playerMoney;
    private string score;
    private string pokedex;
    private string adventure;
    private Texture personPicture;
    private string time;
    private string timeHour;
    private string timeColon;
    private string timeMinute;

    public string Time
    {
        get { return this.time; }
        set { this.Set<string>(ref this.time, value, "Time"); }
    }

    public string TimeHour
    {
        get { return this.timeHour; }
        set { this.Set<string>(ref this.timeHour, value, "TimeHour"); }
    }

    public string TimeColon
    {
        get { return this.timeColon; }
        set { this.Set<string>(ref this.timeColon, value, "TimeColon"); }
    }

    public string TimeMinute
    {
        get { return this.timeMinute; }
        set { this.Set<string>(ref this.timeMinute, value, "TimeMinute"); }
    }

    public string PlayerID
    {
        get { return this.playerID; }
        set { this.Set<string>(ref this.playerID, value, "PlayerID"); }
    }

    public string PlayerName
    {
        get { return this.playerName; }
        set { this.Set<string>(ref this.playerName, value, "PlayerName"); }
    }

    public string PlayerMoney
    {
        get { return this.playerMoney; }
        set { this.Set<string>(ref this.playerMoney, value, "PlayerMoney"); }
    }

    public string Score
    {
        get { return this.score; }
        set { this.Set<string>(ref this.score, value, "Score"); }
    }

    public string Pokedex
    {
        get { return this.pokedex; }
        set { this.Set<string>(ref this.pokedex, value, "Pokedex"); }
    }

    public string Adventure
    {
        get { return this.adventure; }
        set { this.Set<string>(ref this.adventure, value, "Adventure"); }
    }

    public Texture PersonPicture
    {
        get { return this.personPicture; }
        set { this.Set<Texture>(ref this.personPicture, value, "PersonPicture"); }
    }

    public bool Running
    {
        get { return this.running; }
        set { this.Set<bool>(ref this.running, value, "Running"); }
    }

    public void OnReturn()
    {
        running = false;
    }

    public void updateData()
    {
        TrainerManager.Instance.updateData(delegate (bool result)
        {
            if (result)
            {
                TrainerInfoData data = TrainerManager.Instance.getTrainerInfoData();
                Score = "积分:" + data.playerScore;
                PlayerMoney = "金币:" + data.playerMoney;
                PlayerID = "ID:" + data.playerID;
                PlayerName = data.playerName;
                Pokedex = "遭遇:" + data.pokeDexMeet + " 捕捉:" + data.pokeDexCatch;
                PersonPicture = Resources.Load<Texture>("PlayerSprites/" + TrainerManager.Instance.getPlayerSpriteName());
            }

        });
    }
}
