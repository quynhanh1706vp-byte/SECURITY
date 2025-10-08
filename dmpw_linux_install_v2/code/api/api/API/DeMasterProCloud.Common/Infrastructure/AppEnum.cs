using System.ComponentModel;
using DeMasterProCloud.Common.Resources;

namespace DeMasterProCloud.Common.Infrastructure
{
    public enum DeviceType
    {
        [Localization(nameof(DeviceResource.lblIcu300N), typeof(DeviceResource))]
        Icu300N = 0,
        [Localization(nameof(DeviceResource.lbliTouchPop), typeof(DeviceResource))]
        ITouchPop = 1,
        [Localization(nameof(DeviceResource.lblDesktopApp), typeof(DeviceResource))]
        DesktopApp = 2,
        [Localization(nameof(DeviceResource.lblITouchPopX), typeof(DeviceResource))]
        ITouchPopX = 3,
        [Localization(nameof(DeviceResource.lblDQMiniPlus), typeof(DeviceResource))]
        DQMiniPlus = 4,
        [Localization(nameof(DeviceResource.lblIt100), typeof(DeviceResource))]
        IT100 = 5,
        [Localization(nameof(DeviceResource.lblNexpaLPR), typeof(DeviceResource))]
        NexpaLPR = 6,
        [Localization(nameof(DeviceResource.lblXStation2), typeof(DeviceResource))]
        XStation2 = 7,
        [Localization(nameof(DeviceResource.lblFV6000), typeof(DeviceResource))]
        FV6000 = 8,
        [Localization(nameof(DeviceResource.lblPM85), typeof(DeviceResource))]
        PM85 = 9,

        [Localization(nameof(DeviceResource.lblITouch30A), typeof(DeviceResource))]
        ITouch30A = 10,
        [Localization(nameof(DeviceResource.lblDP636X), typeof(DeviceResource))]
        DP636X = 11,

        [Localization(nameof(DeviceResource.lblDF970), typeof(DeviceResource))]
        DF970 = 12,
        
        [Localization(nameof(DeviceResource.lblBiostation2), typeof(DeviceResource))]
        Biostation2 = 13,

        [Localization(nameof(DeviceResource.lblIcu300NX), typeof(DeviceResource))]
        Icu300NX = 14,
        [Localization(nameof(DeviceResource.lblBiostation3), typeof(DeviceResource))]
        Biostation3 = 15,
        [Localization(nameof(DeviceResource.lblEbknReader), typeof(DeviceResource))]
        EbknReader = 16,
        [Localization(nameof(DeviceResource.lblBA8300), typeof(DeviceResource))]
        BA8300 = 17,
        [Localization(nameof(DeviceResource.lblIcu400), typeof(DeviceResource))]
        Icu400 = 18,
        [Localization(nameof(DeviceResource.lblRA08), typeof(DeviceResource))]
        RA08 = 19,
        [Localization(nameof(DeviceResource.lblDQ8500), typeof(DeviceResource))]
        DQ8500 = 20,
        [Localization(nameof(DeviceResource.lblDQ200), typeof(DeviceResource))]
        DQ200 = 21,
        [Localization(nameof(CameraResource.lblCameraDC), typeof(CameraResource))]
        CameraDC = 22,
        [Localization(nameof(DeviceResource.lblTBVision), typeof(DeviceResource))]
        TBVision = 23,
        [Localization(nameof(DeviceResource.lblIcu500), typeof(DeviceResource))]
        Icu500 = 24,
        [Localization(nameof(DeviceResource.lblT2Face), typeof(DeviceResource))]
        T2Face = 25,
    }

    public enum ControllerType
    {
        [Localization(nameof(DeviceResource.lblIcu300N), typeof(DeviceResource))]
        Icu300N,
        [Localization(nameof(DeviceResource.lblNexpaLMS), typeof(DeviceResource))]
        NexpaLMS,
        [Localization(nameof(DeviceResource.lblKorecenServer), typeof(DeviceResource))]
        KorecenServer,
    }

    //public enum IcuDeviceTarget
    //{
    //    [Localization(nameof(DeviceResource.lblIcu300N), typeof(DeviceResource))]
    //    Icu300N,
    //    [Localization(nameof(DeviceResource.lblDe950), typeof(DeviceResource))]
    //    De950,
    //    [Localization(nameof(DeviceResource.lblDe960), typeof(DeviceResource))]
    //    De960
    //}

    //public enum ItouchPopDeviceTarget
    //{
    //    [Localization(nameof(DeviceResource.lblITouchPop2A), typeof(DeviceResource))]
    //    ITouchPop2A,
    //    [Localization(nameof(DeviceResource.lblAbcm), typeof(DeviceResource))]
    //    Abcm,
    //    [Localization(nameof(DeviceResource.lblSoundTrack01), typeof(DeviceResource))]
    //    SoundTrack01,

    //    [Localization(nameof(DeviceResource.lblITouchPopX), typeof(DeviceResource))]
    //    ITouchPopX,
    //    [Localization(nameof(DeviceResource.lblModule), typeof(DeviceResource))]
    //    Module,
    //    [Localization(nameof(DeviceResource.lblLibrary), typeof(DeviceResource))]
    //    Library,
    //}

    public enum DeviceUpdateTarget
    {
        [Localization(nameof(DeviceResource.lblDe950), typeof(DeviceResource))]
        De950,
        [Localization(nameof(DeviceResource.lblDe960), typeof(DeviceResource))]
        De960,
        [Localization(nameof(DeviceResource.lblDQMini), typeof(DeviceResource))]
        DQMini,
        [Localization(nameof(DeviceResource.lblDQCube), typeof(DeviceResource))]
        DQCube,
        [Localization(nameof(DeviceResource.lblIcu300N), typeof(DeviceResource))]
        Icu300N,
        [Localization(nameof(DeviceResource.lblIcu400), typeof(DeviceResource))]
        Icu400,

        [Localization(nameof(DeviceResource.lblAbcm), typeof(DeviceResource))]
        Abcm,
        [Localization(nameof(DeviceResource.lblSoundTrack01), typeof(DeviceResource))]
        SoundTrack01,
        [Localization(nameof(DeviceResource.lblITouchPop2A), typeof(DeviceResource))]
        ITouchPop2A,
        
        [Localization(nameof(DeviceResource.lblModule), typeof(DeviceResource))]
        Module,
        [Localization(nameof(DeviceResource.lblLibrary), typeof(DeviceResource))]
        Library,
        [Localization(nameof(DeviceResource.lblTarFile), typeof(DeviceResource))]
        Tar,
        [Localization(nameof(DeviceResource.lblITouchPopX), typeof(DeviceResource))]
        ITouchPopX,

        [Localization(nameof(DeviceResource.lblDP636X), typeof(DeviceResource))]
        DP636X,

        [Localization(nameof(DeviceResource.lblDQMiniPlus), typeof(DeviceResource))]
        DQMiniPlus,

        [Localization(nameof(DeviceResource.lblIt100), typeof(DeviceResource))]
        IT100,

        [Localization(nameof(DeviceResource.lblPM85), typeof(DeviceResource))]
        PM85,

        [Localization(nameof(DeviceResource.lblITouch30A), typeof(DeviceResource))]
        ITouch30A,

        // This is for the updating DF970 device.
        [Localization(nameof(DeviceResource.lblDF970), typeof(DeviceResource))]
        DF970,
        
        [Localization(nameof(DeviceResource.lblBA8300), typeof(DeviceResource))]
        BA8300,
        [Localization(nameof(DeviceResource.lblRA08), typeof(DeviceResource))]
        RA08,
        [Localization(nameof(DeviceResource.lblDQ8500), typeof(DeviceResource))]
        DQ8500,
        [Localization(nameof(DeviceResource.lblDQ200), typeof(DeviceResource))]
        DQ200,
        [Localization(nameof(DeviceResource.lblTBVision), typeof(DeviceResource))]
        TBVision,
        [Localization(nameof(DeviceResource.lblIcu500), typeof(DeviceResource))]
        Icu500,
        [Localization(nameof(DeviceResource.lblT2Face), typeof(DeviceResource))]
        T2Face,
    }

    public enum ItouchPopFileType
    {
        [Localization(nameof(DeviceResource.lblMainFirmware), typeof(DeviceResource))]
        MainFirmware,
        [Localization(nameof(DeviceResource.lblNfcModule), typeof(DeviceResource))]
        NfcModule,
        [Localization(nameof(DeviceResource.lblExtra), typeof(DeviceResource))]
        Extra,
        [Localization(nameof(DeviceResource.lblModule), typeof(DeviceResource))]
        Module,
        [Localization(nameof(DeviceResource.lblLibrary), typeof(DeviceResource))]
        Library,
        [Localization(nameof(DeviceResource.lblTarFile), typeof(DeviceResource))]
        Tar,
    }

    public enum IcuFileType
    {
        [Localization(nameof(DeviceResource.lblMainFirmware), typeof(DeviceResource))]
        MainFirmware,
        [Localization(nameof(DeviceResource.lblInReader), typeof(DeviceResource))]
        InReader,
        [Localization(nameof(DeviceResource.lblOutReader), typeof(DeviceResource))]
        OutReader,
    }

    public enum AccessTimeType
    {
        [Localization(nameof(AccessTimeResource.lblActive), typeof(AccessTimeResource))]
        Active = 1,
        [Localization(nameof(AccessTimeResource.lblPassage), typeof(AccessTimeResource))]
        Passage,
        [Localization(nameof(AccessTimeResource.lblNormal), typeof(AccessTimeResource))]
        Normal,
    }

    public enum DefaultTimezoneType
    {
        [Localization(nameof(AccessTimeResource.lblDefaultNotUseTimezoneName), typeof(AccessTimeResource))]
        DefaultNotUseTimezone = 1,
        [Localization(nameof(AccessTimeResource.lblDefaultTimezoneName), typeof(AccessTimeResource))]
        DefaultTimezone,
    }

    public enum TransmitType
    {
        [Localization(nameof(DeviceResource.lblTransmitCurrentTime), typeof(DeviceResource))]
        CurrentTime = 1,
        [Localization(nameof(DeviceResource.lblTransmitDeviceSetting), typeof(DeviceResource))]
        DeviceSetting = 2,
        [Localization(nameof(DeviceResource.lblTransmitTimezoneSetting), typeof(DeviceResource))]
        TimezoneSetting = 3,
        [Localization(nameof(DeviceResource.lblTransmitHolidaySetting), typeof(DeviceResource))]
        HolidaySetting = 4,
        [Localization(nameof(DeviceResource.lblTransmitUserInfo), typeof(DeviceResource))]
        UserInfo = 5,
    }

    public enum ProtocolType
    {
        [Localization(nameof(MessageLogResource.lblAddUser), typeof(MessageLogResource))]
        AddUser = 1,
        [Localization(nameof(MessageLogResource.lblDeleteUser), typeof(MessageLogResource))]
        DeleteUser,
        [Localization(nameof(MessageLogResource.lblUpdateDeviceConfig), typeof(MessageLogResource))]
        UpdateDeviceConfig,
        [Localization(nameof(MessageLogResource.lblDeleteDevice), typeof(MessageLogResource))]
        DeleteDevice,
        [Localization(nameof(MessageLogResource.lblUpdateTimezone), typeof(MessageLogResource))]
        UpdateTimezone,
        [Localization(nameof(MessageLogResource.lblDeleteTimezone), typeof(MessageLogResource))]
        DeleteTimezone,
        [Localization(nameof(MessageLogResource.lblUpdateHoliday), typeof(MessageLogResource))]
        UpdateHoliday,
        [Localization(nameof(MessageLogResource.lblNotification), typeof(MessageLogResource))]
        Notification,
        [Localization(nameof(MessageLogResource.lblEventLogResponse), typeof(MessageLogResource))]
        EventLogResponse,
        [Localization(nameof(MessageLogResource.lblLoadDeviceInfo), typeof(MessageLogResource))]
        LoadDeviceInfo,
        [Localization(nameof(MessageLogResource.lblDeviceInstruction), typeof(MessageLogResource))]
        DeviceInstruction,
        [Localization(nameof(MessageLogResource.lblFileDownload), typeof(MessageLogResource))]
        FileDownLoad,
        [Localization(nameof(MessageLogResource.lblLongProcessProgress), typeof(MessageLogResource))]
        LongProcessProgress,
        [Localization(nameof(MessageLogResource.lblConnectionStatus), typeof(MessageLogResource))]
        ConnectionStatus,
    }

    public enum Topic
    {
        [Localization(nameof(MessageLogResource.lblEventLogTopic), typeof(MessageLogResource))]
        EventLogTopic = 1,
        [Localization(nameof(MessageLogResource.lblEventLogJsonTopic), typeof(MessageLogResource))]
        EventLogJsonTopic,
        [Localization(nameof(MessageLogResource.lblEventLogResponseTopic), typeof(MessageLogResource))]
        EventLogResponseTopic,
        [Localization(nameof(MessageLogResource.lblConfigTopic), typeof(MessageLogResource))]
        ConfigurationTopic,
        [Localization(nameof(MessageLogResource.lblConfigResponseTopic), typeof(MessageLogResource))]
        ConfigurationResponseTopic,
        [Localization(nameof(MessageLogResource.lblAccessControlTopic), typeof(MessageLogResource))]
        AccessControlTopic,
        [Localization(nameof(MessageLogResource.lblAccessControlResponseTopic), typeof(MessageLogResource))]
        AccessControlResponseTopic,
        [Localization(nameof(MessageLogResource.lblHolidayTopic), typeof(MessageLogResource))]
        HolidayTopic,
        [Localization(nameof(MessageLogResource.lblHolidayResponseTopic), typeof(MessageLogResource))]
        HolidayResponseTopic,
        [Localization(nameof(MessageLogResource.lblOnlineTopic), typeof(MessageLogResource))]
        DeviceOnlineTopic,
        [Localization(nameof(MessageLogResource.lblDeviceStatusTopic), typeof(MessageLogResource))]
        DeviceStatus,
        [Localization(nameof(MessageLogResource.lblDeviceInfoResponseTopic), typeof(MessageLogResource))]
        DeviceInfoResponseTopic,
        [Localization(nameof(MessageLogResource.lblDeviceInfoTopic), typeof(MessageLogResource))]
        DeviceInfoTopic,
        [Localization(nameof(MessageLogResource.lblTimezoneTopic), typeof(MessageLogResource))]
        TimezoneTopic,
        [Localization(nameof(MessageLogResource.lblTimezoneResponseTopic), typeof(MessageLogResource))]
        TimezoneResponseTopic,
        [Localization(nameof(MessageLogResource.lblNotificationTopic), typeof(MessageLogResource))]
        NotificationTopic,
        [Localization(nameof(MessageLogResource.lblDeviceInstructionTopic), typeof(MessageLogResource))]
        DeviceInstructionTopic,
        [Localization(nameof(MessageLogResource.lblDeviceInstructionResponseTopic), typeof(MessageLogResource))]
        DeviceInstructionResponseTopic,
        [Localization(nameof(MessageLogResource.lblFileTranferTopic), typeof(MessageLogResource))]
        FileTranferTopic,
        [Localization(nameof(MessageLogResource.lblFileTranferResponseTopic), typeof(MessageLogResource))]
        FileTranferResponseTopic,
        [Localization(nameof(MessageLogResource.lblLongProcessProgressTopic), typeof(MessageLogResource))]
        LongProcessProgressTopic

    }

