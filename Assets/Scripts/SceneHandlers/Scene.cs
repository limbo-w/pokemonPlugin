using UnityEngine;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Contexts;
using System.Globalization;
using Loxodon.Framework.Localizations;

public class Scene : MonoBehaviour
{
    public static Scene main;  
    public PokedexHandler Pokedex;
    public SummaryHandler Summary;
    public EvolutionHandler Evolution;
    public PartyHandler Party;
    public BagHandler Bag;
    public TradeHandler Trade;
    public TrainerHandler Trainer;
    public SettingsHandler Settings;
    public PauseHandler Pause;
    public TypingHandler Typing;
    public BattleHandler Battle;
    public CostumeHandler Costume;
    public PCHandler PC;
    public LoginHandler Login;
    public GameObject TCKCanvas;
    public DpadPlayerActions playerInput;

    void Awake()
    {
        if (main == null)
        {
            main = this;
            ApplicationContext context = Context.GetApplicationContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();
        }
        if (playerInput == null)
        {
            playerInput = new DpadPlayerActions();
        }
        SetMapButtonVisible(true);

        CultureInfo cultureInfo = Locale.GetCultureInfo();
        var localization = Localization.Current;
        localization.CultureInfo = cultureInfo;
        localization.AddDataProvider(new DefaultDataProvider("LocalizationTutorials", new XmlDocumentParser()));

        Pokedex.gameObject.SetActive(true);
        Bag.gameObject.SetActive(true);
        Party.gameObject.SetActive(true);
        Trade.gameObject.SetActive(true);
        Trainer.gameObject.SetActive(true);
        Settings.gameObject.SetActive(true);
        Pause.gameObject.SetActive(true);
        Summary.gameObject.SetActive(true);
        Evolution.gameObject.SetActive(true);
        Typing.gameObject.SetActive(true);
        Battle.gameObject.SetActive(true);
        Costume.gameObject.SetActive(true);
        PC.gameObject.SetActive(true);
        Login.gameObject.SetActive(true);
    }

    //设置战斗界面是否出现
    public void SetBattleVisible(bool visible)
    {
        main.Battle.gameObject.SetActive(visible);
    }

    //大地图按键
    public void SetMapButtonVisible(bool visible)
    {
        if (visible)
        {
            main.playerInput.Enable();
        }
        else
        {
            main.playerInput.Disable();
        }
        main.TCKCanvas.SetActive(visible);
    }
}