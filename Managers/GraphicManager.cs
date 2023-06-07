
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GraphicManager : Singleton<GraphicManager> {

    public RectTransform[] graphicContainers;
    
    private float _yMaximum = 100f;

    public Dictionary<CurrencyName, List<int>> GraphicValueDictionary
    {
        get => _graphicValueDictionary;
        set => _graphicValueDictionary = value;
    }
    
    [HideInInspector]
    public RectTransform _currentGraphicContainer = new();
    private Dictionary<CurrencyName, List<int>> _graphicValueDictionary = new ();

    public void UpdateGraphic(CurrencyName currency_value)
    {
        for (int i = 0; i < graphicContainers.Length; i++)
        {
            var key_value_pair = _graphicValueDictionary.ElementAt(i);
            var value_list = key_value_pair.Value;
            int stock_value;

            for (int y = 1; y < value_list.Count; y++)
            {
                value_list[y - 1] = value_list[y];
            }
            
            _currentGraphicContainer = graphicContainers[i];
            
            if (key_value_pair.Key == currency_value)
            {
                stock_value = value_list[^1] + Random.Range(1, 20);
                CurrencyTableManager.Instance.currencyManagerList[i].stockValue++;
            }
            else
            {
                stock_value = value_list[^1] - Random.Range(1, 20);
                CurrencyTableManager.Instance.currencyManagerList[i].stockValue--;
            } 
            
            value_list[^1] = stock_value;

            if (stock_value > (int)_yMaximum)
            {
                value_list = value_list.Select(x => x - (stock_value - (int)_yMaximum)).ToList();
                stock_value = Convert.ToInt32(_yMaximum);
            }
            
            if (stock_value < 0)
            {
                value_list = value_list.Select(x => x + Mathf.Abs(stock_value)).ToList();
                stock_value = 1;
            }
            
            int nbChildren = _currentGraphicContainer.childCount;
            for (int y = nbChildren - 1; y >= 0; y--) {
                DestroyImmediate(_currentGraphicContainer.GetChild(y).gameObject);
            }
            ShowGraphic(value_list);
        }
    }

    private GameObject CreateCircle(Vector2 anchored_position) {
        GameObject game_object = new GameObject("AnchorPoint", typeof(RectTransform));
        game_object.transform.SetParent(_currentGraphicContainer, false);
        RectTransform rect_transform = game_object.GetComponent<RectTransform>();
        rect_transform.anchoredPosition = anchored_position;
        rect_transform.sizeDelta = new Vector2(11, 11);
        rect_transform.anchorMin = new Vector2(0, 0);
        rect_transform.anchorMax = new Vector2(0, 0);
        return game_object;
    }

    public void ShowGraphic(List<int> value_list) {
        var size_delta = _currentGraphicContainer.sizeDelta;
        int value_list_count = value_list.Count;
        float graph_height = size_delta.y;
        float x_size = size_delta.x / value_list_count;
        Color graphic_color = Color.green;
        if (value_list[^1] < value_list[^2])
        {
            graphic_color = Color.red;
        }

        GameObject last_circle_game_object = null;
        for (int i = 0; i < value_list_count; i++) {
            float x_position = i * x_size;
            float y_temp_position = (value_list[i] / _yMaximum) * graph_height;
            float y_position = Mathf.Clamp(y_temp_position, 0, graph_height);
            GameObject circle_game_object = CreateCircle(new Vector2(x_position, y_position));
            if (last_circle_game_object != null) {
                CreateDotConnection(last_circle_game_object.GetComponent<RectTransform>().anchoredPosition, circle_game_object.GetComponent<RectTransform>().anchoredPosition, graphic_color);
            }
            last_circle_game_object = circle_game_object;
        }
    }

    private void CreateDotConnection(Vector2 dot_position_A, Vector2 dot_position_B, Color color) {
        GameObject game_object = new GameObject("dotConnection", typeof(Image));
        game_object.transform.SetParent(_currentGraphicContainer, false);
        game_object.GetComponent<Image>().color = color;
        RectTransform rect_transform = game_object.GetComponent<RectTransform>();
        Vector2 direction = (dot_position_B - dot_position_A).normalized;
        float distance = Vector2.Distance(dot_position_A, dot_position_B);
        rect_transform.anchorMin = new Vector2(0, 0);
        rect_transform.anchorMax = new Vector2(0, 0);
        rect_transform.sizeDelta = new Vector2(distance, 1f);
        rect_transform.anchoredPosition = dot_position_A + direction * (distance * 0.5f);
        rect_transform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(direction));
    }
    
    private static float GetAngleFromVectorFloat(Vector3 direction) {
        direction = direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        return angle;
    }
}