    public enum OperationType
    {
        [Localization(nameof(DeviceResource.lblEntrance), typeof(DeviceResource))]
        Entrance,
        [Localization(nameof(DeviceResource.lblRestaurant), typeof(DeviceResource))]
        Restaurant,
        [Localization(nameof(DeviceResource.lblBusReader), typeof(DeviceResource))]
        BusReader,
        [Localization(nameof(DeviceResource.lblFireDetector), typeof(DeviceResource))]
        FireDetector,
        [Localization(nameof(DeviceResource.lblReception), typeof(DeviceResource))]
        Reception,
    }

    public enum Antipass
    {
        [Localization(nameof(EventLogResource.lblInDevice), typeof(EventLogResource))]
        In = 1,
        [Localization(nameof(EventLogResource.lblOutDevice), typeof(EventLogResource))]
        Out = 2,
        [Localization(nameof(EventLogResource.lblSelfDevice), typeof(EventLogResource))]
        Self = 3,
        [Localization(nameof(EventLogResource.lblPowerDevice), typeof(EventLogResource))]
        Power = 4,
        [Localization(nameof(EventLogResource.lblNormalDevice), typeof(EventLogResource))]
        Normal = 5,
        [Localization(nameof(EventLogResource.lblTimezoneDevice), typeof(EventLogResource))]
        Timezone = 6,
    }

    public enum CommandType
    {
        [Localization(nameof(CommonResource.Open), typeof(CommonResource))]
        Open = 1,
        [Localization(nameof(CommonResource.SetTime), typeof(CommonResource))]
        SetTime,
        [Localization(nameof(CommonResource.Reset), typeof(CommonResource))]
        Reset,
        [Localization(nameof(CommonResource.ForceOpen), typeof(CommonResource))]
        ForceOpen,
        [Localization(nameof(CommonResource.ForceClose), typeof(CommonResource))]
        ForceClose,
        [Localization(nameof(CommonResource.Release), typeof(CommonResource))]
        Release
    }

    public enum SensorType
    {
        //[Localization(nameof(CommonResource.None), typeof(CommonResource))]
        //None = 0,
        [Localization(nameof(CommonResource.NormalOpen), typeof(CommonResource))]
        NormalOpen = 0,
        [Localization(nameof(CommonResource.NormalClose), typeof(CommonResource))]
        NormalClose,
        [Localization(nameof(CommonResource.NotUse), typeof(CommonResource))]
        NotUse
    }

    public enum Status
    {
        [Localization(nameof(AccessTimeResource.lblActive), typeof(AccessTimeResource))]
        Valid,
        [Localization(nameof(CommonResource.Offline), typeof(CommonResource))]
        Invalid
    }

    public enum FileStatus
    {
        [Localization(nameof(CommonResource.Success), typeof(CommonResource))]
        Success,
        [Localization(nameof(CommonResource.Failure), typeof(CommonResource))]
        Failure
    }

    public enum RegisterStatus
    {
        [Localization(nameof(CommonResource.Success), typeof(CommonResource))]
        Success,
        [Localization(nameof(CommonResource.Failure), typeof(CommonResource))]
        Failure,
        [Localization(nameof(CommonResource.Null), typeof(CommonResource))]
        Null,
        [Localization(nameof(CommonResource.Duplicate), typeof(CommonResource))]
        Duplicate,
    }

    public enum IcuStatus
    {
        [Localization(nameof(CommonResource.Disconnected), typeof(CommonResource))]
        Disconnected,
        [Localization(nameof(CommonResource.Connected), typeof(CommonResource))]
        Connected,
        [Localization(nameof(CommonResource.LostKey), typeof(CommonResource))]
        LostKey
    }

    public enum ConnectionStatus
    {
        [Localization(nameof(CommonResource.Offline), typeof(CommonResource))]
        Offline,
        [Localization(nameof(CommonResource.Online), typeof(CommonResource))]
        Online,
        [Localization(nameof(CommonResource.Warning), typeof(CommonResource))]
        LostKey
    }

    public enum CardStatus
    {
        [Localization(nameof(CommonResource.Normal), typeof(CommonResource))]
        Normal = 0,
        [Localization(nameof(CommonResource.Transfer), typeof(CommonResource))]
        Transfer = 1,
        [Localization(nameof(CommonResource.Retire), typeof(CommonResource))]
        Retire = 2,
        [Localization(nameof(CommonResource.Lost), typeof(CommonResource))]
        Lost = 3,
        [Localization(nameof(CommonResource.Invalid), typeof(CommonResource))]
        InValid = 4,
        
        [Localization(nameof(CommonResource.WaitingForPrinting), typeof(CommonResource))]
        WaitingForPrinting = 5,
        [Localization(nameof(CommonResource.WaitingForIssuance), typeof(CommonResource))]
        WaitingForIssuance = 6,
        // [Localization(nameof(CommonResource.Issued), typeof(CommonResource))]
        // Issued = 7




        // For Deleted Card.
        // This enum value is not used in normal situation.
        DeletedCard = 999
    }

    public enum IssuingCardStatus
    {
        [Localization(nameof(CommonResource.Issued), typeof(CommonResource))]
        Issued = 0,
    }

    public enum DoorStatus
    {
        [Localization(nameof(DeviceResource.dsClosedAndLock), typeof(DeviceResource))]
        ClosedAndLock = 0,
        [Localization(nameof(DeviceResource.dsClosedAndUnlocked), typeof(DeviceResource))]
        ClosedAndUnlocked = 1,
        [Localization(nameof(DeviceResource.dsOpened), typeof(DeviceResource))]
        Opened = 2,
        [Localization(nameof(DeviceResource.dsForceOpened), typeof(DeviceResource))]
        ForceOpened = 3,
        [Localization(nameof(DeviceResource.dsPassageOpened), typeof(DeviceResource))]
        PassageOpened = 4,
        [Localization(nameof(DeviceResource.dsEmergencyOpened), typeof(DeviceResource))]
        EmergencyOpened = 5,
        [Localization(nameof(DeviceResource.dsEmergencyClosed), typeof(DeviceResource))]
        EmergencyClosed = 6,
        [Localization(nameof(DeviceResource.dsUnknown), typeof(DeviceResource))]
        Unknown = 7,
        [Localization(nameof(DeviceResource.dsLock), typeof(DeviceResource))]
        Lock = 8,
        [Localization(nameof(DeviceResource.dsUnlock), typeof(DeviceResource))]
        Unlock = 9,

        [Localization(nameof(DeviceResource.dsInactive), typeof(DeviceResource))]
        Inactive = 10,
        [Localization(nameof(DeviceResource.dsNeedSetting), typeof(DeviceResource))]
        NeedSetting = 11,
        [Localization(nameof(DeviceResource.dsInvalid), typeof(DeviceResource))]
        Invalid = 12,

        // For fire detector
        [Localization(nameof(DeviceResource.dsFireDetected), typeof(DeviceResource))]
        Fire = 200,
        [Localization(nameof(DeviceResource.dsFireNoDetected), typeof(DeviceResource))]
        NoFire = 201,
        // For canteen
        [Localization(nameof(DeviceResource.dsNoMealTime), typeof(DeviceResource))]
        NoMealTime = 120,
    }

    public enum AccessGroupType
    {
        [Localization(nameof(CommonResource.NoAccess), typeof(CommonResource))]
        NoAccess,
        [Localization(nameof(CommonResource.FullAccess), typeof(CommonResource))]
        FullAccess,
        [Localization(nameof(CommonResource.NormalAccess), typeof(CommonResource))]
        NormalAccess,
        [Localization(nameof(CommonResource.VisitAccess), typeof(CommonResource))]
        VisitAccess,
        [Localization(nameof(CommonResource.PersonalAccess), typeof(CommonResource))]
        PersonalAccess,
    }

    public enum SyncStatus
    {
        Synced = 0,
        Syncing = 1
    }


    public enum AllocationStatus
    {
        [Localization(nameof(UserResource.lblWaitingApproval), typeof(UserResource))]
        Waiting = 0,
        [Localization(nameof(UserResource.lblWaitingService), typeof(UserResource))]
        StandBy = 1,
        [Localization(nameof(UserResource.lblVehicleInService), typeof(UserResource))]
        InService = 2,
        [Localization(nameof(UserResource.lblFinishedVehicleService), typeof(UserResource))]
        Finished = 3,
    }


    public enum UpdateFlag
    {
        Continue = 0,
        Stop = 1
    }

    //public enum VerifyMode
    //{
    //    [Localization(nameof(CommonResource.lblCardOnly), typeof(CommonResource))]
    //    CardOnly = 1,
    //    [Localization(nameof(CommonResource.lblCardAndPw), typeof(CommonResource))]
    //    CardAndPw
    //}

    public enum CardType
    {
        [Localization(nameof(EventLogResource.lblCard), typeof(EventLogResource))]
        NFC = 0,
        [Localization(nameof(EventLogResource.lblQR), typeof(EventLogResource))]
        QrCode = 1,
        [Localization(nameof(EventLogResource.lblPassCode), typeof(EventLogResource))]
        PassCode = 2,
        [Localization(nameof(EventLogResource.lblNFCPhone), typeof(EventLogResource))]
        NFCPhone = 3,
        [Localization(nameof(EventLogResource.lblFace), typeof(EventLogResource))]
        FaceId = 4,
        [Localization(nameof(EventLogResource.lblHFaceId), typeof(EventLogResource))]
        HFaceId = 5,
        [Localization(nameof(EventLogResource.lblVehicleId), typeof(EventLogResource))]
        VehicleId = 6,
        [Localization(nameof(EventLogResource.lblVein), typeof(EventLogResource))]
        Vein = 7,
        [Localization(nameof(EventLogResource.lblFingerPrint), typeof(EventLogResource))]
        FingerPrint = 8,
        [Localization(nameof(EventLogResource.lblLFaceId), typeof(EventLogResource))]
        LFaceId = 9,
        [Localization(nameof(EventLogResource.lblBioFace), typeof(EventLogResource))]
        BioFaceId = 10,
        [Localization(nameof(EventLogResource.lblEbknFaceId), typeof(EventLogResource))]
        EbknFaceId = 11,
        [Localization(nameof(EventLogResource.lblEbknFingerprint), typeof(EventLogResource))]
        EbknFingerprint = 12,
        [Localization(nameof(EventLogResource.lblAratekFingerPrint), typeof(EventLogResource))]
        AratekFingerPrint = 13,
        [Localization(nameof(EventLogResource.lblDCFaceId), typeof(EventLogResource))]
        DCFaceId = 14,
        [Localization(nameof(EventLogResource.lblVNID), typeof(EventLogResource))]
        VNID = 15,
        [Localization(nameof(EventLogResource.lblTBFace), typeof(EventLogResource))]
        TBFace = 16,
        [Localization(nameof(EventLogResource.lblVehicleMotoBikeId), typeof(EventLogResource))]
        VehicleMotoBikeId = 17,
    }

    public enum WiegandType
    {
        Default = 0,
        Predefined
    }

    public enum DeviceAuthority
    {
        General = 1,
        Manager,
        Supervisor
    }

    public enum RoleRules
    {
        //[Localization(nameof(CommonResource.None), typeof(CommonResource))]
        //None,
        [Localization(nameof(CommonResource.In), typeof(CommonResource))]
        In = 0,
        [Localization(nameof(CommonResource.Out), typeof(CommonResource))]
        Out,
        //[Localization(nameof(CommonResource.InOut), typeof(CommonResource))]
        //InOut
    }

    public enum PassbackRules
    {
        [Localization(nameof(CommonResource.NotUse), typeof(CommonResource))]
        NotUse = 0,
        [Localization(nameof(CommonResource.SoftAPB), typeof(CommonResource))]
        SoftAPB,
        [Localization(nameof(CommonResource.HardAPB), typeof(CommonResource))]
        HardAPB,
        //[Localization(nameof(CommonResource.InOut), typeof(CommonResource))]
        //InOut
    }

    public enum AccountType
    {
        [Localization(nameof(CommonResource.SystemAdmin), typeof(CommonResource))]
        SystemAdmin = 0,
        [Localization(nameof(CommonResource.SuperAdmin), typeof(CommonResource))]
        SuperAdmin = 1,
        [Localization(nameof(CommonResource.PrimaryAdmin), typeof(CommonResource))]
        PrimaryManager = 2,
        [Localization(nameof(CommonResource.SecondaryAdmin), typeof(CommonResource))]
        SecondaryManager = 3,
        [Localization(nameof(CommonResource.Employee), typeof(CommonResource))]
        Employee = 5,
        [Localization(nameof(CommonResource.DynamicRole), typeof(CommonResource))]
        DynamicRole = 6
    }

