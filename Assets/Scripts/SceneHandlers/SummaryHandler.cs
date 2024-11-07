using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Loxodon.Framework.Views;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding.Builder;
using Loxodon.Framework.Views.Variables;
using System.Collections.Specialized;
using System.Collections.Generic;

public class SummaryHandler : UIView
{

    public AudioClip selectClip;
    public AudioClip scrollClip;
    public AudioClip returnClip;

    public VariableArray variables;
    private SummaryViewModel summaryViewModel;
    private List<Texture> spriteAnimation;
    private RawImage pokemonSprite;
    public Sprite[] unSelectedPageIcons, selectedPageIcons;
    public Sprite unselectedMoveSlotBackground, selectedMoveSlotBackground;

    protected override void Awake()
    {
        summaryViewModel = new SummaryViewModel(this, OnUpdateAttribute, OnUpdateMove, OnUpdatePage);
        pokemonSprite = variables.Get<RawImage>("pokemonSprite");
        this.gameObject.SetActive(false);
    }

    protected override void Start()
    {
        IBindingContext bindingContext = this.BindingContext();
        bindingContext.DataContext = summaryViewModel;
        BindingSet<SummaryHandler, SummaryViewModel> bindingSet = this.CreateBindingSet<SummaryHandler, SummaryViewModel>();
        bindingSet.Bind(variables.Get<Button>("returnButton")).For(v => v.onClick).To(vm => vm.OnSummaryReturn);
        bindingSet.Bind(variables.Get<Button>("leftButton")).For(v => v.onClick).To(vm => vm.OnClickLeftButton);
        bindingSet.Bind(variables.Get<Button>("rightButton")).For(v => v.onClick).To(vm => vm.OnClickRightButton);
        bindingSet.Bind(variables.Get<GameObject>("leftButtonObject")).For(v => v.activeSelf).To(vm => vm.LeftButtonVisible);
        bindingSet.Bind(variables.Get<GameObject>("rightButtonObject")).For(v => v.activeSelf).To(vm => vm.RightButtonVisible);


        bindingSet.Bind(pokemonSprite).For(v => v.texture).To(vm => vm.PokemonSprite).TwoWay();
        for (int i = 0; i < 4; i++)
        {
            bindingSet.Bind(variables.Get<Button>("move" + i)).For(v => v.onClick).To(vm => vm.OnMovesButton).CommandParameter(i);
        }
        bindingSet.Bind(variables.Get<Text>("name")).For(v => v.text).To(vm => vm.TitleName);
        bindingSet.Bind(variables.Get<Text>("level")).For(v => v.text).To(vm => vm.TitleLevel);
        bindingSet.Build();
    }

    public long getReplacedMove()
    {
        return summaryViewModel.replacedMove;
    }

