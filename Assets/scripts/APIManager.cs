﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using DefaultNamespace;
using SilknowMap;
using Honeti;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class APIManager : Singleton<APIManager>
{
   

    [DllImport("__Internal")]
    private static extern void CancelLoadingData();
    [DllImport("__Internal")]
    private static extern void DataLoaded();


    [HideInInspector]
    public List<ManMadeObject> objectList;
    public string endpoint = "https://grlc.eurecom.fr/api-git/silknow/api/";
    public InitApp initAppRef;
    [SerializeField]
    public Dictionary<string, TimeElement> timeValues;

    private bool _sendingInfo = false;
    private int _currentLoadedPages = 0;
    private int _currentTotalPages = -1;

    private List<ManMadeObjectMod> prueba = new List<ManMadeObjectMod>();
    public void Awake()
    {

        endpoint = string.IsNullOrEmpty(PlayerPrefs.GetString("API_endpoint")) ? "https://grlc.eurecom.fr/api-git/silknow/api/" :PlayerPrefs.GetString("API_endpoint") ;
        
        timeValues = new Dictionary<string, TimeElement>();
        PopulateTimeDictionary();
    }

    private void PopulateTimeDictionary()
    {
        //AÑADIR SIGLOS 10 a 14 con strings localizados 
        //10th Century
        var tenth = new TimeElement(901,1000,10); 
        timeValues.Add("tenth century (dates CE)",tenth);
        //11th Century
        var eleventh = new TimeElement(1001,1100,11); 
        timeValues.Add("eleventh century (dates CE)",eleventh);
        //12th Century
        var twelfth = new TimeElement(1101,1200,12); 
        timeValues.Add("twelfth century (dates CE)",twelfth);
        //13th Century
        var thirteenth = new TimeElement(1201,1300,13); 
        timeValues.Add("thirteenth century (dates CE)",thirteenth);
        //14th Century
        var fourteenth = new TimeElement(1301,1400,14); 
        timeValues.Add("fourteenth century (dates CE)",fourteenth);
        
        //15th Century
        var fifteenth = new TimeElement(1401, 1500, 15); 
        timeValues.Add("fifteenth century (dates CE)",fifteenth);
        timeValues.Add("siglo quince",fifteenth);
        timeValues.Add("quindicesimo secolo",fifteenth);
        timeValues.Add("quinzième siècle",fifteenth);
        //16th Century
        var sixteenth = new TimeElement(1501, 1600, 16);
        timeValues.Add("sixteenth century (dates CE)",sixteenth);
        timeValues.Add("siglo dieciséis",sixteenth);
        timeValues.Add("sedicesimo secolo",sixteenth);
        timeValues.Add("seizième siècle",sixteenth);
        //17th Century
        var seventeenth = new TimeElement(1601,1700,17);
        timeValues.Add("seventeenth century (dates CE)",seventeenth);
        timeValues.Add("siglo diecisiete",seventeenth);
        timeValues.Add("diciassettesimo secolo",seventeenth);
        timeValues.Add("dix-septième siècle",seventeenth);
        //18th Century
        var eighteenth = new TimeElement(1701,1800,18);
        timeValues.Add("eighteenth century (dates CE)",eighteenth);
        timeValues.Add("siglo dieciocho",eighteenth);
        timeValues.Add("diciottesimo secolo",eighteenth);
        timeValues.Add("dix-huitième siècle",eighteenth);
        //19th Century
        var nineteenth = new TimeElement(1801,1900,19);
        timeValues.Add("nineteenth century (dates CE)",nineteenth);
        timeValues.Add("siglo diecinueve",nineteenth);
        timeValues.Add("diciannovesimo secolo",nineteenth);
        timeValues.Add("dix-neuvième siècle",nineteenth);
        //20th Century
        var twentieth = new TimeElement(1901,2000,20);
        timeValues.Add("twentieth century (dates CE)",twentieth);
        timeValues.Add("siglo veinte",twentieth);
        timeValues.Add("ventesimo secolo",twentieth);
        timeValues.Add("vingtième siècle",twentieth);
    }

    IEnumerator GetObjectList(string country = null,string technique = null, string time = null, string material = null)
    {
        Dictionary<string,string> queryParams = new Dictionary<string, string>();
        queryParams.Add("country",country);
        queryParams.Add("technique",technique);
        queryParams.Add("time",time);
        queryParams.Add("material",material);
        queryParams.Add("lang",I18N.instance.gameLang.ToString().ToLowerInvariant());
        
        string uri = endpoint;

        uri = String.Concat(uri, "obj_map"); 
        var firstElement = true;
        foreach (var param in queryParams)
        {
            if(string.IsNullOrEmpty(param.Value))
                continue;
            if (firstElement)
            {
                firstElement = false;
                uri = String.Concat(uri, "?" + param.Key + "=" + param.Value);
            }
            else
            {
                uri = String.Concat(uri, "&" + param.Key + "=" + param.Value);
            }
        }

        uri = String.Concat(uri, firstElement ? "?endpoint=http%3A%2F%2Fdata.silknow.org%2Fsparql" : "&endpoint=http%3A%2F%2Fdata.silknow.org%2Fsparql");
        
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.Log(": Error: " + webRequest.error);
            }
            else
            {
                var response = webRequest.downloadHandler.text;
                List<ManMadeObject> manMadeObjectList;
                try
                {
                    manMadeObjectList = JsonConvert.DeserializeObject<List<ManMadeObject>>(response);
                    
                }
                catch (JsonReaderException e)
                {
                    print("errorParsing: "+e.Message);
                    yield break;
                }

                if (manMadeObjectList.Count < 1)
                {
                    print("empty response");
                    yield break;
                }

                objectList = manMadeObjectList;
               
                StartCoroutine(initAppRef.LoadRestData(objectList.ToArray()));
                
            }
        }
    }
    public IEnumerator GetObjectDetail(string objectId, Action<string> callback = null,  Action<string> errorCallback = null)
    {
        if (string.IsNullOrEmpty(objectId))
            yield break;
        Dictionary<string,string> queryParams = new Dictionary<string, string>();
        queryParams.Add("id",objectId);
        queryParams.Add("lang",I18N.instance.gameLang.ToString());
        string uri = endpoint;
        uri = String.Concat(uri, "obj_detail"); 
        var firstElement = true;
        foreach (var param in queryParams)
        {
            if(string.IsNullOrEmpty(param.Value))
                continue;
            if (firstElement)
            {
                firstElement = false;
                uri = String.Concat(uri, "?" + param.Key + "=" + param.Value);
            }
            else
            {
                uri = String.Concat(uri, "&" + param.Key + "=" + param.Value);
            }
        }

        uri = String.Concat(uri, firstElement ? "?endpoint=http%3A%2F%2Fdata.silknow.org%2Fsparql" : "&endpoint=http%3A%2F%2Fdata.silknow.org%2Fsparql");

        //Debug.Log(uri);
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(": Error: " + webRequest.error);
                if (errorCallback != null) errorCallback("Error: " + webRequest.error);
            }
            else
            {
                var response = webRequest.downloadHandler.text;

                if (callback == null)
                    print(response);
                else
                    callback(response);
            }
        }
    }
    IEnumerator GetThesaurusConcept(string vocabularyId, string language ="en")
    {
        if (string.IsNullOrEmpty(vocabularyId))
            yield break;
        string uri = endpoint;
        uri = String.Concat(uri, "thesaurus"); 
        uri = String.Concat(uri, "?id=" + vocabularyId);
        uri = String.Concat(uri, "&lang=" + language);
        uri = String.Concat(uri, "&endpoint=http%3A%2F%2Fdata.silknow.org%2Fsparql");

        Debug.Log(uri);
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(": Error: " + webRequest.error);
            }
            else
            {
                var response = webRequest.downloadHandler.text;
                print(response);
            }
        }
    }

    public void OnButtonTestClick()
    {
        StartCoroutine(GetObjectList(technique:"damask"));
    }
    public void TestFranceTextiles()
    {
        StartCoroutine(GetObjectList(country:"Spain"));
    }
    public void TestLoadTextilesFromHTML()
    {
       
        //var jsontest = Resources.Load("testFranceTextiles");
        var jsontest = Resources.Load("parsed_results_lite");
        TextAsset temp = jsontest as TextAsset;
        if (temp != null)
        {
            LoadJSONFromHTML(temp.text);
            Destroy(temp);
            Resources.UnloadAsset(jsontest);
        }

        Resources.UnloadUnusedAssets();


    }
    public void TestObjectDetail()
    {
        StartCoroutine(GetObjectDetail("http://data.silknow.org/object/4fa6a1a1-0aab-333e-9846-3c04a114e236"));
    }

    public void LoadJSONFromHTML(string json)
    {
        var startTime = Stopwatch.StartNew();
        objectList = JsonConvert.DeserializeObject<List<ManMadeObject>>(json);
        EvaluationConsole.instance.AddLine($"Tiempo Deserialización JSON: {startTime.ElapsedMilliseconds * 0.001f} s");
        if (objectList != null)
        {
            StartCoroutine(initAppRef.LoadRestData(objectList.ToArray()));
            objectList.Clear();
        }

        GC.Collect();
    }
    /*public void LoadJSONFromHTML(string json)
    {
        var startTime = Stopwatch.StartNew();

        prueba = JsonUtility.FromJson<RootObject>("{\"manMadeObjects\":"+json+"}").manMadeObjects.ToList();
        EvaluationConsole.instance.AddLine($"Deserializar JSON objetos: {startTime.ElapsedMilliseconds * 0.001f} s");
        EvaluationConsole.instance.AddLine(prueba[25].production.location[0].lat.ToString());
        GC.Collect();
    }*/
    public void LoadJSONFromStream()
    {
        var startTime = Stopwatch.StartNew();
        HttpClient client = new HttpClient();

        using (Stream s = client.GetStreamAsync("https://silknow.eu/silknow/STMAPS_Evaluation/parsed_results30000.json").Result)
        using (StreamReader sr = new StreamReader(s))
        using (JsonReader reader = new JsonTextReader(sr))
        {
            JsonSerializer serializer = new JsonSerializer();

            // read the json from a stream
            // json size doesn't matter because only a small piece is read at a time from the HTTP request
            objectList = serializer.Deserialize<List<ManMadeObject>>(reader);
        }
        EvaluationConsole.instance.AddLine($"Deserializar JSON objetos Stream: {startTime.ElapsedMilliseconds * 0.001f} s");
        //StartCoroutine(initAppRef.LoadRestData(objectList.ToArray()));
    }
    
    public void StartDumpingJSON(string numberOfPages)
    {
        if (_sendingInfo)
            return;
        _sendingInfo = true;
        _currentLoadedPages = 0;
        _currentTotalPages = Int32.Parse(numberOfPages);
        objectList.Clear();
        MapUIManager.instance.ShowProgressBar(Int32.Parse(numberOfPages));
    }

    public void AppendJSONData(string json)
    {
        if(!_sendingInfo || _currentLoadedPages>= _currentTotalPages)
            return;
        
        var adasilkresultpage = JsonConvert.DeserializeObject<AdasilkResultPage>(json);

        
        if(adasilkresultpage.pageNumber != _currentLoadedPages +1)
            return;
        _currentLoadedPages++;
        var manMadeObjectList = adasilkresultpage.pageResults;
       
        if(manMadeObjectList.Count>0)
            objectList  = objectList.Concat(manMadeObjectList).ToList();
        MapUIManager.instance.UpdateProgressBar(_currentLoadedPages.ToString());
        //print("AppendJSONData Total JSON count: "+objectList.Count);
    }

    public void EndDumpingJSON()
    {
        if (!_sendingInfo)
            return;
        //print("EndDumpingJSON");
        _sendingInfo = false;
        initAppRef.LoadRestData(objectList.ToArray());
        MapUIManager.instance.HideProgressBar();
        objectList.Clear();
    }
    public void CancelDumpingJSON()
    {
        //Debug.Log("Cancel Dumping JSON");
        _sendingInfo = false;
        MapUIManager.instance.HideProgressBar();
        objectList.Clear();
        #if UNITY_WEBGL && !UNITY_EDITOR
            CancelLoadingData();
        #endif
    }
    
    
    IEnumerator CallToMapManager(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
     
    }
    
   
    public void OnFlatMap()
    {
        MapUIManager.instance.ToggleMapViewingMode();
        SilkMap.Instance.reactivate();
    }
    public void ToggleLanguage()
    {
        var lang = I18N.instance.gameLang == LanguageCode.EN ? "ES" : "EN";
        I18N.instance.setLanguage(lang);
        OnlineMaps.instance.language = I18N.instance.gameLang.ToString().ToLower(CultureInfo.InvariantCulture);
    }
    public void SetAPIEndpoint(string apiEndpoint)
    {
        if (!string.IsNullOrEmpty(apiEndpoint))
        {
            PlayerPrefs.SetString("API_endpoint",apiEndpoint);
            endpoint = apiEndpoint;
        }
       
    }
    public void SetLanguage(string lang)
    {
        if (!string.IsNullOrEmpty(lang))
        {
            I18N.instance.setLanguage(lang);
            OnlineMaps.instance.language = I18N.instance.gameLang.ToString().ToLower(CultureInfo.InvariantCulture);
        }
       
    }
    public void SetAnalytics(string value)
    {
        print(value);
        if (bool.TryParse(value, out bool result))
        {
            AnalyticsMonitor.instance.SetAnalyticsStatus(result);
            Debug.Log("Analytics tracking: "+result);
        }


        else
        {
            Debug.Log("Incorrect Value for Analytics");
        }

       
    }
    public void ShowLoading()
    {
        MapUIManager.instance.ShowLoadingData();
    }

    public void LoadDataset(int objectCount)
    {
        Object jsontest = null;
        switch (objectCount)
        {
            case 300:
                jsontest = Resources.Load("parsed_results300");
                break;
            case 3000:
                jsontest = Resources.Load("parsed_results3000");
                break;
            case 15000:
                jsontest = Resources.Load("parsed_results15000");
                break;
            case 30000:
                jsontest = Resources.Load("parsed_results_KG");
                break;
        }

        if (jsontest == null)
        {
            print("jsontest es null");
            return;
        }

        TextAsset temp = jsontest as TextAsset;
        if (temp != null)
        {
            LoadJSONFromHTML(temp.text);
            Resources.UnloadAsset(temp);
            Resources.UnloadAsset(jsontest);
            GC.Collect();
        }
        else
        {
            print("temp es null");
        }

        Resources.UnloadUnusedAssets();
    }

    public void OnDataLoaded()
    {
        //Llamada a Javascript
        #if !UNITY_EDITOR
        DataLoaded();
        #endif
    }


}

