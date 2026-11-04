namespace Honeywell.Database
{
    /// <summary>A row the user can flag in Excel to have it removed when the workbook is imported.</summary>
    public interface IDeleteOnImport
    {
        bool DeleteOnImport { get; set; }
    }
}
