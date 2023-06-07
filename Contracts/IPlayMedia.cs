using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayMedia
{
    
    bool IsWinning
    {
        get;
        set;
    }
    
    CurrencyName CurrencyName
    {
        get;
        set;
    }
    
    Vector3 InitialScale
    {
        get;
        set;
    }
    
    public void SetFocusActive();
}