    public enum Role
    {
        [Localization(nameof(CommonResource.Normal), typeof(CommonResource))]
        Normal = 1,
        [Localization(nameof(CommonResource.Admin), typeof(CommonResource))]
        Admin
    }
    public enum InOut
    {
        In = 1, // Link to DB: In: true, Out: false
        Out = 2
    }

    public enum CardReaderLed
    {
        [Localization(nameof(DeviceResource.lblBlue), typeof(DeviceResource))]
        Blue = 0,
        [Localization(nameof(DeviceResource.lblRed), typeof(DeviceResource))]
        Red = 1
    }

    public enum BuzzerReader
    {
        [Localization(nameof(CommonResource.On), typeof(CommonResource))]
        ON = 1,
        [Localization(nameof(CommonResource.Off), typeof(CommonResource))]
        OFF = 0
    }

    public enum UseCardReader
    {
        [Localization(nameof(CommonResource.Use), typeof(CommonResource))]
        Use = 0,
        [Localization(nameof(CommonResource.NotUse), typeof(CommonResource))]
        NotUse = 1
    }

    public enum UserType
    {
        [Localization(nameof(CommonResource.NormalUser), typeof(CommonResource))]
        Normal = 0,
        [Localization(nameof(CommonResource.Visitor), typeof(CommonResource))]
        Visit = 1
    }
    public enum Privilege
    {
        [Localization(nameof(CommonResource.NormalUser), typeof(CommonResource))]
        User = 0,
        [Localization(nameof(CommonResource.Manager), typeof(CommonResource))]
        Manager = 1,
        [Localization(nameof(CommonResource.Admin), typeof(CommonResource))]
        Administrator = 2,
    }

    public enum EventType
    {
        // for access
        [Localization(nameof(EventLogResource.lblNormalAccess), typeof(EventLogResource))]
        NormalAccess = 1,
        [Localization(nameof(EventLogResource.lblUnRegisteredID), typeof(EventLogResource))]
        UnRegisteredID = 2,
        [Localization(nameof(EventLogResource.lblExpirationDate), typeof(EventLogResource))]
        ExpirationDate = 3,
        [Localization(nameof(EventLogResource.lblEffectiveDateNotStarted), typeof(EventLogResource))]
        EffectiveDateNotStarted = 4,
        [Localization(nameof(EventLogResource.lblWrongIssueCount), typeof(EventLogResource))]
        WrongIssueCount = 5,
        [Localization(nameof(EventLogResource.lblNoUserAccessTime), typeof(EventLogResource))]
        NoUserAccessTime = 6,
        [Localization(nameof(EventLogResource.lblPassageAccessTimeOn), typeof(EventLogResource))]
        PassageAccessTimeOn = 7,
        [Localization(nameof(EventLogResource.lblPassageAccessTimeOff), typeof(EventLogResource))]
        PassageAccessTimeOff = 8,
        [Localization(nameof(EventLogResource.lblNoDoorActiveTime), typeof(EventLogResource))]
        NoDoorActiveTime = 9,
        [Localization(nameof(EventLogResource.lblInvalidDoor), typeof(EventLogResource))]
        InvalidDoor = 10,
        [Localization(nameof(EventLogResource.lblValidDoor), typeof(EventLogResource))]
        ValidDoor = 11,
        [Localization(nameof(EventLogResource.lblWrongAccessType), typeof(EventLogResource))]
        WrongAccessType = 12,
        //[Localization(nameof(EventLogResource.lblUnregisteredCardAndPwd), typeof(EventLogResource))]
        //UnregisteredCardAndPwd = 13,
        [Localization(nameof(EventLogResource.lblAntiPassbackNormal), typeof(EventLogResource))]
        AntiPassbackNormal = 14,
        [Localization(nameof(EventLogResource.lblAntiPassbackError), typeof(EventLogResource))]
        AntiPassbackError = 15,
        [Localization(nameof(EventLogResource.lblAntiPassbackDenied), typeof(EventLogResource))]
        AntiPassbackDenied = 16,
        [Localization(nameof(EventLogResource.lblDeviceInstructionOpen), typeof(EventLogResource))]
        DeviceInstructionOpen = 17,
        [Localization(nameof(EventLogResource.lblDeviceInstructionSettime), typeof(EventLogResource))]
        DeviceInstructionSettime = 18,
        [Localization(nameof(EventLogResource.lblDeviceInstructionReset), typeof(EventLogResource))]
        DeviceInstructionReset = 19,
        [Localization(nameof(EventLogResource.lblDeviceInstructionForceOpen), typeof(EventLogResource))]
        DeviceInstructionForceOpen = 20,
        [Localization(nameof(EventLogResource.lblDeviceInstructionForceClose), typeof(EventLogResource))]
        DeviceInstructionForceClose = 21,
        [Localization(nameof(EventLogResource.lblDeviceInstructionRelease), typeof(EventLogResource))]
        DeviceInstructionRelease = 22,
        [Localization(nameof(EventLogResource.lblDeviceInstructionDeleteAllEvent), typeof(EventLogResource))]
        DeviceInstructionDeleteAllEvent = 23,
        [Localization(nameof(EventLogResource.lblTryAccessInEmergencyClose), typeof(EventLogResource))]
        TryAccessInEmergencyClose = 24,
        [Localization(nameof(EventLogResource.lblTryAccessInInvalidDoor), typeof(EventLogResource))]
        TryAccessInInvalidDoor = 25,
        [Localization(nameof(EventLogResource.lblCommunicationFailed), typeof(EventLogResource))]
        CommunicationFailed = 26,
        [Localization(nameof(EventLogResource.lblCommunicationSucceed), typeof(EventLogResource))]
        CommunicationSucceed = 27,
        // This event(28) will be remove.
        [Localization(nameof(EventLogResource.lblDeviceIsRunning), typeof(EventLogResource))]
        DeviceIsRunning = 28,
        [Localization(nameof(EventLogResource.lblErrTwoFactorAuth), typeof(EventLogResource))]
        FailTwoFactorAuth = 29,
        [Localization(nameof(EventLogResource.lblWrongQRcode), typeof(EventLogResource))]
        WrongQRcode = 30,

        // Retired
        [Localization(nameof(EventLogResource.lblRetiredPerson), typeof(EventLogResource))]
        RetiredCard = 31,
        // Lost card
        [Localization(nameof(EventLogResource.lblLostCard), typeof(EventLogResource))]
        LostCard = 32,
        // Transferee
        [Localization(nameof(EventLogResource.lblTransferCard), typeof(EventLogResource))]
        TransferCard = 33,
        // Invalid card
        [Localization(nameof(EventLogResource.lblInvalidCard), typeof(EventLogResource))]
        InvalidCard = 34,

        [Localization(nameof(EventLogResource.lblFirmwareApplicationUpdate), typeof(EventLogResource))]
        FirmwareApplicationUpdate = 35,
        [Localization(nameof(EventLogResource.lblFirmwareDownloadFailed), typeof(EventLogResource))]
        FirmwareDownloadFailed = 36,
        [Localization(nameof(EventLogResource.lblApplicationIsRunning), typeof(EventLogResource))]
        ApplicationIsRunning = 37,
        [Localization(nameof(EventLogResource.lblExitApplication), typeof(EventLogResource))]
        ExitApplication = 38,
        [Localization(nameof(EventLogResource.lblMPRIntervalExpired), typeof(EventLogResource))]
        MprIntervalExpired = 39,
        [Localization(nameof(EventLogResource.lblMPRAuth), typeof(EventLogResource))]
        MprAuth = 40,
        [Localization(nameof(EventLogResource.lblNormalAccessMPR), typeof(EventLogResource))]
        NormalAccessMpr = 41,
        [Localization(nameof(EventLogResource.lblMPRError), typeof(EventLogResource))]
        MprError = 42,
        [Localization(nameof(EventLogResource.lblMasterCardAccess), typeof(EventLogResource))]
        MasterCardAccess = 43,
        //[Localization(nameof(EventLogResource.lblDoorOpenManually), typeof(EventLogResource))]
        //DoorOpenManually = 44,
        [Localization(nameof(EventLogResource.lblExpiredQrCode), typeof(EventLogResource))]
        ExpiredQrCode = 45,
        [Localization(nameof(EventLogResource.lblPressedButton), typeof(EventLogResource))]
        PressedButton = 46,
        
        [Localization(nameof(EventLogResource.lblUnknownFace), typeof(EventLogResource))]
        UnknownPerson = 47,
        [Localization(nameof(EventLogResource.lblCameraResponseExpired), typeof(EventLogResource))]
        CameraResponseExpired = 48,

        [Localization(nameof(EventLogResource.lblNotMatchCardType), typeof(EventLogResource))]
        NotMatchCardType = 49,

        // for vehicle
        [Localization(nameof(EventLogResource.lblNormalVehicle), typeof(EventLogResource))]
        NormalVehicle = 50,
        [Localization(nameof(EventLogResource.lblUnregisteredVehicle), typeof(EventLogResource))]
        UnregisteredVehicle = 51,
        [Localization(nameof(EventLogResource.lblViolationRule2), typeof(EventLogResource))]
        Rule2Vehicle = 52,
        [Localization(nameof(EventLogResource.lblExceptionVehicle), typeof(EventLogResource))]
        ExceptionVehicle = 53,
        [Localization(nameof(EventLogResource.lblViolatedVehicle), typeof(EventLogResource))]
        ViolatedVehicle = 54,
        [Localization(nameof(EventLogResource.lblViolationRule5), typeof(EventLogResource))]
        Rule5Vehicle = 55,
        [Localization(nameof(EventLogResource.lblIdCertifiedVehicle), typeof(EventLogResource))]
        IdCertifiedVehicle = 56,

        [Localization(nameof(EventLogResource.lblRemoteOperation), typeof(EventLogResource))]
        RemoteOperation = 57,

        [Localization(nameof(EventLogResource.lblHighTemperature), typeof(EventLogResource))]
        HighTemperature = 58,
        [Localization(nameof(EventLogResource.lblHighTemperatureDeny), typeof(EventLogResource))]
        HighTemperatureDeny = 59,
        [Localization(nameof(EventLogResource.lblAlarmActivated), typeof(EventLogResource))]
        AlarmActivated = 60,

        [Localization(nameof(EventLogResource.lblContinuousAbnormalAccess), typeof(EventLogResource))]
        ContinuousAbnormalAccess = 61,
        [Localization(nameof(EventLogResource.lblErrorReadingCard), typeof(EventLogResource))]
        ErrorReadingCard = 62,

        [Localization(nameof(EventLogResource.lblDeviceInstructionDeleteAllUser), typeof(EventLogResource))]
        DeviceInstructionDeleteAllUser = 63,
        [Localization(nameof(EventLogResource.lblDeviceInstructionSendAllUser), typeof(EventLogResource))]
        DeviceInstructionSendAllUser = 64,
        
        // camera alarm
        [Localization(nameof(EventLogResource.lblCameraAlarm), typeof(EventLogResource))]
        CameraAlarm = 70,


        // for canteen
        [Localization(nameof(EventLogResource.lblMeal1), typeof(EventLogResource))]
        Meal1 = 101,
        [Localization(nameof(EventLogResource.lblMeal2), typeof(EventLogResource))]
        Meal2 = 102,
        [Localization(nameof(EventLogResource.lblMeal3), typeof(EventLogResource))]
        Meal3 = 103,
        [Localization(nameof(EventLogResource.lblMeal4), typeof(EventLogResource))]
        Meal4 = 104,
        [Localization(nameof(EventLogResource.lblMeal5), typeof(EventLogResource))]
        Meal5 = 105,
        [Localization(nameof(EventLogResource.lblMeal6), typeof(EventLogResource))]
        Meal6 = 106,
        [Localization(nameof(EventLogResource.lblMeal7), typeof(EventLogResource))]
        Meal7 = 107,
        [Localization(nameof(EventLogResource.lblMeal8), typeof(EventLogResource))]
        Meal8 = 108,
        [Localization(nameof(EventLogResource.lblMeal9), typeof(EventLogResource))]
        Meal9 = 109,
        [Localization(nameof(EventLogResource.lblMeal10), typeof(EventLogResource))]
        Meal10 = 110,

        [Localization(nameof(EventLogResource.lblOnlyAccessibleAtMealtime), typeof(EventLogResource))]
        OnlyAccessibleAtMealtime = 120,
        [Localization(nameof(EventLogResource.lblDuplicatedMealEvent), typeof(EventLogResource))]
        DuplicatedMealEvent = 121,
        [Localization(nameof(EventLogResource.lblNoMealPermission), typeof(EventLogResource))]
        NoMealPermission = 122,

        // for FIRE ALARM
        [Localization(nameof(EventLogResource.lblFireOn), typeof(EventLogResource))]
        FireOn = 200,
        [Localization(nameof(EventLogResource.lblFireOff), typeof(EventLogResource))]
        FireOff = 201,
    }


    public enum ExceptionalUserType
    {
        [Localization(nameof(CanteenResource.exceptionalUserTypeSpecified), typeof(CanteenResource))]
        Specified = 0,
        [Localization(nameof(CanteenResource.exceptionalUserTypeAdditional), typeof(CanteenResource))]
        Additional = 1,
        [Localization(nameof(CanteenResource.exceptionalUserTypeDiscount), typeof(CanteenResource))]
        Discount = 2,
    }

    public enum AppliedDiscountType
    {
        [Localization(nameof(CanteenResource.none), typeof(CanteenResource))]
        None = 0,
        [Localization(nameof(CanteenResource.exceptionalUserTypeSpecified), typeof(CanteenResource))]
        Specified = 1,
        [Localization(nameof(CanteenResource.exceptionalUserTypeAdditional), typeof(CanteenResource))]
        Additional = 2,
        [Localization(nameof(CanteenResource.exceptionalUserTypeDiscount), typeof(CanteenResource))]
        Discount = 3,
    }

