using System;
using UnityEngine;

namespace SettingsLogic
{
    public static class UserSettingsService
    {
        private static UserPreferencesData _saveData;
        private const string PREFS_KEY = "UserPreferences_SaveData";

        public static readonly ReactiveProperty<float> FieldOfView = new ReactiveProperty<float>();
        public static readonly ReactiveProperty<float> MouseSensitivity = new ReactiveProperty<float>();
        public static readonly ReactiveProperty<float> MasterVolume = new ReactiveProperty<float>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            LoadFromDisk();
            BindReactiveProperties();
            Application.quitting += SaveToDisk;
        }

        private static void LoadFromDisk()
        {
            if (PlayerPrefs.HasKey(PREFS_KEY))
            {
                string json = PlayerPrefs.GetString(PREFS_KEY);
                _saveData = JsonUtility.FromJson<UserPreferencesData>(json);
            }
            else
            {
                _saveData = new UserPreferencesData();
            }
        }

        private static void BindReactiveProperties()
        {
            FieldOfView.Value = _saveData.fieldOfView;
            MouseSensitivity.Value = _saveData.mouseSensitivity;
            MasterVolume.Value = _saveData.masterVolume;

            FieldOfView.OnValueChanged += (val) => _saveData.fieldOfView = val;
            MouseSensitivity.OnValueChanged += (val) => _saveData.mouseSensitivity = val;
            MasterVolume.OnValueChanged += (val) => _saveData.masterVolume = val;
        }

        public static void SaveToDisk()
        {
            string json = JsonUtility.ToJson(_saveData);
            PlayerPrefs.SetString(PREFS_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("[UserSettings] Ayarlar diske kaydedildi.");
        }
    }
}