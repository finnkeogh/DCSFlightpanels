﻿using System;
using System.Windows;
using System.Windows.Controls;
using CommonClassLibraryJD;
using DCS_BIOS;
using NonVisuals;

namespace ProUsbPanels
{
    /// <summary>
    /// Interaction logic for ChooseProfileModuleWindow.xaml
    /// </summary>
    public partial class ChooseProfileModuleWindow : Window
    {
        private DCSAirframe _dcsAirframe = DCSAirframe.A10C;

        public ChooseProfileModuleWindow()
        {
            InitializeComponent();
        }

        private void ChooseProfileModuleWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            PopulateAirframeCombobox();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetAirframe();
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(23060, ex);
            }
        }


        private void PopulateAirframeCombobox()
        {
            if (!IsLoaded)
            {
                return;
            }
            ComboBoxAirframe.SelectionChanged -= ComboBoxAirframe_OnSelectionChanged;
            ComboBoxAirframe.Items.Clear();
            foreach (DCSAirframe airframe in Enum.GetValues(typeof(DCSAirframe)))
            {
                if (airframe != DCSAirframe.NOFRAMELOADEDYET)
                {
                    ComboBoxAirframe.Items.Add(EnumEx.GetDescription(airframe));
                }
            }
            ComboBoxAirframe.SelectedIndex = 0;
            ComboBoxAirframe.SelectionChanged += ComboBoxAirframe_OnSelectionChanged;
        }

        private void SetAirframe()
        {
            if (IsLoaded && ComboBoxAirframe.SelectedItem != null)
            {
                _dcsAirframe = EnumEx.GetValueFromDescription<DCSAirframe>(ComboBoxAirframe.SelectedItem.ToString());
                DCSBIOSControlLocator.Airframe = _dcsAirframe;
            }
        }

        private void ComboBoxAirframe_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetAirframe();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2060, ex);
            }
        }

        public DCSAirframe DCSAirframe
        {
            get { return _dcsAirframe; }
        }
    }
}
