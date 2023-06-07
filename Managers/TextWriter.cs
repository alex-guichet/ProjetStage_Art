using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


public class TextWriter : MonoBehaviour
{
    public float timeBetweenInvoke;
    public UnityEvent onEndWriting;

    public bool HasStartedWriting => _hasStartedWriting;

    private float _timePerCharacter;
    private float _timerPerCharacter;
    private TextMeshProUGUI _textUI;
    private string _textToWrite;
    private int _characterIndex;
    
    private bool _hasStartedWriting;

    public void StartWriting()
    {
        _hasStartedWriting = true;
    }

    private IEnumerator WaitBeforeInvoke()
    {
        yield return new WaitForSeconds(timeBetweenInvoke);
        onEndWriting?.Invoke();
    }

    void Awake()
    {
        _textUI = GetComponent<TextMeshProUGUI>();
    }
    
    void Start()
    {
        _timePerCharacter = ZoneFManager.Instance.timePerCharacter;
        _timerPerCharacter = _timePerCharacter;
        _textToWrite = _textUI.text;
        _textUI.text = "";
    }

    private void Update()
    {
        if (!_hasStartedWriting)
            return;
        
        if (_characterIndex < _textToWrite.Length) {
            _timerPerCharacter += Time.deltaTime;
            if (_timerPerCharacter >= _timePerCharacter)
            {
                _timerPerCharacter = 0f;
                _characterIndex++;
                string text = _textToWrite.Substring(0, _characterIndex);
                _textUI.text = text;
            }
        }
        else
        {
            _hasStartedWriting = false;
            StartCoroutine(WaitBeforeInvoke());
        }
    }
    
}
