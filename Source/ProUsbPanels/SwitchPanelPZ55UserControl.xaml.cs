﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;

namespace ProUsbPanels
{
    /// <summary>
    /// Interaction logic for SwitchPanelPZ55UserControl.xaml
    /// </summary>
    public partial class SwitchPanelPZ55UserControl : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {

        private readonly SwitchPanelPZ55 _switchPanelPZ55;
        private TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private Image[] _imageArrayUpper = new Image[4];
        private Image[] _imageArrayLeft = new Image[4];
        private Image[] _imageArrayRight = new Image[4];
        private IGlobalHandler _globalHandler;

        public SwitchPanelPZ55UserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler){
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _switchPanelPZ55 = new SwitchPanelPZ55(hidSkeleton);

            _switchPanelPZ55.Attach((ISaitekPanelListener)this);
            globalHandler.Attach(_switchPanelPZ55);
            _globalHandler = globalHandler;

            _imageArrayUpper[0] = ImagePZ55LEDDarkUpper;
            _imageArrayUpper[1] = ImagePZ55LEDGreenUpper;
            _imageArrayUpper[2] = ImagePZ55LEDYellowUpper;
            _imageArrayUpper[3] = ImagePZ55LEDRedUpper;

            _imageArrayLeft[0] = ImagePZ55LEDDarkLeft;
            _imageArrayLeft[1] = ImagePZ55LEDGreenLeft;
            _imageArrayLeft[2] = ImagePZ55LEDYellowLeft;
            _imageArrayLeft[3] = ImagePZ55LEDRedLeft;

            _imageArrayRight[0] = ImagePZ55LEDDarkRight;
            _imageArrayRight[1] = ImagePZ55LEDGreenRight;
            _imageArrayRight[2] = ImagePZ55LEDYellowRight;
            _imageArrayRight[3] = ImagePZ55LEDRedRight;
        }

