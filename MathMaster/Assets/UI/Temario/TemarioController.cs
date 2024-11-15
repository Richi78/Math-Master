using UnityEngine;
using UnityEngine.UIElements;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using Newtonsoft.Json;

public class TemarioController : MonoBehaviour
{
    public VisualElement ui;
    private UIController uIController;
    public Button backButton;
    public VisualElement container;
    private string playFabId;
    public List<string> temas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        uIController = FindAnyObjectByType<UIController>();

        backButton = ui.Q<Button>("backButton");
        backButton.RegisterCallback<ClickEvent>(handleBackButton);

        container = ui.Q<VisualElement>("Temas");

        playFabId = PlayerPrefs.GetString("PlayFabID");
        ObtenerTemasDesdePlayFab();
    }
    void ObtenerTemasDesdePlayFab()
    {
        Debug.Log("clickeaste Temario");
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OnTitleDataSuccess, OnTitleDataError);
    }

    void OnTitleDataSuccess(GetTitleDataResult result)
    {
        Debug.Log("Datos recibidos de Title Data: " + JsonUtility.ToJson(result.Data));

        if (result.Data.ContainsKey("TemasTrivia"))
        {
            Debug.Log("Temas cargados exitosamente.");
            string json = result.Data["TemasTrivia"];

            TemaList temaList = JsonConvert.DeserializeObject<TemaList>(json);
            foreach(var tema in temaList.temas)
            {
                Debug.Log(tema.nombre);
                var label = new Label(tema.nombre);
                label.AddToClassList("label-temas");
                container.Add(label);
            }
        }
        else
        {
            Debug.LogWarning("No se encontro la clave 'TemasTrivia' en los datos.");
        }
    }

    void OnTitleDataError(PlayFabError error)
    {
        Debug.LogError("Error al obtener los temas: " + error.GenerateErrorReport());
    }

    public void handleBackButton(ClickEvent evt)
    {
        uIController.EnableHome();   
    }
}

public class Opcion
{
    public string opcion;
}
public class Pregunta
{
    public string pregunta { get; set; }
    //public List<Opcion> opciones { get; set; }
    public List<string> opciones { get; set; }
    public string respuesta { get; set; }
}
public class Tema
{
    public string nombre { get; set; }
    public List<Pregunta> preguntas { get; set; }
}
public class TemaList
{
    public List<Tema> temas { get; set; }
}
