
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using static GuestLoginManager;

public class HomeController : MonoBehaviour
{
    public VisualElement ui;
    public Button crearSalaButton;
    public Button salasCreadasButton;
    public Button salaIDButton;
    public Button temarioButton;

    private UIController uIController;

    private string playFabId;
    public List<string> temas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {

        ui = GetComponent<UIDocument>().rootVisualElement;

        uIController = FindAnyObjectByType<UIController>();

        playFabId = PlayerPrefs.GetString("PlayFabID");

        crearSalaButton = ui.Q<Button>("crearSala");
        crearSalaButton.RegisterCallback<ClickEvent>(CrearSala);

        salasCreadasButton = ui.Q<Button>("SalasCreadas");

        salaIDButton = ui.Q<Button>("SalaID");

        temarioButton = ui.Q<Button>("temario");
        temarioButton.RegisterCallback<ClickEvent>(ShowTemario);
    }

    public void CrearSala(ClickEvent evt)
    {
        Debug.Log("jajaja");
    }

    public void ShowTemario(ClickEvent evt)
    {
        uIController.EnableTemario();
    }

    

}
