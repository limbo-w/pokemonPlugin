using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Loxodon.Framework.Views;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Binding.Builder;
using Loxodon.Framework.Localizations;
using System.Globalization;
using Loxodon.Framework.Views.Variables;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Observables;
using System.Collections.Specialized;
using UnityEditor;

public class TrainerHandler : UIView
{
    private AudioSource TrainerAudio;
    public AudioClip selectClip;

    public VariableArray variables;
    private TrainerViewModel viewModel = new TrainerViewModel();
    private RawImage[] showBadgeImages = new RawImage[8];

    protected override void Awake()
    {
        Transform badge = transform.Find("Badge");
        for (int i = 0; i < 8; i++)
        {
            showBadgeImages[i] = badge.Find("Badge" + i + "/Icon").GetComponent<RawImage>();
        }
        TrainerAudio = transform.GetComponent<AudioSource>();  
    }

    protected override void Start()
    {
        IBindingContext bindingContext = this.BindingContext();
        bindingContext.DataContext = viewModel;
        BindingSet<TrainerHandler, TrainerViewModel> bindingSet = this.CreateBindingSet<TrainerHandler, TrainerViewModel>();
        bindingSet.Bind(this.variables.Get<Text>("iDnoText")).For(v => v.text).To(vm => vm.PlayerID).TwoWay();
        bindingSet.Bind(this.variables.Get<Text>("nameText")).For(v => v.text).To(vm => vm.PlayerName).TwoWay();
        bindingSet.Bind(this.variables.Get<Text>("moneyText")).For(v => v.text).To(vm => vm.PlayerMoney).TwoWay();
        bindingSet.Bind(this.variables.Get<Text>("scoreText")).For(v => v.text).To(vm => vm.Score).TwoWay();
        bindingSet.Bind(this.variables.Get<Text>("pokedexText")).For(v => v.text).To(vm => vm.Pokedex).TwoWay();
        bindingSet.Bind(this.variables.Get<Text>("adventureText")).For(v => v.text).To(vm => vm.Adventure).TwoWay();
        bindingSet.Bind(this.variables.Get<RawImage>("personPicture")).For(v => v.texture).To(vm => vm.PersonPicture).TwoWay();
        bindingSet.Bind(this.variables.Get<Button>("return")).For(v => v.onClick).To(vm => vm.OnReturn);

        bindingSet.Build();
        this.gameObject.SetActive(false);
    }

    public IEnumerator control()
    {
        Scene.main.SetMapButtonVisible(false);
        this.variables.Get<RawImage>("personPicture").texture = Resources.Load<Texture>("null");
        yield return StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.defaultSpeed));
        viewModel.Running = true;
        viewModel.updateData();
        while (viewModel.Running)
        {
            yield return new WaitForSeconds(0.2f);
        }
        yield return StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.defaultSpeed));
        this.gameObject.SetActive(false);
    }
}