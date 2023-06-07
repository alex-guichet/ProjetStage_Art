using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStep
{
    LanguageSelection,
    Calibration,
    Appearing,
    CircleA,
    CircleB,
    CurrencyTable,
    Magnify,
    Step6Text,
    ShowNotifications,
    StopMagnify,
    ReduceNotifications,
    Step7Text,
    GlitchEffect,
    Restart
}

public class StepManager : Singleton<StepManager>
{
    public GameStep currentStep
    {
        get => _currentStep;

        private set
        {
            _oldStep = _currentStep;
            _currentStep = value;

            if (_waitBeforeChangeCoroutine != null)
            {
                StopCoroutine(_waitBeforeChangeCoroutine);
            }
            _waitBeforeChangeCoroutine = StartCoroutine(WaitBeforeStepChange());
        }
    }

    private GameStep _oldStep;
    private GameStep _currentStep;

    public event StepChangedHandler StepChanged;
    public delegate void StepChangedHandler(GameStep old_step, GameStep new_step);

    private Coroutine _waitBeforeChangeCoroutine;

    public void NextStep()
    {
        var int_step = ( int )currentStep;
        int_step++;
        currentStep = ( GameStep )int_step;
    }

    IEnumerator WaitBeforeStepChange()
    {
        float seconds = 0;

        var flow_timer_manager = DisplayFlowManager.Instance.flowTimerManager;
        int index = flow_timer_manager.FindIndex(x => x.gameStep == _currentStep);
        if (index != -1)
        {
            seconds = flow_timer_manager[index].TimeBeforeExecution;
        }
        
        yield return new WaitForSeconds(seconds);
        StepChanged?.Invoke(_oldStep, _currentStep);
    }

    public override void Awake()
    {
        base.Awake();
        
        _currentStep = GameStep.LanguageSelection;
    }

    private void OnApplicationQuit()
    {
        StopCoroutine(_waitBeforeChangeCoroutine);
    }
}


[System.Serializable]
public struct StepFlow
{
    public GameStep gameStep;
    public float TimeBeforeExecution;
}
