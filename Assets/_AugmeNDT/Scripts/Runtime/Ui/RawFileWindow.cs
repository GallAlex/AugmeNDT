using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MixedReality.Toolkit.UX;

namespace AugmeNDT{
    

    /// <summary>
    /// Class reads Values of the RawFileWindow Inputs
    /// </summary>
    public class RawFileWindow : MonoBehaviour
    {
        [SerializeField] 
        private GameObject window;

        [SerializeField]
        private MRTKUGUIInputField xDimField;
        [SerializeField]
        private MRTKUGUIInputField yDimField;
        [SerializeField]
        private MRTKUGUIInputField zDimField;
        [SerializeField]
        private MRTKUGUIInputField xSpacingField;
        [SerializeField]
        private MRTKUGUIInputField ySpacingField;
        [SerializeField]
        private MRTKUGUIInputField zSpacingField;
        [SerializeField]
        private MRTKUGUIInputField bytesToSkipField;
        [SerializeField]
        private TMP_Dropdown dataFormatDropdown;
        [SerializeField]
        private TMP_Dropdown endiannessDropdown;

        [SerializeField]
        private Button cancelButton;
        [SerializeField]
        private Button importButton;

        private TaskCompletionSource<bool> windowInputFinished = new TaskCompletionSource<bool>();

        // Start is called before the first frame update
        void Start()
        {
            //Fill Dropdowns
            PopulateDropDownWithEnum(dataFormatDropdown, new DataContentFormat());
            PopulateDropDownWithEnum(endiannessDropdown, new Endianness());

            //cancelButton.onClick.AddListener(OnCancleButtonClick);
            //importButton.onClick.AddListener(OnImportButtonClick);
        }

        private static void PopulateDropDownWithEnum(TMP_Dropdown dropdown, Enum targetEnum)
        {
            Type enumType = targetEnum.GetType();
            List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();

            dropdown.ClearOptions();//Clear old options

            for (int i = 0; i < Enum.GetNames(enumType).Length; i++)//Populate new Options
            {
                newOptions.Add(new TMP_Dropdown.OptionData(Enum.GetName(enumType, i)));
            }

            dropdown.AddOptions(newOptions);//Add new options
        }

        public int XDim
        {
            get => int.Parse(xDimField.text);
            set => xDimField.text = value.ToString();
        }

        public int YDim
        {
            get => int.Parse(yDimField.text);
            set => yDimField.text = value.ToString();
        }

        public int ZDim
        {
            get => int.Parse(zDimField.text);
            set => zDimField.text = value.ToString();
        }

        public int XSpacing
        {
            get => int.Parse(xSpacingField.text);
            set => xSpacingField.text = value.ToString();
        }
        public int YSpacing
        {
            get => int.Parse(ySpacingField.text);
            set => ySpacingField.text = value.ToString();
        }
        public int ZSpacing
        {
            get => int.Parse(zSpacingField.text);
            set => zSpacingField.text = value.ToString();
        }

        public int BytesToSkip
        {
            get => int.Parse(bytesToSkipField.text);
            set => bytesToSkipField.text = value.ToString();
        }

        public DataContentFormat DataFormat
        {
            get => (DataContentFormat)dataFormatDropdown.value;
            set => dataFormatDropdown.value = (int)value;
        }

        public Endianness Endianness
        {
            get => (Endianness)endiannessDropdown.value;
            set => endiannessDropdown.value = (int)value;
        }

        /// <summary>
        /// Method waits until input is submitted via button
        /// </summary>
        /// <returns>If Import button was clicked or not</returns>
        public async Task<bool> WaitForInput()
        {
            return await windowInputFinished.Task;
        }

        public void OnCancelButtonClick()
        {
            windowInputFinished.SetResult(false);
        }

        public void OnImportButtonClick()
        {
            windowInputFinished.SetResult(true);
        }

    }
}
