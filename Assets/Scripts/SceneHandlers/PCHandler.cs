using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Loxodon.Framework.Views;
using Loxodon.Framework.Views.Variables;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Binding.Builder;
using System.Collections.Specialized;

public class PCHandler : UIView
{
    private Transform cursor;
    private RawImage cursorBack;
    private RawImage grabbedPokemon;
    private Image grabbedPokemonItem;
    public Texture boxBackTexture;

    public VariableArray variables;
    private PCViewModel pcViewModel;
    private float moveSpeed = 0.16f;
    private RawImage selectedSprite;
    private List<Texture> selectedSpriteAnimation;
    public AudioClip offClip;
    public AudioClip openClip;
    public AudioClip selectClip;
    public AudioClip pickUpClip;
    public AudioClip putDownClip;
    private GameObject currentBox;
    private Vector2[] partyPositions = new Vector2[6];
    //携带精灵图标
    private RawImage[] partyIcons = new RawImage[6];
    //携带物品
    private Image[] partyItems = new Image[6];
    private Text[] partyNames = new Text[6];
    private Text[] partyGenders = new Text[6];
    private Image[] partyLvs = new Image[6];
    private Text[] partyLevels = new Text[6];
    private Text[] partyCurrentHps = new Text[6];
    private Text[] partySlashs = new Text[6];
    private Text[] partyMaxHps = new Text[6];
    private Image[] partyHpBars = new Image[6];
    private Image[] partyHpBarBacks = new Image[6];

    //电脑精灵图标
    private RawImage[] currentBoxIcons = new RawImage[30];
    private Image[] currentBoxItems = new Image[30];
    private Text selectedName, selectedGender;
    private RawImage selectedType1, selectedType2;
    private Text selectedLevel, selectedItem;
    private RawImage selectedStatus;

