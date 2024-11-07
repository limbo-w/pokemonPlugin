using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using Proyecto26;
using UnityEngine;

public class PCViewModel : ViewModelBase
{
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
    private PCHandler pcHandler;
    public bool running = false;
    public bool carrying = false;
    private int currentBoxID;
    private int selectedBoxID;
    private int selectedIndex;
    //当前精灵位置
    private int currentPosition;
    private Pokemon selectedPokemon;
    public ObservableList<SelectedInfo> selectedInfoList;
    public ObservableList<PartyAndBoxInfo> partyAndBoxInfoList;
    public ObservableList<MoveBoxInfo> moveBoxInfoList;
    public ObservableList<OperationPokemon> operationPokemonList;
    public ObservableList<MoveCursor> moveCursorList;

    public class NotifyCollectionChangedObject
    {
        public NotifyCollectionChangedEventHandler selectedInfoHandler;
        public NotifyCollectionChangedEventHandler partyAndBoxInfoHandler;
        public NotifyCollectionChangedEventHandler moveBoxInfoHandler;
        public NotifyCollectionChangedEventHandler operationPokemonHandler;
        public NotifyCollectionChangedEventHandler moveCursorHandler;
    }

    public PCViewModel(PCHandler pcHandler, NotifyCollectionChangedObject changedObject)
    {
        this.pcHandler = pcHandler;
        selectedInfoList = new ObservableList<SelectedInfo>();
        selectedInfoList.CollectionChanged += changedObject.selectedInfoHandler;
        selectedInfoList.Add(new SelectedInfo());
        partyAndBoxInfoList = new ObservableList<PartyAndBoxInfo>();
        partyAndBoxInfoList.CollectionChanged += changedObject.partyAndBoxInfoHandler;
        partyAndBoxInfoList.Add(new PartyAndBoxInfo());
        moveBoxInfoList = new ObservableList<MoveBoxInfo>();
        moveBoxInfoList.CollectionChanged += changedObject.moveBoxInfoHandler;
        moveBoxInfoList.Add(new MoveBoxInfo());
        operationPokemonList = new ObservableList<OperationPokemon>();
        operationPokemonList.CollectionChanged += changedObject.operationPokemonHandler;
        operationPokemonList.Add(new OperationPokemon());
        moveCursorList = new ObservableList<MoveCursor>();
        moveCursorList.CollectionChanged += changedObject.moveCursorHandler;
        moveCursorList.Add(new MoveCursor());
        this.onPartyIcon = new SimpleCommand<int>(OnPartyIconClick);
        this.onCurrentBoxIcon = new SimpleCommand<int>(OnCurrentBoxIconClick);
    }

    public void reset()
    {
        dialog = DialogBoxUIHandler.main;
        running = true;
        currentPosition = 0;
        currentBoxID = 1;
        Pokemon pokemon = PCManager.Instance.getPCBox(currentBoxID)[currentPosition];
        updateBoxesAndParty();
        //更新指定的宝可梦
        updateSelectedInfo(pokemon);
    }

    //更新盒子和精灵
    private void updateBoxesAndParty()
    {
        for (int i = 0; i < 30; i++)
        {
            Pokemon pokemon = PCManager.Instance.getPCBox(currentBoxID)[i];
            if (pokemon == null)
            {
                partyAndBoxInfoList[0] = new PartyAndBoxInfo()
                {
                    CurrentBoxIndex = i,
                    PartyAndBoxInfoPokemon = null
                };
            }
            else
            {
                partyAndBoxInfoList[0] = new PartyAndBoxInfo()
                {
                    CurrentBoxIndex = i,
                    PartyAndBoxInfoPokemon = pokemon
                };
            }
        }
        for (int i = 0; i < 6; i++)
        {
            Pokemon pokemon = PCManager.Instance.getPCBox(0)[i];
            if (pokemon == null)
            {
                partyAndBoxInfoList[0] = new PartyAndBoxInfo()
                {
                    PartyIndex = i,
                    PartyAndBoxInfoPokemon = null
                };
            }
            else
            {
                partyAndBoxInfoList[0] = new PartyAndBoxInfo()
                {
                    PartyIndex = i,
                    PartyAndBoxInfoPokemon = pokemon
                };
            }
        }
    }

    //更新选择精灵信息
    private void updateSelectedInfo(Pokemon selectedPokemon)
    {
        selectedInfoList[0] = new SelectedInfo()
        {
            Pokemon = selectedPokemon
        };
    }

