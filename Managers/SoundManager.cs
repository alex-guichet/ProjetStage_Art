using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundEffectType
{
    LanguageLoading,
    LanguageValidation,
    WorldAppearance,
    MemoryValidation,
    MemoryZoom,
    VacuumEffect,
    BinaryConversion,
    CurrencyHighlight,
    AttentionTimeIncrease,
    CharacterZoom,
    CoinAddition,
    NotificationBubble,
    GlitchEffect,
    BinaryVacuum,
}

public class SoundManager : Singleton<SoundManager>
{
    public List<SoundEffect> soundEffectList;
    public Dictionary<SoundEffectType, AudioSource> soundEffects => _soundEffects;
    
    private Dictionary<SoundEffectType, AudioSource> _soundEffects = new();
    
    void Awake()
    {
        base.Awake();
        foreach (var sound in soundEffectList)
        {
            _soundEffects.TryAdd(sound.soundType, sound.audioSource);
        }
    }
}

[System.Serializable]
public struct SoundEffect
{
    public SoundEffectType soundType;
    public AudioSource audioSource;
}


