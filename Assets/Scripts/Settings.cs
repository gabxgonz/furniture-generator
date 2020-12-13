using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [SerializeField] private GameState state;
    [SerializeField] private FurniturePlacer furniturePlacer;

    [Header("Toggleable Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Sliders")]
    [SerializeField] private TitledSlider beds;
    [SerializeField] private TitledSlider chairs;
    [SerializeField] private TitledSlider couches;
    [SerializeField] private TitledSlider decorations;
    [SerializeField] private TitledSlider doors;
    [SerializeField] private TitledSlider kitchen;
    [SerializeField] private TitledSlider lamps;
    [SerializeField] private TitledSlider length;
    [SerializeField] private TitledSlider tables;
    [SerializeField] private TitledSlider width;

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
        InitSlider(length, state.dimensions["length"]);
        InitSlider(width, state.dimensions["width"]);
        InitSlider(doors, state.furnitureValues["doors"]);
        InitSlider(kitchen, state.furnitureValues["kitchen"]);
        InitSlider(beds, state.furnitureValues["beds"]);
        InitSlider(couches, state.furnitureValues["couches"]);
        InitSlider(tables, state.furnitureValues["tables"]);
        InitSlider(chairs, state.furnitureValues["chairs"]);
        InitSlider(lamps, state.furnitureValues["lamps"]);
        InitSlider(decorations, state.decorationValue);
    }

    private void InitSlider(TitledSlider slider, FurnitureValue value)
    {
        slider.title.SetText(value.title);
        slider.value.SetText(value.value.ToString());
        slider.slider.SetValueWithoutNotify(value.value);
        slider.slider.minValue = value.minValue;
        slider.slider.maxValue = value.maxValue;
        slider.onSetValue = value.SetValue;
        slider.furniturePlacer = furniturePlacer;
    }
}
