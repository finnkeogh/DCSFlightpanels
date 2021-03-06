using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public class RadioPanelPZ69UH1H : RadioPanelPZ69Base, IDCSBIOSStringListener, IRadioPanel
    {
        private HashSet<RadioPanelKnobUH1H> _radioPanelKnobs = new HashSet<RadioPanelKnobUH1H>();
        private CurrentUH1HRadioMode _currentUpperRadioMode = CurrentUH1HRadioMode.UHF;
        private CurrentUH1HRadioMode _currentLowerRadioMode = CurrentUH1HRadioMode.UHF;

        /*UH-1H INTERCOMM*/
        //PVT INT 1 2 3 4  (6 positions 0-5)
        private volatile uint _intercommSkipper;
        private object _lockIntercommDialObject = new object();
        //private volatile uint _intercommDialPosStandby = 0;  <--- Only active, user operates the knob directly, no need for a standby value
        //private volatile uint _intercommSavedActiveDialPosition = 0;
        private DCSBIOSOutput _intercommDcsbiosOutputActivePos;
        private volatile uint _intercommActiveDial1Pos;
        private const string IntercommDialCommandInc = "INT_MODE INC\n";
        private const string IntercommDialCommandDec = "INT_MODE DEC\n";
        private long _intercommThreadNowSynching;
        private long _intercommDialWaitingForFeedback;
        private const string IntercommVolumeKnobCommandInc = "INT_VOL +2500\n";
        private const string IntercommVolumeKnobCommandDec = "INT_VOL -2500\n";

        /*UH-1H AN/ARC-134 VHF Comm Radio Set Left side of lower control panel */
        //Large dial 116-149 [step of 1]
        //Small dial 0.00-0.95 [step of 0.05]
        private object _lockVhfCommDialsObject1 = new object();
        private object _lockVhfCommDialsObject2 = new object();
        private volatile uint _vhfCommBigFrequencyStandby = 116;
        private volatile uint _vhfCommSmallFrequencyStandby;
        private volatile uint _vhfCommSavedActiveSmallFrequency;
        private volatile uint _vhfCommSavedActiveBigFrequency = 116;
        private double _vhfCommActiveFrequency = 116.00;
        private long _vhfCommThreadNowSynching;
        private volatile uint _vhfCommActiveDial1Frequency = 116;
        private volatile uint _vhfCommActiveDial2Frequency = 95;
        private long _vhfCommDial1FreqWaitingForFeedback;
        private long _vhfCommDial2FreqWaitingForFeedback;
        private DCSBIOSOutput _vhfCommDcsbiosOutputActiveFrequency;
        private const string VhfCommFreq1DialCommand = "VHFCOMM_MHZ ";
        private const string VhfCommFreq2DialCommand = "VHFCOMM_KHZ ";
        private Thread _vhfCommSyncThread;


        /*AN/ARC-51BX UHF radio set*/
        //Large dial 200-399 [step of 1]
        //Small dial 0.00-0.95 [step of 0.05]
        private bool _uhfIncreasePresetChannel;
        private uint _uhfActivePresetChannel = 1;
        private const string UhfPresetDialCommandInc = "UHF_PRESET INC\n";
        private const string UhfPresetDialCommandDec = "UHF_PRESET DEC\n";
        private DCSBIOSOutput _uhfDcsbiosOutputActivePresetChannel;
        private object _lockUhfPresetChannelObject = new object();
        //private Thread _uhfPresetChannelSyncThread;
        //private long _uhfPresetChannelThreadNowSynching = 0;
        //private long _uhfPresetChannelWaitingForFeedback = 0;

        private object _lockUhfDialsObject1 = new object();
        private object _lockUhfDialsObject2 = new object();
        private object _lockUhfDialsObject3 = new object();
        private volatile uint _uhfBigFrequencyStandby = 225;
        private volatile uint _uhfSmallFrequencyStandby;
        private volatile uint _uhfSavedActiveBigFrequency = 225;
        private volatile uint _uhfSavedActiveSmallFrequency;
        private DCSBIOSOutput _uhfDcsbiosOutputActiveFrequency;
        private double _uhfActiveFrequency = 225.00;
        private volatile uint _uhfActiveDial1Frequency = 22;
        private volatile uint _uhfActiveDial2Frequency = 5;
        private volatile uint _uhfActiveDial3Frequency;
        private const string UhfFreq1DialCommand = "UHF_10MHZ "; // 20-39
        private const string UhfFreq2DialCommand = "UHF_1MHZ "; //0 1 2 3 4 5 6 7 8 9
        private const string UhfFreq3DialCommand = "UHF_50KHZ "; //00 - 95
        private Thread _uhfSyncThread;
        private long _uhfThreadNowSynching;
        private long _uhfDial1WaitingForFeedback;
        private long _uhfDial2WaitingForFeedback;
        private long _uhfDial3WaitingForFeedback;

        /*UH-1H AN/ARN-82 VHF Navigation Set*/
        //Large dial 107-126 [step of 1]
        //Small dial 0.00-0.95 [step of 0.05]
        private object _lockVhfNavDialsObject1 = new object();
        private object _lockVhfNavDialsObject2 = new object();
        private volatile uint _vhfNavBigFrequencyStandby = 107;
        private volatile uint _vhfNavSmallFrequencyStandby;
        private volatile uint _vhfNavSavedActiveBigFrequency = 107;
        private volatile uint _vhfNavSavedActiveSmallFrequency;
        private DCSBIOSOutput _vhfNavDcsbiosOutputActiveFrequency;
        private double _vhfNavActiveFrequency = 107.00;
        private volatile uint _vhfNavActiveDial1Frequency = 107;
        private volatile uint _vhfNavActiveDial2Frequency;
        private const string VhfNavFreq1DialCommand = "VHFNAV_MHZ ";
        private const string VhfNavFreq2DialCommand = "VHFNAV_KHZ ";
        private Thread _vhfNavSyncThread;
        private long _vhfNavThreadNowSynching;
        private long _vhfNavDial1WaitingForFeedback;
        private long _vhfNavDial2WaitingForFeedback;


        /*UH-1H ARC-131 VHF FM*/
        private uint _vhfFmBigFrequencyStandby = 30;
        private uint _vhfFmSmallFrequencyStandby;
        private uint _vhfFmSavedActiveBigFrequency = 30;
        private uint _vhfFmSavedActiveSmallFrequency;
        private object _lockVhfFmDialsObject1 = new object();
        private object _lockVhfFmDialsObject2 = new object();
        private object _lockVhfFmDialsObject3 = new object();
        private object _lockVhfFmDialsObject4 = new object();
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial1;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial2;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial3;
        private DCSBIOSOutput _vhfFmDcsbiosOutputFreqDial4;
        private volatile uint _vhfFmActiveFreq1DialPos = 1;
        private volatile uint _vhfFmActiveFreq2DialPos = 1;
        private volatile uint _vhfFmActiveFreq3DialPos = 1;
        private volatile uint _vhfFmActiveFreq4DialPos = 1;
        private const string VhfFmFreq1DialCommand = "VHFFM_FREQ1 ";	//3 4 5 6
        private const string VhfFmFreq2DialCommand = "VHFFM_FREQ2 ";	//0 1 2 3 4 5 6 7 8 9
        private const string VhfFmFreq3DialCommand = "VHFFM_FREQ3 ";	//0 1 2 3 4 5 6 7 8 9
        private const string VhfFmFreq4DialCommand = "VHFFM_FREQ4 ";   //0 5
        private Thread _vhfFmSyncThread;
        private long _vhfFmThreadNowSynching;
        private long _vhfFmDial1WaitingForFeedback;
        private long _vhfFmDial2WaitingForFeedback;
        private long _vhfFmDial3WaitingForFeedback;
        private long _vhfFmDial4WaitingForFeedback;

        /*UH-1H ADF*/
        /*
            Small Knob - GAIN
            Large Knob - TUNE (Direct tune)
            ACT/STBY - BAND SELECTOR. 
            ACTIVE SCREEN - Frequency
            STANDBY SCREEN - SIGNAL STRENGTH
            
            190-1800 kHz
         */
        private bool _increaseAdfBand;
        private object _lockAdfFrequencyBandObject = new object();
        private object _lockAdfActiveFrequencyObject = new object();
        private object _lockAdfSignalStrengthObject = new object();
        private DCSBIOSOutput _adfDcsbiosOutputActiveFrequencyBand;
        private DCSBIOSOutput _adfDcsbiosOutputActiveFrequency;
        private DCSBIOSOutput _adfDcsbiosOutputSignalStrength;
        private volatile uint _adfActiveFrequencyRaw = 0;
        private double _adfActiveFrequency;
        private volatile uint _adfSignalStrengthRaw;
        private double _adfSignalStrength;
        private volatile uint _adfActiveFrequencyBand;
        private volatile uint _adfStandbyFrequencyBand;
        private const string AdfTuneKnobCommandInc = "ADF_TUNE -1000\n";
        private const string AdfTuneKnobCommandDec = "ADF_TUNE +1000\n";
        private const string AdfGainKnobCommandInc = "ADF_GAIN -2000\n";
        private const string AdfGainKnobCommandDec = "ADF_GAIN +2000\n";
        private const string AdfFrequencyBandCommand = "ADF_BAND ";
        private Thread _adfSyncThread;
        private long _adfThreadNowSynching;
        private long _adfFrequencyBandWaitingForFeedback;



        public RadioPanelPZ69UH1H(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD05;
            CreateRadioKnobs();
            Startup();
        }

        public void DCSBIOSStringReceived(uint address, string stringData)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(stringData))
                {
                    Common.DebugP("Received DCSBIOS stringData : " + stringData);
                    return;
                }
                if (address.Equals(_vhfCommDcsbiosOutputActiveFrequency.Address))
                {
                    /*UH-1H AN/ARC-134 VHF Comm Radio Set*/

                    //Large dial 116 - 149
                    //Small dial 000 - 975 [step of 25]
                    // NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED NOT USED
                    // 000 025 050 075 100 125 150 175 200 225 250 275 300 325 350 375 400 425 450 475 500 525 550 575 600 625 650 675 700 725 750 775 800 825 850 875 900 925 950 975
                    //  1   2   3   4   5   6   7   8   9   10  11  12  13  14  15  16  17  18  19  *20*  21  22  23  24  25  26  27  28  29  30  31  32  33  34  35  36  37  38  39  40
                    //  
                    // Only these are used because of PZ69 limitations
                    // 00 05 10 15 20 25 30 35 40 45 50 55 60 65 70 75 80 85 90 95
                    //  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20   

                    //"116.975" (7 characters)

                    var tmpFreq = Double.Parse(stringData, NumberFormatInfoFullDisplay);
                    if (!tmpFreq.Equals(_vhfCommActiveFrequency))
                    {
                        //Common.DebugP("Received VHF COMM frequency : " + stringData);
                    }
                    if (tmpFreq.Equals(_vhfCommActiveFrequency))
                    {
                        //No need to process same data over and over
                        return;
                    }
                    _vhfCommActiveFrequency = tmpFreq;
                    lock (_lockVhfCommDialsObject1)
                    {
                        var tmp = _vhfCommActiveDial1Frequency;
                        _vhfCommActiveDial1Frequency = uint.Parse(stringData.Substring(0, 3));
                        if (tmp != _vhfCommActiveDial1Frequency)
                        {
                            //Common.DebugP("VHF COMM 1 Before : " + tmp + "  now: " + _vhfCommActiveDial1Frequency);
                            Interlocked.Exchange(ref _vhfCommDial1FreqWaitingForFeedback, 0);
                        }
                    }
                    lock (_lockVhfCommDialsObject2)
                    {
                        var tmp = _vhfCommActiveDial2Frequency;
                        //var beforeRounding = stringData.Substring(4, 2);
                        _vhfCommActiveDial2Frequency = uint.Parse(stringData.Substring(4, 2));
                        //975
                        //Do not round this. Rounding means that the synch process thinks the frequency is OK which it isn't
                        if (tmp != _vhfCommActiveDial2Frequency)
                        {
                            //Common.DebugP("VHF COMM 2 Before : " + tmp + "  now: " + _vhfCommActiveDial2Frequency);
                            Interlocked.Exchange(ref _vhfCommDial2FreqWaitingForFeedback, 0);
                        }
                    }
                }
                if (address.Equals(_uhfDcsbiosOutputActiveFrequency.Address))
                {
                    /*UH-1H AN/ARC-134 VHF Comm Radio Set*/

                    //Large dial 200 - 399
                    //Small dial 00 - 95 [step of 5]
                    //"225.95" (6 characters)

                    var tmpFreq = Double.Parse(stringData, NumberFormatInfoFullDisplay);
                    if (!tmpFreq.Equals(_uhfActiveFrequency))
                    {
                        //Common.DebugP("Received UHF frequency : " + stringData);
                    }
                    if (tmpFreq.Equals(_uhfActiveFrequency))
                    {
                        //No need to process same data over and over
                        return;
                    }
                    _uhfActiveFrequency = tmpFreq;
                    lock (_lockUhfDialsObject1)
                    {
                        var tmp = _uhfActiveDial1Frequency;
                        _uhfActiveDial1Frequency = uint.Parse(stringData.Substring(0, 2));
                        if (tmp != _uhfActiveDial1Frequency)
                        {
                            //Common.DebugP("UHF 1 Before : " + tmp + "  now: " + _uhfActiveDial1Frequency);
                            Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockUhfDialsObject2)
                    {
                        var tmp = _uhfActiveDial2Frequency;
                        _uhfActiveDial2Frequency = uint.Parse(stringData.Substring(2, 1));
                        if (tmp != _uhfActiveDial2Frequency)
                        {
                            //Common.DebugP("UHF 2 Before : " + tmp + "  now: " + _uhfActiveDial2Frequency);
                            Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockUhfDialsObject3)
                    {
                        var tmp = _uhfActiveDial3Frequency;
                        _uhfActiveDial3Frequency = uint.Parse(stringData.Substring(4, 2));
                        if (tmp != _uhfActiveDial3Frequency)
                        {
                            //Common.DebugP("UHF 3 Before : " + tmp + "  now: " + _uhfActiveDial3Frequency);
                            Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 0);
                        }
                    }
                }
                if (address.Equals(_vhfNavDcsbiosOutputActiveFrequency.Address))
                {

                    /*UH-1H AN/ARN-82 VHF Navigation Set*/
                    //Large dial 107-126 [step of 1]
                    //Small dial 0.00-0.95 [step of 0.05]

                    var tmpFreq = Double.Parse(stringData, NumberFormatInfoFullDisplay);
                    if (!tmpFreq.Equals(_vhfNavActiveFrequency))
                    {
                        //Common.DebugP("Received VHF NAV frequency : " + stringData);
                    }
                    if (tmpFreq.Equals(_vhfNavActiveFrequency))
                    {
                        //No need to process same data over and over
                        return;
                    }
                    _vhfNavActiveFrequency = tmpFreq;
                    //107.95
                    lock (_lockVhfNavDialsObject1)
                    {
                        var tmp = _vhfNavActiveDial1Frequency;
                        _vhfNavActiveDial1Frequency = uint.Parse(stringData.Substring(0, 3));
                        if (tmp != _vhfNavActiveDial1Frequency)
                        {
                            //Common.DebugP("VHF NAV 1 Before : " + tmp + "  now: " + _vhfNavActiveDial1Frequency);
                            Interlocked.Exchange(ref _vhfNavDial1WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockVhfNavDialsObject2)
                    {
                        var tmp = _vhfNavActiveDial2Frequency;
                        _vhfNavActiveDial2Frequency = uint.Parse(stringData.Substring(4, 2));
                        if (tmp != _vhfNavActiveDial2Frequency)
                        {
                            //Common.DebugP("VHF NAV Before : " + tmp + "  now: " + _vhfNavActiveDial2Frequency);
                            Interlocked.Exchange(ref _vhfNavDial2WaitingForFeedback, 0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Common.LogError(349998, e, "DCSBIOSStringReceived()");
            }
            ShowFrequenciesOnPanel();
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            //Common.DebugP("PZ69 UH-1H READ ENTERING");
            //INTERCOMM
            if (address == _intercommDcsbiosOutputActivePos.Address)
            {
                lock (_lockIntercommDialObject)
                {
                    var tmp = _intercommActiveDial1Pos;
                    _intercommActiveDial1Pos = _intercommDcsbiosOutputActivePos.GetUIntValue(data);
                    if (tmp != _intercommActiveDial1Pos)
                    {
                        //Common.DebugP("INTERCOMM Before : " + tmp + "  now: " + _intercommActiveDial1Pos);
                        Interlocked.Exchange(ref _intercommDialWaitingForFeedback, 0);
                    }
                }
            }

            //VHF FM
            if (address == _vhfFmDcsbiosOutputFreqDial1.Address)
            {
                //Common.DebugP("VHF FM 1 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfFmDialsObject1)
                {
                    //Common.DebugP("Just read VHF FM Dial 1 Position: " + _vhfFmActiveFreq1DialPos);
                    var tmp = _vhfFmActiveFreq1DialPos;
                    _vhfFmActiveFreq1DialPos = _vhfFmDcsbiosOutputFreqDial1.GetUIntValue(data);
                    if (tmp != _vhfFmActiveFreq1DialPos)
                    {
                        //Common.DebugP("VHF FM 1 Before : " + tmp + "  now: " + _vhfFmActiveFreq1DialPos);
                        Interlocked.Exchange(ref _vhfFmDial1WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _vhfFmDcsbiosOutputFreqDial2.Address)
            {
                //Common.DebugP("VHF FM_10MHZ_SEL Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfFmDialsObject2)
                {
                    //Common.DebugP("Just read VHF FM Dial 2 Position: " + _vhfFmActiveFreq2DialPos);
                    var tmp = _vhfFmActiveFreq2DialPos;
                    _vhfFmActiveFreq2DialPos = _vhfFmDcsbiosOutputFreqDial2.GetUIntValue(data);
                    if (tmp != _vhfFmActiveFreq2DialPos)
                    {
                        //Common.DebugP("VHF FM 2 Before : " + tmp + "  now: " + _vhfFmActiveFreq2DialPos);
                        Interlocked.Exchange(ref _vhfFmDial2WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _vhfFmDcsbiosOutputFreqDial3.Address)
            {
                //Common.DebugP("VHFAM_FREQ1 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfFmDialsObject3)
                {
                    //Common.DebugP("Just read VHF FM Dial 3 Position: " + _vhfFmActiveFreq3DialPos);
                    var tmp = _vhfFmActiveFreq3DialPos;
                    _vhfFmActiveFreq3DialPos = _vhfFmDcsbiosOutputFreqDial3.GetUIntValue(data);
                    if (tmp != _vhfFmActiveFreq3DialPos)
                    {
                        //Common.DebugP("VHF FM 3 Before : " + tmp + "  now: " + _vhfFmActiveFreq3DialPos);
                        Interlocked.Exchange(ref _vhfFmDial3WaitingForFeedback, 0);
                    }
                }
            }
            if (address == _vhfFmDcsbiosOutputFreqDial4.Address)
            {
                //Common.DebugP("VHF FM 4 Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockVhfFmDialsObject4)
                {
                    //Common.DebugP("Just read VHF FM Dial 4 Position: " + _vhfFmActiveFreq4DialPos);
                    var tmp = _vhfFmActiveFreq4DialPos;
                    _vhfFmActiveFreq4DialPos = _vhfFmDcsbiosOutputFreqDial4.GetUIntValue(data);
                    if (tmp != _vhfFmActiveFreq4DialPos)
                    {
                        //Common.DebugP("VHF FM 4 Before : " + tmp + "  now: " + _vhfFmActiveFreq4DialPos);
                        Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 0);
                    }
                }
            }

            //ADF
            if (address == _adfDcsbiosOutputActiveFrequencyBand.Address)
            {
                //Common.DebugP("ADF BAND Arrived, waiting for lock." + Environment.TickCount);
                lock (_lockAdfFrequencyBandObject)
                {
                    //Common.DebugP("Just read ADF BAND: " + _adfActiveFrequencyBand);
                    var tmp = _adfActiveFrequencyBand;
                    _adfActiveFrequencyBand = _adfDcsbiosOutputActiveFrequencyBand.GetUIntValue(data);
                    if (tmp != _adfActiveFrequencyBand)
                    {
                        //Common.DebugP("ADF BAND Before : " + tmp + "  now: " + _adfActiveFrequencyBand);
                        Interlocked.Exchange(ref _adfFrequencyBandWaitingForFeedback, 0);
                    }
                }
            }
            if (address == _adfDcsbiosOutputActiveFrequency.Address)
            {
                UpdateActiveAdfFrequency(_adfDcsbiosOutputActiveFrequency.GetUIntValue(data));
            }
            if (address == _adfDcsbiosOutputSignalStrength.Address)
            {
                lock (_lockAdfSignalStrengthObject)
                {
                    //Common.DebugP("Just read ADF Signal Strength Raw: " + _adfSignalStrengthRaw);
                    var tmp = _adfSignalStrengthRaw;
                    _adfSignalStrengthRaw = _adfDcsbiosOutputSignalStrength.GetUIntValue(data);
                    if (tmp != _adfSignalStrengthRaw)
                    {
                        const float maxValue = 65535;
                        _adfSignalStrength = ((_adfSignalStrengthRaw / maxValue) * 100);
                    }
                }
            }

            //UHF Preset Channel
            if (address.Equals(_uhfDcsbiosOutputActivePresetChannel.Address))
            {
                lock (_lockUhfPresetChannelObject)
                {
                    _uhfActivePresetChannel = _uhfDcsbiosOutputActivePresetChannel.GetUIntValue(data) + 1;
                }
            }

            //Set once
            DataHasBeenReceivedFromDCSBIOS = true;
            ShowFrequenciesOnPanel();
            //Common.DebugP("PZ69 UH-1H READ EXITING");
        }

        private void UpdateActiveAdfFrequency(uint adfActiveFrequencyRaw)
        {
            lock (_lockAdfActiveFrequencyObject)
            {
                if (adfActiveFrequencyRaw != _adfActiveFrequencyRaw)
                {
                    //Common.DebugP("Just read ADF RAW FREQ: " + adfActiveFrequencyRaw);
                    //Update only if data has changed. Max 65535 
                    const float maxValue = 65535;
                    switch (_adfActiveFrequencyBand)
                    {
                        case 0: //190-400 kHz (~210kHz)
                            {
                                //A = desired freq
                                //B = freq based on adfActiveFrequencyRaw
                                //(A>200) A = B - ((B * -0.04408) + 18.31) //Creds Paul Marsh
                                var b = ((adfActiveFrequencyRaw / maxValue) * 210) + 190;
                                _adfActiveFrequency = b - ((b * -0.04408) + 18.31);
                                break;
                            }
                        case 1: //400-850 kHz (~450kHz)
                            {
                                var b = ((adfActiveFrequencyRaw / maxValue) * 450) + 400;
                                if (b < 451)
                                {
                                    //A = B + ((B * -0.44837) + 177.08)
                                    _adfActiveFrequency = b + ((b * -0.44837) + 181.58);
                                }
                                else
                                {
                                    //A = B + ((B * 0.11291) - 100.61)
                                    _adfActiveFrequency = b + ((b * 0.11291) - 96.11);
                                }
                                break;
                            }
                        case 2: //850-1800 kHz (~950kHz)
                            {
                                //A = B - ((B * -0.04532) + 91.54)
                                var b = ((adfActiveFrequencyRaw / maxValue) * 950) + 850;
                                _adfActiveFrequency = b - ((b * -0.04532) + 91.54);
                                break;
                            }
                    }
                    //Common.DebugP("Just read ADF Active FREQ is now: " + _adfActiveFrequency.ToString("0.00", NumberFormatInfoDefault));
                }
            }
        }
        /*
        private void UpdateActiveAdfFrequency()
        {
            lock (_lockAdfActiveFrequencyObject)
            {
                const float maxValue = 65535;
                switch (_adfActiveFrequencyBand)
                {
                    case 0: //190-400 kHz (~210kHz)
                        {
                            _adfActiveFrequency = ((_adfActiveFrequencyRaw / maxValue) * 210) + 190;
                            break;
                        }
                    case 1: //400-850 kHz (~450kHz)
                        {
                            _adfActiveFrequency = ((_adfActiveFrequencyRaw / maxValue) * 450) + 400;
                            break;
                        }
                    case 2: //850-1800 kHz (~950kHz)
                        {
                            _adfActiveFrequency = ((_adfActiveFrequencyRaw / maxValue) * 950) + 850;
                            break;
                        }
                }
            }
        }
        */
        private void SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsUH1H knob)
        {
            if (!DataHasBeenReceivedFromDCSBIOS)
            {
                //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                return;
            }
            switch (knob)
            {
                case RadioPanelPZ69KnobsUH1H.UPPER_FREQ_SWITCH:
                    {
                        switch (_currentUpperRadioMode)
                        {
                            case CurrentUH1HRadioMode.INTERCOMM:
                                {
                                    SendUhfPresetChannelChangeToDCSBIOS();
                                    break;
                                }
                            case CurrentUH1HRadioMode.VHFCOMM:
                                {
                                    SendVhfCommToDCSBIOS();
                                    break;
                                }
                            case CurrentUH1HRadioMode.UHF:
                                {
                                    SendUhfToDCSBIOS();
                                    break;
                                }
                            case CurrentUH1HRadioMode.VHFFM:
                                {
                                    SendVhfFmToDCSBIOS();
                                    break;
                                }
                            case CurrentUH1HRadioMode.VHFNAV:
                                {
                                    SendVhfNavToDCSBIOS();
                                    break;
                                }
                            case CurrentUH1HRadioMode.ADF:
                                {
                                    SendAdfBandChangeToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
                case RadioPanelPZ69KnobsUH1H.LOWER_FREQ_SWITCH:
                    {
                        switch (_currentLowerRadioMode)
                        {
                            case CurrentUH1HRadioMode.INTERCOMM:
                                {
                                    SendUhfPresetChannelChangeToDCSBIOS();
                                    break;
                                }
                            case CurrentUH1HRadioMode.VHFCOMM:
                                {
                                    SendVhfCommToDCSBIOS();
                                    break;
                                }
                            case CurrentUH1HRadioMode.UHF:
                                {
                                    SendUhfToDCSBIOS();
                                    break;
                                }
                            case CurrentUH1HRadioMode.VHFFM:
                                {
                                    SendVhfFmToDCSBIOS();
                                    break;
                                }
                            case CurrentUH1HRadioMode.VHFNAV:
                                {
                                    SendVhfNavToDCSBIOS();
                                    break;
                                }
                            case CurrentUH1HRadioMode.ADF:
                                {
                                    SendAdfBandChangeToDCSBIOS();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }
        /*
        private void SendIntercommToDCSBIOS()
        {
            if (IntercommSyncing())
            {
                return;
            }
            SaveActiveIntercomm();
            if (_intercommSyncThread != null)
            {
                _intercommSyncThread.Abort();
            }
            //Common.DebugP("CREATING _intercommSyncThread");
            _intercommSyncThread = new Thread(IntercommSynchThreadMethod);
            _intercommSyncThread.Start();
        }

        private void IntercommSynchThreadMethod()
        {
            try
            {
                try
                {
                    String str;
                    Interlocked.Exchange(ref _intercommThreadNowSynching, 1);
                    var inc = "INC\n";
                    var dec = "DEC\n";
                    long dial1Timeout = DateTime.Now.Ticks;
                    long dial1OkTime = 0;
                    var dial1SendCount = 0;

                    do
                    {
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "INTERCOMM dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _intercommDialWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for INTERCOMM");
                        }

                        //0 - 5
                        if (Interlocked.Read(ref _intercommDialWaitingForFeedback) == 0)
                        {
                            lock (_lockIntercommDialObject)
                            {

                                //Common.DebugP("_intercommActiveDial1Pos is " + _intercommActiveDial1Pos + " and should be " + _intercommDialPosStandby);
                                if (_intercommDialPosStandby != _intercommActiveDial1Pos)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    str = IntercommDialCommand + (_intercommDialPosStandby < _intercommActiveDial1Pos ? dec : inc);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _intercommDialWaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (dial1SendCount > 5)
                        {
                            //"Race" condition detected?
                            dial1SendCount = 0;
                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS
                    } while (IsTooShort(dial1OkTime));

                    SwapActiveIntercomm();
                    ShowFrequenciesOnPanel();
                }
                finally
                {
                    Interlocked.Exchange(ref _intercommThreadNowSynching, 0);
                }
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(564523, ex);
            }
        }
        */
        private void SendVhfCommToDCSBIOS()
        {
            if (VhfCommSyncing())
            {
                return;
            }
            SaveActiveFrequencyVhfComm();
            //Frequency selector 1 rotates both ways
            //      "116" "117" "118" .. "149"

            //Frequency selector 2 rotates both ways
            //      0.00 - 0.95

            //Send INC / DEC until frequency is correct. NOT THE DIALS!

            if (_vhfCommSyncThread != null)
            {
                _vhfCommSyncThread.Abort();
            }
            Common.DebugP(Environment.NewLine + "**** CREATING _vhfCommSyncThread ****" + Environment.NewLine + Environment.NewLine);
            _vhfCommSyncThread = new Thread(VhfCommSynchThreadMethod);
            _vhfCommSyncThread.Start();
        }

        private void VhfCommSynchThreadMethod()
        {
            try
            {
                try
                {
                    string str;
                    Interlocked.Exchange(ref _vhfCommThreadNowSynching, 1);
                    long dial1Timeout = DateTime.Now.Ticks;
                    long dial2Timeout = DateTime.Now.Ticks;
                    long dial1OkTime = 0;
                    long dial2OkTime = 0;
                    var dial1SendCount = 0;
                    var dial2SendCount = 0;

                    //Reason for this is to separate the standby frequency from the sync loop
                    //If not the sync would pick up any changes made by the user during the
                    //sync process
                    var localVhfCommBigFrequencyStandby = _vhfCommBigFrequencyStandby;
                    var localVhfCommSmallFrequencyStandby = _vhfCommSmallFrequencyStandby;

                    do
                    {
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "VHF COMM dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfCommDial1FreqWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF COMM 1");
                        }
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "VHF COMM dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfCommDial2FreqWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF COMM 2");
                        }


                        if (Interlocked.Read(ref _vhfCommDial1FreqWaitingForFeedback) == 0)
                        {
                            lock (_lockVhfCommDialsObject1)
                            {

                                //Common.DebugP("_vhfCommActiveDial1Frequency is " + _vhfCommActiveDial1Frequency + " and should be " + localVhfCommBigFrequencyStandby);
                                if (localVhfCommBigFrequencyStandby != _vhfCommActiveDial1Frequency)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    str = VhfCommFreq1DialCommand + GetCommandDirectionForVhfCommDial1(localVhfCommBigFrequencyStandby, _vhfCommActiveDial1Frequency);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vhfCommDial1FreqWaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfCommDial2FreqWaitingForFeedback) == 0)
                        {
                            lock (_lockVhfCommDialsObject2)
                            {

                                //Common.DebugP("_vhfCommActiveDial2Frequency is " + _vhfCommActiveDial2Frequency + " and should be " + localVhfCommSmallFrequencyStandby);
                                if (localVhfCommSmallFrequencyStandby != _vhfCommActiveDial2Frequency)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    str = VhfCommFreq2DialCommand + GetCommandDirectionForVhfCommDial2(localVhfCommSmallFrequencyStandby, _vhfCommActiveDial2Frequency);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _vhfCommDial2FreqWaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (dial1SendCount > 20 || dial2SendCount > 25)
                        {
                            //"Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS
                    } while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime));

                    SwapActiveStandbyFrequencyVhfComm();
                    ShowFrequenciesOnPanel();
                }
                finally
                {
                    Interlocked.Exchange(ref _vhfCommThreadNowSynching, 0);
                }
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(564523, ex);
            }
        }

        private void SendUhfToDCSBIOS()
        {
            if (UhfSyncing())
            {
                return;
            }
            SaveActiveFrequencyUhf();
            //Frequency selector 1 rotates both ways
            //      "20" "21" "22" .. "39"

            //Frequency selector 2 rotates both ways
            //      0 - 9

            //Frequency selector 2 rotates both ways
            //      0 - 95 (-/+ 5)

            //Send INC / DEC until frequency is correct. NOT THE DIALS!

            if (_uhfSyncThread != null)
            {
                _uhfSyncThread.Abort();
            }
            //Common.DebugP("CREATING _uhfSyncThread");
            _uhfSyncThread = new Thread(UhfSynchThreadMethod);
            _uhfSyncThread.Start();
        }

        private void UhfSynchThreadMethod()
        {
            try
            {
                try
                {
                    string str;
                    Interlocked.Exchange(ref _uhfThreadNowSynching, 1);
                    long dial1Timeout = DateTime.Now.Ticks;
                    long dial2Timeout = DateTime.Now.Ticks;
                    long dial3Timeout = DateTime.Now.Ticks;
                    long dial1OkTime = 0;
                    long dial2OkTime = 0;
                    long dial3OkTime = 0;
                    var dial1SendCount = 0;
                    var dial2SendCount = 0;
                    var dial3SendCount = 0;

                    //Reason for having it outside the loop is to separate the standby frequency from the sync loop
                    //If not the sync would pick up any changes made by the user during the
                    //sync process
                    //225.95
                    var filler = "";
                    if (_uhfSmallFrequencyStandby < 10)
                    {
                        filler = "0";
                    }
                    var standbyFrequency = double.Parse(_uhfBigFrequencyStandby + "." + filler + _uhfSmallFrequencyStandby, NumberFormatInfoFullDisplay);
                    var dial1StandbyFrequency = uint.Parse(standbyFrequency.ToString("0.00", NumberFormatInfoFullDisplay).Substring(0, 2));
                    var dial2StandbyFrequency = uint.Parse(standbyFrequency.ToString("0.00", NumberFormatInfoFullDisplay).Substring(2, 1));
                    var dial3StandbyFrequency = uint.Parse(standbyFrequency.ToString("0.00", NumberFormatInfoFullDisplay).Substring(4, 2));

                    do
                    {
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "UHF dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 1");
                        }
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "UHF dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 2");
                        }
                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "UHF dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for UHF 3");
                        }


                        if (Interlocked.Read(ref _uhfDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockUhfDialsObject1)
                            {

                                //Common.DebugP("_uhfActiveDial1Frequency is " + _uhfActiveDial1Frequency + " and should be " + dial1StandbyFrequency);
                                if (dial1StandbyFrequency != _uhfActiveDial1Frequency)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    str = UhfFreq1DialCommand + GetCommandDirectionForUhfDial1(dial1StandbyFrequency, _uhfActiveDial1Frequency);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _uhfDial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _uhfDial2WaitingForFeedback) == 0)
                        {
                            lock (_lockUhfDialsObject2)
                            {

                                //Common.DebugP("_uhfActiveDial2Frequency is " + _uhfActiveDial2Frequency + " and should be " + dial2StandbyFrequency);
                                if (dial2StandbyFrequency != _uhfActiveDial2Frequency)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    str = UhfFreq2DialCommand + GetCommandDirectionForUhfDial2(dial2StandbyFrequency, _uhfActiveDial2Frequency);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _uhfDial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _uhfDial3WaitingForFeedback) == 0)
                        {
                            lock (_lockUhfDialsObject3)
                            {

                                //Common.DebugP("_uhfActiveDial3Frequency is " + _uhfActiveDial3Frequency + " and should be " + dial3StandbyFrequency);
                                if (dial3StandbyFrequency != _uhfActiveDial3Frequency)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                    str = UhfFreq3DialCommand + GetCommandDirectionForUhfDial3(dial3StandbyFrequency, _uhfActiveDial3Frequency);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _uhfDial3WaitingForFeedback, 1);
                                }
                                Reset(ref dial3Timeout);
                            }
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (dial1SendCount > 19 || dial2SendCount > 9 || dial3SendCount > 20)
                        {
                            //"Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS
                    } while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime));
                    SwapActiveStandbyFrequencyUhf();
                    ShowFrequenciesOnPanel();
                }
                finally
                {
                    Interlocked.Exchange(ref _uhfThreadNowSynching, 0);
                }
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(564523, ex);
            }
        }


        private void SendVhfNavToDCSBIOS()
        {
            if (VhfNavSyncing())
            {
                return;
            }
            SaveActiveFrequencyVhfNav();
            if (_vhfNavSyncThread != null)
            {
                _vhfNavSyncThread.Abort();
            }
            _vhfNavSyncThread = new Thread(VhfNavSynchThreadMethod);
            _vhfNavSyncThread.Start();
        }

        private void VhfNavSynchThreadMethod()
        {
            try
            {
                try
                {
                    string str;
                    Interlocked.Exchange(ref _vhfNavThreadNowSynching, 1);
                    long dial1Timeout = DateTime.Now.Ticks;
                    long dial2Timeout = DateTime.Now.Ticks;
                    long dial1OkTime = 0;
                    long dial2OkTime = 0;
                    var dial1SendCount = 0;
                    var dial2SendCount = 0;

                    //Reason for having it outside the loop is to separate the standby frequency from the sync loop
                    //If not the sync would pick up any changes made by the user during the
                    //sync process
                    //107.95
                    var filler = "";
                    if (_vhfNavSmallFrequencyStandby < 10)
                    {
                        filler = "0";
                    }
                    var standbyFrequency = double.Parse(_vhfNavBigFrequencyStandby + "." + filler + _vhfNavSmallFrequencyStandby, NumberFormatInfoFullDisplay);
                    var dial1StandbyFrequency = uint.Parse(standbyFrequency.ToString("0.00", NumberFormatInfoFullDisplay).Substring(0, 3));
                    var dial2StandbyFrequency = uint.Parse(standbyFrequency.ToString("0.00", NumberFormatInfoFullDisplay).Substring(4, 2));

                    do
                    {
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "VHF NAV dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfNavDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF NAV 1");
                        }
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "VHF NAV dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfNavDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF NAV 2");
                        }


                        if (Interlocked.Read(ref _vhfNavDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfNavDialsObject1)
                            {

                                //Common.DebugP("_vhfNavActiveDial1Frequency is " + _vhfNavActiveDial1Frequency + " and should be " + dial1StandbyFrequency);
                                if (dial1StandbyFrequency != _vhfNavActiveDial1Frequency)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                    str = VhfNavFreq1DialCommand + GetCommandDirectionForVhfNavDial1(dial1StandbyFrequency, _vhfNavActiveDial1Frequency);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vhfNavDial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfNavDial2WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfNavDialsObject2)
                            {

                                //Common.DebugP("_vhfNavActiveDial2Frequency is " + _vhfNavActiveDial2Frequency + " and should be " + dial2StandbyFrequency);
                                if (dial2StandbyFrequency != _vhfNavActiveDial2Frequency)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                    //Compatible : GetCommandDirectionForUhfDial3
                                    str = VhfNavFreq2DialCommand + GetCommandDirectionForUhfDial3(dial2StandbyFrequency, _vhfNavActiveDial2Frequency);
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _vhfNavDial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (dial1SendCount > 19 || dial2SendCount > 9)
                        {
                            //"Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            Thread.Sleep(5000);
                        }

                        Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS
                    } while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime));
                    SwapActiveStandbyFrequencyVhfNav();
                    ShowFrequenciesOnPanel();
                }
                finally
                {
                    Interlocked.Exchange(ref _vhfNavThreadNowSynching, 0);
                }
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(564523, ex);
            }
        }


        private void SendVhfFmToDCSBIOS()
        {
            if (VhfFmSyncing())
            {
                return;
            }
            SaveActiveFrequencyVhfFm();
            if (_vhfFmSyncThread != null)
            {
                _vhfFmSyncThread.Abort();
            }
            _vhfFmSyncThread = new Thread(VhfFmSynchThreadMethod);

            _vhfFmSyncThread.Start();
        }


        private void VhfFmSynchThreadMethod()
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _vhfFmThreadNowSynching, 1);
                    long dial1Timeout = DateTime.Now.Ticks;
                    long dial2Timeout = DateTime.Now.Ticks;
                    long dial3Timeout = DateTime.Now.Ticks;
                    long dial4Timeout = DateTime.Now.Ticks;
                    long dial1OkTime = 0;
                    long dial2OkTime = 0;
                    long dial3OkTime = 0;
                    long dial4OkTime = 0;
                    var dial1SendCount = 0;
                    var dial2SendCount = 0;
                    var dial3SendCount = 0;
                    var dial4SendCount = 0;

                    //Frequency selector 1     
                    //       3  4   5   6   7
                    //Pos    0  1   2   3   4

                    //Frequency selector 2      
                    //0 1 2 3 4 5

                    //Frequency selector 3
                    //0 1 2 3 4 5 6 7 8 9


                    //Frequency selector 4
                    //      0 5
                    //Pos   0 1

                    //Large dial 30 - 75 [step of 1]
                    //Small dial 0.00-0.95 [step of 0.05]
                    var filler = "";
                    if (_vhfFmSmallFrequencyStandby < 10)
                    {
                        filler = "0";
                    }
                    var frequencyAsString = _vhfFmBigFrequencyStandby + "." + filler + _vhfFmSmallFrequencyStandby;

                    //75.95
                    var desiredFreqDial1Pos = int.Parse(frequencyAsString.Substring(0, 1)) - 3;
                    var desiredFreqDial2Pos = int.Parse(frequencyAsString.Substring(1, 1));
                    var desiredFreqDial3Pos = int.Parse(frequencyAsString.Substring(3, 1));
                    var desiredFreqDial4Pos = int.Parse(frequencyAsString.Substring(4, 1));
                    if (desiredFreqDial4Pos == 5)
                    {
                        //0 -> pos 0
                        //5 -> pos 1
                        desiredFreqDial4Pos = 1;
                    }

                    do
                    {
                        if (IsTimedOut(ref dial1Timeout, ResetSyncTimeout, "VHF FM dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial1WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 1");
                        }
                        if (IsTimedOut(ref dial2Timeout, ResetSyncTimeout, "VHF FM dial2Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial2WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 2");
                        }
                        if (IsTimedOut(ref dial3Timeout, ResetSyncTimeout, "VHF FM dial3Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial3WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 3");
                        }
                        if (IsTimedOut(ref dial4Timeout, ResetSyncTimeout, "VHF FM dial4Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for VHF FM 4");
                        }

                        if (Interlocked.Read(ref _vhfFmDial1WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfFmDialsObject1)
                            {
                                if (_vhfFmActiveFreq1DialPos != desiredFreqDial1Pos)
                                {
                                    dial1OkTime = DateTime.Now.Ticks;
                                }
                                //Common.DebugP("_vhfFmActiveFreq1DialPos is " + _vhfFmActiveFreq1DialPos + " and should be " + desiredFreqDial1Pos);
                                if (_vhfFmActiveFreq1DialPos < desiredFreqDial1Pos)
                                {
                                    var str = VhfFmFreq1DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial1WaitingForFeedback, 1);
                                }
                                else if (_vhfFmActiveFreq1DialPos > desiredFreqDial1Pos)
                                {
                                    var str = VhfFmFreq1DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial1SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial1WaitingForFeedback, 1);
                                }
                                Reset(ref dial1Timeout);
                            }
                        }
                        else
                        {
                            dial1OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfFmDial2WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfFmDialsObject2)
                            {
                                if (_vhfFmActiveFreq2DialPos != desiredFreqDial2Pos)
                                {
                                    dial2OkTime = DateTime.Now.Ticks;
                                }
                                //Common.DebugP("_vhfFmActiveFreq2DialPos is " + _vhfFmActiveFreq2DialPos + " and should be " + desiredFreqDial2Pos);
                                if (_vhfFmActiveFreq2DialPos < desiredFreqDial2Pos)
                                {
                                    var str = VhfFmFreq2DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial2WaitingForFeedback, 1);
                                }
                                else if (_vhfFmActiveFreq2DialPos > desiredFreqDial2Pos)
                                {
                                    var str = VhfFmFreq2DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial2SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial2WaitingForFeedback, 1);
                                }
                                Reset(ref dial2Timeout);
                            }
                        }
                        else
                        {
                            dial2OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfFmDial3WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfFmDialsObject3)
                            {
                                if (_vhfFmActiveFreq3DialPos != desiredFreqDial3Pos)
                                {
                                    dial3OkTime = DateTime.Now.Ticks;
                                }
                                //Common.DebugP("_vhfFmActiveFreq3DialPos is " + _vhfFmActiveFreq3DialPos + " and should be " + desiredFreqDial3Pos);
                                if (_vhfFmActiveFreq3DialPos < desiredFreqDial3Pos)
                                {
                                    var str = VhfFmFreq3DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial3WaitingForFeedback, 1);
                                }
                                else if (_vhfFmActiveFreq3DialPos > desiredFreqDial3Pos)
                                {
                                    var str = VhfFmFreq3DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial3SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial3WaitingForFeedback, 1);
                                }
                                Reset(ref dial3Timeout);
                            }
                        }
                        else
                        {
                            dial3OkTime = DateTime.Now.Ticks;
                        }

                        if (Interlocked.Read(ref _vhfFmDial4WaitingForFeedback) == 0)
                        {
                            lock (_lockVhfFmDialsObject4)
                            {
                                if (_vhfFmActiveFreq4DialPos != desiredFreqDial4Pos)
                                {
                                    dial4OkTime = DateTime.Now.Ticks;
                                }
                                //Common.DebugP("_vhfFmActiveFreq4DialPos is " + _vhfFmActiveFreq4DialPos + " and should be " + desiredFreqDial4Pos);
                                if (_vhfFmActiveFreq4DialPos < desiredFreqDial4Pos)
                                {
                                    var str = VhfFmFreq4DialCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 1);
                                }
                                else if (_vhfFmActiveFreq4DialPos > desiredFreqDial4Pos)
                                {
                                    var str = VhfFmFreq4DialCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    dial4SendCount++;
                                    Interlocked.Exchange(ref _vhfFmDial4WaitingForFeedback, 1);
                                }
                                Reset(ref dial4Timeout);
                            }
                        }
                        else
                        {
                            dial4OkTime = DateTime.Now.Ticks;
                        }

                        if (dial1SendCount > 4 || dial2SendCount > 10 || dial3SendCount > 10 || dial4SendCount > 2)
                        {
                            //"Race" condition detected?
                            dial1SendCount = 0;
                            dial2SendCount = 0;
                            dial3SendCount = 0;
                            dial4SendCount = 0;
                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS
                    }
                    while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime));
                }
                finally
                {
                    Interlocked.Exchange(ref _vhfFmThreadNowSynching, 0);
                }
                SwapActiveStandbyFrequencyVhfFm();
                ShowFrequenciesOnPanel();
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(56453, ex);
            }
        }


        private void SendAdfBandChangeToDCSBIOS()
        {
            if (_adfSyncThread != null)
            {
                _adfSyncThread.Abort();
            }
            _adfSyncThread = new Thread(AdfBandChangeSynchThreadMethod);

            _adfSyncThread.Start();
        }


        private void AdfBandChangeSynchThreadMethod()
        {
            try
            {
                try
                {
                    Interlocked.Exchange(ref _adfThreadNowSynching, 1);
                    long freqBandDialTimeout = DateTime.Now.Ticks;
                    long freqBandDialOkTime = 0;
                    var freqBandDialSendCount = 0;
                    var once = true;
                    //Frequency Band selector
                    //Pos    0  1   2
                    switch (_adfStandbyFrequencyBand)
                    {
                        case 0:
                            {
                                _increaseAdfBand = true;
                                _adfStandbyFrequencyBand = 1;
                                break;
                            }
                        case 1:
                            {
                                if (_increaseAdfBand)
                                {
                                    _adfStandbyFrequencyBand = 2;
                                }
                                else
                                {
                                    _adfStandbyFrequencyBand = 0;
                                }
                                break;
                            }
                        case 2:
                            {
                                _increaseAdfBand = false;
                                _adfStandbyFrequencyBand = 1;
                                break;
                            }
                    }
                    var desiredFreqBandDialPos = _adfStandbyFrequencyBand;

                    do
                    {
                        if (IsTimedOut(ref freqBandDialTimeout, ResetSyncTimeout, "ADF Frequency Band Selector dial1Timeout"))
                        {
                            //Lets do an ugly reset
                            Interlocked.Exchange(ref _adfFrequencyBandWaitingForFeedback, 0);
                            Common.DebugP("Resetting SYNC for ADF Frequency Band Selector");
                        }
                        if (Interlocked.Read(ref _adfFrequencyBandWaitingForFeedback) == 0)
                        {
                            lock (_lockAdfFrequencyBandObject)
                            {
                                if (_adfActiveFrequencyBand != desiredFreqBandDialPos)
                                {
                                    freqBandDialOkTime = DateTime.Now.Ticks;
                                    once = false;
                                }
                                //Common.DebugP("_adfActiveFrequencyBand is " + _adfActiveFrequencyBand + " and should be " + desiredFreqBandDialPos);
                                if (_adfActiveFrequencyBand < desiredFreqBandDialPos)
                                {
                                    var str = AdfFrequencyBandCommand + "INC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    freqBandDialSendCount++;
                                    Interlocked.Exchange(ref _adfFrequencyBandWaitingForFeedback, 1);
                                }
                                else if (_adfActiveFrequencyBand > desiredFreqBandDialPos)
                                {
                                    var str = AdfFrequencyBandCommand + "DEC\n";
                                    Common.DebugP("Sending " + str);
                                    DCSBIOS.Send(str);
                                    freqBandDialSendCount++;
                                    Interlocked.Exchange(ref _adfFrequencyBandWaitingForFeedback, 1);
                                }
                                Reset(ref freqBandDialTimeout);
                            }
                        }
                        else
                        {
                            freqBandDialOkTime = DateTime.Now.Ticks;
                        }


                        if (freqBandDialSendCount > 3)
                        {
                            //"Race" condition detected?
                            freqBandDialSendCount = 0;
                            Thread.Sleep(5000);
                        }
                        Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS


                        if (once)
                        {
                            UpdateActiveAdfFrequency(_adfActiveFrequencyRaw);
                            once = false;
                        }
                    }
                    while (IsTooShort(freqBandDialOkTime));
                }
                finally
                {
                    Interlocked.Exchange(ref _adfThreadNowSynching, 0);
                }
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Common.LogError(564523, ex);
            }
        }

        private void SendUhfPresetChannelChangeToDCSBIOS()
        {
            try
            {
                //Preset Channel selector
                //Pos    1 - 20
                lock (_lockUhfPresetChannelObject)
                {
                    switch (_uhfActivePresetChannel)
                    {
                        case 1:
                            {
                                _uhfIncreasePresetChannel = true;
                                break;
                            }
                        case 20:
                            {
                                _uhfIncreasePresetChannel = false;
                                break;
                            }
                    }
                }
                if (_uhfIncreasePresetChannel)
                {
                    DCSBIOS.Send(UhfPresetDialCommandInc);
                }
                else
                {
                    DCSBIOS.Send(UhfPresetDialCommandDec);
                }

            }
            catch (Exception ex)
            {
                Common.LogError(564523, ex);
            }
        }

        private void CheckFrequenciesForValidity()
        {
            //Crude fix if any freqs are outside the valid boundaries

            //VHF COMM
            //116.00 - 149.975
            if (_vhfCommBigFrequencyStandby < 116)
            {
                _vhfCommBigFrequencyStandby = 116;
            }
            if (_vhfCommBigFrequencyStandby > 151)
            {
                _vhfCommBigFrequencyStandby = 151;
            }

            //todo
        }

        private string GetVhfCommSmallFreqString()
        {
            if (_vhfCommSmallFrequencyStandby < 10)
            {
                return "0" + _vhfCommSmallFrequencyStandby;
            }
            return _vhfCommSmallFrequencyStandby.ToString();
        }

        private void ShowFrequenciesOnPanel()
        {
            if (!FirstReportHasBeenRead)
            {
                return;
            }
            CheckFrequenciesForValidity();

            /*
                1 byte (header byte 0x0) [0]
                5 bytes upper left LCD   [1 - 5]
                5 bytes upper right LCD  [6 - 10]
                5 bytes lower left LCD   [11- 15]
                5 bytes lower right LCD  [16- 20]

                0x01 - 0x09 displays the figure 1-9
                0xD1 - 0xD9 displays the figure 1.-9. (figure followed by dot)
                0xFF -> blank, nothing is shown in that spot.
             */
            var bytes = new byte[21];
            bytes[0] = 0x0;

            switch (_currentUpperRadioMode)
            {
                case CurrentUH1HRadioMode.INTERCOMM:
                    {
                        lock (_lockIntercommDialObject)
                        {
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_uhfActivePresetChannel), PZ69LCDPosition.UPPER_RIGHT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, _intercommActiveDial1Pos, PZ69LCDPosition.UPPER_LEFT);
                        }
                        break;
                    }
                case CurrentUH1HRadioMode.VHFCOMM:
                    {
                        lock (_lockVhfCommDialsObject1)
                        {
                            lock (_lockVhfCommDialsObject2)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, _vhfCommActiveFrequency, PZ69LCDPosition.UPPER_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_vhfCommBigFrequencyStandby + "." + GetVhfCommSmallFreqString(), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_RIGHT);
                            }
                        }
                        break;
                    }
                case CurrentUH1HRadioMode.UHF:
                    {
                        lock (_lockUhfDialsObject1)
                        {
                            lock (_lockUhfDialsObject2)
                            {
                                lock (_lockUhfDialsObject3)
                                {
                                    //251.75
                                    var filler = "";
                                    if (_uhfActiveDial3Frequency < 10)
                                    {
                                        filler = "0";
                                    }
                                    var fillerUhf = "";
                                    if (_uhfSmallFrequencyStandby < 10)
                                    {
                                        fillerUhf = "0";
                                    }
                                    var lcdFrequencyActive = Double.Parse(_uhfActiveDial1Frequency.ToString() + _uhfActiveDial2Frequency.ToString() + "." + filler + _uhfActiveDial3Frequency.ToString(), NumberFormatInfoFullDisplay);
                                    var lcdFrequencyStandby = Double.Parse(_uhfBigFrequencyStandby.ToString() + "." + fillerUhf + _uhfSmallFrequencyStandby.ToString(), NumberFormatInfoFullDisplay);
                                    SetPZ69DisplayBytesDefault(ref bytes, lcdFrequencyActive, PZ69LCDPosition.UPPER_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, lcdFrequencyStandby, PZ69LCDPosition.UPPER_RIGHT);
                                }
                            }
                        }
                        break;
                    }
                case CurrentUH1HRadioMode.VHFNAV:
                    {
                        lock (_lockVhfNavDialsObject1)
                        {
                            lock (_lockVhfNavDialsObject2)
                            {
                                //107.75
                                var filler = "";
                                if (_vhfNavActiveDial2Frequency < 10)
                                {
                                    filler = "0";
                                }
                                var fillerVhfNav = "";
                                if (_vhfNavSmallFrequencyStandby < 10)
                                {
                                    fillerVhfNav = "0";
                                }
                                var lcdFrequencyActive = Double.Parse(_vhfNavActiveDial1Frequency.ToString() + "." + filler + _vhfNavActiveDial2Frequency.ToString(), NumberFormatInfoFullDisplay);
                                var lcdFrequencyStandby = Double.Parse(_vhfNavBigFrequencyStandby.ToString() + "." + fillerVhfNav + _vhfNavSmallFrequencyStandby.ToString(), NumberFormatInfoFullDisplay);
                                SetPZ69DisplayBytesDefault(ref bytes, lcdFrequencyActive, PZ69LCDPosition.UPPER_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, lcdFrequencyStandby, PZ69LCDPosition.UPPER_RIGHT);

                            }
                        }
                        break;
                    }
                case CurrentUH1HRadioMode.VHFFM:
                    {
                        //Mhz   30-75
                        //Khz   0 - 95
                        lock (_lockVhfFmDialsObject1)
                        {
                            lock (_lockVhfFmDialsObject2)
                            {
                                lock (_lockVhfFmDialsObject3)
                                {
                                    lock (_lockVhfFmDialsObject4)
                                    {
                                        var activeFrequencyAsString = (_vhfFmActiveFreq1DialPos + 3).ToString() + _vhfFmActiveFreq2DialPos.ToString() + "." + _vhfFmActiveFreq3DialPos.ToString() + (_vhfFmActiveFreq4DialPos == 0 ? "0" : "5");
                                        //Common.DebugP("VHF FM activeFrequencyAsString = " + activeFrequencyAsString);
                                        var lcdFrequencyActive = Double.Parse(activeFrequencyAsString, NumberFormatInfoFullDisplay);
                                        //Common.DebugP("VHF FM _lcdFrequencyActiveUpper = " + _lcdFrequencyActiveUpper.ToString("0.00", NumberFormatInfoDefault));
                                        var fillerVhfFm = "";
                                        if (_vhfFmSmallFrequencyStandby < 10)
                                        {
                                            fillerVhfFm = "0";
                                        }
                                        var lcdFrequencyStandby = Double.Parse(_vhfFmBigFrequencyStandby + "." + fillerVhfFm + _vhfFmSmallFrequencyStandby, NumberFormatInfoFullDisplay);
                                        SetPZ69DisplayBytes(ref bytes, lcdFrequencyActive, 2, PZ69LCDPosition.UPPER_LEFT);
                                        SetPZ69DisplayBytes(ref bytes, lcdFrequencyStandby, 2, PZ69LCDPosition.UPPER_RIGHT);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case CurrentUH1HRadioMode.ADF:
                    {
                        lock (_lockAdfActiveFrequencyObject)
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, _adfActiveFrequency, PZ69LCDPosition.UPPER_LEFT);
                        }
                        lock (_lockAdfSignalStrengthObject)
                        {
                            SetPZ69DisplayBytesInteger(ref bytes, Convert.ToInt32(Math.Truncate(_adfSignalStrength)), PZ69LCDPosition.UPPER_RIGHT);
                        }
                        break;
                    }
            }
            switch (_currentLowerRadioMode)
            {
                case CurrentUH1HRadioMode.INTERCOMM:
                    {
                        lock (_lockIntercommDialObject)
                        {
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_uhfActivePresetChannel), PZ69LCDPosition.LOWER_RIGHT);
                            SetPZ69DisplayBytesUnsignedInteger(ref bytes, _intercommActiveDial1Pos, PZ69LCDPosition.LOWER_LEFT);
                        }
                        break;
                    }
                case CurrentUH1HRadioMode.VHFCOMM:
                    {
                        lock (_lockVhfCommDialsObject1)
                        {
                            lock (_lockVhfCommDialsObject2)
                            {
                                SetPZ69DisplayBytesDefault(ref bytes, _vhfCommActiveFrequency, PZ69LCDPosition.LOWER_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_vhfCommBigFrequencyStandby + "." + GetVhfCommSmallFreqString(), NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_RIGHT);
                            }
                        }
                        break;
                    }
                case CurrentUH1HRadioMode.UHF:
                    {
                        lock (_lockUhfDialsObject1)
                        {
                            lock (_lockUhfDialsObject2)
                            {
                                lock (_lockUhfDialsObject3)
                                {
                                    //251.75
                                    var filler = "";
                                    if (_uhfActiveDial3Frequency < 10)
                                    {
                                        filler = "0";
                                    }
                                    var fillerUhf = "";
                                    if (_uhfSmallFrequencyStandby < 10)
                                    {
                                        fillerUhf = "0";
                                    }
                                    var lcdFrequencyActive = Double.Parse(_uhfActiveDial1Frequency.ToString() + _uhfActiveDial2Frequency.ToString() + "." + filler + _uhfActiveDial3Frequency.ToString(), NumberFormatInfoFullDisplay);
                                    var lcdFrequencyStandby = Double.Parse(_uhfBigFrequencyStandby.ToString() + "." + fillerUhf + _uhfSmallFrequencyStandby.ToString(), NumberFormatInfoFullDisplay);
                                    SetPZ69DisplayBytesDefault(ref bytes, lcdFrequencyActive, PZ69LCDPosition.LOWER_LEFT);
                                    SetPZ69DisplayBytesDefault(ref bytes, lcdFrequencyStandby, PZ69LCDPosition.LOWER_RIGHT);
                                }
                            }
                        }
                        break;
                    }
                case CurrentUH1HRadioMode.VHFNAV:
                    {
                        lock (_lockVhfNavDialsObject1)
                        {
                            lock (_lockVhfNavDialsObject2)
                            {
                                //107.75
                                var filler = "";
                                if (_vhfNavActiveDial2Frequency < 10)
                                {
                                    filler = "0";
                                }
                                var fillerVhfNav = "";
                                if (_vhfNavSmallFrequencyStandby < 10)
                                {
                                    fillerVhfNav = "0";
                                }
                                var lcdFrequencyActive = Double.Parse(_vhfNavActiveDial1Frequency.ToString() + "." + filler + _vhfNavActiveDial2Frequency.ToString(), NumberFormatInfoFullDisplay);
                                var lcdFrequencyStandby = Double.Parse(_vhfNavBigFrequencyStandby.ToString() + "." + fillerVhfNav + _vhfNavSmallFrequencyStandby.ToString(), NumberFormatInfoFullDisplay);
                                SetPZ69DisplayBytesDefault(ref bytes, lcdFrequencyActive, PZ69LCDPosition.LOWER_LEFT);
                                SetPZ69DisplayBytesDefault(ref bytes, lcdFrequencyStandby, PZ69LCDPosition.LOWER_RIGHT);
                            }
                        }
                        break;
                    }
                case CurrentUH1HRadioMode.VHFFM:
                    {
                        //Mhz   30-75
                        //Khz   0 - 95
                        lock (_lockVhfFmDialsObject1)
                        {
                            lock (_lockVhfFmDialsObject2)
                            {
                                lock (_lockVhfFmDialsObject3)
                                {
                                    lock (_lockVhfFmDialsObject4)
                                    {
                                        var activeFrequencyAsString = (_vhfFmActiveFreq1DialPos + 3).ToString() + _vhfFmActiveFreq2DialPos.ToString() + "." + _vhfFmActiveFreq3DialPos.ToString() + (_vhfFmActiveFreq4DialPos == 0 ? "0" : "5");
                                        //Common.DebugP("VHF FM activeFrequencyAsString = " + activeFrequencyAsString);
                                        var lcdFrequencyActive = Double.Parse(activeFrequencyAsString, NumberFormatInfoFullDisplay);
                                        //Common.DebugP("VHF FM _lcdFrequencyActiveUpper = " + _lcdFrequencyActiveUpper.ToString("0.00", NumberFormatInfoDefault));
                                        var fillerVhfFm = "";
                                        if (_vhfFmSmallFrequencyStandby < 10)
                                        {
                                            fillerVhfFm = "0";
                                        }
                                        var lcdFrequencyStandby = Double.Parse(_vhfFmBigFrequencyStandby + "." + fillerVhfFm + _vhfFmSmallFrequencyStandby, NumberFormatInfoFullDisplay);
                                        SetPZ69DisplayBytes(ref bytes, lcdFrequencyActive, 2, PZ69LCDPosition.LOWER_LEFT);
                                        SetPZ69DisplayBytes(ref bytes, lcdFrequencyStandby, 2, PZ69LCDPosition.LOWER_RIGHT);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case CurrentUH1HRadioMode.ADF:
                    {
                        lock (_lockAdfActiveFrequencyObject)
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, _adfActiveFrequency, PZ69LCDPosition.LOWER_LEFT);
                        }
                        lock (_lockAdfSignalStrengthObject)
                        {
                            SetPZ69DisplayBytesInteger(ref bytes, Convert.ToInt32(Math.Truncate(_adfSignalStrength)), PZ69LCDPosition.LOWER_RIGHT);
                        }
                        break;
                    }
            }
            SendLCDData(bytes);
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {

            if (SkipCurrentFrequencyChange())
            {
                return;
            }

            foreach (var o in hashSet)
            {
                var radioPanelKnobUh1H = (RadioPanelKnobUH1H)o;
                if (radioPanelKnobUh1H.IsOn)
                {
                    switch (radioPanelKnobUh1H.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsUH1H.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentUH1HRadioMode.INTERCOMM:
                                        {
                                            if (!SkipIntercomm())
                                            {
                                                DCSBIOS.Send(IntercommVolumeKnobCommandDec);
                                            }
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFCOMM:
                                        {
                                            //116-149
                                            if (_vhfCommBigFrequencyStandby.Equals(149))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _vhfCommBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.UHF:
                                        {
                                            //225-399
                                            if (_uhfBigFrequencyStandby.Equals(399))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFFM:
                                        {
                                            //30-75
                                            if (_vhfFmBigFrequencyStandby.Equals(75))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _vhfFmBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFNAV:
                                        {
                                            if (_vhfNavBigFrequencyStandby.Equals(126))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _vhfNavBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.ADF:
                                        {
                                            DCSBIOS.Send(AdfTuneKnobCommandInc);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentUH1HRadioMode.INTERCOMM:
                                        {
                                            if (!SkipIntercomm())
                                            {
                                                DCSBIOS.Send(IntercommVolumeKnobCommandInc);
                                            }
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFCOMM:
                                        {
                                            //116-149
                                            if (_vhfCommBigFrequencyStandby.Equals(116))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _vhfCommBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.UHF:
                                        {
                                            if (_uhfBigFrequencyStandby.Equals(225))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFFM:
                                        {
                                            //30-75
                                            if (_vhfFmBigFrequencyStandby.Equals(30))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _vhfFmBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFNAV:
                                        {
                                            if (_vhfNavBigFrequencyStandby.Equals(107))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _vhfNavBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.ADF:
                                        {
                                            DCSBIOS.Send(AdfTuneKnobCommandDec);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentUH1HRadioMode.INTERCOMM:
                                        {
                                            if (!SkipIntercomm())
                                            {
                                                DCSBIOS.Send(IntercommDialCommandInc);
                                            }
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFCOMM:
                                        {
                                            //Small dial 000 to 975 (x00 && x25 --> x00,     x50 && x75 --> 0x50)
                                            if (_vhfCommSmallFrequencyStandby >= 95)
                                            {
                                                //At max value
                                                _vhfCommSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _vhfCommSmallFrequencyStandby += 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.UHF:
                                        {
                                            //Small dial 0.000 0.025 0.050 0.075 [only 0.00 and 0.05 are used]
                                            if (_uhfSmallFrequencyStandby >= 95)
                                            {
                                                //At max value
                                                _uhfSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby = _uhfSmallFrequencyStandby + 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFFM:
                                        {
                                            if (_vhfFmSmallFrequencyStandby >= 95)
                                            {
                                                //At max value
                                                _vhfFmSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _vhfFmSmallFrequencyStandby += 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFNAV:
                                        {
                                            if (_vhfNavSmallFrequencyStandby >= 95)
                                            {
                                                //At max value
                                                _vhfNavSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _vhfNavSmallFrequencyStandby += 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.ADF:
                                        {
                                            DCSBIOS.Send(AdfGainKnobCommandInc);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentUH1HRadioMode.INTERCOMM:
                                        {
                                            if (!SkipIntercomm())
                                            {
                                                DCSBIOS.Send(IntercommDialCommandDec);
                                            }
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFCOMM:
                                        {
                                            //Small dial 000 to 975 (x00 x25 x50 x75)
                                            if (_vhfCommSmallFrequencyStandby <= 0)
                                            {
                                                //At min value
                                                _vhfCommSmallFrequencyStandby = 95;
                                                break;
                                            }
                                            _vhfCommSmallFrequencyStandby -= 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.UHF:
                                        {
                                            if (_uhfSmallFrequencyStandby <= 0)
                                            {
                                                //At min value
                                                _uhfSmallFrequencyStandby = 95;
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby = _uhfSmallFrequencyStandby - 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFFM:
                                        {
                                            if (_vhfFmSmallFrequencyStandby <= 0)
                                            {
                                                //At min value
                                                _vhfFmSmallFrequencyStandby = 95;
                                                break;
                                            }
                                            _vhfFmSmallFrequencyStandby -= 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFNAV:
                                        {
                                            if (_vhfNavSmallFrequencyStandby <= 0)
                                            {
                                                //At min value
                                                _vhfNavSmallFrequencyStandby = 95;
                                                break;
                                            }
                                            _vhfNavSmallFrequencyStandby -= 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.ADF:
                                        {
                                            DCSBIOS.Send(AdfGainKnobCommandDec);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentUH1HRadioMode.INTERCOMM:
                                        {
                                            if (!SkipIntercomm())
                                            {
                                                DCSBIOS.Send(IntercommVolumeKnobCommandDec);
                                            }
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFCOMM:
                                        {
                                            //116-149
                                            if (_vhfCommBigFrequencyStandby.Equals(149))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _vhfCommBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.UHF:
                                        {
                                            //225-399
                                            if (_uhfBigFrequencyStandby.Equals(399))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFFM:
                                        {
                                            //30-75
                                            if (_vhfFmBigFrequencyStandby.Equals(75))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _vhfFmBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFNAV:
                                        {
                                            if (_vhfNavBigFrequencyStandby.Equals(126))
                                            {
                                                //@ max value
                                                break;
                                            }
                                            _vhfNavBigFrequencyStandby++;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.ADF:
                                        {
                                            DCSBIOS.Send(AdfTuneKnobCommandInc);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentUH1HRadioMode.INTERCOMM:
                                        {
                                            if (!SkipIntercomm())
                                            {
                                                DCSBIOS.Send(IntercommVolumeKnobCommandInc);
                                            }
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFCOMM:
                                        {
                                            //116-149
                                            if (_vhfCommBigFrequencyStandby.Equals(116))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _vhfCommBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.UHF:
                                        {
                                            if (_uhfBigFrequencyStandby.Equals(225))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _uhfBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFFM:
                                        {
                                            //30-75
                                            if (_vhfFmBigFrequencyStandby.Equals(30))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _vhfFmBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFNAV:
                                        {
                                            if (_vhfNavBigFrequencyStandby.Equals(107))
                                            {
                                                //@ min value
                                                break;
                                            }
                                            _vhfNavBigFrequencyStandby--;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.ADF:
                                        {
                                            DCSBIOS.Send(AdfTuneKnobCommandDec);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentUH1HRadioMode.INTERCOMM:
                                        {
                                            if (!SkipIntercomm())
                                            {
                                                DCSBIOS.Send(IntercommDialCommandInc);
                                            }
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFCOMM:
                                        {
                                            //Small dial 000 to 975 (x00 x25 x50 x75)
                                            if (_vhfCommSmallFrequencyStandby >= 95)
                                            {
                                                //At max value
                                                _vhfCommSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _vhfCommSmallFrequencyStandby += 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.UHF:
                                        {
                                            //Small dial 0.000 0.025 0.050 0.075 [only 0.00 and 0.05 are used]
                                            if (_uhfSmallFrequencyStandby >= 95)
                                            {
                                                //At max value
                                                _uhfSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby = _uhfSmallFrequencyStandby + 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFFM:
                                        {
                                            if (_vhfFmSmallFrequencyStandby >= 95)
                                            {
                                                //At max value
                                                _vhfFmSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _vhfFmSmallFrequencyStandby += 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFNAV:
                                        {
                                            if (_vhfNavSmallFrequencyStandby >= 95)
                                            {
                                                //At max value
                                                _vhfNavSmallFrequencyStandby = 0;
                                                break;
                                            }
                                            _vhfNavSmallFrequencyStandby += 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.ADF:
                                        {
                                            DCSBIOS.Send(AdfGainKnobCommandInc);
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentUH1HRadioMode.INTERCOMM:
                                        {
                                            if (!SkipIntercomm())
                                            {
                                                DCSBIOS.Send(IntercommDialCommandDec);
                                            }
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFCOMM:
                                        {
                                            //Small dial 000 to 975 (x00 x25 x50 x75)
                                            if (_vhfCommSmallFrequencyStandby <= 0)
                                            {
                                                //At min value
                                                _vhfCommSmallFrequencyStandby = 95;
                                                break;
                                            }
                                            _vhfCommSmallFrequencyStandby -= 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.UHF:
                                        {
                                            if (_uhfSmallFrequencyStandby <= 0)
                                            {
                                                //At min value
                                                _uhfSmallFrequencyStandby = 95;
                                                break;
                                            }
                                            _uhfSmallFrequencyStandby = _uhfSmallFrequencyStandby - 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFFM:
                                        {
                                            if (_vhfFmSmallFrequencyStandby <= 0)
                                            {
                                                //At min value
                                                _vhfFmSmallFrequencyStandby = 95;
                                                break;
                                            }
                                            _vhfFmSmallFrequencyStandby -= 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.VHFNAV:
                                        {
                                            if (_vhfNavSmallFrequencyStandby <= 0)
                                            {
                                                //At min value
                                                _vhfNavSmallFrequencyStandby = 95;
                                                break;
                                            }
                                            _vhfNavSmallFrequencyStandby -= 5;
                                            break;
                                        }
                                    case CurrentUH1HRadioMode.ADF:
                                        {
                                            DCSBIOS.Send(AdfGainKnobCommandDec);
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                }
            }
            ShowFrequenciesOnPanel();
        }

        private bool SkipIntercomm()
        {
            if (_currentUpperRadioMode == CurrentUH1HRadioMode.INTERCOMM || _currentLowerRadioMode == CurrentUH1HRadioMode.INTERCOMM)
            {
                if (_intercommSkipper > 1)
                {
                    _intercommSkipper = 0;
                    return false;
                }
                else
                {
                    _intercommSkipper++;
                    return true;
                }
            }
            return false;
        }

        public void PZ69KnobChanged(IEnumerable<object> hashSet)
        {
            lock (_lockLCDUpdateObject)
            {
                foreach (var radioPanelKnobObject in hashSet)
                {
                    var radioPanelKnob = (RadioPanelKnobUH1H)radioPanelKnobObject;

                    switch (radioPanelKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsUH1H.UPPER_INTERCOMM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentUH1HRadioMode.INTERCOMM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_VHFCOMM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentUH1HRadioMode.VHFCOMM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentUH1HRadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_VHFFM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentUH1HRadioMode.VHFFM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_VHFNAV:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentUH1HRadioMode.VHFNAV;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_ADF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentUpperRadioMode = CurrentUH1HRadioMode.ADF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_DME:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_INTERCOMM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentUH1HRadioMode.INTERCOMM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_VHFCOMM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentUH1HRadioMode.VHFCOMM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_UHF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentUH1HRadioMode.UHF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_VHFFM:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentUH1HRadioMode.VHFFM;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_VHFNAV:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentUH1HRadioMode.VHFNAV;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_ADF:
                            {
                                if (radioPanelKnob.IsOn)
                                {
                                    _currentLowerRadioMode = CurrentUH1HRadioMode.ADF;
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_DME:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.UPPER_FREQ_SWITCH:
                            {
                                if (_currentUpperRadioMode == CurrentUH1HRadioMode.ADF)
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SendAdfBandChangeToDCSBIOS();
                                    }
                                }
                                else
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsUH1H.UPPER_FREQ_SWITCH);
                                    }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsUH1H.LOWER_FREQ_SWITCH:
                            {
                                if (_currentLowerRadioMode == CurrentUH1HRadioMode.ADF)
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SendAdfBandChangeToDCSBIOS();
                                    }
                                }
                                else
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SendFrequencyToDCSBIOS(RadioPanelPZ69KnobsUH1H.LOWER_FREQ_SWITCH);
                                    }
                                }
                                break;
                            }
                    }


                }
                AdjustFrequency(hashSet);
            }
        }

        public override sealed void Startup()
        {
            try
            {
                StartupBase("UH-1H");

                NumberFormatInfoFullDisplay = new NumberFormatInfo();
                NumberFormatInfoFullDisplay.NumberDecimalSeparator = ".";
                NumberFormatInfoFullDisplay.NumberDecimalDigits = 4;
                NumberFormatInfoFullDisplay.NumberGroupSeparator = "";

                //VHF COMM
                _vhfCommDcsbiosOutputActiveFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFCOMM_FREQ");
                DCSBIOSStringListenerHandler.AddAddress(_vhfCommDcsbiosOutputActiveFrequency.Address, 7, this);

                //UHF
                _uhfDcsbiosOutputActivePresetChannel = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_PRESET");
                _uhfDcsbiosOutputActiveFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("UHF_FREQ");
                DCSBIOSStringListenerHandler.AddAddress(_uhfDcsbiosOutputActiveFrequency.Address, 6, this);

                //VHF NAV
                _vhfNavDcsbiosOutputActiveFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFNAV_FREQ");
                DCSBIOSStringListenerHandler.AddAddress(_vhfNavDcsbiosOutputActiveFrequency.Address, 6, this);

                //INTERCOMM
                _intercommDcsbiosOutputActivePos = DCSBIOSControlLocator.GetDCSBIOSOutput("INT_MODE");

                //VHF FM
                _vhfFmDcsbiosOutputFreqDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ1");
                _vhfFmDcsbiosOutputFreqDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ2");
                _vhfFmDcsbiosOutputFreqDial3 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ3");
                _vhfFmDcsbiosOutputFreqDial4 = DCSBIOSControlLocator.GetDCSBIOSOutput("VHFFM_FREQ4");

                //ADF (0-2)
                _adfDcsbiosOutputActiveFrequencyBand = DCSBIOSControlLocator.GetDCSBIOSOutput("ADF_BAND");
                _adfDcsbiosOutputActiveFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("ADF_FREQ");
                _adfDcsbiosOutputSignalStrength = DCSBIOSControlLocator.GetDCSBIOSOutput("ADF_SIGNAL");

                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP("RadioPanelPZ69UH1H.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                ShutdownBase();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }
        
        public override void ClearSettings()
        {
            //todo
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            //todo
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55();
            dcsOutputAndColorBinding.DCSBiosOutputLED = dcsBiosOutput;
            dcsOutputAndColorBinding.LEDColor = panelLEDColor;
            dcsOutputAndColorBinding.SaitekLEDPosition = saitekPanelLEDPosition;
            return dcsOutputAndColorBinding;
        }

        private void OnReport(HidReport report)
        {
            /*if (IsAttached == false)
            {
                return;
            }
            */
            if (report.Data.Length == 3)
            {
                Array.Copy(NewRadioPanelValue, OldRadioPanelValue, 3);
                Array.Copy(report.Data, NewRadioPanelValue, 3);
                var hashSet = GetHashSetOfChangedKnobs(OldRadioPanelValue, NewRadioPanelValue);
                PZ69KnobChanged(hashSet);
                OnSwitchesChanged(hashSet);
                FirstReportHasBeenRead = true;
                /*if (Common.Debug && 1 == 2)
                {
                    var stringBuilder = new StringBuilder();
                    for (var i = 0; i < report.Data.Length; i++)
                    {
                        stringBuilder.Append(Convert.ToString(report.Data[i], 2).PadLeft(8, '0') + "  ");
                    }
                    Common.DebugP(stringBuilder.ToString());
                    if (hashSet.Count > 0)
                    {
                        Common.DebugP("\nFollowing knobs has been changed:\n");
                        foreach (var radioPanelKnob in hashSet)
                        {
                            var knob = (RadioPanelKnobUH1H)radioPanelKnob;
                            Common.DebugP(knob.RadioPanelPZ69Knob + ", value is " + FlagValue(_newRadioPanelValue, (RadioPanelKnobUH1H)radioPanelKnob));
                        }
                    }
                }
                Common.DebugP("\r\nDone!\r\n");*/
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

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            //Common.DebugP("Old: " + Convert.ToString(oldValue[0], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[1], 2).PadLeft(8, '0') + " " + Convert.ToString(oldValue[2], 2).PadLeft(8, '0'));
            //Common.DebugP("New: " + Convert.ToString(newValue[0], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[1], 2).PadLeft(8, '0') + " " + Convert.ToString(newValue[2], 2).PadLeft(8, '0'));
            for (var i = 0; i < 3; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var radioPanelKnob in _radioPanelKnobs)
                {
                    if (radioPanelKnob.Group == i && (FlagHasChanged(oldByte, newByte, radioPanelKnob.Mask) || !FirstReportHasBeenRead))
                    {
                        radioPanelKnob.IsOn = FlagValue(newValue, radioPanelKnob);
                        result.Add(radioPanelKnob);
                        //Common.DebugP("Following knob has changed : " + radioPanelKnob.RadioPanelPZ69Knob + " isOn? : " + radioPanelKnob.IsOn);
                    }
                }
            }
            return result;
        }

        private void CreateRadioKnobs()
        {
            _radioPanelKnobs = RadioPanelKnobUH1H.GetRadioPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, RadioPanelKnobUH1H radioPanelKnob)
        {
            return (currentValue[radioPanelKnob.Group] & radioPanelKnob.Mask) > 0;
        }

        private string GetCommandDirectionForVhfCommDial1(uint desiredFreq, uint actualFreq)
        {
            /*UH-1H AN/ARC-134 VHF Comm Radio Set*/
            //Large dial 116 - 149 [step of 1]
            var inc = "INC\n";
            var dec = "DEC\n";
            if (desiredFreq > actualFreq && desiredFreq - actualFreq >= 16)
            {
                return dec;
            }
            if (desiredFreq > actualFreq && desiredFreq - actualFreq < 16)
            {
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq >= 16)
            {
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq < 16)
            {
                return dec;
            }
            throw new Exception("Should reach this code. GetCommandDirectionForVhfCommDial1(int desiredFreq, uint actualFreq)) -> " + desiredFreq + "   " + actualFreq);
        }

        private string GetCommandDirectionForVhfCommDial2(uint desiredFreq, uint actualFreq)
        {
            /*UH-1H AN/ARC-134 VHF Comm Radio Set*/

            //Small dial 000 - 975 [step of 25]
            // 000 025 050 075 100 125 150 175 200 225 250 275 300 325 350 375 400 425 450 475 500 525 550 575 600 625 650 675 700 725 750 775 800 825 850 875 900 925 950 975
            //  1   2   3   4   5   6   7   8   9   10  11  12  13  14  15  16  17  18  19  *20*  21  22  23  24  25  26  27  28  29  30  31  32  33  34  35  36  37  38  39  40
            //  
            // Only these are used because of PZ69 limitations
            // 00 05 10 15 20 25 30 35 40 45 50 55 60 65 70 75 80 85 90 95
            //  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20   

            var breakValue = 50;
            var inc = "INC\n";
            var dec = "DEC\n";
            if (desiredFreq > actualFreq && desiredFreq - actualFreq >= breakValue)
            {
                //Common.DebugP("A Dial2 command will be dec desiredFreq = " + desiredFreq + " actualFreq = " + actualFreq + " desiredFreq - actualFreq -> " + (desiredFreq - actualFreq) + " >= " + breakValue);
                return dec;
            }
            if (desiredFreq > actualFreq && desiredFreq - actualFreq < breakValue)
            {
                //Common.DebugP("B Dial2 command will be inc desiredFreq = " + desiredFreq + " actualFreq = " + actualFreq + " desiredFreq - actualFreq -> " + (desiredFreq - actualFreq) + " < " + breakValue);
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq >= breakValue)
            {
                //Common.DebugP("C Dial2 command will be inc desiredFreq = " + desiredFreq + " actualFreq = " + actualFreq + " actualFreq - desiredFreq -> " + (actualFreq - desiredFreq) + " >= " + breakValue);
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq < breakValue)
            {
                //Common.DebugP("D Dial2 command will be dec desiredFreq = " + desiredFreq + " actualFreq = " + actualFreq + " actualFreq - desiredFreq -> " + (actualFreq - desiredFreq) + " < " + breakValue);
                return dec;
            }
            throw new Exception("Should reach this code. GetCommandDirectionForVhfCommDial2(int desiredFreq, uint actualFreq)) -> " + desiredFreq + "   " + actualFreq);
        }

        private string GetCommandDirectionForUhfDial1(uint desiredFreq, uint actualFreq)
        {
            //Large dial 20 - 39 [step of 1]
            // d19 +/-10
            var inc = "INC\n";
            var dec = "DEC\n";
            if (desiredFreq > actualFreq && desiredFreq - actualFreq >= 10)
            {
                return dec;
            }
            if (desiredFreq > actualFreq && desiredFreq - actualFreq < 10)
            {
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq >= 10)
            {
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq < 10)
            {
                return dec;
            }
            throw new Exception("Should reach this code. GetCommandDirectionForUhfDial1(int desiredFreq, uint actualFreq)) -> " + desiredFreq + "   " + actualFreq);
        }

        private string GetCommandDirectionForUhfDial2(uint desiredFreq, uint actualFreq)
        {
            //2nd dial 0 - 9 [step of 1]
            // +/-9
            var inc = "INC\n";
            var dec = "DEC\n";
            if (desiredFreq > actualFreq && desiredFreq - actualFreq >= 5)
            {
                return dec;
            }
            if (desiredFreq > actualFreq && desiredFreq - actualFreq < 5)
            {
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq >= 5)
            {
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq < 5)
            {
                return dec;
            }
            throw new Exception("Should reach this code. GetCommandDirectionForUhfDial2(int desiredFreq, uint actualFreq)) -> " + desiredFreq + "   " + actualFreq);
        }

        private string GetCommandDirectionForUhfDial3(uint desiredFreq, uint actualFreq)
        {
            // 00 05 10 15 20 25 30 35 40 45 50 55 60 65 70 75 80 85 90 95
            //  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20   

            var breakValue = 50;
            var inc = "INC\n";
            var dec = "DEC\n";
            if (desiredFreq > actualFreq && desiredFreq - actualFreq >= breakValue)
            {
                //Common.DebugP("A Dial3 command will be dec desiredFreq = " + desiredFreq + " actualFreq = " + actualFreq + " desiredFreq - actualFreq -> " + (desiredFreq - actualFreq) + " >= " + breakValue);
                return dec;
            }
            if (desiredFreq > actualFreq && desiredFreq - actualFreq < breakValue)
            {
                //Common.DebugP("B Dial3 command will be inc desiredFreq = " + desiredFreq + " actualFreq = " + actualFreq + " desiredFreq - actualFreq -> " + (desiredFreq - actualFreq) + " < " + breakValue);
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq >= breakValue)
            {
                //Common.DebugP("C Dial3 command will be inc desiredFreq = " + desiredFreq + " actualFreq = " + actualFreq + " actualFreq - desiredFreq -> " + (actualFreq - desiredFreq) + " >= " + breakValue);
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq < breakValue)
            {
                //Common.DebugP("D Dial3 command will be dec desiredFreq = " + desiredFreq + " actualFreq = " + actualFreq + " actualFreq - desiredFreq -> " + (actualFreq - desiredFreq) + " < " + breakValue);
                return dec;
            }
            throw new Exception("Should reach this code. GetCommandDirectionForUhfDial3(int desiredFreq, uint actualFreq)) -> " + desiredFreq + "   " + actualFreq);
        }

        private string GetCommandDirectionForVhfNavDial1(uint desiredFreq, uint actualFreq)
        {
            /*UH-1H AN/ARC-134 VHF Comm Radio Set*/
            //Large dial 107-126  [step of 1]
            var inc = "INC\n";
            var dec = "DEC\n";
            if (desiredFreq > actualFreq && desiredFreq - actualFreq >= 10)
            {
                return dec;
            }
            if (desiredFreq > actualFreq && desiredFreq - actualFreq < 10)
            {
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq >= 10)
            {
                return inc;
            }
            if (desiredFreq < actualFreq && actualFreq - desiredFreq < 10)
            {
                return dec;
            }
            throw new Exception("Should reach this code. GetCommandDirectionForVhfNavDial1(int desiredFreq, uint actualFreq)) -> " + desiredFreq + "   " + actualFreq);
        }

        /*
        private void SaveActiveIntercomm()
        {
            lock (_lockIntercommDialObject)
            {
                _intercommSavedActiveDialPosition = _intercommActiveDial1Pos;
            }
        }

        private void SwapActiveIntercomm()
        {
            _intercommDialPosStandby = _intercommSavedActiveDialPosition;
        }
        */

        private void SaveActiveFrequencyVhfComm()
        {
            lock (_lockVhfCommDialsObject1)
            {
                lock (_lockVhfCommDialsObject2)
                {
                    _vhfCommSavedActiveBigFrequency = _vhfCommActiveDial1Frequency;
                    _vhfCommSavedActiveSmallFrequency = _vhfCommActiveDial2Frequency;
                }
            }
        }

        private void SwapActiveStandbyFrequencyVhfComm()
        {
            _vhfCommBigFrequencyStandby = _vhfCommSavedActiveBigFrequency;
            _vhfCommSmallFrequencyStandby = _vhfCommSavedActiveSmallFrequency;
        }

        private void SaveActiveFrequencyUhf()
        {
            lock (_lockUhfDialsObject1)
            {
                lock (_lockUhfDialsObject2)
                {
                    lock (_lockUhfDialsObject3)
                    {
                        _uhfSavedActiveBigFrequency = uint.Parse(_uhfActiveDial1Frequency.ToString() + _uhfActiveDial2Frequency.ToString());
                        _uhfSavedActiveSmallFrequency = _uhfActiveDial3Frequency;
                    }
                }
            }
        }

        private void SwapActiveStandbyFrequencyUhf()
        {
            _uhfBigFrequencyStandby = _uhfSavedActiveBigFrequency;
            _uhfSmallFrequencyStandby = _uhfSavedActiveSmallFrequency;
        }

        private void SaveActiveFrequencyVhfFm()
        {
            lock (_lockVhfFmDialsObject1)
            {
                lock (_lockVhfFmDialsObject2)
                {
                    lock (_lockVhfFmDialsObject3)
                    {
                        lock (_lockVhfFmDialsObject4)
                        {
                            uint dial4 = 0;
                            switch (_vhfFmActiveFreq4DialPos)
                            {
                                case 0:
                                    {
                                        dial4 = 0;
                                        break;
                                    }
                                case 1:
                                    {
                                        dial4 = 5;
                                        break;
                                    }
                            }
                            _vhfFmSavedActiveBigFrequency = uint.Parse((_vhfFmActiveFreq1DialPos + 3).ToString() + _vhfFmActiveFreq2DialPos.ToString());
                            _vhfFmSavedActiveSmallFrequency = uint.Parse(_vhfFmActiveFreq3DialPos.ToString() + dial4.ToString());
                            Common.DebugP("_vhfFmSavedActiveBigFrequency : " + _vhfFmSavedActiveBigFrequency);
                            Common.DebugP("_vhfFmSavedActiveSmallFrequency : " + _vhfFmSavedActiveSmallFrequency);
                        }
                    }
                }
            }
        }

        private void SwapActiveStandbyFrequencyVhfFm()
        {
            _vhfFmBigFrequencyStandby = _vhfFmSavedActiveBigFrequency;
            _vhfFmSmallFrequencyStandby = _vhfFmSavedActiveSmallFrequency;
            Common.DebugP("_vhfFmBigFrequencyStandby : " + _vhfFmBigFrequencyStandby);
            Common.DebugP("_vhfFmSmallFrequencyStandby : " + _vhfFmSmallFrequencyStandby);
        }

        private void SaveActiveFrequencyVhfNav()
        {
            lock (_lockVhfNavDialsObject1)
            {
                lock (_lockVhfNavDialsObject2)
                {
                    _vhfNavSavedActiveBigFrequency = _vhfNavActiveDial1Frequency;
                    _vhfNavSavedActiveSmallFrequency = _vhfNavActiveDial2Frequency;
                }
            }
        }

        private void SwapActiveStandbyFrequencyVhfNav()
        {
            _vhfNavBigFrequencyStandby = _vhfNavSavedActiveBigFrequency;
            _vhfNavSmallFrequencyStandby = _vhfNavSavedActiveSmallFrequency;
        }

        private bool IntercommSyncing()
        {
            return Interlocked.Read(ref _intercommThreadNowSynching) > 0;
        }

        private bool VhfCommSyncing()
        {
            return Interlocked.Read(ref _vhfCommThreadNowSynching) > 0;
        }

        private bool VhfFmSyncing()
        {
            return Interlocked.Read(ref _vhfFmThreadNowSynching) > 0;
        }

        private bool UhfSyncing()
        {
            return Interlocked.Read(ref _uhfThreadNowSynching) > 0;
        }

        private bool VhfNavSyncing()
        {
            return Interlocked.Read(ref _vhfNavThreadNowSynching) > 0;
        }

    }
}