    public enum AvailableMealTypeCode
    {
        [Localization(nameof(EventLogResource.lblMeal1), typeof(EventLogResource))]
        Meal1 = 101,
        [Localization(nameof(EventLogResource.lblMeal2), typeof(EventLogResource))]
        Meal2 = 102,
        [Localization(nameof(EventLogResource.lblMeal3), typeof(EventLogResource))]
        Meal3 = 103,
        [Localization(nameof(EventLogResource.lblMeal4), typeof(EventLogResource))]
        Meal4 = 104,
        [Localization(nameof(EventLogResource.lblMeal5), typeof(EventLogResource))]
        Meal5 = 105,
        [Localization(nameof(EventLogResource.lblMeal6), typeof(EventLogResource))]
        Meal6 = 106,
        [Localization(nameof(EventLogResource.lblMeal7), typeof(EventLogResource))]
        Meal7 = 107,
        [Localization(nameof(EventLogResource.lblMeal8), typeof(EventLogResource))]
        Meal8 = 108,
        [Localization(nameof(EventLogResource.lblMeal9), typeof(EventLogResource))]
        Meal9 = 109,
        [Localization(nameof(EventLogResource.lblMeal10), typeof(EventLogResource))]
        Meal10 = 110,
    } 

    public enum VehicleEventType
    {
        // for vehicle
        [Localization(nameof(EventLogResource.lblNormalVehicle), typeof(EventLogResource))]
        NormalVehicle = 50,
        [Localization(nameof(EventLogResource.lblUnregisteredVehicle), typeof(EventLogResource))]
        UnregisteredVehicle = 51,
        [Localization(nameof(EventLogResource.lblViolationRule2), typeof(EventLogResource))]
        Rule2Vehicle = 52,
        [Localization(nameof(EventLogResource.lblExceptionVehicle), typeof(EventLogResource))]
        ExceptionVehicle = 53,
        [Localization(nameof(EventLogResource.lblViolatedVehicle), typeof(EventLogResource))]
        ViolatedVehicle = 54,
        [Localization(nameof(EventLogResource.lblViolationRule5), typeof(EventLogResource))]
        Rule5Vehicle = 55,
        [Localization(nameof(EventLogResource.lblIdCertifiedVehicle), typeof(EventLogResource))]
        IdCertifiedVehicle = 56,

        [Localization(nameof(EventLogResource.lblRemoteOperation), typeof(EventLogResource))]
        RemoteOperation = 57,
    }

    public enum DataType
    {
        IcuDeviceIndex = 1,
        TimezoneIndex,
        HolidayIndex,
        WiegandIndex,
        MultiPersonRuleIndex,
        UserIndex
    }

    public enum HolidayType
    {
        [Localization(nameof(HolidayResource.lblHolidayType1), typeof(HolidayResource))]
        HolidayType1 = 1,
        [Localization(nameof(HolidayResource.lblHolidayType2), typeof(HolidayResource))]
        HolidayType2,
        [Localization(nameof(HolidayResource.lblHolidayType3), typeof(HolidayResource))]
        HolidayType3
    }

    public enum ActionType
    {
        Add = 1,
        Update,
        Delete,
        DeleteMultiple
    }

    public enum TransferType
    {
        NotSavedToServer = 0, //Default value
        SavedToServerSuccess,
        SavedIcuSuccess
    }

    public enum Confirmation
    {
        [Localization(nameof(CommonResource.lblNo), typeof(CommonResource))]
        No = 0,
        [Localization(nameof(CommonResource.lblYes), typeof(CommonResource))]
        Yes = 1
    }

    public enum MessageStatus
    {
        //TODO UnSent 필요한지 검토 후 필요없을 경우 삭제 예정
        [Localization(nameof(CommonResource.lblUnSent), typeof(CommonResource))]
        UnSent = 1,
        [Localization(nameof(CommonResource.lblSending), typeof(CommonResource))]
        Sending,
        [Localization(nameof(CommonResource.lblSent), typeof(CommonResource))]
        Sent,
        [Localization(nameof(CommonResource.lblReceived), typeof(CommonResource))]
        Received
    }

    public enum SyncType
    {
        [Localization(nameof(DeviceResource.lblDevice), typeof(DeviceResource))]
        IcuDevice = 1,
        [Localization(nameof(AccessTimeResource.lblTimezone), typeof(AccessTimeResource))]
        Timezone,
        [Localization(nameof(HolidayResource.lblHoliday), typeof(HolidayResource))]
        Holiday,
        [Localization(nameof(UserResource.lblUser), typeof(UserResource))]
        User,
        [Localization(nameof(EventLogResource.lblEventLog), typeof(EventLogResource))]
        Event,
        [Localization(nameof(DeviceResource.lblOpenDoor), typeof(DeviceResource))]
        OpenDoor
    }

    public enum SyncErrorCode
    {
        [Localization(nameof(SyncErrorCodeResource.StxError), typeof(SyncErrorCodeResource))]
        StxError = 0,
        [Localization(nameof(SyncErrorCodeResource.DeviceNumberMismatch), typeof(SyncErrorCodeResource))]
        DeviceNumberMismatch,
        [Localization(nameof(SyncErrorCodeResource.DataLengthError), typeof(SyncErrorCodeResource))]
        DataLengthError,
        [Localization(nameof(SyncErrorCodeResource.NoCommand), typeof(SyncErrorCodeResource))]
        NoCommand,
        [Localization(nameof(SyncErrorCodeResource.LrcError), typeof(SyncErrorCodeResource))]
        LrcError,
        [Localization(nameof(SyncErrorCodeResource.EtxError), typeof(SyncErrorCodeResource))]
        EtxError,
        [Localization(nameof(SyncErrorCodeResource.DataError), typeof(SyncErrorCodeResource))]
        DataError,
        [Localization(nameof(SyncErrorCodeResource.ErrorsRelatedToTerminalDb), typeof(SyncErrorCodeResource))]
        ErrorsRelatedToTerminalDb,
        [Localization(nameof(SyncErrorCodeResource.CardReadingFailed), typeof(SyncErrorCodeResource))]
        CardReadingFailed,
        [Localization(nameof(SyncErrorCodeResource.TimSettingFailed), typeof(SyncErrorCodeResource))]
        TimSettingFailed,
        [Localization(nameof(SyncErrorCodeResource.NoDataToLookUp), typeof(SyncErrorCodeResource))]
        NoDataToLookUp,
        [Localization(nameof(SyncErrorCodeResource.UserFullError), typeof(SyncErrorCodeResource))]
        UserFullError
    }

    public enum SystemLogType
    {
        [Localization(nameof(SystemLogTypeResource.Company), typeof(SystemLogTypeResource))]
        Company = 1,
        [Localization(nameof(SystemLogTypeResource.Building), typeof(SystemLogTypeResource))]
        Building,
        [Localization(nameof(SystemLogTypeResource.Department), typeof(SystemLogTypeResource))]
        Department,
        [Localization(nameof(SystemLogTypeResource.AccountManagement), typeof(SystemLogTypeResource))]
        AccountManagement,
        [Localization(nameof(SystemLogTypeResource.DeviceMonitoring), typeof(SystemLogTypeResource))]
        DeviceMonitoring,
        [Localization(nameof(SystemLogTypeResource.DeviceUpdate), typeof(SystemLogTypeResource))]
        DeviceUpdate,
        [Localization(nameof(SystemLogTypeResource.DeviceSetting), typeof(SystemLogTypeResource))]
        DeviceSetting,
        [Localization(nameof(SystemLogTypeResource.User), typeof(SystemLogTypeResource))]
        User,
        [Localization(nameof(SystemLogTypeResource.AccessGroup), typeof(SystemLogTypeResource))]
        AccessGroup,
        [Localization(nameof(SystemLogTypeResource.AccessTime), typeof(SystemLogTypeResource))]
        AccessTime,
        [Localization(nameof(SystemLogTypeResource.Holiday), typeof(SystemLogTypeResource))]
        Holiday,      
        [Localization(nameof(SystemLogTypeResource.MessageSetting), typeof(SystemLogTypeResource))]
        MessageSetting,
        [Localization(nameof(SystemLogTypeResource.SystemSetting), typeof(SystemLogTypeResource))]
        SystemSetting,
        [Localization(nameof(SystemLogTypeResource.EventRecovery), typeof(SystemLogTypeResource))]
        EventRecovery,
        [Localization(nameof(SystemLogTypeResource.CheckUserInformation), typeof(SystemLogTypeResource))]
        CheckUserInformation,
        [Localization(nameof(SystemLogTypeResource.CheckDeviceSetting), typeof(SystemLogTypeResource))]
        CheckDeviceSetting,
        [Localization(nameof(SystemLogTypeResource.TransmitAllData), typeof(SystemLogTypeResource))]
        TransmitAllData,
        [Localization(nameof(SystemLogTypeResource.Emergency), typeof(SystemLogTypeResource))]
        Emergency,
        [Localization(nameof(SystemLogTypeResource.Report), typeof(SystemLogTypeResource))]
        Report,
        [Localization(nameof(SystemLogTypeResource.Category), typeof(SystemLogTypeResource))]
        Category,
        [Localization(nameof(SystemLogTypeResource.PlugIn), typeof(SystemLogTypeResource))]
        PlugIn,
        [Localization(nameof(SystemLogTypeResource.DynamicRole), typeof(SystemLogTypeResource))]
        Role,
        [Localization(nameof(SystemLogTypeResource.Canteen), typeof(SystemLogTypeResource))]
        Canteen,
        [Localization(nameof(SystemLogTypeResource.Attendance), typeof(SystemLogTypeResource))]
        Attendance,
        [Localization(nameof(SystemLogTypeResource.RegisteredUser), typeof(SystemLogTypeResource))]
        RegisteredUser,
        [Localization(nameof(SystemLogTypeResource.VisitManagement), typeof(SystemLogTypeResource))]
        VisitManagement,
        [Localization(nameof(SystemLogTypeResource.Camera), typeof(SystemLogTypeResource))]
        Camera,
        [Localization(nameof(SystemLogTypeResource.VehicleAllocation), typeof(SystemLogTypeResource))]
        VehicleAllocation,
        [Localization(nameof(SystemLogTypeResource.Vehicle), typeof(SystemLogTypeResource))]
        Vehicle,
    }

    public enum ActionLogType
    {
        [Localization(nameof(ActionLogTypeResource.Login), typeof(ActionLogTypeResource))]
        Login = 1,
        [Localization(nameof(ActionLogTypeResource.Logout), typeof(ActionLogTypeResource))]
        Logout,
        [Localization(nameof(ActionLogTypeResource.ChangePassword), typeof(ActionLogTypeResource))]
        ChangePassword,
        [Localization(nameof(ActionLogTypeResource.Add), typeof(ActionLogTypeResource))]
        Add,
        [Localization(nameof(ActionLogTypeResource.Update), typeof(ActionLogTypeResource))]
        Update,
        [Localization(nameof(ActionLogTypeResource.Delete), typeof(ActionLogTypeResource))]
        Delete,
        [Localization(nameof(ActionLogTypeResource.DeleteMultiple), typeof(ActionLogTypeResource))]
        DeleteMultiple,
        [Localization(nameof(ActionLogTypeResource.Export), typeof(ActionLogTypeResource))]
        Export,
        [Localization(nameof(ActionLogTypeResource.Import), typeof(ActionLogTypeResource))]
        Import,
        [Localization(nameof(ActionLogTypeResource.AssignDoor), typeof(ActionLogTypeResource))]
        AssignDoor,
        [Localization(nameof(ActionLogTypeResource.UnassignDoor), typeof(ActionLogTypeResource))]
        UnassignDoor,
        [Localization(nameof(ActionLogTypeResource.AssignUser), typeof(ActionLogTypeResource))]
        AssignUser,
        [Localization(nameof(ActionLogTypeResource.UnassignUser), typeof(ActionLogTypeResource))]
        UnassignUser,
        [Localization(nameof(ActionLogTypeResource.ForcedOpen), typeof(ActionLogTypeResource))]
        ForcedOpen,
        [Localization(nameof(ActionLogTypeResource.ForcedClose), typeof(ActionLogTypeResource))]
        ForcedClose,
        [Localization(nameof(ActionLogTypeResource.Release), typeof(ActionLogTypeResource))]
        Release,
        [Localization(nameof(ActionLogTypeResource.DoorOpen), typeof(ActionLogTypeResource))]
        DoorOpen,
        [Localization(nameof(ActionLogTypeResource.Reset), typeof(ActionLogTypeResource))]
        Reset,
        [Localization(nameof(ActionLogTypeResource.Sync), typeof(ActionLogTypeResource))]
        Sync,
        [Localization(nameof(ActionLogTypeResource.ValidDoor), typeof(ActionLogTypeResource))]
        ValidDoor,
        [Localization(nameof(ActionLogTypeResource.InvalidDoor), typeof(ActionLogTypeResource))]
        InvalidDoor,
        [Localization(nameof(ActionLogTypeResource.Fail), typeof(ActionLogTypeResource))]
        Fail,
        [Localization(nameof(ActionLogTypeResource.Success), typeof(ActionLogTypeResource))]
        Success,
        [Localization(nameof(ActionLogTypeResource.CopyDoorSetting), typeof(ActionLogTypeResource))]
        CopyDoorSetting,
        [Localization(nameof(ActionLogTypeResource.Reinstall), typeof(ActionLogTypeResource))]
        Reinstall,

