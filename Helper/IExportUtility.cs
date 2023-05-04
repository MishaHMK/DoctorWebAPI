using NPOI.SS.UserModel;

namespace DoctorWebApi.Helper
{
    public interface IExportUtility
    {
        IWorkbook WriteExcelWithNPOI<T>(List<T> data, string extension);
    }
}
