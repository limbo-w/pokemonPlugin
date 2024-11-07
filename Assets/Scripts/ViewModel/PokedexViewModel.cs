using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class PokedexViewModel : ViewModelBase
{
    private PokedexHandler handler;
    public bool running;
    private ObservableList<PokeIconInfo> pokeInfoList = new ObservableList<PokeIconInfo>();
    private ObservableList<PokeIconClick> pokeInfoClickList = new ObservableList<PokeIconClick>();
    private ObservableList<PokeDetailInfo> pokeDetailInfoList = new ObservableList<PokeDetailInfo>();

    public class PokeIconInfo : ObservableObject
    {
        private int index;
        private Texture pokePreview;
        private string pokeName;
        private int pokeStatus;

        public Texture PokePreview
        {
            get { return this.pokePreview; }
            set { this.Set<Texture>(ref this.pokePreview, value, "PokePreview"); }
        }

        public string PokeName
        {
            get { return this.pokeName; }
            set { this.Set<string>(ref this.pokeName, value, "PokeName"); }
        }

        public int Index
        {
            get { return this.index; }
            set { this.Set<int>(ref this.index, value, "Index"); }
        }

        public int PokeStatus
        {
            get { return this.pokeStatus; }
            set { this.Set<int>(ref this.pokeStatus, value, "PokeStatus"); }
        }
    }

    public class PokeIconClick : ObservableObject
    {
        private long id;
        public long Id
        {
            get { return this.id; }
            set { this.Set<long>(ref this.id, value, "Id"); }
        }

        private bool ispokedexDialogShow;

        public bool IspokedexDialogShow
        {
            get { return this.ispokedexDialogShow; }
            set { this.Set<bool>(ref this.ispokedexDialogShow, value, "IspokedexDialogShow"); }
        }

        private string pokePicName;
        public string PokePicName
        {
            get { return this.pokePicName; }
            set { this.Set<string>(ref this.pokePicName, value, "PokePicName"); }
        }

        private int hp;
        public int Hp
        {
            get { return this.hp; }
            set { this.Set<int>(ref this.hp, value, "Hp"); }
        }

        private int atk;
        public int Atk
        {
            get { return this.atk; }
            set { this.Set<int>(ref this.atk, value, "Atk"); }
        }

        private int def;
        public int Def
        {
            get { return this.def; }
            set { this.Set<int>(ref this.def, value, "Def"); }
        }

        private int spa;
        public int Spa
        {
            get { return this.spa; }
            set { this.Set<int>(ref this.spa, value, "Spa"); }
        }

        private int spd;
        public int Spd
        {
            get { return this.spd; }
            set { this.Set<int>(ref this.spd, value, "Spd"); }
        }

        private int spe;
        public int Spe
        {
            get { return this.spe; }
            set { this.Set<int>(ref this.spe, value, "Spe"); }
        }

        private int sum;
        public int Sum
        {
            get { return this.sum; }
            set { this.Set<int>(ref this.sum, value, "Sum"); }
        }

        private string pokeName;
        public string PokeName
        {
            get { return this.pokeName; }
            set { this.Set<string>(ref this.pokeName, value, "PokeName"); }
        }
    }

    public class PokeDetailInfo : ObservableObject
    {
        private string pokeGetNumber;
        public string PokeGetNumber
        {
            get { return this.pokeGetNumber; }
            set { this.Set<string>(ref this.pokeGetNumber, value, "PokeGetNumber"); }
        }

        private string pokeIconNo;
        public string PokeIconNo
        {
            get { return this.pokeIconNo; }
            set { this.Set<string>(ref this.pokeIconNo, value, "PokeIconNo"); }
        }

        private string pokeInfo;
        public string PokeInfo
        {
            get { return this.pokeInfo; }
            set { this.Set<string>(ref this.pokeInfo, value, "PokeInfo"); }
        }

        private string pokeIconName;
        public string PokeIconName
        {
            get { return this.pokeIconName; }
            set { this.Set<string>(ref this.pokeIconName, value, "PokeIconName"); }
        }

        private Texture pokePreview;
        public Texture PokePreview
        {
            get { return this.pokePreview; }
            set { this.Set<Texture>(ref this.pokePreview, value, "PokePreview"); }
        }
    }

    public PokedexViewModel(PokedexHandler pokedexHandler, NotifyCollectionChangedEventHandler pokeInfoHandler,
         NotifyCollectionChangedEventHandler pokeInfoClickHandler, NotifyCollectionChangedEventHandler pokeDetailInfoHandler)
    {
        handler = pokedexHandler;
        this.onIcon = new SimpleCommand<int>(onIconClick);
        pokeInfoList.CollectionChanged += pokeInfoHandler;
        for (int i = 0; i < boxMaxNum; i++)
        {
            pokeInfoList.Add(new PokeIconInfo() { PokeName = "", PokePreview = getIcon(0), Index = i });
        }
        pokeInfoClickList.CollectionChanged += pokeInfoClickHandler;
        pokeInfoClickList.Add(new PokeIconClick());
        pokeDetailInfoList.CollectionChanged += pokeDetailInfoHandler;
        pokeDetailInfoList.Add(new PokeDetailInfo());
    }

    //============================================================================


    public const int boxMaxNum = 25;
    //对应的图鉴编号
    private int boxNum;

    private long[] pokeSpritesIds = new long[boxMaxNum];
    //对应的图鉴号
    private int[] pokemon = new int[boxMaxNum];
    private static PokedexData[] pokedexDatas = new PokedexData[boxMaxNum];
    private bool audioPlaying;

    private string label;
    public string Label
    {
        get { return this.label; }
        set { this.Set<string>(ref this.label, value, "Label"); }
    }

    private string pageNumberInputField;
    public string PageNumberInputField
    {
        get { return this.pageNumberInputField; }
        set { this.Set<string>(ref this.pageNumberInputField, value, "PageNumberInputField"); }
    }

    public void resetBox(int index)
    {
        boxNum = index;
        updateBox(boxNum);
    }

    //向前翻页
    void onDownRefresh()
    {
        SfxHandler.Play(handler.selectClip);
        if (boxNum > 0)
        {
            boxNum--;
        }
        updateBox(boxNum);
    }

    //向后翻页
    void onUpRefresh()
    {
        SfxHandler.Play(handler.selectClip);
        boxNum++;
        updateBox(boxNum);
    }

    //请求网络
    private void updateBox(int boxNum)
    {
        Label = "Pokedex " + toNum(boxNum * boxMaxNum + 1) + " - " + toNum(boxNum * boxMaxNum + boxMaxNum);
        //恢复界面UI
        for (int i = 0; i < boxMaxNum; i++)
        {
            pokedexDatas[i] = null; ;
            pokemon[i] = boxNum * boxMaxNum + i + 1;
            pokeInfoList[i] = new PokeIconInfo() { PokePreview = getIcon(0), PokeName = toNum(pokemon[i]), Index = i };
        }
        PokedexManager.Instance.getHandbookInfoByPage(boxNum, boxMaxNum, delegate(PokedexData[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                PokedexData data = result[i];
                int id = (int)data.id;
                int index = id - 1 - boxNum * boxMaxNum;
                if (index >= 0)
                {
                    pokedexDatas[index] = data;
                    pokeSpritesIds[index] = id;
                    pokeInfoList[index] = new PokeIconInfo()
                    {
                        PokePreview = getIcon(id),
                        PokeName = toNum(id),
                        Index = index,
                        PokeStatus = data.status
                    };
                }
            }
            updateInfo(0);
        });
    }

    private void updateInfo(int index)
    {
        string pokeIconNo = "";
        string pokeGetNumber = "未知";
        string pokeInfo = "没有记录";
        string pokePicName = "";
        if (index >= 0)
        {
            int pokemonId = boxNum * boxMaxNum + index + 1;
            pokeIconNo = toNum(pokemonId);
            pokeInfo = pokedexDatas[index].description;
            pokePicName = pokedexDatas[index].pokemonName;
            pokeDetailInfoList[0] = new PokeDetailInfo()
            {
                PokeIconNo = pokeIconNo,
                PokeGetNumber = pokeGetNumber,
                PokeInfo = pokeInfo,
                PokeIconName = pokePicName,
                PokePreview = getIcon(pokemonId)
            };
        }
    }

    public void OnLeftButton()
    {
        onDownRefresh();
    }

    public void OnRightButton()
    {
        onUpRefresh();
    }

    //跳转到具体的图鉴页
    public void OnPageNumberGo()
    {
        SfxHandler.Play(handler.selectClip);
        int pageNumber = Convert.ToInt32(PageNumberInputField);
        if (pageNumber > 0)
        {
            boxNum = pageNumber - 1;
            updateBox(boxNum);
        }
    }

    public void OnDialogCancel()
    {
        SfxHandler.Play(handler.selectClip);
        pokeInfoClickList[0] = new PokeIconClick()
        {
            IspokedexDialogShow = false,
        };
    }

    public IEnumerator playCry(int pokemonNum)
    {
        audioPlaying = true;
        try
        {
            AudioClip cry = Resources.Load<AudioClip>("Audio/cry/" + toNum(pokemonNum));
            Debug.Log(cry);
            SfxHandler.Play(cry);
        }
        catch
        {

        }
        yield return new WaitForSeconds(1);
        audioPlaying = false;
    }

    private Texture getIcon(int id)
    {
        Texture[] icons = Resources.LoadAll<Texture>("PokemonIcons/icon" + toNum(id));
        if (icons.Length == 0)
        {
            icons = Resources.LoadAll<Texture>("Button/icon000");
        }
        return icons[0];
    }

    private string toNum(int n)
    {
        string result = n.ToString();
        while (result.Length < 3)
        {
            result = "0" + result;
        }
        return result;
    }

    //======================================================================================
    public void OnReturn()
    {
        running = false;
    }

    private readonly SimpleCommand<int> onIcon;

    public ICommand OnIcon
    {
        get { return this.onIcon; }
    }

    void onIconClick(int index)
    {
        SfxHandler.Play(handler.selectClip);
        if (pokedexDatas != null && index >= 0 && index < pokedexDatas.Length
            && pokedexDatas[index] != null)
        {
            //计算精灵图鉴号
            int hp = pokedexDatas[index].initialHp;
            int atk = pokedexDatas[index].initialAtk;
            int def = pokedexDatas[index].initialDef;
            int spa = pokedexDatas[index].initialSpA;
            int spd = pokedexDatas[index].initialSpD;
            int spe = pokedexDatas[index].initialSpe;
            int sum = hp + atk + def + spa + spd + spe;
            pokeInfoClickList[0] = new PokeIconClick()
            {
                IspokedexDialogShow = true,
                PokeName = pokedexDatas[index].pokemonName,
                Id = pokedexDatas[index].id,
                Hp = hp,
                Atk = atk,
                Def = def,
                Spa = spa,
                Spd = spd,
                Spe = spe,
                Sum = sum
            };
            updateInfo(index);
        }
    }
}
