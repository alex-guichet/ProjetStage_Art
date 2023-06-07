using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CircleAManager : Singleton<CircleAManager>
{
    public Animator waterBackGroundAnimator;
    public MeshRenderer waterBackGround;
    public Transform photoCylinder;
    public Vector3 endScale;
    public float fadeTime = 3f;

    private Coroutine _fadeExpandCoroutine;
    
    public void FadeBackGround()
    {
        //waterBackGroundAnimator.SetTrigger("FadeIn");
        if (_fadeExpandCoroutine != null)
        {
            StopCoroutine(_fadeExpandCoroutine);
        }
        _fadeExpandCoroutine = StartCoroutine(FadeAndExpand());
    }

    private IEnumerator FadeAndExpand()
    {
        float fade_timer = 0f;
        Vector3 start_scale = photoCylinder.localScale;
        while (fade_timer < fadeTime)
        {
            float interpolation = fade_timer / fadeTime;
            photoCylinder.localScale = Vector3.Lerp(start_scale, endScale, interpolation);
            waterBackGround.material.SetFloat("_Opacity", Mathf.Lerp(1f,0f,interpolation));
            fade_timer += Time.deltaTime;
            yield return null;
        }

        photoCylinder.localScale = endScale;
        waterBackGround.gameObject.SetActive(false);
        EyeTrackingController.Instance.ResumePhotoSelection();
        DisplayFlowManager.Instance.StartWriting();
    }
}