        private void SwitchPanelPZ55UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetContextMenuClickHandlers();
        }

        public SaitekPanel GetSaitekPanel()
        {
            return _switchPanelPZ55;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(DCSAirframe dcsAirframe)
        {
            try
            {
                foreach (var image in Common.FindVisualChildren<Image>(this))
                {
                    if (image.Name.StartsWith("ImagePZ55LED") && dcsAirframe == DCSAirframe.NONE)
                    {
                        image.ContextMenu = null;
                    }
                    else
                        if (image.Name.StartsWith("ImagePZ55LED") && image.ContextMenu == null && dcsAirframe != DCSAirframe.NONE)
                        {
                            image.ContextMenu = (ContextMenu)Resources["PZ55LEDContextMenu"];
                            image.ContextMenu.Tag = image.Name;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471373, ex);
            }
        }

        public void UpdatesHasBeenMissed(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, int count)
        {
            try
            {
                //
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471076, ex);
            }
        }

        public void SwitchesChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, HashSet<object> hashSet)
        {
            try
            {
                if (saitekPanelsEnum == SaitekPanelsEnum.PZ55SwitchPanel && uniqueId.Equals(_switchPanelPZ55.InstanceId))
                {
                    NotifySwitchChanges(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1066, ex);
            }
        }

        public void PanelSettingsReadFromFile(List<string> settings)
        {
            try
            {
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1067, ex);
            }
        }

        public void SettingsCleared(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                ClearAll(false);
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1068, ex);
            }
        }

        public void LedLightChanged(string uniqueId, SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor)
        {
            try
            {
                if (_switchPanelPZ55.InstanceId.Equals(uniqueId))
                {
                    var position = (SwitchPanelPZ55LEDPosition)saitekPanelLEDPosition.Position;
                    var imageArray = _imageArrayUpper;

                    switch (position)
                    {
                        case SwitchPanelPZ55LEDPosition.UP:
                            {
                                HideLedImages(SwitchPanelPZ55LEDPosition.UP);
                                break;
                            }
                        case SwitchPanelPZ55LEDPosition.LEFT:
                            {
                                HideLedImages(SwitchPanelPZ55LEDPosition.LEFT);
                                imageArray = _imageArrayLeft;
                                break;
                            }
                        case SwitchPanelPZ55LEDPosition.RIGHT:
                            {
                                HideLedImages(SwitchPanelPZ55LEDPosition.RIGHT);
                                imageArray = _imageArrayRight;
                                break;
                            }
                    }

                    switch (panelLEDColor)
                    {
                        case PanelLEDColor.DARK:
                            {
                                Dispatcher.BeginInvoke((Action)(() => imageArray[0].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.GREEN:
                            {
                                Dispatcher.BeginInvoke((Action)(() => imageArray[1].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.YELLOW:
                            {
                                Dispatcher.BeginInvoke((Action)(() => imageArray[2].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.RED:
                            {
                                Dispatcher.BeginInvoke((Action)(() => imageArray[3].Visibility = Visibility.Visible));
                                break;
                            }
                    }

                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1069, ex);
            }
        }

        private void HideLedImages(SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition)
        {
            var imageArray = _imageArrayUpper;
            switch (switchPanelPZ55LEDPosition)
            {
                case SwitchPanelPZ55LEDPosition.UP:
                    {

                        break;
                    }
                case SwitchPanelPZ55LEDPosition.LEFT:
                    {
                        imageArray = _imageArrayLeft;
                        break;
                    }
                case SwitchPanelPZ55LEDPosition.RIGHT:
                    {
                        imageArray = _imageArrayRight;
                        break;
                    }
            }
            for (int i = 0; i < 4; i++)
            {
                var image1 = imageArray[i];
                Dispatcher.BeginInvoke((Action)(() => image1.Visibility = Visibility.Collapsed));
            }
        }

        public void PanelDataAvailable(string stringData)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1085, ex);
            }
        }

        public void DeviceAttached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                if (saitekPanelsEnum == SaitekPanelsEnum.PZ55SwitchPanel && uniqueId.Equals(_switchPanelPZ55.InstanceId))
                {
                    //Dispatcher.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (connected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2008, ex);
            }
        }

        public void DeviceDetached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                if (saitekPanelsEnum == SaitekPanelsEnum.PZ55SwitchPanel && uniqueId.Equals(_switchPanelPZ55.InstanceId))
                {
                    //Dispatcher.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (disconnected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2031, ex);
            }
        }

        public void SettingsApplied(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                if (uniqueId.Equals(_switchPanelPZ55.InstanceId) && saitekPanelsEnum == SaitekPanelsEnum.PZ55SwitchPanel)
                {
                    Dispatcher.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher.BeginInvoke((Action)(() => TextBoxLogPZ55.Text = ""));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2032, ex);
            }
        }

        public void PanelSettingsChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                Dispatcher.BeginInvoke((Action)(ShowGraphicConfiguration));
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2010, ex);
            }
        }

        private void MouseDownFocusLogTextBox(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBoxLogPZ55.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2014, ex);
            }
        }

        private void MenuContextEditTextBoxClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                SequenceWindow sequenceWindow;
                if (textBox.Tag is SortedList<int, KeyPressInfo>)
                {
                    sequenceWindow = new SequenceWindow(textBox.Text, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                else
                {
                    sequenceWindow = new SequenceWindow();
                }
                sequenceWindow.ShowDialog();
                if (sequenceWindow.DialogResult.HasValue && sequenceWindow.DialogResult.Value)
                {
                    //Clicked OK
                    //If the user added only a single key stroke combo then let's not treat this as a sequence
                    if (!sequenceWindow.IsDirty)
                    {
                        //User made no changes
                        return;
                    }
                    var sequenceList = sequenceWindow.GetSequence;
                    textBox.ToolTip = null;
                    if (sequenceList.Count > 1)
                    {
                        textBox.Tag = sequenceList;
                        textBox.Text = string.IsNullOrEmpty(sequenceWindow.GetInformation) ? "Key press sequence" : sequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = sequenceWindow.GetInformation };
                            textBox.ToolTip = toolTip;
                        }
                        UpdateKeyBindingProfileSequencedKeyStrokesPZ55(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        textBox.Tag = sequenceList.Values[0].LengthOfKeyPress;
                        textBox.Text = sequenceList.Values[0].VirtualKeyCodesAsString;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = sequenceWindow.GetInformation };
                            textBox.ToolTip = toolTip;
                        }
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

        private void MenuContextEditDCSBIOSControlTextBoxClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                DCSBIOSControlsConfigsWindow dcsBIOSControlsConfigsWindow;
                if (textBox.Tag is List<DCSBIOSInput>)
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), (List<DCSBIOSInput>)textBox.Tag, textBox.Text);
                }
                else
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), null);
                }
                dcsBIOSControlsConfigsWindow.ShowDialog();
                if (dcsBIOSControlsConfigsWindow.DialogResult.HasValue && dcsBIOSControlsConfigsWindow.DialogResult == true && dcsBIOSControlsConfigsWindow.DCSBIOSInputs.Count > 0)
                {
                    var dcsBiosInputs = dcsBIOSControlsConfigsWindow.DCSBIOSInputs;
                    var text = string.IsNullOrWhiteSpace(dcsBIOSControlsConfigsWindow.Description) ? "DCS-BIOS" : dcsBIOSControlsConfigsWindow.Description;
                    //1 appropriate text to textbox
                    //2 update bindings
                    textBox.Text = text;
                    textBox.Tag = dcsBiosInputs;
                    textBox.ToolTip = textBox.Text;
                    UpdateDCSBIOSBinding(textBox);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }



        private void TextBoxContextMenuIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!(bool)e.NewValue)
                {
                    //Do not show if not visible
                    return;
                }

                var textBox = GetTextBoxInFocus();
                var contextMenu = (ContextMenu)sender;
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                if (textBox.Tag == null || textBox.Tag is SortedList<int, KeyPressInfo> || textBox.Tag is List<DCSBIOSInput>)
                {
                    return;
                }
                var keyPressLength = (KeyPressLength)textBox.Tag;

                foreach (MenuItem item in contextMenu.Items)
                {
                    item.IsChecked = false;
                }

                foreach (MenuItem item in contextMenu.Items)
                {
                    /*if (item.Name == "contextMenuItemZero" && keyPressLength == KeyPressLength.Zero)
                    {
                        item.IsChecked = true;
                    }*/
                    if (item.Name == "contextMenuItemFiftyMilliSec" && keyPressLength == KeyPressLength.FiftyMilliSec)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemHalfSecond" && keyPressLength == KeyPressLength.HalfSecond)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemSecond" && keyPressLength == KeyPressLength.Second)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemSecondAndHalf" && keyPressLength == KeyPressLength.SecondAndHalf)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemTwoSeconds" && keyPressLength == KeyPressLength.TwoSeconds)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemThreeSeconds" && keyPressLength == KeyPressLength.ThreeSeconds)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemFourSeconds" && keyPressLength == KeyPressLength.FourSeconds)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemFiveSecs" && keyPressLength == KeyPressLength.FiveSecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemFifteenSecs" && keyPressLength == KeyPressLength.FifteenSecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemTenSecs" && keyPressLength == KeyPressLength.TenSecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemTwentySecs" && keyPressLength == KeyPressLength.TwentySecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemThirtySecs" && keyPressLength == KeyPressLength.ThirtySecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemFortySecs" && keyPressLength == KeyPressLength.FortySecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemSixtySecs" && keyPressLength == KeyPressLength.SixtySecs)
                    {
                        item.IsChecked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2061, ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile)
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                    textBox.Text = "";
                    textBox.Tag = null;
            }
            if (clearAlsoProfile)
            {
                _switchPanelPZ55.ClearSettings();
            }
        }

        private void SetContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (textBox != TextBoxLogPZ55)
                {
                    textBox.ContextMenu = (ContextMenu)Resources["TextBoxContextMenuPZ55"];
                    textBox.ContextMenuOpening += TextBoxContextMenuOpening;
                }
            }
            foreach (var image in Common.FindVisualChildren<Image>(this))
            {
                if (image.Name.StartsWith("ImagePZ55LED"))
                {
                    image.ContextMenu = (ContextMenu)Resources["PZ55LEDContextMenu"];
                    image.ContextMenu.Tag = image.Name;
                }
            }
            //ImagePZ55LED
            //SetWindowState();
        }


        private void ContextConfigureLandingGearLED_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(204165, ex);
            }
        }

        private void ContextConfigureLandingGearLEDClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = (MenuItem)sender;
                var contextMenu = (ContextMenu)menuItem.Parent;
                var imageName = contextMenu.Tag.ToString();
                var position = SwitchPanelPZ55LEDPosition.LEFT;

                if (imageName.Contains("Upper"))
                {
                    position = SwitchPanelPZ55LEDPosition.UP;
                }
                else if (imageName.Contains("Left"))
                {
                    position = SwitchPanelPZ55LEDPosition.LEFT;
                }
                else if (imageName.Contains("Right"))
                {
                    position = SwitchPanelPZ55LEDPosition.RIGHT;
                }
                var ledConfigsWindow = new LEDConfigsWindow(_globalHandler.GetAirframe(), "Set configuration for LED : " + position, new SaitekPanelLEDPosition(position), _switchPanelPZ55.GetLedDcsBiosOutputs(position), _switchPanelPZ55);
                if (ledConfigsWindow.ShowDialog() == true)
                {
                    //must include position because if user has deleted all entries then there is nothing to go after regarding position
                    _switchPanelPZ55.SetLedDcsBiosOutput(position, ledConfigsWindow.ColorOutputBindings);
                }
                SetConfigExistsImageVisibility();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2065, ex);
            }
        }


        private void TextBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                //Timing values
                //Edit sequence
                //Edit DCS-BIOC Control
                var textBox = GetTextBoxInFocus();

                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                var contextMenu = textBox.ContextMenu;

                // 1) If textbox.tag is List<DCSBIOSInput>, show Edit DCS-BIOS Control
                // 2) If textbox.tag is keyvaluepair, show Edit sequence
                // 3) If textbox.tag is null & text is empty && module!=NONE, show Edit sequence & DCS-BIOS Control

                // 4) If textbox has text and tag is not keyvaluepair/DCSBIOSInput, show press times
                // 5) If textbox is not empty, no tag show key press times
                // 6) If textbox is not empty, key press tag show key press times

                //1
                if (textBox.Tag != null && textBox.Tag is List<DCSBIOSInput>)
                {
                    // 1) If textbox.tag is List<DCSBIOSInput>, show Edit DCS-BIOS Control    
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (textBox.Tag != null && textBox.Tag is SortedList<int, KeyPressInfo>)
                {
                    // 2) If textbox.tag is keyvaluepair, show Edit sequence
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (textBox.Tag == null && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // 3) If textbox.tag is null & text is empty, show Edit sequence & DCS-BIOS Control
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (!_switchPanelPZ55.KeyboardEmulationOnly && item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(textBox.Text) && (textBox.Tag == null || (!(textBox.Tag is List<DCSBIOSInput>) && !(textBox.Tag is SortedList<int, KeyPressInfo>))))
                {
                    // 4) If textbox has text and tag is not keyvaluepair/List<DCSBIOSInput>, show press times
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditSequence") || item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(textBox.Text) && (textBox.Tag == null))
                {
                    // 5) If textbox is not empty, no tag show key press times
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditDCSBIOS") || item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }

                // 6) If textbox is not empty, key press tag show key press times
                if ((string.IsNullOrEmpty(textBox.Text) && textBox.Tag != null) && textBox.Tag is KeyPressInfo)
                {
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditDCSBIOS") || item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                /*else
                {
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!item.Name.Contains("Sequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                    }
                }*/

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2081, ex);
            }
        }

        private TextBox GetTextBoxInFocus()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (textBox != TextBoxLogPZ55 && textBox.IsFocused && textBox.Background == Brushes.Yellow)
                {
                    return textBox;
                }
            }
            return null;
        }

        private void TextBoxContextMenuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                var contextMenuItem = (MenuItem)sender;
                /*if(contextMenuItem.Name == "contextMenuItemZero")
                {
                    textBox.Tag = KeyPressLength.Zero;
                }*/
                /*if (contextMenuItem.Name == "contextMenuItemIndefinite")
                {
                    textBox.Tag = KeyPressLength.Indefinite;
                }*/
                if (contextMenuItem.Name == "contextMenuItemFiftyMilliSec")
                {
                    textBox.Tag = KeyPressLength.FiftyMilliSec;
                }
                else if (contextMenuItem.Name == "contextMenuItemHalfSecond")
                {
                    textBox.Tag = KeyPressLength.HalfSecond;
                }
                else if (contextMenuItem.Name == "contextMenuItemSecond")
                {
                    textBox.Tag = KeyPressLength.Second;
                }
                else if (contextMenuItem.Name == "contextMenuItemSecondAndHalf")
                {
                    textBox.Tag = KeyPressLength.SecondAndHalf;
                }
                else if (contextMenuItem.Name == "contextMenuItemTwoSeconds")
                {
                    textBox.Tag = KeyPressLength.TwoSeconds;
                }
                else if (contextMenuItem.Name == "contextMenuItemThreeSeconds")
                {
                    textBox.Tag = KeyPressLength.ThreeSeconds;
                }
                else if (contextMenuItem.Name == "contextMenuItemFourSeconds")
                {
                    textBox.Tag = KeyPressLength.FourSeconds;
                }
                else if (contextMenuItem.Name == "contextMenuItemFiveSecs")
                {
                    textBox.Tag = KeyPressLength.FiveSecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemTenSecs")
                {
                    textBox.Tag = KeyPressLength.TenSecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemFifteenSecs")
                {
                    textBox.Tag = KeyPressLength.FifteenSecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemTwentySecs")
                {
                    textBox.Tag = KeyPressLength.TwentySecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemThirtySecs")
                {
                    textBox.Tag = KeyPressLength.ThirtySecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemFortySecs")
                {
                    textBox.Tag = KeyPressLength.FortySecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemSixtySecs")
                {
                    textBox.Tag = KeyPressLength.SixtySecs;
                }

                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2082, ex);
            }
        }
        private void ImageLEDClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //Only left mouse clicks here!
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    return;
                }
                var imageArray = new Image[4];
                var clickedImage = (Image)sender;
                var position = SwitchPanelPZ55LEDPosition.UP;
                //Name of graphics file
                var imageSource = (string)clickedImage.Tag;

                if (clickedImage.Name.Contains("Upper"))
                {
                    position = SwitchPanelPZ55LEDPosition.UP;
                    imageArray = _imageArrayUpper;
                }
                else if (clickedImage.Name.Contains("Left"))
                {
                    position = SwitchPanelPZ55LEDPosition.LEFT;
                    imageArray = _imageArrayLeft;
                }
                else if (clickedImage.Name.Contains("Right"))
                {
                    position = SwitchPanelPZ55LEDPosition.RIGHT;
                    imageArray = _imageArrayRight;
                }
                var nextImageIndex = 0;
                HideLedImages(position);
                for (int i = 0; i < 4; i++)
                {
                    var image = imageArray[i];
                    if (clickedImage.Equals(image))
                    {
                        nextImageIndex = i + 1;
                        if (nextImageIndex > 3)
                        {
                            nextImageIndex = 0;
                        }
                    }
                }

                imageArray[nextImageIndex].Visibility = Visibility.Visible;


                if (imageArray[nextImageIndex].Name.Contains("Dark"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.DARK);
                }
                else if (imageArray[nextImageIndex].Name.Contains("Green"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.GREEN);
                }
                else if (imageArray[nextImageIndex].Name.Contains("Yellow"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.YELLOW);
                }
                else if (imageArray[nextImageIndex].Name.Contains("Red"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.RED);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2083, ex);
            }
        }
        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;

                if (e.ChangedButton == MouseButton.Left)
                {

                    //Check if this textbox contains DCS-BIOS information. If so then prompt the user for deletion
                    if (textBox.Tag != null && textBox.Tag is List<DCSBIOSInput>)
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS configuration?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.ToolTip = null;
                        textBox.Text = "";
                        _switchPanelPZ55.ClearAllBindings(GetPZ55Key(textBox));
                        textBox.Tag = null;
                    }
                    else if (textBox.Tag != null && textBox.Tag is SortedList<int, KeyPressInfo>)
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Tag = null;
                        textBox.ToolTip = null;
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else
                    {
                        textBox.Tag = null;
                        textBox.ToolTip = null;
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3001, ex);
            }
        }


        private void ButtonClearAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Clear all settings for the Switch Panel?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    ClearAll(true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3003, ex);
            }
        }


        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TextBox)sender).Background = Brushes.Yellow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3004, ex);
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TextBox)sender).Background = Brushes.White;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3005, ex);
            }
        }


        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);

                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (textBox.Tag != null && (textBox.Tag is SortedList<int, KeyPressInfo> || textBox.Tag is List<DCSBIOSInput>))
                {
                    return;
                }
                var hashSetOfKeysPressed = new HashSet<string>();

                if (textBox.Tag == null)
                {
                    textBox.Tag = KeyPressLength.FiftyMilliSec;
                }

                var keyCode = KeyInterop.VirtualKeyFromKey(e.Key);
                e.Handled = true;

                if (keyCode > 0)
                {
                    //Common.DebugP("Pressed key is " + keyCode);
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyCode));
                }
                var modifiers = Common.GetPressedVirtualKeyCodesThatAreModifiers();
                foreach (var virtualKeyCode in modifiers)
                {
                    //Common.DebugP("Pressed modifiers -->  " + virtualKeyCode);
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                }
                var result = "";
                foreach (var str in hashSetOfKeysPressed)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result = str + " + " + result;
                    }
                    else
                    {
                        result = str + " " + result;
                    }
                }
                textBox.Text = result;
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3006, ex);
            }
        }


        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //MAKE SURE THE TAG IS SET BEFORE SETTING TEXT! OTHERWISE THIS DOESN'T FIRE
                var textBox = (TextBox)sender;
                if (textBox.Tag is SortedList<int, KeyPressInfo>)
                {
                    textBox.FontStyle = FontStyles.Oblique;
                }
                else
                {
                    textBox.FontStyle = FontStyles.Normal;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3007, ex);
            }
        }


        private void TextBoxShortcutKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);
                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (textBox.Tag != null && (textBox.Tag is SortedList<int, KeyPressInfo> || textBox.Tag is List<DCSBIOSInput>))
                {
                    return;
                }
                if (textBox.Tag == null)
                {
                    textBox.Tag = KeyPressLength.FiftyMilliSec;
                }
                var keyPressed = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(e.Key);
                e.Handled = true;

                var hashSetOfKeysPressed = new HashSet<string>();
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyPressed));

                var modifiers = Common.GetPressedVirtualKeyCodesThatAreModifiers();
                foreach (var virtualKeyCode in modifiers)
                {
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                }
                var result = "";
                foreach (var str in hashSetOfKeysPressed)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result = str + " + " + result;
                    }
                    else
                    {
                        result = str + " " + result;
                    }
                }
                textBox.Text = result;
                UpdateKeyBindingProfileSequencedKeyStrokesPZ55(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3008, ex);
            }
        }

        private void NotifySwitchChanges(HashSet<object> switches)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher.BeginInvoke((Action)(() => TextBoxLogPZ55.Focus()));
                foreach (var switchPanelKey in switches)
                {
                    var key = (SwitchPanelKey)switchPanelKey;

                    if (_switchPanelPZ55.ForwardKeyPresses)
                    {
                        if (!string.IsNullOrEmpty(_switchPanelPZ55.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogPZ55.Text =
                                 TextBoxLogPZ55.Text.Insert(0, _switchPanelPZ55.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogPZ55.Text =
                             TextBoxLogPZ55.Text = TextBoxLogPZ55.Text.Insert(0, "No action taken, virtual key press disabled.\n")));
                    }
                }
                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogPZ55.Text = TextBoxLogPZ55.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(3009, ex);
            }
        }

        private void SetGraphicsState(HashSet<object> switches)
        {
            try
            {
                foreach (var switchPanelKeyObject in switches)
                {
                    var switchPanelKey = (SwitchPanelKey)switchPanelKeyObject;
                    switch (switchPanelKey.SwitchPanelPZ55Key)
                    {
                        case SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageAvMasterOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //This button is special. The Panel reports the button ON when it us switched upwards towards [CLOSE]. This is confusing semantics.
                                        //The button is considered OFF by the program when it is upwards which is opposite to the other buttons which all are considered ON when upwards.
                                        ImageCowlClosed.Visibility = !key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageDeIceOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageFuelPumpOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.LEVER_GEAR_DOWN:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageGearUp.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.LEVER_GEAR_UP:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageGearUp.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageBeaconOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLandingOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageNavOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImagePanelOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageStrobeOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageTaxiOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageMasterAltOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageMasterBatOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImagePitotHeatOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                    }
                    if (switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH || switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT ||
                        switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT || switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF ||
                        switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START)
                    {
                        var key = switchPanelKey;
                        Dispatcher.BeginInvoke(
                            (Action)delegate
                            {
                                if (key.IsOn)
                                {
                                    ImageKnobAll.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH ? Visibility.Visible : Visibility.Collapsed;
                                    ImageKnobL.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT ? Visibility.Visible : Visibility.Collapsed;
                                    ImageKnobR.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT ? Visibility.Visible : Visibility.Collapsed;
                                    ImageKnobStart.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START ? Visibility.Visible : Visibility.Collapsed;
                                }
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3010, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesPZ55(TextBox textBox)
        {
            try
            {
                if (textBox.Tag == null)
                {
                    textBox.Tag = new SortedList<int, KeyPressInfo>();
                }


                if (textBox.Equals(TextBoxKnobOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxKnobR))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxKnobL))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxKnobAll))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxKnobStart))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.KNOB_ENGINE_START, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxCowlClose))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxCowlOpen))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPanelOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxPanelOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxBeaconOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxBeaconOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxNavOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxNavOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxStrobeOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxStrobeOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxTaxiOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxTaxiOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLandingOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxLandingOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxMasterBatOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxMasterBatOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxMasterAltOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxMasterAltOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxFuelPumpOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxFuelPumpOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxDeIceOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxDeIceOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPitotHeatOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxPitotHeatOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxGearUp))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.LEVER_GEAR_UP, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxGearDown))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }


        private void UpdateKeyBindingProfileSimpleKeyStrokes(TextBox textBox)
        {
            try
            {
                KeyPressLength keyPressLength;
                if (textBox.Tag == null)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((KeyPressLength)textBox.Tag);
                }
                if (textBox.Equals(TextBoxKnobOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, TextBoxKnobOff.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxKnobR))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, TextBoxKnobR.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxKnobL))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, TextBoxKnobL.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxKnobAll))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, TextBoxKnobAll.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxKnobStart))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_START, TextBoxKnobStart.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxCowlClose))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, TextBoxCowlClose.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxCowlOpen))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, TextBoxCowlOpen.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxPanelOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, TextBoxPanelOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxPanelOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, TextBoxPanelOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxBeaconOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, TextBoxBeaconOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxBeaconOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, TextBoxBeaconOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxNavOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, TextBoxNavOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxNavOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, TextBoxNavOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxStrobeOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, TextBoxStrobeOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxStrobeOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, TextBoxStrobeOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxTaxiOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, TextBoxTaxiOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxTaxiOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, TextBoxTaxiOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLandingOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, TextBoxLandingOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxLandingOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, TextBoxLandingOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxMasterBatOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, TextBoxMasterBatOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxMasterBatOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, TextBoxMasterBatOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxMasterAltOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, TextBoxMasterAltOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxMasterAltOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, TextBoxMasterAltOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, TextBoxAvionicsMasterOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, TextBoxAvionicsMasterOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxFuelPumpOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, TextBoxFuelPumpOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxFuelPumpOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, TextBoxFuelPumpOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxDeIceOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, TextBoxDeIceOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxDeIceOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, TextBoxDeIceOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxPitotHeatOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, TextBoxPitotHeatOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxPitotHeatOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, TextBoxPitotHeatOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxGearUp))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.LEVER_GEAR_UP, TextBoxGearUp.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxGearDown))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, TextBoxGearDown.Text, keyPressLength);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
        }

        private void UpdateDCSBIOSBinding(TextBox textBox)
        {
            try
            {
                List<DCSBIOSInput> dcsBiosInputs = null;
                if (textBox.Tag == null)
                {
                    return;
                }
                if (textBox.Tag is List<DCSBIOSInput>)
                {
                    dcsBiosInputs = ((List<DCSBIOSInput>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxKnobOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxKnobR))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxKnobL))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxKnobAll))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxKnobStart))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_START, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxCowlClose))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxCowlOpen))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxPanelOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxPanelOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxBeaconOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxBeaconOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxNavOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxNavOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxStrobeOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxStrobeOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxTaxiOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxTaxiOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxLandingOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxLandingOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxMasterBatOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxMasterBatOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxMasterAltOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxMasterAltOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxFuelPumpOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxFuelPumpOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxDeIceOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxDeIceOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxPitotHeatOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxPitotHeatOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxGearUp))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.LEVER_GEAR_UP, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxGearDown))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, dcsBiosInputs, textBox.Text);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
        }

        private SwitchPanelPZ55KeyOnOff GetPZ55Key(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxKnobOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, true);
                }
                if (textBox.Equals(TextBoxKnobR))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, true);
                }
                if (textBox.Equals(TextBoxKnobL))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, true);
                }
                if (textBox.Equals(TextBoxKnobAll))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, true);
                }
                if (textBox.Equals(TextBoxKnobStart))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_START, true);
                }
                if (textBox.Equals(TextBoxCowlClose))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, false);
                }
                if (textBox.Equals(TextBoxCowlOpen))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, true);
                }
                if (textBox.Equals(TextBoxPanelOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, false);
                }
                if (textBox.Equals(TextBoxPanelOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, true);
                }
                if (textBox.Equals(TextBoxBeaconOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, false);
                }
                if (textBox.Equals(TextBoxBeaconOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, true);
                }
                if (textBox.Equals(TextBoxNavOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, false);
                }
                if (textBox.Equals(TextBoxNavOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, true);
                }
                if (textBox.Equals(TextBoxStrobeOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, false);
                }
                if (textBox.Equals(TextBoxStrobeOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, true);
                }
                if (textBox.Equals(TextBoxTaxiOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, false);
                }
                if (textBox.Equals(TextBoxTaxiOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, true);
                }
                if (textBox.Equals(TextBoxLandingOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, false);
                }
                if (textBox.Equals(TextBoxLandingOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, true);
                }
                if (textBox.Equals(TextBoxMasterBatOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, false);
                }
                if (textBox.Equals(TextBoxMasterBatOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, true);
                }
                if (textBox.Equals(TextBoxMasterAltOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, false);
                }
                if (textBox.Equals(TextBoxMasterAltOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, true);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, false);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, true);
                }
                if (textBox.Equals(TextBoxFuelPumpOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, false);
                }
                if (textBox.Equals(TextBoxFuelPumpOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, true);
                }
                if (textBox.Equals(TextBoxDeIceOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, false);
                }
                if (textBox.Equals(TextBoxDeIceOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, true);
                }
                if (textBox.Equals(TextBoxPitotHeatOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, false);
                }
                if (textBox.Equals(TextBoxPitotHeatOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, true);
                }
                if (textBox.Equals(TextBoxGearUp))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.LEVER_GEAR_UP, true);
                }
                if (textBox.Equals(TextBoxGearDown))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
            throw new Exception("Should not reach this point");
        }

        private void ShowGraphicConfiguration()
        {
            try
            {
                foreach (var keyBinding in _switchPanelPZ55.KeyBindingsHashSet)
                {
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxKnobOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxKnobOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxKnobOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxKnobOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxKnobR.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxKnobR.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxKnobR.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxKnobR.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxKnobL.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxKnobL.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxKnobL.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxKnobL.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxKnobAll.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxKnobAll.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxKnobAll.Text = keyBinding.OSKeyPress.Information;
                                TextBoxKnobAll.Tag = keyBinding.OSKeyPress.GetSequence;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxKnobStart.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxKnobStart.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxKnobStart.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxKnobStart.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }

                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxCowlOpen.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxCowlOpen.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxCowlOpen.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxCowlOpen.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxCowlClose.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxCowlClose.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxCowlClose.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxCowlClose.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPanelOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPanelOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPanelOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPanelOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPanelOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPanelOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPanelOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPanelOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxBeaconOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxBeaconOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxBeaconOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxBeaconOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxBeaconOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxBeaconOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxBeaconOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxBeaconOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxNavOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxNavOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxNavOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxNavOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxNavOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxNavOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxNavOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxNavOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxStrobeOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxStrobeOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxStrobeOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxStrobeOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxStrobeOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxStrobeOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxStrobeOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxStrobeOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxTaxiOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxTaxiOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxTaxiOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxTaxiOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxTaxiOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxTaxiOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxTaxiOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxTaxiOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLandingOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxLandingOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLandingOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxLandingOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLandingOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxLandingOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLandingOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxLandingOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxMasterBatOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxMasterBatOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxMasterBatOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxMasterBatOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxMasterBatOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxMasterBatOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxMasterBatOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxMasterBatOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxMasterAltOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxMasterAltOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxMasterAltOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxMasterAltOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxMasterAltOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxMasterAltOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxMasterAltOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxMasterAltOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAvionicsMasterOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxAvionicsMasterOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAvionicsMasterOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxAvionicsMasterOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAvionicsMasterOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxAvionicsMasterOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAvionicsMasterOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxAvionicsMasterOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxFuelPumpOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxFuelPumpOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxFuelPumpOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxFuelPumpOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxFuelPumpOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxFuelPumpOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxFuelPumpOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxFuelPumpOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxDeIceOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxDeIceOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxDeIceOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxDeIceOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxDeIceOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxDeIceOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxDeIceOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxDeIceOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPitotHeatOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPitotHeatOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPitotHeatOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPitotHeatOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPitotHeatOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPitotHeatOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPitotHeatOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPitotHeatOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_UP)
                    {
                        //When gear is down is it OFF -> NOT BEING USED
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxGearUp.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxGearUp.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxGearUp.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxGearUp.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_DOWN)
                    {
                        //When gear is down is it ON -> BEING USED
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxGearDown.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxGearDown.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxGearDown.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxGearDown.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                }





                foreach (var dcsBiosBinding in _switchPanelPZ55.DCSBiosBindings)
                {
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxKnobOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxKnobOff.Text = dcsBiosBinding.Description;
                        TextBoxKnobOff.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxKnobR.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxKnobR.Text = dcsBiosBinding.Description;
                        TextBoxKnobR.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxKnobL.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxKnobL.Text = dcsBiosBinding.Description;
                        TextBoxKnobL.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxKnobAll.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxKnobAll.Text = dcsBiosBinding.Description;
                        TextBoxKnobAll.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxKnobStart.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxKnobStart.Text = dcsBiosBinding.Description;
                        TextBoxKnobStart.ToolTip = "DCS-BIOS";
                    }

                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxCowlOpen.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxCowlOpen.Text = dcsBiosBinding.Description;
                                TextBoxCowlOpen.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxCowlClose.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxCowlClose.Text = dcsBiosBinding.Description;
                                TextBoxCowlClose.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxPanelOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxPanelOn.Text = dcsBiosBinding.Description;
                                TextBoxPanelOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxPanelOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxPanelOff.Text = dcsBiosBinding.Description;
                                TextBoxPanelOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxBeaconOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxBeaconOn.Text = dcsBiosBinding.Description;
                                TextBoxBeaconOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxBeaconOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxBeaconOff.Text = dcsBiosBinding.Description;
                                TextBoxBeaconOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxNavOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxNavOn.Text = dcsBiosBinding.Description;
                                TextBoxNavOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxNavOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxNavOff.Text = dcsBiosBinding.Description;
                                TextBoxNavOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxStrobeOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxStrobeOn.Text = dcsBiosBinding.Description;
                                TextBoxStrobeOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxStrobeOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxStrobeOff.Text = dcsBiosBinding.Description;
                                TextBoxStrobeOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxTaxiOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxTaxiOn.Text = dcsBiosBinding.Description;
                                TextBoxTaxiOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxTaxiOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxTaxiOff.Text = dcsBiosBinding.Description;
                                TextBoxTaxiOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxLandingOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxLandingOn.Text = dcsBiosBinding.Description;
                                TextBoxLandingOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxLandingOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxLandingOff.Text = dcsBiosBinding.Description;
                                TextBoxLandingOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxMasterBatOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxMasterBatOn.Text = dcsBiosBinding.Description;
                                TextBoxMasterBatOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxMasterBatOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxMasterBatOff.Text = dcsBiosBinding.Description;
                                TextBoxMasterBatOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxMasterAltOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxMasterAltOn.Text = dcsBiosBinding.Description;
                                TextBoxMasterAltOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxMasterAltOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxMasterAltOff.Text = dcsBiosBinding.Description;
                                TextBoxMasterAltOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxAvionicsMasterOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAvionicsMasterOn.Text = dcsBiosBinding.Description;
                                TextBoxAvionicsMasterOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxAvionicsMasterOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAvionicsMasterOff.Text = dcsBiosBinding.Description;
                                TextBoxAvionicsMasterOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxFuelPumpOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxFuelPumpOn.Text = dcsBiosBinding.Description;
                                TextBoxFuelPumpOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxFuelPumpOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxFuelPumpOff.Text = dcsBiosBinding.Description;
                                TextBoxFuelPumpOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxDeIceOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxDeIceOn.Text = dcsBiosBinding.Description;
                                TextBoxDeIceOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxDeIceOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxDeIceOff.Text = dcsBiosBinding.Description;
                                TextBoxDeIceOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxPitotHeatOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxPitotHeatOn.Text = dcsBiosBinding.Description;
                                TextBoxPitotHeatOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxPitotHeatOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxPitotHeatOff.Text = dcsBiosBinding.Description;
                                TextBoxPitotHeatOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_UP && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        //When gear is down is it OFF -> NOT BEING USED
                        TextBoxGearUp.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxGearUp.Text = dcsBiosBinding.Description;
                        TextBoxGearUp.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_DOWN && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        //When gear is down is it ON -> BEING USED
                        TextBoxGearDown.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxGearDown.Text = dcsBiosBinding.Description;
                        TextBoxGearDown.ToolTip = "DCS-BIOS";
                    }
                }

                checkBoxManualLEDs.IsChecked = _switchPanelPZ55.ManualLandingGearLeds;
                SetConfigExistsImageVisibility();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3013, ex);
            }
        }

        private void SetConfigExistsImageVisibility()
        {
            foreach (SwitchPanelPZ55LEDPosition value in Enum.GetValues(typeof(SwitchPanelPZ55LEDPosition)))
            {
                var hasConfiguration = _switchPanelPZ55.LedIsConfigured(value);
                switch (value)
                {
                    case SwitchPanelPZ55LEDPosition.UP:
                        {
                            ImageConfigFoundUpper.Visibility = hasConfiguration ? Visibility.Visible : Visibility.Hidden;
                            break;
                        }
                    case SwitchPanelPZ55LEDPosition.LEFT:
                        {
                            ImageConfigFoundLeft.Visibility = hasConfiguration ? Visibility.Visible : Visibility.Hidden;
                            break;
                        }
                    case SwitchPanelPZ55LEDPosition.RIGHT:
                        {
                            ImageConfigFoundRight.Visibility = hasConfiguration ? Visibility.Visible : Visibility.Hidden;
                            break;
                        }
                }
            }
        }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_switchPanelPZ55 != null)
                {
                    Clipboard.SetText(_switchPanelPZ55.InstanceId);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3015, ex);
            }
        }

        private void ButtonSwitchPanelInfo_OnClick(object sender, RoutedEventArgs e)
        {

            var bytes = Encoding.UTF8.GetBytes(Properties.Resources.PZ55Notes);
            var informationWindow = new InformationWindow(bytes);
            informationWindow.ShowDialog();
        }

        private void CheckBoxManualLEDs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_switchPanelPZ55 != null)
                {
                    _switchPanelPZ55.ManualLandingGearLeds = checkBoxManualLEDs.IsChecked.HasValue && checkBoxManualLEDs.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(30545, ex);
            }
        }
    }
}