        //TODO Auto Register / (Add) Missing Device 용어 통일 필요(정해야함)
        [Localization(nameof(ActionLogTypeResource.AutoRegister), typeof(ActionLogTypeResource))]
        AutoRegister,
        [Localization(nameof(ActionLogTypeResource.UpdateDoor), typeof(ActionLogTypeResource))]
        UpdateDoor,
        [Localization(nameof(ActionLogTypeResource.UpdateMultipleUser), typeof(ActionLogTypeResource))]
        UpdateMultipleUser,
        [Localization(nameof(ActionLogTypeResource.BasicInfoTransmit), typeof(ActionLogTypeResource))]
        BasicInfoTransmit,
        [Localization(nameof(ActionLogTypeResource.TimezoneTransmit), typeof(ActionLogTypeResource))]
        TimezoneTransmit,
        [Localization(nameof(ActionLogTypeResource.HolidayTransmit), typeof(ActionLogTypeResource))]
        HolidayTransmit,
        [Localization(nameof(ActionLogTypeResource.Transmit), typeof(ActionLogTypeResource))]
        Transmit,
        [Localization(nameof(ActionLogTypeResource.EventExport), typeof(ActionLogTypeResource))]
        EventExport,
        [Localization(nameof(ActionLogTypeResource.SystemLogExport), typeof(ActionLogTypeResource))]
        SystemLogExport,
        [Localization(nameof(ActionLogTypeResource.AccessibleDoorExport), typeof(ActionLogTypeResource))]
        AccessibleDoorExport,
        [Localization(nameof(ActionLogTypeResource.RegisteredUserExport), typeof(ActionLogTypeResource))]
        RegisteredUserExport,
        [Localization(nameof(ActionLogTypeResource.AnalysisExport), typeof(ActionLogTypeResource))]
        AnalysisExport,
        [Localization(nameof(ActionLogTypeResource.Recovery), typeof(ActionLogTypeResource))]
        Recovery,

        [Localization(nameof(ActionLogTypeResource.AddMaster), typeof(ActionLogTypeResource))]
        AddMaster,
        [Localization(nameof(ActionLogTypeResource.DeleteMaster), typeof(ActionLogTypeResource))]
        DeleteMaster,

        [Localization(nameof(ActionLogTypeResource.ExportMaster), typeof(ActionLogTypeResource))]
        ExportMaster,
        [Localization(nameof(ActionLogTypeResource.ExportDoor), typeof(ActionLogTypeResource))]
        ExportDoor,
    }

    public enum SexType
    {
        [Localization(nameof(UserResource.lblFemale), typeof(UserResource))]
        Female,
        [Localization(nameof(UserResource.lblMale), typeof(UserResource))]
        Male
    }

    public enum SettingType
    {
        ResetAppPassword = 1
    }

    public enum LoginUnauthorized
    {
        CompanyNonExist = 1000,
        CompanyExpired = 1001,
        InvalidCredentials = 1002,
        InvalidToken = 1003,
        AccountNonExist = 1004,
        AccountUseAnotherDevice = 1005,
        PasswordExpired = 1006,
        PasswordNeedToChange = 1007,
        AccountLocked = 1008,
    }

    //public enum VisitStatus
    //{
    //    [Localization(nameof(VisitResource.lblApprovalWaiting1), typeof(VisitResource))]
    //    ApprovalWaiting1,
    //    [Localization(nameof(VisitResource.lblApprovalWaiting2), typeof(VisitResource))]
    //    ApprovalWaiting2,
    //    [Localization(nameof(VisitResource.lblIssueing), typeof(VisitResource))]
    //    Issueing,
    //    [Localization(nameof(VisitResource.lblDeliveryWaiting), typeof(VisitResource))]
    //    DeliveryWaiting,
    //    [Localization(nameof(VisitResource.lblDeliveryOk), typeof(VisitResource))]
    //    DeliveryOk,
    //    [Localization(nameof(VisitResource.lblReturn), typeof(VisitResource))]
    //    Return,
    //    [Localization(nameof(VisitResource.lblReject), typeof(VisitResource))]
    //    Reject,
    //    [Localization(nameof(VisitResource.lblReclamation), typeof(VisitResource))]
    //    Reclamation,
    //    [Localization(nameof(VisitResource.lblNotUse), typeof(VisitResource))]
    //    NotUse,
    //    [Localization(nameof(VisitResource.lblPreRegister), typeof(VisitResource))]
    //    PreRegister,
    //}

    public enum VisitChangeStatusType
    {
        [Localization(nameof(VisitResource.lblWaiting), typeof(VisitResource))]
        Waiting = 0,
        [Localization(nameof(VisitResource.lblApproved1), typeof(VisitResource))]
        Approved1 = 1,
        [Localization(nameof(VisitResource.lblApproved), typeof(VisitResource))]
        Approved = 2,
        [Localization(nameof(VisitResource.lblReject), typeof(VisitResource))]
        Rejected = 3,
        [Localization(nameof(VisitResource.lblCardIssued), typeof(VisitResource))]
        CardIssued = 4,
        [Localization(nameof(VisitResource.lblFinished), typeof(VisitResource))]
        Finished = 5,
        [Localization(nameof(VisitResource.lblFinishedWithoutReturnCard), typeof(VisitResource))]
        FinishedWithoutReturnCard = 6,
        [Localization(nameof(VisitResource.lblCardReturned), typeof(VisitResource))]
        CardReturned = 7,
        [Localization(nameof(VisitResource.lblAutoApproved), typeof(VisitResource))]
        AutoApproved = 8,
        [Localization(nameof(VisitResource.lblPreRegister), typeof(VisitResource))]
        PreRegister = 9,
    }

    public enum VisitType
    {
        [Localization(nameof(VisitResource.lblInsider), typeof(VisitResource))]
        Insider = 0,
        [Localization(nameof(VisitResource.lblOutsider), typeof(VisitResource))]
        OutSider = 1
    }

    public enum VisitArmyType
    {
        [Localization(nameof(VisitResource.lblCivilian), typeof(VisitResource))]
        Civilian = 10,
        //[Localization(nameof(VisitResource.lblResidentCivilian), typeof(VisitResource))]
        //ResidentCivilian = 11,
        [Localization(nameof(VisitResource.lblAnotherArmy), typeof(VisitResource))]
        AnotherArmy = 12,
        //[Localization(nameof(VisitResource.lblArmyFamily), typeof(VisitResource))]
        //ArmyFamily = 13,
        //[Localization(nameof(VisitResource.lblOtherUnitMember), typeof(VisitResource))]
        //OtherUnitMember = 14
    }

    public enum VisitingCardStatusType
    {
        [Localization(nameof(VisitResource.lblDelivered), typeof(VisitResource))]
        Delivered,
        [Localization(nameof(VisitResource.lblReturned), typeof(VisitResource))]
        Returned,
        [Localization(nameof(VisitResource.lblNotUse), typeof(VisitResource))]
        NotUse,
        [Localization(nameof(VisitResource.lblRequest), typeof(VisitResource))]
        Request,
    }

    public enum VisitingCardStatusNormalType
    {
        [Localization(nameof(VisitResource.lblDelivered), typeof(VisitResource))]
        Delivered,
        [Localization(nameof(VisitResource.lblReturned), typeof(VisitResource))]
        Returned,
        [Localization(nameof(VisitResource.lblNotUse), typeof(VisitResource))]
        NotUse,
    }

    public enum VisitingCardStatusPreRegisterType
    {
        [Localization(nameof(VisitResource.lblNotUse), typeof(VisitResource))]
        NotUse = 2,
        [Localization(nameof(VisitResource.lblRequest), typeof(VisitResource))]
        Request,

    }


    public enum WorkType
    {
        [Localization(nameof(UserResource.lblPermanentWorker), typeof(UserResource))]
        PermanentWorker = 0,
        [Localization(nameof(UserResource.lblContractWorker), typeof(UserResource))]
        ContractWorker = 1,
        [Localization(nameof(UserResource.lblResidentWorker), typeof(UserResource))]
        ResidentWorker = 2,
        [Localization(nameof(UserResource.lblPartTimeWorker), typeof(UserResource))]
        PartTimeWorker = 3,
        [Localization(nameof(UserResource.lblContractorStaff), typeof(UserResource))]
        ContractorStaff = 4,
        [Localization(nameof(UserResource.lblPersonnel), typeof(UserResource))]
        Personnel = 5,
    }

    public enum Visit_WorkType
    {
        [Localization(nameof(CardIssuingResource.lblConstruction), typeof(CardIssuingResource))]
        Construction = 5,
        [Localization(nameof(CardIssuingResource.lblMeetingVisit), typeof(CardIssuingResource))]
        MeetingVisit = 6,
        [Localization(nameof(CardIssuingResource.lblTraning), typeof(CardIssuingResource))]
        Training = 7,
    }

    public enum Army_WorkType
    {
        /// <summary>
        /// 현역 간부
        /// </summary>
        [Localization(nameof(UserResource.lblActiveDutySoldier), typeof(UserResource))]
        ActiveDutySoldier = 0,
        
        /// <summary>
        /// 군무원
        /// </summary>
        [Localization(nameof(UserResource.lblCivilianWorker), typeof(UserResource))]
        CivilianWorker = 1,
        
        /// <summary>
        /// 병사
        /// </summary>
        [Localization(nameof(UserResource.lblSoldier), typeof(UserResource))]
        Soldier = 2,
        
        /// <summary>
        /// 군가족
        /// </summary>
        [Localization(nameof(UserResource.lblSoldierFamily), typeof(UserResource))]
        SoldierFamily = 3,
        
        /// <summary>
        /// 민간인
        /// </summary>
        [Localization(nameof(UserResource.lblCivilian), typeof(UserResource))]
        Civilian = 4,

        /// <summary>
        /// 장교
        /// </summary>
        [Localization(nameof(UserResource.lblOfficer), typeof(UserResource))]
        Officer = 5,

        /// <summary>
        /// 부사관
        /// </summary>
        [Localization(nameof(UserResource.lblNCOfficer), typeof(UserResource))]
        NCOfficer = 6,

        /// <summary>
        /// 공무직 근로자
        /// </summary>
        [Localization(nameof(UserResource.lblPublicOfficer), typeof(UserResource))]
        PublicOfficer = 7,

        /// <summary>
        /// 상주 근로자
        /// </summary>
        [Localization(nameof(UserResource.lblResidedWorker), typeof(UserResource))]
        ResidedWorker = 8,

        /// <summary>
        /// 고정 출입자
        /// </summary>
        [Localization(nameof(UserResource.lblFixedWorker), typeof(UserResource))]
        FixedWorker = 9,

        /// <summary>
        /// 비상근 예비군
        /// </summary>
        [Localization(nameof(UserResource.lblReservedArmy), typeof(UserResource))]
        ReservedArmy = 10,
    }

    public enum ArmyVisit_WorkType
    {
        [Localization(nameof(UserResource.lblTemp), typeof(UserResource))]
        Temp = 1,
        [Localization(nameof(AttendanceResource.lblVisitOut), typeof(AttendanceResource))]
        VisitOut = 2,
        [Localization(nameof(UserResource.lblSoldierFamily), typeof(UserResource))]
        SoldierFamily = 3,
        [Localization(nameof(VisitResource.lblVisitFromOut), typeof(VisitResource))]
        VisitingOutside = 4,
        [Localization(nameof(CardIssuingResource.lblConstruction), typeof(CardIssuingResource))]
        Construction = 5,
        [Localization(nameof(CardIssuingResource.lblMeetingVisit), typeof(CardIssuingResource))]
        MeetingVisit = 6,
        [Localization(nameof(CardIssuingResource.lblTraning), typeof(CardIssuingResource))]
        Training = 7,
        [Localization(nameof(VisitResource.lblAnotherArmy), typeof(VisitResource))]
        AnotherArmy = 8,
        [Localization(nameof(UserResource.lblFixedWorker), typeof(UserResource))]
        FixedWorker = 9,
        [Localization(nameof(UserResource.lblReservedArmy), typeof(UserResource))]
        ReservedArmy = 10,
        [Localization(nameof(UserResource.lblResidedWorker), typeof(UserResource))]
        ResidedWorker = 11,
    }

    public enum ArmyTemp_WorkType
    {
        [Localization(nameof(UserResource.lblTemp), typeof(UserResource))]
        Temporary = 10
    }

    public enum PassType
    {
        
        [Localization(nameof(UserResource.lblPassCard), typeof(UserResource))]
        PassCard,
        [Localization(nameof(UserResource.lblVisitCard), typeof(UserResource))]
        VisitCard,

    }

    public enum UserStatus
    {
        [Localization(nameof(UserResource.lblUse), typeof(UserResource))]
        Use,
        [Localization(nameof(UserResource.lblNotUse), typeof(UserResource))]
        NotUse
    }


    public enum GetUserType
    {
        [Localization(nameof(UserResource.lblValidUser), typeof(UserResource))]
        Valid = 0,
        [Localization(nameof(UserResource.lblInvalidUser), typeof(UserResource))]
        Invalid,
        [Localization(nameof(UserResource.lblAllUser), typeof(UserResource))]
        All
    }

    public enum PermissionType
    {
        [Localization(nameof(UserResource.lblNotUse), typeof(UserResource))]
        NotUse,
        [Localization(nameof(UserResource.lblSystemAdmin), typeof(UserResource))]
        SystemAdmin,
        //[Localization(nameof(CommonResource.SuperAdmin), typeof(CommonResource))]
        //SuperAdmin,
    }
    
    public enum AttendanceType
    {
        [Localization(nameof(UserResource.lblLateIn), typeof(UserResource))]
        LateIn,
        [Localization(nameof(UserResource.lblEarlyOut), typeof(UserResource))]
        EarlyOut,
        [Localization(nameof(UserResource.lblAbsentNoReason), typeof(UserResource))]
        AbsentNoReason,
        [Localization(nameof(UserResource.lblOverTime), typeof(UserResource))]
        OverTime,
        [Localization(nameof(UserResource.lblNormal), typeof(UserResource))]
        Normal,
        [Localization(nameof(UserResource.lblBusinessTrip), typeof(UserResource))]
        BusinessTrip,
        [Localization(nameof(UserResource.lblVacation), typeof(UserResource))]
        Vacation,
        [Localization(nameof(UserResource.lblSickness), typeof(UserResource))]
        Sickness,
        [Localization(nameof(UserResource.lblLateInEarlyOut), typeof(UserResource))]
        LateInEarlyOut,
        [Localization(nameof(UserResource.lblRemote), typeof(UserResource))]
        Remote,
        [Localization(nameof(UserResource.lblAbNormal), typeof(UserResource))]
        AbNormal,
        [Localization(nameof(UserResource.lblHoliday), typeof(UserResource))]
        Holiday,
        [Localization(nameof(UserResource.lblOffDutyBreak), typeof(UserResource))]
        OffDutyBreak,
    }