    //交换精灵
    private IEnumerator switchPokemon(int currentBoxID, int currentPosition)
    {
        Pokemon pokemon = PCManager.Instance.getPCBox(currentBoxID)[currentPosition];
        if (pokemon != null)
        {
            operationPokemonList[0] = new OperationPokemon()
            {
                Type = OperationPokemon.OperationType.Switch,
                CurrentBoxID = currentBoxID,
                CurrentPosition = currentPosition
            };
            updateSelectedInfo(pokemon);
            PCManager.Instance.swapPokemon(selectedBoxID, selectedIndex, currentBoxID, currentPosition, delegate (bool result)
            {
                if (currentBoxID != 0)
                {
                    pokemon.healFull();
                }
            });
        }
        yield return null;
    }

    //收回宝可梦
    private IEnumerator withdrawPokemon(int currentPosition)
    {
        int targetPosition = 6;
        for (int i = 1; i < 6; i++)
        {
            Pokemon pokemon = PCManager.Instance.getPartyBox()[i];
            if (pokemon == null)
            {
                targetPosition = i;
                i = 6;
            }
        }
        if (targetPosition < 6)
        {
            yield return StaticCoroutine.StartCoroutine(pickUpPokemon(currentBoxID, currentPosition, false));
            //停顿0.5s用来等待grabbedPokemon清空
            yield return new WaitForSeconds(0.5f);
            yield return StaticCoroutine.StartCoroutine(putDownPokemon(0, targetPosition, false));
        }
    }

    //存储宝可梦
    private IEnumerator depositPokemon(int currentPosition, int targetPosition)
    {
        if (targetPosition < 30)
        {
            yield return StaticCoroutine.StartCoroutine(pickUpPokemon(0, currentPosition, false));
            //停顿0.5s用来等待grabbedPokemon清空
            yield return new WaitForSeconds(0.5f);
            yield return StaticCoroutine.StartCoroutine(putDownPokemon(currentBoxID, targetPosition, false));
        }
    }

    //拿起宝可梦
    private IEnumerator pickUpPokemon(int currentBoxID, int currentPosition, bool isOperation = true)
    {
        selectedPokemon = PCManager.Instance.getPCBox(currentBoxID)[currentPosition];
        selectedBoxID = currentBoxID;
        selectedIndex = currentPosition;
        carrying = true;
        if (isOperation)
        {
            operationPokemonList[0] = new OperationPokemon()
            {
                Type = OperationPokemon.OperationType.PickUp,
                Pokemon = selectedPokemon,
                CurrentPosition = currentPosition,
                CurrentBoxID = currentBoxID
            };
        }
        yield return null;
    }

    //放下精灵
    private IEnumerator putDownPokemon(int currentBoxID, int currentPosition, bool isOperation = true)
    {
        bool originalSpot = false;
        if (currentBoxID == selectedBoxID && currentPosition == selectedIndex)
        {
            originalSpot = true;
        }
        //查找是否有精灵
        Pokemon pokemon = PCManager.Instance.getPCBox(currentBoxID)[currentPosition];
        if (pokemon == null || originalSpot)
        {
            yield return null;
            PCManager.Instance.swapPokemon(selectedBoxID, selectedIndex, currentBoxID, currentPosition, delegate
            {
                carrying = false;
                if (pokemon != null && currentBoxID != 0)
                {
                    pokemon.healFull();
                    updateSelectedInfo(pokemon);
                }
                SfxHandler.Play(pcHandler.putDownClip);
                if (isOperation)
                {
                    operationPokemonList[0] = new OperationPokemon()
                    {
                        Type = OperationPokemon.OperationType.PutDown,
                        CurrentPosition = currentPosition,
                        CurrentBoxID = currentBoxID,
                    };
                }
                updateBoxesAndParty();
            });
        }
    }

