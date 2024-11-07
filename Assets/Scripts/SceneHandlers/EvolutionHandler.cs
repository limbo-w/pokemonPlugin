using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Loxodon.Framework.Views;
using Loxodon.Framework.Views.Variables;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Binding.Builder;
using System.Collections.Generic;

public class EvolutionHandler : UIView
{
    DialogBoxUIHandler dialog = DialogBoxUIHandler.main;
    private GUIParticleHandler particles;
    private List<Texture> pokemonSpriteAnimation;
    private List<Texture> evolutionSpriteAnimation;
    private RawImage pokemonSprite;
    private RawImage evolutionSprite;

    private Image topBorder;
    private Image bottomBorder;
    private Image glow;

    private Pokemon selectedPokemon;

    public AudioClip
        evolutionBGM,
        evolvingClip,
        evolvedClip,
        forgetMoveClip;

    public Sprite smokeParticle;

    private bool stopAnimations = false;
    private bool evolving = true;
    private bool evolved = false;

    private Coroutine c_animateEvolution;
    private Coroutine c_pokemonGlow;
    private Coroutine c_glowGrow;
    private Coroutine c_pokemonPulsate;
    private Coroutine c_glowPulsate;


    protected override void Start()
    {
        particles = transform.GetComponent<GUIParticleHandler>();
        pokemonSprite = transform.Find("PokemonSprite").GetComponent<RawImage>();
        evolutionSprite = transform.Find("EvolutionSprite").GetComponent<RawImage>();
        topBorder = transform.Find("TopBorder").GetComponent<Image>();
        bottomBorder = transform.Find("BottomBorder").GetComponent<Image>();
        glow = transform.Find("Glow").GetComponent<Image>();
        gameObject.SetActive(false);
    }

    //精灵进化动画
    private IEnumerator animatePokemon()
    {
        int pokemonFrame = 0;
        int evolutionFrame = 0;
        while (true)
        {
            if (pokemonSpriteAnimation.Count > 0)
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
            if (evolutionSpriteAnimation.Count > 0)
            {
                if (evolutionFrame < evolutionSpriteAnimation.Count - 1)
                {
                    evolutionFrame += 1;
                }
                else
                {
                    evolutionFrame = 0;
                }
                evolutionSprite.texture = evolutionSpriteAnimation[evolutionFrame];
            }
            yield return new WaitForSeconds(0.08f);
        }
    }


    public IEnumerator control(Pokemon pokemonToEvolve, string methodOfEvolution)
    {
        dialog = DialogBoxUIHandler.main;
        selectedPokemon = pokemonToEvolve;
        int evolutionID = selectedPokemon.getEvolutionID(methodOfEvolution);
        if (evolutionID > 0)
        {
            string selectedPokemonName = selectedPokemon.getName();
            pokemonSpriteAnimation = selectedPokemon.GetFrontAnimFromGif();
            evolutionSpriteAnimation = Pokemon.GetAnimFromGif("PokemonSprites", evolutionID, selectedPokemon.getGender(), false);
            pokemonSprite.texture = pokemonSpriteAnimation[0];
            evolutionSprite.texture = evolutionSpriteAnimation[0];
            yield return StartCoroutine(animatePokemon());

            pokemonSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(128, 128);
            evolutionSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);

            pokemonSprite.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            evolutionSprite.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            topBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(342, 0);
            bottomBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(342, 0);

            glow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);

            stopAnimations = false;
            yield return StartCoroutine(ScreenFade.main.Fade(true, 1f));
            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(CommonUtils.showDialogText("什么?"));
            yield return StartCoroutine(CommonUtils.showDialogText(selectedPokemon.getName() + "正在进化!"));
            evolving = true;

            AudioClip cry = selectedPokemon.GetCry();
            SfxHandler.Play(cry);

            BgmHandler.main.PlayOverlay(evolutionBGM, 753100);
            yield return new WaitForSeconds(0.5f);

