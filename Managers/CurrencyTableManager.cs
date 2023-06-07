using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CurrencyTableManager : Singleton<CurrencyTableManager>
{
    [Header("Border Update Setting")] 
    public Color highlightColor;
    public Vector2 highlightOutlineThickness;
    public float updateColorTime = 2f;
    public float flickerTime = 2f;
    public float flickerTimeInterval = 0.5f;
    
    [Header("Numbers Update Setting")]
    public float updateRowTime = 5f;
    public float updateGraphTime = 0.5f;

    [Header("Currency Settings")]
    public List<CurrencyManager> currencyManagerList;
    public List<GameObject> borderList;
    public List<Currency> coinCurrencies;
    public float[] rotationSpeeds;

    private Coroutine _lerpBorderColor;
    private Coroutine _updateRows;
    private Color _initBorderColor;
    private Vector2 _initOutlineThickness;


    public void SingleUpdateCurrencyTable(CurrencyName currency)
    {
        float update_watch_time = (EyeTrackingController.Instance.updateWatchTime * 100f)/12f;
        foreach (var currency_manager in currencyManagerList)
        {
            if (currency_manager.currencyObject.currencyName == currency)
            {
                currency_manager.attentionTime += update_watch_time;
                continue;
            }
            currency_manager.attentionTime -= update_watch_time;
        }
        GraphicManager.Instance.UpdateGraphic(currency);
    }

    public void WinningUpdateCurrencyTable(CurrencyName currency)
    {
        int index = currencyManagerList.FindIndex(x => x.currencyObject.currencyName == currency);
        //var outline_group = currencyManagerList[index].GetComponentsInChildren<Outline>();
        var outline_transform = borderList[index].transform;
        var outline_group = outline_transform.GetComponentsInChildren<Image>();
        var currency_row = currencyManagerList[index];
        outline_transform.GetComponentInChildren<Canvas>().sortingOrder++;
        _initBorderColor = outline_group[0].color;
        //_initBorderColor = outline_group[0].effectColor;
        //_initOutlineThickness = outline_group[0].effectDistance;

        if (_lerpBorderColor != null)
        {
            StopCoroutine(_lerpBorderColor);
        }
        _lerpBorderColor = StartCoroutine(LerpBorderColor());
        
        IEnumerator LerpBorderColor()
        {
            SoundManager.Instance.soundEffects[SoundEffectType.CurrencyHighlight].Play();
            float color_timer = 0f;
            Color outline_color = new();
            Vector2 outline_thickness = new();
            
            while (color_timer < updateColorTime)
            {
                color_timer += Time.deltaTime;
                float interpolation = color_timer / updateColorTime;
                outline_color = Color.Lerp(_initBorderColor, highlightColor, interpolation);
                //outline_thickness = Vector2.Lerp(_initOutlineThickness, highlightOutlineThickness, interpolation);

                foreach (var outline in outline_group)
                {
                    outline.color = outline_color;
                    //outline.effectDistance = outline_thickness;
                }
                yield return new WaitForEndOfFrame();
            }
            
            SoundManager.Instance.soundEffects[SoundEffectType.CurrencyHighlight].Stop();
            
            if (_updateRows != null)
            {
                StopCoroutine(_updateRows);
            }
            _updateRows = StartCoroutine(UpdateRows());
        }
        
        IEnumerator UpdateRows()
        {
            float update_row_timer = 0f;
            float update_graph_timer = 0f;
            float time_divider = 1f;
            float winning_watch_time = (EyeTrackingController.Instance.winningWatchTime * 100f);
            
            Dictionary<CurrencyName, float> current_attention_times = new();
            
            SoundManager.Instance.soundEffects[SoundEffectType.AttentionTimeIncrease].Play();
            
            foreach (var currency_manager in currencyManagerList)
            {
                current_attention_times.Add(currency_manager.currencyObject.currencyName, currency_manager.attentionTime);
            }
            
            while (update_row_timer < updateRowTime)
            {
                currency_row.attentionTime = Mathf.Lerp(current_attention_times[currency_row.currencyObject.currencyName],current_attention_times[currency_row.currencyObject.currencyName] + winning_watch_time, update_row_timer/ updateRowTime);
                
                foreach (var currency_manager in currencyManagerList.Where(x => x.currencyObject.currencyName != currency))
                {
                    currency_manager.attentionTime = Mathf.Lerp(current_attention_times[currency_manager.currencyObject.currencyName],current_attention_times[currency_manager.currencyObject.currencyName] - winning_watch_time/12f, update_row_timer/ updateRowTime);
                }
                
                float time =  Time.deltaTime;
                update_row_timer += time;
                update_graph_timer += time;
                
                if (update_graph_timer > updateGraphTime)
                {
                    GraphicManager.Instance.UpdateGraphic(currency);
                    update_graph_timer = 0f;
                }
                yield return new WaitForEndOfFrame();
            }

            StartCoroutine(FlickerBorderColor());
        }
        
        IEnumerator FlickerBorderColor()
        {
            float number_repetition = Mathf.Round(flickerTime / flickerTimeInterval)/2;

            for (int i = 0; i < number_repetition; i++)
            {
                foreach (var outline in outline_group)
                {
                    outline.color = _initBorderColor;
                }
                
                yield return new WaitForSeconds(flickerTimeInterval);
                
                foreach (var outline in outline_group)
                {
                    outline.color = highlightColor;
                }
                
                yield return new WaitForSeconds(flickerTimeInterval);
            }
            
            foreach (var outline in outline_group)
            {
                outline.color = _initBorderColor;
                //outline.effectDistance = _initOutlineThickness;
            }

            CircleBManager.Instance.VacuumImage();
        }
    }

    public void Start()
    {
        List<int> index_currencies = new();
        
        foreach (var currency_manager in currencyManagerList)
        {
            int index = Random.Range(0, coinCurrencies.Count);
            while (index_currencies.FindIndex(x => x == index) != -1)
            {
                index = Random.Range(0, coinCurrencies.Count); 
            }
            index_currencies.Add(index);
            currency_manager.currencyObject = coinCurrencies[index];
            GameObject coin = Instantiate(currency_manager.currencyObject.currencyGameObject, currency_manager.coinPlaceHolder);
            LayerMask UI_layer_mask = LayerMask.NameToLayer("UI");
            coin.layer = UI_layer_mask;
            coin.transform.GetChild(0).gameObject.layer = UI_layer_mask;
        }

        int random_index = index_currencies[Random.Range(0, index_currencies.Count)];
        ZoneFManager.Instance.InitializeIntroPanel(coinCurrencies[random_index].currencyName);
        
        for (int i = 0; i < GraphicManager.Instance.graphicContainers.Length; i++)
        {
            List<int> value_list = new List<int>();
            
            for (int y = 0; y < 10; y++)
            {
                value_list.Add(Random.Range(10,60));
            }
            
            GraphicManager.Instance.GraphicValueDictionary.Add(currencyManagerList[i].currencyObject.currencyName,value_list);
            GraphicManager.Instance._currentGraphicContainer = GraphicManager.Instance.graphicContainers[i];
            GraphicManager.Instance.ShowGraphic(value_list);
        }
        
        for (int i = 0; i < currencyManagerList.Count; i++)
        {
            var value_list = GraphicManager.Instance.GraphicValueDictionary.ElementAt(i).Value;
            currencyManagerList[i].stockValue = value_list[^1];
            currencyManagerList[i].attentionTime = value_list.Sum();
        }
        PhotoPoolManager.Instance.InitializePhotoCurrency();
        //PhotoPoolManager.Instance.CreatePhotoCloud();
    }

    private void Update()
    {
        for(int i = 0; i < currencyManagerList.Count; i++)
        {
            currencyManagerList[i].coinPlaceHolder.Rotate(0f, rotationSpeeds[i]* Time.deltaTime,0f);
        }
    }

    private void OnApplicationQuit()
    {
        StopCoroutine(_lerpBorderColor);
        StopCoroutine(_updateRows);
    }
}

[System.Serializable]
public class Currency
{
    public CurrencyName currencyName;
    public GameObject currencyGameObject;
    public GameObject characterGameObject;
}
