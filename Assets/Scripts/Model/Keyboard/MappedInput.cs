using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MappedInputKeyboardType = Type.Keyboard.MappedInput;
using CombinedInputKeyboardType = Type.Keyboard.CombinedInput;

namespace Model.Keyboard
{
    public class MappedInput
    {
        public string InputName { get; set; }
        public List<KeyCode> KeyCodes { get; set; }
        public MappedInputKeyboardType MappedKey { get; set; }
        public CombinedInputKeyboardType CombinedKey { get; set; }
        public System.Action OnTrigger { get; set; }
        
        public MappedInput()
        {
            KeyCodes = new List<KeyCode>();
        }

        public void AddKeyCodes(IEnumerable<KeyCode> keycodes)
        {
            foreach (var keycode in keycodes) {
                if (!KeyCodes.Contains(keycode)) {
                    KeyCodes.Add(keycode);
                }
            }
        }

        public void TriggerActionIfInputValid()
        {
            if (UserUsedInputThisFrame()) {
                OnTrigger?.Invoke();
            }
        }

        private bool UserUsedInputThisFrame()
        {
            if (CombinedTypeActive()) {
                return MappedKey switch {
                    MappedInputKeyboardType.Key => GetKey(),
                    MappedInputKeyboardType.KeyUp => GetKeyUp(),
                    MappedInputKeyboardType.KeyDown => GetKeyDown(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return false;
        }

        private bool CombinedTypeActive()
        {
            var currentlyPressed = CombinedInputKeyboardType.None;

            if (GetCombinedKey(CombinedInputKeyboardType.Shift)) {
                currentlyPressed |= CombinedInputKeyboardType.Shift;
            }

            if (GetCombinedKey(CombinedInputKeyboardType.Control)) {
                currentlyPressed |= CombinedInputKeyboardType.Control;
            }

            if (GetCombinedKey(CombinedInputKeyboardType.Alt)) {
                currentlyPressed |= CombinedInputKeyboardType.Alt;
            }

            return currentlyPressed == CombinedKey;
        }

        private static bool GetCombinedKey(CombinedInputKeyboardType combined)
        {
            return combined switch {
                CombinedInputKeyboardType.None => !(GetCombinedKey(CombinedInputKeyboardType.Shift) || GetCombinedKey(CombinedInputKeyboardType.Control) || GetCombinedKey(CombinedInputKeyboardType.Alt)),
                CombinedInputKeyboardType.Shift => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
                CombinedInputKeyboardType.Control => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl),
                CombinedInputKeyboardType.Alt => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt),
                _ => false
            };
        }

        private bool GetKey()
        {
            return KeyCodes.Any(Input.GetKey);
        }

        private bool GetKeyUp()
        {
            return KeyCodes.Any(Input.GetKeyUp);
        }

        private bool GetKeyDown()
        {
            return KeyCodes.Any(Input.GetKeyUp);
        }
    }
}
