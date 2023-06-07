using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class EarthCircleCManager : Singleton<EarthCircleCManager>
{
    [Header("Magnifying glass settings")]
    public GameObject magnifyingGlass;
    public float expansionTime;
    public CurrencyName currencySelected;

    [Header("Extra Coin settings")]
    public GameObject extraCoin;
    public float appearingDelayTime = 2f;
    public float appearingTime = 1f;
    
    private CharacterSetup _currentCharacterSetup;
    private GameObject _currentCoinGameObject;
    private GameObject _currentMagnifyingGlass;

    private Vector3 _characterStartPosition;
    private Quaternion _characterStartRotation;
    private Animator _characterAnimator;

    private Coroutine _expandCoroutine;
    private Coroutine _addExtraCoin;
    private Coroutine _stopMagnify;
    

    public void Magnify()
    {
        //CurrencyName winning_currency = EyeTrackingController.Instance.WinningCurrency;
        CurrencyName winning_currency = currencySelected;
        var coin_currency_list = CurrencyTableManager.Instance.coinCurrencies;
        var index_currency = coin_currency_list.FindIndex(x => x.currencyName == winning_currency);

        if (index_currency == -1)
            return;
        _currentCoinGameObject = coin_currency_list[index_currency].currencyGameObject;
        var character_object = coin_currency_list[index_currency].characterGameObject;
        _currentCharacterSetup = character_object.GetComponent<CharacterSetup>();
        _characterAnimator = character_object.GetComponentInChildren<Animator>();

        if (winning_currency != CurrencyName.Tychi )
        {
            _characterAnimator.enabled = false;
        }
        
        _currentCharacterSetup.IsStopped = true;

        if (_currentCharacterSetup.bgCurve != null)
        {
            _currentCharacterSetup.bgCurve.gameObject.SetActive(false);
        }
        
        _characterStartPosition = character_object.transform.localPosition;

        var character_position = character_object.transform.position;
        
        if (character_position is { x: < 0.77f, y: > 0f } or { x: > 1.8f })
        {
            character_position.z = 9.25f;
        }
        else
        {
            character_position.z = 9.25f;
        }
        
        Vector3 spawn_position = character_position;
        spawn_position.z -= 0.5f;
        
        _currentMagnifyingGlass = Instantiate(magnifyingGlass, spawn_position, magnifyingGlass.transform.rotation);

        if (_expandCoroutine != null)
        {
            StopCoroutine(_expandCoroutine);
        }
        _expandCoroutine = StartCoroutine(ExpandMagnifyGlass(character_position));

        Camera camera_magnifying_glass = _currentMagnifyingGlass.GetComponentInChildren<Camera>();
        camera_magnifying_glass.orthographicSize = _currentCharacterSetup.projectionSize;
        
        Transform camera_transform = camera_magnifying_glass.transform;

        Vector3 camera_local_position = camera_transform.localPosition;
        camera_local_position += _currentCharacterSetup.cameraOffset;
        camera_transform.localPosition = camera_local_position;
    }

    public void StopMagnify()
    {
        _stopMagnify = StartCoroutine(StopMagnifyCoroutine());
    }

    private IEnumerator ExpandMagnifyGlass(Vector3 character_position)
    {
        float expansion_timer = 0f;
        Vector3 start_scale = _currentMagnifyingGlass.transform.localScale;
        _characterStartRotation = _currentCharacterSetup.transform.rotation;
        Quaternion start_rotation = _characterStartRotation;
        Quaternion end_rotation = Quaternion.Euler(0f,180f,0f);
        Vector3 start_position = _currentCharacterSetup.transform.position;
        float interpolation;
        bool has_played_sound = false;
        
        while (expansion_timer < expansionTime)
        {
            interpolation = expansion_timer / expansionTime;
            _currentMagnifyingGlass.transform.localScale = Vector3.Lerp(Vector3.zero, start_scale, interpolation);
            _currentCharacterSetup.transform.rotation = Quaternion.Slerp(start_rotation, end_rotation, interpolation);
            _currentCharacterSetup.transform.position = Vector3.Lerp(start_position, character_position, interpolation);

            if (interpolation > 0.7f && !has_played_sound)
            {
                SoundManager.Instance.soundEffects[SoundEffectType.CharacterZoom].Play();
                has_played_sound = true;
            }
            
            expansion_timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _currentCharacterSetup.transform.rotation = end_rotation;
        _currentMagnifyingGlass.transform.localScale = start_scale;
        
        if (_addExtraCoin != null)
        {
            StopCoroutine(_addExtraCoin);
        }
        _addExtraCoin = StartCoroutine(AddExtraCoin());
    }

    private IEnumerator AddExtraCoin()
    {
        yield return new WaitForSeconds(appearingDelayTime);
        
        SoundManager.Instance.soundEffects[SoundEffectType.CoinAddition].Play();
        
        Transform coin_place_holder = _currentMagnifyingGlass.transform.GetComponentInChildren<Canvas>().transform;
        GameObject extra_coin = Instantiate(extraCoin, coin_place_holder);
        
        GameObject coin = Instantiate(_currentCoinGameObject, extra_coin.transform);
        coin.gameObject.GetComponentInChildren<MeshRenderer>().gameObject.layer = LayerMask.NameToLayer("MagnifyingGlass");
        
        float appearing_timer = 0f;
        Vector3 start_position = coin_place_holder.position;
        
        while (appearing_timer < appearingTime)
        {
            coin_place_holder.position = Vector3.Lerp(start_position, start_position + Vector3.up, appearing_timer / appearingTime);
            appearing_timer += Time.deltaTime;
            yield return null;
        }

        coin_place_holder.position = start_position + Vector3.up;
        coin_place_holder.gameObject.SetActive(false);
        
        yield return new WaitForSeconds(appearingDelayTime);
        
        //StepManager.Instance.NextStep();
    }
    
    private IEnumerator StopMagnifyCoroutine()
    {
        float expansion_timer = 0f;
        Vector3 start_scale = _currentMagnifyingGlass.transform.localScale;
        Quaternion start_rotation = _currentCharacterSetup.transform.rotation;
        Vector3 start_position = _currentCharacterSetup.transform.localPosition;
        float interpolation;
        
        while (expansion_timer < expansionTime)
        {
            interpolation = expansion_timer / expansionTime;
            _currentMagnifyingGlass.transform.localScale = Vector3.Lerp(start_scale, Vector3.zero, interpolation);
            _currentCharacterSetup.transform.rotation = Quaternion.Slerp(start_rotation, _characterStartRotation, interpolation);
            _currentCharacterSetup.transform.localPosition = Vector3.Lerp(start_position, _characterStartPosition, interpolation);
                
            expansion_timer += Time.deltaTime;
            yield return null;
        }

        _currentCharacterSetup.transform.rotation = _characterStartRotation;
        _currentCharacterSetup.transform.localPosition = _characterStartPosition;
        
        if (_currentCharacterSetup.bgCurve != null)
        {
            _currentCharacterSetup.bgCurve.gameObject.SetActive(true);
        }

        _characterAnimator.enabled = true;
        _currentCharacterSetup.IsStopped = false;
        
        StepManager.Instance.NextStep();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Magnify();
        }
    }

    void OnApplicationQuit()
    {
        StopCoroutine(_expandCoroutine);
        StopCoroutine(_addExtraCoin);
        StopCoroutine(_stopMagnify);
    }

}