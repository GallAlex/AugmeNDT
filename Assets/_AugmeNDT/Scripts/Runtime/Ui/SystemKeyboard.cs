using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace AugmeNDT{
    [RequireComponent(typeof(TMP_InputField))]
    public class SystemKeyboard : MonoBehaviour
    {
        private TMP_InputField _inputField = null;
        private Process _keyboard;


        private void Start()
        {
            _inputField = GetComponent<TMP_InputField>();
            if (_inputField != null)
            {
                _inputField.onSelect.AddListener(onInputSelect);
                _inputField.onDeselect.AddListener(onInputDeselect);
            }
            else
            {
                UnityEngine.Debug.LogError("Please add the TMP_InputField component to the object", this);
            }
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return)) closeKeyboard(); // If enter key pressed, close keyboard
        }


        private void closeKeyboard()
        {
            if (_keyboard != null)
            {
                _keyboard.Kill();
                _keyboard = null;
            }
        }


        private void launchKeyboard()
        {
            if (_keyboard == null) _keyboard = Process.Start("osk.exe");
        }


        private void onInputSelect(string pSelectionEvent)
        {
            launchKeyboard();
        }


        private void onInputDeselect(string pSelectionEvent)
        {
            closeKeyboard();
        }


        private void OnDestroy()
        {
            closeKeyboard();
            _inputField.onSelect.RemoveListener(onInputSelect);
            _inputField.onDeselect.RemoveListener(onInputDeselect);
        }
    }
}