    public enum AttendanceStatus
    {
        [Localization(nameof(AttendanceResource.lblStatusWaiting), typeof(AttendanceResource))]
        Waiting = 1,
        [Localization(nameof(AttendanceResource.lblStatusAprroval), typeof(AttendanceResource))]
        Approved = 2,
        [Localization(nameof(AttendanceResource.lblStatusReject), typeof(AttendanceResource))]
        Reject = 3,
    }


    public enum SoldierManagementType
    {
        [Localization(nameof(AttendanceResource.lblWork), typeof(AttendanceResource))]
        Work = 0,
        [Localization(nameof(AttendanceResource.lblAnnualVacation), typeof(AttendanceResource))]
        AnnualVacation = 1,
        [Localization(nameof(AttendanceResource.lblOfficialVacation), typeof(AttendanceResource))]
        OfficialVacation = 2,
        [Localization(nameof(AttendanceResource.lblPetitionVacation), typeof(AttendanceResource))]
        PetitionVacation = 3,
        [Localization(nameof(AttendanceResource.lblSpecialVacation), typeof(AttendanceResource))]
        SpecialVacation = 4,
        [Localization(nameof(AttendanceResource.lblSleepOut), typeof(AttendanceResource))]
        SleepOut = 5,
        [Localization(nameof(AttendanceResource.lblGoOut), typeof(AttendanceResource))]
        GoOut = 6,
        [Localization(nameof(AttendanceResource.lblVisitOut), typeof(AttendanceResource))]
        VisitOut = 7,
        [Localization(nameof(AttendanceResource.lblHospitalization), typeof(AttendanceResource))]
        Hospitalization = 8,
    }


    public enum VisitSettingType
    {
        [Localization(nameof(VisitResource.lblVisitNoStep), typeof(DeviceResource))]
        NoStep,
        [Localization(nameof(VisitResource.lblVisitFirstStep), typeof(DeviceResource))]
        FirstStep,
        [Localization(nameof(VisitResource.lblVisitSecondStep), typeof(DeviceResource))]
        SecondStep
    }

    /// <summary>
    /// All headers related with user are in this enum list.
    /// </summary>
    //public enum UserHeaderColumns
    //{
    //    [Localization(nameof(UserResource.lblId), typeof(UserResource))]
    //    Id = 0,
    //    [Localization(nameof(UserResource.lblName), typeof(UserResource))]
    //    FirstName,
    //    [Localization(nameof(UserResource.lblUserCode), typeof(UserResource))]
    //    UserCode,
    //    [Localization(nameof(UserResource.lblDepartment), typeof(UserResource))]
    //    DepartmentName,
    //    [Localization(nameof(UserResource.lblEmployeeNumber), typeof(UserResource))]
    //    EmployeeNo,
    //    [Localization(nameof(UserResource.lblMilitaryNumber), typeof(UserResource))]
    //    MilitaryNo,
    //    [Localization(nameof(UserResource.lblExpiredDate), typeof(UserResource))]
    //    ExpiredDate,
    //    [Localization(nameof(UserResource.lblCardId), typeof(UserResource))]
    //    CardList,
    //    [Localization(nameof(UserResource.lblAction), typeof(UserResource))]
    //    Action,
    //    [Localization(nameof(UserResource.lblApprovalStatus), typeof(UserResource))]
    //    ApprovalStatus,
    //    [Localization(nameof(EventLogResource.lblPlateNumber), typeof(EventLogResource))]
    //    PlateNumberList,
    //    [Localization(nameof(UserResource.lblPosition), typeof(UserResource))]
    //    Position,
    //    [Localization(nameof(UserResource.lblWorkType), typeof(UserResource))]
    //    WorkTypeName,
    //    [Localization(nameof(UserResource.lblAccessGroup), typeof(UserResource))]
    //    AccessGroupName,
    //    [Localization(nameof(AccountResource.lblAccount), typeof(AccountResource))]
    //    Email,
    //}

    public enum UserHeaderColumn
    {
        [Localization(nameof(UserResource.lblName), typeof(UserResource))]
        FirstName = 0,
        [Localization(nameof(UserResource.lblUserCode), typeof(UserResource))]
        UserCode,
        [Localization(nameof(UserResource.lblDepartment), typeof(UserResource))]
        DepartmentName,
        [Localization(nameof(UserResource.lblEmployeeNumber), typeof(UserResource))]
        EmployeeNo,
        [Localization(nameof(UserResource.lblMilitaryNumber), typeof(UserResource))]
        MilitaryNo,
        [Localization(nameof(UserResource.lblExpiredDate), typeof(UserResource))]
        ExpiredDate,
        [Localization(nameof(UserResource.lblCardId), typeof(UserResource))]
        CardList,
        [Localization(nameof(UserResource.lblAction), typeof(UserResource))]
        Action,
        [Localization(nameof(UserResource.lblApprovalStatus), typeof(UserResource))]
        ApprovalStatus,
        [Localization(nameof(EventLogResource.lblPlateNumber), typeof(EventLogResource))]
        PlateNumberList,
        [Localization(nameof(UserResource.lblPosition), typeof(UserResource))]
        Position,
        [Localization(nameof(UserResource.lblWorkType), typeof(UserResource))]
        WorkTypeName,
        [Localization(nameof(UserResource.lblAccessGroup), typeof(UserResource))]
        AccessGroupName
    }

    public enum AccessibleUserHeader
    {
        [Localization(nameof(UserResource.lblName), typeof(UserResource))]
        FirstName = 0,
        [Localization(nameof(UserResource.lblDepartment), typeof(UserResource))]
        DepartmentName,
        [Localization(nameof(UserResource.lblEmployeeNumber), typeof(UserResource))]
        EmployeeNo,
        [Localization(nameof(UserResource.lblExpiredDate), typeof(UserResource))]
        ExpiredDate,
        [Localization(nameof(UserResource.lblCardId), typeof(UserResource))]
        CardList,
        [Localization(nameof(EventLogResource.lblPlateNumber), typeof(EventLogResource))]
        PlateNumberList,
        [Localization(nameof(UserResource.lblPosition), typeof(UserResource))]
        Position,
        [Localization(nameof(UserResource.lblWorkType), typeof(UserResource))]
        WorkTypeName,
        //[Localization(nameof(UserResource.lblAction), typeof(UserResource))]
        //Action
    }

    public enum UserArmyHeaderColumn
    {
        [Localization(nameof(UserResource.lblId), typeof(UserResource))]
        Id = 0,
        [Localization(nameof(UserResource.lblName), typeof(UserResource))]
        FirstName,
        [Localization(nameof(UserResource.lblUserCode), typeof(UserResource))]
        UserCode,
        [Localization(nameof(UserResource.lblDepartment), typeof(UserResource))]
        DepartmentName,
        [Localization(nameof(UserResource.lblMilitaryNumber), typeof(UserResource))]
        MilitaryNo,
        [Localization(nameof(UserResource.lblPosition), typeof(UserResource))]
        Position,
        [Localization(nameof(UserResource.lblExpiredDate), typeof(UserResource))]
        ExpiredDate,
        [Localization(nameof(UserResource.lblArmyWorkType), typeof(UserResource))]
        WorkType,
        [Localization(nameof(UserResource.lblCardId), typeof(UserResource))]
        CardList,
        [Localization(nameof(UserResource.lblAction), typeof(UserResource))]
        Action,
        [Localization(nameof(EventLogResource.lblPlateNumber), typeof(EventLogResource))]
        PlateNumberList,
    }

    public enum AccessibleArmyUserHeader
    {
        [Localization(nameof(UserResource.lblId), typeof(UserResource))]
        Id = 0,
        [Localization(nameof(UserResource.lblName), typeof(UserResource))]
        FirstName,
        [Localization(nameof(UserResource.lblDepartment), typeof(UserResource))]
        DepartmentName,
        [Localization(nameof(UserResource.lblMilitaryNumber), typeof(UserResource))]
        MilitaryNo,
        [Localization(nameof(UserResource.lblExpiredDate), typeof(UserResource))]
        ExpiredDate,
        [Localization(nameof(UserResource.lblCardId), typeof(UserResource))]
        CardList,
        [Localization(nameof(EventLogResource.lblPlateNumber), typeof(EventLogResource))]
        PlateNumberList,
        [Localization(nameof(UserResource.lblPosition), typeof(UserResource))]
        Position,
        [Localization(nameof(UserResource.lblWorkType), typeof(UserResource))]
        WorkTypeName,
        [Localization(nameof(UserResource.lblAction), typeof(UserResource))]
        Action
    }

    public enum VehicleEventLogHeaderColumn
    {
        [Localization(nameof(UserResource.lblId), typeof(UserResource))]
        Id = 0,
        [Localization(nameof(EventLogResource.lblAccessTime), typeof(EventLogResource))]
        EventTime,
        [Localization(nameof(EventLogResource.lblDoorName), typeof(EventLogResource))]
        DoorName,
        [Localization(nameof(EventLogResource.lblVehicleModel), typeof(EventLogResource))]
        Model,
        [Localization(nameof(EventLogResource.lblPlateNumber), typeof(EventLogResource))]
        PlateNumber,
        [Localization(nameof(EventLogResource.lblDepartment), typeof(EventLogResource))]
        DepartmentName,
        [Localization(nameof(EventLogResource.lblUserName), typeof(EventLogResource))]
        UserName,
        [Localization(nameof(EventLogResource.lblEventDetail), typeof(EventLogResource))]
        EventDetail,
        [Localization(nameof(EventLogResource.lblInOut), typeof(EventLogResource))]
        InOut,
        [Localization(nameof(EventLogResource.lblVehicleImage), typeof(EventLogResource))]
        VehicleImage,
        [Localization(nameof(EventLogResource.lblVehicleType), typeof(EventLogResource))]
        VehicleType,
    }

    public enum CultureCodes
    {
        [Localization(nameof(CommonResource.lblEnglish), typeof(CommonResource))]
        English = 0,
        [Localization(nameof(CommonResource.lblJapanese), typeof(CommonResource))]
        Japanese = 1,
        [Localization(nameof(CommonResource.lblKorean), typeof(CommonResource))]
        Korean = 2,
        [Localization(nameof(CommonResource.lblVietnamese), typeof(CommonResource))]
        Vietnamese = 3,
    }

    public enum PreferredSystem
    {
        [Localization(nameof(CommonResource.lblAccessSystem), typeof(CommonResource))]
        AccessSystem = 0,
        [Localization(nameof(CommonResource.lblCanteenSystem), typeof(CommonResource))]
        CanteenSystem = 1,
        [Localization(nameof(CommonResource.lblCardIssuingSystem), typeof(CommonResource))]
        CardIssuingSystem = 2,
    }

    public enum EBKNMode
    {
        [Localization(nameof(DeviceResource.verifyFaceOrFingerPrintOrPassCode), typeof(DeviceResource))]
        FaceOrFingerprintOrPassCode = 1,
        [Localization(nameof(DeviceResource.verifyFingerprintOnly), typeof(DeviceResource))]
        FingerprintOnly = 2,
        [Localization(nameof(DeviceResource.verifyFingerprintAndPassCode), typeof(DeviceResource))]
        FingerprintAndPassCode = 8,
        [Localization(nameof(DeviceResource.verifyFace), typeof(DeviceResource))]
        Face = 10,
        [Localization(nameof(DeviceResource.verifyFaceAndPassCode), typeof(DeviceResource))]
        FaceAndPassCode = 12,
        [Localization(nameof(DeviceResource.verifyFingerprintAndFace), typeof(DeviceResource))]
        FingerprintAndFace = 14,
    }

    /// <summary>
    /// Verify Mode
    /// </summary>
    public enum VerifyMode
    {
        [Localization(nameof(DeviceResource.verifyCard), typeof(DeviceResource))]
        Card = 1,
        [Localization(nameof(DeviceResource.verifyQR), typeof(DeviceResource))]
        QR = 2,
        [Localization(nameof(DeviceResource.verifyFace), typeof(DeviceResource))]
        Face = 3,
        //[Localization(nameof(DeviceResource.verifyIris), typeof(DeviceResource))]
        //Iris = 4,
        [Localization(nameof(DeviceResource.verifyVehicle), typeof(DeviceResource))]
        Vehicle = 5,
        [Localization(nameof(DeviceResource.verifyPassCode), typeof(DeviceResource))]
        PassCode = 9,
        [Localization(nameof(DeviceResource.verifyCardAndQR), typeof(DeviceResource))]
        CardAndQR = 12,
        [Localization(nameof(DeviceResource.verifyFaceAndPassCode), typeof(DeviceResource))]
        FaceAndPassCode = 13,
        [Localization(nameof(DeviceResource.verifyFingerprintAndFace), typeof(DeviceResource))]
        FingerprintAndFace = 14,
        [Localization(nameof(DeviceResource.verifyVehicleAndFace), typeof(DeviceResource))]
        VehicleAndFace = 15,
        [Localization(nameof(DeviceResource.verifyCardAndPassCode), typeof(DeviceResource))]
        CardAndPassCode = 19,
        [Localization(nameof(DeviceResource.verifyFingerprintOnly), typeof(DeviceResource))]
        FingerprintOnly = 20,
        [Localization(nameof(DeviceResource.verifyCardOrFingerprint), typeof(DeviceResource))]
        CardOrFingerprint = 21,
        [Localization(nameof(DeviceResource.verifyCardAndFingerprint), typeof(DeviceResource))]
        CardAndFingerprint = 22,
        [Localization(nameof(DeviceResource.verifyFingerprintAndPassCode), typeof(DeviceResource))]
        FingerprintAndPassCode = 23,
        [Localization(nameof(DeviceResource.verifyCardOrQrOrFingerprintOrFaceId), typeof(DeviceResource))]
        CardOrQrOrFingerprintOrFaceId = 24,
        //[Localization(nameof(DeviceResource.verifyFaceOrIris), typeof(DeviceResource))]
        //FaceAndIris = 34,
        [Localization(nameof(DeviceResource.verifyCardAndFace), typeof(DeviceResource))]
        CardAndFace = 29,
        [Localization(nameof(DeviceResource.verifyCardAndFaceAndPassCode), typeof(DeviceResource))]
        CardAndFaceAndPassCode = 30,
        [Localization(nameof(DeviceResource.verifyFaceOrFingerPrintOrPassCode), typeof(DeviceResource))]
        FaceOrFingerprintOrPassCode = 31,
        [Localization(nameof(DeviceResource.verifyPlateNumberAndVNID), typeof(DeviceResource))]
        PlateNumberAndVNID = 32,
        [Localization(nameof(DeviceResource.verifyVNID), typeof(DeviceResource))]
        VNID = 33,
        [Localization(nameof(DeviceResource.verifyCardOrQR), typeof(DeviceResource))]
        CardOrQR = 120,
        [Localization(nameof(DeviceResource.VerifyCardOrPassCode), typeof(DeviceResource))]
        CardOrPassCode = 190,
        [Localization(nameof(DeviceResource.verifyFaceOrIris), typeof(DeviceResource))]
        FaceOrIris = 340,
        [Localization(nameof(DeviceResource.VerifyCardOrQROrPassCode), typeof(DeviceResource))]
        CardOrQROrPassCode = 1290,
        [Localization(nameof(DeviceResource.verifyCardOrFaceOrIris), typeof(DeviceResource))]
        CardOrFaceOrIris = 1340,
    }

