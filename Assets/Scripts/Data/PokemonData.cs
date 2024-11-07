using UnityEngine;
using System.Collections;

public class PokemonData
{
    public enum Type
    {
        NONE,
        NORMAL,
        FIGHTING,
        FLYING,
        POISON,
        GROUND,
        ROCK,
        BUG,
        GHOST,
        STEEL,
        FIRE,
        WATER,
        GRASS,
        ELECTRIC,
        PSYCHIC,
        ICE,
        DRAGON,
        DARK,
        FAIRY
    };

    public enum EggGroup
    {
        NONE,
        MONSTER,
        WATER1,
        BUG,
        FLYING,
        FIELD,
        FAIRY,
        GRASS,
        HUMANLIKE,
        WATER3,
        MINERAL,
        AMORPHOUS,
        WATER2,
        DITTO,
        DRAGON,
        UNDISCOVERED
    };

    public enum LevelingRate
    {
        ERRATIC,
        FAST,
        MEDIUMFAST,
        MEDIUMSLOW,
        SLOW,
        FLUCTUATING
    };

    public enum PokedexColor
    {
        RED,
        BLUE,
        YELLOW,
        GREEN,
        BLACK,
        BROWN,
        PURPLE,
        GRAY,
        WHITE,
        PINK
    };

    private long ID;
    private string name;

    private Type type1;
    private Type type2;

    private float maleRatio; //-1f is interpreted as genderless
    //捕捉概率
    private int catchRate;

    //基础经验值
    private int baseExpYield;
    //升级率
    private LevelingRate levelingRate;
    //基础友情值
    private int baseFriendship;

    private int baseStatsHP;
    private int baseStatsATK;
    private int baseStatsDEF;
    private int baseStatsSPA;
    private int baseStatsSPD;
    private int baseStatsSPE;
    //一共可学习技能列表
    private long[] movesetMoves;
    //升级可学习技能等级
    private int[] movesetLevels;
    //可学习技能物品列表
    private long[] tmList; 

    private int[] evolutionID;
    private string[] evolutionRequirements;


    public PokemonData(long ID, string name, Type type1, Type type2, float maleRatio, int catchRate,
        int baseExpYield, LevelingRate levelingRate, int baseFriendship,
        int baseStatsHP, int baseStatsATK, int baseStatsDEF, int baseStatsSPA, int baseStatsSPD, int baseStatsSPE,
        int[] movesetLevels, long[] movesetMoves, long[] tmList,
        int[] evolutionID, string[] evolutionRequirements)
    {
        this.ID = ID;
        this.name = name;
        this.type1 = type1;
        this.type2 = type2;


        this.maleRatio = maleRatio;
        this.catchRate = catchRate;

        this.baseExpYield = baseExpYield;
        this.levelingRate = levelingRate;

        this.baseFriendship = baseFriendship;

        this.baseStatsHP = baseStatsHP;
        this.baseStatsATK = baseStatsATK;
        this.baseStatsDEF = baseStatsDEF;
        this.baseStatsSPA = baseStatsSPA;
        this.baseStatsSPD = baseStatsSPD;
        this.baseStatsSPE = baseStatsSPE;

        this.movesetLevels = movesetLevels;
        this.movesetMoves = movesetMoves;
        this.tmList = tmList;

        this.evolutionID = evolutionID;
        this.evolutionRequirements = evolutionRequirements;
    }


    public long getID()
    {
        return ID;
    }

    public string getName()
    {
        return name;
    }

    public float getMaleRatio()
    {
        return maleRatio;
    }

    public PokemonData.Type getType1()
    {
        return type1;
    }

    public PokemonData.Type getType2()
    {
        return type2;
    }

    public int getBaseFriendship()
    {
        return baseFriendship;
    }

    public int getBaseExpYield()
    {
        return baseExpYield;
    }

    public int[] getBaseStats()
    {
        return new int[] { baseStatsHP, baseStatsATK, baseStatsDEF, baseStatsSPA, baseStatsSPD, baseStatsSPE };
    }

    public int[] getMovesetLevels()
    {
        return movesetLevels;
    }

    public long[] getMovesetMoves()
    {
        return movesetMoves;
    }

    public long[] getTmList()
    {
        return tmList;
    }

    public int[] getEvolutions()
    {
        return evolutionID;
    }

    public string[] getEvolutionRequirements()
    {
        return evolutionRequirements;
    }

    public PokemonData.LevelingRate getLevelingRate()
    {
        return levelingRate;
    }

    public int getCatchRate()
    {
        return catchRate;
    }

    //自定义生成满足条件的4个技能
    public long[] GenerateMoveset(int level)
    {
        long[] moveset = new long[4];
        int i = movesetLevels.Length - 1;
        int k = 0;
        while (i >= 0 && k < 4)
        {
            if (movesetLevels[i] <= level)
            {
                moveset[k] = movesetMoves[i];
                k++;
            }
            i -= 1;

        }
        
        return moveset;
    }
}