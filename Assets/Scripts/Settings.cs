using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [SerializeField] private GameState state;
    [SerializeField] private TitledSlider sliderPrefab;
    [SerializeField] private Transform settingsContent;
    [SerializeField] private GameObject settingsPanel;

    void Start()
    {
        CreateSliders();
    }

    void CreateSliders()
    {
        foreach (FurnitureCount count in state.counts)
        {
            TitledSlider slider = Instantiate(sliderPrefab, Vector3.zero, Quaternion.identity);
            slider.transform.SetParent(settingsContent);
            // slider.transform.localScale = new Vector3(1f, 1f, 1f);
            slider.title.SetText(count.title);
            slider.count.SetText(count.count.ToString());
            slider.slider.maxValue = count.maxValue;
            slider.slider.SetValueWithoutNotify(count.count);
            slider.onSetCount = count.SetCount;
        }
    }

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeInHierarchy);
    }

}
