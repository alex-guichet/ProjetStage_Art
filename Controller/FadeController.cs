using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class FadeController : Singleton<FadeController>
{
    public UnityEvent onEndFade;
    public MeshRenderer meshRenderer;
    public float fadeTime = 4f;
    public LensFlareComponentSRP[] lensFlares;

    private Coroutine _fadeLensFlareCoroutine;
    
    public void InvokeFadeEvents()
    {
        onEndFade?.Invoke();
    }

    public void FadeLensFlares()
    {
        if (_fadeLensFlareCoroutine != null)
        {
            StopCoroutine(_fadeLensFlareCoroutine);
        }
        _fadeLensFlareCoroutine = StartCoroutine(FadeLensFlaresCoroutine());
    }

    private IEnumerator FadeLensFlaresCoroutine()
    {
        float fade_timer = 0f;
        float end_intensity = lensFlares[0].intensity;
        
        while (fade_timer < fadeTime)
        {
            foreach (var lens_flare in lensFlares)
            {
                lens_flare.intensity = Mathf.Lerp(0f, end_intensity, fade_timer / fadeTime);
            }
            fade_timer += Time.deltaTime;
            yield return null;
        }
        
        foreach (var lens_flare in lensFlares)
        {
            lens_flare.intensity =  end_intensity;
        }
    }

    public override void Awake()
    {
        base.Awake();
        meshRenderer.enabled = true;
    }
}