    protected void OnUpdateMove(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (SummaryViewModel.UpdateMove item in eventArgs.NewItems)
                {
                    if (item.Pokemon == null)
                    {
                        return;
                    }
                    Pokemon pokemon = item.Pokemon;
                    List<SkillInfo> skillInfos = pokemon.getSkillInfos();
                    for (int i = 0; i < skillInfos.Count; i++)
                    {
                        Image movesBackground = Transform.Find("Move/MoveList/Move" + i).GetComponent<Image>();
                        movesBackground.sprite = unselectedMoveSlotBackground;
                        Text movesName = Transform.Find("Move/MoveList/Move" + i + "/MoveName").GetComponent<Text>();
                        Text movesPP = Transform.Find("Move/MoveList/Move" + i + "/PP").GetComponent<Text>();
                        movesName.text = MoveDatabase.getMoveName(skillInfos[i].skillId);
                        movesPP.text = skillInfos[i].curPp + "/" + skillInfos[i].maxPp;
                    }

                    Text selectedMoveType = Transform.Find("Move/SelectedMoveType/Type").GetComponent<Text>();
                    Text selectedMovePower = Transform.Find("Move/SelectedMovePower/Power").GetComponent<Text>();
                    Text selectedMoveAccuracy = Transform.Find("Move/SelectedMoveAccuracy/Accuracy").GetComponent<Text>();
                    Text selectedMoveIntro = Transform.Find("Move/SelectedMoveIntro/Text").GetComponent<Text>();

                    int currentMoevIndex = item.CurrentMoevIndex;
                    if (currentMoevIndex >= 0)
                    {
                        Image movesBackground = Transform.Find("Move/MoveList/Move" + currentMoevIndex).GetComponent<Image>();
                        movesBackground.sprite = selectedMoveSlotBackground;

                        long selectedMoveId = skillInfos[currentMoevIndex].skillId;
                        if (string.IsNullOrEmpty(MoveDatabase.getMoveName(selectedMoveId)))
                        {
                            selectedMoveType.text = null;
                            selectedMovePower.text = null;
                            selectedMoveAccuracy.text = null;
                            selectedMoveIntro.text = null;
                        }
                        else
                        {
                            MoveData selectedMove = MoveDatabase.getMove(selectedMoveId);
                            //selectedMoveType.text = selectedMove.getType();
                            selectedMoveType.text = "Normal";
                            selectedMovePower.text = selectedMove.getPower() == 0 ? "-" : selectedMove.getPower().ToString();
                            selectedMoveAccuracy.text = Mathf.Round(selectedMove.getAccuracy() * 100f) == 0 ? "-" : Mathf.Round(selectedMove.getAccuracy() * 100f).ToString(); ;
                            selectedMoveIntro.text = selectedMove.getDescription(); ;
                        }
                    }
                }
                break;
        }
    }

    protected void OnUpdateAttribute(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (SummaryViewModel.UpdateAttribute item in eventArgs.NewItems)
                {
                    if (item.Pokemon == null)
                    {
                        return;
                    }
                    Text ivsHpText = Transform.Find("Attribute/IVs/HP/HPText").GetComponent<Text>();
                    Image ivsBarValuesTrnHp = Transform.Find("Attribute/IVs/HP/HPBar/value").GetComponent<Image>();
                    float height = ivsBarValuesTrnHp.GetComponent<RectTransform>().rect.height;
                    int total = 0;
                    Pokemon pokemon = item.Pokemon;
                    ivsHpText.text = pokemon.getIV_HP().ToString();
                    ivsBarValuesTrnHp.GetComponent<RectTransform>().sizeDelta = new Vector2(pokemon.getIV_HP() / 31.0f * 140, height);
                    total += pokemon.getIV_HP();

                    Text ivsATKText = Transform.Find("Attribute/IVs/ATK/ATKText").GetComponent<Text>();
                    Image ivsBarValuesTrnATK = Transform.Find("Attribute/IVs/ATK/ATKBar/value").GetComponent<Image>();
                    ivsATKText.text = pokemon.getIV_ATK().ToString();
                    ivsBarValuesTrnATK.GetComponent<RectTransform>().sizeDelta = new Vector2(pokemon.getIV_ATK() / 31.0f * 140, height);
                    total += pokemon.getIV_ATK();

                    Text ivsSPATKText = Transform.Find("Attribute/IVs/SPATK/SPATKText").GetComponent<Text>();
                    Image ivsBarValuesTrnSpATK = Transform.Find("Attribute/IVs/SPATK/SPATKBar/value").GetComponent<Image>();
                    ivsSPATKText.text = pokemon.getIV_SPA().ToString();
                    ivsBarValuesTrnSpATK.GetComponent<RectTransform>().sizeDelta = new Vector2(pokemon.getIV_SPA() / 31.0f * 140, height);
                    total += pokemon.getIV_SPA();

                    Text ivsDEFText = Transform.Find("Attribute/IVs/DEF/DEFText").GetComponent<Text>();
                    Image ivsBarValuesTrnDEF = Transform.Find("Attribute/IVs/DEF/DEFBar/value").GetComponent<Image>();
                    ivsDEFText.text = pokemon.getIV_DEF().ToString();
                    ivsBarValuesTrnDEF.GetComponent<RectTransform>().sizeDelta = new Vector2(pokemon.getIV_DEF() / 31.0f * 140, height);
                    total += pokemon.getIV_DEF();

                    Text ivsSPDEFText = Transform.Find("Attribute/IVs/SPDEF/SPDEFText").GetComponent<Text>();
                    Image ivsBarValuesTrnSPDEF = Transform.Find("Attribute/IVs/SPDEF/SPDEFBar/value").GetComponent<Image>();
                    ivsSPDEFText.text = pokemon.getIV_SPD().ToString();
                    ivsBarValuesTrnSPDEF.GetComponent<RectTransform>().sizeDelta = new Vector2(pokemon.getIV_SPD() / 31.0f * 140, height);
                    total += pokemon.getIV_SPD();

                    Text ivsSPEText = Transform.Find("Attribute/IVs/SPE/SPEText").GetComponent<Text>();
                    Image ivsBarValuesTrnSPE = Transform.Find("Attribute/IVs/SPE/SPEBar/value").GetComponent<Image>();
                    ivsSPEText.text = pokemon.getIV_SPE().ToString();
                    ivsBarValuesTrnSPE.GetComponent<RectTransform>().sizeDelta = new Vector2(pokemon.getIV_SPE() / 31.0f * 140, height);
                    total += pokemon.getIV_SPE();

                    Text ivsTotalText = Transform.Find("Attribute/IVs/Total/Text").GetComponent<Text>();
                    Text abilityText = Transform.Find("Attribute/Ability/Text").GetComponent<Text>();
                    Text introText = Transform.Find("Attribute/Intro/Text").GetComponent<Text>();
                    ivsTotalText.text = "总计: " + total.ToString();
                    introText.text = "特性说明...";
                }
                break;
        }
    }
    protected void OnUpdatePage(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (SummaryViewModel.UpdatePage item in eventArgs.NewItems)
                {
                    Image[] pageIcons = new Image[3];
                    Transform[] pagesTrn = new Transform[3];

                    pageIcons[0] = Transform.Find("PageSelector/AttributePageIcon").GetComponent<Image>();
                    pageIcons[1] = Transform.Find("PageSelector/MovePageIcon").GetComponent<Image>();
                    pageIcons[2] = Transform.Find("PageSelector/PositionPageIcon").GetComponent<Image>();

                    pagesTrn[0] = Transform.Find("Attribute");
                    pagesTrn[1] = Transform.Find("Move");
                    pagesTrn[2] = Transform.Find("Position");

                    if (item.LastPageIndex >= 0)
                    {
                        pageIcons[item.LastPageIndex].sprite = unSelectedPageIcons[item.LastPageIndex];
                        pagesTrn[item.LastPageIndex].gameObject.SetActive(false);
                    }
                    if (item.CurrentPageIndex >= 0)
                    {
                        pageIcons[item.CurrentPageIndex].sprite = selectedPageIcons[item.CurrentPageIndex];
                        pagesTrn[item.CurrentPageIndex].gameObject.SetActive(true);
                    }
                }
                break;
        }
    }

    public IEnumerator control(Pokemon[] pokemonList, int currentPosition)
    {
        yield return StartCoroutine(control(pokemonList, currentPosition, false, 0));
    }

    //支持技能学习
    public IEnumerator control(Pokemon pokemon, long newMove)
    {
        yield return StartCoroutine(control(new Pokemon[] { pokemon }, 0, true, newMove));
    }

    public IEnumerator control(Pokemon[] pokemonList, int currentPosition, bool learning, long newMove)
    {
        Scene.main.SetMapButtonVisible(false);
        Pokemon currentPoke = pokemonList[currentPosition];
        spriteAnimation = currentPoke.GetFrontAnimFromGif();
        CommonUtils.changeAnimateImageScale(spriteAnimation, pokemonSprite, 100.0f);
        summaryViewModel.reset(currentPoke, learning, newMove);
        yield return StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
        StartCoroutine("animatePokemon");
        while (summaryViewModel.running)
        {
            yield return null;
        }
        StopCoroutine("animatePokemon");
        yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
        this.gameObject.SetActive(false);
    }

    private IEnumerator animatePokemon()
    {
        yield return CommonUtils.animateImage(spriteAnimation, pokemonSprite);
    }
}