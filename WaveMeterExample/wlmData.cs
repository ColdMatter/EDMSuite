using System.Runtime.InteropServices;

// a namespace with a "WLM" class covering the header of the wlmData.dll since headers, includes and macros are not supported by C#
namespace wlmData
{
    unsafe public class WLM
    {

        // ***********  Functions for general usage  ****************************

        unsafe public delegate void CallbackProc(int Mode, int IntVal, double DblVal) ;
        unsafe public delegate void CallbackProcEx(int Ver, int Mode, int IntVal, double DblVal, int Res1) ;

        [DllImport("wlmData.dll")]  public static extern int    Instantiate(int RFC, int Mode, CallbackProcEx P1, int P2) ;
        [DllImport("wlmData.dll")]  public static extern int    Instantiate(int RFC, int Mode, long P1, int P2) ;
        [DllImport("wlmData.dll")]  public static extern int    WaitForWLMEvent(int* Mode, int* IntVal, double* DblVal) ;
        [DllImport("wlmData.dll")]  public static extern int    WaitForWLMEventEx(int* Ver, int *Mode, int *IntVal, double *DblVal, int *Res1) ;
        [DllImport("wlmData.dll")]  public static extern int    WaitForNextWLMEvent(int *Mode, int *IntVal, double *DblVal) ;
        [DllImport("wlmData.dll")]  public static extern int    WaitForNextWLMEventEx(int* Ver, int *Mode, int *IntVal, double *DblVal, int *Res1) ;
        [DllImport("wlmData.dll")]  public static extern void   ClearWLMEvents() ;

        [DllImport("wlmData.dll")]  public static extern int    ControlWLM(int Action, long App, int Ver) ;
        [DllImport("wlmData.dll")]  public static extern int    ControlWLMEx(int Action, long App, int Ver, int Delay, int Res) ;
        [DllImport("wlmData.dll")]  public static extern System.Int64  SynchroniseWLM(int Mode, System.Int64 TS) ;
        [DllImport("wlmData.dll")]  public static extern int    SetMeasurementDelayMethod(int Mode, int Delay) ;
        [DllImport("wlmData.dll")]  public static extern int    SetWLMPriority(int PPC, int Res1, int Res2) ;
        [DllImport("wlmData.dll")]  public static extern int    PresetWLMIndex(int Ver) ;

        [DllImport("wlmData.dll")]  public static extern int    GetWLMVersion(int Ver) ;
        [DllImport("wlmData.dll")]  public static extern int    GetWLMIndex(int Ver) ;
        [DllImport("wlmData.dll")]  public static extern int    GetWLMCount(int V) ;


        // ***********  General Get... & Set...-functions  **********************
        [DllImport("wlmData.dll")]  public static extern double GetWavelength(double WL) ;
        [DllImport("wlmData.dll")]  public static extern double GetWavelength2(double WL2) ;
        [DllImport("wlmData.dll")]  public static extern double GetWavelengthNum(int num, double WL) ;
        [DllImport("wlmData.dll")]  public static extern double GetCalWavelength(int ba, double WL) ;
        [DllImport("wlmData.dll")]  public static extern double GetCalibrationEffect(double CE) ;
        [DllImport("wlmData.dll")]  public static extern double GetFrequency(double F) ;
        [DllImport("wlmData.dll")]  public static extern double GetFrequency2(double F2) ;
        [DllImport("wlmData.dll")]  public static extern double GetFrequencyNum(int num, double F) ;
        [DllImport("wlmData.dll")]  public static extern double GetLinewidth(int Index, double LW) ;
        [DllImport("wlmData.dll")]  public static extern double GetLinewidthNum(int num, double LW) ;
        [DllImport("wlmData.dll")]  public static extern double GetDistance(double D) ;
        [DllImport("wlmData.dll")]  public static extern double GetAnalogIn(double AI) ;
        [DllImport("wlmData.dll")]  public static extern double GetTemperature(double T) ;
        [DllImport("wlmData.dll")]  public static extern int    SetTemperature(double T) ;
        [DllImport("wlmData.dll")]  public static extern double GetPressure(double P) ;
        [DllImport("wlmData.dll")]  public static extern int    SetPressure(int Mode, double P) ;
        [DllImport("wlmData.dll")]  public static extern int    GetAirParameters(int Mode, int* State, double* Val) ;
        [DllImport("wlmData.dll")]  public static extern int    SetAirParameters(int Mode, int* State, double Val) ;
        [DllImport("wlmData.dll")]  public static extern double GetExternalInput(int Index, double I) ;
        [DllImport("wlmData.dll")]  public static extern int    SetExternalInput(int Index, double I) ;
        [DllImport("wlmData.dll")]  public static extern int    GetExtraSetting(int Index, int* lGet, double* dGet, char *sGet) ;
        [DllImport("wlmData.dll")]  public static extern int    SetExtraSetting(int Index, int lSet, double dSet, char *sSet) ;