    public enum SingleVerify
    {
        [Localization(nameof(DeviceResource.verifyCard), typeof(DeviceResource))]
        Card = 1,
        [Localization(nameof(DeviceResource.verifyQR), typeof(DeviceResource))]
        QR = 2,
        [Localization(nameof(DeviceResource.verifyFace), typeof(DeviceResource))]
        Face = 3,
        //[Localization(nameof(DeviceResource.verifyIris), typeof(DeviceResource))]
        //Iris = 4,
        [Localization(nameof(DeviceResource.verifyVehicle), typeof(DeviceResource))]
        Vehicle = 5,
        [Localization(nameof(DeviceResource.verifyPassCode), typeof(DeviceResource))]
        PassCode = 9,
        [Localization(nameof(DeviceResource.verifyFingerprintOnly), typeof(DeviceResource))]
        FingerprintOnly = 20,
        [Localization(nameof(DeviceResource.verifyVNID), typeof(DeviceResource))]
        VNID = 33,
    }
    public enum BioStationMode
    {
        [Localization(nameof(DeviceResource.lblNone), typeof(DeviceResource))]
        None = 0,
        [Localization(nameof(DeviceResource.lblMaster), typeof(DeviceResource))]
        Master = 1,
        [Localization(nameof(DeviceResource.lblSlave), typeof(DeviceResource))]
        Slave = 2,
        [Localization(nameof(DeviceResource.lblStandalone), typeof(DeviceResource))]
        Standalone = 3,
    }

    public enum SingleVerifyOR
    {
        [Localization(nameof(DeviceResource.verifyCardOrQrOrFingerprintOrFaceId), typeof(DeviceResource))]
        CardOrQrOrFingerprintOrFaceId = 24,
        [Localization(nameof(DeviceResource.verifyCardOrQR), typeof(DeviceResource))]
        CardOrQR = 120,
        [Localization(nameof(DeviceResource.VerifyCardOrPassCode), typeof(DeviceResource))]
        CardOrPassCode = 190,
        [Localization(nameof(DeviceResource.verifyFaceOrIris), typeof(DeviceResource))]
        FaceOrIris = 340,
        [Localization(nameof(DeviceResource.verifyCardOrFaceOrIris), typeof(DeviceResource))]
        CardOrFaceOrIris = 1340,
        [Localization(nameof(DeviceResource.VerifyCardOrQROrPassCode), typeof(DeviceResource))]
        CardOrQROrPassCode = 1290,
        [Localization(nameof(DeviceResource.verifyCardOrFingerprint), typeof(DeviceResource))]
        CardOrFingerprint = 21,
        [Localization(nameof(DeviceResource.verifyFaceOrFingerPrintOrPassCode), typeof(DeviceResource))]
        FaceOrFingerprintOrPassCode = 31,
    }

    public enum MultiVerify
    {
        [Localization(nameof(DeviceResource.verifyCardAndQR), typeof(DeviceResource))]
        CardAndQR = 12,
        [Localization(nameof(DeviceResource.verifyFaceAndPassCode), typeof(DeviceResource))]
        FaceAndPassCode = 13,
        [Localization(nameof(DeviceResource.verifyFingerprintAndFace), typeof(DeviceResource))]
        FingerprintAndFace = 14,
        [Localization(nameof(DeviceResource.verifyVehicleAndFace), typeof(DeviceResource))]
        VehicleAndFace = 15,
        [Localization(nameof(DeviceResource.verifyCardAndPassCode), typeof(DeviceResource))]
        CardAndPassCode = 19,
        [Localization(nameof(DeviceResource.verifyCardAndFingerprint), typeof(DeviceResource))]
        CardAndFingerprint = 22,
        [Localization(nameof(DeviceResource.verifyFingerprintAndPassCode), typeof(DeviceResource))]
        FingerprintAndPassCode = 23,
        //[Localization(nameof(DeviceResource.verifyFaceAndIris), typeof(DeviceResource))]
        //FaceAndIris = 34,
        [Localization(nameof(DeviceResource.verifyCardAndFace), typeof(DeviceResource))]
        CardAndFace = 29,  //must 22 but it already in CardAndFingerprint
        [Localization(nameof(DeviceResource.verifyCardAndFaceAndPassCode), typeof(DeviceResource))]
        CardAndFaceAndPassCode = 30,
        [Localization(nameof(DeviceResource.verifyPlateNumberAndVNID), typeof(DeviceResource))]
        PlateNumberAndVNID = 32,
    }

    public enum Registertype
    {
        [Localization(nameof(DeviceResource.NewDevice), typeof(DeviceResource))]
        NewDevice = 0,
        [Localization(nameof(DeviceResource.ReplaceDevice), typeof(DeviceResource))]
        Replace = 1,
        [Localization(nameof(DeviceResource.RelocatedDevice), typeof(DeviceResource))]
        Relocation = 2,
        //[Localization(nameof(DeviceResource.), typeof(DeviceResource))]
        // = 3,
    }

    public enum WorkingHourType
    {
        [Localization(nameof(AttendanceResource.lblOnlyInOffice), typeof(AttendanceResource))]
        OnlyInOffice = 0,
        [Localization(nameof(AttendanceResource.lblTotalInOffice), typeof(AttendanceResource))]
        TotalInCompany = 1
    }

    public enum TimeFormatType
    {
        [Localization(nameof(AttendanceResource.lblHHmmss), typeof(AttendanceResource))]
        HHmmss = 0,
        [Localization(nameof(AttendanceResource.lblHHmm), typeof(AttendanceResource))]
        HHmm = 1,
        [Localization(nameof(AttendanceResource.lblHHmmDot), typeof(AttendanceResource))]
        HHmmDot = 2,
        OnlyHours = 3,
    }

    public enum AttendanceReportType
    {
        [Localization(nameof(AttendanceResource.lblAttendanceReportType0), typeof(AttendanceResource))]
        Type0 = 0,
        [Localization(nameof(AttendanceResource.lblAttendanceReportType1), typeof(AttendanceResource))]
        Type1 = 1,
        [Localization(nameof(AttendanceResource.lblAttendanceReportType2), typeof(AttendanceResource))]
        Type2 = 2,
        [Localization(nameof(AttendanceResource.lblAttendanceReportType3), typeof(AttendanceResource))]
        Type3 = 3,
        [Localization(nameof(AttendanceResource.lblAttendanceReportType4), typeof(AttendanceResource))]
        Type4 = 4,
    }

    public enum CameraType
    {
        [Localization(nameof(CameraResource.lblCameraHanet), typeof(CameraResource))]
        CameraHanet = 0,
        [Localization(nameof(CameraResource.lblCameraLPR), typeof(CameraResource))]
        CameraLPR = 1,
        [Localization(nameof(CameraResource.lblCameraCCTV), typeof(CameraResource))]
        CameraCCTV = 2,
        [Localization(nameof(CameraResource.lblCameraDC), typeof(CameraResource))]
        CameraDC = 3,
    }

    public enum NotificationType
    {
        [Localization(nameof(NotificationResource.TitleNotificationAction), typeof(NotificationResource))]
        NotificationAction = 1,
        [Localization(nameof(NotificationResource.TitleNotificationInform), typeof(NotificationResource))]
        NotificationInform = 2,
        [Localization(nameof(NotificationResource.TitleNotificationEmergency), typeof(NotificationResource))]
        NotificationEmergency = 3,
        [Localization(nameof(NotificationResource.TitleNotificationWarning), typeof(NotificationResource))]
        NotificationWarning = 4,
        
        [Localization(nameof(NotificationResource.TitleNotificationAccess), typeof(NotificationResource))]
        NotificationAccess = 5,
        [Localization(nameof(NotificationResource.TitleNotificationAttendance), typeof(NotificationResource))]
        NotificationAttendance = 6,
        [Localization(nameof(NotificationResource.TitleNotificationVisit), typeof(NotificationResource))]
        NotificationVisit = 7,
        [Localization(nameof(NotificationResource.TitleNotificationDoors), typeof(NotificationResource))]
        NotificationDoors = 8,
        [Localization(nameof(NotificationResource.TitlleNotificationNotice), typeof(NotificationResource))]
        NotificationNotice = 9
    }


    public enum VehicleType
    {
        [Localization(nameof(DeviceResource.lblCar), typeof(DeviceResource))]
        Car = 0,
        [Localization(nameof(DeviceResource.lblMotoBike), typeof(DeviceResource))]
        MotoBike = 1,
    }

    public enum VehicleClass
    {
        [Localization(nameof(DeviceResource.lblNormalVehicle), typeof(DeviceResource))]
        NormalVehicle = 0,
        [Localization(nameof(DeviceResource.lblRFIDVehicle), typeof(DeviceResource))]
        RFIDVehicle = 1,
    }

    public enum VehicleStatus
    {
        [Localization(nameof(DeviceResource.lblVehicleAllow), typeof(DeviceResource))]
        Allow = 0,
        [Localization(nameof(DeviceResource.lblVehicleDisallow), typeof(DeviceResource))]
        Disallow = 1,
    }

    public enum VehicleRule
    {
        [Localization(nameof(DeviceResource.lblNormalVehicle), typeof(DeviceResource))]
        Normal = 0,
        [Localization(nameof(DeviceResource.lblRule2Vehicle), typeof(DeviceResource))]
        Rule2 = 1,
        [Localization(nameof(DeviceResource.lblExceptionVehicle), typeof(DeviceResource))]
        Exception = 2,
        [Localization(nameof(DeviceResource.lblViolationVehicle), typeof(DeviceResource))]
        Violation = 3,
        [Localization(nameof(DeviceResource.lblRule5Vehicle), typeof(DeviceResource))]
        Rule5 = 4,
    }

    public enum DayOfWeekOption
    {
        [Localization(nameof(AttendanceResource.lblSunday), typeof(AttendanceResource))]
        Sunday = 0,
        [Localization(nameof(AttendanceResource.lblMonday), typeof(AttendanceResource))]
        Monday = 1,
        [Localization(nameof(AttendanceResource.lblTuesday), typeof(AttendanceResource))]
        Tuesday = 2,
        [Localization(nameof(AttendanceResource.lblWednesday), typeof(AttendanceResource))]
        Wednesday = 3,
        [Localization(nameof(AttendanceResource.lblThursday), typeof(AttendanceResource))]
        Thursday = 4,
        [Localization(nameof(AttendanceResource.lblFriday), typeof(AttendanceResource))]
        Friday = 5,
        [Localization(nameof(AttendanceResource.lblSaturday), typeof(AttendanceResource))]
        Saturday = 6,
    }

    public enum ReportProblemType
    {
        [Localization(nameof(DeviceResource.msgDeviceNotWorking), typeof(DeviceResource))]
        DeviceNotWorking = 0,
        [Localization(nameof(DeviceResource.msgCameraNotRecognizeFace), typeof(DeviceResource))]
        CameraNotRecognizeFace = 1
    }

    public enum AlarmState
    {
        [Localization(nameof(DeviceResource.lblAlarmStateOff), typeof(DeviceResource))]
        Off = 0,
        [Localization(nameof(DeviceResource.lblAlarmStateOn), typeof(DeviceResource))]
        On = 1
    }
    
    public enum CardRole
    {
        [Localization(nameof(CardIssuingResource.lblTemporaryAccessCard), typeof(CardIssuingResource))]
        TemporaryAccessCard = 1,
        [Localization(nameof(CardIssuingResource.lblAccessCard), typeof(CardIssuingResource))]
        AccessCard = 2,
        [Localization(nameof(CardIssuingResource.lblVisitCard), typeof(CardIssuingResource))]
        VisitCard = 3
    }

    // public enum CardRoleType
    // {
    //     [Localization(nameof(CardIssuingResource.lblSoldier), typeof(CardIssuingResource))]
    //     Soldier = 1,
    //     [Localization(nameof(CardIssuingResource.lblFamily), typeof(CardIssuingResource))]
    //     Family = 2,
    //     [Localization(nameof(CardIssuingResource.lblRegularCivilian), typeof(CardIssuingResource))]
    //     RegularCivilian = 3,
    //     [Localization(nameof(CardIssuingResource.lblResidentCivilian), typeof(CardIssuingResource))]
    //     ResidentCivilian = 4,
    //     
    //     [Localization(nameof(CardIssuingResource.lblConstruction), typeof(CardIssuingResource))]
    //     Construction = 5,
    //     [Localization(nameof(CardIssuingResource.lblMeetingVisit), typeof(CardIssuingResource))]
    //     MeetingVisit = 6,
    //     [Localization(nameof(CardIssuingResource.lblTraning), typeof(CardIssuingResource))]
    //     Training = 7
    // }

