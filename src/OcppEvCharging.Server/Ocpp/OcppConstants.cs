namespace OcppEvCharging.Server.Ocpp;

public static class OcppConstants
{
    public const int MessageTypeCall = 2;
    public const int MessageTypeCallResult = 3;
    public const int MessageTypeCallError = 4;

    public const string ProtocolOcpp16 = "ocpp1.6";

    public static class Actions
    {
        public const string BootNotification = "BootNotification";
        public const string Heartbeat = "Heartbeat";
        public const string Authorize = "Authorize";
        public const string StatusNotification = "StatusNotification";
        public const string MeterValues = "MeterValues";
        public const string StartTransaction = "StartTransaction";
        public const string StopTransaction = "StopTransaction";
        public const string DataTransfer = "DataTransfer";
        public const string DiagnosticsStatusNotification = "DiagnosticsStatusNotification";
        public const string FirmwareStatusNotification = "FirmwareStatusNotification";
    }

    public static class ErrorCodes
    {
        public const string NotImplemented = "NotImplemented";
        public const string NotSupported = "NotSupported";
        public const string InternalError = "InternalError";
        public const string ProtocolError = "ProtocolError";
        public const string FormationViolation = "FormationViolation";
        public const string GenericError = "GenericError";
    }
}
