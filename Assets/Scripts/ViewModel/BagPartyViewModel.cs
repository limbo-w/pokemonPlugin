using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Execution;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class BagPartyViewModel : ViewModelBase
{
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
    private BagHandler handler;
    public BagItemViewModel itemViewModel;
    public BattleViewModel battleViewModel;
    private ObservableList<PokeIconInfo> pokeInfoList;
    private float[] pokeHpStartLength = new float[6] { 1, 1, 1, 1, 1, 1 };
    //精灵选择位置
    public int partyPosition = -1;

    public class PokeIconInfo : ObservableObject
    {
        private int index;
        private Texture partySlotTex;
        private Texture iconTex;
        private Vector3 scale = new Vector3(1, 1, 1);
        private Color color = Color.black;
        private bool isActive = true;
        private Vector3 hpScale = Vector3.zero;
        private string gender = "";
        private string pokemonName = "";
        private string level = "";
        private Texture statusTexture;
        private bool itemEnable;
        private Color partyTextDisplayColor = new Color(0f, 0f, 0f, 0f);
        private string partyTextDisplay = "";

        public string PartyTextDisplay
        {
            get { return this.partyTextDisplay; }
            set { this.Set<string>(ref this.partyTextDisplay, value, "PartyTextDisplay"); }
        }

        public Color PartyTextDisplayColor
        {
            get { return this.partyTextDisplayColor; }
            set { this.Set<Color>(ref this.partyTextDisplayColor, value, "PartyTextDisplayColor"); }
        }

        public bool ItemEnable
        {
            get { return this.itemEnable; }
            set { this.Set<bool>(ref this.itemEnable, value, "ItemEnable"); }
        }

        public Texture StatusTexture
        {
            get { return this.statusTexture; }
            set { this.Set<Texture>(ref this.statusTexture, value, "StatusTexture"); }
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

        public Vector3 HpScale
        {
            get { return this.hpScale; }
            set { this.Set<Vector3>(ref this.hpScale, value, "HpScale"); }
        }

        public int Index
        {
            get { return this.index; }
            set { this.Set<int>(ref this.index, value, "Index"); }
        }

        public Texture PartySlotTex
        {
            get { return this.partySlotTex; }
            set { this.Set<Texture>(ref this.partySlotTex, value, "PartySlotTex"); }
        }

        public Texture IconTex
        {
            get { return this.iconTex; }
            set { this.Set<Texture>(ref this.iconTex, value, "IconTex"); }
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

        public bool IsActive
        {
            get { return this.isActive; }
            set { this.Set<bool>(ref this.isActive, value, "IsActive"); }
        }
    }

    private bool partyVisible = true;
    public bool PartyVisible
    {
        get { return this.partyVisible; }
        set { this.Set<bool>(ref this.partyVisible, value, "PartyVisible"); }
    }


    public BagPartyViewModel(BagHandler bagHandler,
         NotifyCollectionChangedEventHandler pokeInfoHandler)
    {
        handler = bagHandler;
        this.onParty = new SimpleCommand<int>(OnPartyClick);
        this.pokeInfoList = new ObservableList<PokeIconInfo>();
        pokeInfoList.CollectionChanged += pokeInfoHandler;
        for (int i = 0; i < 6; i++)
        {
            pokeInfoList.Add(new PokeIconInfo()
            {
                Index = i,
                IsActive = false
            });
        }
    }

    public void clear()
    {
        partyPosition = -1;
        updatePartyInfo();
    }
    //==================================================================================

    private readonly SimpleCommand<int> onParty;

    public ICommand OnParty
    {
        get { return this.onParty; }
    }

    private void OnPartyClick(int index)
    {
        StaticCoroutine.StartCoroutine(onPartyClick(index));
    }

    //精灵点击
    private IEnumerator onPartyClick(int position)
    {
        SfxHandler.Play(handler.selectClip);
        partyPosition = position;
        //不在交换过程中         
        for (int i = 0; i < 6 && i < PCManager.Instance.getSubBoxPokemonLength(0); i++)
        {
            if (i != partyPosition)
            {
                pokeInfoList[i] = new PokeIconInfo()
                {
                    Index = i,
                    PartySlotTex = handler.partySlotTex,
                    Scale = new Vector3(1f, 1f, 1f),
                    Color = Color.black,
                };
            }
            else
            {
                pokeInfoList[i] = new PokeIconInfo()
                {
                    Index = i,
                    PartySlotTex = handler.partySlotSelectedTex,
                    Scale = new Vector3(1.05f, 1.05f, 1f),
                    Color = Color.white,
                };
            }
        }
        Pokemon currentPokemon = PCManager.Instance.getPartyBox()[partyPosition];
        if (!itemViewModel.switching)
        {
            if (battleViewModel == null)
            {
                BagItem heldItem = currentPokemon.getHeldItem();
                if (heldItem != null)
                {
                    //当前选中精灵有物品
                    yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilent("要从" + currentPokemon.getName() + "拿走持有物?"));
                    string[] choiceInfos = new string[] { "同意", "反对" };
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choiceInfos));
                    int chosenIndex = dialog.buttonIndex;
                    if (chosenIndex == 1)
                    {
                        SfxHandler.Play(handler.selectClip);
                        BagItem receivedItem = currentPokemon.swapHeldItem(null);
                        itemViewModel.addItem(receivedItem);
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("从" + currentPokemon.getName() + "取下" + receivedItem + "."));
                        yield return StaticCoroutine.StartCoroutine(handler.ResetControl());
                    }
                    else if (chosenIndex == 0)
                    {
                        SfxHandler.Play(handler.selectClip);
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("放弃对物品的操作."));
                    }
                }
            }
        }
        else
        {
            //在交换中
            if (itemViewModel.chosenChoice == "使用")
            {
                BagItem chosenItem = itemViewModel.chosenItem;
                ItemData selectedItem = ItemDatabase.getItem(chosenItem.basicGoodsId);
                yield return StaticCoroutine.StartCoroutine(runItemEffect(selectedItem));
                yield return StaticCoroutine.StartCoroutine(handler.ResetControl());
                BagManager.Instance.removeItem(new BagItem
                {
                    holdId = chosenItem.holdId,
                    basicGoodsId = chosenItem.basicGoodsId,
                    itemName = chosenItem.itemName,
                    amount = 1
                }, delegate (bool result)
                {
                    if (result)
                    {
                        itemViewModel.updateItemInfo();
                    }
                    else
                    {
                        StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("使用物品失败"));
                    }
                });
            }
        }
        yield return null;
    }

    //更新当前精灵
    public void updatePartyInfo()
    {
        dialog = DialogBoxUIHandler.main;
        Pokemon currentPokemon;
        for (int i = 0; i < 6 && i < PCManager.Instance.getSubBoxPokemonLength(0); i++)
        {
            currentPokemon = PCManager.Instance.getPartyBox()[i];
            if (currentPokemon == null)
            {
                pokeInfoList[i] = new PokeIconInfo()
                {
                    Index = i,
                    IsActive = false
                };
            }
            else
            {
                string partyTextDisplay = "";
                Color partyTextDisplayColor = Color.black;
                BagItem[] currentItemList = itemViewModel.currentItemList;
                if (currentItemList != null && currentItemList.Length > 0)
                {
                    ItemData selectedItem = ItemDatabase.getItem(currentItemList[itemViewModel.selectedPosition
                        + itemViewModel.currentTopPosition[itemViewModel.currentScreen]].basicGoodsId);
                    if (selectedItem.getItemEffect() == ItemData.ItemEffect.EVOLVE)
                    {
                        if (currentPokemon != null)
                        {
                            if (currentPokemon.canEvolve("Stone," + selectedItem.getName()))
                            {
                                partyTextDisplay = "ABLE!";
                                partyTextDisplayColor = new Color(0, 0.5f, 1, 1);
                            }
                            else
                            {
                                partyTextDisplay = "UNABLE!";
                                partyTextDisplayColor = new Color(1, 0.25f, 0, 1);
                            }
                        }
                    }
                    else if (selectedItem.getItemEffect() == ItemData.ItemEffect.TM)
                    {
                        if (currentPokemon != null)
                        {
                            if (currentPokemon.HasMove(selectedItem.getBasicGoodsId()))
                            {
                                partyTextDisplay = "LEARNED!";
                                partyTextDisplayColor = new Color(1, 1, 1, 1);
                            }
                            else if (currentPokemon.CanLearnMove(selectedItem.getBasicGoodsId()))
                            {
                                partyTextDisplay = "ABLE!";
                                partyTextDisplayColor = new Color(0, 0.5f, 1, 1);
                            }
                            else
                            {
                                partyTextDisplay = "UNABLE!";
                                partyTextDisplayColor = new Color(1, 0.25f, 0, 1);
                            }
                        }
                    }
                }
                else
                {
                    itemViewModel.updateItemInfo();
                }
                string gender = "";
                if (currentPokemon.getGender() == Pokemon.Gender.FEMALE)
                {
                    gender = "♀";
                }
                else if (currentPokemon.getGender() == Pokemon.Gender.MALE)
                {
                    gender = "♂";
                }
                Texture statusTexture;
                if (currentPokemon.getStatus() != Pokemon.Status.NONE)
                {
                    statusTexture =
                        Resources.Load<Texture>("PCSprites/status" + currentPokemon.getStatus().ToString());
                }
                else
                {
                    statusTexture = null;
                }
                bool itemEnable;
                if (currentPokemon.getHeldItem() != null)
                {
                    itemEnable = true;
                }
                else
                {
                    itemEnable = false;
                }
                pokeHpStartLength[i] = (float)currentPokemon.getCurrentHP() / (float)currentPokemon.getHP();
                pokeInfoList[i] = new PokeIconInfo()
                {
                    IsActive = true,
                    Index = i,
                    IconTex = currentPokemon.GetIcons(),
                    HpScale = new Vector3(pokeHpStartLength[i], 1, 1),
                    Gender = gender,
                    PokemonName = currentPokemon.getName(),
                    Level = "" + currentPokemon.getLevel(),
                    StatusTexture = statusTexture,
                    ItemEnable = itemEnable,
                    Color = Color.black,
                    PartySlotTex = handler.partySlotTex,
                    PartyTextDisplay = partyTextDisplay,
                    PartyTextDisplayColor = partyTextDisplayColor
                };
            }
        }
    }

    private IEnumerator runItemEffect(ItemData selectedItem)
    {
        yield return runItemEffect(selectedItem, false);
    }

    private IEnumerator runItemEffect(ItemData selectedItem, bool booted)
    {
        //战斗中使用完道具退出这个界面  0表示未使用，1表示已经使用，2表示等战斗回合使用
        bool effectState = false;
        Pokemon currentPokemon = PCManager.Instance.getPartyBox()[partyPosition];
        if (selectedItem.getItemEffect() == ItemData.ItemEffect.HP)
        {
            //HP
            if (currentPokemon.getCurrentHP() < currentPokemon.getHP() &&
                currentPokemon.getStatus() != Pokemon.Status.FAINTED)
            {
                //determine amount / intialise HP Bar Animation variables
                float amount = selectedItem.getFloatParameter();
                if (amount <= 1)
                {
                    amount = currentPokemon.healHP(currentPokemon.getHP() * amount);
                }
                else
                {
                    amount = currentPokemon.healHP(amount);
                }
                float difference = (float)amount / (float)currentPokemon.getHP();

                float increment = 0;
                float speed = 0.5f;

                //itemViewModel.removeItem(selectedItem.getName(), 1);
                //animate HP bar restoring
                while (increment < 1)
                {
                    increment += (1 / speed) * Time.deltaTime;
                    if (increment > 1)
                    {
                        increment = 1;
                    }
                    pokeInfoList[partyPosition] = new PokeIconInfo()
                    {
                        IsActive = true,
                        Index = partyPosition,
                        HpScale = new Vector3((float)(pokeHpStartLength[partyPosition] + (difference * increment)), 1, 1),
                        Color = Color.black
                    };
                    yield return null;
                }
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("恢复了" + amount + "点."));
                effectState = true;
            }
            else
            {
                effectState = false;
            }
        }
        else if (selectedItem.getItemEffect() == ItemData.ItemEffect.PP)
        {
            //PP
            List<SkillInfo> skillInfos = currentPokemon.getSkillInfos();
            bool loweredPP = false;
            for (int i = 0; i < skillInfos.Count; i++)
            {
                if (skillInfos[i].skillId > 0)
                {
                    if (skillInfos[i].curPp < skillInfos[i].maxPp)
                    {
                        loweredPP = true;
                    }
                }
            }
            if (loweredPP)
            {
                if (selectedItem.getStringParameter().ToUpper() == "ALL")
                {
                    float amount = selectedItem.getFloatParameter();
                    for (int i = 0; i < skillInfos.Count; i++)
                    {
                        if (skillInfos[i].skillId > 0)
                        {
                            amount = currentPokemon.healPP(i, amount);
                        }
                    }

                    //itemViewModel.removeItem(selectedItem.getName(), 1);
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("恢复了所有的技能点."));
                }
                else
                {
                    List<string> choices = new List<string>();
                    foreach (SkillInfo skillInfo in skillInfos)
                    {
                        choices.Add(MoveDatabase.getMoveName(skillInfo.skillId));
                    }
                    choices.Add("取消");

                    yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilent("恢复哪个技能PP?"));
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices.ToArray()));
                    int chosenIndex = dialog.buttonIndex;
                    if (chosenIndex != 0)
                    {
                        //resolve move number
                        int moveNumber = 0;
                        for (int i = 0; i < skillInfos.Count; i++)
                        {
                            if (MoveDatabase.getMoveName(skillInfos[i].skillId) == choices[choices.Count - chosenIndex - 1])
                            {
                                moveNumber = i;
                            }
                        }
                        //heal PP for selected move.
                        float amount = selectedItem.getFloatParameter();
                        amount = currentPokemon.healPP(moveNumber, amount);

                        //itemViewModel.removeItem(selectedItem.getName(), 1);
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("恢复了" + amount + "点."));
                    }
                }
                effectState = true;
            }
            else
            {
                effectState = false;
            }
        }
        else if (selectedItem.getItemEffect() == ItemData.ItemEffect.STATUS)
        {
            string statusCurer = selectedItem.getStringParameter().ToUpper();
            if (statusCurer == "ALL" && currentPokemon.getStatus().ToString() != "FAINTED" &&
                currentPokemon.getStatus().ToString() != "NONE")
            {
                statusCurer = currentPokemon.getStatus().ToString();
            }

            if (currentPokemon.getStatus().ToString() == statusCurer)
            {
                if (statusCurer == "FAINTED")
                {
                    //Revive
                    currentPokemon.setStatus(Pokemon.Status.NONE);

                    //determine amount / intialise HP Bar Animation variables
                    float amount = selectedItem.getFloatParameter();
                    if (amount <= 1)
                    {
                        amount = currentPokemon.healHP(currentPokemon.getHP() * amount);
                    }
                    else
                    {
                        amount = currentPokemon.healHP(amount);
                    }
                    float difference = (float)amount / (float)currentPokemon.getHP();

                    float increment = 0;
                    float speed = 0.5f;

                    //itemViewModel.removeItem(selectedItem.getName(), 1);

                    //animate HP bar restoring
                    while (increment < 1)
                    {
                        increment += (1 / speed) * Time.deltaTime;
                        if (increment > 1)
                        {
                            increment = 1;
                        }
                        pokeInfoList[partyPosition] = new PokeIconInfo()
                        {
                            IsActive = true,
                            Index = partyPosition,
                            HpScale = new Vector3((float)(pokeHpStartLength[partyPosition] + (difference * increment)), 1, 1),
                            Color = Color.black
                        };
                        yield return null;
                    }
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("恢复了" + amount + "点."));
                }
                else
                {
                    currentPokemon.setStatus(Pokemon.Status.NONE);
                    //itemViewModel.removeItem(selectedItem.getName(), 1);
                    if (statusCurer == "ASLEEP")
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "清醒过来了!"));
                    }
                    else if (statusCurer == "BURNED")
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "烧伤恢复了!"));
                    }
                    else if (statusCurer == "FROZEN")
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "冻伤恢复了!"));
                    }
                    else if (statusCurer == "PARALYZED")
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "麻痹恢复了!"));
                    }
                    else if (statusCurer == "POISONED")
                    {
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "中毒恢复了!"));
                    }
                }
                effectState = true;
            }
            else
            {
                effectState = false;
            }
        }
        else if (selectedItem.getItemEffect() == ItemData.ItemEffect.EV)
        {
            string statBooster = selectedItem.getStringParameter().ToUpper();
            float amount = selectedItem.getFloatParameter();
            bool evsAdded = currentPokemon.addEVs(statBooster, amount);
            if (evsAdded)
            {
                //itemViewModel.removeItem(selectedItem.getName(), 1);

                if (statBooster == "HP")
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "的HP上升!"));
                }
                else if (statBooster == "ATK")
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "的攻击力上升!"));
                }
                else if (statBooster == "DEF")
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "的防御力上升!"));
                }
                else if (statBooster == "SPA")
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "的特攻上升!"));
                }
                else if (statBooster == "SPD")
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "的特防上升!"));
                }
                else if (statBooster == "SPE")
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "的速度上升!"));
                }
                effectState = true;
            }
            else
            {
                effectState = false;
            }
        }
        else if (selectedItem.getItemEffect() == ItemData.ItemEffect.EVOLVE)
        {
            //EVOLVE
            if (currentPokemon.canEvolve("Stone," + selectedItem.getName()))
            {
                long oldID = currentPokemon.getID();
                BgmHandler.main.PlayOverlay(null, 0, 0.5f);
                //yield return new WaitForSeconds(sceneTransition.FadeOut());
                yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));

                Scene.main.Evolution.gameObject.SetActive(true);
                yield return StaticCoroutine.StartCoroutine(Scene.main.Evolution.control(currentPokemon, "Stone," + selectedItem.getName()));
                while (Scene.main.Evolution.gameObject.activeSelf)
                {
                    yield return null;
                }

                if (oldID != currentPokemon.getID())
                {
                    //if evolved
                    //itemViewModel.removeItem(selectedItem.getName(), 1);
                } //remove item

                //yield return new WaitForSeconds(sceneTransition.FadeIn());
                yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
                effectState = true;
            }
            else
            {
                effectState = false;
            }
        }
        else if (selectedItem.getItemEffect() == ItemData.ItemEffect.TM)
        {
            if (currentPokemon.HasMove(selectedItem.getTMNo()))
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName()
                    + "已经学了\n" + selectedItem.getName() + "."));
                effectState = true;
            }
            else if (currentPokemon.CanLearnMove(selectedItem.getTMNo()))
            {
                if (!booted)
                {
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("启动了一个技能."));
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("它的名字是" + selectedItem.getName() + "."));
                    yield return StaticCoroutine.StartCoroutine(dialog.drawTextSilent("教导" + currentPokemon.getName() + selectedItem.getName() + "吗?"));
                    string[] choiceInfos = new string[] { "同意", "反对" };
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choiceInfos));
                    if (dialog.buttonIndex == 1)
                    {
                        yield return StaticCoroutine.StartCoroutine(LearnMove(currentPokemon, selectedItem.getTMNo()));
                    }
                }
                else
                {
                    yield return StaticCoroutine.StartCoroutine(LearnMove(currentPokemon, selectedItem.getTMNo()));
                }
                effectState = true;
            }
            else
            {
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "不能学习这个技能."));
                effectState = false;
            }
        }
        else if (selectedItem.getItemEffect() == ItemData.ItemEffect.UNIQUE)
        {
            //UNIQUE
            string selectedItemName = selectedItem.getName();
            if (selectedItemName == "Rare Candy")
            {
                if (currentPokemon.getLevel() < 100)
                {
                    currentPokemon.healHP(1);
                    currentPokemon.addExp(currentPokemon.getExpNext() - currentPokemon.getExp());
                    updatePartyInfo();
                    //itemViewModel.removeItem(selectedItem.getName(), 1); //remove item
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "的等级提升到" + currentPokemon.getLevel() + "!"));
                    long pkmnID = currentPokemon.getID();
                    if (currentPokemon.canEvolve("Level"))
                    {
                        BgmHandler.main.PlayOverlay(null, 0, 0.5f);
                        yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));

                        Scene.main.Evolution.gameObject.SetActive(true);
                        StaticCoroutine.StartCoroutine(Scene.main.Evolution.control(currentPokemon, "Level"));
                        while (Scene.main.Evolution.gameObject.activeSelf)
                        {
                            yield return null;
                        }

                        updatePartyInfo();
                        yield return StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
                    }
                    if (pkmnID == currentPokemon.getID())
                    {
                        long move = currentPokemon.MoveLearnedAtLevel(currentPokemon.getLevel());
                        if (move > 0 && !currentPokemon.HasMove(move))
                        {
                            yield return StaticCoroutine.StartCoroutine(LearnMove(currentPokemon, move));
                        }
                    }
                    effectState = true;
                }
                else
                {
                    effectState = false;
                }
            }
        }
        if (effectState)
        {
            if (battleViewModel != null)
            {
                itemViewModel.running = false;
                battleViewModel.jumpTurn();
            }
            //更新精灵能力
            List<Pokemon> partyPokemons = new List<Pokemon>();
            partyPokemons.Add(currentPokemon);
            PCManager.Instance.updatePokemons(partyPokemons, delegate (bool result)
            {
                if (!result)
                {
                    StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("更新精灵能力失败"));
                }
            });
        }
        else
        {
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("没有任何效果."));
            yield return StaticCoroutine.StartCoroutine(handler.ResetControl());
        }
    }

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
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(selectedPokemon.getName()
                        + "想要学习技能" + moveName + ".", 0.75f));
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(selectedPokemon.getName()
                        + "但是, " + selectedPokemon.getName() + "已经学会了四个技能.", 0.75f));
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("是否删除一个技能替换为" + moveName + "?", 0.75f));
                    string[] choiceInfos = new string[] { "是", "否" };
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choiceInfos));
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
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("呼地!"));

                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(selectedPokemon.getName()
                               + "忘记使用" + MoveDatabase.getMoveName(replacedMove) + ".", 0.75f));
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("并且...", 0.75f));

                            AudioClip mfx = Resources.Load<AudioClip>("Audio/mfx/GetAverage");
                            BgmHandler.main.PlayMFX(mfx);
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(
                                selectedPokemon.getName() + "学习了技能" + moveName + "!"));
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
                        yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choiceInfos));
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
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(selectedPokemon.getName() + "学习了技能" + moveName + "!"));
                    learning = false;
                }
            }
        }
        if (chosenIndex == 0)
        {
            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(selectedPokemon.getName() + "没有学习技能" + move + "."));
        }
    }
}
