using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Loxodon.Framework.Views;
using Loxodon.Framework.Views.Variables;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Binding.Builder;
using System.Collections.Specialized;
using System.Collections.Generic;
using System;

public class BattleHandler : UIView
{
    public VariableArray variables;
    private BattleViewModel viewModel;
    private BattlePartyViewModel partyViewModel;

    //private AudioSource BattleAudio;
    public AudioClip defaultTrainerBGM;
    public int defaultTrainerBGMLoopStart = 577000;

    public AudioClip defaultTrainerVictoryBGM;
    public int defaultTrainerVictoryBGMLoopStart = 79000;

    public AudioClip defaultWildBGM;
    public int defaultWildBGMLoopStart = 578748;

    public AudioClip defaultWildVictoryBGM;
    public int defaultWildVictoryBGMLoopStart = 65000;

    public AudioClip
        scrollClip,
        selectClip,
        runClip,
        statUpClip,
        statDownClip,
        healFieldClip,
        fillExpClip,
        expFullClip,
        pokeballOpenClip,
        pokeballBounceClip,
        faintClip,
        hitClip,
        hitSuperClip,
        hitPoorClip;

    public Sprite
        partySpaceTex,
        partyBallTex,
        partyStatusTex,
        partyFaintTex;

    public Texture overlayHealTex, overlayStatUpTex, overlayStatDownTex;
    //精灵
    public static string[] playerTypes = new string[] { "Player0", "Opponent0" };
    private Dictionary<string, Image> trainerImageDic = new Dictionary<string, Image>();
    //记录trainer战斗时的位置
    private Dictionary<string, Vector3> trainerLocalVecDic = new Dictionary<string, Vector3>();
    private Dictionary<string, Image> platformDic = new Dictionary<string, Image>();
    //记录platform战斗时的位置
    private Dictionary<string, Vector3> platformLocalVecDic = new Dictionary<string, Vector3>();
    private Dictionary<string, GameObject> pokemonDic = new Dictionary<string, GameObject>();
    private Dictionary<string, Image> maskDic = new Dictionary<string, Image>();
    private Dictionary<string, RawImage> spriteDic = new Dictionary<string, RawImage>();
    private Dictionary<string, Vector3> spriteVecDic = new Dictionary<string, Vector3>();
    private Dictionary<string, List<Texture>> animateDic = new Dictionary<string, List<Texture>>();
    private Dictionary<string, RawImage> overLayDic = new Dictionary<string, RawImage>();
    private Dictionary<string, RawImage> pokeBallDic = new Dictionary<string, RawImage>();
    //精灵状态栏
    public static string[] playerStatTypes = new string[] { "PlayerStats0", "OpponentStats0" };
    private Dictionary<string, GameObject> pokemonStatsDisplayDic = new Dictionary<string, GameObject>();
    private Dictionary<string, RectTransform> pokemonStatstransformDic = new Dictionary<string, RectTransform>();
    private Dictionary<string, float> pokemonStatsDisplayDicX = new Dictionary<string, float>();
    private Dictionary<string, Image> expBarDic = new Dictionary<string, Image>();
    private Dictionary<string, Image[]> spaceDic = new Dictionary<string, Image[]>();
    //血条Bar
    public static string[] playerHpBars = new string[] { "PlayerStats0/HPBarBack/HPBar", "OpponentStats0/HPBarBack/HPBar" };
    //经验Bar
    public static string expBarPath = "PlayerStats0/ExpBar/Bar";
    //覆盖层动画
    public static string overlayStatUpPath = "Sprites/GUI/Battle/OverlayStatUp";

    //天气效果
    private enum WeatherEffect
    {
        NONE,
        RAIN,
        SUN,
        SAND,
        HAIL,
        HEAVYRAIN,
        HEAVYSUN,
        STRONGWINDS
    }

    //地形效果
    private enum TerrainEffect
    {
        NONE,
        ELECTRIC,
        GRASSY,
        MISTY
    }

