using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Proyecto26;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using UnityEditor;
using System;
using Loxodon.Framework.Views.Variables;
using Loxodon.Framework.Views;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Binding.Builder;
using System.Collections.Specialized;
using Loxodon.Framework.Observables;

public class PokedexHandler : UIView
{
    public VariableArray variables;
    private PokedexViewModel viewModel;
    private List<Texture> pokemonSpriteAnimation;
    private RawImage pokemonSprite;

    public Texture[] types;
    public AudioClip selectClip;

    protected override void Awake()
    {
        viewModel = new PokedexViewModel(this, OnPokeIconInfoChanged, OnPokeIconInfoClicked, OnPokeDetailInfo);
        types = new Texture[20];
        for (int i = 0; i < 20; i++)
        {
            types[i] = new Texture2D(32, 32);
        }
        this.gameObject.SetActive(false);
    }

    protected override void Start()
    {
        pokemonSprite = transform.Find("PokedexDialog/BasePic/Pokemon").GetComponent<RawImage>();
        variables.Get<InputField>("pageNumberInput").onValueChanged.AddListener((param) => { viewModel.PageNumberInputField = param; });

        IBindingContext bindingContext = this.BindingContext();
        bindingContext.DataContext = viewModel;
        BindingSet<PokedexHandler, PokedexViewModel> bindingSet = this.CreateBindingSet<PokedexHandler, PokedexViewModel>();

        bindingSet.Bind(variables.Get<Button>("leftButton")).For(v => v.onClick).To(vm => vm.OnLeftButton);
        bindingSet.Bind(variables.Get<Button>("rightButton")).For(v => v.onClick).To(vm => vm.OnRightButton);
        bindingSet.Bind(variables.Get<Text>("label")).For(v => v.text).To(vm => vm.Label).TwoWay();
        bindingSet.Bind(variables.Get<Button>("pageNumberButton")).For(v => v.onClick).To(vm => vm.OnPageNumberGo);
        bindingSet.Bind(variables.Get<Button>("dialogCancel")).For(v => v.onClick).To(vm => vm.OnDialogCancel);
        bindingSet.Bind(variables.Get<Button>("returnButton")).For(v => v.onClick).To(vm => vm.OnReturn);
        for (int i = 0; i < PokedexViewModel.boxMaxNum; i++)
        {
            bindingSet.Bind(transform.Find("ScrollView/Viewport/Content/Pokemon" + i).GetComponent<Button>())
                 .For(v => v.onClick).To(vm => vm.OnIcon).CommandParameter(i);
        }
        bindingSet.Build();
    }

