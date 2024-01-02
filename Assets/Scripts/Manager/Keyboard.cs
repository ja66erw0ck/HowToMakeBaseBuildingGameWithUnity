using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TimeManager = Manager.Time;
using KeyboardManager = Manager.Keyboard;

using MappedInputKeyboardModel = Model.Keyboard.MappedInput;
using MappedInputKeyboardType = Type.Keyboard.MappedInput;
using CombinedInputKeyboardType = Type.Keyboard.CombinedInput;

namespace Manager
{
    public class Keyboard
    {
        private static KeyboardManager _instance;
        public static KeyboardManager Instance => _instance ?? new KeyboardManager();

        private readonly Dictionary<string, MappedInputKeyboardModel> _mapping;
        private List<InputField> ModalInputFields { get; set; }

        private Keyboard()
        {
            _instance = this; 
            
            _mapping = new Dictionary<string, MappedInputKeyboardModel>();
            ModalInputFields = new List<InputField>();
            TimeManager.Instance.EveryFrameNotModal += (time) => Update();

            ReadKeyboardMapping();
        }
        
        public void Destroy()
        {
            _instance = null;
        }

        private void ReadKeyboardMapping()
        {
            // mock data for now. Should be converted to something else (json?) 
            RegisterInputMapping("MoveCameraEast", CombinedInputKeyboardType.None, KeyCode.D, KeyCode.RightArrow);
            RegisterInputMapping("MoveCameraWest", CombinedInputKeyboardType.None, KeyCode.A, KeyCode.LeftArrow);
            RegisterInputMapping("MoveCameraNorth", CombinedInputKeyboardType.None, KeyCode.W, KeyCode.UpArrow);
            RegisterInputMapping("MoveCameraSouth", CombinedInputKeyboardType.None, KeyCode.S, KeyCode.DownArrow);

            RegisterInputMapping("ZoomOut", CombinedInputKeyboardType.None, KeyCode.PageUp);
            RegisterInputMapping("ZoomIn", CombinedInputKeyboardType.None, KeyCode.PageDown);

            RegisterInputMapping("MoveCameraUp", CombinedInputKeyboardType.None, KeyCode.Home);
            RegisterInputMapping("MoveCameraDown", CombinedInputKeyboardType.None, KeyCode.End);

            RegisterInputMapping("ApplyCameraPreset1", CombinedInputKeyboardType.None, KeyCode.F1);
            RegisterInputMapping("ApplyCameraPreset2", CombinedInputKeyboardType.None, KeyCode.F2);
            RegisterInputMapping("ApplyCameraPreset3", CombinedInputKeyboardType.None, KeyCode.F3);
            RegisterInputMapping("ApplyCameraPreset4", CombinedInputKeyboardType.None, KeyCode.F4);
            RegisterInputMapping("ApplyCameraPreset5", CombinedInputKeyboardType.None, KeyCode.F5);
            RegisterInputMapping("AssignCameraPreset1", CombinedInputKeyboardType.Control, KeyCode.F1);
            RegisterInputMapping("AssignCameraPreset2", CombinedInputKeyboardType.Control, KeyCode.F2);
            RegisterInputMapping("AssignCameraPreset3", CombinedInputKeyboardType.Control, KeyCode.F3);
            RegisterInputMapping("AssignCameraPreset4", CombinedInputKeyboardType.Control, KeyCode.F4);
            RegisterInputMapping("AssignCameraPreset5", CombinedInputKeyboardType.Control, KeyCode.F5);

            RegisterInputMapping("SetSpeed1", CombinedInputKeyboardType.None, KeyCode.Alpha1, KeyCode.Keypad1);
            RegisterInputMapping("SetSpeed2", CombinedInputKeyboardType.None, KeyCode.Alpha2, KeyCode.Keypad2);
            RegisterInputMapping("SetSpeed3", CombinedInputKeyboardType.None, KeyCode.Alpha3, KeyCode.Keypad3);
            RegisterInputMapping("DecreaseSpeed", CombinedInputKeyboardType.None, KeyCode.Minus, KeyCode.KeypadMinus);
            RegisterInputMapping("IncreaseSpeed", CombinedInputKeyboardType.None, KeyCode.Plus, KeyCode.KeypadPlus);

            RegisterInputMapping("RotateFurnitureLeft", CombinedInputKeyboardType.None, KeyCode.R);
            RegisterInputMapping("RotateFurnitureRight", CombinedInputKeyboardType.None, KeyCode.T);

            RegisterInputMapping("Pause", CombinedInputKeyboardType.None, KeyCode.Space, KeyCode.Pause);
            RegisterInputMapping("Return", CombinedInputKeyboardType.None, KeyCode.Return);

            RegisterInputMapping("Escape", CombinedInputKeyboardType.None, KeyCode.Escape);
            RegisterInputMapping("ToggleCoords", CombinedInputKeyboardType.Control, KeyCode.M);

            // Todo - 레벨 변경??? 월드맵???
            RegisterInputMapping("SetLevel0", CombinedInputKeyboardType.None, KeyCode.Alpha0);
            RegisterInputMapping("SetLevel1", CombinedInputKeyboardType.None, KeyCode.Alpha1);
            RegisterInputMapping("SetLevel2", CombinedInputKeyboardType.None, KeyCode.Alpha2);
            RegisterInputMapping("SetLevel3", CombinedInputKeyboardType.None, KeyCode.Alpha3);
        }

        private void Update()
        {
            if (ModalInputFields.Any(f => f.isFocused)) {
                return;
            }

            foreach (var input in _mapping.Values) {
                input.TriggerActionIfInputValid();
            }
        }

        public void RegisterModalInputField(InputField filterField)
        {
            if (!ModalInputFields.Contains(filterField)) {
                ModalInputFields.Add(filterField);
            }
        }

        public void UnregisterModalInputField(InputField filterField)
        {
            if (ModalInputFields.Contains(filterField)) {
                ModalInputFields.Remove(filterField);
            }
        }

        public void RegisterInputAction(string inputName, MappedInputKeyboardType inputKey, System.Action onTrigger)
        {
            if (_mapping.TryGetValue(inputName, out var mappedInput)) {
                mappedInput.OnTrigger = onTrigger;
                mappedInput.MappedKey = inputKey;
            } else {
                _mapping.Add(
                    inputName,
                    new MappedInputKeyboardModel {
                        InputName = inputName,
                        OnTrigger = onTrigger,
                        MappedKey = inputKey
                    });
            }
        }

        public void UnregisterInputAction(string inputName)
        {
            _mapping.Remove(inputName);
        }

        private void RegisterInputMapping(string inputName, CombinedInputKeyboardType inputCombinedKey, params KeyCode[] keyCodes)
        {
            if (_mapping.TryGetValue(inputName, out var mappedInput)) {
                mappedInput.CombinedKey = inputCombinedKey;
                mappedInput.AddKeyCodes(keyCodes);
            } else {
                _mapping.Add(
                    inputName,
                    new MappedInputKeyboardModel {
                        InputName = inputName,
                        CombinedKey = inputCombinedKey,
                        KeyCodes = keyCodes.ToList()
                    });
            }
        }
    }
}