            c_animateEvolution = StartCoroutine(animateEvolution());
            SfxHandler.Play(evolvingClip);
            yield return new WaitForSeconds(0.5f);

            while (evolving)
            {
                bool AButtonPressed = Scene.main.playerInput.Player.AButton.IsPressed();
                bool BButtonPressed = Scene.main.playerInput.Player.BButton.IsPressed();
                if (BButtonPressed)
                {
                    evolving = false;
                    yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));

                    stopAnimateEvolution();
                    yield return StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
                    yield return StartCoroutine(CommonUtils.showDialogText("哈?"));
                    yield return StartCoroutine(CommonUtils.showDialogText(selectedPokemon.getName() + "停止进化."));
                }

                yield return null;
            }

            if (evolved)
            {
                selectedPokemon.evolve(methodOfEvolution);
                yield return new WaitForSeconds(3f);

                cry = selectedPokemon.GetCry();
                BgmHandler.main.PlayMFX(cry);
                yield return new WaitForSeconds(cry.length);
                AudioClip evoMFX = Resources.Load<AudioClip>("Audio/mfx/GetGreat");
                BgmHandler.main.PlayMFXConsecutive(evoMFX);

                yield return StartCoroutine(CommonUtils.showDialogText("恭喜!"));
                yield return StartCoroutine(CommonUtils.showDialogText("你的" + selectedPokemonName + "进化成" +
                                          PokemonDatabase.getPokemon(evolutionID).getName() + "!"));

                float extraTime = (evoMFX.length - 0.8f > 0) ? evoMFX.length - 0.8f : 0;
                yield return new WaitForSeconds(extraTime);

                bool AButtonPressed = Scene.main.playerInput.Player.AButton.IsPressed();
                bool BButtonPressed = Scene.main.playerInput.Player.BButton.IsPressed();

                while (!AButtonPressed && !BButtonPressed)
                {
                    yield return null;
                }

                long newMove = selectedPokemon.MoveLearnedAtLevel(selectedPokemon.getLevel());
                if (newMove > 0 && !selectedPokemon.HasMove(newMove))
                {
                    yield return StartCoroutine(LearnMove(selectedPokemon, newMove));
                }

                bool running = true;
                while (running)
                {
                    if (AButtonPressed || BButtonPressed)
                    {
                        running = false;
                    }

                    yield return null;
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        yield return StartCoroutine(ScreenFade.main.Fade(false, 1f));
        BgmHandler.main.ResumeMain(1.5f);
        yield return new WaitForSeconds(1.5f);
        this.gameObject.SetActive(false);
    }


    private void stopAnimateEvolution()
    {
        stopAnimations = true;
        StopCoroutine(c_animateEvolution);
        if (c_glowGrow != null)
        {
            StopCoroutine(c_glowGrow);
        }
        if (c_glowPulsate != null)
        {
            StopCoroutine(c_glowPulsate);
        }
        if (c_pokemonGlow != null)
        {
            StopCoroutine(c_pokemonGlow);
        }
        if (c_pokemonPulsate != null)
        {
            StopCoroutine(c_pokemonPulsate);
        }
        particles.cancelAllParticles();
        glow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        bottomBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(342, 0);
        topBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(342, 0);

        pokemonSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(128, 128);
        evolutionSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        pokemonSprite.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        BgmHandler.main.PlayOverlay(null, 0, 0.5f);
    }

    private IEnumerator animateEvolution()
    {
        StartCoroutine(smokeSpiral());
        StartCoroutine(borderDescend());
        yield return new WaitForSeconds(0.5f);
        c_pokemonGlow = StartCoroutine(pokemonGlow());
        yield return new WaitForSeconds(0.5f);
        c_glowGrow = StartCoroutine(glowGrow());
        yield return new WaitForSeconds(0.5f);
        evolutionSprite.color = new Color(1, 1, 1, 0.5f);
        c_pokemonPulsate = StartCoroutine(pokemonPulsate(19));
        yield return new WaitForSeconds(0.5f);
        yield return c_glowPulsate = StartCoroutine(glowPulsate(7));
        evolved = true;
        evolving = false;
        SfxHandler.Play(evolvedClip);
        StartCoroutine(glowDissipate());
        StartCoroutine(brightnessExplosion());
        StartCoroutine(borderRetract());
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(evolutionUnglow());
        yield return new WaitForSeconds(0.5f);
    }


    private IEnumerator brightnessExplosion()
    {
        float speed = ScreenFade.slowedSpeed;
        StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.slowedSpeed, Color.white));

        float increment = 0f;
        while (increment < 1)
        {
            increment += (1f / speed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            yield return null;
        }
        StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.slowedSpeed, Color.white));
    }

    private IEnumerator glowGrow()
    {
        float increment = 0f;
        float speed = 0.8f;
        float endSize = 96f;

        glow.color = new Color(0.8f, 0.8f, 0.5f, 0.2f);
        while (increment < 1)
        {
            increment += (1 / speed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            glow.GetComponent<RectTransform>().sizeDelta = new Vector2(endSize * increment, endSize * increment);
            yield return null;
        }
    }

    private IEnumerator glowDissipate()
    {
        float increment = 0f;
        float speed = 1.5f;
        float startSize = glow.GetComponent<RectTransform>().sizeDelta.x;
        float sizeDifference = 280f - startSize;
        while (increment < 1)
        {
            increment += (1 / speed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            glow.GetComponent<RectTransform>().sizeDelta = new Vector2(startSize + sizeDifference * increment,
                startSize + sizeDifference * increment);
            glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, 0.2f - (0.2f * increment));
            yield return null;
        }
    }

    private IEnumerator glowPulsate(int repetitions)
    {
        float increment = 0f;
        float speed = 0.9f;
        float maxSize = 160f;
        float minSize = 96f;
        float sizeDifference = maxSize - minSize;
        bool glowShrunk = true;

        for (int i = 0; i < repetitions; i++)
        {
            increment = 0f;
            while (increment < 1)
            {
                increment += (1 / speed) * Time.deltaTime;
                if (increment > 1)
                {
                    increment = 1;
                }
                if (glowShrunk)
                {
                    glow.GetComponent<RectTransform>().sizeDelta = new Vector2(minSize + sizeDifference * increment,
                        minSize + sizeDifference * increment);
                }
                else
                {
                    glow.GetComponent<RectTransform>().sizeDelta = new Vector2(maxSize - sizeDifference * increment,
                        maxSize - sizeDifference * increment);
                }
                yield return null;
            }
            if (glowShrunk)
            {
                glowShrunk = false;
            }
            else
            {
                glowShrunk = true;
            }
        }
    }

    private IEnumerator pokemonPulsate(int repetitions)
    {
        float increment = 0f;
        float baseSpeed = 1.2f;
        float speed = baseSpeed;
        Vector3 originalPosition = pokemonSprite.transform.localPosition;
        Vector3 centerPosition = new Vector3(0.5f, 0.47f, originalPosition.z);
        float distance = centerPosition.y - originalPosition.y;
        bool originalPokemonShrunk = false;
        for (int i = 0; i < repetitions; i++)
        {
            increment = 0f;
            speed *= 0.85f;
            while (increment < 1)
            {
                if (speed < 0.15f)
                {
                    speed = 0.15f;
                }
                increment += (1 / speed) * Time.deltaTime;
                if (increment > 1)
                {
                    increment = 1;
                }
                if (originalPokemonShrunk)
                {
                    pokemonSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(128f * increment, 128f * increment);
                    pokemonSprite.transform.localPosition = new Vector3(originalPosition.x,
                        centerPosition.y - (distance * increment), originalPosition.z);
                    evolutionSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(128f - 128f * increment,
                        128f - 128f * increment);
                    evolutionSprite.transform.localPosition = new Vector3(originalPosition.x,
                        originalPosition.y + (distance * increment), originalPosition.z);
                }
                else
                {
                    pokemonSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(128f - 128f * increment, 128f - 128f * increment);
                    pokemonSprite.transform.localPosition = new Vector3(originalPosition.x,
                        originalPosition.y + (distance * increment), originalPosition.z);
                    evolutionSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(128f * increment, 128f * increment);
                    evolutionSprite.transform.localPosition = new Vector3(originalPosition.x,
                        centerPosition.y - (distance * increment), originalPosition.z);
                }
                yield return null;
            }
            if (originalPokemonShrunk)
            {
                originalPokemonShrunk = false;
            }
            else
            {
                originalPokemonShrunk = true;
            }
        }
        pokemonSprite.transform.localPosition = originalPosition;
        evolutionSprite.transform.localPosition = originalPosition;
    }

    private IEnumerator pokemonGlow()
    {
        float increment = 0f;
        float speed = 1.8f;
        while (increment < 1)
        {
            increment += (1 / speed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            pokemonSprite.color = new Color(0.5f + (0.5f * increment), 0.5f + (0.5f * increment),
                0.5f + (0.5f * increment), 0.5f);
            yield return null;
        }
    }

    private IEnumerator evolutionUnglow()
    {
        float increment = 0f;
        float speed = 1.8f;
        while (increment < 1)
        {
            increment += (1 / speed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            evolutionSprite.color = new Color(1f - (0.5f * increment), 1f - (0.5f * increment), 1f - (0.5f * increment),
                0.5f);
            yield return null;
        }
    }

    private IEnumerator borderDescend()
    {
        float increment = 0f;
        float speed = 0.4f;
        while (increment < 1)
        {
            increment += (1 / speed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            topBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(342, 40 * increment);
            bottomBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(342, 64 * increment);
            yield return null;
        }
    }

    private IEnumerator borderRetract()
    {
        float increment = 0f;
        float speed = 0.4f;
        while (increment < 1)
        {
            increment += (1 / speed) * Time.deltaTime;
            if (increment > 1)
            {
                increment = 1;
            }
            topBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(342, 40 - 40 * increment);
            bottomBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(342, 64 - 64 * increment);
            yield return null;
        }
    }

    private IEnumerator smokeSpiral()
    {
        StartCoroutine(smokeTrail(-1, 0.6f, 0.56f, 16f));
        yield return new WaitForSeconds(0.08f);
        StartCoroutine(smokeTrail(1, 0.36f, 0.44f, 18f));
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(smokeTrail(-1, 0.6f, 0.36f, 16f));
        yield return new WaitForSeconds(0.26f);
        StartCoroutine(smokeTrail(1, 0.36f, 0.54f, 16f));
        yield return new WaitForSeconds(0.08f);
        StartCoroutine(smokeTrail(-1, 0.6f, 0.42f, 18f));
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(smokeTrail(1, 0.36f, 0.34f, 16f));
        yield return new WaitForSeconds(0.26f);
        StartCoroutine(smokeTrail(-1, 0.6f, 0.52f, 16f));
        yield return new WaitForSeconds(0.08f);
        StartCoroutine(smokeTrail(1, 0.36f, 0.4f, 18f));
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(smokeTrail(-1, 0.6f, 0.32f, 16f));
        yield return new WaitForSeconds(0.26f);
        StartCoroutine(smokeTrail(-1, 0.6f, 0.5f, 16f));
        yield return new WaitForSeconds(0.08f);
        StartCoroutine(smokeTrail(1, 0.36f, 0.38f, 18f));
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(smokeTrail(-1, 0.6f, 0.3f, 16f));
        yield return new WaitForSeconds(0.26f);
    }

    private IEnumerator smokeTrail(int direction, float positionX, float positionY, float maxSize)
    {
        float positionYmodified;
        float sizeModified;
        for (int i = 0; i < 8; i++)
        {
            positionYmodified = positionY + Random.Range(-0.03f, 0.03f);
            sizeModified = (((float)i / 7f) * maxSize + maxSize) / 2f;
            if (!stopAnimations)
            {
                Image particle = particles.createParticle(smokeParticle, ScaleToScreen(positionX, positionYmodified),
                    sizeModified, 0, 0.6f,
                    ScaleToScreen(positionX + Random.Range(0.01f, 0.04f), positionYmodified - 0.02f), 0,
                    sizeModified * 0.33f);
                if (particle != null)
                {
                    particle.color = new Color((float)i / 7f * 0.7f, (float)i / 7f * 0.7f, (float)i / 7f * 0.7f,
                        0.3f + ((float)i / 7f * 0.3f));
                }
            }
            if (direction > 0)
            {
                positionX += 0.03f;
            }
            else
            {
                positionX -= 0.03f;
            }
            positionY -= 0.0025f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private Vector2 ScaleToScreen(float x, float y)
    {
        Vector2 vector = new Vector2(x * 342f - 171f, (1 - y) * 192f - 96f);
        return vector;
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
                    yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(selectedPokemon.getName()
                        + "想要学习技能" + moveName + ".", 0.75f));
                    yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(selectedPokemon.getName()
                        + "但是, " + selectedPokemon.getName() + "已经学会了四个技能.", 0.75f));
                    yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("是否删除一个技能替换为" + moveName + "?", 0.75f));
                    string[] choices = { "是", "否", };
                    yield return StartCoroutine(CommonUtils.showChoiceText(choices));
                    chosenIndex = dialog.buttonIndex;
                    if (chosenIndex == 1)
                    {
                        yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
                        Scene.main.Summary.gameObject.SetActive(true);
                        StartCoroutine(Scene.main.Summary.control(selectedPokemon, move));
                        while (Scene.main.Summary.gameObject.activeSelf)
                        {
                            yield return null;
                        }

                        long replacedMove = Scene.main.Summary.getReplacedMove();
                        yield return StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));

                        if (replacedMove > 0)
                        {
                            yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("1, ", 0.75f));
                            yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("2, ", 0.75f));
                            yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("和... ", 0.75f));
                            yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("... ", 0.75f));
                            yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("... ", 0.75f));
                            SfxHandler.Play(forgetMoveClip);
                            yield return StartCoroutine(CommonUtils.showDialogText("呼地!"));
                            yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd(selectedPokemon.getName()
                                + "忘记使用" + MoveDatabase.getMoveName(replacedMove) + ".",0.75f));
                            yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("并且...",0.75f));

                            AudioClip mfx = Resources.Load<AudioClip>("Audio/mfx/GetAverage");
                            BgmHandler.main.PlayMFX(mfx);
                            yield return StartCoroutine(CommonUtils.showDialogText(selectedPokemon.getName() + "学习了技能" + moveName + "!"));
                            learning = false;
                        }
                        else
                        {
                            chosenIndex = 0;
                        }
                    }
                    if (chosenIndex == 0)
                    {
                        yield return StartCoroutine(CommonUtils.showDialogTextDelayNoEnd("放弃学习技能" + moveName + "?",0.75f));
                        yield return StartCoroutine(CommonUtils.showChoiceText(choices));
                        chosenIndex = dialog.buttonIndex;
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
                    yield return StartCoroutine(CommonUtils.showDialogText(selectedPokemon.getName() + "学习了技能" + moveName + "!"));
                    learning = false;
                }
            }
        }
        if (chosenIndex == 0)
        {
            yield return StartCoroutine(CommonUtils.showDialogText(selectedPokemon.getName() + "没有学习技能" + moveName + "."));
        }
    }
}