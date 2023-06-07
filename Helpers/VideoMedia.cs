using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoMedia : MonoBehaviour, IPlayMedia
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
    
    public VideoPlayer videoPlayer;

    private bool _receivedActivation;
    
    public void SetFocusActive()
    {
        _receivedActivation = true;
        
        if( !videoPlayer.isPlaying )
        {
            videoPlayer.Play();
        }
    }

    private IEnumerator StopSounds()
    {
        var timer = 0.0f;

        //while( timer < 1.0f )
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        videoPlayer.Pause();
        videoPlayer.isLooping = true;
        videoPlayer.SetDirectAudioMute( 0, false );
    }

    private void Awake()
    {
        videoPlayer.SetDirectAudioMute( 0, true );
    }

    private void Start()
    {
        StartCoroutine( StopSounds() );
    }

    private void LateUpdate()
    {
        if( _receivedActivation )
        {
            _receivedActivation = false;
        }
        else
        {
            videoPlayer.Pause();
        }
    }
}
