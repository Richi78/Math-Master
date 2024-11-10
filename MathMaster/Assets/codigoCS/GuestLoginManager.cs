using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.Networking; // Para UnityWebRequest
using Photon.Pun;
using Photon.Realtime;
using System.Linq;


public class GuestLoginManager : MonoBehaviourPunCallbacks // Heredamos de MonoBehaviourPunCallbacks
{
    public Button botonInvitado;  // Boton para iniciar sesion como invitado
    public InputField inputNombre; //esta en la parte del panel donde se ve el nombre del usuario
    public Image imagenPerfil;//imagen de perfil
    public Button botonEditarNombre; 
    public Button botonEditarImagen;
    public Button botonCrearSala;   // Boton para crear la sala
    public GameObject MenuPrincipalPanel;
    public GameObject LoginPanel;
    public GameObject AjustesPanel;
    public GameObject ModoJuegoPanel;
    public GameObject BuscarJugadorPanel; // Panel donde se ve la sala
    public GameObject TemarioPanel;
    //public Button botonComenzar;
    //public GameObject prefabJugador; 
    public Text idSala;
    public Text jugadoresText;
    //public Button botonCrearSala; //boton para crear sala

    public GameObject togglePrefab;  // Prefab para el toggle
    public Transform toggleGroupParent;  // El contenedor donde se colocaran los toggles
    private TemasContainer temasContainer; // Esta variable deberia estar declarada para almacenar los temas


    private string playFabId;
    private const string PREFS_PLAYFAB_ID = "PlayFabID";
    private const string PROFILE_IMAGE_KEY = "ProfileImageURL";

    void Start(){
        botonInvitado.onClick.AddListener(IniciarSesionComoInvitado);  // Asigna accion al boton

        if (PlayerPrefs.HasKey(PREFS_PLAYFAB_ID))
        {
            playFabId = PlayerPrefs.GetString(PREFS_PLAYFAB_ID);
        }
        botonEditarNombre.onClick.AddListener(HabilitarEdicionNombre);//de entrada le digo que ya tiene una funcion pero creo q no funciona
        botonEditarImagen.onClick.AddListener(EditarImagen);

        botonCrearSala.onClick.AddListener(CrearSala);
        // Conectate al Master Server si aun no estas conectado
        if (!PhotonNetwork.IsConnected){
            PhotonNetwork.ConnectUsingSettings();
        }
        ActualizarConteoJugadores();
    }

    void IniciarSesionComoInvitado(){
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, iniciadoExito, iniciadoError);//(request/formulario)
    }//                                            ,exito        , fracaso

    void iniciadoExito(LoginResult result){//aqui le digo que va hacer si todo va bien
        playFabId = result.PlayFabId;
        string sessionTicket = result.SessionTicket; // obtener el session ticket
        PlayerPrefs.SetString(PREFS_PLAYFAB_ID, playFabId);
        PlayerPrefs.Save();
        Debug.Log("Inicio de sesion exitoso como invitado.");


        // Conectar a Photon despues de iniciar sesion en PlayFab
        PhotonNetwork.NickName = playFabId;  // Usar el ID de PlayFab como nombre de usuario en Photon
        PhotonNetwork.ConnectUsingSettings(); // Conectar a Photon



        CargarDatosUsuario();
        ObtenerTemasDesdePlayFab();  // Ahora obtenemos los temas -->estan en playFab
        MenuPrincipalMostrar();
    }

    void iniciadoError(PlayFabError error){//aqui le digo que va hacer si todo va mal
        Debug.LogError("Error al iniciar sesion: " + error.GenerateErrorReport());
    }
    
    public override void OnConnectedToMaster() {
        Debug.Log("Conectado a Photon.");
        // Aqui puedes proceder a crear una sala o mostrar el panel para unirse a una sala.
        PhotonNetwork.JoinLobby();
        // Si intentabamos crear una sala y nos reconectamos al Master Server, ahora si la creamos
        
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Unido al lobby, ahora se puede crear una sala.");
    }
