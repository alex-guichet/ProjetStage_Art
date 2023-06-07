using System;
using UnityEngine;
using Tobii.Gaming;
using UnityEngine.Serialization;
using Debug = System.Diagnostics.Debug;
using UnityEngine.SceneManagement;

public class EyeTrackingController : Singleton<EyeTrackingController>
{
    public Camera camera; 
    public Camera otherCamera;
    public Collider circleCollider;
    public LayerMask photoLayer;
    public float winningWatchTime;
    public float updateWatchTime;
    public float positionUpdateSpeed = 10.0f;
    public bool isUsingEyeTracker;
    
    public CurrencyName WinningCurrency => _winningCurrency;

    private const float _raycast_distance = 500f;
    private float _watchTimer;
    private float _updateTimer;
    private bool _isInRadius;
    private bool _stopPhotoSelection;
    
    private GameObject _lastTargetedGameObject;
    private RaycastHit[] _hitResults;
    private CurrencyName _winningCurrency;


    private void MoveMedia(int hit_count)
    {
        var average_position = Vector3.zero;  

        for( int index = 0; index < hit_count; index++ )
        {
            average_position += _hitResults[ index ].collider.transform.position;
        }

        average_position /= hit_count;
        
        for( int index = 0; index < hit_count; index++ )
        {
            var direction = _hitResults[ index ].collider.transform.position - average_position;
            _hitResults[ index ].collider.transform.position += direction * (Time.deltaTime * positionUpdateSpeed);
        }
    }

    public void ResumePhotoSelection()
    {
        _stopPhotoSelection = false;
    }

    public void Awake()
    {
        base.Awake();
        _stopPhotoSelection = true;
        _hitResults = new RaycastHit[50];
    }

    private void Update()
    {
        if(_stopPhotoSelection)
            return;

        UserPresence user_presence = TobiiAPI.GetUserPresence();
        Vector3 screen_position = Vector3.zero;

        if (!isUsingEyeTracker)
        {
            screen_position = Input.mousePosition;
        }
        else
        {
            if( TobiiAPI.IsConnected && user_presence.IsUserPresent())
            {
                GazePoint gaze_point = TobiiAPI.GetGazePoint();
                screen_position = gaze_point.Screen - CalibrationManager.Instance.eyeOffset;
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
        
        var hit_count = 0;
        //screen_position = camera.ScreenToWorldPoint( screen_position );
        screen_position.z = circleCollider.transform.position.z;
        var ray = camera.ScreenPointToRay( screen_position );

        if(Physics.Raycast( ray.origin, ray.direction, out RaycastHit hit, _raycast_distance ) && hit.collider == circleCollider )
        {
            ray = otherCamera.ViewportPointToRay( hit.textureCoord );
            
            //Debug.DrawRay( ray.origin, ray.direction * _raycast_distance, Color.blue );
            hit_count = Physics.RaycastNonAlloc( ray, _hitResults, _raycast_distance, photoLayer );

            MoveMedia( hit_count );
            
            for(int index = 0; index < hit_count; index++ )
            {
                var photo = _hitResults[index].collider.GetComponent<IPlayMedia>();
                
                photo?.SetFocusActive();
                Debug.Assert(photo != null, nameof(photo) + " != null");
                
                if( _hitResults[index].collider.gameObject != _lastTargetedGameObject )
                {
                    _watchTimer = 0f;
                }

                _lastTargetedGameObject = _hitResults[index].collider.gameObject;

                if( _updateTimer < updateWatchTime )
                {
                    _updateTimer += Time.deltaTime;
                }
                else
                {
                    _updateTimer = 0f;
                    CurrencyTableManager.Instance.SingleUpdateCurrencyTable(photo.CurrencyName);
                }
                
                if( _watchTimer < winningWatchTime )
                {
                    _watchTimer += Time.deltaTime;
                    continue;
                }

                _winningCurrency = photo.CurrencyName;
                photo.IsWinning = true;
                _stopPhotoSelection = true;
                PhotoPoolManager.Instance.ExpandPhotoAndEvacuate( _hitResults[index].collider.transform );
                SoundManager.Instance.soundEffects[SoundEffectType.MemoryValidation].Play();
            }
        }
        
        if( hit_count == 0 )
        {
            _watchTimer = 0f;
        }
    }
}