        [DllImport("wlmData.dll")]  public static extern ushort GetExposure(ushort E) ;
        [DllImport("wlmData.dll")]  public static extern int    SetExposure(ushort E) ;
        [DllImport("wlmData.dll")]  public static extern ushort GetExposure2(ushort E2) ;
        [DllImport("wlmData.dll")]  public static extern int    SetExposure2(ushort E2) ;
        [DllImport("wlmData.dll")]  public static extern int    GetExposureNum(int num, int arr, int E) ;
        [DllImport("wlmData.dll")]  public static extern int    SetExposureNum(int num, int arr, int E) ;
        [DllImport("wlmData.dll")]  public static extern double GetExposureNumEx(int num, int arr, double E) ;
        [DllImport("wlmData.dll")]  public static extern int    SetExposureNumEx(int num, int arr, double E) ;
        [DllImport("wlmData.dll")]  public static extern bool   GetExposureMode(bool EM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetExposureMode(bool EM) ;
        [DllImport("wlmData.dll")]  public static extern int    GetExposureModeNum(int num, bool EM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetExposureModeNum(int num, bool EM) ;
        [DllImport("wlmData.dll")]  public static extern int    GetExposureRange(int ER) ;
        [DllImport("wlmData.dll")]  public static extern double GetExposureRangeEx(int ER) ;
        [DllImport("wlmData.dll")]  public static extern int    GetAutoExposureSetting(int num, int AES, int* iVal, double* dVal) ;
        [DllImport("wlmData.dll")]  public static extern int    SetAutoExposureSetting(int num, int AES, int iVal, double dVal) ;

        [DllImport("wlmData.dll")]  public static extern ushort GetResultMode(ushort RM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetResultMode(ushort RM) ;
        [DllImport("wlmData.dll")]  public static extern ushort GetRange(ushort R) ;
        [DllImport("wlmData.dll")]  public static extern int    SetRange(ushort R) ;
        [DllImport("wlmData.dll")]  public static extern ushort GetPulseMode(ushort PM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetPulseMode(ushort PM) ;
        [DllImport("wlmData.dll")]  public static extern int    GetPulseDelay(int PD) ;
        [DllImport("wlmData.dll")]  public static extern int    SetPulseDelay(int PD) ;
        [DllImport("wlmData.dll")]  public static extern ushort GetWideMode(ushort WM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetWideMode(ushort WM) ;

        [DllImport("wlmData.dll")]  public static extern int    GetDisplayMode(int DM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetDisplayMode(int DM) ;
        [DllImport("wlmData.dll")]  public static extern bool   GetFastMode(bool FM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetFastMode(bool FM) ;

        [DllImport("wlmData.dll")]  public static extern bool   GetLinewidthMode(bool LM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetLinewidthMode(bool LM) ;

        [DllImport("wlmData.dll")]  public static extern bool   GetDistanceMode(bool DM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetDistanceMode(bool DM) ;

        [DllImport("wlmData.dll")]  public static extern int    GetSwitcherMode(int SM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetSwitcherMode(int SM) ;
        [DllImport("wlmData.dll")]  public static extern int    GetSwitcherChannel(int CH) ;
        [DllImport("wlmData.dll")]  public static extern int    SetSwitcherChannel(int CH) ;
        [DllImport("wlmData.dll")]  public static extern int    GetSwitcherSignalStates(int Signal, int *Use, int *Show) ;
        [DllImport("wlmData.dll")]  public static extern int    SetSwitcherSignalStates(int Signal, int Use, int Show) ;
        [DllImport("wlmData.dll")]  public static extern int    SetSwitcherSignal(int Signal, int Use, int Show) ;

        [DllImport("wlmData.dll")]  public static extern int    GetAutoCalMode(int ACM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetAutoCalMode(int ACM) ;
        [DllImport("wlmData.dll")]  public static extern int    GetAutoCalSetting(int ACS, int *val, int Res1, int *Res2) ;
        [DllImport("wlmData.dll")]  public static extern int    SetAutoCalSetting(int ACS, int val, int Res1, int Res2) ;

        [DllImport("wlmData.dll")]  public static extern int    GetActiveChannel(int Mode, int *Port, int Res1) ;
        [DllImport("wlmData.dll")]  public static extern int    SetActiveChannel(int Mode, int Port, int CH, int Res1) ;
        [DllImport("wlmData.dll")]  public static extern int    GetChannelsCount(int C) ;

        [DllImport("wlmData.dll")]  public static extern ushort GetOperationState(ushort Op) ;
        [DllImport("wlmData.dll")]  public static extern int    Operation(ushort Op) ;
        [DllImport("wlmData.dll")]  public static extern int    SetOperationFile(char *lpFile) ;
        [DllImport("wlmData.dll")]  public static extern int    Calibration(int Type, int Unit, double Value, int Channel) ;
        [DllImport("wlmData.dll")]  public static extern int    RaiseMeasurementEvent(int Mode) ;
        [DllImport("wlmData.dll")]  public static extern int    TriggerMeasurement(int Action) ;
        [DllImport("wlmData.dll")]  public static extern int    GetTriggerState(int TS) ;
        [DllImport("wlmData.dll")]  public static extern int    GetInterval(int I) ;
        [DllImport("wlmData.dll")]  public static extern int    SetInterval(int I) ;
        [DllImport("wlmData.dll")]  public static extern bool   GetIntervalMode(bool IM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetIntervalMode(bool IM) ;
        [DllImport("wlmData.dll")]  public static extern int    GetBackground(int BG) ;
        [DllImport("wlmData.dll")]  public static extern int    SetBackground(int BG) ;
        [DllImport("wlmData.dll")]  public static extern int    GetAveragingSettingNum(int num, int AS, int Value) ;
        [DllImport("wlmData.dll")]  public static extern int    SetAveragingSettingNum(int num, int AS, int Value) ;

        [DllImport("wlmData.dll")]  public static extern bool   GetLinkState(bool LS) ;
        [DllImport("wlmData.dll")]  public static extern int    SetLinkState(bool LS) ;
        [DllImport("wlmData.dll")]  public static extern void   LinkSettingsDlg() ;

        [DllImport("wlmData.dll")]  public static extern int    GetPatternItemSize(int Index) ;
        [DllImport("wlmData.dll")]  public static extern int    GetPatternItemCount(int Index) ;
        [DllImport("wlmData.dll")]  public static extern int    GetPattern(int Index) ;
        [DllImport("wlmData.dll")]  public static extern int    GetPatternNum(int Chn, int Index) ;
        [DllImport("wlmData.dll")]  public static extern int    GetPatternData(int Index, ulong PArray) ;
        [DllImport("wlmData.dll")]  public static extern int    GetPatternDataNum(int Chn, int Index, ulong PArray) ;
        [DllImport("wlmData.dll")]  public static extern int    SetPattern(int Index, int iEnable) ;
        [DllImport("wlmData.dll")]  public static extern int    SetPatternData(int Index, ulong PArray) ;

        [DllImport("wlmData.dll")]  public static extern bool   GetAnalysisMode(bool AM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetAnalysisMode(bool AM) ;
        [DllImport("wlmData.dll")]  public static extern int    GetAnalysisItemSize(int Index) ;
        [DllImport("wlmData.dll")]  public static extern int    GetAnalysisItemCount(int Index) ;
        [DllImport("wlmData.dll")]  public static extern int    GetAnalysis(int Index) ;
        [DllImport("wlmData.dll")]  public static extern int    GetAnalysisData(int Index, ulong PArray) ;
        [DllImport("wlmData.dll")]  public static extern int    SetAnalysis(int Index, int iEnable) ;

        [DllImport("wlmData.dll")]  public static extern int    GetMinPeak(int M1) ;
        [DllImport("wlmData.dll")]  public static extern int    GetMinPeak2(int M2) ;
        [DllImport("wlmData.dll")]  public static extern int    GetMaxPeak(int X1) ;
        [DllImport("wlmData.dll")]  public static extern int    GetMaxPeak2(int X2) ;
        [DllImport("wlmData.dll")]  public static extern int    GetAvgPeak(int A1) ;
        [DllImport("wlmData.dll")]  public static extern int    GetAvgPeak2(int A2) ;
        [DllImport("wlmData.dll")]  public static extern int    SetAvgPeak(int PA) ;

        [DllImport("wlmData.dll")]  public static extern int    GetAmplitudeNum(int num, int Index, int A) ;
        [DllImport("wlmData.dll")]  public static extern double GetIntensityNum(int num, double I) ;
        [DllImport("wlmData.dll")]  public static extern double GetPowerNum(int num, double P) ;



// ***********  Deviation (Laser Control) and PID-functions  ************
        [DllImport("wlmData.dll")]  public static extern bool   GetDeviationMode(bool DM) ;
        [DllImport("wlmData.dll")]  public static extern int    SetDeviationMode(bool DM) ;
        [DllImport("wlmData.dll")]  public static extern double GetDeviationReference(double DR) ;
        [DllImport("wlmData.dll")]  public static extern int    SetDeviationReference(double DR) ;
        [DllImport("wlmData.dll")]  public static extern int    GetDeviationSensitivity(int DS) ;
        [DllImport("wlmData.dll")]  public static extern int    SetDeviationSensitivity(int DS) ;
        [DllImport("wlmData.dll")]  public static extern double GetDeviationSignal(double DS) ;
        [DllImport("wlmData.dll")]  public static extern double GetDeviationSignalNum(int Port, double DS) ;
        [DllImport("wlmData.dll")]  public static extern int    SetDeviationSignal(double DS) ;
        [DllImport("wlmData.dll")]  public static extern int    SetDeviationSignalNum(int Port, double DS) ;
        [DllImport("wlmData.dll")]  public static extern double RaiseDeviationSignal(int iType, double dSignal) ;

        [DllImport("wlmData.dll")]  public static extern int    GetPIDCourse(char *PIDC) ;
        [DllImport("wlmData.dll")]  public static extern int    SetPIDCourse(char *PIDC) ;
        [DllImport("wlmData.dll")]  public static extern int    GetPIDCourseNum(int Port, char *PIDC) ;
        [DllImport("wlmData.dll")]  public static extern int    SetPIDCourseNum(int Port, char *PIDC) ;
        [DllImport("wlmData.dll")]  public static extern int    GetPIDSetting(int PS, int Port, int *iSet, double *dSet) ;
        [DllImport("wlmData.dll")]  public static extern int    SetPIDSetting(int PS, int Port, int iSet, double dSet) ;
        [DllImport("wlmData.dll")]  public static extern int    GetLaserControlSetting(int PS, int Port, int *iSet, double *dSet, char* sSet) ;
        [DllImport("wlmData.dll")]  public static extern int    SetLaserControlSetting(int PS, int Port, int iSet, double dSet, char* sSet) ;
        [DllImport("wlmData.dll")]  public static extern int    ClearPIDHistory(int Port) ;


// ***********  Other...-functions  *************************************
        [DllImport("wlmData.dll")]  public static extern double ConvertUnit(double Val, int uFrom, int uTo) ;
        [DllImport("wlmData.dll")]  public static extern double ConvertDeltaUnit(double Base, double Delta, int uBase, int uFrom, int uTo) ;


// ***********  Constants  **********************************************

        // Instantiating Constants for 'RFC' parameter
        public const int cInstCheckForWLM = -1;
        public const int cInstResetCalc = 0;
        public const int cInstReturnMode = cInstResetCalc;
        public const int cInstNotification = 1;
        public const int cInstCopyPattern = 2;
        public const int cInstCopyAnalysis = cInstCopyPattern;
        public const int cInstControlWLM = 3;
        public const int cInstControlDelay = 4;
        public const int cInstControlPriority = 5;

        // Notification Constants for 'Mode' parameter
        public const int cNotifyInstallCallback = 0;
        public const int cNotifyRemoveCallback = 1;
        public const int cNotifyInstallWaitEvent = 2;
        public const int cNotifyRemoveWaitEvent = 3;
        public const int cNotifyInstallCallbackEx = 4;
        public const int cNotifyInstallWaitEventEx = 5;

        // ResultError Constants of Set...-functions
        public const int ResERR_NoErr = 0;
        public const int ResERR_WlmMissing = -1;
        public const int ResERR_CouldNotSet = -2;
        public const int ResERR_ParmOutOfRange = -3;
        public const int ResERR_WlmOutOfResources = -4;
        public const int ResERR_WlmInternalError = -5;
        public const int ResERR_NotAvailable = -6;
        public const int ResERR_WlmBusy = -7;
        public const int ResERR_NotInMeasurementMode = -8;
        public const int ResERR_OnlyInMeasurementMode = -9;
        public const int ResERR_ChannelNotAvailable = -10;
        public const int ResERR_ChannelTemporarilyNotAvailable = -11;
        public const int ResERR_CalOptionNotAvailable = -12;
        public const int ResERR_CalWavelengthOutOfRange = -13;
        public const int ResERR_BadCalibrationSignal = -14;
        public const int ResERR_UnitNotAvailable = -15;
        public const int ResERR_FileNotFound = -16;
        public const int ResERR_FileCreation = -17;
        public const int ResERR_TriggerPending = -18;
        public const int ResERR_TriggerWaiting = -19;
        public const int ResERR_NoLegitimation = -20;
        public const int ResERR_NoTCPLegitimation = -21;
        public const int ResERR_NotInPulseMode = -22;
        public const int ResERR_OnlyInPulseMode = -23;
        public const int ResERR_NotInSwitchMode = -24;
        public const int ResERR_OnlyInSwitchMode = -25;
        public const int ResERR_TCPErr = -26;

        // Mode Constants for Callback-Export and WaitForWLMEvent-function
        public const int cmiResultMode = 1;
        public const int cmiRange = 2;
        public const int cmiPulse = 3;
        public const int cmiPulseMode = cmiPulse;
        public const int cmiWideLine = 4;
        public const int cmiWideMode = cmiWideLine;
        public const int cmiFast = 5;
        public const int cmiFastMode = cmiFast;
        public const int cmiExposureMode = 6;
        public const int cmiExposureValue1 = 7;
        public const int cmiExposureValue2 = 8;
        public const int cmiDelay = 9;
        public const int cmiShift = 10;
        public const int cmiShift2 = 11;
        public const int cmiReduce = 12;
        public const int cmiReduced = cmiReduce;
        public const int cmiScale = 13;
        public const int cmiTemperature = 14;
        public const int cmiLink = 15;
        public const int cmiOperation = 16;
        public const int cmiDisplayMode = 17;
        public const int cmiPattern1a = 18;
        public const int cmiPattern1b = 19;
        public const int cmiPattern2a = 20;
        public const int cmiPattern2b = 21;
        public const int cmiMin1 = 22;
        public const int cmiMax1 = 23;
        public const int cmiMin2 = 24;
        public const int cmiMax2 = 25;
        public const int cmiNowTick = 26;
        public const int cmiCallback = 27;
        public const int cmiFrequency1 = 28;
        public const int cmiFrequency2 = 29;
        public const int cmiDLLDetach = 30;
        public const int cmiVersion = 31;
        public const int cmiAnalysisMode = 32;
        public const int cmiDeviationMode = 33;
        public const int cmiDeviationReference = 34;
        public const int cmiDeviationSensitivity = 35;
        public const int cmiAppearance = 36;
        public const int cmiAutoCalMode = 37;
        public const int cmiWavelength1 = 42;
        public const int cmiWavelength2 = 43;
        public const int cmiLinewidth = 44;
        public const int cmiLinewidthMode = 45;
        public const int cmiLinkDlg = 56;
        public const int cmiAnalysis = 57;
        public const int cmiAnalogIn = 66;
        public const int cmiAnalogOut = 67;
        public const int cmiDistance = 69;
        public const int cmiWavelength3 = 90;
        public const int cmiWavelength4 = 91;
        public const int cmiWavelength5 = 92;
        public const int cmiWavelength6 = 93;
        public const int cmiWavelength7 = 94;
        public const int cmiWavelength8 = 95;
        public const int cmiVersion0 = cmiVersion;
        public const int cmiVersion1 = 96;
        public const int cmiPulseDelay = 99;
        public const int cmiDLLAttach = 121;
        public const int cmiSwitcherSignal = 123;
        public const int cmiSwitcherMode = 124;
        public const int cmiExposureValue11 = cmiExposureValue1;
        public const int cmiExposureValue12 = 125;
        public const int cmiExposureValue13 = 126;
        public const int cmiExposureValue14 = 127;
        public const int cmiExposureValue15 = 128;
        public const int cmiExposureValue16 = 129;
        public const int cmiExposureValue17 = 130;
        public const int cmiExposureValue18 = 131;
        public const int cmiExposureValue21 = cmiExposureValue2;
        public const int cmiExposureValue22 = 132;
        public const int cmiExposureValue23 = 133;
        public const int cmiExposureValue24 = 134;
        public const int cmiExposureValue25 = 135;
        public const int cmiExposureValue26 = 136;
        public const int cmiExposureValue27 = 137;
        public const int cmiExposureValue28 = 138;
        public const int cmiPatternAverage = 139;
        public const int cmiPatternAvg1 = 140;
        public const int cmiPatternAvg2 = 141;
        public const int cmiAnalogOut1 = cmiAnalogOut;
        public const int cmiAnalogOut2 = 142;
        public const int cmiMin11 = cmiMin1;
        public const int cmiMin12 = 146;
        public const int cmiMin13 = 147;
        public const int cmiMin14 = 148;
        public const int cmiMin15 = 149;
        public const int cmiMin16 = 150;
        public const int cmiMin17 = 151;
        public const int cmiMin18 = 152;
        public const int cmiMin21 = cmiMin2;
        public const int cmiMin22 = 153;
        public const int cmiMin23 = 154;
        public const int cmiMin24 = 155;
        public const int cmiMin25 = 156;
        public const int cmiMin26 = 157;
        public const int cmiMin27 = 158;
        public const int cmiMin28 = 159;
        public const int cmiMax11 = cmiMax1;
        public const int cmiMax12 = 160;
        public const int cmiMax13 = 161;
        public const int cmiMax14 = 162;
        public const int cmiMax15 = 163;
        public const int cmiMax16 = 164;
        public const int cmiMax17 = 165;
        public const int cmiMax18 = 166;
        public const int cmiMax21 = cmiMax2;
        public const int cmiMax22 = 167;
        public const int cmiMax23 = 168;
        public const int cmiMax24 = 169;
        public const int cmiMax25 = 170;
        public const int cmiMax26 = 171;
        public const int cmiMax27 = 172;
        public const int cmiMax28 = 173;
        public const int cmiAvg11 = cmiPatternAvg1;
        public const int cmiAvg12 = 174;
        public const int cmiAvg13 = 175;
        public const int cmiAvg14 = 176;
        public const int cmiAvg15 = 177;
        public const int cmiAvg16 = 178;
        public const int cmiAvg17 = 179;
        public const int cmiAvg18 = 180;
        public const int cmiAvg21 = cmiPatternAvg2;
        public const int cmiAvg22 = 181;
        public const int cmiAvg23 = 182;
        public const int cmiAvg24 = 183;
        public const int cmiAvg25 = 184;
        public const int cmiAvg26 = 185;
        public const int cmiAvg27 = 186;
        public const int cmiAvg28 = 187;
        public const int cmiPatternAnalysisWritten = 202;
        public const int cmiSwitcherChannel = 203;
        public const int cmiStartCalibration = 235;
        public const int cmiEndCalibration = 236;
        public const int cmiAnalogOut3 = 237;
        public const int cmiAnalogOut4 = 238;
        public const int cmiAnalogOut5 = 239;
        public const int cmiAnalogOut6 = 240;
        public const int cmiAnalogOut7 = 241;
        public const int cmiAnalogOut8 = 242;
        public const int cmiIntensity = 251;
        public const int cmiPower = 267;
        public const int cmiActiveChannel = 300;
        public const int cmiPIDCourse = 1030;
        public const int cmiPIDUseTa = 1031;
        public const int cmiPIDUseT = cmiPIDUseTa;
        public const int cmiPID_T = 1033;
        public const int cmiPID_P = 1034;
        public const int cmiPID_I = 1035;
        public const int cmiPID_D = 1036;
        public const int cmiDeviationSensitivityDim = 1040;
        public const int cmiDeviationSensitivityFactor = 1037;
        public const int cmiDeviationPolarity = 1038;
        public const int cmiDeviationSensitivityEx = 1039;
        public const int cmiDeviationUnit = 1041;
        public const int cmiDeviationBoundsMin = 1042;
        public const int cmiDeviationBoundsMax = 1043;
        public const int cmiDeviationRefMid = 1044;
        public const int cmiDeviationRefAt = 1045;
        public const int cmiPIDConstdt = 1059;
        public const int cmiPID_dt = 1060;
        public const int cmiPID_AutoClearHistory = 1061;
        public const int cmiDeviationChannel = 1063;
        public const int cmiPID_ClearHistoryOnRangeExceed = 1069;
        public const int cmiAutoCalPeriod = 1120;
        public const int cmiAutoCalUnit = 1121;
        public const int cmiAutoCalChannel = 1122;
        public const int cmiServerInitialized = 1124;
        public const int cmiWavelength9 = 1130;
        public const int cmiExposureValue19 = 1155;
        public const int cmiExposureValue29 = 1180;
        public const int cmiMin19 = 1205;
        public const int cmiMin29 = 1230;
        public const int cmiMax19 = 1255;
        public const int cmiMax29 = 1280;
        public const int cmiAvg19 = 1305;
        public const int cmiAvg29 = 1330;
        public const int cmiWavelength10 = 1355;
        public const int cmiWavelength11 = 1356;
        public const int cmiWavelength12 = 1357;
        public const int cmiWavelength13 = 1358;
        public const int cmiWavelength14 = 1359;
        public const int cmiWavelength15 = 1360;
        public const int cmiWavelength16 = 1361;
        public const int cmiWavelength17 = 1362;
        public const int cmiExternalInput = 1400;
        public const int cmiPressure = 1465;
        public const int cmiBackground = 1475;
        public const int cmiDistanceMode = 1476;
        public const int cmiInterval = 1477;
        public const int cmiIntervalMode = 1478;
        public const int cmiCalibrationEffect = 1480;
        public const int cmiLinewidth1 = cmiLinewidth;
        public const int cmiLinewidth2 = 1481;
        public const int cmiLinewidth3 = 1482;
        public const int cmiLinewidth4 = 1483;
        public const int cmiLinewidth5 = 1484;
        public const int cmiLinewidth6 = 1485;
        public const int cmiLinewidth7 = 1486;
        public const int cmiLinewidth8 = 1487;
        public const int cmiLinewidth9 = 1488;
        public const int cmiLinewidth10 = 1489;
        public const int cmiLinewidth11 = 1490;
        public const int cmiLinewidth12 = 1491;
        public const int cmiLinewidth13 = 1492;
        public const int cmiLinewidth14 = 1493;
        public const int cmiLinewidth15 = 1494;
        public const int cmiLinewidth16 = 1495;
        public const int cmiLinewidth17 = 1496;
        public const int cmiTriggerState = 1497;
        public const int cmiDeviceAttach = 1501;
        public const int cmiDeviceDetach = 1502;
        public const int cmiTimePerMeasurement = 1514;
        public const int cmiAutoExpoMin = 1517;
        public const int cmiAutoExpoMax = 1518;
        public const int cmiAutoExpoStepUp = 1519;
        public const int cmiAutoExpoStepDown = 1520;
        public const int cmiAutoExpoAtSaturation = 1521;
        public const int cmiAutoExpoAtLowSignal = 1522;
        public const int cmiAutoExpoFeedback = 1523;
        public const int cmiAveragingCount = 1524;
        public const int cmiAveragingMode = 1525;
        public const int cmiAveragingType = 1526;
        public const int cmiAirMode = 1532;
        public const int cmiAirTemperature = 1534;
        public const int cmiAirPressure = 1535;
        public const int cmiAirHumidity = 1536;

        // Index constants for Get- and SetExtraSetting
        public const int cesCalculateLive = 4501;

        // WLM Control Mode Constants
        public const int cCtrlWLMShow    = 1;
        public const int cCtrlWLMHide    = 2;
        public const int cCtrlWLMExit    = 3;
        public const int cCtrlWLMStore   = 4;
        public const int cCtrlWLMCompare = 5;
        public const int cCtrlWLMWait        = 0x0010;
        public const int cCtrlWLMStartSilent = 0x0020;
        public const int cCtrlWLMSilent      = 0x0040;
        public const int cCtrlWLMStartDelay  = 0x0080;

        // Operation Mode Constants (for "Operation" and "GetOperationState" functions)
        public const int cStop = 0;
        public const int cAdjustment = 1;
        public const int cMeasurement = 2;

        // Base Operation Constants (To be used exclusively, only one of this list at a time,
        // but still can be combined with "Measurement Action Addition Constants". See below.)
        public const int cCtrlStopAll = cStop;
        public const int cCtrlStartAdjustment = cAdjustment;
        public const int cCtrlStartMeasurement = cMeasurement;
        public const int cCtrlStartRecord = 0x0004;
        public const int cCtrlStartReplay = 0x0008;
        public const int cCtrlStoreArray  = 0x0010;
        public const int cCtrlLoadArray   = 0x0020;

        // Additional Operation Flag Constants (combine with "Base Operation Constants" above.)
        public const int cCtrlDontOverwrite = 0x0000;
        public const int cCtrlOverwrite     = 0x1000; // don't combine with cCtrlFileDialog
        public const int cCtrlFileGiven     = 0x0000;
        public const int cCtrlFileDialog    = 0x2000; // don't combine with cCtrlOverwrite and cCtrlFileASCII
        public const int cCtrlFileBinary    = 0x0000; // *.smr, *.ltr
        public const int cCtrlFileASCII     = 0x4000; // *.smx, *.ltx, don't combine with cCtrlFileDialog

        // Measurement Control Mode Constants
        public const int cCtrlMeasDelayRemove = 0;
        public const int cCtrlMeasDelayGenerally = 1;
        public const int cCtrlMeasDelayOnce = 2;
        public const int cCtrlMeasDelayDenyUntil = 3;
        public const int cCtrlMeasDelayIdleOnce = 4;
        public const int cCtrlMeasDelayIdleEach = 5;
        public const int cCtrlMeasDelayDefault = 6;

        // Measurement Triggering Action Constants
        public const int cCtrlMeasurementContinue = 0;
        public const int cCtrlMeasurementInterrupt = 1;
        public const int cCtrlMeasurementTriggerPoll = 2;
        public const int cCtrlMeasurementTriggerSuccess = 3;
        public const int cCtrlMeasurementEx = 0x0100;

        // ExposureRange Constants
        public const int cExpoMin = 0;
        public const int cExpoMax = 1;
        public const int cExpo2Min = 2;
        public const int cExpo2Max = 3;

        // Amplitude Constants
        public const int cMin1 = 0;
        public const int cMin2 = 1;
        public const int cMax1 = 2;
        public const int cMax2 = 3;
        public const int cAvg1 = 4;
        public const int cAvg2 = 5;

        // Measurement Range Constants
        public const int cRange_250_410 = 4;
        public const int cRange_250_425 = 0;
        public const int cRange_300_410 = 3;
        public const int cRange_350_500 = 5;
        public const int cRange_400_725 = 1;
        public const int cRange_700_1100 = 2;
        public const int cRange_800_1300 = 6;
        public const int cRange_900_1500 = cRange_800_1300;
        public const int cRange_1100_1700 = 7;
        public const int cRange_1100_1800 = cRange_1100_1700;

        // Measurement Range Model Constants
        public const int cRangeModelOld = 65535;
        public const int cRangeModelByOrder = 65534;
        public const int cRangeModelByWavelength = 65533;

        // Unit Constants for Get-/SetResultMode, GetLinewidth, Convert... and Calibration
        public const int cReturnWavelengthVac = 0;
        public const int cReturnWavelengthAir = 1;
        public const int cReturnFrequency = 2;
        public const int cReturnWavenumber = 3;
        public const int cReturnPhotonEnergy = 4;

        // Power Unit Constants
        public const int cPower_muW = 0;
        public const int cPower_dBm = 1;

        // Source Type Constants for Calibration
        public const int cHeNe633 = 0;
        public const int cHeNe1152 = 0;
        public const int cNeL = 1;
        public const int cOther = 2;
        public const int cFreeHeNe = 3;
        public const int cSLR1530 = 5;

        // Unit Constants for Autocalibration
        public const int cACOnceOnStart = 0;
        public const int cACMeasurements = 1;
        public const int cACDays = 2;
        public const int cACHours = 3;
        public const int cACMinutes = 4;

        // ExposureRange Constants
        public const int cGetSync = 1;
        public const int cSetSync = 2;

        // Pattern- and Analysis Constants
        public const int cPatternDisable = 0;
        public const int cPatternEnable = 1;
        public const int cAnalysisDisable = cPatternDisable;
        public const int cAnalysisEnable = cPatternEnable;

        public const int cSignal1Interferometers = 0;
        public const int cSignal1WideInterferometer = 1;
        public const int cSignal1Grating = 1;
        public const int cSignal2Interferometers = 2;
        public const int cSignal2WideInterferometer = 3;
        public const int cSignalAnalysis = 4;
        public const int cSignalAnalysisX = cSignalAnalysis;
        public const int cSignalAnalysisY = cSignalAnalysis + 1;

// State constants used with AutoExposureSetting functions
        public const int cJustStepDown = 0;
        public const int cRestartAtMinimum = 1;
        public const int cJustStepUp = 0;
        public const int cDriveToLevel = 1;
        public const int cConsiderFeedback = 1;
        public const int cDontConsiderFeedback = 0;

// State constants used with AveragingSetting functions
        public const int cAvrgFloating = 1;
        public const int cAvrgSucceeding = 2;
        public const int cAvrgSimple = 0;
        public const int cAvrgPattern = 1;

        // Return errorvalues of GetFrequency, GetWavelength and GetWLMVersion
        public const int ErrNoValue = 0;
        public const int ErrNoSignal = -1;
        public const int ErrBadSignal = -2;
        public const int ErrLowSignal = -3;
        public const int ErrBigSignal = -4;
        public const int ErrWlmMissing = -5;
        public const int ErrNotAvailable = -6;
        public const int InfNothingChanged = -7;
        public const int ErrNoPulse = -8;
        public const int ErrChannelNotAvailable = -10;
        public const int ErrDiv0 = -13;
        public const int ErrOutOfRange = -14;
        public const int ErrUnitNotAvailable = -15;
        public const int ErrTCPErr = -26;
        public const int ErrMaxErr = ErrTCPErr;

        // Return errorvalues of GetTemperature and GetPressure
        public const int ErrTemperature = -1000;
        public const int ErrTempNotMeasured = ErrTemperature + ErrNoValue;
        public const int ErrTempNotAvailable = ErrTemperature + ErrNotAvailable;
        public const int ErrTempWlmMissing = ErrTemperature + ErrWlmMissing;

        // Return errorvalues of GetDistance
        // real errorvalues are ErrDistance combined with those of GetWavelength
        public const int ErrDistance = -1000000000;
        public const int ErrDistanceNotAvailable = ErrDistance + ErrNotAvailable;
        public const int ErrDistanceWlmMissing = ErrDistance + ErrWlmMissing;

        // Return flags of ControlWLMEx in combination with Show or Hide, Wait and Res = 1
        public const int flServerStarted           = 0x00000001;
        public const int flErrDeviceNotFound       = 0x00000002;
        public const int flErrDriverError          = 0x00000004;
        public const int flErrUSBError             = 0x00000008;
        public const int flErrUnknownDeviceError   = 0x00000010;
        public const int flErrWrongSN              = 0x00000020;
        public const int flErrUnknownSN            = 0x00000040;
        public const int flErrTemperatureError     = 0x00000080;
        public const int flErrPressureError        = 0x00000100;
        public const int flErrCancelledManually    = 0x00000200;
        public const int flErrWLMBusy              = 0x00000400;
        public const int flErrUnknownError         = 0x00001000;
        public const int flNoInstalledVersionFound = 0x00002000;
        public const int flDesiredVersionNotFound  = 0x00004000;
        public const int flErrFileNotFound         = 0x00008000;
        public const int flErrParmOutOfRange       = 0x00010000;
        public const int flErrCouldNotSet          = 0x00020000;
        public const int flErrEEPROMFailed         = 0x00040000;
        public const int flErrFileFailed           = 0x00080000;
        public const int flDeviceDataNewer         = 0x00100000;
        public const int flFileDataNewer           = 0x00200000;
        public const int flErrDeviceVersionOld     = 0x00400000;
        public const int flErrFileVersionOld       = 0x00800000;
        public const int flDeviceStampNewer        = 0x01000000;
        public const int flFileStampNewer          = 0x02000000;

        // Return file info flags of SetOperationFile
        public const int flFileInfoDoesntExist = 0x0000;
        public const int flFileInfoExists      = 0x0001;
        public const int flFileInfoCantWrite   = 0x0002;
        public const int flFileInfoCantRead    = 0x0004;
        public const int flFileInfoInvalidName = 0x0008;
        public const int cFileParameterError = -1;

    }
}
