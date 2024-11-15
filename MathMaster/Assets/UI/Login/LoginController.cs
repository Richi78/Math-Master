using Photon.Pun;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;

public class LoginController : MonoBehaviour
{
    public VisualElement ui;
    public Button loginButton;

    private UIController uiController;

    private string playFabId;
    private const string PREFS_PLAYFAB_ID = "PlayFabID";

   
    private void Awake()
    {
        if (PlayerPrefs.HasKey(PREFS_PLAYFAB_ID))
        {
            playFabId = PlayerPrefs.GetString(PREFS_PLAYFAB_ID);
        }
        ui = GetComponent<UIDocument>().rootVisualElement;
        
        //uiController.EnableLogin();

    }
    private void OnEnable()
    {
        uiController = FindAnyObjectByType<UIController>();

        loginButton = ui.Q<Button>("loginButton");
        loginButton.RegisterCallback<ClickEvent>(IniciarSesionComoInvitado);
    }


    void IniciarSesionComoInvitado(ClickEvent evt)
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, iniciadoExito, iniciadoError);//(request/formulario)
    }//                                            ,exito        , fracaso

    void iniciadoExito(LoginResult result)
    {//aqui le digo que va hacer si todo va bien
        uiController.EnableHome();
        playFabId = result.PlayFabId;
        string sessionTicket = result.SessionTicket; // obtener el session ticket
        PlayerPrefs.SetString(PREFS_PLAYFAB_ID, playFabId);
        PlayerPrefs.Save();
        Debug.Log("Inicio de sesion exitoso como invitado.");

        // Conectar a Photon despues de iniciar sesion en PlayFab
        PhotonNetwork.NickName = playFabId;  // Usar el ID de PlayFab como nombre de usuario en Photon
        PhotonNetwork.ConnectUsingSettings(); // Conectar a Photon

    }

    void iniciadoError(PlayFabError error)
    {//aqui le digo que va hacer si todo va mal
        Debug.LogError("Error al iniciar sesion: " + error.GenerateErrorReport());
    }
    private void OnDisable()
    {
        loginButton.UnregisterCallback<ClickEvent>(IniciarSesionComoInvitado);
    }
}
