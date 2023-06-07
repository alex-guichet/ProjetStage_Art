using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PictureMedia : MonoBehaviour, IPlayMedia
{
    public CurrencyName currencyName;
    public Vector3 initialScale;
    public bool isWinning;
    
    public CurrencyName CurrencyName
    {
        get => currencyName;
        set => currencyName = value;
    }
    
    public Vector3 InitialScale
    {
        get => initialScale;
        set => initialScale = value;
    }
    
    public bool IsWinning
    {
        get => isWinning;
        set => isWinning = value;
    }
    
    public void SetFocusActive()
    {
    }
}
