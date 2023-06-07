using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleBManager : Singleton<CircleBManager>
{
    public Renderer circleRenderer;

    public float highlightStrengthTarget = 60.0f;

    public float highlightAmountTarget = 5.0f;

    public float highlightTime = 10.0f;
    public float stoppingTime = 0.5f;
    public float vacuumTime = 4f;

    public float flipSpeed = 0.002f;
    public Vector2 offsetSpeed = new Vector2( 0.5f, 0.0f );

    private float _highlightTimer;
    private float _stoppingTimer;
    private float _flipValue;
    private float _initialFlipSpeed;
    private Vector2 _offset;
    private Vector2 _initialOffsetSpeed;
    
    private Coroutine _binaryConversionCoroutine;
    private Coroutine _vacuumConversionCoroutine;
    private Material _circleMaterial;
    private static readonly int HighlightStrength = Shader.PropertyToID( "_HighlightStrength" );
    private static readonly int HighlightAmount = Shader.PropertyToID( "_HighlightAmount" );
    private static readonly int FlipSpeed = Shader.PropertyToID( "_FlipSpeed" );
    private static readonly int Offset = Shader.PropertyToID( "_Offset" );

    public void ConvertImage()
    {
        if (_binaryConversionCoroutine != null)
        {
            StopCoroutine(_binaryConversionCoroutine);
        }
        
        _binaryConversionCoroutine = StartCoroutine(BinaryConversion());
    }
    
    public void VacuumImage()
    {
        if (_vacuumConversionCoroutine != null)
        {
            StopCoroutine(_vacuumConversionCoroutine);
        }
        
        _vacuumConversionCoroutine = StartCoroutine(VacuumBinaryCode());
    }

    private IEnumerator BinaryConversion()
    {
        SoundManager.Instance.soundEffects[SoundEffectType.BinaryConversion].Play();
        while( _highlightTimer <= highlightTime )
        {
            _highlightTimer += Time.deltaTime;

            _circleMaterial.SetFloat( HighlightStrength, Mathf.Lerp( 0.0f, highlightStrengthTarget, _highlightTimer / highlightTime ) );
            _circleMaterial.SetFloat( HighlightAmount, Mathf.Lerp( 1f, highlightAmountTarget, _highlightTimer / highlightTime ) );

            yield return null;
        }
        
        _circleMaterial.SetFloat( HighlightStrength, highlightStrengthTarget );
        _circleMaterial.SetFloat( HighlightAmount, highlightAmountTarget );
        
        yield return null;

        while( _stoppingTimer <= stoppingTime )
        {
            _stoppingTimer += Time.deltaTime;
            
            offsetSpeed = Vector2.Lerp( _initialOffsetSpeed, Vector2.zero, _stoppingTimer / stoppingTime );
            flipSpeed = Mathf.Lerp( _initialFlipSpeed, 0.0f, _stoppingTimer / stoppingTime );
            
            yield return null;
        }
        
        _circleMaterial.SetFloat( FlipSpeed, 0.0f );

        yield return null;
        
        SoundManager.Instance.soundEffects[SoundEffectType.BinaryConversion].Stop();
        StepManager.Instance.NextStep();
    }

    public IEnumerator VacuumBinaryCode()
    {
        SoundManager.Instance.soundEffects[SoundEffectType.BinaryVacuum].Play();
        float vacuum_timer = 0f;
        while( vacuum_timer <= vacuumTime )
        {
            vacuum_timer += Time.deltaTime;
            _circleMaterial.SetFloat( HighlightAmount, Mathf.Lerp( highlightAmountTarget, 0.0f, vacuum_timer / vacuumTime ) );
            yield return null;
        }
        _circleMaterial.SetFloat( HighlightAmount, 0f );
        
        SoundManager.Instance.soundEffects[SoundEffectType.BinaryVacuum].Stop();
        StepManager.Instance.NextStep();
    }

    public void Restart()
    {
        _offset = Vector2.zero;
        offsetSpeed = _initialOffsetSpeed;
        flipSpeed = _initialFlipSpeed;
        _circleMaterial.SetFloat( HighlightStrength, 0 );
        _circleMaterial.SetFloat( HighlightAmount, 0 );
        _circleMaterial.SetFloat( FlipSpeed, flipSpeed );
        _circleMaterial.SetVector( Offset, _offset );
    }

    public override void Awake()
    {
        base.Awake();

        _circleMaterial = circleRenderer.material;
        _initialOffsetSpeed = offsetSpeed;
        _initialFlipSpeed = flipSpeed;
    }

    private void Update()
    {
        _offset += offsetSpeed * Time.deltaTime;
        _flipValue += flipSpeed * Time.deltaTime;
        _circleMaterial.SetVector( Offset, _offset );
        _circleMaterial.SetFloat( FlipSpeed, _flipValue );
    }
    
    void OnApplicationQuit()
    {
        StopCoroutine(_binaryConversionCoroutine);
        StopCoroutine(_vacuumConversionCoroutine);
    }
}