    //点击对应的PokemonIcon
    private IEnumerator selectPokemonIcon(int selectBoxID, int currentPosition)
    {
        Pokemon pokemon = PCManager.Instance.getPCBox(selectBoxID)[currentPosition];
        if (pokemon != null)
        {
            updateSelectedInfo(pokemon);
            if (!carrying)
            {
                dialog.drawTextInstant("你想要对" + pokemon.getName() + "干什么呢?");
                string[] choices = new string[]
                  {"拿起", "概述", "物品", "收回", "放生", "取消"};
                if (selectBoxID == 0)
                {
                    choices[3] = "存储";
                }
                yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choices));
                int chosenIndex = dialog.buttonIndex;
                if (chosenIndex == 5)
                {
                    SfxHandler.Play(pcHandler.selectClip);
                    yield return StaticCoroutine.StartCoroutine(pickUpPokemon(selectBoxID, currentPosition));
                }
                else if (chosenIndex == 4)
                {
                    SfxHandler.Play(pcHandler.selectClip);
                    StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(false, 0.4f));
                    Scene.main.Summary.gameObject.SetActive(true);
                    StaticCoroutine.StartCoroutine(Scene.main.Summary.control(PCManager.Instance.getPCBox(selectBoxID),
                            currentPosition));
                    while (Scene.main.Summary.gameObject.activeSelf)
                    {
                        yield return new WaitForSeconds(0.2f);
                    }
                    StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, 0.4f));
                }
                else if (chosenIndex == 3)
                {
                    //todo 需要修改
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("暂时不支持这个功能"));
                    //Pokemon currentPokemon = pokemon;
                    //if (!string.IsNullOrEmpty(currentPokemon.getHeldItem().itemName))
                    //{
                    //    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(currentPokemon.getName() + "正在持有"
                    //        + currentPokemon.getHeldItem() + "."));
                    //    string[] itemChoices = new string[]
                    //    {
                    //            "交换", "持有", "取消"
                    //    };
                    //    yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(itemChoices));
                    //    int itemChosenIndex = dialog.buttonIndex;
                    //    if (itemChosenIndex == 2)
                    //    {
                    //        //交换
                    //        SfxHandler.Play(pcHandler.selectClip);
                    //        StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(false, 0.4f));

                    //        Scene.main.Bag.gameObject.SetActive(true);
                    //        StaticCoroutine.StartCoroutine(Scene.main.Bag.control());
                    //        while (Scene.main.Bag.gameObject.activeSelf)
                    //        {
                    //            yield return new WaitForSeconds(0.2f);
                    //        }
                    //        BagItem chosenItem = Scene.main.Bag.getChosenItem();
                    //        StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, 0.4f));

                    //        if (!string.IsNullOrEmpty(chosenItem.itemName))
                    //        {
                    //            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("交换" + currentPokemon.getHeldItem() + "为" + chosenItem + "?"));
                    //            string[] choiceInfos = new string[] { "同意", "反对" };
                    //            yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choiceInfos));
                    //            itemChosenIndex = dialog.buttonIndex;
                    //            if (itemChosenIndex == 1)
                    //            {
                    //                SfxHandler.Play(pcHandler.selectClip);
                    //                BagItem item = currentPokemon.swapHeldItem(chosenItem);
                    //                BagItem receivedItem = new BagItem()
                    //                {
                    //                    amount = 1,
                    //                    itemName = item.itemName,
                    //                    holdId = item.holdId
                    //                };
                    //                BagManager.Instance.addItem(receivedItem, delegate (bool result)
                    //                {
                    //                    StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("给予" + chosenItem + "给"
                    //                                                               + currentPokemon.getName() + ","));
                    //                    StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("并且获得" + receivedItem + "."));
                    //                });
                    //                BagItem removeItem = new BagItem()
                    //                {
                    //                    amount = 1,
                    //                    itemName = chosenItem.itemName,
                    //                    holdId = chosenItem.holdId
                    //                };
                    //                BagManager.Instance.removeItem(removeItem, delegate (bool result)
                    //                {

                    //                });
                    //            }
                    //            else
                    //            {
                    //                SfxHandler.Play(pcHandler.selectClip);
                    //            }
                    //        }
                    //    }
                    //    else if (itemChosenIndex == 1)
                    //    {
                    //        //持有
                    //        SfxHandler.Play(pcHandler.selectClip);
                    //        BagItem receivedItem = currentPokemon.swapHeldItem(null);
                    //        BagManager.Instance.addItem(receivedItem, delegate (bool result)
                    //        {
                    //            updateSelectedInfo(currentPokemon);
                    //            StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(
                    //                currentPokemon.getName() + "持有了" + receivedItem));
                    //        });
                    //    }
                    //}
                    //else
                    //{
                    //    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(
                    //        currentPokemon.getName() + "没有持有任何物品."));
                    //    string[] itemChoices = new string[] { "给予", "取消" };
                    //    yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(itemChoices));
                    //    int itemChosenIndex = dialog.buttonIndex;
                    //    if (itemChosenIndex == 1)
                    //    {
                    //        //给予
                    //        SfxHandler.Play(pcHandler.selectClip);
                    //        StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(false, 0.4f));
                    //        Scene.main.Bag.gameObject.SetActive(true);
                    //        StaticCoroutine.StartCoroutine(Scene.main.Bag.control());
                    //        while (Scene.main.Bag.gameObject.activeSelf)
                    //        {
                    //            yield return new WaitForSeconds(0.2f);
                    //        }
                    //        BagItem chosenItem = Scene.main.Bag.getChosenItem();
                    //        StaticCoroutine.StartCoroutine(ScreenFade.main.Fade(true, 0.4f));

                    //        if (!string.IsNullOrEmpty(chosenItem.itemName))
                    //        {
                    //            currentPokemon.swapHeldItem(chosenItem);
                    //            BagManager.Instance.removeItem(chosenItem, delegate (bool result)
                    //            {
                    //                updateSelectedInfo(currentPokemon);
                    //                StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("给予" + chosenItem + "给"
                    //                    + currentPokemon.getName() + "."));
                    //            });
                    //        }
                    //    }
                    //    else
                    //    {
                    //        SfxHandler.Play(pcHandler.selectClip);
                    //    }
                    //}
                }
                else if (chosenIndex == 2)
                {
                    //存储或者收回
                    SfxHandler.Play(pcHandler.selectClip);
                    if (selectBoxID == 0)
                    {
                        if (PCManager.Instance.getPartyBox()[1] != null)
                        {
                            int targetPosition = 30;
                            for (int i = 0; i < 30; i++)
                            {
                                if (PCManager.Instance.getPCBox(currentBoxID)[i] == null)
                                {
                                    targetPosition = i;
                                    i = 30;
                                }
                            }
                            if (targetPosition >= 30)
                            {
                                yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("箱子已经满了!"));
                            }
                            else
                            {
                                yield return StaticCoroutine.StartCoroutine(depositPokemon(currentPosition, targetPosition));
                                updateSelectedInfo(PCManager.Instance.getPartyBox()[currentPosition]);
                            }
                        }
                        else
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("这是你最后的精灵了!"));
                        }
                    }
                    else
                    {
                        if (PCManager.Instance.getPartyBox()[5] != null)
                        {
                            yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("你的队伍满员了!"));
                        }
                        else
                        {
                            StaticCoroutine.StartCoroutine(withdrawPokemon(currentPosition));
                            updateSelectedInfo(PCManager.Instance.getPCBox(currentBoxID)[currentPosition]);
                        }
                    }
                }
                else if (chosenIndex == 1)
                {
                    //释放
                    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("暂时不支持这个功能"));
                    //SfxHandler.Play(pcHandler.selectClip);
                    //int releaseIndex = 1;
                    //string pokemonName = PCManager.Instance.getPCBox(selectBoxID)[currentPosition].getName();
                    //while (releaseIndex != 0)
                    //{
                    //    yield return StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("你想要释放" + pokemonName + "吗?"));
                    //    string[] choiceInfos = new string[] { "同意", "反对" };
                    //    yield return StaticCoroutine.StartCoroutine(CommonUtils.showChoiceText(choiceInfos));
                    //    releaseIndex = dialog.buttonIndex;
                    //    if (releaseIndex == 1)
                    //    {
                    //        SfxHandler.Play(pcHandler.selectClip);
                    //        releaseIndex = 0;
                    //        PCManager.Instance.releasePokemon(pokemon, delegate (bool result)
                    //        {
                    //            if (result)
                    //            {
                    //                StaticCoroutine.StartCoroutine(CommonUtils.showDialogText(pokemonName + "被释放了."));
                    //                StaticCoroutine.StartCoroutine(CommonUtils.showDialogText("再见, " + pokemonName + "!"));
                    //            }
                    //        });
                    //    }
                    //}
                }
            }
            else if (selectBoxID == selectedBoxID && currentPosition == selectedIndex)
            {
                StaticCoroutine.StartCoroutine(putDownPokemon(selectBoxID, currentPosition));
            }
            else
            {
                StaticCoroutine.StartCoroutine(switchPokemon(selectBoxID, currentPosition));
            }
        }
        else if (carrying)
        {
            StaticCoroutine.StartCoroutine(putDownPokemon(selectBoxID, currentPosition));
        }
    }

    //移动box盒子
    private IEnumerator moveBox(int direction)
    {
        moveBoxInfoList[0] = new MoveBoxInfo()
        {
            Direction = direction
        };
        currentBoxID += direction;
        int boxLength = PCManager.Instance.getBoxLength();

        if (currentBoxID < 1)
        {
            currentBoxID = boxLength - 1;
        }
        else if (currentBoxID > boxLength - 1)
        {
            currentBoxID = 1;
        }
        for (int i = 0; i < 30; i++)
        {
            Pokemon pokemon = PCManager.Instance.getPCBox(currentBoxID)[i];
            if (pokemon == null)
            {
                partyAndBoxInfoList[0] = new PartyAndBoxInfo()
                {
                    CurrentBoxIndex = i,
                    PartyAndBoxInfoPokemon = null
                };
            }
            else
            {
                partyAndBoxInfoList[0] = new PartyAndBoxInfo()
                {
                    PartyIndex = i,
                    PartyAndBoxInfoPokemon = pokemon

                };
            }
        }
        yield return null;
    }
    //==========================================================================
    public class SelectedInfo : ObservableObject
    {
        private Pokemon pokemon;

        public Pokemon Pokemon
        {
            get { return this.pokemon; }
            set { this.Set<Pokemon>(ref this.pokemon, value, "Pokemon"); }
        }
    }

    public class PartyAndBoxInfo : ObservableObject
    {
        private int currentBoxIndex = -1;
        private int partyIndex = -1;
        private Pokemon partyAndBoxPokemon;

        public Pokemon PartyAndBoxInfoPokemon
        {
            get { return this.partyAndBoxPokemon; }
            set { this.Set<Pokemon>(ref this.partyAndBoxPokemon, value, "PartyAndBoxInfoPokemon"); }
        }

        public int CurrentBoxIndex
        {
            get { return this.currentBoxIndex; }
            set { this.Set<int>(ref this.currentBoxIndex, value, "CurrentBoxIndex"); }
        }

        public int PartyIndex
        {
            get { return this.partyIndex; }
            set { this.Set<int>(ref this.partyIndex, value, "PartyIndex"); }
        }
    }

    public class MoveBoxInfo : ObservableObject
    {
        private int direction;

        public int Direction
        {
            get { return this.direction; }
            set { this.Set<int>(ref this.direction, value, "Direction"); }
        }
    }

    public class OperationPokemon : ObservableObject
    {
        public enum OperationType
        {
            PickUp,
            PutDown,
            Switch
        }
        private OperationType type;
        private int currentPosition;
        private int currentBoxID;
        private Pokemon pokemon;

        public OperationType Type
        {
            get { return this.type; }
            set { this.Set<OperationType>(ref this.type, value, "Type"); }
        }

        public Pokemon Pokemon
        {
            get { return this.pokemon; }
            set { this.Set<Pokemon>(ref this.pokemon, value, "Pokemon"); }
        }


        public int CurrentBoxID
        {
            get { return this.currentBoxID; }
            set { this.Set<int>(ref this.currentBoxID, value, "CurrentBoxID"); }
        }


        public int CurrentPosition
        {
            get { return this.currentPosition; }
            set { this.Set<int>(ref this.currentPosition, value, "CurrentPosition"); }
        }
    }

    public class MoveCursor : ObservableObject
    {
        public enum CursorType
        {
            Party,
            Box
        }
        private CursorType type;
        private int currentPosition;

        public CursorType Type
        {
            get { return this.type; }
            set { this.Set<CursorType>(ref this.type, value, "Type"); }
        }

        public int CurrentPosition
        {
            get { return this.currentPosition; }
            set { this.Set<int>(ref this.currentPosition, value, "CurrentPosition"); }
        }
    }

    //==========================================================================
    private readonly SimpleCommand<int> onPartyIcon;

    public ICommand OnPartyIcon
    {
        get { return this.onPartyIcon; }
    }

    public void OnPartyIconClick(int index)
    {
        //携带精灵选择
        SfxHandler.Play(pcHandler.selectClip);
        currentPosition = index;
        StaticCoroutine.StartCoroutine(selectPokemonIcon(0, currentPosition));
        moveCursorList[0] = new MoveCursor()
        {
            Type = MoveCursor.CursorType.Party,
            CurrentPosition = currentPosition
        };
    }

    private readonly SimpleCommand<int> onCurrentBoxIcon;

    public ICommand OnCurrentBoxIcon
    {
        get { return this.onCurrentBoxIcon; }
    }

    public void OnCurrentBoxIconClick(int index)
    {
        //电脑精灵选择
        SfxHandler.Play(pcHandler.selectClip);
        currentPosition = index;
        StaticCoroutine.StartCoroutine(selectPokemonIcon(currentBoxID, currentPosition));
        moveCursorList[0] = new MoveCursor()
        {
            Type = MoveCursor.CursorType.Box,
            CurrentPosition = currentPosition
        };
    }

    public void OnReturn()
    {
        PCManager.Instance.packParty();
        running = false;
    }

    public void OnLeft()
    {
        SfxHandler.Play(pcHandler.selectClip);
        StaticCoroutine.StartCoroutine(moveBox(-1));
    }

    public void OnRight()
    {
        SfxHandler.Play(pcHandler.selectClip);
        StaticCoroutine.StartCoroutine(moveBox(1));
    }
}
