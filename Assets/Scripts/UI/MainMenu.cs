using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   [SerializeField] private GameObject connectingPanel;
   [SerializeField] private GameObject menuPanel;
   [SerializeField] private TMP_InputField joinCodeInputField;

   private async void Start()
   {
      try
      {
         // initialize 
         await UnityServices.InitializeAsync();
         await AuthenticationService.Instance.SignInAnonymouslyAsync();
         Debug.Log($"Player Id: {AuthenticationService.Instance.PlayerId}");
      }
      catch (Exception e)
      {
        Debug.LogError(e);
         return;
      }
      connectingPanel.SetActive(false);
      menuPanel.SetActive(true);
   }

   public void StartHost()
   {
      HostManager.Instance.StartHost();
   }

   /*public void StartServer()
   {
      ServerManager.Instance.StartServer();
   }*/

   public async void StartClient()
   {
      await ClientManager.Instance.StartClient(joinCodeInputField.text);
   }
}
