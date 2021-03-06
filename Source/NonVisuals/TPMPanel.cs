﻿using System;
using System.Collections.Generic;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{

    public class TPMPanel : SaitekPanel
    {

        /*
         * For a switch the TPM can have :
         * - single key binding
         * - seqenced key binding
         * - DCS-BIOS control
         */
        private HashSet<DCSBIOSBindingTPM> _dcsBiosBindings = new HashSet<DCSBIOSBindingTPM>();
        private HashSet<KeyBindingTPM> _keyBindings = new HashSet<KeyBindingTPM>();
        private HashSet<TPMPanelSwitch> _tpmPanelSwitches = new HashSet<TPMPanelSwitch>();
        private bool _isFirstNotification = true;
        private byte[] _oldTPMPanelValue = { 0, 0, 0, 0, 0 };
        private byte[] _newTPMPanelValue = { 0, 0, 0, 0, 0 };
        //private HidDevice _hidReadDevice;
        private object _dcsBiosDataReceivedLock = new object();

        public TPMPanel(HIDSkeleton hidSkeleton) : base(SaitekPanelsEnum.TPM, hidSkeleton)
        {
            //Fixed values
            VendorId = 0x6A3;
            ProductId = 0xB4D;
            CreateSwitchKeys();
            Startup();
        }

        public override sealed void Startup()
        {
            try
            {


                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP("TPMPanel.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Closed = true;
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        public override void ImportSettings(List<string> settings)
        {
            //Clear current bindings
            ClearSettings();
            if (settings == null || settings.Count == 0)
            {
                return;
            }
            foreach (var setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2 && setting.Contains(InstanceId))
                {
                    if (setting.StartsWith("TPMPanelSwitch{"))
                    {
                        var keyBinding = new KeyBindingTPM();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("TPMPanelDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingTPM = new DCSBIOSBindingTPM();
                        dcsBIOSBindingTPM.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBindingTPM);
                    }
                }
            }
            OnSettingsApplied();
        }

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }
            var result = new List<string>();

            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null)
                {
                    result.Add(keyBinding.ExportSettings());
                }
            }
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                {
                    result.Add(dcsBiosBinding.ExportSettings());
                }
            }
            return result;
        }

        public override void SavePanelSettings(ProfileHandler panelProfileHandler)
        {
            panelProfileHandler.RegisterProfileData(this, ExportSettings());
        }
        
        public override void DcsBiosDataReceived(uint address, uint data)
        {
            //Common.DebugP("TPM READ ENTERING");
            lock (_dcsBiosDataReceivedLock)
            {
                UpdateCounter(address, data);
            }
            //Common.DebugP("TPM READ EXITING");
        }
        

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            return null;
        }

        public override void ClearSettings()
        {
            _keyBindings.Clear();
            _dcsBiosBindings.Clear();
        }

        public HashSet<KeyBindingTPM> KeyBindingsHashSet
        {
            get { return _keyBindings; }
            set { _keyBindings = value; }
        }

        private void TPMSwitchChanged(TPMPanelSwitch tpmPanelSwitch)
        {
            if (!ForwardKeyPresses)
            {
                return;
            }
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && keyBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                {
                    keyBinding.OSKeyPress.Execute();
                }
            }
        }

        private void TPMSwitchChanged(IEnumerable<object> hashSet)
        {
            if (!ForwardKeyPresses)
            {
                return;
            }
            foreach (var tpmPanelSwitchObject in hashSet)
            {
                var tpmPanelSwitch = (TPMPanelSwitch)tpmPanelSwitchObject;
                var found = false;
                foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.OSKeyPress != null && keyBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && keyBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                    {
                        keyBinding.OSKeyPress.Execute();
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && dcsBiosBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands();
                            break;
                        }
                    }
                }
            }
        }

        public string GetKeyPressForLoggingPurposes(TPMPanelSwitch tpmPanelSwitch)
        {
            var result = "";
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null && keyBinding.TPMSwitch == tpmPanelSwitch.TPMSwitch && keyBinding.WhenTurnedOn == tpmPanelSwitch.IsOn)
                {
                    result = keyBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }
            return result;
        }

        public void AddOrUpdateSingleKeyBinding(TPMPanelSwitches tpmPanelSwitch, string keys, KeyPressLength keyPressLength, bool whenTurnedOn = true)
        {
            if (string.IsNullOrEmpty(keys))
            {
                var tmp = new TPMPanelSwitch.TPMPanelSwitchOnOff(tpmPanelSwitch, whenTurnedOn);
                ClearAllBindings(tmp);
                return;
            }
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitch && keyBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (string.IsNullOrEmpty(keys))
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        keyBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                        keyBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                }
            }
            if (!found && !string.IsNullOrEmpty(keys))
            {
                var keyBinding = new KeyBindingTPM();
                keyBinding.TPMSwitch = tpmPanelSwitch;
                keyBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            Common.DebugP("TPMPanel _keyBindings : " + _keyBindings.Count);
            IsDirtyMethod();
        }

        public void ClearAllBindings(TPMPanelSwitch.TPMPanelSwitchOnOff tpmPanelSwitchOnOff)
        {
            //This must accept lists
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitchOnOff.TPMSwitch && keyBinding.WhenTurnedOn == tpmPanelSwitchOnOff.On)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.TPMSwitch == tpmPanelSwitchOnOff.TPMSwitch && dcsBiosBinding.WhenTurnedOn == tpmPanelSwitchOnOff.On)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                }
            }
            Common.DebugP("TPMPanel _keyBindings : " + _keyBindings.Count);
            Common.DebugP("TPMPanel _dcsBiosBindings : " + _dcsBiosBindings.Count);
            IsDirtyMethod();
        }

        public void AddOrUpdateSequencedKeyBinding(string information, TPMPanelSwitches tpmPanelSwitch, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            RemoveTPMPanelSwitchFromList(2, tpmPanelSwitch, whenTurnedOn);
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitch && keyBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (sortedList.Count == 0)
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        keyBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                        keyBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                    break;
                }
            }
            if (!found && sortedList.Count > 0)
            {
                var keyBinding = new KeyBindingTPM();
                keyBinding.TPMSwitch = tpmPanelSwitch;
                keyBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            IsDirtyMethod();
        }

        public void AddOrUpdateDCSBIOSBinding(TPMPanelSwitches tpmPanelSwitch, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn = true)
        {
            //!!!!!!!
            //If all DCS-BIOS commands has been deleted then provide a empty list, not null object!!!

            //This must accept lists
            var found = false;
            RemoveTPMPanelSwitchFromList(1, tpmPanelSwitch, whenTurnedOn);
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.TPMSwitch == tpmPanelSwitch && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                {
                    dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                    dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                    dcsBiosBinding.Description = description;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var dcsBiosBinding = new DCSBIOSBindingTPM();
                dcsBiosBinding.TPMSwitch = tpmPanelSwitch;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            IsDirtyMethod();
        }

        private void RemoveTPMPanelSwitchFromList(int list, TPMPanelSwitches tpmPanelSwitch, bool whenTurnedOn = true)
        {
            switch (list)
            {
                case 1:
                    {
                        foreach (var keyBinding in _keyBindings)
                        {
                            if (keyBinding.TPMSwitch == tpmPanelSwitch && keyBinding.WhenTurnedOn == whenTurnedOn)
                            {
                                keyBinding.OSKeyPress = null;
                            }
                            break;
                        }
                        break;
                    }
                case 2:
                    {
                        foreach (var dcsBiosBinding in _dcsBiosBindings)
                        {
                            if (dcsBiosBinding.TPMSwitch == tpmPanelSwitch && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                            {
                                dcsBiosBinding.DCSBIOSInputs.Clear();
                            }
                            break;
                        }
                        break;
                    }
            }
        }

        private void IsDirtyMethod()
        {
            OnSettingsChanged();
            IsDirty = true;
        }

        public void Clear(TPMPanelSwitches tpmPanelSwitch)
        {
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.TPMSwitch == tpmPanelSwitch)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            IsDirtyMethod();
        }

        private void OnReport(HidReport report)
        {
            //if (IsAttached == false) { return; }

            if (report.Data.Length == 5)
            {
                Array.Copy(_newTPMPanelValue, _oldTPMPanelValue, 5);
                Array.Copy(report.Data, _newTPMPanelValue, 5);
                var hashSet = GetHashSetOfChangedSwitches(_oldTPMPanelValue, _newTPMPanelValue);
                TPMSwitchChanged(hashSet);
                OnSwitchesChanged(hashSet);
                _isFirstNotification = false;
                /*if (Common.Debug)
                {
                    var stringBuilder = new StringBuilder();
                    for (var i = 0; i < report.Data.Length; i++)
                    {
                        stringBuilder.Append(report.Data[i] + " ");
                    }
                    Common.DebugP(stringBuilder.ToString());
                    if (hashSet.Count > 0)
                    {
                        Common.DebugP("\nFollowing switches has been changed:\n");
                        foreach (var tpmPanelSwitch in hashSet)
                        {
                            Common.DebugP(((TPMPanelSwitch)tpmPanelSwitch).TPMSwitch + ", value is " + FlagValue(_newTPMPanelValue, ((TPMPanelSwitch)tpmPanelSwitch)));
                        }
                    }
                }*/
                Common.DebugP("\r\nDone!\r\n");
            }
            try
            {
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    Common.DebugP("Adding callback " + TypeOfSaitekPanel + " " + GuidString);
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void DeviceAttachedHandler()
        {
            Startup();
            OnDeviceAttached();
        }

        private void DeviceRemovedHandler()
        {
            Shutdown();
            OnDeviceDetached();
        }

        private HashSet<object> GetHashSetOfChangedSwitches(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();

            //Common.DebugP("Old: " + Convert.ToString(oldValue[0], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[1], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[2], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[3], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[4], 2).PadLeft(8, '0'));
            //Common.DebugP("New: " + Convert.ToString(newValue[0], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[1], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[2], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[3], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[4], 2).PadLeft(8, '0'));
            for (var i = 0; i < 5; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var tpmPanelSwitch in _tpmPanelSwitches)
                {
                    if (tpmPanelSwitch.Group == i && (FlagHasChanged(oldByte, newByte, tpmPanelSwitch.Mask) || _isFirstNotification))
                    {
                        tpmPanelSwitch.IsOn = FlagValue(newValue, tpmPanelSwitch);
                        result.Add(tpmPanelSwitch);
                    }
                }
            }
            return result;
        }

        private static bool FlagValue(byte[] currentValue, TPMPanelSwitch tpmPanelSwitch)
        {
            return (currentValue[tpmPanelSwitch.Group] & tpmPanelSwitch.Mask) > 0;
        }

        private void CreateSwitchKeys()
        {
            _tpmPanelSwitches = TPMPanelSwitch.GetTPMPanelSwitches();
        }

        public HashSet<DCSBIOSBindingTPM> DCSBiosBindings
        {
            get { return _dcsBiosBindings; }
            set { _dcsBiosBindings = value; }
        }
    }


}
