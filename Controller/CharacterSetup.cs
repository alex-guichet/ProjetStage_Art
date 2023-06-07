using System;
using System.Collections;
using System.Collections.Generic;
using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
using UnityEngine;
using UnityEngine.Rendering;

public enum RouteType
{
    RotateAround,
    PingPong,
    StartOver
}

public class CharacterSetup : MonoBehaviour
{
    [Header("General Settings")]
    public RouteType routeType;
    public float moveSpeed;
    public bool isGoingBackward;

    [Header("Magnifying Glass Settings")] 
    public Vector3 cameraOffset;
    public float projectionSize;

    [Header("Rotate Around Settings")]
    public Transform target;

    [Header("Start Over and Ping-Pong Settings")]
    public BGCurve bgCurve;
    
    [Header("Lens Flare Settings")]
    public LensFlareComponentSRP lensFlare;
    public Vector2[] shutOffIntervals;
    public float switchLightTime = 1f;
    
    private BGCcCursorObjectRotate _objectRotate;
    private BGCcCursor _objectCursor;
    private float _distanceRatio;
    private float _initialIntensity;
    private bool _isSwitchedOff;
    private Coroutine _switchLightCoroutine;

    public bool IsStopped {
        get;
        set;
    }

    private IEnumerator SwitchLight(Vector2 interval)
    {
        float start_intensity = _initialIntensity;
        float end_intensity = 0f;

        float switch_light_timer = 0f;
        while (switch_light_timer < switchLightTime)
        {
            switch_light_timer += Time.deltaTime;
            lensFlare.intensity = Mathf.Lerp(start_intensity, end_intensity, switch_light_timer / switchLightTime);
            yield return null;
        }
        lensFlare.intensity = end_intensity;

        while (_objectCursor.DistanceRatio < interval.y)
        {
            yield return null;
        }

        start_intensity = 0f;
        end_intensity = _initialIntensity;
        
        switch_light_timer = 0f;
        while (switch_light_timer < switchLightTime)
        {
            switch_light_timer += Time.deltaTime;
            lensFlare.intensity = Mathf.Lerp(start_intensity, end_intensity, switch_light_timer / switchLightTime);
            yield return null;
        }
        lensFlare.intensity = end_intensity;
        _isSwitchedOff = false;
    }

    void Awake()
    {
        if (lensFlare != null)
        {
            _initialIntensity = lensFlare.intensity;
        }
            
        if (bgCurve == null)
            return;
        
        _objectCursor = bgCurve.GetComponent<BGCcCursor>();
        _objectRotate = bgCurve.GetComponent<BGCcCursorObjectRotate>();
    }
    
    void Update()
    {
        if (IsStopped)
            return;

        if (lensFlare != null && !_isSwitchedOff)
        {
            foreach (var interval in shutOffIntervals)
            {
                if (_objectCursor.DistanceRatio > interval.x && _objectCursor.DistanceRatio < interval.y)
                {
                    _isSwitchedOff = true;
                    if (_switchLightCoroutine != null)
                    {
                        StopCoroutine(_switchLightCoroutine);
                    }
                    _switchLightCoroutine = StartCoroutine(SwitchLight(interval));
                    break;
                }
            }
            
        }
        
        switch (routeType)
        {
            case RouteType.RotateAround:
                transform.RotateAround(target.transform.position, Vector3.back, moveSpeed * Time.deltaTime);
                break;
            case RouteType.PingPong:
                if (_objectCursor.DistanceRatio < 1 && !isGoingBackward)
                {
                    _objectCursor.DistanceRatio += Time.deltaTime * moveSpeed;
                }
                else
                {
                    if (!isGoingBackward)
                    {
                        isGoingBackward = true;
                        _objectRotate.OffsetAngle = new Vector3(0, 180, 0);
                    }
                }
        
                if (_objectCursor.DistanceRatio > 0 && isGoingBackward)
                {
                    _objectCursor.DistanceRatio -= Time.deltaTime * moveSpeed;
                }
                else
                {
                    if (isGoingBackward)
                    {
                        isGoingBackward = false;
                        _objectRotate.OffsetAngle = new Vector3(0, 0, 0);
                    }
                }
                break;
            case RouteType.StartOver:
                _distanceRatio = (_objectCursor.DistanceRatio + Time.deltaTime * moveSpeed) % 1f;
                _objectCursor.DistanceRatio = _distanceRatio;

                if (_distanceRatio < 0f && isGoingBackward)
                {
                    _distanceRatio = 1f;
                    _objectCursor.DistanceRatio = _distanceRatio;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