    //状态栏滑动改变
    protected void OnSlideStatsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BattleViewModelInfo.SlideStats item in eventArgs.NewItems)
                {
                    if (item.StatType != null)
                    {
                        StartCoroutine(slidePokemonStats(item.StatType, item.Retract, item.Partys, item.TrainerBattle));
                    }
                }
                break;
        }
    }

    //设置Battle主界面精灵动画
    protected void OnBattleBaseChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BattleViewModelInfo.BattleBase item in eventArgs.NewItems)
                {
                    if (item.StatType != null)
                    {
                        if (item.PlayerAnimation != null)
                        {
                            animatePokemon(item.StatType, item.PlayerAnimation);
                        }
                    }
                }
                break;
        }
    }

    //Battle主界面技能状态
    protected void OnMoveStatsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BattleViewModelInfo.MoveStats item in eventArgs.NewItems)
                {
                    if (item.Index >= 0)
                    {
                        string path = "OptionBox/Move" + item.Index;
                        Image buttonMoveType = transform.Find(path + "/Type").GetComponent<Image>();
                        buttonMoveType.sprite = item.ButtonMoveType;
                        Text buttonMoveName = transform.Find(path + "/Name").GetComponent<Text>();
                        Text buttonMoveNameShadow = transform.Find(path + "/Name/Shadow").GetComponent<Text>();
                        buttonMoveName.text = item.ButtonMoveName;
                        buttonMoveNameShadow.text = item.ButtonMoveName;
                        Text buttonMovePP = transform.Find(path + "/PP").GetComponent<Text>();
                        buttonMovePP.text = item.ButtonMovePP;
                        Image buttonMoveCover = transform.Find(path + "/Cover").GetComponent<Image>();
                        buttonMoveCover.sprite = item.ButtonMoveCover;
                    }
                }
                break;
        }
    }


    //收回精灵
    protected void OnWithdrawPokemonChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BattleViewModelInfo.WithdrawPokemon item in eventArgs.NewItems)
                {
                    if (item.StatType != null)
                    {
                        StartCoroutine(withdrawPokemon(item.StatType));
                    }
                }
                break;
        }
    }

    //扔释放精灵
    protected void OnReleasePokemonChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BattleViewModelInfo.ReleasePokemon item in eventArgs.NewItems)
                {
                    if (item.StatType != null)
                    {
                        StartCoroutine(onReleasePokemon(item.StatType, item.PlayerTrainer));
                    }
                }
                break;
        }
    }

    //释放精灵
    private IEnumerator onReleasePokemon(string statType, Sprite[] throwAnim)
    {
        if (throwAnim != null)
        {
            yield return StartCoroutine(animatePlayerThrow(statType, throwAnim));
        }
        yield return StartCoroutine(releasePokemon(statType));
    }

    //扔宝可梦球
    protected void OnThrowPokemonBallChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BattleViewModelInfo.ThrowPokemonBall item in eventArgs.NewItems)
                {
                    if (item.StatType != null)
                    {
                        if (item.CatchType == 0)
                        {
                            StartCoroutine(onThrowPokemonBall(item.StatType, item.PlayerTrainer));
                        }
                        else if (item.CatchType == 1)
                        {
                            StartCoroutine(outOfPokemonBall(item.StatType));
                        }
                    }
                }
                break;
        }
    }

    private IEnumerator onThrowPokemonBall(string statType, Sprite[] throwAnim)
    {
        if (throwAnim != null)
        {
            yield return StartCoroutine(animatePlayerThrow(statType, throwAnim));
        }
        yield return StartCoroutine(throwPokemonBall(statType));
    }

    //滑动人物
    protected void OnSlidePokemonChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Replace:
                foreach (BattleViewModelInfo.SlidePokemon item in eventArgs.NewItems)
                {
                    if (item.StatType != null)
                    {
                        StartCoroutine(slidePokemon(item.StatType, item.ShowTrainer, item.StartVectorX,
                            item.DesVectorX, item.PlayerTrainer));
                    }
                }
                break;
        }
    }

    //精灵死亡时
    protected void OnFaintPokemonChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BattleViewModelInfo.FaintPokemon item in eventArgs.NewItems)
                {
                    if (item.StatType != null)
                    {
                        StartCoroutine(faintPokemonAnimation(item.StatType));
                    }
                }
                break;
        }
    }

    //进度条修改
    protected void OnStretchBarChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BattleViewModelInfo.StretchBar item in eventArgs.NewItems)
                {
                    if (item.Path != null)
                    {
                        StartCoroutine(stretchBar(item.Path, item.TargetSize));
                    }
                }
                break;
        }
    }

    //战斗特效动画
    protected void OnAnimateOverlayer(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BattleViewModelInfo.AnimateOverlayer item in eventArgs.NewItems)
                {
                    if (item.StatType != null)
                    {
                        StartCoroutine(animateOverlayer(item, 0, 0, 0));
                    }
                }
                break;
        }
    }

    //精灵Stats数值信息
    protected void OnPokeInfoChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (BattlePartyViewModel.PokeIconInfo item in eventArgs.NewItems)
                {
                    StartCoroutine(updatePokeIconInfo(item));
                }
                break;
        }
    }

    protected override void Awake()
    {
        BattleViewModelInfo.NotifyCollectionChangedObject changedObject = new BattleViewModelInfo.NotifyCollectionChangedObject();
        changedObject.battleBaseHandler = OnBattleBaseChanged;
        changedObject.battleStatsHandler = OnPokeInfoChanged;
        changedObject.slideStatsHandler = OnSlideStatsChanged;
        changedObject.moveStatsHandler = OnMoveStatsChanged;
        changedObject.withdrawPokemonHandler = OnWithdrawPokemonChanged;
        changedObject.slidePokemonHandler = OnSlidePokemonChanged;
        changedObject.releasePokemonHandler = OnReleasePokemonChanged;
        changedObject.throwPokemonBallHandler = OnThrowPokemonBallChanged;
        changedObject.faintPokemonHandler = OnFaintPokemonChanged;
        changedObject.stretchBarHandler = OnStretchBarChanged;
        changedObject.animateOverlayerHandler = OnAnimateOverlayer;

        BattleViewModelInfo.AudioInfoObject audioInfoObject = new BattleViewModelInfo.AudioInfoObject();
        audioInfoObject.runClip = runClip;
        audioInfoObject.scrollClip = scrollClip;
        audioInfoObject.selectClip = selectClip;
        audioInfoObject.runClip = runClip;
        audioInfoObject.statUpClip = statUpClip;
        audioInfoObject.statDownClip = statDownClip;
        audioInfoObject.healFieldClip = healFieldClip;
        audioInfoObject.fillExpClip = fillExpClip;
        audioInfoObject.expFullClip = expFullClip;
        audioInfoObject.pokeballOpenClip = pokeballOpenClip;
        audioInfoObject.pokeballBounceClip = pokeballBounceClip;
        audioInfoObject.faintClip = faintClip;
        audioInfoObject.hitClip = hitClip;
        audioInfoObject.hitSuperClip = hitSuperClip;
        audioInfoObject.hitPoorClip = hitPoorClip;

        viewModel = new BattleViewModel(changedObject, audioInfoObject);
        partyViewModel = new BattlePartyViewModel(this, OnPokeInfoChanged);
        partyViewModel.battleViewModel = viewModel;
        viewModel.partyViewModel = partyViewModel;

        foreach (string statType in playerStatTypes)
        {
            pokemonStatsDisplayDic[statType] = transform.Find(statType).GetComponent<RawImage>().gameObject;
            pokemonStatstransformDic[statType] = pokemonStatsDisplayDic[statType].GetComponent<RectTransform>();
            pokemonStatsDisplayDicX[statType] = (statType == playerStatTypes[0]) ? Screen.width + pokemonStatstransformDic[statType].sizeDelta.x
                : -pokemonStatstransformDic[statType].sizeDelta.x;
            expBarDic[statType] = transform.Find(statType + "/Party/Bar").GetComponent<Image>();
            Image[] space = new Image[6];
            for (int i = 0; i < 6; i++)
            {
                space[i] = transform.Find(statType + "/Party/Space" + i).GetComponent<Image>();
            }
            spaceDic[statType] = space;
        }
        foreach (string statType in playerTypes)
        {
            platformDic[statType] = transform.Find(statType).GetComponent<Image>();
            trainerImageDic[statType] = transform.Find(statType + "/Trainer").GetComponent<Image>();
            trainerLocalVecDic[statType] = trainerImageDic[statType].GetComponent<RectTransform>().localPosition;
            pokemonDic[statType] = transform.Find(statType + "/Pokemon").gameObject;
            maskDic[statType] = transform.Find(statType + "/Pokemon/Mask").GetComponent<Image>();
            spriteDic[statType] = transform.Find(statType + "/Pokemon/Mask/Sprite").GetComponent<RawImage>();
            spriteVecDic[statType] = spriteDic[statType].GetComponent<RectTransform>().localPosition;
            animateDic[statType] = new List<Texture>();
            overLayDic[statType] = transform.Find(statType + "/Pokemon/Mask/Sprite/Overlay").GetComponent<RawImage>();
            pokeBallDic[statType] = transform.Find(statType + "/PokeBall").GetComponent<RawImage>();
        }
        //BattleAudio = transform.GetComponent<AudioSource>();
    }

    protected override void Start()
    {
        IBindingContext bindingContext = this.BindingContext();
        bindingContext.DataContext = viewModel;

        BindingSet<BattleHandler, BattleViewModel> bindingSet = this.CreateBindingSet<BattleHandler, BattleViewModel>();
        bindingSet.Bind(variables.Get<GameObject>("partyObject"))
            .For(v => v.activeSelf).To(vm => vm.partyViewModel.PartyObject);
        for (int i = 0; i < 6; i++)
        {
            bindingSet.Bind(variables.Get<Button>("slot" + i))
                 .For(v => v.onClick).To(vm => vm.partyViewModel.OnPartyButton).CommandParameter(i);
        }

        bindingSet.Bind(variables.Get<Image>("background"))
            .For(v => v.sprite).To(vm => vm.viewModelInfo.Background);
        bindingSet.Bind(variables.Get<Image>("playerShadow"))
            .For(v => v.sprite).To(vm => vm.viewModelInfo.PlayerShadow);
        bindingSet.Bind(variables.Get<Image>("opponentShadow"))
            .For(v => v.sprite).To(vm => vm.viewModelInfo.OpponentShadow);
        bindingSet.Bind(variables.Get<Button>("buttonFight"))
             .For(v => v.onClick).To(vm => vm.OnButtonFight);
        bindingSet.Bind(variables.Get<GameObject>("buttonFightObject"))
            .For(v => v.activeSelf).To(vm => vm.viewModelInfo.FightButtonActive);
        bindingSet.Bind(variables.Get<Button>("buttonBag"))
             .For(v => v.onClick).To(vm => vm.OnButtonBag);
        bindingSet.Bind(variables.Get<GameObject>("buttonBagObject"))
            .For(v => v.activeSelf).To(vm => vm.viewModelInfo.BagButtonActive);
        bindingSet.Bind(variables.Get<Button>("buttonRun"))
             .For(v => v.onClick).To(vm => vm.OnButtonRun);
        bindingSet.Bind(variables.Get<GameObject>("buttonRunObject"))
            .For(v => v.activeSelf).To(vm => vm.viewModelInfo.RunButtonActive);
        bindingSet.Bind(variables.Get<Button>("buttonParty"))
            .For(v => v.onClick).To(vm => vm.OnButtonParty);
        bindingSet.Bind(variables.Get<GameObject>("buttonPartyObject"))
            .For(v => v.activeSelf).To(vm => vm.viewModelInfo.PartyButtonActive);
        bindingSet.Bind(variables.Get<Button>("moveReturn"))
            .For(v => v.onClick).To(vm => vm.OnTaskReturn);
        bindingSet.Bind(variables.Get<GameObject>("moveReturnObject"))
           .For(v => v.activeSelf).To(vm => vm.viewModelInfo.MoveReturnObject);
        bindingSet.Bind(variables.Get<GameObject>("move1"))
            .For(v => v.activeSelf).To(vm => vm.viewModelInfo.ButtonMove0);
        bindingSet.Bind(variables.Get<Button>("buttonMove1"))
            .For(v => v.onClick).To(vm => vm.OnMoveButton).CommandParameter(0);
        bindingSet.Bind(variables.Get<GameObject>("move2"))
            .For(v => v.activeSelf).To(vm => vm.viewModelInfo.ButtonMove1);
        bindingSet.Bind(variables.Get<Button>("buttonMove2"))
            .For(v => v.onClick).To(vm => vm.OnMoveButton).CommandParameter(1);
        bindingSet.Bind(variables.Get<GameObject>("move3"))
            .For(v => v.activeSelf).To(vm => vm.viewModelInfo.ButtonMove2);
        bindingSet.Bind(variables.Get<Button>("buttonMove3"))
            .For(v => v.onClick).To(vm => vm.OnMoveButton).CommandParameter(2);
        bindingSet.Bind(variables.Get<GameObject>("move4"))
            .For(v => v.activeSelf).To(vm => vm.viewModelInfo.ButtonMove3);
        bindingSet.Bind(variables.Get<Button>("buttonMove4"))
            .For(v => v.onClick).To(vm => vm.OnMoveButton).CommandParameter(3);
        bindingSet.Build();
        gameObject.SetActive(false);
    }

    public bool isAllOpponentsDefeated()
    {
        return viewModel.allOpponentsDefeated;
    }

    public bool isAllPlayersDefeated()
    {
        return viewModel.allPlayersDefeated;
    }

    /// 野生精灵战斗
    public IEnumerator control(Pokemon wildPokemon)
    {
        yield return StartCoroutine(control(false, new Trainer(new Pokemon[] { wildPokemon }), false, false));
    }

    /// 战斗NPC对战
    public IEnumerator control(Trainer trainer)
    {
        yield return StartCoroutine(control(true, trainer, false, false));
    }
    public IEnumerator control(Trainer trainer, bool doubles)
    {
        yield return StartCoroutine(control(true, trainer, false, doubles));
    }

    public IEnumerator control(bool isTrainerBattle, Trainer trainer, bool healedOnDefeat, bool doubles)
    {
        Scene.main.SetMapButtonVisible(false);
        StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
        StartCoroutine(doControl(isTrainerBattle, trainer, healedOnDefeat, doubles));
        yield return new WaitForSeconds(1.5f);
    }

    //真正的处理Control
    public IEnumerator doControl(bool isTrainerBattle, Trainer trainer, bool healedOnDefeat, bool doubles)
    {
        //用来比较战斗后的经验变化
        int[] initialLevels = new int[6];
        for (int i = 0; i < initialLevels.Length; i++)
        {
            Pokemon pokemon = PCManager.Instance.getPartyBox()[i];
            if (pokemon != null)
            {
                //获取等级
                initialLevels[i] = pokemon.getLevel();
            }
        }
        if (isTrainerBattle)
        {
            trainerImageDic[playerTypes[1]].sprite = trainer.GetSprites()[0];
        }
        //重置场景
        platformDic[playerTypes[0]].transform.position += new Vector3(-10000, 0, 0);
        platformDic[playerTypes[1]].transform.position += new Vector3(-10000, 0, 0);
        spriteDic[playerTypes[0]].GetComponent<RectTransform>().localPosition = spriteVecDic[playerTypes[0]];
        spriteDic[playerTypes[1]].GetComponent<RectTransform>().localPosition = spriteVecDic[playerTypes[1]];
        pokeBallDic[playerTypes[0]].gameObject.SetActive(false);

        BgmHandler.main.PlayOverlay(defaultWildBGM, defaultWildBGMLoopStart);
        yield return viewModel.reset(isTrainerBattle, trainer);
        while (viewModel.running)
        {
            //循环处理交替作战场景
            yield return viewModel.doControl(isTrainerBattle, trainer);
        }
        yield return new WaitForSeconds(1f);
        //检查是否可以进化
        if (healedOnDefeat)
        {
            for (int i = 0; i < initialLevels.Length; i++)
            {
                Pokemon pokemon = PCManager.Instance.getPartyBox()[i];
                if (pokemon != null)
                {
                    if (pokemon.getLevel() != initialLevels[i])
                    {
                        //if can evolve
                        if (pokemon.canEvolve("Level"))
                        {
                            BgmHandler.main.PlayOverlay(null, 0, 0);
                            Scene.main.Evolution.gameObject.SetActive(true);
                            StartCoroutine(Scene.main.Evolution.control(pokemon, "Level"));
                            while (Scene.main.Evolution.gameObject.activeSelf)
                            {
                                yield return null;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            //todo Respawn
            //GlobalVariables.global.Respawn();
        }

        yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
        yield return StartCoroutine(slidePokemonStats(playerStatTypes[0], false, null, false));
        yield return StartCoroutine(slidePokemonStats(playerStatTypes[1], true, null, false));
        GlobalVariables.global.resetFollower();
        BgmHandler.main.Pause();
        this.gameObject.SetActive(false);
    }

    //训练师滑动动画
    private IEnumerator slidePokemon(string statType, bool showTrainer,
        float startPositionX, float destinationPositionX, Sprite[] throwAnim)
    {
        if (showTrainer)
        {
            if (throwAnim != null && throwAnim.Length > 0)
            {
                trainerImageDic[statType].sprite = throwAnim[0];
            }
            trainerImageDic[statType].gameObject.SetActive(true);
            pokemonDic[statType].gameObject.SetActive(false);
        }
        else
        {
            trainerImageDic[statType].gameObject.SetActive(false);
            pokemonDic[statType].gameObject.SetActive(true);
        }
        //隐藏技能效果
        overLayDic[statType].gameObject.SetActive(false);

        float destinationPositionY = platformDic[statType].GetComponent<RectTransform>().localPosition.y;
        Vector3 startPosition = new Vector3(startPositionX, destinationPositionY, 0);
        Vector3 destinationPosition = new Vector3(destinationPositionX, destinationPositionY, 0);
        Vector3 distance = destinationPosition - startPosition;

        float increment = 0;
        float internalTime = Math.Abs(Screen.width / (distance.x * 350));
        while (increment < 1)
        {
            increment += internalTime;
            if (increment > 1)
            {
                increment = 1f;
            }
            platformDic[statType].GetComponent<RectTransform>().localPosition = startPosition + increment * distance;
            yield return new WaitForFixedUpdate();
        }
        platformLocalVecDic[statType] = platformDic[statType].GetComponent<RectTransform>().localPosition;
    }

    //滑动精灵状态面板，retract是反向标记
    //retract为true表示往作，retract为false表示往右
    private IEnumerator slidePokemonStats(string statType, bool retract, Pokemon[] partys, bool trainBattle)
    {
        hidePartyBar(statType);
        pokemonStatsDisplayDic[statType].SetActive(true);
        float startX = pokemonStatsDisplayDicX[statType];
        float distanceX = pokemonStatstransformDic[statType].sizeDelta.x;
        if (retract)
        {
            distanceX = -distanceX;
        }
        float speed = 0.3f;
        float increment = 0f;
        float internalTime = (1 / speed) * Time.deltaTime;
        while (increment < 1)
        {
            increment += internalTime;
            if (increment > 1)
            {
                increment = 1f;
            }
            pokemonStatsDisplayDic[statType].GetComponent<RectTransform>().position = new Vector3(startX + (distanceX * increment),
                pokemonStatsDisplayDic[statType].GetComponent<RectTransform>().position.y, 0);
            yield return null;
        }
        if (trainBattle)
        {
            displayPartyBar(statType, partys);
        }
    }

    //丢精灵动画
    private IEnumerator animatePlayerThrow(string statType, Sprite[] throwAnim)
    {
        platformDic[statType].GetComponent<RectTransform>().localPosition = platformLocalVecDic[statType];
        trainerImageDic[statType].GetComponent<RectTransform>().localPosition = trainerLocalVecDic[statType];
        trainerImageDic[statType].gameObject.SetActive(true);
        if (throwAnim.Length > 4)
        {
            Vector3 targetPosition = trainerImageDic[statType].GetComponent<RectTransform>().position + new Vector3(-80, 0, 0);
            iTween.MoveTo(trainerImageDic[statType].gameObject, targetPosition, 0.3f);
            trainerImageDic[statType].sprite = throwAnim[0];
            yield return new WaitForSeconds(0.1f);
            trainerImageDic[statType].sprite = throwAnim[1];
            yield return new WaitForSeconds(0.1f);
            trainerImageDic[statType].sprite = throwAnim[2];
            yield return new WaitForSeconds(0.1f);
            trainerImageDic[statType].sprite = throwAnim[3];
            yield return new WaitForSeconds(0.1f);
            trainerImageDic[statType].sprite = throwAnim[4];
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }

    //释放精灵
    private IEnumerator releasePokemon(string statType)
    {
        platformDic[statType].GetComponent<RectTransform>().localPosition = platformLocalVecDic[statType];
        pokemonDic[statType].gameObject.SetActive(true);
        spriteDic[statType].GetComponent<RectTransform>().localPosition = spriteVecDic[statType];
        Vector2 normalSize = spriteDic[statType].GetComponent<RectTransform>().sizeDelta;
        spriteDic[statType].GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);

        SfxHandler.Play(pokeballOpenClip);
        float speed = 0.3f;
        float increment = 0f;
        while (increment < 1)
        {
            increment += (1 / speed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1f;
            }
            spriteDic[statType].GetComponent<RectTransform>().sizeDelta = normalSize * increment;
            yield return null;
        }
        spriteDic[statType].GetComponent<RectTransform>().sizeDelta = normalSize;
        //player方训练师隐藏
        if (statType == playerTypes[0])
        {
            trainerImageDic[statType].gameObject.SetActive(false);
        }
    }

    //撤回精灵
    private IEnumerator withdrawPokemon(string statType)
    {
        Vector2 normalSize = spriteDic[statType].GetComponent<RectTransform>().sizeDelta;
        SfxHandler.Play(pokeballOpenClip);
        float speed = 0.3f;
        float increment = 0f;
        while (increment < 1)
        {
            increment += (1 / speed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1f;
            }
            spriteDic[statType].GetComponent<RectTransform>().sizeDelta = normalSize * (1 - increment);
            yield return null;
        }
        spriteDic[statType].GetComponent<RectTransform>().sizeDelta = normalSize;
        pokemonDic[statType].gameObject.SetActive(false);
    }

    //精灵Img帧动画
    private void animatePokemon(string statType, List<Texture> animation)
    {
        animateDic[statType] = animation;
        CommonUtils.changeAnimateImageScale(animateDic[statType], spriteDic[statType], 200.0f);
        if (statType == playerTypes[1])
        {
            StopCoroutine("animateOpponentImage");
            StartCoroutine("animateOpponentImage");
        }
        else
        {
            StopCoroutine("animatePlayerImage");
            StartCoroutine("animatePlayerImage");
        }
    }

    private IEnumerator animatePlayerImage()
    {
        int frame = 0;
        while (animateDic[playerTypes[0]] != null)
        {
            if (animateDic[playerTypes[0]].Count > 0)
            {
                if (frame < animateDic[playerTypes[0]].Count - 1)
                {
                    frame += 1;
                }
                else
                {
                    frame = 0;

                }
                spriteDic[playerTypes[0]].texture = animateDic[playerTypes[0]][frame];
            }
            yield return new WaitForSeconds(0.075f);
        }
    }

    private IEnumerator animateOpponentImage()
    {
        int frame = 0;
        while (animateDic[playerTypes[1]] != null)
        {
            if (animateDic[playerTypes[1]].Count > 0)
            {
                if (frame < animateDic[playerTypes[1]].Count - 1)
                {
                    frame += 1;
                }
                else
                {
                    frame = 0;

                }
                spriteDic[playerTypes[1]].texture = animateDic[playerTypes[1]][frame];
            }
            yield return new WaitForSeconds(0.075f);
        }
    }

    //动画覆盖层
    private IEnumerator animateOverlayer(BattleViewModelInfo.AnimateOverlayer item, float verMovement, float hozMovement, float fadeTime)
    {
        overLayDic[item.StatType].gameObject.SetActive(true);
        overLayDic[item.StatType].texture = Resources.Load<Texture>(item.AnimatPath);
        float fadeStartIncrement = (item.Time - fadeTime) / item.Time;
        float initialAlpha = overLayDic[item.StatType].color.a;
        float increment = 0;
        while (increment < 1)
        {
            increment += (1 / item.Time) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            overLayDic[item.StatType].uvRect.Set(hozMovement * increment, verMovement * increment, 1, 1);
            if (increment > fadeStartIncrement)
            {
                float increment2 = (increment - fadeStartIncrement) / (1 - fadeStartIncrement);
                overLayDic[item.StatType].color = new Color(overLayDic[item.StatType].color.r, overLayDic[item.StatType].color.g,
                    overLayDic[item.StatType].color.b, initialAlpha * (1 - increment2));
            }
            yield return null;
        }
        overLayDic[item.StatType].color = new Color(overLayDic[item.StatType].color.r, overLayDic[item.StatType].color.g,
            overLayDic[item.StatType].color.b, initialAlpha);
        overLayDic[item.StatType].gameObject.SetActive(false);
    }

    //精灵死亡动画
    private IEnumerator faintPokemonAnimation(string statType)
    {
        platformDic[statType].GetComponent<RectTransform>().localPosition = platformLocalVecDic[statType];
        Vector3 distance = new Vector3(0, spriteDic[statType].GetComponent<RectTransform>().rect.height, 0);
        SfxHandler.Play(faintClip);

        float speed = 0.5f;
        float increment = 0f;
        while (increment < 1)
        {
            increment += (1 / speed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1f;
            }
            spriteDic[statType].GetComponent<RectTransform>().localPosition = spriteVecDic[statType] - (distance * increment);
            yield return null;
        }
    }

    //投掷捕捉精灵球
    private IEnumerator throwPokemonBall(string statType)
    {
        GameObject pokeBallObject = pokeBallDic[statType].gameObject;
        pokeBallObject.SetActive(true);
        iTween.MoveFrom(pokeBallObject, spriteDic["Player0"].GetComponent<RectTransform>().position, 0.3f);
        iTween.MoveTo(pokeBallObject, spriteDic["Player0"].GetComponent<RectTransform>().position + new Vector3(80, 100, 0), 0.3f);
        iTween.MoveFrom(pokeBallObject, spriteDic["Player0"].GetComponent<RectTransform>().position + new Vector3(80, 100, 0), 0.3f);
        iTween.MoveTo(pokeBallObject, spriteDic["Player0"].GetComponent<RectTransform>().position + new Vector3(160, 180, 0), 0.3f);
        iTween.MoveFrom(pokeBallObject, spriteDic["Player0"].GetComponent<RectTransform>().position + new Vector3(160, 180, 0), 0.4f);
        iTween.MoveTo(pokeBallObject, spriteDic["Opponent0"].GetComponent<RectTransform>().position, 0.4f);
        yield return new WaitForSeconds(1f);
        StartCoroutine(withdrawPokemon(playerTypes[1]));
        yield return new WaitForSeconds(1f);
        iTween.RotateTo(pokeBallObject, new Vector3(0, 0, 10), 0.3f);
        yield return new WaitForSeconds(0.5f);
        iTween.RotateTo(pokeBallObject, new Vector3(0, 0, -10), 0.3f);
        trainerImageDic[statType].gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        iTween.RotateTo(pokeBallObject, new Vector3(0, 0, 10), 0.3f);
        yield return new WaitForSeconds(0.5f);
        iTween.RotateTo(pokeBallObject, new Vector3(0, 0, -10), 0.3f);
        yield return new WaitForSeconds(1f);
    }

    //淘出精灵球
    private IEnumerator outOfPokemonBall(string statType)
    {
        yield return StartCoroutine(releasePokemon(playerTypes[1]));
        pokeBallDic[statType].gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
    }

    //party列表和状态栏精灵信息显示
    private IEnumerator updatePokeIconInfo(BattlePartyViewModel.PokeIconInfo item)
    {
        if (item.Path != null)
        {
            RawImage slot = transform.Find(item.Path).GetComponent<RawImage>();
            GameObject slotObject = slot.gameObject;
            slotObject.SetActive(item.IsActive);
            Image itemImage = transform.Find(item.Path + "/Item").GetComponent<Image>();
            itemImage.enabled = item.ItemEnable;
            if (item.PokemonName != null)
            {
                Text name = transform.Find(item.Path + "/Name").GetComponent<Text>();
                name.text = item.PokemonName;
            }
            if (item.MaxHp != null)
            {
                Text maxHP = transform.Find(item.Path + "/MaxHP").GetComponent<Text>();
                maxHP.text = item.MaxHp;
            }
            if (item.CurrentHp != null)
            {
                Text currentHP = transform.Find(item.Path + "/CurrentHP").GetComponent<Text>();
                currentHP.text = item.CurrentHp;
            }
            if (item.Level != null)
            {
                Text level = transform.Find(item.Path + "/Level").GetComponent<Text>();
                level.text = item.Level;
            }
            if (item.IconTex != null)
            {
                RawImage icon = transform.Find(item.Path + "/Icon").GetComponent<RawImage>();
                icon.texture = item.IconTex;
            }
            if (item.Gender != null)
            {
                Text gender = transform.Find(item.Path + "/Gender").GetComponent<Text>();
                gender.text = item.Gender;
            }
            RawImage status = transform.Find(item.Path + "/Status").GetComponent<RawImage>();
            if (item.StatusTexture != null)
            {
                status.texture = item.StatusTexture;
            }
            else
            {
                status.texture = Resources.Load<Texture>("null");
            }
            if (item.HpScale != Vector3.zero)
            {
                Image hpBar = transform.Find(item.Path + "/HPBarBack/HPBar").GetComponent<Image>();
                hpBar.GetComponent<RectTransform>().localScale = item.HpScale;
                if (item.HpScale.x < 0.35f)
                {
                    hpBar.color = Color.red;
                }
                else if (item.HpScale.x < 0.65f)
                {
                    hpBar.color = Color.yellow;
                }
                else
                {
                    hpBar.color = Color.green;
                }
            }
        }
        yield return null;
    }

    //血条变化
    private IEnumerator stretchBar(string path, float targetSize)
    {
        yield return StartCoroutine(stretchBar(path, targetSize, false));
    }

    private IEnumerator stretchBar(string path, float targetSize, bool isHP)
    {
        Image bar = transform.Find(path).GetComponent<Image>();
        float increment = 0f;
        float startSize = bar.GetComponent<RectTransform>().sizeDelta.x;
        float distance = targetSize - startSize;
        float time = Mathf.Abs(distance) / 32;

        while (increment < 1)
        {
            increment += (5 / time) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }

            bar.GetComponent<RectTransform>().sizeDelta = new Vector2(startSize + (distance * increment),
                bar.GetComponent<RectTransform>().sizeDelta.y);
            if (isHP)
            {
                setHPBarColor(bar, 76f);
            }
            yield return null;
        }
    }

    private void setHPBarColor(Image bar, float maxSize)
    {
        if (bar.GetComponent<RectTransform>().sizeDelta.x < maxSize / 4f)
        {
            bar.color = new Color(0.625f, 0.125f, 0, 1);
        }
        else if (bar.GetComponent<RectTransform>().sizeDelta.x < maxSize / 2f)
        {
            bar.color = new Color(0.687f, 0.562f, 0, 1);
        }
        else
        {
            bar.color = new Color(0.125f, 0.625f, 0, 1);
        }
    }

    //显示精灵球bar
    private void displayPartyBar(string path, Pokemon[] party)
    {
        expBarDic[path].gameObject.SetActive(true);
        for (int i = 0; i < 6; i++)
        {
            spaceDic[path][i].gameObject.SetActive(true);
            spaceDic[path][i].sprite = partySpaceTex;
            if (party.Length > i)
            {
                if (party[i] != null)
                {
                    if (party[i].getStatus() == Pokemon.Status.FAINTED)
                    {
                        spaceDic[path][i].sprite = partyFaintTex;
                    }
                    else if (party[i].getStatus() == Pokemon.Status.NONE)
                    {
                        spaceDic[path][i].sprite = partyBallTex;
                    }
                    else
                    {
                        spaceDic[path][i].sprite = partyStatusTex;
                    }
                }
            }
        }
    }

    //隐藏精灵球bar
    private void hidePartyBar(string path)
    {
        expBarDic[path].gameObject.SetActive(false);
        for (int i = 0; i < 6; i++)
        {
            spaceDic[path][i].gameObject.SetActive(false);
        }
    }
}