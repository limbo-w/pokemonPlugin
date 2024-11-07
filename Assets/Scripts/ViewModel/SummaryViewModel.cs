using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class SummaryViewModel : ViewModelBase
{
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
    private const int PAGE_COUNT = 3;
    public bool running = false;
    private SummaryHandler summaryHandler;
    private int currentMoevIndex;
    //上一页
    private int lastPage;
    //当前页
    private int currentPage;
    private ObservableList<UpdateAttribute> updateAttributeList;
    private ObservableList<UpdateMove> updateMoveList;
    private ObservableList<UpdatePage> updatePageList;
    private Pokemon curPokemon;
    //是否是技能学习
    private bool isLearnMove;
    //被替换的技能
    public long replacedMove;
    //替换为的技能
    public long replaceMove;

    public SummaryViewModel(SummaryHandler summaryHandler, NotifyCollectionChangedEventHandler updateAttributeHandler,
        NotifyCollectionChangedEventHandler updateMovesHandler, NotifyCollectionChangedEventHandler updatePageHandler)
    {
        this.summaryHandler = summaryHandler;
        this.updateAttributeList = new ObservableList<UpdateAttribute>();
        updateAttributeList.CollectionChanged += updateAttributeHandler;
        updateAttributeList.Add(new UpdateAttribute());
        this.updateMoveList = new ObservableList<UpdateMove>();
        updateMoveList.CollectionChanged += updateMovesHandler;
        updateMoveList.Add(new UpdateMove());
        this.updatePageList = new ObservableList<UpdatePage>();
        updatePageList.CollectionChanged += updatePageHandler;
        updatePageList.Add(new UpdatePage());
        this.onMovesButton = new SimpleCommand<int>(OnClickMoveButton);
    }

    public void reset(Pokemon pokemon, bool isLearnMove, long newMove)
    {
        dialog = DialogBoxUIHandler.main;
        running = true;
        curPokemon = pokemon;

        this.isLearnMove = isLearnMove;
        replaceMove = newMove;
        replacedMove = 0;
        if (isLearnMove)
        {
            LeftButtonVisible = false;
            RightButtonVisible = false;
            currentPage = 1;
        }
        else
        {
            currentPage = lastPage;
        }
        currentMoevIndex = -1;
        updateTitle();
        updateAttribute();
        updateSelectedMove();
        updatePage();
    }

    private void updateTitle()
    {
        TitleName = curPokemon.getName();
        TitleLevel = "Lv." + curPokemon.getLevel().ToString();
    }

    private void updateAttribute()
    {
        updateAttributeList[0] = new UpdateAttribute
        {
            Pokemon = curPokemon
        };
    }

    private void updatePage()
    {
        updatePageList[0] = new UpdatePage
        {
            LastPageIndex = lastPage,
            CurrentPageIndex = currentPage,
        };
    }

    //更新技能界面
    private void updateSelectedMove()
    {
        updateMoveList[0] = new UpdateMove()
        {
            Pokemon = curPokemon,
            CurrentMoevIndex = currentMoevIndex
        };
    }

    private void exitSummary()
    {
        lastPage = currentPage;
        currentPage = 0;
        running = false;
    }

    //=========================================================================
    //点击Summary返回
    public void OnSummaryReturn()
    {
        StaticCoroutine.StartCoroutine(onSummaryReturn());
    }

    private IEnumerator onSummaryReturn()
    {
        bool exit = true;
        if (isLearnMove)
        {
            yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilent("技能学习中，要退出吗?"));
            string[] choices = { "是", "否", };
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
            int chosenIndex = dialog.buttonIndex;
            if (chosenIndex == 0)
            {
                exit = false;
            }
        }
        if (exit)
        {
            exitSummary();
        }
    }

    public void OnClickMoveButton(int index)
    {
        StaticCoroutine.StartCoroutine(onClickMoveButton(index));
    }

    private IEnumerator onClickMoveButton(int index)
    {
        SfxHandler.Play(summaryHandler.selectClip);
        if (isLearnMove)
        {
            yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilent("是否替换这个技能?"));
            string[] choices = { "是", "否", };
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
            int chosenIndex = dialog.buttonIndex;
            if (chosenIndex == 1)
            {
                SfxHandler.Play(summaryHandler.selectClip);
                List<SkillInfo> skillInfos = curPokemon.getSkillInfos();
                replacedMove = skillInfos[index].skillId;
                curPokemon.replaceMove(index, replaceMove);
                exitSummary();
            }
            else
            {
                SfxHandler.Play(summaryHandler.selectClip);
            }
        }
        else
        {
            currentMoevIndex = index;
            updateSelectedMove();
        }
    }

    public void OnClickLeftButton()
    {
        SfxHandler.Play(summaryHandler.selectClip);
        lastPage = currentPage;
        currentPage -= 1;
        if (currentPage < 0)
        {
            currentPage = PAGE_COUNT - 1;
        }
        updatePage();
    }

    public void OnClickRightButton()
    {
        SfxHandler.Play(summaryHandler.selectClip);
        lastPage = currentPage;
        currentPage += 1;
        if (currentPage >= PAGE_COUNT)
        {
            currentPage = 0;
        }
        updatePage();
    }

    private readonly SimpleCommand<int> onMovesButton;

    public ICommand OnMovesButton
    {
        get { return this.onMovesButton; }
    }

    private bool leftButtonVisible;
    public bool LeftButtonVisible
    {
        get { return this.leftButtonVisible; }
        set { this.Set<bool>(ref this.leftButtonVisible, value, "LeftButtonVisible"); }
    }

    private bool rightButtonVisible;
    public bool RightButtonVisible
    {
        get { return this.rightButtonVisible; }
        set { this.Set<bool>(ref this.rightButtonVisible, value, "RightButtonVisible"); }
    }

    private Texture pokemonSprite;
    public Texture PokemonSprite
    {
        get { return this.pokemonSprite; }
        set { this.Set<Texture>(ref this.pokemonSprite, value, "PokemonSprite"); }
    }

    private string titleName;
    public string TitleName
    {
        get { return this.titleName; }
        set { this.Set<string>(ref this.titleName, value, "TitleName"); }
    }

    private string titleLevel;
    public string TitleLevel
    {
        get { return this.titleLevel; }
        set { this.Set<string>(ref this.titleLevel, value, "TitleLevel"); }
    }

    public class UpdateAttribute : ObservableObject
    {
        private Pokemon pokemon;
        public Pokemon Pokemon
        {
            get { return this.pokemon; }
            set { this.Set<Pokemon>(ref this.pokemon, value, "Pokemon"); }
        }
    }

    public class UpdateMove : ObservableObject
    {
        private Pokemon pokemon;
        public Pokemon Pokemon
        {
            get { return this.pokemon; }
            set { this.Set<Pokemon>(ref this.pokemon, value, "Pokemon"); }
        }

        private int currentMoevIndex;
        public int CurrentMoevIndex
        {
            get { return this.currentMoevIndex; }
            set { this.Set<int>(ref this.currentMoevIndex, value, "CurrentMoevIndex"); }
        }
    }

    public class UpdatePage : ObservableObject
    {

        private int lastPageIndex;
        public int LastPageIndex
        {
            get { return this.lastPageIndex; }
            set { this.Set<int>(ref this.lastPageIndex, value, "LastPageIndex"); }
        }

        private int currentPageIndex;
        public int CurrentPageIndex
        {
            get { return this.currentPageIndex; }
            set { this.Set<int>(ref this.currentPageIndex, value, "CurrentPageIndex"); }
        }
    }
}