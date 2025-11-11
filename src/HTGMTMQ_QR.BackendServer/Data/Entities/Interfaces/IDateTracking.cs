namespace HTGMTMQ_QR.BackendServer.Data.Entities.Interfaces
{
    public interface IDateTracking
    {
        DateTime CreateDate { get; set; }
        DateTime LastModifiedDate { get; set; }
    }
}
