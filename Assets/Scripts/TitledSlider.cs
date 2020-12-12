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
    [HideInInspector] public FurniturePlacer furniturePlacer;

    void Start()
    {
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
