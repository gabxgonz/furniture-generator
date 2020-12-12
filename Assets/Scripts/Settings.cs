using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [SerializeField] private GameState state;
    [SerializeField] private TitledSlider sliderPrefab;
    [SerializeField] private Transform settingsContent;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private FurniturePlacer furniturePlacer;

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeInHierarchy);
    }

    protected void Start()
    {
        CreateSliders();
    }

    private void CreateSliders()
    {
        foreach (FurnitureValue value in state.dimensions.Values) InitSlider(value);
        foreach (FurnitureValue value in state.values) InitSlider(value);
    }

    private void InitSlider(FurnitureValue value)
    {
        TitledSlider slider = Instantiate(sliderPrefab, Vector3.zero, Quaternion.identity);
        slider.transform.SetParent(settingsContent);
        slider.title.SetText(value.title);
        slider.value.SetText(value.value.ToString());
        slider.slider.minValue = value.minValue;
        slider.slider.maxValue = value.maxValue;
        slider.slider.SetValueWithoutNotify(value.value);
        slider.onSetValue = value.SetValue;
        slider.furniturePlacer = furniturePlacer;
    }
}
