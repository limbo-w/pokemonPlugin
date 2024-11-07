using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    protected DpadPlayerActions playerInput;
    void Awake()
    {
        playerInput = new DpadPlayerActions();
    }

    void Update()
    {
        bool isPressed = playerInput.Player.AButton.IsPressed();
        if (isPressed)
        {
            TrainerManager.Instance.setTaskValue("getPokedex", 1);
            GameObject gameObject = Scene.main.Battle.gameObject;
            TrainerManager.Instance.setTaskValue("getPokedex", 1);
            GameObject gameObject = Scene.main.Pause.gameObject;
            //GameObject gameObject = Scene.main.Trainer.gameObject;
            //GameObject gameObject = Scene.main.Evolution.gameObject;
            GameObject gameObject = Scene.main.PC.gameObject;
            gameObject.SetActive(true);
            StartCoroutine(runSceneUntilDeactivated(gameObject));
        }
    }

    //启动PokedexHandler图鉴面板
    private IEnumerator runSceneUntilDeactivated(GameObject sceneInterface)
    {
        //TradeHandler
        //StartCoroutine(Scene.main.Trade.control());
        //yield return null;

        //BagHandler
        //StartCoroutine(Scene.main.Bag.control());
        //yield return null;

        //LoginHandler
        //StartCoroutine(Scene.main.Login.control());

        //TrainHandler
        //Scene.SetMapButtonVisible(false);
        //StartCoroutine(Scene.main.Trainer.control());
        //yield return null;
        //StartCoroutine(Scene.main.Trainer.control());
        //StartCoroutine(Scene.main.Trade.control());

        //PokedexHandler
        //Scene.SetMapButtonVisible(false);
        //StartCoroutine(Scene.main.Pokedex.control());

        //PauseHandler
        //StartCoroutine(Scene.main.Pause.control());

        //PCHandler
        //Scene.SetMapButtonVisible(false);
        //StartCoroutine(Scene.main.PC.control());
        //yield return null;

        StartCoroutine(Scene.main.PC.control());
        //SummaryHandler
        //Scene.SetMapButtonVisible(false);
        //Pokemon pokemon1 = new Pokemon(7, 130, "", Pokemon.Gender.MALE, 10, 0, null, null, 20, 10, 10, 10, 10, 10,
        //    0, 0, 0, 0, 0, 0, "", new long[] { 1, 2, 3, 4 });
        //Pokemon pokemon2 = new Pokemon(7, 150, "", Pokemon.Gender.CALCULATE, 8, 0, null, null, 20, 10, 10, 10, 10, 10,
        //0, 0, 0, 0, 0, 0, "", new long[] { 1, 2, 3, 4 });
        //Pokemon[] pokemons = new Pokemon[] { pokemon1, pokemon2 };
        //StartCoroutine(Scene.main.Summary.control(pokemons, 0));
        //StartCoroutine(Scene.main.Bag.control());
        //StartCoroutine(Scene.main.Party.control());

        //EvolutionHandler
        //Scene.SetMapButtonVisible(false);
        //Pokemon pokemon = new Pokemon(7, 129, "", Pokemon.Gender.MALE, 10, 0, null, null, 20, 10, 10, 10, 10, 10,
        //    0, 0, 0, 0, 0, 0, "", new long[] { 1, 2, 3, 4 });
        //StartCoroutine(Scene.main.Evolution.control(pokemon, "Level"));
        //Scene.SetMapButtonVisible(false);
        //Pokemon pokemon = new Pokemon(7, 129, "", Pokemon.Gender.MALE, 10, 0, null, null, 20, 10, 10, 10, 10, 10,
        //    0, 0, 0, 0, 0, 0, "", new long[] { 1, 2, 3, 4 });
        //StartCoroutine(Scene.main.Evolution.control(pokemon, "Level"));
        //Scene.SetMapButtonVisible(false);
        //Pokemon pokemon = new Pokemon(7, 129, "", Pokemon.Gender.MALE, 10, 0, null, null, 20, 10, 10, 10, 10, 10,
        //    0, 0, 0, 0, 0, 0, "", new long[] { 1, 2, 3, 4 });
        //StartCoroutine(Scene.main.Evolution.control(pokemon, "Level"));

        //BattleHandler
        Scene.main.SetMapButtonVisible(false);
        Pokemon pokemon1 = new Pokemon(7, 130, "", Pokemon.Gender.MALE, 3, 0, null, null, 20, 10, 10, 10, 10, 10,
            0, 0, 0, 0, 0, 0, "", new List<SkillInfo>());
        Pokemon pokemon2 = new Pokemon(7, 150, "", Pokemon.Gender.CALCULATE, 31, 0, null, null, 20, 10, 10, 10, 10, 10,
        0, 0, 0, 0, 0, 0, "", new List<SkillInfo>());
        //Pokemon[] pokemons = new Pokemon[] { pokemon1, pokemon2 };
        StartCoroutine(Scene.main.Battle.control(pokemon2));
        //StartCoroutine(Scene.main.Battle.control(new Trainer(pokemons)));
        while (Scene.main.Battle.gameObject.activeSelf)
        {
            yield return null;
        }
        Scene.main.SetMapButtonVisible(true);
        yield return StartCoroutine(ScreenFade.main.Fade(true, 0.4f));
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }
}
