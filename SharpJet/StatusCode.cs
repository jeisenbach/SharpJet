namespace Hbm.Devices.Jet
{
    public enum StatusCode
    {
        Success = 0,
        ChangeWithoutAdd = -1,
        RemoveWithoutAdd = -2,
        MultipleAdd = -3,
        ParamsNotSpecified = -4,
        FetchEventNotSpecified = -5
    }
}
