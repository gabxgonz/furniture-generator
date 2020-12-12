using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public delegate void OnSetCount(int count);

public class TitledSlider : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI count;
    public Slider slider;
    public OnSetCount onSetCount;


    void Start()
    {
        slider.onValueChanged.AddListener(delegate { SetCount(); });
    }

    void Update()
    {
    }

    private void SetCount()
    {
        onSetCount((int)slider.value);
        count.SetText(slider.value.ToString());
    }
}