/*
    void IntentarCrearSala()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby)
        {
            CrearSala();
        }
        else
        {
            Debug.LogWarning("El cliente no está en el Master Server, reconectando...");
            intentadoCrearSala = true; // Marca que queremos crear la sala una vez reconectado
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings(); // Reconectar al Master Server si no está conectado
            }
        }
    } 
    */
    // Este metodo se llama automaticamente cuando un jugador entra a la sala
    public override void OnPlayerEnteredRoom(Player newPlayer){
        ActualizarConteoJugadores();
    }

    // Este metodo se llama automaticamente cuando un jugador sale de la sala
    public override void OnPlayerLeftRoom(Player otherPlayer){
        ActualizarConteoJugadores();
    }

    void ActualizarConteoJugadores(){
        if (jugadoresText != null){
            int jugadoresEnSala = PhotonNetwork.CurrentRoom.PlayerCount;
            jugadoresText.text = "Jugadores en sala: " + jugadoresEnSala.ToString();
        }else{
            Debug.LogWarning("jugadoresText no está asignado en el Inspector.");
        }
    }
    // Metodo para generar un ID aleatorio de 6 caracteres (letras y numeros)
    string GenerarIDSala(){
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] id = new char[6];
        for (int i = 0; i < id.Length; i++)
        {
            id[i] = caracteres[UnityEngine.Random.Range(0, caracteres.Length)];
        }
        return new string(id);
    }

    void CrearSala(){
        // Configura las opciones de la sala
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;  // Numero maximo de jugadores en la sala
        roomOptions.IsVisible = true; // Haz la sala visible para otros jugadores
        roomOptions.IsOpen = true;    // Permite que otros jugadores se unan

        // Genera un ID unico para la sala
        string roomID = GenerarIDSala(); // Genera un ID de 6 caracteres
        idSala.text = roomID;
        // Crear la sala en Photon con el ID generado
        PhotonNetwork.CreateRoom(roomID, roomOptions);
        Debug.Log("Sala creada con ID: " + roomID);
    }

    // Este metodo se llama automaticamente cuando la sala se crea correctamente
    public override void OnCreatedRoom()
    {
        Debug.Log("Sala creada exitosamente: " + PhotonNetwork.CurrentRoom.Name);
        // Aqui puedes mostrar la interfaz de la sala, por ejemplo, `BuscarJugadorPanel`.
        BuscarJugadorPanelMostrar();
    }

    // Este metodo se llama automaticamente si ocurre un error al crear la sala
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("Error al crear la sala: " + message);
        // Aqui puedes mostrar un mensaje de error al usuario o intentar crear la sala de nuevo.
    }

    void ObtenerTemasDesdePlayFab()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OnTitleDataSuccess, OnTitleDataError);
    }

    void OnTitleDataSuccess(GetTitleDataResult result)
    {
        Debug.Log("Datos recibidos de Title Data: " + JsonUtility.ToJson(result.Data));

        if (result.Data.ContainsKey("TemasTrivia"))
        {
            Debug.Log("Temas cargados exitosamente.");
            string json = result.Data["TemasTrivia"];
            temasContainer = JsonUtility.FromJson<TemasContainer>(json);
            CrearToggles();  // Llama al metodo para crear los toggles
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

    void CrearToggles()
    {
        // Limpia los toggles existentes, si los hay
        foreach (Transform child in toggleGroupParent)
        {
            Destroy(child.gameObject);
        }

        // Verifica si hay temas disponibles
        if (temasContainer == null || temasContainer.temas.Count == 0)
        {
            Debug.LogWarning("No hay temas disponibles para mostrar.");
            return;
        }

        // Crea un toggle para cada tema
        foreach (var tema in temasContainer.temas)
        {
            // Instancia un nuevo toggle desde el prefab
            GameObject nuevoToggle = Instantiate(togglePrefab, toggleGroupParent);
            nuevoToggle.transform.localScale = Vector3.one; // Asegura que mantenga la escala correcta

            // Obten el componente Toggle y el componente Text del hijo
            Toggle toggleComponent = nuevoToggle.GetComponent<Toggle>();
            Text textoComponent = nuevoToggle.GetComponentInChildren<Text>();

            // Asigna el nombre del tema al texto del toggle
            if (textoComponent != null)
            {
                textoComponent.text = tema.nombre;
            }
            else
            {
                Debug.LogWarning("El componente Text no se encontro en el prefab.");
            }

            // Agrega un evento para detectar la seleccion del toggle
            toggleComponent.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    Debug.Log("Tema seleccionado: " + tema.nombre);
                    textoComponent.color = Color.white; // Cambia el color a blanco
                }
                else
                {
                    textoComponent.color = Color.black; // Cambia el color a negro cuando no esta seleccionado
                }
            });
        }

        // Forzar actualizacion del layout (por si no se reordena automaticamente)
        LayoutRebuilder.ForceRebuildLayoutImmediate(toggleGroupParent as RectTransform);
    }


    void CargarDatosUsuario(){
        ObtenerNombreUsuario();
        CargarAvatarImagen();
    }

    void ObtenerNombreUsuario(){
        var request = new GetAccountInfoRequest { PlayFabId = playFabId };//busco al usuario por el id
        PlayFabClientAPI.GetAccountInfo(request, getUserExito, getUserFracaso);
    }

    void getUserExito(GetAccountInfoResult result){//si todo va bien
        string nombre = !string.IsNullOrEmpty(result.AccountInfo.TitleInfo.DisplayName)//si tiene nombre(no esta vacio) cargo su nombre
                        ? result.AccountInfo.TitleInfo.DisplayName 
                        : result.AccountInfo.PlayFabId;  // Usa el PlayFabId si el display name esta vacio
        Debug.Log("Nombre de usuario: " + nombre);//result.AccountInfo.TitleInfo.DisplayName  --> aqui obtengo al player(jugador).usuario.Nombre en palabras mas simples obtengo su nombre si es que tiene
        inputNombre.text = nombre;  // aqui reemplazo el input nombre que esta en ajustes por el nombre que obtuvimos
    }

    void getUserFracaso(PlayFabError error){//si todo va mal
        Debug.LogError("Error al obtener la informacion del usuario: " + error.GenerateErrorReport());
    }

    public void HabilitarEdicionNombre(){
        inputNombre.interactable = true;//aqui lo vuelvo editable el input nombre que esta en ajustes
        inputNombre.Select();
        inputNombre.onEndEdit.AddListener(GuardarNombreEditado);  // Guarda automaticamente
    }

    void GuardarNombreEditado(string nuevoNombre){
        inputNombre.interactable = false;
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = nuevoNombre };// aqui le digo a play fab en el campo de nombre(displayname) que lo cambie por nuevoNombre , esta en playFab para que lo vean
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, cambioNombreExito, cambioNombreFallo);//aqui lo subo los cambios a playFab
    }

    void cambioNombreExito(UpdateUserTitleDisplayNameResult result){
        Debug.Log("Nombre actualizado: " + result.DisplayName);
    }

    void cambioNombreFallo(PlayFabError error){
        Debug.LogError("Error al actualizar el nombre: " + error.GenerateErrorReport());
    }

    public void EditarImagen(){//aqui es donde importo lo de galery que me proporciona unity 
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>{//si tiene permiso abre galeria
            if (path != null){
                Debug.Log("Path de la imagen seleccionada: " + path);
                StartCoroutine(CargarYSubirImagen(path));
            }
            else{
                Debug.LogWarning("No se selecciono ninguna imagen.");
            }
        }, "Selecciona una imagen de perfil", "image/*");
    }

    private IEnumerator CargarYSubirImagen(string path){   
        byte[] imageBytes = System.IO.File.ReadAllBytes(path);// Lee la imagen como bytes (lo vuelvo bytes 101010110...) usando io a la fuerza
        StartCoroutine(cargarAlServidorImgur(imageBytes));
        /*Texture2D texture = NativeGallery.LoadImageAtPath(path); //uso texture2d porque me ofrece mas opciones para editar la imagen
        if (texture != null){
            byte[] imageBytes = texture.EncodeToPNG(); // aqui puedo comvertir las imagenes a png , jpg ,etc
            StartCoroutine(cargarAlServidorImgur(imageBytes));
        }
        else{
            byte[] imageBytes = System.IO.File.ReadAllBytes(path);// Lee la imagen como bytes (lo vuelvo bytes 101010110...) usando io a la fuerza
            StartCoroutine(cargarAlServidorImgur(imageBytes));
        }//lo subo al servidor estando bien o mal 
         //aqui lo subo(StartCoroutine --> me sirve para no detener mi app sin importar nada)*/
        //CargarDatosUsuario();//si no lo usara digamos que el usuario sube una imagen muy pesada o hay mala conexion podria detenerce la app , se congelaria , te daria error ,etc
        yield return null;
    }

    private IEnumerator cargarAlServidorImgur(byte[] imageBytes){
        WWWForm form = new WWWForm();
        form.AddField("image", Convert.ToBase64String(imageBytes));
        using (UnityWebRequest www = UnityWebRequest.Post("https://api.imgur.com/3/image", form)){
            www.SetRequestHeader("Authorization", "Client-ID 306dd4e0d46cff1");  // este id no borrar ni cambiar es el id del proyecto que cree en imgur pa que guarde imagenes
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success){
                Debug.LogError("Error al subir la imagen a Imgur: " + www.error);
            }
            else{
                ImgurResponse jsonResponse = JsonUtility.FromJson<ImgurResponse>(www.downloadHandler.text);
                string imgurLink = jsonResponse.data.link;//aqui guardo el link donde esta guardado la imagen , si no funciona usar este string imgurLink = jsonResponse["data"]["link"];
                Debug.Log("Imagen subida exitosamente: " + imgurLink);
                // Guardar la URL en PlayFab
                GuardarImagenEnPlayFab(imgurLink);//
                CargarDatosUsuario();
            }
        }
    }

    private void GuardarImagenEnPlayFab(string imgurLink){
        // Crea una nueva instancia de UpdateAvatarUrlRequest
        var request = new UpdateAvatarUrlRequest//el campo de la tabla player se llama avatarUrl solo guardo el link
        {
            ImageUrl = imgurLink // Asegurate de que el campo se llame correctamente no cambiar ImageUrl es propio de playFab
        };
        PlayFabClientAPI.UpdateAvatarUrl(request, 
            result => Debug.Log("Avatar URL actualizado en PlayFab."),//si todo va bien
            error => Debug.LogError("Error al actualizar el Avatar URL: " + error.GenerateErrorReport()));//si todo va mal
    }

    void CargarAvatarImagen(){//este metodo se encarga de obtener la imagen y lo podemos visualizar en ajustes
        var request = new GetAccountInfoRequest { PlayFabId = playFabId };
        PlayFabClientAPI.GetAccountInfo(request, cargaImagenExito, cargaImagenFallo);
    }

    void cargaImagenExito(GetAccountInfoResult result){
        // Verificar si el usuario tiene un AvatarUrl valido
        if (!string.IsNullOrEmpty(result.AccountInfo.TitleInfo.AvatarUrl)){
            string avatarUrl = result.AccountInfo.TitleInfo.AvatarUrl;
            StartCoroutine(cargarImagenDesdeUrl(avatarUrl));  // Cargar solo si tiene imagen
        }
        else{
            Debug.Log("No hay avatar configurado, se mantiene la imagen predeterminada.");
        }
        //CargarDatosUsuario();  // Cargar otros datos del usuario
    }
    void cargaImagenFallo(PlayFabError error){
        Debug.LogError("Error al obtener los datos: " + error.GenerateErrorReport());
    }

    private IEnumerator cargarImagenDesdeUrl(string url){
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url)){
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success){
                Debug.LogError("Error al cargar la imagen: " + www.error);
            }
            else{
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                imagenPerfil.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
    // Clases auxiliares para el manejo del JSON de Imgur
    [System.Serializable]
    public class ImgurResponseData{
        public string link;
    }

    [System.Serializable]
    public class ImgurResponse{
        public ImgurResponseData data;
    }


    //
    [System.Serializable]
    public class Pregunta
    {
        public string pregunta;
        public List<string> opciones;
        public string respuesta_correcta;
    }

    [System.Serializable]
    public class Tema
    {
        public string nombre;
        public List<Pregunta> preguntas;
    }

    [System.Serializable]
    public class TemasContainer
    {
        public List<Tema> temas;
    }



    public void SalirDeSala(){
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("Saliendo de la sala actual.");
        }else{
            Debug.Log("No estas en ninguna sala");
        }
    }


    // manejo de paneles
    public void MenuPrincipalMostrar(){
        MenuPrincipalPanel.SetActive(true);
        LoginPanel.SetActive(false);
        AjustesPanel.SetActive(false);   
        ModoJuegoPanel.SetActive(false);
        BuscarJugadorPanel.SetActive(false);    
        TemarioPanel.SetActive(false);
        //SalirDeSala();
    }
    public void AjustesMostrar(){
        CargarDatosUsuario();
        MenuPrincipalPanel.SetActive(false);
        LoginPanel.SetActive(false);
        AjustesPanel.SetActive(true);   
        ModoJuegoPanel.SetActive(false);
        BuscarJugadorPanel.SetActive(false);    
        TemarioPanel.SetActive(false);
    }
    public void ModoJuegoPanelMostrar(){
        MenuPrincipalPanel.SetActive(false);
        LoginPanel.SetActive(false);
        AjustesPanel.SetActive(false);   
        ModoJuegoPanel.SetActive(true);
        BuscarJugadorPanel.SetActive(false);    
        TemarioPanel.SetActive(false);
        SalirDeSala();
    }
    public void LoginPanelMostrar(){
        MenuPrincipalPanel.SetActive(false);
        LoginPanel.SetActive(true);
        AjustesPanel.SetActive(false);   
        ModoJuegoPanel.SetActive(false);
        BuscarJugadorPanel.SetActive(false);    
        TemarioPanel.SetActive(false);
    }
    public void BuscarJugadorPanelMostrar(){
        MenuPrincipalPanel.SetActive(false);
        LoginPanel.SetActive(false);
        AjustesPanel.SetActive(false);   
        ModoJuegoPanel.SetActive(false);
        BuscarJugadorPanel.SetActive(true);    
        TemarioPanel.SetActive(false);
    }
    public void TemarioPanelMostrar(){
        ObtenerTemasDesdePlayFab(); 
        MenuPrincipalPanel.SetActive(false);
        LoginPanel.SetActive(false);
        AjustesPanel.SetActive(false);   
        ModoJuegoPanel.SetActive(false);
        BuscarJugadorPanel.SetActive(false);    
        TemarioPanel.SetActive(true);
    }
}