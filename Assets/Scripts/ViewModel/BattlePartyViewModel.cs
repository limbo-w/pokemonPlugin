using System;
using System.Collections;
using System.Collections.Specialized;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class BattlePartyViewModel : ViewModelBase
{
    private bool partyObject;
    public bool PartyObject
    {
        get { return this.partyObject; }
        set { this.Set<bool>(ref this.partyObject, value, "PartyObject"); }
    }

    public BattleViewModel battleViewModel;
    private ObservableList<PokeIconInfo> pokeInfoList;
    private Pokemon[] pokemonList;
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
    private BattleHandler battleHandler;

    public class PokeIconInfo : ObservableObject
    {
        private string path;
        private Texture iconTex;
        private bool isActive = true;
        private string pokemonName;
        private Color color = Color.black;
        private Vector3 hpScale = Vector3.zero;
        private string level;
        private Texture statusTexture;
        private string gender;
        private string currentHp;
        private string maxHp;
        private bool itemEnable;

        public bool ItemEnable
        {
            get { return this.itemEnable; }
            set { this.Set<bool>(ref this.itemEnable, value, "ItemEnable"); }
        }

        public string Path
        {
            get { return this.path; }
            set { this.Set<string>(ref this.path, value, "Path"); }
        }

        public bool IsActive
        {
            get { return this.isActive; }
            set { this.Set<bool>(ref this.isActive, value, "IsActive"); }
        }

        public Texture IconTex
        {
            get { return this.iconTex; }
            set { this.Set<Texture>(ref this.iconTex, value, "IconTex"); }
        }

        public string PokemonName
        {
            get { return this.pokemonName; }
            set { this.Set<string>(ref this.pokemonName, value, "PokemonName"); }
        }

        public Vector3 HpScale
        {
            get { return this.hpScale; }
            set { this.Set<Vector3>(ref this.hpScale, value, "HpScale"); }
        }

        public string Level
        {
            get { return this.level; }
            set { this.Set<string>(ref this.level, value, "Level"); }
        }

        public string MaxHp
        {
            get { return this.maxHp; }
            set { this.Set<string>(ref this.maxHp, value, "MaxHp"); }
        }

        public string CurrentHp
        {
            get { return this.currentHp; }
            set { this.Set<string>(ref this.currentHp, value, "CurrentHp"); }
        }

        public string Gender
        {
            get { return this.gender; }
            set { this.Set<string>(ref this.gender, value, "Gender"); }
        }

        public Texture StatusTexture
        {
            get { return this.statusTexture; }
            set { this.Set<Texture>(ref this.statusTexture, value, "StatusTexture"); }
        }

        public Color Color
        {
            get { return this.color; }
            set { this.Set<Color>(ref this.color, value, "Color"); }
        }
    }

    private readonly SimpleCommand<int> onPartyButton;

    public ICommand OnPartyButton
    {
        get { return this.onPartyButton; }
    }

    public BattlePartyViewModel(BattleHandler handler, NotifyCollectionChangedEventHandler pokeInfoHandler)
    {
        this.battleHandler = handler;
        this.pokeInfoList = new ObservableList<PokeIconInfo>();
        pokeInfoList.CollectionChanged += pokeInfoHandler;
        for (int i = 0; i < 6; i++)
        {
            pokeInfoList.Add(new PokeIconInfo()
            {
                Path = "OptionBox/Party/Slot" + i,
                IsActive = false
            });
        }
        this.onPartyButton = new SimpleCommand<int>(DoPokemonSlotButtonClickDown);
    }

    private void DoPokemonSlotButtonClickDown(int index)
    {
        StaticCoroutine.StartCoroutine(doPokemonSlotButtonClickDown(index));
    }

    //点击一个精灵
    private IEnumerator doPokemonSlotButtonClickDown(int index)
    {
        dialog = DialogBoxUIHandler.main;
        SfxHandler.Play(battleHandler.selectClip);
        dialog.drawTextInstant("要对这个精灵做什么呢!");
        string[] choices = new string[] { "替换", "概述", "取消" };
        yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
        int chosenIndex = dialog.buttonIndex;
        if (chosenIndex == 2)
        {
            SfxHandler.Play(battleHandler.selectClip);
            battleViewModel.DoPokemonSwitchButton(index);
        }
        else if (chosenIndex == 1)
        {
            SfxHandler.Play(battleHandler.selectClip);
            Scene.main.Summary.gameObject.SetActive(true);
            StaticCoroutine.StartCoroutine(Scene.main.Summary.control(pokemonList, index));
            while (Scene.main.Summary.gameObject.activeSelf)
            {
                yield return null;
            }
            yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
        }
        else
        {
            SfxHandler.Play(battleHandler.selectClip);
        }
    }

    //隐藏party面版显示
    public void dismissPokemonSlotsDisplay()
    {
        PartyObject = false;
    }

    //宝可梦Slot展示刷新
    public void updatePokemonSlotsDisplay(Pokemon[] pokemons)
    {
        //显示PartyObject
        PartyObject = true;
        pokemonList = pokemons;
        for (int i = 0; i < pokemons.Length; i++)
        {
            if (pokemons[i] == null)
            {
                pokeInfoList[i] = new PokeIconInfo()
                {
                    Path = "OptionBox/Party/Slot" + i,
                    IsActive = false
                };
            }
            else
            {
                string gender = "";
                if (pokemons[i].getGender() == Pokemon.Gender.FEMALE)
                {
                    gender = "♀";
                }
                else if (pokemons[i].getGender() == Pokemon.Gender.MALE)
                {
                    gender = "♂";
                }
                Texture statusTexture = null;
                if (pokemons[i].getStatus() != Pokemon.Status.NONE)
                {
                    statusTexture =
                        Resources.Load<Texture>("PCSprites/status" + pokemons[i].getStatus().ToString());
                }
                pokeInfoList[i] = new PokeIconInfo()
                {
                    Path = "OptionBox/Party/Slot" + i,
                    IsActive = true,
                    IconTex = Resources.Load<Texture>("PokemonIcons/icon" + CommonUtils.convertLongID(pokemons[i].getID())),
                    PokemonName = pokemons[i].getName(),
                    HpScale = new Vector3(pokemons[i].getPercentHP(), 1, 1),
                    Gender = gender,
                    Level = "" + pokemons[i].getLevel(),
                    StatusTexture = statusTexture,
                    Color = Color.black,
                    MaxHp = "" + pokemons[i].getHP(),
                    CurrentHp = "" + pokemons[i].getCurrentHP()
                };
            }
        }
    }
}