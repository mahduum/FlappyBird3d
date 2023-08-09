using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Behaviors
{
    public class LoadingManager : MonoBehaviour
    {
        [SerializeField] private Camera _splashCamera;
        private static LoadingManager _instance;
        
        public static LoadingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LoadingManager>();
                    
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("_LoadingManager");
                        _instance = obj.AddComponent<LoadingManager>();
                    }

                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
        }

        private async void Start()
        {
            //load all external assets
            //...
            //load main menu scene:
            Debug.Log("Started delay.");
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            Debug.Log("Ended delay.");
            await LoadMainMenuAsSingleScene();
        }

        public static async UniTask LoadMainMenuAsSingleScene()
        { 
            await SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        }
    }
}
