using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PhotoPoolManager : Singleton<PhotoPoolManager>
{
    [Header("Photo cloud pool settings")]
    [Tooltip("Pool of photo to be organized")]
    public Transform photoPool;
    [Tooltip("Collider of the calibration sphere")]
    public SphereCollider calibrationSphereCollider;
    [Tooltip("Speed of the photo move")]
    public float photoMoveSpeed = 0.1f;
    [Tooltip("Random scale of the photo")]
    public Vector2 photoScale;
    

    [Header("Photo expansion settings")]
    [Tooltip("Circle A object")]
    public Transform circleATransform;
    [Tooltip("Time it takes for the photo to reach it's max size")]
    public float expansionTime;
    [Tooltip("Multiplier applied on the photo to get it's max size")]
    public float photoMaxSizeMultiplier;
    [Tooltip("Photo collider for calibration for expansion")]
    public BoxCollider calibrationCollider;
    [Tooltip("Expansion Animation Curve")]
    public AnimationCurve expansionCurve;
    
    [Header("Photo Vacuum settings")]
    [Tooltip("Evacuation time of the photo")]
    public float photoEvacuationTime;
    [Tooltip("Opening and closing time of the hole")]
    public float holeOpeningTime;
    [Tooltip("Scale of the opened pool hole")]
    public Vector3 openedHoleScale;
    [Tooltip("Transform of the photo pool hole")]
    public Transform photoPoolHole;
    [Tooltip("Vacuum Material")]
    public Material vacuumMaterial;
    [Tooltip("Black Hole Position")]
    public Vector3 blackHolePosition;
    [Tooltip("Vacuum Animation Curve")]
    public AnimationCurve vacuumCurve;


    private float _calibrationPhotoMagnitude;
    private float _moveTimer;
    private float _moveTime = 4f;

    private Coroutine _expandCoroutine;
    private Coroutine _evacuateCoroutine;
    private static readonly int BlackHole = Shader.PropertyToID("_Black_hole");
    private static readonly int vacuumEffect = Shader.PropertyToID("_Effect");

    public void CreatePhotoCloud()
    {
        List<Vector3> photo_positions = new();
        float circle_size = calibrationSphereCollider.bounds.extents.magnitude/2f;
        
        for (var i = 0; i < photoPool.childCount; i++)
        {
            Transform photo_transform = photoPool.GetChild(i).transform;
            Vector3 photo_position = photo_transform.position + (Vector3)Random.insideUnitCircle * circle_size;
            int try_attempt_count = 0;

            while (photo_positions.FindIndex(x => Vector3.Distance(x, photo_position) < 0.55f) != -1)
            {
                photo_position = photo_transform.position + (Vector3)Random.insideUnitCircle * circle_size;
                try_attempt_count++;
                
                if (try_attempt_count > 30)
                {
                    circle_size += 0.1f;
                }
            }

            photo_positions.Add(photo_position);
            photo_transform.position = photo_position;
            float scale = Random.Range(photoScale.x, photoScale.y);
            photo_transform.GetComponent<IPlayMedia>().InitialScale = photo_transform.localScale;
            photo_transform.localScale *= scale;
            photo_transform.Rotate(Vector3.forward, Random.Range(-30f, 30f));

            SpriteRenderer photo_sprite_renderer = photo_transform.GetComponent<SpriteRenderer>();
            Color temp_color = photo_sprite_renderer.color;
            temp_color.a = scale/photoScale.y;
            photo_sprite_renderer.color = temp_color;
        }
    }
    
    public void MovePhotoCloud()
    {
        _moveTimer += Time.deltaTime;
        
        if (_moveTimer > _moveTime)
        {
            _moveTimer = 0f;
            photoMoveSpeed = -photoMoveSpeed;
            _moveTime = Random.Range(3f, 5f);
        }
        
        for (var i = 0; i < photoPool.childCount; i++)
        {
            Transform photo_transform = photoPool.GetChild(i).transform;
            if (photo_transform.GetComponent<IPlayMedia>().IsWinning)
            {
                continue;
            }
            
            if (i % 2 == 0)
            {
                photo_transform.position += photo_transform.right * (photoMoveSpeed * Time.deltaTime);
            }
            else
            {
                photo_transform.position -= photo_transform.right * (photoMoveSpeed * Time.deltaTime);
            }
            
        }
    }
    
    public void ExpandPhotoAndEvacuate(Transform photo_transform)
    {
        /*
        //first solution
        Vector3 start_position = photo_transform.position;
        Vector3 end_position = transform.position;
        */
        
        //second solution to be approved by the artist
        Vector3 start_position = photo_transform.position;
        Vector3 end_position = circleATransform.position;
        end_position.z -= 2f;
        start_position.z = end_position.z;
        photo_transform.position = start_position;

        SoundManager.Instance.soundEffects[SoundEffectType.MemoryZoom].Play();
        
        var local_scale = photo_transform.localScale;
        Vector3 start_scale = local_scale;
        Vector3 end_scale = photo_transform.GetComponent<IPlayMedia>().InitialScale * photoMaxSizeMultiplier;
        
        Quaternion start_rotation = photo_transform.rotation;
        Quaternion end_rotation = Quaternion.Euler(0f,0f,0f);
        photo_transform.GetComponent<Collider>().enabled = false;
        
        SpriteRenderer photo_sprite_renderer = photo_transform.GetComponent<SpriteRenderer>();
        photo_sprite_renderer.sortingOrder += 10;
        Color temp_color = photo_sprite_renderer.color;
        float total_remaining_alpha = 1f - temp_color.a;
        float increment_alpha = total_remaining_alpha / expansionTime;

        if (_expandCoroutine != null)
        {
            StopCoroutine(_expandCoroutine);
        }
        _expandCoroutine = StartCoroutine(Expand());
        
        IEnumerator Expand()
        {
            float expansion_timer = 0f;
            while (expansion_timer < expansionTime)
            {
                float interpolation = expansionCurve.Evaluate(expansion_timer / expansionTime);
                photo_transform.localScale = Vector3.Lerp(start_scale, end_scale, interpolation);
                photo_transform.position = Vector3.Lerp(start_position, end_position, interpolation);
                photo_transform.rotation = Quaternion.Slerp(start_rotation, end_rotation, interpolation);
                
                Color temp_color = photo_sprite_renderer.color;
                temp_color.a += increment_alpha * Time.deltaTime;
                photo_sprite_renderer.color = temp_color;
                
                expansion_timer += Time.deltaTime;    
                yield return new WaitForEndOfFrame();
            }
            photo_transform.localScale = end_scale;
            
            if (_evacuateCoroutine != null)
            {
                StopCoroutine(_evacuateCoroutine);
            }
            _evacuateCoroutine = StartCoroutine(Evacuate());
        }
        
        IEnumerator Evacuate()
        {
            SoundManager.Instance.soundEffects[SoundEffectType.VacuumEffect].Play();
            
            Vector3 init_hole_scale = photoPoolHole.localScale;
            Material black_hole_material = new Material(vacuumMaterial);

            Vector3 transform_position = photo_transform.position;
            blackHolePosition.x = transform_position.x;
            blackHolePosition.y += transform_position.y;
            blackHolePosition.z += transform_position.z;
            
            //black_hole_material.SetVector(BlackHole, blackHolePosition);
            photo_transform.GetComponent<Renderer>().material = black_hole_material;
            float start_vacuum_effect = black_hole_material.GetFloat(vacuumEffect);
            float end_vacuum_effect = start_vacuum_effect + 0.21f;
            
            float hole_opening_timer = 0f;

            while (hole_opening_timer < holeOpeningTime)
            {
                photoPoolHole.localScale = Vector3.Lerp(init_hole_scale, openedHoleScale, hole_opening_timer/holeOpeningTime);
                hole_opening_timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            photoPoolHole.localScale = openedHoleScale;

            float photo_evacuation_timer = 0f;
                
            while (photo_evacuation_timer < photoEvacuationTime)
            {
                float interpolation = photo_evacuation_timer / photoEvacuationTime;
                black_hole_material.SetFloat(vacuumEffect, Mathf.Lerp(start_vacuum_effect, end_vacuum_effect, vacuumCurve.Evaluate(interpolation)));
                photo_evacuation_timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            while (hole_opening_timer > 0)
            {
                photoPoolHole.localScale = Vector3.Lerp(init_hole_scale, openedHoleScale, hole_opening_timer/holeOpeningTime);
                hole_opening_timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            photoPoolHole.localScale = init_hole_scale;
            photoPoolHole.gameObject.SetActive(false);
            photo_transform.gameObject.SetActive(false);
            StepManager.Instance.NextStep();
        }
    }
    
    

    public void InitializePhotoCurrency()
    {
        int currency_index = 0;
        foreach (Transform photo in photoPool)
        {
            var currency_manager_list = CurrencyTableManager.Instance.currencyManagerList;
            photo.GetComponent<IPlayMedia>().CurrencyName = currency_manager_list[currency_index].currencyObject.currencyName;
            currency_index++;

            if (currency_index >= currency_manager_list.Count)
            {
                currency_index = 0;
            }
        }
    }

    public override void Awake()
    {
        base.Awake();
        _calibrationPhotoMagnitude = calibrationCollider.bounds.extents.magnitude;
        CreatePhotoCloud();
    }
    
    public void Update()
    {
        MovePhotoCloud();
    }
    

    void OnApplicationQuit()
    {
        StopCoroutine(_expandCoroutine);
        StopCoroutine(_evacuateCoroutine);
    }
}
