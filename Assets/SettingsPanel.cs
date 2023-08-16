using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public TMPro.TMP_Dropdown backgroundDropdown;
    public Toggle cheatmodeToggle;

    public void Awake() {
        SetBackgroundOptions();
        SetCheatOption();
    }

    public void SetBackgroundOptions() {
        backgroundDropdown.options.Clear();
        int bgCount = GameManager.instance.GetBackgroundCount();
        List<TMPro.TMP_Dropdown.OptionData> optionList = new List<TMPro.TMP_Dropdown.OptionData>();
        for (int x = 0; x < bgCount; x++) {
            TMPro.TMP_Dropdown.OptionData data = new TMPro.TMP_Dropdown.OptionData();

            Sprite s = GameManager.instance.GetBackgroundByIndex(x);
            data.text = s.name;
            optionList.Add(data);
        }
        backgroundDropdown.AddOptions(optionList);
        int currentSetting = 0;
        if (PlayerPrefs.HasKey("background")) {
            currentSetting = PlayerPrefs.GetInt("background");
        }
        backgroundDropdown.SetValueWithoutNotify(currentSetting);
        UpdateBackground(currentSetting);
    }

    public void UpdateBackground(int index) {
        GameManager.instance.SetBackground(index);
        PlayerPrefs.SetInt("background", index);
    }

    public void OnDropdownSelected(int choice) {
        UpdateBackground(choice);
    }

    public void SetCheatOption() {
        bool currentSetting = false;
        if (PlayerPrefs.HasKey("cheatmode")) {
            int temp = PlayerPrefs.GetInt("cheatmode");
            currentSetting = temp == 1;
        }
        cheatmodeToggle.SetIsOnWithoutNotify(currentSetting);
        UpdateCheat(currentSetting);
    }

    public void UpdateCheat(bool state) {
        GameManager.instance.cheatMode = state;

        int val = 0;
        if (state)
            val = 1;

        PlayerPrefs.SetInt("cheatmode", val);
    }

    public void OnToggleSelected(bool state) {
        UpdateCheat(state);
    }
}
