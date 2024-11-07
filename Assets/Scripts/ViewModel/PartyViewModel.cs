using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Execution;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class PartyViewModel : ViewModelBase
{
    public class TechShotInfo : ObservableObject
    {
        private Texture texture;
        private Vector3 scale = new Vector3(1, 1, 1);
        private int index;
        private string techShotName;
        private string techShotNum;
        private Boolean isObjectActive;

        public string TechShotName
        {
            get { return this.techShotName; }
            set { this.Set<string>(ref this.techShotName, value, "TechShotName"); }
        }

        public string TechShotNum
        {
            get { return this.techShotNum; }
            set { this.Set<string>(ref this.techShotNum, value, "TechShotNum"); }
        }

        public Boolean IsObjectActive
        {
            get { return this.isObjectActive; }
            set { this.Set<Boolean>(ref this.isObjectActive, value, "IsObjectActive"); }
        }

        public int Index
        {
            get { return this.index; }
            set { this.Set<int>(ref this.index, value, "Index"); }
        }


        public Texture Texture
        {
            get { return this.texture; }
            set { this.Set<Texture>(ref this.texture, value, "Texture"); }
        }

        public Vector3 Scale
        {
            get { return this.scale; }
            set { this.Set<Vector3>(ref this.scale, value, "Scale"); }
        }
    }

    public class SlotInfo : ObservableObject
    {
        private Texture texture;
        private Vector3 postion = Vector3.zero;
        private Texture selectBallTexture;
        private int index;
        private Color color;
        private Vector3 scale = new Vector3(1, 1, 1);
        private string gender;
        private Texture iconTexture;
        private string pokemonName;
        private string level;
        private string currentHp;
        private string maxHp;
        private Texture statusTexture;
        private Vector3 hpScale = new Vector3(1, 1, 1);
        private Boolean itemEnable;

        public Vector3 Postion
        {
            get { return this.postion; }
            set { this.Set<Vector3>(ref this.postion, value, "Postion"); }
        }

        public Boolean ItemEnable
        {
            get { return this.itemEnable; }
            set { this.Set<Boolean>(ref this.itemEnable, value, "ItemEnable"); }
        }

        public Vector3 HpScale
        {
            get { return this.hpScale; }
            set { this.Set<Vector3>(ref this.hpScale, value, "HpScale"); }
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

        public string Level
        {
            get { return this.level; }
            set { this.Set<string>(ref this.level, value, "Level"); }
        }

        public string PokemonName
        {
            get { return this.pokemonName; }
            set { this.Set<string>(ref this.pokemonName, value, "PokemonName"); }
        }

        public string Gender
        {
            get { return this.gender; }
            set { this.Set<string>(ref this.gender, value, "Gender"); }
        }

        public Vector3 Scale
        {
            get { return this.scale; }
            set { this.Set<Vector3>(ref this.scale, value, "Scale"); }
        }

        public Color Color
        {
            get { return this.color; }
            set { this.Set<Color>(ref this.color, value, "Color"); }
        }

        public int Index
        {
            get { return this.index; }
            set { this.Set<int>(ref this.index, value, "Index"); }
        }


        public Texture Texture
        {
            get { return this.texture; }
            set { this.Set<Texture>(ref this.texture, value, "Texture"); }
        }

        public Texture SelectBallTexture
        {
            get { return this.selectBallTexture; }
            set { this.Set<Texture>(ref this.selectBallTexture, value, "SelectBallTexture"); }
        }

        public Texture IconTexture
        {
            get { return this.iconTexture; }
            set { this.Set<Texture>(ref this.iconTexture, value, "IconTexture"); }
        }

        public Texture StatusTexture
        {
            get { return this.statusTexture; }
            set { this.Set<Texture>(ref this.statusTexture, value, "StatusTexture"); }
        }
    }

    private string charcterInfo;

    public string CharcterInfo
    {
        get { return this.charcterInfo; }
        set { this.Set<string>(ref this.charcterInfo, value, "CharcterInfo"); }
    }


    public bool running;
    public int partyPosition;
    public int swapPosition = -1;
    public bool switching = false;

    private PartyHandler handler;
    private ObservableList<PartyViewModel.TechShotInfo> techList;
    private ObservableList<PartyViewModel.SlotInfo> slotList;
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;

    public PartyViewModel(PartyHandler partyHandler,
        NotifyCollectionChangedEventHandler techShotInfoHandler,
        NotifyCollectionChangedEventHandler slotShotInfoHandler)
    {
        handler = partyHandler;
        this.techList = new ObservableList<PartyViewModel.TechShotInfo>();
        techList.CollectionChanged += techShotInfoHandler;
        for (int i = 0; i < 4; i++)
        {
            techList.Add(new PartyViewModel.TechShotInfo()
            {
                IsObjectActive = true,
                Texture = handler.unItemCheck,
                Scale = new Vector3(1f, 1f, 1f),
                Index = i
            });
        }

        this.slotList = new ObservableList<PartyViewModel.SlotInfo>();
        slotList.CollectionChanged += slotShotInfoHandler;

        this.onTechShot = new SimpleCommand<int>(OnTechShotClick);
        this.onSlotShot = new SimpleCommand<int>(OnSlotShotClick);

        running = true;
        switching = false;
        swapPosition = -1;
    }

    public IEnumerator SetItemPosition(int index)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i == index)
            {
                techList[i] = new PartyViewModel.TechShotInfo()
                {
                    IsObjectActive = true,
                    Texture = handler.itemCheck,
                    Scale = new Vector3(1.2f, 1.35f, 1f),
                    Index = i
                };
            }
            else
            {
                techList[i] = new PartyViewModel.TechShotInfo()
                {
                    IsObjectActive = true,
                    Texture = handler.unItemCheck,
                    Scale = new Vector3(1f, 1f, 1f),
                    Index = i
                };
            }
        }
        yield return null;
    }

    //点击显示Slot精灵
    public IEnumerator SetSlotPosition(int index)
    {
        slotList.Clear();
        for (int i = 0; i < 6 && i < PCManager.Instance.getSubBoxPokemonLength(0); i++)
        {
            slotList.Add(new PartyViewModel.SlotInfo()
            {
                Scale = new Vector3(1f, 1f, 1f),
                Index = i,
                Color = Color.white
            });
        }
        dialog = DialogBoxUIHandler.main;
        for (int i = 0; i < 6 && i < PCManager.Instance.getSubBoxPokemonLength(0); i++)
        {
            Pokemon selectedPokemon = PCManager.Instance.getPartyBox()[i];
            string gender;
            if (selectedPokemon.getGender() == Pokemon.Gender.FEMALE)
            {
                gender = "♀";
            }
            else if (selectedPokemon.getGender() == Pokemon.Gender.MALE)
            {
                gender = "♂";
            }
            else
            {
                gender = null;
            }
            Vector3 hpScale = new Vector3((float)selectedPokemon.getCurrentHP() / (float)selectedPokemon.getHP(), 1, 1);
            Texture statusTexture;
            if (selectedPokemon.getStatus() != Pokemon.Status.NONE)
            {
                statusTexture =
                    Resources.Load<Texture>("PCSprites/status" + selectedPokemon.getStatus().ToString());
            }
            else
            {
                statusTexture = null;
            }
            bool itemEnable = false;
            if (selectedPokemon.getHeldItem() != null)
            {
                itemEnable = true;
            }
            else
            {
                itemEnable = false;
            }

            if (i == index)
            {
                slotList[i] = new PartyViewModel.SlotInfo()
                {
                    Texture = handler.slotCheck,
                    Color = Color.white,
                    SelectBallTexture = handler.selectBallOpen,
                    Scale = new Vector3(1.05f, 1.05f, 1f),
                    Index = i,
                    Gender = gender,
                    StatusTexture = statusTexture,
                    PokemonName = selectedPokemon.getName(),
                    Level = "" + selectedPokemon.getLevel(),
                    CurrentHp = "" + selectedPokemon.getCurrentHP(),
                    MaxHp = "" + selectedPokemon.getHP(),
                    IconTexture = selectedPokemon.GetIcons(),
                    HpScale = hpScale,
                    ItemEnable = itemEnable
                };
                List<SkillInfo> skillInfos = selectedPokemon.getSkillInfos();
                int moveLength = skillInfos.Count;
                for (int j = 0; j < 4; j++)
                {
                    if (j < moveLength)
                    {
                        techList[j] = new PartyViewModel.TechShotInfo()
                        {
                            IsObjectActive = true,
                            TechShotNum = skillInfos[j].curPp + "/" + skillInfos[j].maxPp,
                            TechShotName = "" + MoveDatabase.getMoveName(skillInfos[j].skillId),
                            Index = j
                        };
                    }
                    else
                    {
                        techList[j] = new PartyViewModel.TechShotInfo()
                        {
                            IsObjectActive = false,
                            TechShotNum = "",
                            TechShotName = "",
                            Index = j
                        };
                    }
                }
            }
            else
            {
                slotList[i] = new PartyViewModel.SlotInfo()
                {
                    Texture = handler.unSlotCheck,
                    Color = Color.black,
                    SelectBallTexture = handler.selectBallClosed,
                    Index = i,
                    Gender = gender,
                    StatusTexture = statusTexture,
                    PokemonName = selectedPokemon.getName(),
                    Level = "" + selectedPokemon.getLevel(),
                    CurrentHp = "" + selectedPokemon.getCurrentHP(),
                    MaxHp = "" + selectedPokemon.getHP(),
                    IconTexture = selectedPokemon.GetIcons(),
                    HpScale = hpScale,
                    ItemEnable = itemEnable
                };
            }
        }
        if (index >= 0)
        {
            partyPosition = index;
            yield return StaticCoroutine.StartCoroutine(showSlotChoices());
        }
        yield return null;
    }


    //显示提示文案
    private IEnumerator showSlotChoices()
    {
        if (!switching)
        {
            Pokemon selectedPokemon = PCManager.Instance.getPartyBox()[partyPosition];
            string[] choices = new string[]
            {
              "概述","交换","取消",
            };
            SfxHandler.Play(handler.selectClip);
            yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilent(selectedPokemon.getName() + "现在要干嘛呢?"));
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
            //记录任务号
            int chosenIndex = dialog.buttonIndex;
            if (chosenIndex == 2)
            {
                //概述
                SfxHandler.Play(handler.selectClip);
                yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(false, 0.4f));
                Scene.main.Summary.gameObject.SetActive(true);
                yield return StaticCoroutine.StartCoroutine(Scene.main.Summary.control(PCManager.Instance.getPartyBox(), partyPosition));
                while (Scene.main.Summary.gameObject.activeSelf)
                {
                    yield return null;
                }
                yield return StaticCoroutine.StartCoroutine(SetSlotPosition(-1));
                CharcterInfo = "选择一个精灵.";
                yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, 0.4f));
            }
            else if (chosenIndex == 1)
            {
                //交换
                SfxHandler.Play(handler.selectClip);
                switching = true;
                swapPosition = partyPosition;
                CharcterInfo = "移动" + selectedPokemon.getName() + "到哪里?";
            }
            else
            {
                SfxHandler.Play(handler.selectClip);
                if (!switching)
                {
                    yield return StaticCoroutine.StartCoroutine(SetSlotPosition(-1));
                    CharcterInfo = "选择一个精灵.";
                }
            }
        }
        else
        {
            //交换中
            if (partyPosition == swapPosition)
            {
                CharcterInfo = "选择一个精灵.";
            }
            else
            {
                yield return StaticCoroutine.StartCoroutine(switchPokemon(swapPosition, partyPosition));
                switching = false;
                swapPosition = -1;
                CharcterInfo = "选择一个精灵.";
            }
        }
    }

    //交换精灵
    private IEnumerator switchPokemon(int position1, int position2)
    {
        if (position1 != position2 && position1 >= 0 && position2 >= 0 && position1 < 6 && position2 < 6)
        {
            //数据更新交换精灵
            PCManager.Instance.swapPokemon(0, position1, 0, position2, delegate (bool result) {
                StaticCoroutine.StartCoroutine(SetSlotPosition(-1));
            });
        }
        yield return null;
    }

    //=========================================================================
    private readonly SimpleCommand<int> onTechShot;
    public ICommand OnTechShot
    {
        get { return this.onTechShot; }
    }

    public void OnTechShotClick(int index)
    {
        Executors.RunOnCoroutineNoReturn(SetItemPosition(index));
    }
    //=========================================================================
    private readonly SimpleCommand<int> onSlotShot;

    public ICommand OnSlotShot
    {
        get { return this.onSlotShot; }
    }

    public void OnSlotShotClick(int index)
    {
        Executors.RunOnCoroutineNoReturn(SetSlotPosition(index));
    }
    //=========================================================================
    public void OnReturn()
    {
        running = false;
    }
}
