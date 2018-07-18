//this script shows the user the register and sign in panel and allows the user to toggle in between them. it also removes the panels when they are not needed
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RegisterClick : MonoBehaviour {
    public GameObject SignInPanel;
    public GameObject RegisterPanel;
    public void Togclicked()//User does not have an account and switches from signInPanel to register
    {
        SignInPanel.SetActive(false);
        RegisterPanel.SetActive(true);

    }
    public void OtherClicked()//user already has an account and wants to switch from register to sign in
    {
        SignInPanel.SetActive(true);
        RegisterPanel.SetActive(false);
    }
    public void removePanels()//user registered --> remove signin/register options
    {
        SignInPanel.SetActive(false);
        RegisterPanel.SetActive(false);
    }
}
