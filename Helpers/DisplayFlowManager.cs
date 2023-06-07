using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using URPGlitch.Runtime.AnalogGlitch;
using URPGlitch.Runtime.DigitalGlitch;

public class DisplayFlowManager : Singleton<DisplayFlowManager>
{
    
    public List<StepFlow> flowTimerManager;
    
    public GameObject languageSelector;
    public CircleAManager circleAManager;
    public GameObject calibrationCircle;
    public GameObject circlesParent;
    public GameObject gameObjectStep2;
    public TextWriter textWriterStep6;
    public TextWriter textWriterStep7;

    [Header("Canvas Settings")] 
    public CanvasGroup mainCanvas;
    public float displayTime;
    
    [Header("Glitch Settings")] 
    public float glitchTime = 4f;
    public Volume volume;

    private AnalogGlitchVolume _analogVolume;
    private DigitalGlitchVolume _digitalVolume;

    private Coroutine _playGlitchCoroutine;
    private Coroutine _displayMainCanvasCoroutine;

    private TextWriter _textWriterStep2;


    public void StepManager_OnStepChanged( GameStep old_step, GameStep new_step )
    {
        switch (new_step)
        {
            case GameStep.Calibration:
                circleAManager.gameObject.SetActive(true);
                languageSelector.SetActive(false);
                break;
            case GameStep.Appearing:
                SoundManager.Instance.soundEffects[SoundEffectType.WorldAppearance].Play();
                circleAManager.FadeBackGround();
                calibrationCircle.SetActive(false);
                circlesParent.SetActive(true);
                //DisplayMainCanvas();
                FadeController.Instance.FadeLensFlares();
                StepManager.Instance.NextStep();
                break;
            case GameStep.CircleA: 
                break;
            case GameStep.CircleB:
                CircleBManager.Instance.ConvertImage();
                break;
            case GameStep.CurrencyTable:
                CurrencyTableManager.Instance.WinningUpdateCurrencyTable(EyeTrackingController.Instance.WinningCurrency);
                break;
            case GameStep.Magnify:
                EarthCircleCManager.Instance.Magnify();
                break;
            case GameStep.Step6Text:
                gameObjectStep2.SetActive(false);
                textWriterStep6.gameObject.SetActive(true);
                textWriterStep6.StartWriting();
                break;
            case GameStep.ShowNotifications:
                ZoneFManager.Instance.ExpandBubbles();
                break;
            case GameStep.StopMagnify:
                EarthCircleCManager.Instance.StopMagnify();
                break;
            case GameStep.ReduceNotifications:
                ZoneFManager.Instance.ReduceBubbles();
                break;
            case GameStep.Step7Text:
                textWriterStep6.gameObject.SetActive(false);
                textWriterStep7.gameObject.SetActive(true);
                textWriterStep7.StartWriting();
                break;
            case GameStep.GlitchEffect:
                if (_playGlitchCoroutine != null)
                {
                    StopCoroutine(_playGlitchCoroutine);
                }
                _playGlitchCoroutine = StartCoroutine(PlayGlitch());
                break;
            case GameStep.Restart:
                SceneManager.LoadScene(0);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(new_step), new_step, null);
        }
    }

    public void DisplayMainCanvas()
    {
        if (_displayMainCanvasCoroutine != null)
        {
            StopCoroutine(_displayMainCanvasCoroutine);
        }
        _displayMainCanvasCoroutine = StartCoroutine(DisplayMainCanvasCoroutine());
    }
    
    
    private IEnumerator DisplayMainCanvasCoroutine()
    {
        float display_timer = 0f;
        
        while (display_timer < displayTime)
        {
            mainCanvas.alpha = Mathf.Lerp(0f,1f,display_timer/displayTime);
            display_timer += Time.deltaTime;
            yield return null;
        }
        _textWriterStep2.StartWriting();
    }

    public void StartWriting()
    {
        _textWriterStep2.StartWriting();
    }
    
    private IEnumerator PlayGlitch()
    {
        SoundManager.Instance.soundEffects[SoundEffectType.GlitchEffect].Play();
        float glitch_timer = 0f;
        while (glitch_timer < glitchTime)
        {
            float interpolation = (glitch_timer / glitchTime)/4f;
            _analogVolume.colorDrift.Override(interpolation);
            _analogVolume.scanLineJitter.Override(interpolation);
            _analogVolume.horizontalShake.Override(interpolation);
            _digitalVolume.intensity.Override(interpolation);
            glitch_timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SoundManager.Instance.soundEffects[SoundEffectType.GlitchEffect].Stop();
        StepManager.Instance.NextStep();
    }

    public override void Awake()
    {
        base.Awake();

        _textWriterStep2 = gameObjectStep2.GetComponentInChildren<TextWriter>();
        //mainCanvas.alpha = 0.0f;
        circlesParent.SetActive( false ); // must stay in awake because of coroutines in the circleA
        
        if (volume.profile.TryGet<AnalogGlitchVolume>(out var analog_volume))
        {
            _analogVolume = analog_volume;
        }
        
        if (volume.profile.TryGet<DigitalGlitchVolume>(out var digital_volume))
        {
            _digitalVolume = digital_volume;
        }
    }

    private void Start()
    {
        StepManager.Instance.StepChanged += StepManager_OnStepChanged;
    }
}
