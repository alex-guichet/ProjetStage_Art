using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using Tobii.Gaming;
using UnityEngine.Rendering;
using UnityEngine.UI;

public enum Language
{
    French,
    English,
    Dutch
}

public class LanguageSelectionManager : Singleton<LanguageSelectionManager>
{
    public float watchTime;
    public Image[] image_button_list;
    
    [ColorUsage(true, true)]
    public Color startLightBulbColor;
    [ColorUsage(true, true)]
    public Color endLightBulbColor;

    private const float _raycast_distance = 500f;
    private float _watchTimer;
    private Camera _camera;
    private Color _resetColorButton;
    private bool _isSelected;
    private bool _isSoundPlaying;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private bool _hasSelectedMesh;
    private MeshRenderer[] lightBulbMeshRenderers;

    public void SelectFrenchLanguage()
    {
        //Switch language to French
        SwitchLanguage(Language.French);
    }
    
    public void SelectEnglishLanguage()
    {
        //Switch language to English
        SwitchLanguage(Language.English);
    }
    
    public void SelectDutchLanguage()
    {
        //Switch language to Dutch
        SwitchLanguage(Language.Dutch);
    }

    public void SwitchLanguage(Language language)
    {
        Debug.Log(language);
        //Switch I2loc languages
        LocalizationManager.CurrentLanguage = language.ToString();
        StepManager.Instance.NextStep();
    }

    public override void Awake()
    {
        base.Awake();
        _camera = Camera.main;
        _resetColorButton = image_button_list[0].color;
    }
    
    private void Update()
    {

        if (_isSelected)
            return;
        
        UserPresence user_presence = TobiiAPI.GetUserPresence();
        Vector3 screen_position = Vector3.zero;
        
        if( TobiiAPI.IsConnected && user_presence.IsUserPresent() )
        {
            GazePoint gaze_point = TobiiAPI.GetGazePoint();

            screen_position = gaze_point.Screen;
        }
        else
        {
            screen_position = Input.mousePosition;
        }
        
        screen_position.z = _raycast_distance;
        var ray = _camera.ScreenPointToRay( screen_position );
        
        Debug.DrawRay(ray.origin, ray.direction * 500f, Color.blue);

        if(Physics.Raycast( ray.origin, ray.direction, out RaycastHit hit, _raycast_distance ))
        {
            Button button = hit.collider.GetComponent<Button>();
            
            if (!button)
                return;

            if (!_isSoundPlaying)
            {
                _isSoundPlaying = true;
                SoundManager.Instance.soundEffects[SoundEffectType.LanguageLoading].Play();
            }
            
            if (!_hasSelectedMesh)
            {
                lightBulbMeshRenderers = hit.collider.GetComponentsInChildren<MeshRenderer>();
                _hasSelectedMesh = true;
            }

            foreach (var mesh_renderer in lightBulbMeshRenderers)
            {
                mesh_renderer.material.SetColor(EmissionColor, Vector4.Lerp(startLightBulbColor, endLightBulbColor, _watchTimer/watchTime));
            }

            if (_watchTimer < watchTime)
            {
                _watchTimer += Time.deltaTime;
                return;
            }
            button.onClick?.Invoke();
            _isSelected = true;
            _watchTimer = 0f;
            SoundManager.Instance.soundEffects[SoundEffectType.LanguageValidation].Play();
            SoundManager.Instance.soundEffects[SoundEffectType.LanguageLoading].Stop();
        }
        else
        {
            if (_isSoundPlaying)
            {
                _isSoundPlaying = false;
                SoundManager.Instance.soundEffects[SoundEffectType.LanguageLoading].Stop();
            }

            if (_hasSelectedMesh)
            {
                foreach (var mesh_renderer in lightBulbMeshRenderers)
                {
                    mesh_renderer.material.SetColor(EmissionColor, startLightBulbColor);
                }
                _hasSelectedMesh = false;
            }
            
            _watchTimer = 0f;
        }
    }
}
