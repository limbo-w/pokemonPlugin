using System.Collections;
using System.Collections.Generic;
using Loxodon.Framework.Commands;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleViewModel : ViewModelBase
{
    //全部对手被打败
    public bool allOpponentsDefeated;
    public bool allPlayersDefeated;
    public bool running = false;
    //是否在对战操作中，false代表可以跳过该回合
    private bool runState = true;
    //是否是Trainer战斗
    private bool trainerBattle;
    //尝试逃跑次数
    private int playerFleeAttempts = 0;

    public BattleViewModelInfo viewModelInfo;
    private BattleViewModelInfo.AudioInfoObject audioInfoObject;
    public BattlePartyViewModel partyViewModel;
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;

    //属性1
    private PokemonData.Type[] pokemonType1 = new PokemonData.Type[]{
    PokemonData.Type.NONE, PokemonData.Type.NONE, PokemonData.Type.NONE,
    PokemonData.Type.NONE, PokemonData.Type.NONE, PokemonData.Type.NONE
    };
    //属性2
    private PokemonData.Type[] pokemonType2 = new PokemonData.Type[]{
    PokemonData.Type.NONE, PokemonData.Type.NONE, PokemonData.Type.NONE,
    PokemonData.Type.NONE, PokemonData.Type.NONE, PokemonData.Type.NONE
    };
    //属性3
    private PokemonData.Type[] pokemonType3 = new PokemonData.Type[]{
    PokemonData.Type.NONE, PokemonData.Type.NONE, PokemonData.Type.NONE,
    PokemonData.Type.NONE, PokemonData.Type.NONE, PokemonData.Type.NONE
    };
    //当前任务状态
    public enum TaskState
    {
        ClearChoice, TaskChoice, MoveChoice, BagChoice,
        PartyChoice, FaintPartyChoice, RunChoice
    }
    //对应的Task
    private TaskState currentTask = TaskState.TaskChoice;
    //指令类型
    public enum CommandType { None, Move, Item, Switch, Flee }
    //对战者名称
    private string opponentName;
    private Pokemon[] pokemon = new Pokemon[6];
    //我方精灵
    private Pokemon[] playerParty = new Pokemon[6];
    //敌方精灵
    private Pokemon[] opponentParty = new Pokemon[6];
    private CommandType[] command = new CommandType[6];
    private int[] commandTarget = new int[6];
    private MoveData[] commandMove = new MoveData[6];
    private ItemData[] commandItem = new ItemData[6];
    private Pokemon[] commandPokemon = new Pokemon[6];
    private string[] hpBarPath = new string[]
    {
        BattleHandler.playerHpBars[0],BattleHandler.playerHpBars[0],
        BattleHandler.playerHpBars[0], BattleHandler.playerHpBars[1],
        BattleHandler.playerHpBars[1],BattleHandler.playerHpBars[1]
    };

    //精灵效果
    private bool[] confused = new bool[6];
    private int[] infatuatedBy = new int[] { -1, -1, -1, -1, -1, -1 };
    private bool[] flinched = new bool[6];
    private int[] statusEffectTurns = new int[6];
    private int[] lockedTurns = new int[6];
    private int[] partTrappedTurns = new int[6];
    private bool[] trapped = new bool[6];
    private bool[] charging = new bool[6];
    private bool[] recharging = new bool[6];
    private bool[] protect = new bool[6];
    //特殊技能附加影响
    private int[] seededBy = new int[] { -1, -1, -1, -1, -1, -1 };
    private bool[] destinyBond = new bool[6];
    private bool[] minimized = new bool[6];
    private bool[] defenseCurled = new bool[6];
    //精灵动画
    private Sprite[] playerTrainer1Animation;
    private Sprite[] trainer1Animation;

    public BattleViewModel(BattleViewModelInfo.NotifyCollectionChangedObject changedObject,
        BattleViewModelInfo.AudioInfoObject audioInfoObject)
    {
        viewModelInfo = new BattleViewModelInfo(changedObject);
        this.onMoveButton = new SimpleCommand<int>(DoMoveButtonClickDown);
        this.audioInfoObject = audioInfoObject;
    }

    //自动释放敌人或者自己的精灵到指定位置
    private bool autoSwitchPokemon(int switchPosition, Pokemon[] party)
    {
        //还没死亡
        if (pokemon[switchPosition] != null
            && pokemon[switchPosition].getStatus() != Pokemon.Status.FAINTED)
        {
            return false;
        }
        //开始替换
        for (int i = 0; i < party.Length; i++)
        {
            if (party[i] != null)
            {
                if (party[i].getStatus() != Pokemon.Status.FAINTED)
                {
                    return switchPokemon(switchPosition, party[i]);
                }
            }
        }
        return false;
    }

    //释放对应的精灵位置
    private bool switchPokemon(int switchPosition, Pokemon newPokemon)
    {
        if (newPokemon == null)
        {
            return false;
        }
        if (newPokemon.getStatus() == Pokemon.Status.FAINTED)
        {
            return false;
        }
        //设置出战的精灵
        pokemon[switchPosition] = newPokemon;

        pokemonType1[switchPosition] = PokemonDatabase.getPokemon(newPokemon.getID()).getType1();
        pokemonType2[switchPosition] = PokemonDatabase.getPokemon(newPokemon.getID()).getType2();
        pokemonType3[switchPosition] = PokemonData.Type.NONE;
        //重新设置效果
        confused[switchPosition] = false;
        infatuatedBy[switchPosition] = -1;
        flinched[switchPosition] = false;
        statusEffectTurns[switchPosition] = 0;
        lockedTurns[switchPosition] = 0;
        partTrappedTurns[switchPosition] = 0;
        trapped[switchPosition] = false;
        charging[switchPosition] = false;
        recharging[switchPosition] = false;
        protect[switchPosition] = false;
        //特殊技能重置
        seededBy[switchPosition] = -1;
        destinyBond[switchPosition] = false;
        minimized[switchPosition] = false;
        defenseCurled[switchPosition] = false;
        return true;
    }

    //显示交换精灵的动画
    private IEnumerator showSwitchPokemonAnim(string text, int pos, bool slideStats)
    {
        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(text, 0.75f));
        float targetHpSize = Mathf.CeilToInt(pokemon[pos].getPercentHP() * 76f);
        float stretchDelayTime = 1f * pokemon[pos].getPercentHP();
        StaticCoroutine.StartCoroutine(viewModelInfo.setStretchBar(hpBarPath[pos], targetHpSize, stretchDelayTime));
        if (pos == 0)
        {
            yield return viewModelInfo.setBattleBase(pos, pokemon[pos].GetBackAnimFromGif(), 0.25f);
            StaticCoroutine.StartCoroutine(viewModelInfo.releasePokemon(pos, playerTrainer1Animation));
            yield return StaticCoroutine.StartCoroutine(viewModelInfo.updatePokemonStatsDisplay(pos, pokemon[pos]));
            if (slideStats)
            {
                viewModelInfo.slidePokemonStatsDisplay(pos, true, playerParty, true);
            }
            yield return new WaitForSeconds(1f);
            yield return new WaitForSeconds(PlayCry(pokemon[pos]));
            yield return StaticCoroutine.StartCoroutine(setExp(pokemon[pos]));
        }
        else
        {
            //更新遭遇精灵的接口
            PokedexManager.Instance.updateAtlas(pokemon[pos].getID(), 1);
            if (trainerBattle)
            {
                yield return viewModelInfo.setBattleBase(pos, pokemon[pos].GetFrontAnimFromGif(), 0.25f);
                StaticCoroutine.StartCoroutine(viewModelInfo.releasePokemon(pos, trainer1Animation));
            }
            yield return StaticCoroutine.StartCoroutine(viewModelInfo.updatePokemonStatsDisplay(pos, pokemon[pos]));
            if (slideStats)
            {
                viewModelInfo.slidePokemonStatsDisplay(pos, false, opponentParty, trainerBattle);
            }
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForSeconds(PlayCry(pokemon[pos]));
        }
    }

    //显示消失精灵的动画
    private IEnumerator faintPokemonAnim(int pos)
    {
        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(generatePreString(pos)
                                    + pokemon[pos].getName() + " 濒临死亡!"));
        yield return new WaitForSeconds(PlayCry(pokemon[pos]));
        //死亡刷新界面
        yield return StaticCoroutine.StartCoroutine(viewModelInfo.updatePokemonStatsDisplay(pos, pokemon[pos]));
        yield return StaticCoroutine.StartCoroutine(viewModelInfo.setPokemonFaint(pos));
    }

    public IEnumerator reset(bool isTrainerBattle, Trainer trainer)
    {
        dialog = DialogBoxUIHandler.main;
        running = true;
        playerFleeAttempts = 0;
        trainerBattle = isTrainerBattle;
        yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.ClearChoice));
        //根据位置获取战斗背景
        //MapCollider currentMapCollider = PlayerMovement.player.currentMapCollider;
        //if (currentMapCollider != null)
        //{
        //    viewModelInfo.Background = currentMapCollider.getBattleBack();
        //    viewModelInfo.PlayerShadow = currentMapCollider.getBattleBase();
        //    viewModelInfo.OpponentShadow = currentMapCollider.getBattleBase();
        //}
        pokemon = new Pokemon[6];
        opponentParty = trainer.GetParty();
        autoSwitchPokemon(3, opponentParty);
        opponentName = trainer.GetName();
        //加载我方背部
        playerTrainer1Animation = Resources.LoadAll<Sprite>("PlayerSprites/m_hgss_back");
        //设置训练师
        if (trainerBattle)
        {
            trainer1Animation = trainer.GetSprites();
        }
        //释放可用的精灵
        playerParty = PCManager.Instance.getPartyBox();
        //自动切换出战精灵
        autoSwitchPokemon(0, playerParty);
        if (trainerBattle)
        {
            //0代表当前位置，340为右侧
            viewModelInfo.slidePokemon(3, trainerBattle, 0, 340, trainer1Animation);
            viewModelInfo.slidePokemon(0, trainerBattle, 0, -340, playerTrainer1Animation);
            yield return new WaitForSeconds(2.5f);
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(opponentName + "想要战斗!", 0.5f));
            StaticCoroutine.StartCoroutine(viewModelInfo.shrinkPokemon(BattleHandler.playerTypes[1], 0.9f, 0));
            yield return StaticCoroutine.StartCoroutine(showSwitchPokemonAnim(opponentName + "放出" + pokemon[3].getName()
                + "!", 3, true));
        }
        else
        {
            StaticCoroutine.StartCoroutine(viewModelInfo.shrinkPokemon(BattleHandler.playerTypes[1], 0.9f, 0));
            yield return viewModelInfo.setBattleBase(3, pokemon[3].GetFrontAnimFromGif(), 0.25f);
            viewModelInfo.slidePokemon(3, trainerBattle, 0, 340, trainer1Animation);
            viewModelInfo.slidePokemon(0, trainerBattle, 0, -340, playerTrainer1Animation);
            yield return new WaitForSeconds(2.5f);
            yield return StaticCoroutine.StartCoroutine(showSwitchPokemonAnim("野生的" + pokemon[3].getName()
                + "出现了!", 3, true));
        }
        yield return StaticCoroutine.StartCoroutine(showSwitchPokemonAnim("去吧! " + pokemon[0].getName()
            + "!", 0, true));
    }

    public IEnumerator doControl(bool isTrainerBattle, Trainer trainer)
    {
        if (currentTask != TaskState.FaintPartyChoice)
        {
            yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.TaskChoice));
        }
        runState = true;
        while (runState)
        {
            yield return null;
        }
        if (pokemon[3] != null)
        {
            //设置指令精灵
            commandPokemon[3] = pokemon[3];
            command[3] = CommandType.Move;
            if (isCrazyAttack(3))
            {
                commandMove[3] = MoveDatabase.getMove(0);
            }
            else
            {
                //设置对应攻击技能
                List<SkillInfo> skillInfos = pokemon[3].getSkillInfos();
                int AImoveIndex = Random.Range(0, skillInfos.Count);
                while (skillInfos[AImoveIndex].curPp == 0)
                {
                    AImoveIndex = Random.Range(0, skillInfos.Count);
                }
                commandMove[3] = MoveDatabase.getMove(skillInfos[AImoveIndex].skillId);
            }
        }
        ////////////////////////////////////////
        /// 战斗状态
        ////////////////////////////////////////
        int excludePos = -1;
        for (int battleTime = 0; battleTime < 2; battleTime++)
        {
            TrainerInfoData trainerInfoData = TrainerManager.Instance.getTrainerInfoData();
            int movingPokemon = getHighestSpeedIndex(excludePos);    //0或者3
            excludePos = movingPokemon;
            if (pokemon[movingPokemon] != null)
            {
                if (command[movingPokemon] == CommandType.Flee)  //如果指令是逃走
                {
                    //如果是己方的操作
                    playerFleeAttempts += 1;
                    int fleeChance = (pokemon[movingPokemon].getSPE() * 128) / pokemon[3].getSPE() + 30 * playerFleeAttempts;
                    if (Random.Range(0, 256) < fleeChance)
                    {
                        SfxHandler.Play(audioInfoObject.runClip);
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("安全离开!"));
                        updatePartPokemons();
                        running = false;
                        battleTime = 2;
                        continue;
                    }
                    else
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("无法逃跑!"));
                    }
                }
                else if (command[movingPokemon] == CommandType.Item)
                {
                    if (movingPokemon < 3)
                    {
                        //使用精灵球
                        if (commandItem[movingPokemon].getItemEffect() == ItemData.ItemEffect.BALL)
                        {
                            yield return new WaitForSeconds(2f);
                            int targetIndex = 3;
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(trainerInfoData.playerName
                                + "使用" + commandItem[movingPokemon].getName() + "!", 0.75f));
                            if (trainerBattle)
                            {
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("这是训练家的精灵!", 0.75f));
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("怎么能当小偷呢!"));
                            }
                            else
                            {
                                //扔精灵球 catchType控制
                                yield return StaticCoroutine.StartCoroutine(viewModelInfo.throwPokemonBall(movingPokemon,
                                    playerTrainer1Animation, 0, 5f));
                                float ballRate = (float)commandItem[movingPokemon].getFloatParameter();
                                float catchRate = (float)PokemonDatabase.getPokemon(pokemon[targetIndex].getID()).getCatchRate();
                                float statusRate = 1f;
                                if ((pokemon[targetIndex].getStatus() != Pokemon.Status.NONE))
                                {
                                    statusRate = (pokemon[targetIndex].getStatus() == Pokemon.Status.ASLEEP ||
                                              pokemon[targetIndex].getStatus() == Pokemon.Status.FROZEN) ? 2.5f : 1.5f;
                                }

                                int modifiedRate = Mathf.FloorToInt(((3 * (float)pokemon[targetIndex].getHP() - 2 * (float)pokemon[targetIndex].getCurrentHP())
                                              * catchRate * ballRate) / (3 * (float)pokemon[targetIndex].getHP()) * statusRate);
                                int shakeProbability = Mathf.FloorToInt(65536f / Mathf.Sqrt(Mathf.Sqrt(255f / modifiedRate)));
                                //精灵球摇动
                                int shakes = 0;
                                for (int shake = 0; shake < 4; shake++)
                                {
                                    int shakeCheck = Random.Range(0, 65535);
                                    if (shakeCheck < shakeProbability)
                                    {
                                        shakes += 1;
                                    }
                                    else
                                    {
                                        shake = 4;
                                    }
                                }
                                if (shakes == 4)
                                {
                                    //精灵球捕获成功
                                    yield return StaticCoroutine.StartCoroutine(viewModelInfo.throwPokemonBall(movingPokemon,
                                        playerTrainer1Animation, 2, 1f));
                                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(generatePreString(targetIndex)
                                        + pokemon[targetIndex].getName() + "被成功捕获!", 0.75f));
                                    //yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(
                                    //    "你想要给" + pokemon[targetIndex].getName() + "起别名吗?"));
                                    //string[] choices = new string[] { "是", "否" };
                                    //yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
                                    //int chosenIndex = dialog.buttonIndex;
                                    //string nickname = null;
                                    //if (chosenIndex == 1)
                                    //{
                                    //    SfxHandler.Play(audioInfoObject.selectClip);
                                    //    yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(false, 0.4f));

                                    //    Scene.main.Typing.gameObject.SetActive(true);
                                    //    StaticCoroutine.StartCoroutine(Scene.main.Typing.control(10, "",
                                    //        pokemon[targetIndex].getGender(), pokemon[targetIndex].GetIcons_()));
                                    //    while (Scene.main.Typing.gameObject.activeSelf)
                                    //    {
                                    //        yield return null;
                                    //    }
                                    //    if (Scene.main.Typing.typedString.Length > 0)
                                    //    {
                                    //        nickname = Scene.main.Typing.typedString;
                                    //    }
                                    //    yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, 0.4f));
                                    //}
                                    PCManager.Instance.addPokemon(new Pokemon(pokemon[targetIndex],
                                        "", commandItem[movingPokemon].getBasicGoodsId()),
                                        delegate (bool result)
                                        {
                                            if (!result)
                                            {
                                                StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay("加入" + pokemon[targetIndex].getName() + "失败", 0.75f));
                                            }
                                        });
                                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("恭喜获得" + pokemon[targetIndex].getName()));
                                    updatePartPokemons();
                                    running = false;
                                    battleTime = 2;
                                    continue;
                                }
                                else
                                {
                                    //精灵球捕获失败
                                    yield return StaticCoroutine.StartCoroutine(viewModelInfo.throwPokemonBall(movingPokemon,
                                        playerTrainer1Animation, 1, 1f));
                                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(
                                        pokemon[targetIndex].getName() + "捕获失败!"));
                                }
                            }
                        }
                    }
                    else
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(opponentName
                            + "使用" + commandItem[movingPokemon].getName() + "!", 0.75f));
                    }
                }
                else if (command[movingPokemon] == CommandType.Move) //选择技能
                {
                    int targetIndex = 3;  //默认是敌人
                    if (commandMove[movingPokemon] != null
                        && (commandMove[movingPokemon].getTarget() == MoveData.Target.SELF
                        || commandMove[movingPokemon].getTarget() == MoveData.Target.ADJACENTALLYSELF))
                    {
                        targetIndex = movingPokemon;
                    }
                    else
                    {
                        if (movingPokemon > 2) //攻击自己
                        {
                            targetIndex = 0;
                        }
                    }
                    if (pokemon[movingPokemon].getStatus() != Pokemon.Status.FAINTED)
                    {
                        float accuracy = 0f;
                        if (commandMove[movingPokemon] != null)
                        {
                            accuracy = commandMove[movingPokemon].getAccuracy()
                                * viewModelInfo.calculateAccuracyModifier(5, movingPokemon)
                                / viewModelInfo.calculateAccuracyModifier(6, targetIndex);
                        }

                        bool canMove = true;  //可以使用技能
                        if (pokemon[movingPokemon].getStatus() == Pokemon.Status.PARALYZED)
                        {
                            if (Random.value > 0.75f)
                            {
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(generatePreString(movingPokemon)
                                    + pokemon[movingPokemon].getName() + "麻痹了! 无法使用技能!", 0.75f));
                                canMove = false;
                            }
                        }
                        else if (pokemon[movingPokemon].getStatus() == Pokemon.Status.FROZEN)
                        {
                            if (Random.value > 0.2f)
                            {
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(generatePreString(movingPokemon)
                                    + pokemon[movingPokemon].getName() + "冻僵了!", 0.75f));
                                canMove = false;
                            }
                            else
                            {
                                pokemon[movingPokemon].setStatus(Pokemon.Status.NONE);
                                yield return StaticCoroutine.StartCoroutine(viewModelInfo.updatePokemonStatsDisplay(movingPokemon, pokemon[movingPokemon]));
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(generatePreString(movingPokemon)
                                    + pokemon[movingPokemon].getName() + "解冻了!", 0.75f));
                            }
                        }
                        else if (pokemon[movingPokemon].getStatus() == Pokemon.Status.ASLEEP)
                        {
                            pokemon[movingPokemon].removeSleepTurn();
                            if (pokemon[movingPokemon].getStatus() == Pokemon.Status.ASLEEP)
                            {
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(generatePreString(movingPokemon)
                                    + pokemon[movingPokemon].getName() + "快速进入睡眠.", 0.75f));
                                canMove = false;
                            }
                            else
                            {
                                yield return StaticCoroutine.StartCoroutine(viewModelInfo.updatePokemonStatsDisplay(movingPokemon, pokemon[movingPokemon]));
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(generatePreString(movingPokemon)
                                    + pokemon[movingPokemon].getName() + " 醒过来了!", 0.75f));
                            }
                        }
                        if (canMove && pokemon[movingPokemon] != null && commandMove[movingPokemon] != null)
                        {
                            pokemon[movingPokemon].removePP(commandMove[movingPokemon].getTmNo(), 1);
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(generatePreString(movingPokemon)
                                + pokemon[movingPokemon].getName() + "使用" + commandMove[movingPokemon].getName() + "!", 0.75f));
                            if (accuracy != 0 && Random.value > accuracy)
                            {
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(generatePreString(movingPokemon)
                                    + pokemon[movingPokemon].getName() + "的攻击没有命中!", 0.75f));
                                canMove = false;
                            }
                        }
                        if (canMove && commandMove[movingPokemon] != null)
                        {
                            float damageToDeal = 0;
                            bool applyCritical = false;
                            float superEffectiveModifier = -1;
                            if (commandMove[movingPokemon].hasMoveEffect(MoveData.Effect.Heal)) //治愈能力的技能
                            {
                                yield return StaticCoroutine.StartCoroutine(Heal(targetIndex, commandMove[movingPokemon]
                                    .getMoveParameter(MoveData.Effect.Heal)));
                            }
                            else if (commandMove[movingPokemon].hasMoveEffect(MoveData.Effect.SetDamage)) //设置破坏
                            {
                                damageToDeal = commandMove[movingPokemon].getMoveParameter(MoveData.Effect.SetDamage);
                                if (damageToDeal == 0)
                                {
                                    damageToDeal = pokemon[movingPokemon].getLevel();
                                }
                                superEffectiveModifier =
                                    viewModelInfo.getSuperEffectiveModifier(commandMove[movingPokemon].getType(),
                                    pokemonType1[targetIndex]) *
                                    viewModelInfo.getSuperEffectiveModifier(commandMove[movingPokemon].getType(),
                                    pokemonType2[targetIndex]) *
                                    viewModelInfo.getSuperEffectiveModifier(commandMove[movingPokemon].getType(),
                                    pokemonType3[targetIndex]);
                                if (superEffectiveModifier > 0f)
                                {
                                    superEffectiveModifier = 1f;
                                }
                            }
                            else
                            {
                                //计算伤害
                                damageToDeal = viewModelInfo.calculateDamage(pokemon[movingPokemon], pokemon[targetIndex],
                                    commandMove[movingPokemon]);
                                applyCritical = viewModelInfo.calculateCritical(movingPokemon, targetIndex,
                                    commandMove[movingPokemon]);
                                if (applyCritical)
                                {
                                    damageToDeal *= 1.5f;
                                }
                                superEffectiveModifier =
                                    viewModelInfo.getSuperEffectiveModifier(commandMove[movingPokemon].getType(),
                                    pokemonType1[targetIndex]) *
                                    viewModelInfo.getSuperEffectiveModifier(commandMove[movingPokemon].getType(),
                                    pokemonType2[targetIndex]) *
                                    viewModelInfo.getSuperEffectiveModifier(commandMove[movingPokemon].getType(),
                                    pokemonType3[targetIndex]);
                                damageToDeal *= superEffectiveModifier;
                                if (commandMove[movingPokemon].getCategory() == MoveData.Category.PHYSICAL)
                                {
                                    damageToDeal *= viewModelInfo.calculateStatModifier(0, movingPokemon);
                                    if (applyCritical)
                                    {
                                        damageToDeal *= viewModelInfo.calculateStatModifier(1, targetIndex);
                                    }
                                    else
                                    {
                                        damageToDeal /= viewModelInfo.calculateStatModifier(1, targetIndex);
                                        if (pokemon[movingPokemon].getStatus() == Pokemon.Status.BURNED)
                                        {
                                            damageToDeal /= 2f;
                                        }
                                    }
                                }
                                else if (commandMove[movingPokemon].getCategory() == MoveData.Category.SPECIAL)
                                {
                                    damageToDeal *= viewModelInfo.calculateStatModifier(2, movingPokemon);
                                    if (applyCritical)
                                    {
                                        damageToDeal *= viewModelInfo.calculateStatModifier(3, targetIndex);
                                    }
                                    else
                                    {
                                        damageToDeal /= viewModelInfo.calculateStatModifier(3, targetIndex);
                                    }
                                }
                            }
                            float originHpSize = Mathf.CeilToInt(pokemon[targetIndex].getPercentHP() * 76f);
                            pokemon[targetIndex].removeHP(damageToDeal);
                            yield return StaticCoroutine.StartCoroutine(viewModelInfo.animateOverlayer(targetIndex, BattleHandler.overlayStatUpPath, 0.002f));
                            if (damageToDeal > 0)
                            {
                                if (superEffectiveModifier > 1.01f)
                                {
                                    SfxHandler.Play(audioInfoObject.hitSuperClip);
                                }
                                else if (superEffectiveModifier < 0.99f)
                                {
                                    SfxHandler.Play(audioInfoObject.hitPoorClip);
                                }
                                else
                                {
                                    SfxHandler.Play(audioInfoObject.hitClip);
                                }
                            }
                            else
                            {
                                SfxHandler.Play(audioInfoObject.hitPoorClip);
                            }
                            yield return StaticCoroutine.StartCoroutine(viewModelInfo.updatePokemonStatsDisplay(targetIndex, pokemon[targetIndex]));
                            float targetHpSize = Mathf.CeilToInt(pokemon[targetIndex].getPercentHP() * 76f);
                            float distance = originHpSize - targetHpSize;
                            yield return StaticCoroutine.StartCoroutine(viewModelInfo.setStretchBar(hpBarPath[targetIndex],
                                targetHpSize, 2f * distance / 76));
                            if (superEffectiveModifier == 0)
                            {
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("貌似不是很有效...", 0.75f));
                            }
                            else if (commandMove[movingPokemon].getCategory() != MoveData.Category.STATUS)
                            {
                                if (applyCritical)
                                {
                                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("会心一击!", 0.75f));
                                }
                                if (superEffectiveModifier > 1)
                                {
                                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("效果拔群!", 0.75f));
                                }
                                else if (superEffectiveModifier < 1)
                                {
                                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("不是很有效.", 0.75f));
                                }
                                else
                                {
                                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("效果一般.", 0.75f));
                                }
                            }
                            else
                            {
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("效果一般.", 0.75f));
                            }
                            MoveData.Effect[] moveEffects = commandMove[movingPokemon].getMoveEffects();
                            float[] moveEffectParameters = commandMove[movingPokemon].getMoveParameters();

                            bool statUpRun = false;
                            bool statDownRun = false;
                            bool statUpSelfRun = false;
                            bool statDownSelfRun = false;
                            for (int i2 = 0; i2 < moveEffects.Length; i2++)
                            {
                                if (moveEffects[i2] == MoveData.Effect.Chance)
                                {
                                    if (Random.value > moveEffectParameters[i2])
                                    {
                                        i2 = moveEffects.Length;
                                    }
                                }
                                else
                                {
                                    bool animate = false;
                                    if (moveEffects[i2] == MoveData.Effect.ATK ||
                                        moveEffects[i2] == MoveData.Effect.DEF ||
                                        moveEffects[i2] == MoveData.Effect.SPA ||
                                        moveEffects[i2] == MoveData.Effect.SPD ||
                                        moveEffects[i2] == MoveData.Effect.SPE ||
                                        moveEffects[i2] == MoveData.Effect.ACC ||
                                        moveEffects[i2] == MoveData.Effect.EVA)
                                    {
                                        if (moveEffectParameters[i2] > 0 && !statUpRun)
                                        {
                                            statUpRun = true;
                                            animate = true;
                                        }
                                        else if (moveEffectParameters[i2] < 0 && !statDownRun)
                                        {
                                            statDownRun = true;
                                            animate = true;
                                        }
                                    }
                                    else if (moveEffects[i2] == MoveData.Effect.ATKself ||
                                         moveEffects[i2] == MoveData.Effect.DEFself ||
                                         moveEffects[i2] == MoveData.Effect.SPAself ||
                                         moveEffects[i2] == MoveData.Effect.SPDself ||
                                         moveEffects[i2] == MoveData.Effect.SPEself ||
                                         moveEffects[i2] == MoveData.Effect.ACCself ||
                                         moveEffects[i2] == MoveData.Effect.EVAself)
                                    {
                                        if (moveEffectParameters[i2] > 0 && !statUpSelfRun)
                                        {
                                            statUpSelfRun = true;
                                            animate = true;
                                        }
                                        else if (moveEffectParameters[i2] < 0 && !statDownSelfRun)
                                        {
                                            statDownSelfRun = true;
                                            animate = true;
                                        }
                                    }
                                    else
                                    {
                                        animate = true;
                                    }
                                    yield return StaticCoroutine.StartCoroutine(applyEffect(movingPokemon, targetIndex, moveEffects[i2],
                                        moveEffectParameters[i2], animate));
                                }
                            }
                            yield return StaticCoroutine.StartCoroutine(viewModelInfo.updatePokemonStatsDisplay(targetIndex, pokemon[targetIndex]));
                        }
                        dialog.undrawDialogBox();
                    }
                }
                else if (command[movingPokemon] == CommandType.Switch)
                {
                    //是否需要状态栏移动
                    bool needSlideStatus = false;
                    if (pokemon[movingPokemon].getStatus() != Pokemon.Status.FAINTED)
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(pokemon[movingPokemon].getName() + ", 回来了!", 0.75f));
                        yield return StaticCoroutine.StartCoroutine(viewModelInfo.withdrawPokemon(movingPokemon));
                        needSlideStatus = false;
                    }
                    else
                    {
                        needSlideStatus = true;
                    }
                    if (switchPokemon(movingPokemon, commandPokemon[movingPokemon]))
                    {
                        yield return StaticCoroutine.StartCoroutine(showSwitchPokemonAnim("去吧! " + pokemon[movingPokemon].getName()
                                               + "!", movingPokemon, needSlideStatus));
                    }
                }
            }
            ////////////////////////////////////////
            /// 使用完技能后的附加效果伤害
            ////////////////////////////////////////
            if (battleTime == 0)  //单回合有效
            {
                for (int i = 0; i < 6; i++)
                {
                    if (pokemon[i] != null)
                    {
                        if (pokemon[i].getStatus() == Pokemon.Status.BURNED ||
                            pokemon[i].getStatus() == Pokemon.Status.POISONED)
                        {
                            float originHpSize = Mathf.CeilToInt(pokemon[i].getPercentHP() * 76f);
                            float amount = Mathf.Floor((float)pokemon[i].getHP() / 8f);
                            pokemon[i].removeHP(amount);
                            if (pokemon[i].getStatus() == Pokemon.Status.BURNED)
                            {
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(generatePreString(i)
                                    + pokemon[i].getName() + "被灼烧造成伤害!", 0.75f));
                            }
                            else if (pokemon[i].getStatus() == Pokemon.Status.POISONED)
                            {
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(generatePreString(i)
                                    + pokemon[i].getName() + "被毒造成伤害!", 0.75f));
                            }
                            SfxHandler.Play(audioInfoObject.hitClip);
                            yield return StaticCoroutine.StartCoroutine(viewModelInfo.updatePokemonStatsDisplay(i, pokemon[i]));
                            float targetHpSize = Mathf.CeilToInt(pokemon[i].getPercentHP() * 76f);
                            float distance = originHpSize - targetHpSize;
                            yield return StaticCoroutine.StartCoroutine(viewModelInfo.setStretchBar(hpBarPath[i], targetHpSize,
                                2f * distance / 76));
                        }
                    }
                }
            }

            if (pokemon[3] != null && pokemon[3].getStatus() == Pokemon.Status.FAINTED)
            {
                yield return StaticCoroutine.StartCoroutine(faintPokemonAnim(3));
                if (pokemon[0].getStatus() != Pokemon.Status.FAINTED)
                {
                    float isWildMod = (trainerBattle) ? 1.5f : 1f;
                    float baseExpYield = PokemonDatabase.getPokemon(pokemon[3].getID()).getBaseExpYield();
                    BagItem heldItem = pokemon[0].getHeldItem();
                    float luckyEggMod = (heldItem != null && heldItem.itemName == "Lucky Egg") ? 1.5f : 1f;
                    float OTMod = (pokemon[0].getIDno() != trainerInfoData.playerID) ? 1.5f : 1f;
                    float sharedMod = 1f;
                    float IVMod = 0.85f + (float)(pokemon[3].getIV_HP() +
                               pokemon[3].getIV_ATK() +
                               pokemon[3].getIV_DEF() +
                               pokemon[3].getIV_SPA() +
                               pokemon[3].getIV_SPD() +
                               pokemon[3].getIV_SPE()) / 480f;
                    int exp = Mathf.CeilToInt((isWildMod * baseExpYield * IVMod * OTMod *
                                 luckyEggMod * (float)pokemon[3].getLevel()) / 7 * sharedMod);
                    yield return StaticCoroutine.StartCoroutine(addExp(0, exp));
                }
                // 敌人重新放出精灵
                if (autoSwitchPokemon(3, opponentParty))
                {
                    yield return StaticCoroutine.StartCoroutine(showSwitchPokemonAnim(opponentName + "放出"
                        + pokemon[3].getName() + "!", 3, true));
                }
                battleTime = 2;
            }
            else if (pokemon[0] != null && pokemon[0].getStatus() == Pokemon.Status.FAINTED)
            {
                yield return StaticCoroutine.StartCoroutine(faintPokemonAnim(0));
                // 我方重新放出精灵
                yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.FaintPartyChoice));
                battleTime = 2;
            }

            // 所以敌人被打败
            allOpponentsDefeated = true;
            for (int i = 0; i < opponentParty.Length; i++)
            {
                if (opponentParty[i] != null && opponentParty[i].getStatus() != Pokemon.Status.FAINTED)
                {
                    allOpponentsDefeated = false;
                }
            }
            allPlayersDefeated = true;
            for (int i = 0; i < playerParty.Length; i++)
            {
                if (playerParty[i] != null && playerParty[i].getStatus() != Pokemon.Status.FAINTED)
                {
                    allPlayersDefeated = false;
                }
            }
            ////////////////////////////////////////
            /// 最后的胜利校验
            ////////////////////////////////////////
            if (allOpponentsDefeated)
            {
                if (trainerBattle)
                {
                    if (trainer.victoryBGM == null)
                    {
                        //BgmHandler.main.PlayOverlay(defaultTrainerVictoryBGM, defaultTrainerVictoryBGMLoopStart);
                    }
                    else
                    {
                        BgmHandler.main.PlayOverlay(trainer.victoryBGM, trainer.victorySamplesLoopStart);
                    }
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(trainerInfoData.playerName
                        + "打败" + opponentName + "!"));
                    if (trainer.playerVictoryDialog != null)
                    {
                        for (int di = 0; di < trainer.playerVictoryDialog.Length; di++)
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(trainer.playerVictoryDialog[di]));
                        }
                    }
                    TrainerManager.Instance.fightSettlement(trainer.GetPrizeMoney(), delegate (bool result)
                    {
                        if (result)
                        {
                            StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(trainerInfoData.playerName
                                + "获得" + trainer.GetPrizeMoney() + "金币胜利奖励!"));
                        }
                    });
                }
                else
                {
                    if (trainer.victoryBGM == null)
                    {
                        //BgmHandler.main.PlayOverlay(defaultWildVictoryBGM, defaultWildVictoryBGMLoopStart);
                    }
                    else
                    {
                        //BgmHandler.main.PlayOverlay(trainer.victoryBGM, trainer.victorySamplesLoopStart);
                    }
                }
            }
            else if (allPlayersDefeated)
            {
                if (trainerBattle)
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(opponentName + "打败" + trainerInfoData.playerName + "!"));
                    string[] playerLossDialog = trainer.playerLossDialog;
                    for (int di = 0; playerLossDialog != null && di < playerLossDialog.Length; di++)
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(playerLossDialog[di]));
                    }
                    TrainerManager.Instance.fightSettlement(-trainer.GetPrizeMoney(), delegate (bool result)
                    {
                        if (result)
                        {
                            StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(trainerInfoData.playerName
                                + "失去" + trainer.GetPrizeMoney() + "金币!"));
                        }
                    });
                }
                else
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(trainerInfoData.playerName
                        + "没有可用的精灵!", 0.75f));
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("... ... ... ...", 0.75f));
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(trainerInfoData.playerName
                        + "昏过去了!", 0.75f));
                }
            }
            if (allPlayersDefeated || allOpponentsDefeated)
            {
                updatePartPokemons();
                yield return new WaitForSeconds(1f);
                running = false;
                battleTime = 2;
                continue;
            }
        }
    }

    // 计算列表中速度最快的精灵
    private int getHighestSpeedIndex(int excludePos)
    {
        int topSpeed = 0;
        int topPriority = -7;
        int topSpeedPosition = 0;

        for (int i = 0; i < 6; i++)
        {
            if (i != excludePos)
            {
                float calculatedPokemonPriority = 0;
                float calculatedPokemonSpeed = 0;
                if (pokemon[i] != null)
                {
                    calculatedPokemonSpeed = (float)pokemon[i].getSPE() * viewModelInfo.calculateStatModifier(4, i);
                    if (pokemon[i].getStatus() == Pokemon.Status.PARALYZED)
                    {
                        calculatedPokemonSpeed /= 4f;
                    }
                }
                //我方逃跑，暂时只有0有选择，3为敌人的move
                if (command[i] == CommandType.Flee && i < 3)
                {
                    calculatedPokemonPriority = 6;
                }
                else if (command[i] == CommandType.Item)
                {
                    calculatedPokemonPriority = 6;
                }
                else if (command[i] == CommandType.Move)
                {
                    if (commandMove[i] != null)
                    {
                        calculatedPokemonPriority = commandMove[i].getPriority();
                    }
                }
                else if (command[i] == CommandType.Switch)
                {
                    calculatedPokemonPriority = 6;
                }
                if ((calculatedPokemonSpeed >= topSpeed && calculatedPokemonPriority >= topPriority) ||
                    calculatedPokemonPriority > topPriority)
                {
                    if (calculatedPokemonSpeed == topSpeed && calculatedPokemonPriority == topPriority)
                    {
                        if (Random.value > 0.5f)
                        {
                            topSpeedPosition = i;
                        }
                    }
                    else
                    {
                        //更新最快速度精灵位置
                        topSpeed = Mathf.FloorToInt(calculatedPokemonSpeed);
                        topPriority = Mathf.FloorToInt(calculatedPokemonPriority);
                        topSpeedPosition = i;
                    }
                }
            }
        }
        return topSpeedPosition;
    }

    // 状态机
    public IEnumerator updateCurrentTask(TaskState newState)
    {
        viewModelInfo.setFourButtonVisible(false);
        viewModelInfo.setMoveButtonVisible(false, pokemon[0]);
        viewModelInfo.MoveReturnObject = false;
        partyViewModel.dismissPokemonSlotsDisplay();

        currentTask = newState;
        if (currentTask == TaskState.TaskChoice)
        {
            viewModelInfo.setFourButtonVisible(true);
        }
        else if (currentTask == TaskState.MoveChoice)
        {
            viewModelInfo.setMoveButtonVisible(true, pokemon[0]);
            if (pokemon[0] != null)
            {
                viewModelInfo.updateMovesetDisplay(pokemon[0].getSkillInfos());
            }
            viewModelInfo.MoveReturnObject = true;
        }
        else if (currentTask == TaskState.BagChoice)
        {
            Scene.main.Bag.gameObject.SetActive(true);
            StaticCoroutine.StartCoroutine(Scene.main.Bag.control(this));
            while (Scene.main.Bag.gameObject.activeSelf)
            {
                yield return null;
            }
            yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, 0.01f));
        }
        else if (currentTask == TaskState.PartyChoice)
        {
            partyViewModel.updatePokemonSlotsDisplay(playerParty);
            viewModelInfo.MoveReturnObject = true;
        }
        else if (currentTask == TaskState.FaintPartyChoice)
        {
            partyViewModel.updatePokemonSlotsDisplay(playerParty);
        }
        yield return null;
    }
    //========================================================================== 
    //增加经验
    private IEnumerator addExp(int position, int exp)
    {
        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(pokemon[position].getName()
            + "获得 " + exp + "经验值!", 0.75f));
        int expPool = exp;
        while (expPool > 0)
        {
            int expToNextLevel = pokemon[position].getExpNext() - pokemon[position].getExp();
            if (expPool >= expToNextLevel)
            {
                pokemon[position].addExp(expToNextLevel);
                expPool -= expToNextLevel;

                //AudioSource fillSource = SfxHandler.Play(fillExpClip);
                yield return StaticCoroutine.StartCoroutine(viewModelInfo.setStretchBar(BattleHandler.expBarPath, 80f, 1f));
                //SfxHandler.FadeSource(fillSource, 0.2f);
                SfxHandler.Play(audioInfoObject.expFullClip);
                yield return StaticCoroutine.StartCoroutine(viewModelInfo.updatePokemonStatsDisplay(position, pokemon[position]));

                BgmHandler.main.PlayMFX(Resources.Load<AudioClip>("Audio/mfx/GetAverage"));
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(pokemon[position].getName()
                    + " 成长到等级 " + pokemon[position].getLevel() + "!", 0.75f));
                //newMove可以学习的新技能
                long newMove = pokemon[position].MoveLearnedAtLevel(pokemon[position].getLevel());
                if (newMove > 0 && !pokemon[position].HasMove(newMove))
                {
                    yield return StaticCoroutine.StartCoroutine(LearnMove(pokemon[position], newMove));
                }
            }
            else
            {
                pokemon[position].addExp(expPool);
                expPool = 0;
                yield return StaticCoroutine.StartCoroutine(setExp(pokemon[position]));
            }
            yield return null;
        }
    }

    //设置经验值
    private IEnumerator setExp(Pokemon pokemon)
    {
        float levelStartExp = PokemonDatabase.getLevelExp(PokemonDatabase.getPokemon(pokemon.getID()).getLevelingRate(), pokemon.getLevel());
        float currentExpMinusStart = pokemon.getExp() - levelStartExp;
        float nextLevelExpMinusStart = pokemon.getExpNext() - levelStartExp;

        //AudioSource fillSource = SfxHandler.Play(fillExpClip);
        yield return StaticCoroutine.StartCoroutine(viewModelInfo.setStretchBar(BattleHandler.expBarPath,
            80f * (currentExpMinusStart / nextLevelExpMinusStart), 1f));
    }

    //学习新技能
    private IEnumerator LearnMove(Pokemon selectedPokemon, long move)
    {
        string moveName = MoveDatabase.getMoveName(move);
        int chosenIndex = 1;
        if (chosenIndex == 1)
        {
            bool learning = true;
            while (learning)
            {
                if (selectedPokemon.getMoveCount() == 4)
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(
                            selectedPokemon.getName() + "想要学习技能" + moveName + ".", 0.75f));
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("但是, "
                        + selectedPokemon.getName() + "已经有四个技能了.", 0.75f));
                    yield return
                        StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("需要一个技能被遗忘并且替换为"
                        + moveName + "?", 0.75f));
                    string[] choices = new string[] { "是", "否" };
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
                    chosenIndex = dialog.buttonIndex;
                    if (chosenIndex == 1)
                    {
                        yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
                        Scene.main.Summary.gameObject.SetActive(true);
                        StaticCoroutine.StartCoroutine(Scene.main.Summary.control(selectedPokemon, move));
                        while (Scene.main.Summary.gameObject.activeSelf)
                        {
                            yield return null;
                        }

                        long replacedMove = Scene.main.Summary.getReplacedMove();
                        yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));

                        if (replacedMove > 0)
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("1, ", 0.75f));
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("2, ", 0.75f));
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("和... ", 0.75f));
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("... ", 0.75f));
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("... ", 0.75f));
                            SfxHandler.Play(audioInfoObject.pokeballBounceClip);
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay("呼地!", 0.75f));
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(selectedPokemon.getName()
                                + "忘记使用" + MoveDatabase.getMoveName(replacedMove) + ".", 0.75f));
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("并且...", 0.75f));
                            AudioClip mfx = Resources.Load<AudioClip>("Audio/mfx/GetAverage");
                            BgmHandler.main.PlayMFX(mfx);
                            StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(selectedPokemon.getName() + "学会了" + moveName + "!"));
                            yield return new WaitForSeconds(mfx.length);
                            learning = false;
                        }
                        else
                        {
                            chosenIndex = 0;
                        }
                    }
                    if (chosenIndex == 0)
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("放弃学习技能" + moveName + "?", 0.75f));
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
                        chosenIndex = dialog.buttonIndex;
                        if (chosenIndex == 1)
                        {
                            learning = false;
                            chosenIndex = 0;
                        }
                    }
                }
                else
                {
                    selectedPokemon.addMove(move);
                    AudioClip mfx = Resources.Load<AudioClip>("Audio/mfx/GetAverage");
                    BgmHandler.main.PlayMFX(mfx);
                    StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(selectedPokemon.getName() + "学会了" + moveName + "!"));
                    yield return new WaitForSeconds(mfx.length);
                    learning = false;
                }
            }

        }
        if (chosenIndex == 0)
        {
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(selectedPokemon.getName() + "没有学习" + moveName + "."));
        }
    }

    private IEnumerator applyEffect(int attackerPosition, int targetPosition, MoveData.Effect effect, float parameter)
    {
        yield return StaticCoroutine.StartCoroutine(applyEffect(attackerPosition, targetPosition, effect, parameter, true));
    }

    private IEnumerator applyEffect(int attackerPosition, int targetPosition, MoveData.Effect effect, float parameter,
        bool animate)
    {
        if (pokemon[targetPosition] != null)
        {
            if (pokemon[targetPosition].getStatus() != Pokemon.Status.FAINTED)
            {
                if (effect == MoveData.Effect.ATK)
                {
                    yield return StaticCoroutine.StartCoroutine(ModifyStat(targetPosition, 0, parameter, animate));
                }
                else if (effect == MoveData.Effect.DEF)
                {
                    yield return StaticCoroutine.StartCoroutine(ModifyStat(targetPosition, 1, parameter, animate));
                }
                else if (effect == MoveData.Effect.SPA)
                {
                    yield return StaticCoroutine.StartCoroutine(ModifyStat(targetPosition, 2, parameter, animate));
                }
                else if (effect == MoveData.Effect.SPD)
                {
                    yield return StaticCoroutine.StartCoroutine(ModifyStat(targetPosition, 3, parameter, animate));
                }
                else if (effect == MoveData.Effect.SPE)
                {
                    yield return StaticCoroutine.StartCoroutine(ModifyStat(targetPosition, 4, parameter, animate));
                }
                else if (effect == MoveData.Effect.ACC)
                {
                    yield return StaticCoroutine.StartCoroutine(ModifyStat(targetPosition, 5, parameter, animate));
                }
                else if (effect == MoveData.Effect.EVA)
                {
                    yield return StaticCoroutine.StartCoroutine(ModifyStat(targetPosition, 6, parameter, animate));
                }
                else if (effect == MoveData.Effect.Burn)
                {
                    if (Random.value <= parameter)
                    {
                        if (pokemon[targetPosition].setStatus(Pokemon.Status.BURNED))
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                                    generatePreString(targetPosition) + pokemon[targetPosition].getName() +
                                    "被灼烧了!", 0.75f));
                        }
                    }
                }
                else if (effect == MoveData.Effect.Freeze)
                {
                    if (Random.value <= parameter)
                    {
                        if (pokemon[targetPosition].setStatus(Pokemon.Status.FROZEN))
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                                    generatePreString(targetPosition) + pokemon[targetPosition].getName() +
                                    "被冻伤了!", 0.75f));
                        }
                    }
                }
                else if (effect == MoveData.Effect.Paralyze)
                {
                    if (Random.value <= parameter)
                    {
                        if (pokemon[targetPosition].setStatus(Pokemon.Status.PARALYZED))
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                                    generatePreString(targetPosition) + pokemon[targetPosition].getName() +
                                    "麻痹了! 它不能动弹!", 0.75f));
                        }
                    }
                }
                else if (effect == MoveData.Effect.Poison)
                {
                    if (Random.value <= parameter)
                    {
                        if (pokemon[targetPosition].setStatus(Pokemon.Status.POISONED))
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                                    generatePreString(targetPosition) + pokemon[targetPosition].getName() +
                                    "中毒了!", 0.75f));
                        }
                    }
                }
                else if (effect == MoveData.Effect.Toxic)
                {
                    if (Random.value <= parameter)
                    {
                        if (pokemon[targetPosition].setStatus(Pokemon.Status.POISONED))
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                                    generatePreString(targetPosition) + pokemon[targetPosition].getName() +
                                    "中毒严重!", 0.75f));
                        }
                    }
                }
                else if (effect == MoveData.Effect.Sleep)
                {
                    if (Random.value <= parameter)
                    {
                        if (pokemon[targetPosition].setStatus(Pokemon.Status.ASLEEP))
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                                    generatePreString(targetPosition) + pokemon[targetPosition].getName() +
                                    "陷入沉睡!", 0.75f));
                        }
                    }
                }
            }
        }
        if (effect == MoveData.Effect.ATKself)
        {
            yield return StaticCoroutine.StartCoroutine(ModifyStat(attackerPosition, 0, parameter, animate));
        }
        else if (effect == MoveData.Effect.DEFself)
        {
            yield return StaticCoroutine.StartCoroutine(ModifyStat(attackerPosition, 1, parameter, animate));
        }
        else if (effect == MoveData.Effect.SPAself)
        {
            yield return StaticCoroutine.StartCoroutine(ModifyStat(attackerPosition, 2, parameter, animate));
        }
        else if (effect == MoveData.Effect.SPDself)
        {
            yield return StaticCoroutine.StartCoroutine(ModifyStat(attackerPosition, 3, parameter, animate));
        }
        else if (effect == MoveData.Effect.SPEself)
        {
            yield return StaticCoroutine.StartCoroutine(ModifyStat(attackerPosition, 4, parameter, animate));
        }
        else if (effect == MoveData.Effect.ACCself)
        {
            yield return StaticCoroutine.StartCoroutine(ModifyStat(attackerPosition, 5, parameter, animate));
        }
        else if (effect == MoveData.Effect.EVAself)
        {
            yield return StaticCoroutine.StartCoroutine(ModifyStat(attackerPosition, 6, parameter, animate));
        }
    }

    private IEnumerator ModifyStat(int targetPosition, int statIndex, float param, bool animate)
    {
        int parameter = Mathf.FloorToInt(param);
        string[] statName = new string[] {
        "攻击", "防御", "特攻", "特防", "速度", "Accuracy", "Evasion"
        };

        bool canModify = viewModelInfo.changeStatsMod(statIndex, targetPosition, parameter);
        if (canModify)
        {
            if (animate)
            {
                yield return StaticCoroutine.StartCoroutine(viewModelInfo
                    .animateOverlayer(targetPosition, "overlayStatUpTex", 1f));
            }

            if (parameter == 1)
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                        generatePreString(targetPosition) + pokemon[targetPosition].getName() + "'s " +
                        statName[statIndex] + " \\nrose!", 0.75f));
            }
            else if (parameter == -1)
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                        generatePreString(targetPosition) + pokemon[targetPosition].getName() + "'s " +
                        statName[statIndex] + " 下降!", 0.75f));
            }
            else if (parameter == 2)
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                        generatePreString(targetPosition) + pokemon[targetPosition].getName() + "'s " +
                        statName[statIndex] + " 猛烈上升!", 0.75f));
            }
            else if (parameter == -2)
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                        generatePreString(targetPosition) + pokemon[targetPosition].getName() + "'s " +
                        statName[statIndex] + " 暴跌!", 0.75f));
            }
            else if (parameter >= 3)
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                        generatePreString(targetPosition) + pokemon[targetPosition].getName() + "'s " +
                        statName[statIndex] + " 急剧上升!", 0.75f));
            }
            else if (parameter <= -3)
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                        generatePreString(targetPosition) + pokemon[targetPosition].getName() + "'s " +
                        statName[statIndex] + " 严重下降!", 0.75f));
            }
        }
        else
        {
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(
                     generatePreString(targetPosition) + pokemon[targetPosition].getName() + "的" +
                     statName[statIndex] + " 不能修改了!", 0.75f));
        }
    }

    private IEnumerator Heal(int index, float healAmount)
    {
        yield return StaticCoroutine.StartCoroutine(Heal(index, healAmount, false));
    }

    private IEnumerator Heal(int index, bool curingStatus)
    {
        yield return StaticCoroutine.StartCoroutine(Heal(index, -1, curingStatus));
    }

    private IEnumerator Heal(int index, float healAmount, bool curingStatus)
    {
        if ((!curingStatus && pokemon[index].getCurrentHP() == pokemon[index].getHP()) ||
            (curingStatus &&
             (pokemon[index].getStatus() == Pokemon.Status.NONE || pokemon[index].getStatus() == Pokemon.Status.FAINTED)))
        {
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay("没有效果...", 0.75f));
        }
        else
        {
            SfxHandler.Play(audioInfoObject.healFieldClip);
            yield return StaticCoroutine.StartCoroutine(viewModelInfo.updateAnimateOverlayer(index));
            if (curingStatus)
            {
                Pokemon.Status status = pokemon[index].getStatus();
                pokemon[index].healStatus();
                yield return StaticCoroutine.StartCoroutine(viewModelInfo.updatePokemonStatsDisplay(index, pokemon[index]));
                if (status == Pokemon.Status.ASLEEP)
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(generatePreString(index)
                        + pokemon[index].getName() + "醒过来了!", 0.75f));
                }
                else if (status == Pokemon.Status.BURNED)
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(generatePreString(index)
                        + pokemon[index].getName() + "的烧伤被治愈了!", 0.75f));
                }
                else if (status == Pokemon.Status.FROZEN)
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(generatePreString(index)
                        + pokemon[index].getName() + "解冻了!", 0.75f));
                }
                else if (status == Pokemon.Status.PARALYZED)
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(generatePreString(index)
                        + pokemon[index].getName() + "的麻痹被治愈了!", 0.75f));
                }
                else if (status == Pokemon.Status.POISONED)
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(generatePreString(index)
                        + pokemon[index].getName() + "毒被治愈了!", 0.75f));
                }
            }
            else
            {
                if (healAmount < 1.01f)
                {
                    healAmount = Mathf.CeilToInt((float)pokemon[index].getHP() * healAmount);
                }
                int healedHP = pokemon[index].getCurrentHP();
                pokemon[index].healHP(healAmount);
                healedHP = pokemon[index].getCurrentHP() - healedHP;
                yield return StaticCoroutine.StartCoroutine(viewModelInfo.setStretchBar(hpBarPath[index],
                    pokemon[index].getPercentHP() * 76, 2f));
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelay(generatePreString(index)
                    + pokemon[index].getName() + "血量恢复了" + healedHP + "点.", 0.75f));
            }
        }
    }

    //========================================================================== 
    public void OnButtonFight()
    {
        StaticCoroutine.StartCoroutine(doButtonFight());
    }

    private IEnumerator doButtonFight()
    {
        SfxHandler.Play(audioInfoObject.selectClip);
        yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.MoveChoice));
    }

    public void OnButtonParty()
    {
        StaticCoroutine.StartCoroutine(doButtonParty());
    }

    private IEnumerator doButtonParty()
    {
        SfxHandler.Play(audioInfoObject.selectClip);
        yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.PartyChoice));
    }

    public void OnButtonRun()
    {
        StaticCoroutine.StartCoroutine(doButtonRun());
    }

    //点击逃跑
    private IEnumerator doButtonRun()
    {
        int currentPokemon = 0;
        SfxHandler.Play(audioInfoObject.selectClip);
        if (trainerBattle)
        {
            yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.RunChoice));
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("不,怎么能在和训练者的战斗中逃跑呢!"));
            yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.TaskChoice));
        }
        else
        {
            command[currentPokemon] = CommandType.Flee;
            runState = false;
            yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.RunChoice));
        }
    }

    public void OnButtonBag()
    {
        StaticCoroutine.StartCoroutine(doButtonBag());
    }

    private IEnumerator doButtonBag()
    {
        SfxHandler.Play(audioInfoObject.selectClip);
        yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.BagChoice));
    }

    public void OnTaskReturn()
    {
        StaticCoroutine.StartCoroutine(doTaskReturn());
    }

    private IEnumerator doTaskReturn()
    {
        SfxHandler.Play(audioInfoObject.selectClip);
        if (currentTask == TaskState.PartyChoice
            || currentTask == TaskState.MoveChoice
            || currentTask == TaskState.BagChoice)
        {
            yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.TaskChoice));
        }
    }

    private readonly SimpleCommand<int> onMoveButton;

    public ICommand OnMoveButton
    {
        get { return this.onMoveButton; }
    }

    public void DoMoveButtonClickDown(int index)
    {
        SfxHandler.Play(audioInfoObject.selectClip);
        StaticCoroutine.StartCoroutine(doMoveButtonClickDown(index));
    }

    // 技能按钮点击
    private IEnumerator doMoveButtonClickDown(int index)
    {
        int currentPokemon = 0;
        if (isCrazyAttack(currentPokemon))
        {
            commandMove[currentPokemon] = MoveDatabase.getMove(0);
            runState = false;
        }
        else
        {
            List<SkillInfo> skillInfos = pokemon[currentPokemon].getSkillInfos();
            if (skillInfos[index].curPp > 0)
            {
                commandMove[currentPokemon] = MoveDatabase.getMove(skillInfos[index].skillId);
                runState = false;
            }
            else
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("没有PP值了!"));
            }
        }
        if (!runState)
        {
            SfxHandler.Play(audioInfoObject.selectClip);
            command[currentPokemon] = CommandType.Move;
            yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.ClearChoice));
        }
    }

    //外部调用这个方法更换精灵
    public void DoPokemonSwitchButton(int index)
    {
        StaticCoroutine.StartCoroutine(doPokemonSwitchButton(index));
    }

    //点击Switch对应的精灵
    public IEnumerator doPokemonSwitchButton(int index)
    {
        Pokemon partyPokemon = playerParty[index];
        if (partyPokemon != null && partyPokemon.getStatus() != Pokemon.Status.FAINTED)
        {
            if (pokemon[0] != partyPokemon)
            {
                command[0] = CommandType.Switch;
                commandPokemon[0] = partyPokemon;
                runState = false;
                SfxHandler.Play(audioInfoObject.selectClip);
                yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.ClearChoice));
            }
            else
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(partyPokemon.getName() + "已经在战斗中!"));
            }
        }
        else
        {
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(partyPokemon.getName() + "无法进行战斗!"));
        }
    }

    //跳过这个回合操作
    public void jumpTurn()
    {
        int currentPokemon = 0;
        command[currentPokemon] = CommandType.None;
        runState = false;
        StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.ClearChoice));
    }

    //物品界面使用精灵球
    public void DoBagItemButtonClickDown(ItemData selectedItem, int target)
    {
        StaticCoroutine.StartCoroutine(doBagItemButtonClickDown(selectedItem, target));
    }

    //点击BagItem对应的物品
    private IEnumerator doBagItemButtonClickDown(ItemData selectedItem, int target)
    {
        int currentPokemon = 0;
        command[currentPokemon] = CommandType.Item;
        commandItem[currentPokemon] = selectedItem;
        commandTarget[currentPokemon] = target;
        runState = false;
        yield return StaticCoroutine.StartCoroutine(updateCurrentTask(TaskState.ClearChoice));
    }
    //==========================================================================
    private string generatePreString(int pokemonPosition)
    {
        string preString = "";
        if (pokemonPosition > 2)
        {
            preString = (trainerBattle) ? "训练师的" : "野生的";
        }
        return preString;
    }

    private float PlayCry(Pokemon pokemon)
    {
        if (pokemon.GetCry() != null)
        {
            AudioClip cry = pokemon.GetCry();
            SfxHandler.Play(cry, pokemon.GetCryPitch());
            return cry.length / pokemon.GetCryPitch();
        }
        return 0;
    }

    private IEnumerator PlayCryAndWait(Pokemon pokemon)
    {
        yield return new WaitForSeconds(PlayCry(pokemon));
    }

    //是否是疯狂攻击
    private bool isCrazyAttack(int curPokemonIndex)
    {
        bool crazyAttack = true;
        List<SkillInfo> skillInfos = pokemon[curPokemonIndex].getSkillInfos();
        if (skillInfos != null)
        {
            foreach (SkillInfo skillInfo in skillInfos)
            {
                if (skillInfo.curPp > 0)
                {
                    crazyAttack = false;
                }
            }
        }
        return crazyAttack;
    }

    //批量更新精灵
    private void updatePartPokemons()
    {
        List<Pokemon> partyPokemons = new List<Pokemon>();
        foreach (Pokemon pokemon in playerParty)
        {
            partyPokemons.Add(pokemon);
        }
        PCManager.Instance.updatePokemons(partyPokemons, delegate (bool result)
        {
            if (!result)
            {
                StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("更新精灵能力失败"));
            }
        });
    }
}