    public enum IssuingDeviceType
    {
        [Localization(nameof(CardIssuingResource.lblIDPSmartCardPrinter), typeof(CardIssuingResource))]
        IDPSmartCardPrinter = 0,
        [Localization(nameof(CardIssuingResource.lblCXD80), typeof(CardIssuingResource))]
        CXD80 = 1,
        [Localization(nameof(CardIssuingResource.lblCardPrinter), typeof(CardIssuingResource))]
        CardPrinter = 2,
        [Localization(nameof(CardIssuingResource.lblCardReader), typeof(CardIssuingResource))]
        CardReader = 3,
    }
    
    public enum AlignmentType
    {
        [Localization(nameof(CardIssuingResource.lblVerticalAlignment), typeof(CardIssuingResource))]
        VerticalAlignment = 0,
        [Localization(nameof(CardIssuingResource.lblHorizontalAlignment), typeof(CardIssuingResource))]
        HorizontalAlignment = 1
    }
    
    public enum CardLayoutValueType
    {
        [Localization(nameof(CardIssuingResource.lblBackGround), typeof(CardIssuingResource))]
        BackGround = 0,
        [Localization(nameof(CardIssuingResource.lblPictureFixed), typeof(CardIssuingResource))]
        PictureFixed = 1,
        [Localization(nameof(CardIssuingResource.lblPictureUser), typeof(CardIssuingResource))]
        PictureUser = 2,
        [Localization(nameof(CardIssuingResource.lblTextFixed), typeof(CardIssuingResource))]
        TextFixed = 3,
        [Localization(nameof(CardIssuingResource.lblTextDbInfo), typeof(CardIssuingResource))]
        TextDbInfo = 4
    }


    public enum ApprovalStatus
    {
        [Localization(nameof(UserResource.lblNotUseApproval), typeof(UserResource))]
        NotUse = 0,
        [Localization(nameof(UserResource.lblApprovalWaiting1), typeof(UserResource))]
        ApprovalWaiting1,
        [Localization(nameof(UserResource.lblApprovalWaiting2), typeof(UserResource))]
        ApprovalWaiting2,
        [Localization(nameof(UserResource.lblApproved), typeof(UserResource))]
        Approved,
        [Localization(nameof(UserResource.lblRejected), typeof(UserResource))]
        Rejected,

        [Localization(nameof(UserResource.lblUpdateWaiting1), typeof(UserResource))]
        UpdateWaiting1,
        [Localization(nameof(UserResource.lblUpdateWaiting2), typeof(UserResource))]
        UpdateWaiting2,

        //[Localization(nameof(.lblIssueing), typeof())]
        //Issueing,
        //[Localization(nameof(.lblDeliveryWaiting), typeof())]
        //DeliveryWaiting,
        //[Localization(nameof(.lblDeliveryOk), typeof())]
        //DeliveryOk,
        //[Localization(nameof(.lblReturn), typeof())]
        //Return,
        //[Localization(nameof(.lblReject), typeof())]
        //Reject,
        //[Localization(nameof(.lblReclamation), typeof())]
        //Reclamation,
        //[Localization(nameof(.lblNotUse), typeof())]
        //NotUse,
        //[Localization(nameof(.lblPreRegister), typeof())]
        //PreRegister,
    }

    public enum TypeSubMonitoring
    {
        [Localization(nameof(MonitoringResource.typeMonitoring1), typeof(MonitoringResource))]
        Template1 = 1,
        [Localization(nameof(MonitoringResource.typeMonitoring2), typeof(MonitoringResource))]
        Template2 = 2,
        [Localization(nameof(MonitoringResource.typeMonitoring3), typeof(MonitoringResource))]
        Template3 = 3,
        [Localization(nameof(MonitoringResource.typeMonitoring4), typeof(MonitoringResource))]
        Template4 = 4,
        [Localization(nameof(MonitoringResource.typeMonitoring5), typeof(MonitoringResource))]
        Template5 = 5,
        [Localization(nameof(MonitoringResource.typeMonitoring6), typeof(MonitoringResource))]
        Template6 = 6,
        [Localization(nameof(MonitoringResource.typeMonitoring7), typeof(MonitoringResource))]
        Template7 = 7,
    }

    public enum BookStatus
    {
        [Localization(nameof(BookResource.lblAvailable), typeof(BookResource))]
        Available = 0,
        [Localization(nameof(BookResource.lblRented), typeof(BookResource))]
        Rented = 1,
    }
    
    public enum BorrowStatus
    {
        [Localization(nameof(BookResource.lblPaid), typeof(BookResource))]
        Paid = 0,
        [Localization(nameof(BookResource.lblUnPaid), typeof(BookResource))]
        UnPaid = 1,
    }

    public enum IssueTables
    {
        [Localization(nameof(UserResource.lblUser), typeof(UserResource))]
        User,
        [Localization(nameof(UserResource.lblDepartment), typeof(UserResource))]
        Department,
        [Localization(nameof(UserResource.lblCard), typeof(UserResource))]
        Card,
        [Localization(nameof(UserResource.lblVehicle), typeof(UserResource))]
        Vehicle,
    }


    public enum IssueUserColumn
    {
        [Localization(nameof(UserResource.lblAddress), typeof(UserResource))]
        Address,
        [Localization(nameof(UserResource.lblUserCode), typeof(UserResource))]
        UserCode,
        [Localization(nameof(UserResource.lblCity), typeof(UserResource))]
        City,
        [Localization(nameof(UserResource.lblExpiredDate), typeof(UserResource))]
        ExpiredDate,
        [Localization(nameof(UserResource.lblEffectiveDate), typeof(UserResource))]
        EffectiveDate,
        [Localization(nameof(UserResource.lblEmployeeNumber), typeof(UserResource))]
        EmpNumber,
        [Localization(nameof(UserResource.lblName), typeof(UserResource))]
        Name,
        [Localization(nameof(UserResource.lblEmail), typeof(UserResource))]
        Email,
        [Localization(nameof(UserResource.lblSex), typeof(UserResource))]
        Sex,
        [Localization(nameof(UserResource.lblBirthday), typeof(UserResource))]
        BirthDay,
        [Localization(nameof(UserResource.lblJob), typeof(UserResource))]
        Job,
        [Localization(nameof(UserResource.lblHomePhone), typeof(UserResource))]
        HomePhone,
        [Localization(nameof(UserResource.lblNationality), typeof(UserResource))]
        Nationality,
        [Localization(nameof(UserResource.lblOfficePhone), typeof(UserResource))]
        OfficePhone,
        [Localization(nameof(UserResource.lblPosition), typeof(UserResource))]
        Position,
        [Localization(nameof(UserResource.lblPostCode), typeof(UserResource))]
        PostCode,
        // [Localization(nameof(UserResource.lblMilitaryNumber), typeof(UserResource))]
        // MilitaryNumber,
        // [Localization(nameof(UserResource.lblArmyUserFamilyName), typeof(UserResource))]
        // ArmyUserFamily_Name,
        // [Localization(nameof(UserResource.lblArmyUserFamilyRank), typeof(UserResource))]
        // ArmyUserFamily_Rank,
        // [Localization(nameof(UserResource.lblArmyUserFamilyDepartment), typeof(UserResource))]
        // ArmyUserFamily_Department,
    }


    public enum IssueDepartmentColumn
    {
        [Localization(nameof(DepartmentResource.lblDepartmentName), typeof(DepartmentResource))]
        DepartName,
        [Localization(nameof(DepartmentResource.lblDepartmentNumber), typeof(DepartmentResource))]
        DepartNo,
    }



    public enum IssueCardColumn
    {
        [Localization(nameof(CardIssuingResource.lblCardId), typeof(CardIssuingResource))]
        CardId,
        [Localization(nameof(CardIssuingResource.lblManagementNumber), typeof(CardIssuingResource))]
        ManagementNumber,
    }


    public enum IssueVehicleColumn
    {
        [Localization(nameof(EventLogResource.lblPlateNumber), typeof(EventLogResource))]
        PlateNumber,
        [Localization(nameof(EventLogResource.lblVehicleModel), typeof(EventLogResource))]
        Model,
        [Localization(nameof(EventLogResource.lblPlateRFID), typeof(EventLogResource))]
        PlateRFID,
        [Localization(nameof(EventLogResource.lblVehicleType), typeof(EventLogResource))]
        Vehicle_Type,
        [Localization(nameof(EventLogResource.lblVehicleClassification), typeof(EventLogResource))]
        Vehicle_Classification,
    }

    public enum RabbitMqConnections
    {
        ConnectionDefault = 0,
        AccessControl = 1,
        SendMessageToWebapp = 2,
        LoadDeviceInfo = 3,
        WebHookHanet = 4,
        ImportData = 5,
        AccessControlElevator = 6,
        EbknHandlerMessage = 7,
        AccessTime = 8,

        RefreshDeviceInfo = 9,
        
        ConsumerService = 100,
        CronjobQueue = 101,
        HandleResponseConsumer = 102,
        DeviceConnectionStatusConsumer = 103,
        DoorStatusConsumer = 104,
        ConvertVideo = 105,
        ExportDataToFile = 106,
    }

    public enum TransactionType
    {
        [Localization(nameof(TransactionResource.addType), typeof(TransactionResource))]
        AddType = 0,
        [Localization(nameof(TransactionResource.deleteType), typeof(TransactionResource))]
        DeleteType = 1,
        [Localization(nameof(TransactionResource.visitType), typeof(TransactionResource))]
        VisitType = 2,
        [Localization(nameof(TransactionResource.UserType), typeof(TransactionResource))]
        UserType = 3,
    }


    public enum ListDataType
    {
        /// <summary>
        /// Header setting
        /// </summary>
        [Localization(nameof(SettingResource.lblHeaderSetting), typeof(SettingResource))]
        Header = 1,

        /// <summary>
        /// List of doors setting (filtering)
        /// </summary>
        [Localization(nameof(SettingResource.lblDoorListSetting), typeof(SettingResource))]
        DoorListSettings = 2,

        /// <summary>
        /// List of events setting (filtering)
        /// </summary>
        [Localization(nameof(SettingResource.lblMonitoringEventsSetting), typeof(SettingResource))]
        MonitoringEvents = 3,

        /// <summary>
        /// List of buildings setting (filtering)
        /// </summary>
        [Localization(nameof(SettingResource.lblBuildingListSetting), typeof(SettingResource))]
        BuildingListSettings = 4,
    }


    public enum ListLanguage
    {
        [Localization(nameof(SettingResource.english_language), typeof(SettingResource))]
        English = 0,
        [Localization(nameof(SettingResource.japan_language), typeof(SettingResource))]
        Japan = 1,
        [Localization(nameof(SettingResource.korean_language), typeof(SettingResource))]
        Korea = 2,
        [Localization(nameof(SettingResource.vietnamese_language), typeof(SettingResource))]
        VietNam = 3,
        [Localization(nameof(SettingResource.vietnamese_school_language), typeof(SettingResource))]
        VietNamSchool = 4,
    }

    public enum LogFileDeviceStatus
    {
        NoFile = 0,
        OldFile = 1,
        NewFile = 2,
    }

    public enum VideoCallPriorityType
    {
        ReceptionToUser = 0,
        UserToReception = 1,
        LobbyToReception = 2,
        LobbyToUser = 3,
        UserToLobby = 4,
        Other = 5,
    }

    public enum CardType_IT100
    {
        /// <summary>
        /// No face data
        /// </summary>
        [Localization(nameof(UserResource.lblNone), typeof(UserResource))]
        None = 0,

        /// <summary>
        /// There are face data and iris data both.
        /// </summary>
        [Localization(nameof(UserResource.lblFaceAndIris), typeof(UserResource))]
        FaceAndIris = 1,

        /// <summary>
        /// There is only face data.
        /// </summary>
        [Localization(nameof(UserResource.lblFaceOnly), typeof(UserResource))]
        FaceOnly = 2,

        /// <summary>
        /// There is only iris data.
        /// </summary>
        [Localization(nameof(UserResource.lblIrisOnly), typeof(UserResource))]
        IrisOnly = 3
    }

    public enum ObjectTypeEvent
    {
        Warning = 0,
        User = 1,
        Visit = 2,
    }

    public enum IdentificationType
    {
        [Localization(nameof(VisitResource.lblCMND), typeof(VisitResource))]
        CMND = 0,
        [Localization(nameof(VisitResource.lblCCCD), typeof(VisitResource))]
        CCCD = 1,
        [Localization(nameof(VisitResource.lblPassport), typeof(VisitResource))]
        Passport = 2,
        [Localization(nameof(VisitResource.lblIdentificationOther), typeof(VisitResource))]
        Other = 3,
    }
    
    public enum CronjobType
    {
        ScheduleEveryTime = 0,
        ScheduleEveryDay = 1,
        ScheduleEveryMonth = 2,
    }

    public enum CronjobTypeName
    {
        UpdateUptimePerADay,
        CheckNotifyOverDueBook,
        AutoDeleteNotification,
        BackupFileToS3,
        CheckLimitStoredFileMedia,
        SyncCheckInStudent,
        RecheckAttendanceCheckin,
        CheckImageEventLogCameraHanet,
        CameraInfoCronJob,
        SendDeviceCommonInstruction,
        CheckVisitorExpired,
        CheckMeetingSendToDevice,
        CreateAttendanceNewDay,
        NotifyUserCheckinLate,
        CheckHFaceIdSyncBetweenDmpAndHanet,
        BackupEventLog,
        BackupSystemLog,
        ResetLoginFailCount,
    }
}