    //选择精灵信息更新
    protected void OnSelectedInfoChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Replace:
                foreach (PCViewModel.SelectedInfo item in eventArgs.NewItems)
                {
                    Pokemon selectPokemon = item.Pokemon;
                    if (selectPokemon == null)
                    {
                        selectedName.text = null;
                        selectedGender.text = null;
                        selectedSpriteAnimation = new List<Texture>();
                        selectedSprite.texture = Resources.Load<Texture>("null");
                        selectedType1.texture = Resources.Load<Texture>("null");
                        selectedType2.texture = Resources.Load<Texture>("null");
                        selectedLevel.text = null;
                        selectedItem.text = null;
                        selectedStatus.texture = Resources.Load<Texture>("null");
                    }
                    else
                    {
                        selectedName.text = selectPokemon.getName();
                        if (selectPokemon.getGender() == Pokemon.Gender.FEMALE)
                        {
                            selectedGender.text = "♀";
                            selectedGender.color = new Color(1, 0.2f, 0.2f, 1);
                        }
                        else if (selectPokemon.getGender() == Pokemon.Gender.MALE)
                        {
                            selectedGender.text = "♂";
                            selectedGender.color = new Color(0.2f, 0.4f, 1, 1);
                        }
                        else
                        {
                            selectedGender.text = null;
                        }
                        selectedSpriteAnimation = selectPokemon.GetFrontAnimFromGif();
                        CommonUtils.changeAnimateImageScale(selectedSpriteAnimation, selectedSprite, 100.0f);
                        string type1 = PokemonDatabase.getPokemon(selectPokemon.getID()).getType1().ToString();
                        string type2 = PokemonDatabase.getPokemon(selectPokemon.getID()).getType2().ToString();
                        selectedType1.texture = Resources.Load<Texture>("null");
                        selectedType2.texture = Resources.Load<Texture>("null");
                        if (type1 != "NONE")
                        {
                            selectedType1.texture = Resources.Load<Texture>("Sprites/GUI/Types/type" + type1);
                        }
                        if (type2 != "NONE")
                        {
                            selectedType2.texture = Resources.Load<Texture>("Sprites/GUI/Types/type" + type2);
                        }
                        selectedLevel.text = "Level " + selectPokemon.getLevel();
                        selectedItem.text = "None";
                        if (selectPokemon.getHeldItem() != null)
                        {
                            selectedItem.text = selectPokemon.getHeldItem().itemName;
                        }
                        selectedStatus.texture = Resources.Load<Texture>("null");
                        if (selectPokemon.getStatus() != Pokemon.Status.NONE)
                        {
                            selectedStatus.texture =
                                Resources.Load<Texture>("Sprites/GUI/Status/status" + selectPokemon.getStatus().ToString());
                        }
                    }
                }
                break;
        }
    }

    //Party和Box信息更新
    protected void OnBoxesAndPartyChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Replace:
                foreach (PCViewModel.PartyAndBoxInfo item in eventArgs.NewItems)
                {
                    if (item.CurrentBoxIndex >= 0 && currentBoxIcons[item.CurrentBoxIndex] != null)
                    {
                        Pokemon pokemon = item.PartyAndBoxInfoPokemon;
                        if (pokemon != null)
                        {
                            bool itemEnable = false;
                            BagItem bagItem = pokemon.getHeldItem();
                            if (bagItem != null && !string.IsNullOrEmpty(bagItem.itemName))
                            {
                                itemEnable = true;
                            }
                            else
                            {
                                itemEnable = false;
                            }
                            currentBoxIcons[item.CurrentBoxIndex].texture = pokemon.GetIcons();
                            currentBoxItems[item.CurrentBoxIndex].enabled = itemEnable;
                        }
                        else
                        {
                            currentBoxIcons[item.CurrentBoxIndex].texture = Resources.Load<Texture>("null");
                            currentBoxItems[item.CurrentBoxIndex].enabled = false;
                        }
                    }
                    if (item.PartyIndex >= 0 && partyIcons[item.PartyIndex] != null)
                    {
                        Pokemon pokemon = item.PartyAndBoxInfoPokemon;
                        if (pokemon != null)
                        {
                            bool itemEnable = false;
                            BagItem bagItem = pokemon.getHeldItem();
                            if (bagItem != null && !string.IsNullOrEmpty(bagItem.itemName))
                            {
                                itemEnable = true;
                            }
                            else
                            {
                                itemEnable = false;
                            }
                            string gender = "";
                            if (pokemon.getGender() == Pokemon.Gender.FEMALE)
                            {
                                gender = "♀";
                            }
                            else if (pokemon.getGender() == Pokemon.Gender.MALE)
                            {
                                gender = "♂";
                            }
                            partyIcons[item.PartyIndex].texture = pokemon.GetIcons();
                            partyItems[item.PartyIndex].enabled = itemEnable;
                            partySlashs[item.PartyIndex].gameObject.SetActive(true);
                            partyNames[item.PartyIndex].gameObject.SetActive(true);
                            partyNames[item.PartyIndex].text = pokemon.getName();
                            partyLvs[item.PartyIndex].gameObject.SetActive(true);
                            partyLevels[item.PartyIndex].gameObject.SetActive(true);
                            partyLevels[item.PartyIndex].text = "" + pokemon.getLevel();
                            partyGenders[item.PartyIndex].gameObject.SetActive(true);
                            partyGenders[item.PartyIndex].text = gender;
                            partyCurrentHps[item.PartyIndex].gameObject.SetActive(true);
                            partyCurrentHps[item.PartyIndex].text = "" + pokemon.getCurrentHP();
                            partyMaxHps[item.PartyIndex].gameObject.SetActive(true);
                            partyMaxHps[item.PartyIndex].text = "" + pokemon.getHP();
                            partyHpBarBacks[item.PartyIndex].gameObject.SetActive(true);
                            partyHpBars[item.PartyIndex].gameObject.SetActive(true);
                            partyHpBars[item.PartyIndex].GetComponent<RectTransform>().localScale =
                                new Vector3((float)pokemon.getCurrentHP() / (float)pokemon.getHP(), 1, 1);
                        }
                        else
                        {
                            partyIcons[item.PartyIndex].texture = Resources.Load<Texture>("null");
                            partyItems[item.PartyIndex].enabled = false;
                            partySlashs[item.PartyIndex].gameObject.SetActive(false);
                            partyNames[item.PartyIndex].gameObject.SetActive(false);
                            partyLvs[item.PartyIndex].gameObject.SetActive(false);
                            partyLevels[item.PartyIndex].gameObject.SetActive(false);
                            partyGenders[item.PartyIndex].gameObject.SetActive(false);
                            partyCurrentHps[item.PartyIndex].gameObject.SetActive(false);
                            partyMaxHps[item.PartyIndex].gameObject.SetActive(false);
                            partyHpBarBacks[item.PartyIndex].gameObject.SetActive(false);
                        }
                    }
                }
                break;
        }
    }

    protected void OnMoveBoxChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Replace:
                foreach (PCViewModel.MoveBoxInfo item in eventArgs.NewItems)
                {
                    StartCoroutine(moveBox(item.Direction));
                }
                break;
        }
    }

    protected void OnOperationPokemonChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Replace:
                foreach (PCViewModel.OperationPokemon item in eventArgs.NewItems)
                {
                    if (item.Type == PCViewModel.OperationPokemon.OperationType.PickUp)
                    {
                        StartCoroutine(pickUpPokemon(item));
                    }
                    else if (item.Type == PCViewModel.OperationPokemon.OperationType.PutDown)
                    {
                        StartCoroutine(putDownPokemon(item));
                    }
                    else if (item.Type == PCViewModel.OperationPokemon.OperationType.Switch)
                    {
                        StartCoroutine(switchPokemon(item));
                    }
                }
                break;
        }
    }

    //光标位置改变（Party/Box两种）
    protected void OnMoveCursorChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Replace:
                foreach (PCViewModel.MoveCursor item in eventArgs.NewItems)
                {
                    if (item.Type == PCViewModel.MoveCursor.CursorType.Party && partyIcons[item.CurrentPosition] != null)
                    {
                        Vector3 partyPostion = partyIcons[item.CurrentPosition].transform.position;
                        StartCoroutine(moveCursor(new Vector2(partyPostion.x, partyPostion.y + 10)));
                    }
                    else if (item.Type == PCViewModel.MoveCursor.CursorType.Box && currentBoxIcons[item.CurrentPosition] != null)
                    {
                        Vector3 currentBoxIconPostion = currentBoxIcons[item.CurrentPosition].transform.position;
                        StartCoroutine(moveCursor(new Vector2(currentBoxIconPostion.x, currentBoxIconPostion.y + 10)));
                    }
                }
                break;
        }
    }

    protected override void Awake()
    {
        cursor = transform.Find("Cursor");
        cursorBack = cursor.Find("CursorBack").GetComponent<RawImage>();
        grabbedPokemon = cursor.transform.Find("GrabbedPokemon").GetComponent<RawImage>();
        grabbedPokemon.texture = Resources.Load<Texture>("null");
        grabbedPokemonItem = grabbedPokemon.transform.Find("Item").GetComponent<Image>();
        grabbedPokemonItem.sprite= Resources.Load<Sprite>("null");
        currentBox = transform.Find("CurrentBox").gameObject;
        Transform selectedInfo = currentBox.transform.Find("SelectedInfo");
        selectedName = selectedInfo.Find("SelectedName").GetComponent<Text>();
        selectedSprite = selectedInfo.Find("SelectedSprite").GetComponent<RawImage>();
        selectedLevel = selectedInfo.Find("SelectedLevel").GetComponent<Text>();
        selectedGender = selectedInfo.Find("SelectedGender").GetComponent<Text>();
        selectedType1 = selectedInfo.Find("SelectedType1").GetComponent<RawImage>();
        selectedType2 = selectedInfo.Find("SelectedType2").GetComponent<RawImage>();
        selectedItem = selectedInfo.Find("SelectedItem").GetComponent<Text>();
        selectedStatus = selectedInfo.Find("SelectedStatus").GetComponent<RawImage>();

        for (int i = 0; i < 6; i++)
        {
            partyIcons[i] = transform.Find("Party/Back" + i).Find("Pokemon").GetComponent<RawImage>();
            partyIcons[i].texture = Resources.Load<Texture>("null");
            partyItems[i] = partyIcons[i].transform.Find("Item").GetComponent<Image>();
            partyItems[i].sprite = Resources.Load<Sprite>("null");
            partyNames[i] = transform.Find("Party/Back" + i).Find("Name").GetComponent<Text>();
            partyGenders[i] = transform.Find("Party/Back" + i).Find("Gender").GetComponent<Text>();
            partyLvs[i] = transform.Find("Party/Back" + i).Find("Lv").GetComponent<Image>();
            partyLevels[i] = transform.Find("Party/Back" + i).Find("Level").GetComponent<Text>();
            partyCurrentHps[i] = transform.Find("Party/Back" + i).Find("CurrentHP").GetComponent<Text>();
            partySlashs[i] = transform.Find("Party/Back" + i).Find("Slash").GetComponent<Text>();
            partyMaxHps[i] = transform.Find("Party/Back" + i).Find("MaxHP").GetComponent<Text>();
            partyHpBarBacks[i]= transform.Find("Party/Back" + i).Find("HPBarBack").GetComponent<Image>();
            partyHpBars[i] = transform.Find("Party/Back" + i).Find("HPBarBack/HPBar").GetComponent<Image>();
            partyPositions[i] = new Vector2(partyIcons[i].GetComponent<RectTransform>().rect.x, partyIcons[i].GetComponent<RectTransform>().rect.y);
        }
        for (int i = 0; i < 30; i++)
        {
            currentBoxIcons[i] = currentBox.transform.Find("Back" + i + "/Pokemon").GetComponent<RawImage>();
            currentBoxIcons[i].texture = Resources.Load<Texture>("null");
            currentBoxItems[i] = currentBoxIcons[i].transform.Find("Item").GetComponent<Image>();
            currentBoxItems[i].sprite = Resources.Load<Sprite>("null");

        }

        PCViewModel.NotifyCollectionChangedObject changedObject = new PCViewModel.NotifyCollectionChangedObject();
        changedObject.selectedInfoHandler = OnSelectedInfoChanged;
        changedObject.partyAndBoxInfoHandler = OnBoxesAndPartyChanged;
        changedObject.moveBoxInfoHandler = OnMoveBoxChanged;
        changedObject.operationPokemonHandler = OnOperationPokemonChanged;
        changedObject.moveCursorHandler = OnMoveCursorChanged;
        pcViewModel = new PCViewModel(this, changedObject);
    }

    protected override void Start()
    {
        IBindingContext bindingContext = this.BindingContext();
        bindingContext.DataContext = pcViewModel;
        BindingSet<PCHandler, PCViewModel> bindingSet = this.CreateBindingSet<PCHandler, PCViewModel>();
        for (int i = 0; i < 6; i++)
        {
            bindingSet.Bind(variables.Get<Button>("pokemon" + i))
                 .For(v => v.onClick).To(vm => vm.OnPartyIcon).CommandParameter(i);
        }
        for (int i = 0; i < 30; i++)
        {
            bindingSet.Bind(variables.Get<Button>("back" + i))
                 .For(v => v.onClick).To(vm => vm.OnCurrentBoxIcon).CommandParameter(i);
        }
        bindingSet.Bind(variables.Get<Button>("return")).For(v => v.onClick).To(vm => vm.OnReturn);
        bindingSet.Bind(variables.Get<Button>("left")).For(v => v.onClick).To(vm => vm.OnLeft);
        bindingSet.Bind(variables.Get<Button>("right")).For(v => v.onClick).To(vm => vm.OnRight);
        bindingSet.Build();
        this.gameObject.SetActive(false);
    }

    private IEnumerator switchPokemon(PCViewModel.OperationPokemon item)
    {
        if (cursorBack != null)
        {
            RawImage targetIcon = null;
            Image targetItem = null;
            if (item.CurrentBoxID == 0)
            {
                targetIcon = partyIcons[item.CurrentPosition];
                targetItem = partyItems[item.CurrentPosition];
            }
            else
            {
                targetIcon = currentBoxIcons[item.CurrentPosition];
                targetItem = currentBoxItems[item.CurrentPosition];
            }
            cursorBack.uvRect = new Rect(0f, 0f, cursorBack.uvRect.width, cursorBack.uvRect.height);
            SfxHandler.Play(putDownClip);
            StartCoroutine(moveIcon(grabbedPokemon, new Vector2(grabbedPokemon.GetComponent<RectTransform>().rect.x + 5,
                grabbedPokemon.GetComponent<RectTransform>().rect.y - 5)));
            yield return StartCoroutine(moveIcon(targetIcon, new Vector2(targetIcon.GetComponent<RectTransform>().rect.x - 5,
                targetIcon.GetComponent<RectTransform>().rect.y + 5)));

            Texture temp = targetIcon.texture;
            bool itemTemp = targetItem.enabled;
            targetIcon.GetComponent<RectTransform>().rect.Set(targetIcon.GetComponent<RectTransform>().rect.x + 10,
                targetIcon.GetComponent<RectTransform>().rect.y, targetIcon.GetComponent<RectTransform>().rect.width,
                targetIcon.GetComponent<RectTransform>().rect.height);
            targetItem.GetComponent<RectTransform>().rect.Set(targetItem.GetComponent<RectTransform>().rect.x + 10,
                targetItem.GetComponent<RectTransform>().rect.y, targetItem.GetComponent<RectTransform>().rect.width,
                targetItem.GetComponent<RectTransform>().rect.height);
            targetIcon.texture = grabbedPokemon.texture;
            targetItem.enabled = grabbedPokemonItem.enabled;
            grabbedPokemon.GetComponent<RectTransform>().rect.Set(grabbedPokemon.GetComponent<RectTransform>().rect.x - 10,
                grabbedPokemon.GetComponent<RectTransform>().rect.y, grabbedPokemon.GetComponent<RectTransform>().rect.width,
                grabbedPokemon.GetComponent<RectTransform>().rect.height);
            grabbedPokemonItem.GetComponent<RectTransform>().rect.Set(grabbedPokemonItem.GetComponent<RectTransform>().rect.x - 10,
                grabbedPokemonItem.GetComponent<RectTransform>().rect.y, grabbedPokemonItem.GetComponent<RectTransform>().rect.width,
                grabbedPokemonItem.GetComponent<RectTransform>().rect.height);
            grabbedPokemon.texture = temp;
            grabbedPokemonItem.enabled = itemTemp;

            SfxHandler.Play(pickUpClip);
            StartCoroutine(moveIcon(grabbedPokemon,
                new Vector2(grabbedPokemon.GetComponent<RectTransform>().rect.x + 5, grabbedPokemon.GetComponent<RectTransform>().rect.y + 5)));
            yield return
                StartCoroutine(moveIcon(targetIcon,
                    new Vector2(targetIcon.GetComponent<RectTransform>().rect.x - 5, targetIcon.GetComponent<RectTransform>().rect.y - 5)));
            cursorBack.uvRect = new Rect(0.5f, 0, cursorBack.uvRect.width, cursorBack.uvRect.height);
        }
    }

    private IEnumerator pickUpPokemon(PCViewModel.OperationPokemon item)
    {
        if (cursorBack != null)
        {
            cursorBack.uvRect = new Rect(0f, 0, cursorBack.uvRect.width, cursorBack.uvRect.height);
            SfxHandler.Play(pickUpClip);
            yield return StartCoroutine(moveCursor(new Vector2(cursor.transform.position.x, cursor.transform.position.y - 10)));
            Pokemon selectedPokemon = item.Pokemon;
            if (selectedPokemon.GetIcons() != null)
            {
                grabbedPokemon.texture = selectedPokemon.GetIcons();
            }
            BagItem bagItem = selectedPokemon.getHeldItem();
            if (bagItem != null && !string.IsNullOrEmpty(bagItem.itemName))
            {
                grabbedPokemonItem.enabled = true;
            }
            else
            {
                grabbedPokemonItem.enabled = false;
            }
            if (item.CurrentBoxID == 0)
            {
                partyIcons[item.CurrentPosition].texture = Resources.Load<Texture>("null");
                partyItems[item.CurrentPosition].enabled = false;
            }
            else
            {
                currentBoxIcons[item.CurrentPosition].texture = Resources.Load<Texture>("null");
                currentBoxItems[item.CurrentPosition].enabled = false;
            }
            cursorBack.uvRect = new Rect(0.5f, 0, cursorBack.uvRect.width, cursorBack.uvRect.height);
            yield return StartCoroutine(moveCursor(new Vector2(cursor.transform.position.x, cursor.transform.position.y + 10)));
        }
    }

    private IEnumerator putDownPokemon(PCViewModel.OperationPokemon item)
    {
        if (cursorBack != null)
        {
            if (item.CurrentBoxID == 0)
            {
                partyIcons[item.CurrentPosition].texture = grabbedPokemon.texture;
                partyItems[item.CurrentPosition].enabled = grabbedPokemonItem.enabled;
            }
            else
            {
                currentBoxIcons[item.CurrentPosition].texture = grabbedPokemon.texture;
                currentBoxItems[item.CurrentPosition].enabled = grabbedPokemonItem.enabled;
            }
            grabbedPokemon.texture = Resources.Load<Texture>("null");
            grabbedPokemonItem.enabled = false;
            cursorBack.uvRect = new Rect(0f, 0f, cursorBack.uvRect.width, cursorBack.uvRect.height);
            yield return null;
        }
    }

    //移动光标位置
    private IEnumerator moveCursor(Vector3 position)
    {
        float startX = cursor.transform.position.x;
        float startY = cursor.transform.position.y;
        float distanceX = position.x - startX;
        float distanceY = position.y - startY;
        float increment = 0;

        yield return null;
        while (increment < 1)
        {
            increment += (1 / moveSpeed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            cursor.transform.position = new Vector3(startX + (increment * distanceX), startY + (increment * distanceY), 0);
            yield return null;
        }
    }

    //移动上下Box
    private IEnumerator moveBox(int direction)
    {
        float increment = 0;
        float scrollSpeed = 0.4f;
        Vector3 startPosition = currentBox.transform.position;
        if (direction > 0)
        {
            Vector3 destinationPosition = startPosition + new Vector3(-0.537f, 0, 0);
            while (increment <= 1)
            {
                increment += (1 / scrollSpeed) * Time.deltaTime;
                currentBox.transform.position = Vector3.Lerp(startPosition, destinationPosition, increment);
                yield return null;
            }
        }
        else if (direction < 0)
        {
            Vector3 destinationPosition = startPosition + new Vector3(0.537f, 0, 0);
            while (increment <= 1)
            {
                increment += (1 / scrollSpeed) * Time.deltaTime;
                currentBox.transform.position = Vector3.Lerp(startPosition, destinationPosition, increment);
                yield return null;
            }
        }
    }

    public IEnumerator control()
    {
        Scene.main.SetMapButtonVisible(false);
        yield return StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
        grabbedPokemonItem.enabled = false;
        pcViewModel.running = true;
        //持有精灵动画
        StartCoroutine("animateIcons");
        StartCoroutine("animateCursor");
        StartCoroutine("animatePokemon");
        PCManager.Instance.fetchBoxPokemonList(delegate (bool result)
        {
            if (result)
            {
                pcViewModel.reset();
            }
            else
            {
                pcViewModel.running = false;
            }
        });

        pcViewModel.running = true;
        while (pcViewModel.running)
        {
            yield return null;
        }
        StopCoroutine("animateIcons");
        StopCoroutine("animateCursor");
        StopCoroutine("animatePokemon");
        SfxHandler.Play(offClip);
        yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
        //todo后面恢复
        GlobalVariables.global.resetFollower();
        yield return new WaitForSeconds(0.4f);
        this.gameObject.SetActive(false);
    }

    private IEnumerator moveIcon(RawImage icon, Vector2 destination)
    {
        Image item = icon.transform.Find("Item").GetComponent<Image>();
        float startX = icon.GetComponent<RectTransform>().rect.x;
        float startY = icon.GetComponent<RectTransform>().rect.y;
        float distanceX = destination.x - startX;
        float distanceY = destination.y - startY;
        float itemOffsetX = item.GetComponent<RectTransform>().rect.x - icon.GetComponent<RectTransform>().rect.x;
        float itemOffsetY = item.GetComponent<RectTransform>().rect.y - icon.GetComponent<RectTransform>().rect.y;

        float increment = 0;
        while (increment < 1)
        {
            increment += (1 / moveSpeed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            icon.GetComponent<RectTransform>().rect.Set(startX + (increment * distanceX), startY + (increment * distanceY),
                icon.GetComponent<RectTransform>().rect.width, icon.GetComponent<RectTransform>().rect.height);
            item.GetComponent<RectTransform>().rect.Set(startX + (increment * distanceX) + itemOffsetX,
                startY + (increment * distanceY) + itemOffsetY, item.GetComponent<RectTransform>().rect.width, item.GetComponent<RectTransform>().rect.height);
            yield return null;
        }
    }

    //图标的动画
    private IEnumerator animateIcons()
    {
        float interTime = 0.2f;
        while (pcViewModel.running)
        {
            StartCoroutine(CommonUtils.StartUVAnimation(partyIcons, interTime));
            yield return new WaitForSeconds(interTime * 2);
        }
        yield return null;
    }

    private IEnumerator animateCursor()
    {
        while (pcViewModel.running)
        {
            while (!pcViewModel.carrying)
            {
                cursorBack.uvRect = new Rect(0.5f, 0.5f, cursorBack.uvRect.width, cursorBack.uvRect.height);
                yield return new WaitForSeconds(0.4f);
                if (!pcViewModel.carrying)
                {
                    cursorBack.uvRect = new Rect(0, 0.5f, cursorBack.uvRect.width, cursorBack.uvRect.height);
                    yield return new WaitForSeconds(0.4f);
                }
            }
            yield return null;
        }
    }

    private IEnumerator animatePokemon()
    {
        yield return StartCoroutine(CommonUtils.animateImage(selectedSpriteAnimation, selectedSprite));
    }
}