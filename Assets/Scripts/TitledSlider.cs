using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public delegate void OnSetValue(int count);

public class TitledSlider : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI value;
    public Slider slider;
    public OnSetValue onSetValue;
    public string titleCopy = "Slider";
    public int startValue = 1;
    public int sliderMin = 0;
    public int sliderMax = 10;
    [HideInInspector] public FurniturePlacer furniturePlacer;

    void Start()
    {
        slider.value = startValue;
        title.SetText(titleCopy);
        value.SetText(startValue.ToString());
        slider.SetValueWithoutNotify(startValue);
        slider.minValue = sliderMin;
        slider.maxValue = sliderMax;
        slider.maxValue = sliderMax;
        slider.onValueChanged.AddListener(delegate { SetValue(); });
    }

    private void SetValue()
    {
        onSetValue((int)slider.value);
        value.SetText(slider.value.ToString());

        if (furniturePlacer == null) return;
        furniturePlacer.RearrangeFurniture();
        furniturePlacer.PlaySound();
    }
}
