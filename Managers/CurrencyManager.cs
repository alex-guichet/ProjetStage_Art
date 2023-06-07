using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CurrencyName
{
    Angafama,
    Cherribym,
    Donaldino,
    Empereur,
    Funes,
    Melimelo,
    Tychi,
    YueTu
}

public class CurrencyManager : MonoBehaviour
{
    public Currency currencyObject;
    public RectTransform coinPlaceHolder;
    public TextMeshProUGUI stockValueTextMeshPro;
    public TextMeshProUGUI attentionTimeTextMeshPro;

    private float _stockValue;
    private float _attentionTime;
    
    public float stockValue
    {
        get => _stockValue;
        set
        {
            _stockValue = Mathf.Round(value);
            stockValueTextMeshPro.text = _stockValue.ToString(CultureInfo.InvariantCulture);
        }
    }
    
    public float attentionTime
    {
        get => _attentionTime;
        set
        {
            _attentionTime = Mathf.Round(value);
            attentionTimeTextMeshPro.text = _attentionTime.ToString(CultureInfo.InvariantCulture)+" cs.";
        }
    }
}
