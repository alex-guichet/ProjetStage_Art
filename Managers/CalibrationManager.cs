using System;
using System.Collections;
using System.Collections.Generic;
using Tobii.Gaming;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class CalibrationManager : Singleton<CalibrationManager>
{
    public Vector2 eyeOffset{ get; private set; }
    
    public TextMeshProUGUI instructionText;
    public RectTransform gazePanel;
    public Canvas canvas;
    public float calibrationTime = 5.0f;
    public float acceptableOffset = 50.0f;
    public bool isUsingEyeTracker;

    private List<Vector2> _offsetArray = new List<Vector2>(50);
    private float _calibrationTimer;
    private bool _isCalibrationEnded;

    private void EndCalibration()
    {
        StepManager.Instance.NextStep();
    }
    
    private void Update()
    {
        if( StepManager.Instance.currentStep != GameStep.Calibration )
        {
            return;
        }
        
        UserPresence user_presence = TobiiAPI.GetUserPresence();
        GazePoint gaze_point = TobiiAPI.GetGazePoint();

        if (!isUsingEyeTracker)
        {
            if( Input.GetButtonDown( "Jump" ) && !_isCalibrationEnded)
            {
                _isCalibrationEnded = true;
                EndCalibration();
            }

            return;
        }
        
        if (TobiiAPI.IsConnected && user_presence.IsUserPresent())
        {
            if (gaze_point.IsRecent() && gaze_point.IsValid)
            {
                var target_position = canvas.renderingDisplaySize / 2.0f;

                _offsetArray.Add( gaze_point.Screen - target_position);

                while( _offsetArray.Count > 50 )
                {
                    _offsetArray.RemoveAt( 0 );
                }
                
                Vector2 offset_average = Vector2.zero;
                
                foreach( Vector2 offset in _offsetArray )
                {
                    offset_average += offset;
                }

                offset_average /= _offsetArray.Count;
                
                // Note: origin point is bottom left. Can be negative if user is looking outside of screen.
                gazePanel.position = gaze_point.Screen - offset_average;
                instructionText.text = $"Fixez le carré central pendant {calibrationTime} secondes\n{offset_average}";

                if( offset_average.magnitude < acceptableOffset )
                {
                    _calibrationTimer += Time.deltaTime;

                    if( _calibrationTimer >= calibrationTime )
                    {
                        eyeOffset = offset_average;
                        EndCalibration();
                    }
                }
                else
                {
                    instructionText.text = $"Please check the initial calibration in the Tobii Experience software";
                    _calibrationTimer = 0.0f;
                }
            }
        }
        else
        {
            instructionText.text = "Aucun utilisateur devant la barre ou la barre n'est pas connectée";
            SceneManager.LoadScene(0);
        }
    }
}
