using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Trainer : MonoBehaviour
{
    private static string[] defaultTrainerName = new string[]
    {
        "gold",
        "Ace Trainer"
    };

    private static int[] defaultPrizeMoney = new int[]
    {
        100,
        60
    };

    private Pokemon[] party;
    public PokemonGetData[] pokemonGetDatas;
    public string trainerName;
    public int customPrizeMoney = 0;
    public bool isFemale = false;

    public AudioClip battleBGM;
    public int samplesLoopStart;

    public AudioClip victoryBGM;
    public int victorySamplesLoopStart;

    public string[] playerVictoryDialog;
    public string[] playerLossDialog;

    public Trainer(Pokemon[] party)
    {
        this.trainerName = "";
        this.party = party;
    }

    public Pokemon[] GetParty()
    {
        if (party != null)
        {
            return party;
        }
        int dataLength = pokemonGetDatas.Length;
        party = new Pokemon[dataLength];
        for (int i = 0; i < pokemonGetDatas.Length; i++)
        {
            PokemonGetData data = pokemonGetDatas[i];
            AppDetailedPokemon appDetailedPokemon = data.appDetailedPokemon;
            Pokemon pokemon = new Pokemon(data.id, data.pokemonId, data.nickName, Pokemon.Gender.CALCULATE, data.level, 0, new BagItem(), "",
                appDetailedPokemon.individualHp, appDetailedPokemon.individualAtk, appDetailedPokemon.individualDef, appDetailedPokemon.individualSpA, appDetailedPokemon.individualSpD, appDetailedPokemon.individualSpe,
                appDetailedPokemon.effortHp, appDetailedPokemon.effortAtk, appDetailedPokemon.effortDef, appDetailedPokemon.effortSpA, appDetailedPokemon.effortSpD, appDetailedPokemon.effortSpe,
                "", data.skillIdsObj != null ? data.skillIdsObj.ToList() : new List<SkillInfo>());
            party[i] = pokemon;
        }
        return party;
    }

    public string GetName()
    {
        return (!string.IsNullOrEmpty(trainerName)) ? trainerName : defaultTrainerName[0];
    }

    public Sprite[] GetSprites()
    {
        Sprite[] sprites = new Sprite[0];
        if (isFemale)
        {
            sprites = Resources.LoadAll<Sprite>("NPCSprites/" + GetName() + "_f");
        }
        if (!isFemale || (isFemale && sprites.Length < 1))
        {
            sprites = Resources.LoadAll<Sprite>("NPCSprites/" + GetName());
        }
        if (sprites.Length == 0)
        {
            sprites = new Sprite[] { Resources.Load<Sprite>("null") };
        }
        return sprites;
    }

    public int GetPrizeMoney()
    {
        int prizeMoney = (customPrizeMoney > 0) ? customPrizeMoney : defaultPrizeMoney[0];
        int averageLevel = 0;
        for (int i = 0; i < party.Length; i++)
        {
            averageLevel += party[i].getLevel();
        }
        averageLevel = Mathf.CeilToInt((float)averageLevel / (float)party.Length);
        return averageLevel * prizeMoney;
    }
}