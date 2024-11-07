using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleViewModelInfo : ViewModelBase
{
    public class NotifyCollectionChangedObject
    {
        public NotifyCollectionChangedEventHandler battleBaseHandler;
        public NotifyCollectionChangedEventHandler withdrawPokemonHandler;
        public NotifyCollectionChangedEventHandler battleStatsHandler;
        public NotifyCollectionChangedEventHandler slideStatsHandler;
        public NotifyCollectionChangedEventHandler moveStatsHandler;
        public NotifyCollectionChangedEventHandler slidePokemonHandler;
        public NotifyCollectionChangedEventHandler releasePokemonHandler;
        public NotifyCollectionChangedEventHandler throwPokemonBallHandler;
        public NotifyCollectionChangedEventHandler faintPokemonHandler;
        public NotifyCollectionChangedEventHandler stretchBarHandler;
        public NotifyCollectionChangedEventHandler shrinkPokemonHandler;
        public NotifyCollectionChangedEventHandler animateOverlayerHandler;
    }

    public class AudioInfoObject
    {
        public AudioClip scrollClip;
        public AudioClip selectClip;
        public AudioClip runClip;
        public AudioClip statUpClip;
        public AudioClip statDownClip;
        public AudioClip healFieldClip;
        public AudioClip fillExpClip;
        public AudioClip expFullClip;
        public AudioClip pokeballOpenClip;
        public AudioClip pokeballBounceClip;
        public AudioClip faintClip;
        public AudioClip hitClip;
        public AudioClip hitSuperClip;
        public AudioClip hitPoorClip;
    }

    public ObservableList<SlideStats> slideStatsList;
    public ObservableList<BattlePartyViewModel.PokeIconInfo> battleStatsList;
    public ObservableList<BattleBase> battleBaseList;
    public ObservableList<WithdrawPokemon> withdrawPokemonList;
    public ObservableList<MoveStats> moveStatsList;
    public ObservableList<SlidePokemon> slidePokemonList;
    public ObservableList<ReleasePokemon> releasePokemonList;
    public ObservableList<ThrowPokemonBall> throwPokemonBallList;
    public ObservableList<FaintPokemon> fainPokemonList;
    public ObservableList<StretchBar> stretchBarList;
    public ObservableList<ShrinkPokemon> shrinkPokemonList;
    public ObservableList<AnimateOverlayer> animateOverlayerList;

    private int[][] pokemonStatsMod = new int[][]{
        new int[6], //ATK
        new int[6], //DEF
        new int[6], //SPA
        new int[6], //SPD
        new int[6], //SPE
        new int[6], //ACC
        new int[6], //EVA
    };

    public BattleViewModelInfo(NotifyCollectionChangedObject changedObject)
    {
        battleBaseList = new ObservableList<BattleBase>();
        battleBaseList.CollectionChanged += changedObject.battleBaseHandler;
        battleBaseList.Add(new BattleBase());
        withdrawPokemonList = new ObservableList<WithdrawPokemon>();
        withdrawPokemonList.CollectionChanged += changedObject.withdrawPokemonHandler;
        withdrawPokemonList.Add(new WithdrawPokemon());
        slideStatsList = new ObservableList<SlideStats>();
        slideStatsList.CollectionChanged += changedObject.slideStatsHandler;
        slideStatsList.Add(new SlideStats());
        battleStatsList = new ObservableList<BattlePartyViewModel.PokeIconInfo>();
        battleStatsList.CollectionChanged += changedObject.battleStatsHandler;
        battleStatsList.Add(new BattlePartyViewModel.PokeIconInfo());
        moveStatsList = new ObservableList<MoveStats>();
        moveStatsList.CollectionChanged += changedObject.moveStatsHandler;
        moveStatsList.Add(new MoveStats());
        slidePokemonList = new ObservableList<SlidePokemon>();
        slidePokemonList.CollectionChanged += changedObject.slidePokemonHandler;
        slidePokemonList.Add(new SlidePokemon());
        releasePokemonList = new ObservableList<ReleasePokemon>();
        releasePokemonList.CollectionChanged += changedObject.releasePokemonHandler;
        releasePokemonList.Add(new ReleasePokemon());
        throwPokemonBallList = new ObservableList<ThrowPokemonBall>();
        throwPokemonBallList.CollectionChanged += changedObject.throwPokemonBallHandler;
        throwPokemonBallList.Add(new ThrowPokemonBall());
        fainPokemonList = new ObservableList<FaintPokemon>();
        fainPokemonList.CollectionChanged += changedObject.faintPokemonHandler;
        fainPokemonList.Add(new FaintPokemon());
        stretchBarList = new ObservableList<StretchBar>();
        stretchBarList.CollectionChanged += changedObject.stretchBarHandler;
        stretchBarList.Add(new StretchBar());
        shrinkPokemonList = new ObservableList<ShrinkPokemon>();
        shrinkPokemonList.CollectionChanged += changedObject.shrinkPokemonHandler;
        shrinkPokemonList.Add(new ShrinkPokemon());
        animateOverlayerList = new ObservableList<AnimateOverlayer>();
        animateOverlayerList.CollectionChanged += changedObject.animateOverlayerHandler;
        animateOverlayerList.Add(new AnimateOverlayer());
    }

    public class SlideStats : ObservableObject
    {
        private string statType;
        private bool retract;
        private bool trainerBattle;
        private Pokemon[] partys;

        public bool TrainerBattle
        {
            get { return this.trainerBattle; }
            set { this.Set<bool>(ref this.trainerBattle, value, "TrainerBattle"); }
        }

        public Pokemon[] Partys
        {
            get { return this.partys; }
            set { this.Set<Pokemon[]>(ref this.partys, value, "Partys"); }
        }

        public bool Retract
        {
            get { return this.retract; }
            set { this.Set<bool>(ref this.retract, value, "Retract"); }
        }

        public string StatType
        {
            get { return this.statType; }
            set { this.Set<string>(ref this.statType, value, "StatType"); }
        }
    }

    public class MoveStats : ObservableObject
    {
        private int index;
        private Sprite buttonMoveCover;
        private Sprite buttonMoveType;
        private string buttonMoveName;
        private string buttonMovePP;

        public int Index
        {
            get { return this.index; }
            set { this.Set<int>(ref this.index, value, "Index"); }
        }

        public string ButtonMoveName
        {
            get { return this.buttonMoveName; }
            set { this.Set<string>(ref this.buttonMoveName, value, "ButtonMoveName"); }
        }

        public string ButtonMovePP
        {
            get { return this.buttonMovePP; }
            set { this.Set<string>(ref this.buttonMovePP, value, "ButtonMovePP"); }
        }

        public Sprite ButtonMoveCover
        {
            get { return this.buttonMoveCover; }
            set { this.Set<Sprite>(ref this.buttonMoveCover, value, "ButtonMoveCover"); }
        }

        public Sprite ButtonMoveType
        {
            get { return this.buttonMoveType; }
            set { this.Set<Sprite>(ref this.buttonMoveType, value, "ButtonMoveType"); }
        }
    }

    public class SlidePokemon : ObservableObject
    {
        private string statType;
        private float desVectorX;
        private float startVectorX;
        private bool showTrainer;
        private Sprite[] playerTrainer;

        public Sprite[] PlayerTrainer
        {
            get { return this.playerTrainer; }
            set { this.Set<Sprite[]>(ref this.playerTrainer, value, "PlayerTrainer"); }
        }

        public bool ShowTrainer
        {
            get { return this.showTrainer; }
            set { this.Set<bool>(ref this.showTrainer, value, "ShowTrainer"); }
        }

        public string StatType
        {
            get { return this.statType; }
            set { this.Set<string>(ref this.statType, value, "StatType"); }
        }

        public float StartVectorX
        {
            get { return this.startVectorX; }
            set { this.Set<float>(ref this.startVectorX, value, "StartVectorX"); }
        }

        public float DesVectorX
        {
            get { return this.desVectorX; }
            set { this.Set<float>(ref this.desVectorX, value, "DesVectorX"); }
        }
    }

    public class ReleasePokemon : ObservableObject
    {
        private string statType;
        private Sprite[] playerTrainer;

        public string StatType
        {
            get { return this.statType; }
            set { this.Set<string>(ref this.statType, value, "StatType"); }
        }

        public Sprite[] PlayerTrainer
        {
            get { return this.playerTrainer; }
            set { this.Set<Sprite[]>(ref this.playerTrainer, value, "PlayerTrainer"); }
        }
    }

    public class ThrowPokemonBall : ObservableObject
    {
        private string statType;
        private Sprite[] playerTrainer;
        //0表示捕捉，1表示捕捉失败，2表示捕捉成功
        private int catchType;

        public int CatchType
        {
            get { return this.catchType; }
            set { this.Set<int>(ref this.catchType, value, "CatchType"); }
        }

        public string StatType
        {
            get { return this.statType; }
            set { this.Set<string>(ref this.statType, value, "StatType"); }
        }

        public Sprite[] PlayerTrainer
        {
            get { return this.playerTrainer; }
            set { this.Set<Sprite[]>(ref this.playerTrainer, value, "PlayerTrainer"); }
        }
    }

    public class FaintPokemon : ObservableObject
    {
        private string statType;
        public string StatType
        {
            get { return this.statType; }
            set { this.Set<string>(ref this.statType, value, "StatType"); }
        }
    }

    public class ShrinkPokemon : ObservableObject
    {
        private string path;
        private float targetSize;

        public float TargetSize
        {
            get { return this.targetSize; }
            set { this.Set<float>(ref this.targetSize, value, "TargetSize"); }
        }

        public string Path
        {
            get { return this.path; }
            set { this.Set<string>(ref this.path, value, "Path"); }
        }
    }

    public class StretchBar : ObservableObject
    {
        private string path;
        private float targetSize;

        public float TargetSize
        {
            get { return this.targetSize; }
            set { this.Set<float>(ref this.targetSize, value, "TargetSize"); }
        }

        public string Path
        {
            get { return this.path; }
            set { this.Set<string>(ref this.path, value, "Path"); }
        }
    }

    public class AnimateOverlayer : ObservableObject
    {
        private string statType;
        private String animatPath;
        private float time;

        public float Time
        {
            get { return this.time; }
            set { this.Set<float>(ref this.time, value, "Time"); }
        }

        public string AnimatPath
        {
            get { return this.animatPath; }
            set { this.Set<string>(ref this.animatPath, value, "AnimatPath"); }
        }

        public string StatType
        {
            get { return this.statType; }
            set { this.Set<string>(ref this.statType, value, "StatType"); }
        }
    }

    public class WithdrawPokemon : ObservableObject
    {
        private string statType;
        public string StatType
        {
            get { return this.statType; }
            set { this.Set<string>(ref this.statType, value, "StatType"); }
        }
    }

    public class BattleBase : ObservableObject
    {
        private string statType;
        private Sprite back;
        private List<Texture> playerAnimation;

        public string StatType
        {
            get { return this.statType; }
            set { this.Set<string>(ref this.statType, value, "StatType"); }
        }

        public List<Texture> PlayerAnimation
        {
            get { return this.playerAnimation; }
            set { this.Set<List<Texture>>(ref this.playerAnimation, value, "PlayerAnimation"); }
        }

        public Sprite Back
        {
            get { return this.back; }
            set { this.Set<Sprite>(ref this.back, value, "Back"); }
        }
    }

    private Sprite background;
    public Sprite Background
    {
        get { return this.background; }
        set { this.Set<Sprite>(ref this.background, value, "Background"); }
    }

    private Sprite playerShadow;
    public Sprite PlayerShadow
    {
        get { return this.playerShadow; }
        set { this.Set<Sprite>(ref this.playerShadow, value, "PlayerShadow"); }
    }

    private Sprite opponentShadow;
    public Sprite OpponentShadow
    {
        get { return this.opponentShadow; }
        set { this.Set<Sprite>(ref this.opponentShadow, value, "OpponentShadow"); }
    }

    private bool bagButtonActive;
    public bool BagButtonActive
    {
        get { return this.bagButtonActive; }
        set { this.Set<bool>(ref this.bagButtonActive, value, "BagButtonActive"); }
    }

    private bool partyButtonActive;
    public bool PartyButtonActive
    {
        get { return this.partyButtonActive; }
        set { this.Set<bool>(ref this.partyButtonActive, value, "PartyButtonActive"); }
    }

    private bool fightButtonActive;
    public bool FightButtonActive
    {
        get { return this.fightButtonActive; }
        set { this.Set<bool>(ref this.fightButtonActive, value, "FightButtonActive"); }
    }

    private bool runButtonActive;
    public bool RunButtonActive
    {
        get { return this.runButtonActive; }
        set { this.Set<bool>(ref this.runButtonActive, value, "RunButtonActive"); }
    }

    private bool buttonMove0;
    public bool ButtonMove0
    {
        get { return this.buttonMove0; }
        set { this.Set<bool>(ref this.buttonMove0, value, "ButtonMove0"); }
    }

    private bool buttonMove1;
    public bool ButtonMove1
    {
        get { return this.buttonMove1; }
        set { this.Set<bool>(ref this.buttonMove1, value, "ButtonMove1"); }
    }

    private bool buttonMove2;
    public bool ButtonMove2
    {
        get { return this.buttonMove2; }
        set { this.Set<bool>(ref this.buttonMove2, value, "ButtonMove2"); }
    }

    private bool buttonMove3;
    public bool ButtonMove3
    {
        get { return this.buttonMove3; }
        set { this.Set<bool>(ref this.buttonMove3, value, "ButtonMove3"); }
    }

    private bool moveReturnObject;
    public bool MoveReturnObject
    {
        get { return this.moveReturnObject; }
        set { this.Set<bool>(ref this.moveReturnObject, value, "MoveReturnObject"); }
    }

    public void setFourButtonVisible(bool visible)
    {
        BagButtonActive = visible;
        FightButtonActive = visible;
        PartyButtonActive = visible;
        RunButtonActive = visible;
    }

    public void setMoveButtonVisible(bool visible, Pokemon pokemon)
    {
        bool[] moveVisible = new bool[4];
        for (int i = 0; pokemon != null && i < pokemon.getMoveCount(); i++)
        {
            moveVisible[i] = visible;
        }
        ButtonMove0 = moveVisible[0];
        ButtonMove1 = moveVisible[1];
        ButtonMove2 = moveVisible[2];
        ButtonMove3 = moveVisible[3];
    }

    public IEnumerator updateAnimateOverlayer(int position)
    {
        string statType = (position < 3) ? BattleHandler.playerTypes[0] : BattleHandler.playerTypes[1];
        animateOverlayerList[0] = new BattleViewModelInfo.AnimateOverlayer()
        {
            StatType = statType,
            AnimatPath = "overlayHealTex"
        };
        yield return new WaitForSeconds(1f);
    }

    //更新精灵信息
    public IEnumerator updatePokemonStatsDisplay(int position, Pokemon pokemon)
    {
        if (pokemon != null)
        {
            string gender = "";
            if (pokemon.getGender() == Pokemon.Gender.FEMALE)
            {
                gender = "♀";
            }
            else if (pokemon.getGender() == Pokemon.Gender.MALE)
            {
                gender = "♂";
            }
            else
            {
                gender = null;
            }
            Texture statusTexture = null;
            if (pokemon.getStatus() != Pokemon.Status.NONE)
            {
                statusTexture =
                    Resources.Load<Texture>("PCSprites/status" + pokemon.getStatus().ToString());
            }
            string path = (position < 3) ? BattleHandler.playerStatTypes[0] : BattleHandler.playerStatTypes[1];
            battleStatsList[0] = new BattlePartyViewModel.PokeIconInfo()
            {
                Path = path,
                IsActive = true,
                PokemonName = pokemon.getName(),
                Gender = gender,
                Level = "Lv." + pokemon.getLevel(),
                StatusTexture = statusTexture,
                Color = Color.black,
                MaxHp = "" + pokemon.getHP(),
                CurrentHp = "" + pokemon.getCurrentHP(),
            };
        }
        yield return new WaitForSeconds(0.5f);
    }

    //移动精灵状态
    public void slidePokemonStatsDisplay(int position, bool retract, Pokemon[] partys, bool trainerBattle)
    {
        string statType = (position < 3) ? BattleHandler.playerStatTypes[0] : BattleHandler.playerStatTypes[1];
        slideStatsList[0] = new BattleViewModelInfo.SlideStats()
        {
            StatType = statType,
            Retract = retract,
            Partys = partys,
            TrainerBattle = trainerBattle
        };
    }

    //释放精灵
    public IEnumerator releasePokemon(int position, Sprite[] animation)
    {
        string statType = (position < 3) ? BattleHandler.playerTypes[0] : BattleHandler.playerTypes[1];
        releasePokemonList[0] = new BattleViewModelInfo.ReleasePokemon()
        {
            StatType = statType,
            PlayerTrainer = animation,
        };
        yield return null;
    }

    //扔精灵球
    public IEnumerator throwPokemonBall(int position, Sprite[] animation, int catchType, float waitTime)
    {
        string statType = (position < 3) ? BattleHandler.playerTypes[0] : BattleHandler.playerTypes[1];
        throwPokemonBallList[0] = new BattleViewModelInfo.ThrowPokemonBall()
        {
            StatType = statType,
            PlayerTrainer = animation,
            CatchType = catchType
        };
        yield return new WaitForSeconds(waitTime);
    }

    //设置BattleBase
    public IEnumerator setBattleBase(int position, List<Texture> animation, float waitTime)
    {
        string statType = (position < 3) ? BattleHandler.playerTypes[0] : BattleHandler.playerTypes[1];
        battleBaseList[0] = new BattleViewModelInfo.BattleBase()
        {
            StatType = statType,
            //Back= back
            PlayerAnimation = animation
        };
        yield return new WaitForSeconds(waitTime);
    }

    //移动人物
    public void slidePokemon(int position, bool trainerBattle, float startVecX, float desVecX,
        Sprite[] playerTrainer)
    {
        if (position == 0)
        {
            slidePokemonList[0] = new BattleViewModelInfo.SlidePokemon()
            {
                StatType = BattleHandler.playerTypes[0],
                StartVectorX = startVecX,
                DesVectorX = desVecX,
                ShowTrainer = true,
                PlayerTrainer = playerTrainer,
            };
        }
        else
        {
            slidePokemonList[0] = new BattleViewModelInfo.SlidePokemon()
            {
                StatType = BattleHandler.playerTypes[1],
                StartVectorX = startVecX,
                DesVectorX = desVecX,
                ShowTrainer = trainerBattle,
            };
        }
    }

    public IEnumerator shrinkPokemon(string path, float targetSize, float waitTime)
    {
        shrinkPokemonList[0] = new BattleViewModelInfo.ShrinkPokemon()
        {
            Path = path,
            TargetSize = targetSize,
        };
        yield return new WaitForSeconds(waitTime);
    }


    public IEnumerator setStretchBar(string path, float targetSize, float waitTime)
    {
        stretchBarList[0] = new BattleViewModelInfo.StretchBar()
        {
            Path = path,
            TargetSize = targetSize,
        };
        yield return new WaitForSeconds(waitTime);
    }

    public IEnumerator setPokemonFaint(int position)
    {
        if (position == 0)
        {
            fainPokemonList[0] = new BattleViewModelInfo.FaintPokemon()
            {
                StatType = BattleHandler.playerTypes[0],
            };
            slidePokemonStatsDisplay(position, false, null, false);
        }
        else
        {
            fainPokemonList[0] = new BattleViewModelInfo.FaintPokemon()
            {
                StatType = BattleHandler.playerTypes[1],
            };
            slidePokemonStatsDisplay(position, true, null, false);
        }
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator withdrawPokemon(int position)
    {
        string statType = (position < 3) ? BattleHandler.playerTypes[0] : BattleHandler.playerTypes[1];
        withdrawPokemonList[0] = new WithdrawPokemon()
        {
            StatType = statType,
        };
        yield return new WaitForSeconds(1f);
    }

    //动效动画
    public IEnumerator animateOverlayer(int position, string animatePath, float time)
    {
        string statType = (position < 3) ? BattleHandler.playerTypes[0] : BattleHandler.playerTypes[1];
        animateOverlayerList[0] = new BattleViewModelInfo.AnimateOverlayer()
        {
            StatType = statType,
            AnimatPath = animatePath,
            Time = time
        };
        yield return null;
    }

    //更新技能显示按钮
    public void updateMovesetDisplay(List<SkillInfo> skillInfos)
    {
        for (int i = 0; i < skillInfos.Count; i++)
        {
            Sprite buttonMoveCover = null;
            Sprite buttonMoveType = null;
            string buttonMoveName = "";
            string buttonMovePP = "";
            if (skillInfos[i].skillId > 0)
            {
                PokemonData.Type type = MoveDatabase.getMove(skillInfos[i].skillId).getType();
                if (type == PokemonData.Type.BUG)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillbug" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.DARK)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skilldark" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.DRAGON)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skilldragon" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.ELECTRIC)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillelec" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.FAIRY)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillfairy" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.FIGHTING)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillfight" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.FIRE)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillfire" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.FLYING)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillfly" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.GHOST)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillghost" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.GRASS)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillgrass" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.GROUND)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillground" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.ICE)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillice" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.NORMAL)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillnormal" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.POISON)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillposion" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.PSYCHIC)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillpsychic" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.ROCK)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillrock" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.STEEL)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillsteel" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                else if (type == PokemonData.Type.WATER)
                {
                    buttonMoveCover = Resources.Load<Sprite>("Sprites/GUI/Battle/skillwater" + (i % 2));
                    buttonMoveType = Resources.Load<Sprite>("Sprites/GUI/Types/type" + type.ToString());
                }
                buttonMoveName = MoveDatabase.getMoveName(skillInfos[i].skillId);
                buttonMovePP = skillInfos[i].curPp + "/" + skillInfos[i].maxPp;
            }
            else
            {
                buttonMoveType = Resources.Load<Sprite>("null");
                buttonMoveName = "";
                buttonMovePP = "";
            }
            moveStatsList[0] = new BattleViewModelInfo.MoveStats()
            {
                Index = i,
                ButtonMoveName = buttonMoveName,
                ButtonMovePP = buttonMovePP,
                ButtonMoveCover = buttonMoveCover,
                ButtonMoveType = buttonMoveType
            };
        }
    }

    public bool calculateCritical(int attackerPosition, int targetPosition, MoveData move)
    {
        int attackerCriticalRatio = 0;
        if (move.hasMoveEffect(MoveData.Effect.Critical))
        {
            attackerCriticalRatio += 1;
        }

        bool applyCritical = false;
        if (move.getCategory() != MoveData.Category.STATUS)
        {
            if (attackerCriticalRatio == 0)
            {
                if (Random.value <= 0.0625)
                {
                    applyCritical = true;
                }
            }
            else if (attackerCriticalRatio == 1)
            {
                if (Random.value <= 0.125)
                {
                    applyCritical = true;
                }
            }
            else if (attackerCriticalRatio == 2)
            {
                if (Random.value <= 0.5)
                {
                    applyCritical = true;
                }
            }
            else if (attackerCriticalRatio > 2)
            {
                applyCritical = true;
            }
        }

        return applyCritical;
    }

    public float getSuperEffectiveModifier(PokemonData.Type attackingType, PokemonData.Type targetType)
    {
        if (attackingType == PokemonData.Type.BUG)
        {
            if (targetType == PokemonData.Type.DARK || targetType == PokemonData.Type.GRASS ||
                targetType == PokemonData.Type.PSYCHIC)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.FAIRY || targetType == PokemonData.Type.FIGHTING ||
                 targetType == PokemonData.Type.FIRE || targetType == PokemonData.Type.FLYING ||
                 targetType == PokemonData.Type.GHOST || targetType == PokemonData.Type.POISON ||
                 targetType == PokemonData.Type.STEEL)
            {
                return 0.5f;
            }
        }
        else if (attackingType == PokemonData.Type.DARK)
        {
            if (targetType == PokemonData.Type.GHOST || targetType == PokemonData.Type.PSYCHIC)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.DARK || targetType == PokemonData.Type.FAIRY ||
                 targetType == PokemonData.Type.FIGHTING)
            {
                return 0.5f;
            }
        }
        else if (attackingType == PokemonData.Type.DRAGON)
        {
            if (targetType == PokemonData.Type.DRAGON)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.STEEL)
            {
                return 0.5f;
            }
            else if (targetType == PokemonData.Type.FAIRY)
            {
                return 0f;
            }
        }
        else if (attackingType == PokemonData.Type.ELECTRIC)
        {
            if (targetType == PokemonData.Type.FLYING || targetType == PokemonData.Type.WATER)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.DRAGON || targetType == PokemonData.Type.ELECTRIC ||
                 targetType == PokemonData.Type.GRASS)
            {
                return 0.5f;
            }
            else if (targetType == PokemonData.Type.GROUND)
            {
                return 0f;
            }
        }
        else if (attackingType == PokemonData.Type.FAIRY)
        {
            if (targetType == PokemonData.Type.DARK || targetType == PokemonData.Type.DRAGON ||
                targetType == PokemonData.Type.FIGHTING)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.FIRE || targetType == PokemonData.Type.POISON ||
                 targetType == PokemonData.Type.STEEL)
            {
                return 0.5f;
            }
        }
        else if (attackingType == PokemonData.Type.FIGHTING)
        {
            if (targetType == PokemonData.Type.DARK || targetType == PokemonData.Type.ICE ||
                targetType == PokemonData.Type.NORMAL || targetType == PokemonData.Type.ROCK ||
                targetType == PokemonData.Type.STEEL)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.BUG || targetType == PokemonData.Type.FAIRY ||
                 targetType == PokemonData.Type.FLYING || targetType == PokemonData.Type.POISON ||
                 targetType == PokemonData.Type.PSYCHIC)
            {
                return 0.5f;
            }
            else if (targetType == PokemonData.Type.GHOST)
            {
                return 0f;
            }
        }
        else if (attackingType == PokemonData.Type.FIRE)
        {
            if (targetType == PokemonData.Type.BUG || targetType == PokemonData.Type.GRASS ||
                targetType == PokemonData.Type.ICE || targetType == PokemonData.Type.STEEL)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.DRAGON || targetType == PokemonData.Type.FIRE ||
                 targetType == PokemonData.Type.ROCK || targetType == PokemonData.Type.WATER)
            {
                return 0.5f;
            }
        }
        else if (attackingType == PokemonData.Type.FLYING)
        {
            if (targetType == PokemonData.Type.BUG || targetType == PokemonData.Type.FIGHTING ||
                targetType == PokemonData.Type.GRASS)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.ELECTRIC || targetType == PokemonData.Type.ROCK ||
                 targetType == PokemonData.Type.STEEL)
            {
                return 0.5f;
            }
        }
        else if (attackingType == PokemonData.Type.GHOST)
        {
            if (targetType == PokemonData.Type.GHOST || targetType == PokemonData.Type.PSYCHIC)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.DARK)
            {
                return 0.5f;
            }
            else if (targetType == PokemonData.Type.NORMAL)
            {
                return 0f;
            }
        }
        else if (attackingType == PokemonData.Type.GRASS)
        {
            if (targetType == PokemonData.Type.GROUND || targetType == PokemonData.Type.ROCK ||
                targetType == PokemonData.Type.WATER)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.BUG || targetType == PokemonData.Type.DRAGON ||
                 targetType == PokemonData.Type.FIRE || targetType == PokemonData.Type.FLYING ||
                 targetType == PokemonData.Type.GRASS || targetType == PokemonData.Type.POISON ||
                 targetType == PokemonData.Type.STEEL)
            {
                return 0.5f;
            }
        }
        else if (attackingType == PokemonData.Type.GROUND)
        {
            if (targetType == PokemonData.Type.ELECTRIC || targetType == PokemonData.Type.FIRE ||
                targetType == PokemonData.Type.POISON || targetType == PokemonData.Type.ROCK ||
                targetType == PokemonData.Type.STEEL)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.BUG || targetType == PokemonData.Type.GRASS)
            {
                return 0.5f;
            }
            else if (targetType == PokemonData.Type.FLYING)
            {
                return 0f;
            }
        }
        else if (attackingType == PokemonData.Type.ICE)
        {
            if (targetType == PokemonData.Type.DRAGON || targetType == PokemonData.Type.FLYING ||
                targetType == PokemonData.Type.GRASS || targetType == PokemonData.Type.GROUND)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.FIRE || targetType == PokemonData.Type.ICE ||
                 targetType == PokemonData.Type.STEEL || targetType == PokemonData.Type.WATER)
            {
                return 0.5f;
            }
        }
        else if (attackingType == PokemonData.Type.NORMAL)
        {
            if (targetType == PokemonData.Type.ROCK || targetType == PokemonData.Type.STEEL)
            {
                return 0.5f;
            }
            else if (targetType == PokemonData.Type.GHOST)
            {
                return 0f;
            }
        }
        else if (attackingType == PokemonData.Type.POISON)
        {
            if (targetType == PokemonData.Type.FAIRY || targetType == PokemonData.Type.GRASS)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.POISON || targetType == PokemonData.Type.GROUND ||
                 targetType == PokemonData.Type.ROCK || targetType == PokemonData.Type.GHOST)
            {
                return 0.5f;
            }
            else if (targetType == PokemonData.Type.STEEL)
            {
                return 0f;
            }
        }
        else if (attackingType == PokemonData.Type.PSYCHIC)
        {
            if (targetType == PokemonData.Type.FIGHTING || targetType == PokemonData.Type.POISON)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.PSYCHIC || targetType == PokemonData.Type.STEEL)
            {
                return 0.5f;
            }
            else if (targetType == PokemonData.Type.DARK)
            {
                return 0f;
            }
        }
        else if (attackingType == PokemonData.Type.ROCK)
        {
            if (targetType == PokemonData.Type.BUG || targetType == PokemonData.Type.FIRE ||
                targetType == PokemonData.Type.FLYING || targetType == PokemonData.Type.ICE)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.FIGHTING || targetType == PokemonData.Type.GROUND ||
                 targetType == PokemonData.Type.STEEL)
            {
                return 0.5f;
            }
        }
        else if (attackingType == PokemonData.Type.STEEL)
        {
            if (targetType == PokemonData.Type.FAIRY || targetType == PokemonData.Type.ICE ||
                targetType == PokemonData.Type.ROCK)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.ELECTRIC || targetType == PokemonData.Type.FIRE ||
                 targetType == PokemonData.Type.STEEL || targetType == PokemonData.Type.WATER)
            {
                return 0.5f;
            }
        }
        else if (attackingType == PokemonData.Type.WATER)
        {
            if (targetType == PokemonData.Type.FIRE || targetType == PokemonData.Type.GROUND ||
                targetType == PokemonData.Type.ROCK)
            {
                return 2f;
            }
            else if (targetType == PokemonData.Type.DRAGON || targetType == PokemonData.Type.GRASS ||
                 targetType == PokemonData.Type.WATER)
            {
                return 0.5f;
            }
        }
        return 1f;
    }

    //修改战斗时努力值参数
    public bool changeStatsMod(int type, int pokemonPos, int parameter)
    {
        if (pokemonStatsMod[type][pokemonPos] >= 6 && parameter > 0)
        {
            return false;
        }
        if (pokemonStatsMod[type][pokemonPos] <= -6 && parameter < 0)
        {
            return false;
        }
        pokemonStatsMod[type][pokemonPos] += parameter;
        if (pokemonStatsMod[type][pokemonPos] > 6)
        {
            pokemonStatsMod[type][pokemonPos] = 6;
        }
        else if (pokemonStatsMod[type][pokemonPos] < -6)
        {
            pokemonStatsMod[type][pokemonPos] = -6;
        }
        return true;
    }

    //计算努力值参数
    public float calculateStatModifier(int type, int pokemonPos)
    {
        int modifier = pokemonStatsMod[type][pokemonPos];
        if (modifier > 0)
        {
            return ((2f + (float)modifier) / 2f);
        }
        else if (modifier < 0)
        {
            return (2f / (2f + Mathf.Abs((float)modifier)));
        }
        return 1f;
    }

    //计算命中精度
    public float calculateAccuracyModifier(int type, int pokemonPos)
    {
        int modifier = pokemonStatsMod[type][pokemonPos];
        if (modifier > 0)
        {
            return ((3f + modifier) / 3f);
        }
        else if (modifier < 0)
        {
            return (3f / (3f + modifier));
        }
        return 1f;
    }

    //计算伤害
    public float calculateDamage(Pokemon attacker, Pokemon target, MoveData move)
    {
        float baseDamage = 0;
        if (move.getCategory() == MoveData.Category.PHYSICAL)
        {
            baseDamage = ((2f * (float)attacker.getLevel() + 10f) / 250f) *
                     ((float)attacker.getATK() / (float)target.getDEF()) *
                     (float)move.getPower() + 2f;
        }
        else if (move.getCategory() == MoveData.Category.SPECIAL)
        {
            baseDamage = ((2f * (float)attacker.getLevel() + 10f) / 250f) *
                     ((float)attacker.getSPA() / (float)target.getSPD()) *
                     (float)move.getPower() + 2f;
        }

        baseDamage *= Random.Range(0.85f, 1f);
        return baseDamage;
    }

}
