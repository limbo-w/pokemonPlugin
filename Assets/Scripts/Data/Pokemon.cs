using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    public enum Status
    {
        NONE,
        BURNED, //燃烧的
        FROZEN,  //冻僵的
        PARALYZED, //瘫痪的
        POISONED, //中毒的
        ASLEEP,  //睡觉的
        FAINTED
    }

    public enum Gender
    {
        NONE,
        MALE,
        FEMALE,
        CALCULATE
    }


    private long holdId;
    //图鉴ID
    private long pokemonID;
    //昵称
    private string nickname;
    private Gender gender;
    private int level;
    private int exp;
    private int nextLevelExp;
    //友好度
    private int friendship;
    //状态
    private Status status;
    //是否是睡觉
    private int sleepTurns;

    private long caughtBallId;
    private BagItem heldItem;

    //if OT = null, pokemon may be caught.
    private string OT;
    private long IDno;
    //精灵个体值
    private int IV_HP;
    private int IV_ATK;
    private int IV_DEF;
    private int IV_SPA;
    private int IV_SPD;
    private int IV_SPE;
    //精灵努力值默认(0, 0, 0, 0, 0, 0)
    private int EV_HP;
    private int EV_ATK;
    private int EV_DEF;
    private int EV_SPA;
    private int EV_SPD;
    private int EV_SPE;

    private string nature;

    private int HP;
    //当前血条值
    private int currentHP;
    private int ATK;
    private int DEF;
    private int SPA;
    private int SPD;
    private int SPE;
    //历史技能表
    private long[] moveHistoryIds;
    //持有技能信息
    private List<SkillInfo> skillInfos = new List<SkillInfo>();

    //todo 异色

    public Pokemon(long holdId, long id, string nickname, Gender gender, int level,
        long caughtBallId, BagItem heldItem, string OT,
        int IV_HP, int IV_ATK, int IV_DEF, int IV_SPA, int IV_SPD, int IV_SPE,
         int EV_HP, int EV_ATK, int EV_DEF, int EV_SPA, int EV_SPD, int EV_SPE,
        string nature, List<SkillInfo> skillInfos)
    {
        this.holdId = holdId;
        PokemonData thisPokemonData = PokemonDatabase.getPokemon(id);
        this.pokemonID = id;
        this.nickname = nickname;
        this.gender = gender;
        //if gender is CALCULATE, then calculate gender using maleRatio
        if (gender == Gender.CALCULATE)
        {
            if (thisPokemonData.getMaleRatio() < 0)
            {
                this.gender = Gender.NONE;
            }
            else if (Random.Range(0f, 100f) <= thisPokemonData.getMaleRatio())
            {
                this.gender = Gender.MALE;
            }
            else
            {
                this.gender = Gender.FEMALE;
            }
        }
        this.level = level;
        //Find exp for current level, and next level.
        this.exp = PokemonDatabase.getLevelExp(thisPokemonData.getLevelingRate(), level);
        this.nextLevelExp = PokemonDatabase.getLevelExp(thisPokemonData.getLevelingRate(), level + 1);
        this.friendship = thisPokemonData.getBaseFriendship();

        this.status = Status.NONE;
        this.sleepTurns = 0;
        this.caughtBallId = caughtBallId;
        this.heldItem = heldItem;

        TrainerInfoData data = TrainerManager.Instance.getTrainerInfoData();
        this.OT = (string.IsNullOrEmpty(OT)) ? data.playerName : OT;
        if (this.OT != data.playerName)
        {
            this.IDno = Random.Range(0, 65536); //if owned by another trainer, assign a random number. 
        }
        else
        {
            this.IDno = data.playerID;
        }

        //Set IVs 
        this.IV_HP = IV_HP;
        this.IV_ATK = IV_ATK;
        this.IV_DEF = IV_DEF;
        this.IV_SPA = IV_SPA;
        this.IV_SPD = IV_SPD;
        this.IV_SPE = IV_SPE;
        //set EVs
        this.EV_HP = EV_HP;
        this.EV_ATK = EV_ATK;
        this.EV_DEF = EV_DEF;
        this.EV_SPA = EV_SPA;
        this.EV_SPD = EV_SPD;
        this.EV_SPE = EV_SPE;
        //set nature
        this.nature = nature;
        this.calculateStats();
        //set currentHP to HP, as a new pokemon will be undamaged.
        this.currentHP = HP;
        this.skillInfos = skillInfos;

        this.moveHistoryIds = PokemonDatabase.getPokemon(pokemonID).GenerateMoveset(this.level);
    }


    public Pokemon(long holdId, int pokemonID, Gender gender, int level, long caughtBallId, BagItem heldItem, string OT)
    {
        this.holdId = holdId;
        PokemonData thisPokemonData = PokemonDatabase.getPokemon(pokemonID);
        this.pokemonID = pokemonID;
        this.gender = gender;
        //if gender is CALCULATE, then calculate gender using maleRatio
        if (gender == Gender.CALCULATE)
        {
            if (thisPokemonData.getMaleRatio() < 0)
            {
                this.gender = Gender.NONE;
            }
            else if (Random.Range(0f, 100f) <= thisPokemonData.getMaleRatio())
            {
                this.gender = Gender.MALE;
            }
            else
            {
                this.gender = Gender.FEMALE;
            }
        }

        this.level = level;
        //Find exp for current level, and next level.
        this.exp = PokemonDatabase.getLevelExp(thisPokemonData.getLevelingRate(), level);
        this.nextLevelExp = PokemonDatabase.getLevelExp(thisPokemonData.getLevelingRate(), level + 1);

        this.friendship = thisPokemonData.getBaseFriendship();

        this.status = Status.NONE;
        this.sleepTurns = 0;

        this.caughtBallId = caughtBallId;
        this.heldItem = heldItem;

        TrainerInfoData data = TrainerManager.Instance.getTrainerInfoData();
        this.OT = (string.IsNullOrEmpty(OT)) ? data.playerName : OT;
        if (this.OT != data.playerName)
        {
            this.IDno = Random.Range(0, 65536); //if owned by another trainer, assign a random number. 
        } //this way if they trade it to you, it will have a different number to the player's.
        else
        {
            this.IDno = data.playerID;
        }

        //Set IVs randomly between 0 and 32 (32 is exlcuded)
        this.IV_HP = Random.Range(0, 32);
        this.IV_ATK = Random.Range(0, 32);
        this.IV_DEF = Random.Range(0, 32);
        this.IV_SPA = Random.Range(0, 32);
        this.IV_SPD = Random.Range(0, 32);
        this.IV_SPE = Random.Range(0, 32);

        //unless specified with a full new Pokemon constructor, set EVs to 0.
        this.EV_HP = 0;
        this.EV_ATK = 0;
        this.EV_DEF = 0;
        this.EV_SPA = 0;
        this.EV_SPD = 0;
        this.EV_SPE = 0;

        //Randomize nature
        this.nature = NatureDatabase.getRandomNature().getName();

        //calculate stats
        this.calculateStats();
        //set currentHP to HP, as a new pokemon will be undamaged.
        this.currentHP = HP;

        //Set moveset based off of the highest level moves possible.
        long[] movesetIds = thisPokemonData.GenerateMoveset(this.level);
        this.skillInfos = new List<SkillInfo>();

        //set maxPP and PP to be the regular PP defined by the move in the database.
        for (int i = 0; i < 4; i++)
        {
            if (movesetIds[i] > 0)
            {
                skillInfos.Add(getMoveSkillInfo(movesetIds[i]));
            }
        }
    }

    //用于捕捉到精灵生成
    public Pokemon(Pokemon pokemon, string nickname, long caughtBallId)
    {
        this.holdId = pokemon.holdId;
        this.pokemonID = pokemon.pokemonID;
        this.nickname = nickname;
        this.gender = pokemon.gender;

        this.level = pokemon.level;
        //Find exp for current level, and next level.
        this.exp = pokemon.exp;
        this.nextLevelExp = pokemon.nextLevelExp;
        this.friendship = pokemon.friendship;

        this.status = pokemon.status;
        this.sleepTurns = pokemon.sleepTurns;
        this.caughtBallId = caughtBallId;
        this.heldItem = pokemon.heldItem;

        TrainerInfoData data = TrainerManager.Instance.getTrainerInfoData();
        this.OT = data.playerName;
        this.IDno = data.playerID;

        //Set IVs 
        this.IV_HP = pokemon.IV_HP;
        this.IV_ATK = pokemon.IV_ATK;
        this.IV_DEF = pokemon.IV_DEF;
        this.IV_SPA = pokemon.IV_SPA;
        this.IV_SPD = pokemon.IV_SPD;
        this.IV_SPE = pokemon.IV_SPE;
        //set EVs
        this.EV_HP = pokemon.EV_HP;
        this.EV_ATK = pokemon.EV_ATK;
        this.EV_DEF = pokemon.EV_DEF;
        this.EV_SPA = pokemon.EV_SPA;
        this.EV_SPD = pokemon.EV_SPD;
        this.EV_SPE = pokemon.EV_SPE;
        //set nature
        this.nature = pokemon.nature;
        //calculate stats
        this.calculateStats();
        this.currentHP = pokemon.currentHP;

        //Set moveset based off of the highest level moves possible.
        if (pokemon.skillInfos != null)
        {
            this.skillInfos = pokemon.skillInfos;
        }
    }


    //重新计算精灵的战斗数值
    private void calculateStats()
    {
        int[] baseStats = PokemonDatabase.getPokemon(pokemonID).getBaseStats();
        if (baseStats[0] == 1)
        {
            this.HP = 1;
        }
        else
        {
            int prevMaxHP = this.HP;
            this.HP = Mathf.FloorToInt(((IV_HP + (2 * baseStats[0]) + (EV_HP / 4) + 100) * level) / 100 + 10);
            this.currentHP = (this.currentHP + (this.HP - prevMaxHP) < this.HP)
                ? this.currentHP + (this.HP - prevMaxHP)
                : this.HP;
        }
        if (baseStats[1] == 1)
        {
            this.ATK = 1;
        }
        else
        {
            this.ATK =
                Mathf.FloorToInt(Mathf.FloorToInt(((IV_ATK + (2 * baseStats[1]) + (EV_ATK / 4)) * level) / 100 + 5) *
                                 NatureDatabase.getNature(nature).getATK());
        }
        if (baseStats[2] == 1)
        {
            this.DEF = 1;
        }
        else
        {
            this.DEF =
                Mathf.FloorToInt(Mathf.FloorToInt(((IV_DEF + (2 * baseStats[2]) + (EV_DEF / 4)) * level) / 100 + 5) *
                                 NatureDatabase.getNature(nature).getDEF());
        }
        if (baseStats[3] == 1)
        {
            this.SPA = 1;
        }
        else
        {
            this.SPA =
                Mathf.FloorToInt(Mathf.FloorToInt(((IV_SPA + (2 * baseStats[3]) + (EV_SPA / 4)) * level) / 100 + 5) *
                                 NatureDatabase.getNature(nature).getSPA());
        }
        if (baseStats[4] == 1)
        {
            this.SPD = 1;
        }
        else
        {
            this.SPD =
                Mathf.FloorToInt(Mathf.FloorToInt(((IV_SPD + (2 * baseStats[4]) + (EV_SPD / 4)) * level) / 100 + 5) *
                                 NatureDatabase.getNature(nature).getSPD());
        }
        if (baseStats[5] == 1)
        {
            this.SPE = 1;
        }
        else
        {
            this.SPE =
                Mathf.FloorToInt(Mathf.FloorToInt(((IV_SPE + (2 * baseStats[5]) + (EV_SPE / 4)) * level) / 100 + 5) *
                                 NatureDatabase.getNature(nature).getSPE());
        }
    }

    //设置昵称
    public void setNickname(string nickname)
    {
        this.nickname = nickname;
    }

    //交换物品
    public BagItem swapHeldItem(BagItem newItem)
    {
        BagItem oldItem = this.heldItem;
        this.heldItem = newItem;
        return oldItem;
    }

    //增加特殊能力值
    public bool addEVs(string stat, float amount)
    {
        int intAmount = Mathf.FloorToInt(amount);
        int evTotal = EV_HP + EV_ATK + EV_DEF + EV_SPA + EV_SPD + EV_SPE;
        if (evTotal < 510)
        {
            //if total EV cap is already reached.
            if (evTotal + intAmount > 510)
            {
                //if this addition will pass the total EV cap.
                intAmount = 510 - evTotal; //set intAmount to be the remaining points before cap is reached.
            }
            if (stat == "HP")
            {
                //if adding to HP.
                if (EV_HP < 252)
                {
                    //if HP is not full.
                    EV_HP += intAmount;
                    if (EV_HP > 252)
                    {
                        //if single stat EV cap is passed.
                        EV_HP = 252;
                    } //set stat back to the cap.
                    return true;
                }
            }
            else if (stat == "ATK")
            {
                //if adding to ATK.
                if (EV_ATK < 252)
                {
                    //if ATK is not full.
                    EV_ATK += intAmount;
                    if (EV_ATK > 252)
                    {
                        //if single stat EV cap is passed.
                        EV_ATK = 252;
                    } //set stat back to the cap.
                    return true;
                }
            }
            else if (stat == "DEF")
            {
                //if adding to DEF.
                if (EV_DEF < 252)
                {
                    //if DEF is not full.
                    EV_DEF += intAmount;
                    if (EV_DEF > 252)
                    {
                        //if single stat EV cap is passed.
                        EV_DEF = 252;
                    } //set stat back to the cap.
                    return true;
                }
            }
            else if (stat == "SPA")
            {
                //if adding to SPA.
                if (EV_SPA < 252)
                {
                    //if SPA is not full.
                    EV_SPA += intAmount;
                    if (EV_SPA > 252)
                    {
                        //if single stat EV cap is passed.
                        EV_SPA = 252;
                    } //set stat back to the cap.
                    return true;
                }
            }
            else if (stat == "SPD")
            {
                //if adding to SPD.
                if (EV_SPD < 252)
                {
                    //if SPD is not full.
                    EV_SPD += intAmount;
                    if (EV_SPD > 252)
                    {
                        //if single stat EV cap is passed.
                        EV_SPD = 252;
                    } //set stat back to the cap.
                    return true;
                }
            }
            else if (stat == "SPE")
            {
                //if adding to SPE.
                if (EV_SPE < 252)
                {
                    //if SPE is not full.
                    EV_SPE += intAmount;
                    if (EV_SPE > 252)
                    {
                        //if single stat EV cap is passed.
                        EV_SPE = 252;
                    } //set stat back to the cap.
                    return true;
                }
            }
        }
        return false; //returns false if total or relevant EV cap was reached before running.
    }

    //===================================基础相关========================================
    public long getHoldId()
    {
        return holdId;
    }


    public long getID()
    {
        return pokemonID;
    }

    public string getName()
    {
        if (string.IsNullOrEmpty(nickname))
        {
            return PokemonDatabase.getPokemon(pokemonID).getName();
        }
        else
        {
            return nickname;
        }
    }

    public Gender getGender()
    {
        return gender;
    }

    public int getLevel()
    {
        return level;
    }

    public int getExp()
    {
        return exp;
    }

    public int getExpNext()
    {
        return nextLevelExp;
    }

    public int getFriendship()
    {
        return friendship;
    }

    public Status getStatus()
    {
        return status;
    }

    public long getCaughtBallId()
    {
        return caughtBallId;
    }

    public BagItem getHeldItem()
    {
        return heldItem;
    }

    public string getOT()
    {
        return OT;
    }

    public long getIDno()
    {
        return IDno;
    }

    public int getIV_HP()
    {
        return IV_HP;
    }

    public int getIV_ATK()
    {
        return IV_ATK;
    }

    public int getIV_DEF()
    {
        return IV_DEF;
    }

    public int getIV_SPA()
    {
        return IV_SPA;
    }

    public int getIV_SPD()
    {
        return IV_SPD;
    }

    public int getIV_SPE()
    {
        return IV_SPE;
    }

    public string getNature()
    {
        return nature;
    }

    public int getHP()
    {
        return HP;
    }

    public int getCurrentHP()
    {
        return currentHP;
    }

    public void setCurrentHp(int currentHp)
    {
        this.currentHP = currentHp;
    }

    public float getPercentHP()
    {
        return 1f - (((float)HP - (float)currentHP) / (float)HP);
    }

    public int getATK()
    {
        return ATK;
    }

    public int getDEF()
    {
        return DEF;
    }

    public int getSPA()
    {
        return SPA;
    }

    public int getSPD()
    {
        return SPD;
    }

    public int getSPE()
    {
        return SPE;
    }

    //=================================状态相关==========================================

    //全治疗精灵
    public void healFull()
    {
        currentHP = HP;
        foreach (SkillInfo info in skillInfos)
        {
            info.curPp = info.maxPp;
        }
        status = Status.NONE;
    }

    ///回复血量
    public int healHP(float amount)
    {
        int excess = 0;
        int intAmount = Mathf.RoundToInt(amount);
        currentHP += intAmount;
        if (currentHP > HP)
        {
            excess = currentHP - HP;
            currentHP = HP;
        }
        if (status == Status.FAINTED && currentHP > 0)
        {
            status = Status.NONE;
        }
        return intAmount - excess;
    }

    //扣除血量
    public void removeHP(float amount)
    {
        int intAmount = Mathf.RoundToInt(amount);
        this.currentHP -= intAmount;
        if (this.currentHP <= 0)
        {
            this.currentHP = 0;
            this.status = Status.FAINTED;
        }
    }

    //治疗状态
    public void healStatus()
    {
        status = Status.NONE;
    }

    //设置状态
    public bool setStatus(Status status)
    {
        if (this.status == Status.NONE)
        {
            this.status = status;
            if (status == Status.ASLEEP)
            {
                sleepTurns = Random.Range(1, 4);
            }
            return true;
        }
        else
        {
            if (status == Status.NONE || status == Status.FAINTED)
            {
                this.status = status;
                sleepTurns = 0;
                return true;
            }
        }
        return false;
    }

    //处理睡觉回合
    public void removeSleepTurn()
    {
        if (status == Status.ASLEEP)
        {
            sleepTurns -= 1;
            if (sleepTurns <= 0)
            {
                setStatus(Status.NONE);
            }
        }
    }

    public int getSleepTurns()
    {
        return sleepTurns;
    }

    //===================================PP相关========================================

    //回复PP值
    public int healPP(int move, float amount)
    {
        int excess = 0;
        int intAmount = Mathf.RoundToInt(amount);
        skillInfos[move].curPp += intAmount;
        if (skillInfos[move].curPp > skillInfos[move].maxPp)
        {
            excess = skillInfos[move].curPp - skillInfos[move].maxPp;
            skillInfos[move].curPp = skillInfos[move].maxPp;
        }
        return intAmount - excess;
    }

    //扣除PP
    public void removePP(long moveId, float amount)
    {
        int move = getMoveIndex(moveId);
        if (move >= 0)
        {
            int intAmount = Mathf.RoundToInt(amount);
            skillInfos[move].curPp -= intAmount;
            if (skillInfos[move].curPp < 0)
            {
                skillInfos[move].curPp = 0;
            }
        }
    }

    //=================================技能相关==========================================
    //获取四个技能信息
    public List<SkillInfo> getSkillInfos()
    {
        return skillInfos;
    }

    //构造SkillInfo
    private SkillInfo getMoveSkillInfo(long newMoveId)
    {
        if (newMoveId > 0)
        {
            return new SkillInfo
            {
                maxPp = MoveDatabase.getMove(newMoveId).getPP(),
                pp = MoveDatabase.getMove(newMoveId).getPP(),
                curPp = MoveDatabase.getMove(newMoveId).getPP(),
                skillId = newMoveId
            };
        }
        return null;
    }

    //交换技能
    public void swapMoves(int target1, int target2)
    {
        if (target1 < skillInfos.Count && target2 < skillInfos.Count)
        {
            SkillInfo temp = skillInfos[target1];
            skillInfos[target1] = skillInfos[target2];
            skillInfos[target2] = temp;
        }
    }

    //增加技能
    public bool addMove(long newMoveId)
    {
        if (!HasMove(newMoveId) && skillInfos.Count < 4 && newMoveId > 0)
        {
            skillInfos.Add(getMoveSkillInfo(newMoveId));
            return true;
        }
        return false;
    }

    //替换技能
    public void replaceMove(int index, long newMoveId)
    {
        if (index >= 0 && index < skillInfos.Count && newMoveId > 0)
        {
            skillInfos[index] = getMoveSkillInfo(newMoveId);
            addMoveToHistory(newMoveId);
        }
    }

    //遗忘技能
    public bool forgetMove(int index)
    {
        if (getMoveCount() > 1 && index < skillInfos.Count)
        {
            skillInfos.RemoveAt(index);
            return true;
        }
        return false;
    }

    //获取学会的技能数
    public int getMoveCount()
    {
        return skillInfos.Count;
    }


    //获取技能索引号
    private int getMoveIndex(long moveId)
    {
        for (int i = 0; i < skillInfos.Count; i++)
        {
            if (skillInfos[i].skillId > 0)
            {
                if (skillInfos[i].skillId == moveId)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private void addMoveToHistory(long moveId)
    {
        if (!HasMoveInHistory(moveId))
        {
            long[] newHistory = new long[moveHistoryIds.Length + 1];
            for (int i = 0; i < moveHistoryIds.Length; i++)
            {
                newHistory[i] = moveHistoryIds[i];
            }
            newHistory[moveHistoryIds.Length] = moveId;
            moveHistoryIds = newHistory;
        }
    }

    public bool HasMove(long moveId)
    {
        if (moveId == 0)
        {
            return false;
        }
        for (int i = 0; i < skillInfos.Count; i++)
        {
            if (skillInfos[i].skillId == moveId)
            {
                return true;
            }
        }
        return false;
    }

    //是否学习过这个技能
    public bool HasMoveInHistory(long moveId)
    {
        for (int i = 0; i < moveHistoryIds.Length; i++)
        {
            if (moveHistoryIds[i] == moveId)
            {
                return true;
            }
        }
        return false;
    }

    //是否可以学习改技能
    public bool CanLearnMove(long moveId)
    {
        PokemonData thisPokemonData = PokemonDatabase.getPokemon(pokemonID);
        long[] moves = thisPokemonData.getMovesetMoves();
        for (int i = 0; i < moves.Length; i++)
        {
            if (moves[i] == moveId)
            {
                return true;
            }
        }
        moves = thisPokemonData.getTmList();
        for (int i = 0; i < moves.Length; i++)
        {
            if (moves[i] == moveId)
            {
                return true;
            }
        }
        return false;
    }

    //获取对应等级的技能id
    public long MoveLearnedAtLevel(int level)
    {
        PokemonData thisPokemonData = PokemonDatabase.getPokemon(pokemonID);
        int[] movesetLevels = thisPokemonData.getMovesetLevels();
        for (int i = 0; i < movesetLevels.Length; i++)
        {
            if (movesetLevels[i] == level)
            {
                return thisPokemonData.getMovesetMoves()[i];
            }
        }
        return 0;
    }

    //=================================进化相关==========================================

    //增加经验
    public void addExp(int expAdded)
    {
        if (level < 100)
        {
            this.exp += expAdded;
            while (exp >= nextLevelExp)
            {
                this.level += 1;
                this.nextLevelExp = PokemonDatabase.getLevelExp(
                    PokemonDatabase.getPokemon(pokemonID).getLevelingRate(), level + 1);
                calculateStats();
                if (this.HP > 0 && this.status == Status.FAINTED)
                {
                    this.status = Status.NONE;
                }
            }
        }
    }

    //获取可进化的精灵
    public int getEvolutionID(string currentMethod)
    {
        PokemonData thisPokemonData = PokemonDatabase.getPokemon(pokemonID);
        int[] evolutions = thisPokemonData.getEvolutions();
        string[] evolutionRequirements = thisPokemonData.getEvolutionRequirements();
        for (int i = 0; i < evolutions.Length; i++)
        {
            if (checkEvolutionMethods(currentMethod, evolutionRequirements[i]))
            {
                return evolutions[i];
            }
        }
        return -1;
    }

    //检查是否可以进化
    public bool canEvolve(string currentMethod)
    {
        PokemonData thisPokemonData = PokemonDatabase.getPokemon(pokemonID);
        int[] evolutions = thisPokemonData.getEvolutions();
        string[] evolutionRequirements = thisPokemonData.getEvolutionRequirements();

        for (int i = 0; i < evolutions.Length; i++)
        {
            if (checkEvolutionMethods(currentMethod, evolutionRequirements[i]))
            {
                return true;
            }
        }
        return false;
    }

    //检查是否可以进化
    private bool checkEvolutionMethods(string currentMethod, string evolutionRequirements)
    {
        string[] evolutionSplit = evolutionRequirements.Split(',');
        string[] methods = evolutionSplit[0].Split('\\');
        string[] currentMethodSplit = currentMethod.Split(',');
        string[] parameters = new string[] { };
        if (evolutionSplit.Length > 0)
        {
            parameters = evolutionSplit[1].Split('\\');
        }
        for (int i = 0; i < methods.Length; i++)
        {
            if (methods[i] == "Level")
            {
                if (currentMethodSplit[0] != "Level")
                {
                    return false;
                }
                else
                {
                    if (this.level < int.Parse(parameters[i]))
                    {
                        return false;
                    }
                }
            }
            else if (methods[i] == "Stone")
            {
                if (currentMethodSplit[0] != "Stone")
                {
                    return false;
                }
                else
                {
                    if (currentMethodSplit[1] != parameters[i])
                    {
                        return false;
                    }
                }
            }
            else if (methods[i] == "Trade")
            {
                if (currentMethodSplit[0] != "Trade")
                {
                    return false;
                }
            }
            else if (methods[i] == "Friendship")
            {
                if (this.friendship < 220)
                {
                    return false;
                }
            }
            else if (methods[i] == "Item")
            {
                if (this.heldItem.itemName == parameters[i])
                {
                    return false;
                }
            }
            else if (methods[i] == "Gender")
            {
                if (this.gender.ToString() != parameters[i])
                {
                    return false;
                }
            }
            else if (methods[i] == "Move")
            {
                //todo 技能
                //if (!HasMove(parameters[i]))
                //{
                //    return false; 
                //}
            }
            else if (methods[i] == "Map")
            {

            }
            else if (methods[i] == "Time")
            {
                string dayNight = "Day";
                if (System.DateTime.Now.Hour >= 21 || System.DateTime.Now.Hour < 4)
                {
                    dayNight = "Night";
                }
                if (dayNight != parameters[i])
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    //进行进化
    public bool evolve(string currentMethod)
    {
        int[] evolutions = PokemonDatabase.getPokemon(pokemonID).getEvolutions();
        string[] evolutionRequirements = PokemonDatabase.getPokemon(pokemonID).getEvolutionRequirements();
        for (int i = 0; i < evolutions.Length; i++)
        {
            if (checkEvolutionMethods(currentMethod, evolutionRequirements[i]))
            {
                float hpPercent = ((float)this.currentHP / (float)this.HP);
                this.pokemonID = evolutions[i];
                calculateStats();
                i = evolutions.Length;
                currentHP = Mathf.RoundToInt(HP * hpPercent);
                return true;
            }
        }
        return false;
    }

    //==============================其他=============================================

    public Sprite[] GetIcons_()
    {
        return GetIconsFromID_(pokemonID, false);
    }

    public static Sprite[] GetIconsFromID_(long ID, bool isShiny)
    {
        string shiny = (isShiny) ? "s" : "";
        Sprite[] icons = Resources.LoadAll<Sprite>("PokemonIcons/icon" + CommonUtils.convertLongID(ID) + shiny);
        if (icons == null)
        {
            Debug.LogWarning("Shiny Variant NOT Found");
            icons = Resources.LoadAll<Sprite>("PokemonIcons/icon" + CommonUtils.convertLongID(ID));
        }
        return icons;
    }

    public Texture GetIcons()
    {
        return GetIconsFromID(pokemonID, false);
    }

    public static Texture GetIconsFromID(long ID, bool isShiny)
    {
        string shiny = (isShiny) ? "s" : "";
        Texture icons = Resources.Load<Texture>("PokemonIcons/icon" + CommonUtils.convertLongID(ID) + shiny);
        if (icons == null)
        {
            Debug.LogWarning("Shiny Variant NOT Found");
            icons = Resources.Load<Texture>("PokemonIcons/icon" + CommonUtils.convertLongID(ID));
        }
        return icons;
    }

    public Sprite[] GetSprite(bool getLight)
    {
        return GetSpriteFromID(pokemonID, false, getLight);
    }

    public static Sprite[] GetSpriteFromID(long ID, bool isShiny, bool getLight)
    {
        string shiny = (isShiny) ? "s" : "";
        string light = (getLight) ? "Lights/" : "";
        Sprite[] spriteSheet = Resources.LoadAll<Sprite>("OverworldPokemonSprites/" + light + CommonUtils.convertLongID(ID) + shiny);
        if (spriteSheet.Length == 0)
        {
            //No Light found AND/OR No Shiny found, load non-shiny
            if (isShiny)
            {
                if (getLight)
                {
                    Debug.LogWarning("Shiny Light NOT Found (may not be required)");
                }
                else
                {
                    Debug.LogWarning("Shiny Variant NOT Found");
                }
            }
            spriteSheet = Resources.LoadAll<Sprite>("OverworldPokemonSprites/" + light + CommonUtils.convertLongID(ID));
        }
        if (spriteSheet.Length == 0)
        {
            //No Light found OR No Sprite found, return 8 blank sprites
            if (!getLight)
            {
                Debug.LogWarning("Sprite NOT Found");
            }
            else
            {
                Debug.LogWarning("Light NOT Found (may not be required)");
            }
            return new Sprite[8];
        }
        return spriteSheet;
    }

    public List<Texture> GetFrontAnimFromGif()
    {
        return GetAnimFromGif("PokemonSprites", pokemonID, gender, false);
    }

    public List<Texture> GetBackAnimFromGif()
    {
        return GetAnimFromGif("PokemonBackSprites", pokemonID, gender, false);
    }

    public static List<Texture> GetAnimFromGif(string folder, long ID, Gender gender, bool isShiny)
    {
        List<Texture> animation = new List<Texture>();
        string shiny = (isShiny) ? "s" : "";
        animation = CommonUtils.loadTexturesFromGif(folder + "/t" + CommonUtils.convertLongID(ID) + shiny + ".gif");
        if (animation.Count == 0 && isShiny)
        {
            Debug.LogWarning("Shiny Variant NOT Found");
            animation = CommonUtils.loadTexturesFromGif(folder + "/t" + CommonUtils.convertLongID(ID) + ".gif");
        }
        return animation;
    }

    public float GetCryPitch()
    {
        return (status == Pokemon.Status.FAINTED) ? 0.9f : 1f - (0.06f * (1 - getPercentHP()));
    }

    public AudioClip GetCry()
    {
        return GetCryFromID(pokemonID);
    }

    public static AudioClip GetCryFromID(long ID)
    {
        return Resources.Load<AudioClip>("Audio/cry/" + CommonUtils.convertLongID(ID));
    }
}