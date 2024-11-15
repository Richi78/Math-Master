using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject Login;
    public GameObject Home;
    public GameObject Temario;

    public void Awake()
    {
        EnableLogin();
    }
    public void EnableLogin()
    {
        Login.SetActive(true);
        Home.SetActive(false);
        Temario.SetActive(false);
    }

    public void EnableHome()
    {
        Login.SetActive(false);
        Home.SetActive(true);
        Temario.SetActive(false);
    }
    public void EnableTemario()
    {
        Login.SetActive(false);
        Home.SetActive(false);
        Temario.SetActive(true);
    }

}
