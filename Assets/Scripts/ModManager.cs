using System;
using System.Threading.Tasks;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ModManager : MonoBehaviour
{
    [SerializeReference] public GameObject modPrefab;
    [SerializeReference] public GameObject modArray;
    [SerializeReference] public GameObject modPage;
    [SerializeReference] public APIHandler apiHandler;
    public TextMeshProUGUI modDescription;
    public TextMeshProUGUI modTitle;
    public TextMeshProUGUI modIDObject;
    public TMP_InputField searchQuery;
    public RawImage modImage;
    public GameObject modManagerMainpage;
    public GameObject modSearchMenu;
    public GameObject instanceMenu;
    public GameObject DLDImage;
    public GameObject DLImage;
    public GameObject errorMenu;

    public async void CreateMods()
    {
        ResetArray();
        SearchParser sq = apiHandler.GetSearchedMods();
        
        foreach (SearchResults searchResults in sq.hits)
        {
            async Task GetSetTexture()
            {
                UnityWebRequest modImageLink = UnityWebRequestTexture.GetTexture(searchResults.icon_url);
                modImageLink.SendWebRequest();

                while (!modImageLink.isDone)
                {
                    await Task.Delay(50);
                }

                if (modImageLink.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(modImageLink.error);
                }
                else
                {
                    Texture modImageTexture = ((DownloadHandlerTexture)modImageLink.downloadHandler).texture;
                    GameObject modObject = Instantiate(modPrefab, new Vector3(-10, -10, -10), Quaternion.identity);
                    modObject.GetComponentInChildren<RawImage>().texture = modImageTexture;
                    modObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = searchResults.title;
                    modObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = searchResults.description;
                    modObject.transform.SetParent(modArray.transform, false);
                    modObject.name = searchResults.project_id;
                    string currInstName = JNIStorage.apiClass.CallStatic<string>("getQCSupportedVersionName", InstanceButton.currentVersion);
                    AndroidJavaObject instance = JNIStorage.apiClass.CallStatic<AndroidJavaObject>("load", currInstName + "-fabric", JNIStorage.home);

                    try
                    {
                        if (!JNIStorage.apiClass.CallStatic<Boolean>("hasMod", InstanceButton.GetInstance(), searchResults.title))
                        {
                            modObject.transform.GetChild(3).gameObject.SetActive(false);
                        }
                        else 
                        {
                            modObject.transform.GetChild(3).gameObject.SetActive(true);
                            modObject.transform.GetChild(3).GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(delegate { RemoveMod(searchResults.title); });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"An error occurred: {ex}");
                        modObject.transform.gameObject.transform.GetChild(3).gameObject.SetActive(false);
                    }

                    modObject.GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(delegate
                    {
                        EventSystem.current.SetSelectedGameObject(modObject);
                        GameObject mod = GameObject.Find(EventSystem.current.currentSelectedGameObject.transform.name);
                        apiHandler.modID = mod.ToString().Replace("(UnityEngine.GameObject)", "");
                        CreateModPage();
                    });
                }
            }

            await GetSetTexture();
        }
    }
    
    public async void CreateModPage()
    {
        MetaParser mp = apiHandler.GetModInfo();
        instanceMenu.SetActive(false);
        modSearchMenu.SetActive(false);
        modManagerMainpage.SetActive(false);
        modPage.SetActive(true);

        async Task GetSetTexture()
        {
            UnityWebRequest modImageLink = UnityWebRequestTexture.GetTexture(mp.icon_url);
            modImageLink.SendWebRequest();

            while (!modImageLink.isDone)
            {
                await Task.Delay(50);
            }

            Texture modImageTexture = ((DownloadHandlerTexture)modImageLink.downloadHandler).texture;
            modDescription.text = mp.description;
            modTitle.text = mp.title;
            modImage.texture = modImageTexture;
            modIDObject.text = mp.slug;
            string currInstName = JNIStorage.apiClass.CallStatic<string>("getQCSupportedVersionName", InstanceButton.currentVersion);
            AndroidJavaObject instance = JNIStorage.apiClass.CallStatic<AndroidJavaObject>("load", currInstName + "-fabric", JNIStorage.home);


            try
            {
                if (!JNIStorage.apiClass.CallStatic<bool>("hasMod", InstanceButton.GetInstance(), mp.title))
                {
                    DLDImage.SetActive(false);
                    DLImage.SetActive(true);
                }
                else
                {
                    DLImage.SetActive(false);
                    DLDImage.SetActive(true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred: {ex}");
                DLDImage.SetActive(false);
                DLImage.SetActive(true);
            }

        }

        await GetSetTexture();
    }
    
    public void AddMod()
    {
        apiHandler.modID = modIDObject.text;
        MetaParser mp = apiHandler.GetModInfo();
        MetaInfo[] mi = apiHandler.GetModDownloads();

        string currentInstanceName = InstanceButton.currInstName;
        foreach (MetaInfo metaInfo in mi)
        {
            foreach (FileInfo file in metaInfo.files)
            {
                if (metaInfo.game_versions.Contains(currentInstanceName))
                {
                    string modName = mp.title;
                    string modUrl = file.url;
                    string modVersion = currentInstanceName;
                    Debug.Log("modName: " + modName + " | modUrl: " + modUrl + " | modVersion: " + modVersion);
                    string currInstName = JNIStorage.apiClass.CallStatic<string>("getQCSupportedVersionName", InstanceButton.currentVersion);
                    AndroidJavaObject instance = JNIStorage.apiClass.CallStatic<AndroidJavaObject>("load", currInstName + "-fabric", JNIStorage.home);
                    
                    if (instance == null)
                    {
                        errorMenu.GetComponentInChildren<TextMeshProUGUI>().text = "You must run this version of the game at least once before adding mods to the instance with ModManger!";
                        errorMenu.SetActive(true);
                    }
                    else
                    {
                        JNIStorage.apiClass.CallStatic("addCustomMod", InstanceButton.GetInstance(), modName, modVersion, modUrl);
                        DLImage.SetActive(false);
                        DLDImage.SetActive(true);
                    }
                    
                    return;
                }
            }
        }
    }
    
    public void RemoveMod(string name)
    {
        string currInstName = JNIStorage.apiClass.CallStatic<string>("getQCSupportedVersionName", InstanceButton.currentVersion);
        AndroidJavaObject instance = JNIStorage.apiClass.CallStatic<AndroidJavaObject>("load", currInstName + "-fabric", JNIStorage.home);
        JNIStorage.apiClass.CallStatic<bool>("removeMod", InstanceButton.GetInstance(), name);
        DLDImage.SetActive(false);
        DLImage.SetActive(true);
    }

    public void SearchMods()
    {
        apiHandler.searchQuery = searchQuery.text;
        CreateMods();
    }

    public void ResetArray()
    {
        int childCount = modArray.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--) {
            Transform child = modArray.transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}
