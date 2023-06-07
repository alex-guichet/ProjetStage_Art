using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMedia : MonoBehaviour, IPlayMedia
{
    public CurrencyName currencyName;
    public bool isWinning;
    
    public CurrencyName CurrencyName
    {
        get => currencyName;
        set => currencyName = value;
    }
    
    public Vector3 initialScale;
    
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
    
    public AudioSource audioSource;

    private bool _receivedActivation;
    
    public void SetFocusActive()
    {
        _receivedActivation = true;
        
        if( !audioSource.isPlaying )
        {
            audioSource.Play();
        }
    }

    private void Awake()
    {
        audioSource.Stop();
        audioSource.loop = true;
    }

    private void LateUpdate()
    {
        if( _receivedActivation )
        {
            _receivedActivation = false;
        }
        else
        {
            audioSource.Pause();
        }
    }
}