    //更新图鉴列表Icon列表
    protected void OnPokeIconInfoChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (PokedexViewModel.PokeIconInfo item in eventArgs.NewItems)
                {
                    RawImage image = transform.Find("ScrollView/Viewport/Content/Pokemon" + item.Index + "/PokemonIcon").GetComponent<RawImage>();
                    image.texture = item.PokePreview;
                    Text text = transform.Find("ScrollView/Viewport/Content/Pokemon" + item.Index + "/PokemonName").GetComponent<Text>();
                    text.text = item.PokeName;
                    Image pokeBallImage= transform.Find("ScrollView/Viewport/Content/Pokemon" + item.Index + "/PokemonBall").GetComponent<Image>();
                    if (item.PokeStatus == 2)
                    {
                        pokeBallImage.gameObject.SetActive(true);
                    }
                    else
                    {
                        pokeBallImage.gameObject.SetActive(false);
                    }
                }
                break;
        }
    }

    //点击一个精灵，显示Dialog
    protected void OnPokeIconInfoClicked(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (PokedexViewModel.PokeIconClick item in eventArgs.NewItems)
                {
                    GameObject pokedexDialog = transform.Find("PokedexDialog").gameObject;
                    pokedexDialog.SetActive(item.IspokedexDialogShow);

                    if (item.Id > 0)
                    {
                        pokemonSpriteAnimation = Pokemon.GetAnimFromGif("PokemonSprites", item.Id, Pokemon.Gender.CALCULATE, false);
                        CommonUtils.changeAnimateImageScale(pokemonSpriteAnimation, pokemonSprite, 120.0f);
                        StopCoroutine("animatePokemon");
                        StartCoroutine("animatePokemon");
                    }

                    Text pokemonName= transform.Find("PokedexDialog/Back/Back2/PokemonName").GetComponent<Text>();
                    pokemonName.text = item.PokeName;

                    Text pokemonName2 = transform.Find("PokedexDialog/BasePic/Name").GetComponent<Text>();
                    pokemonName2.text = item.PokeName;

                    Slider sliderHp = transform.Find("PokedexDialog/BaseInfo/Info0/Info").GetComponent<Slider>();
                    sliderHp.value = item.Hp;
                    Text textHp = transform.Find("PokedexDialog/BaseInfo/Info0/Name").GetComponent<Text>();
                    textHp.text = "血量   " + item.Hp;

                    Slider sliderAtk = transform.Find("PokedexDialog/BaseInfo/Info1/Info").GetComponent<Slider>();
                    sliderAtk.value = item.Atk;
                    Text textAtk = transform.Find("PokedexDialog/BaseInfo/Info1/Name").GetComponent<Text>();
                    textAtk.text = "攻击   " + item.Atk;

                    Slider sliderDef = transform.Find("PokedexDialog/BaseInfo/Info2/Info").GetComponent<Slider>();
                    sliderDef.value = item.Def;
                    Text textDef = transform.Find("PokedexDialog/BaseInfo/Info2/Name").GetComponent<Text>();
                    textDef.text = "防御   " + item.Def;

                    Slider sliderSpa = transform.Find("PokedexDialog/BaseInfo/Info3/Info").GetComponent<Slider>();
                    sliderSpa.value = item.Spa;
                    Text textSpa = transform.Find("PokedexDialog/BaseInfo/Info3/Name").GetComponent<Text>();
                    textSpa.text = "特攻   " + item.Spa;

                    Slider sliderSpd = transform.Find("PokedexDialog/BaseInfo/Info4/Info").GetComponent<Slider>();
                    sliderSpd.value = item.Spd;
                    Text textSpd = transform.Find("PokedexDialog/BaseInfo/Info4/Name").GetComponent<Text>();
                    textSpd.text = "特防   " + item.Spd;

                    Slider sliderSpe = transform.Find("PokedexDialog/BaseInfo/Info5/Info").GetComponent<Slider>();
                    sliderSpe.value = item.Spe;
                    Text textSpe = transform.Find("PokedexDialog/BaseInfo/Info5/Name").GetComponent<Text>();
                    textSpe.text = "速度   " + item.Spe;

                    Slider sliderSum = transform.Find("PokedexDialog/BaseInfo/Info6/Info").GetComponent<Slider>();
                    sliderSum.value = item.Sum;
                    Text textSum = transform.Find("PokedexDialog/BaseInfo/Info6/Name").GetComponent<Text>();
                    textSum.text = "总计   " + item.Sum;
                }
                break;
        }
    }

    //显示精灵详细信息
    protected void OnPokeDetailInfo(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                foreach (PokedexViewModel.PokeDetailInfo item in eventArgs.NewItems)
                {
                    Text pokeIconNo = transform.Find("PokeTitle/PokeIconNo").GetComponent<Text>();
                    pokeIconNo.text = item.PokeIconNo;

                    Text pokeInfo = transform.Find("PokeInfo").GetComponent<Text>();
                    pokeInfo.text = item.PokeInfo;

                    Text pokeGetNumber = transform.Find("PokeGetTitle/PokeGetNumber").GetComponent<Text>();
                    pokeGetNumber.text = item.PokeGetNumber;

                    Text pokeIconName = transform.Find("PokeTitle/PokeIconName").GetComponent<Text>();
                    pokeIconName.text = item.PokeIconName;

                    RawImage pokeIconImage= transform.Find("PokeTitle/PokeIconImage").GetComponent<RawImage>();
                    pokeIconImage.texture = item.PokePreview;
                }
                break;
        }
    }

    public IEnumerator control()
    {
        this.gameObject.SetActive(true);
        Scene.main.SetMapButtonVisible(false);
        yield return StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
        viewModel.resetBox(0);

        viewModel.running = true;
        while (viewModel.running)
        {
            yield return null;
        }
        yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
        this.gameObject.SetActive(false);
    }

    private IEnumerator animatePokemon()
    {
        int pokemonFrame = 0;
        while (true)
        {
            if (pokemonSpriteAnimation != null && pokemonSpriteAnimation.Count > 0)
            {
                if (pokemonFrame < pokemonSpriteAnimation.Count - 1)
                {
                    pokemonFrame += 1;
                }
                else
                {
                    pokemonFrame = 0;
                }
                pokemonSprite.texture = pokemonSpriteAnimation[pokemonFrame];
            }
            yield return new WaitForSeconds(0.08f);
        }
    }

    private Texture typeToImage(PokemonData.Type type)
    {
        if (type == PokemonData.Type.NONE)
        {
            return types[0];
        }
        else if (type == PokemonData.Type.NORMAL)
        {
            return types[0];
        }
        else if (type == PokemonData.Type.FIGHTING)
        {
            return types[1];
        }
        else if (type == PokemonData.Type.FLYING)
        {
            return types[2];
        }
        else if (type == PokemonData.Type.POISON)
        {
            return types[3];
        }
        else if (type == PokemonData.Type.GROUND)
        {
            return types[4];
        }
        else if (type == PokemonData.Type.ROCK)
        {
            return types[5];
        }
        else if (type == PokemonData.Type.BUG)
        {
            return types[6];
        }
        else if (type == PokemonData.Type.GHOST)
        {
            return types[7];
        }
        else if (type == PokemonData.Type.STEEL)
        {
            return types[8];
        }
        else if (type == PokemonData.Type.NONE)
        {
            return types[9];
        }
        else if (type == PokemonData.Type.FIRE)
        {
            return types[10];
        }
        else if (type == PokemonData.Type.WATER)
        {
            return types[11];
        }
        else if (type == PokemonData.Type.GRASS)
        {
            return types[12];
        }
        else if (type == PokemonData.Type.ELECTRIC)
        {
            return types[13];
        }
        else if (type == PokemonData.Type.PSYCHIC)
        {
            return types[14];
        }
        else if (type == PokemonData.Type.ICE)
        {
            return types[15];
        }
        else if (type == PokemonData.Type.DRAGON)
        {
            return types[16];
        }
        else if (type == PokemonData.Type.DARK)
        {
            return types[17];
        }
        else if (type == PokemonData.Type.FAIRY)
        {
            return types[18];
        }
        else
        {
            return types[9];
        }
    }
}