using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class ZoneFManager : Singleton<ZoneFManager>
{

    [Header("General Settings")]
    public float timePerCharacter = 0.4f;
    
    [Header("Panel Intro settings")]
    public List<CurrencyScale> characterScaleList;
    public RectTransform characterPlaceHolder;
    
    [Header("Notifications settings")]
    public List<Transform> bubblesTransformList;
    public float expansionTime = 2f;
    
    private Coroutine _bubbleExpansionCoroutine;
    private Coroutine _bubbleReductionCoroutine;
    private TextWriter _lastTextWriter;

    public void InitializeIntroPanel(CurrencyName character_currency)
    {
        int index = characterScaleList.FindIndex(x => x.currencyName == character_currency);
        characterPlaceHolder.localScale = characterScaleList[index].scalePlaceHolder;

        float y_position = characterScaleList[index].yPositionOverride;
        
        if (y_position != 0)
        {
            Vector3 local_position = characterPlaceHolder.localPosition;
            local_position.y = y_position;
            characterPlaceHolder.localPosition = local_position;
        }
        
        GameObject character = Instantiate(characterScaleList[index].characterPrefab, characterPlaceHolder);
        character.GetComponentInChildren<Animator>().enabled = false;
        character.GetComponentInChildren<SkinnedMeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        
        LayerMask layer_UI = LayerMask.NameToLayer("UI");
        character.layer = layer_UI;
        var children_transform = character.GetComponentsInChildren<Transform>();
        foreach(var child_transform in children_transform )
        {
            child_transform.gameObject.layer = layer_UI;
        }
    }


    private IEnumerator ExpandBubblesCoroutine()
    {
        foreach( var bubble in bubblesTransformList)
        {
            if (_lastTextWriter != null)
            {
                while (_lastTextWriter.HasStartedWriting)
                {
                    yield return null;
                }
            }
            
            SoundManager.Instance.soundEffects[SoundEffectType.NotificationBubble].Play();
            bubble.gameObject.SetActive(true);
            Vector3 end_scale = bubble.localScale;

            float expansion_timer = 0f;
            while (expansion_timer <= expansionTime)
            {
                var interpolation = expansion_timer / expansionTime;
                bubble.localScale = Vector3.Lerp(Vector3.zero, end_scale, interpolation);
                expansion_timer += Time.deltaTime;
                yield return null;
            }
            _lastTextWriter = bubble.GetComponentInChildren<TextWriter>();
            _lastTextWriter.StartWriting();
        }
        StepManager.Instance.NextStep();
    }

    private IEnumerator ReduceBubblesCoroutine()
    {
        List<Vector3> start_scale_list = new();

        foreach (var bubble in bubblesTransformList)
        {
            start_scale_list.Add(bubble.localScale);
        }

        float expansion_timer = 0f;
        while (expansion_timer <= expansionTime)
        {
            var interpolation = expansion_timer / expansionTime;
            for ( int i = 0; i < bubblesTransformList.Count; i++)
            {
                Transform bubble = bubblesTransformList[i];
                bubble.localScale = Vector3.Lerp(start_scale_list[i], Vector3.zero, interpolation);
            }
            expansion_timer += Time.deltaTime;
            yield return null;
        }
        
        foreach (var bubble in bubblesTransformList)
        {
            bubble.gameObject.SetActive(false);
        }
        
        StepManager.Instance.NextStep();
    }

    public void ExpandBubbles()
    {
        if (_bubbleExpansionCoroutine != null)
        {
            StopCoroutine(_bubbleExpansionCoroutine);
        }
        _bubbleExpansionCoroutine = StartCoroutine(ExpandBubblesCoroutine());
    }
    

    public void ReduceBubbles()
    {
        if (_bubbleReductionCoroutine != null)
        {
            StopCoroutine(_bubbleReductionCoroutine);
        }
        _bubbleReductionCoroutine = StartCoroutine(ReduceBubblesCoroutine());
    }

    private void OnApplicationQuit()
    {
        StopCoroutine(_bubbleExpansionCoroutine);
        StopCoroutine(_bubbleReductionCoroutine);
    }
}

[System.Serializable]
public struct CurrencyScale
{
    public CurrencyName currencyName;
    public Vector3 scalePlaceHolder;
    public GameObject characterPrefab;
    public float yPositionOverride;
}