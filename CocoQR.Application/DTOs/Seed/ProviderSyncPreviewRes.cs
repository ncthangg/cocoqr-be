using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.Seed
{
    public class ProviderSyncPreviewRes
    {
        public string SourceFile { get; set; }
        public DateTime FileLastModified { get; set; }
        public ProviderSyncSummary Summary { get; set; }
        public List<ProviderSyncDiffItem> Changes { get; set; }
    }

    public class ProviderSyncSummary
    {
        public int TotalInFile { get; set; }
        public int TotalInDb { get; set; }
        public int ToInsert { get; set; }
        public int ToUpdate { get; set; }
        public int ToDelete { get; set; }
        public int Unchanged { get; set; }
    }
    public enum ProviderSyncAction { Insert, Update, Delete, Unchanged }

    public class ProviderSyncDiffItem
    {
        public ProviderSyncAction Action { get; set; }
        public ProviderCode ProviderCode { get; set; }
        public string ProviderName { get; set; }

        // Chỉ có giá trị khi Action = Update
        public List<ProviderFieldDiff> Diffs { get; set; }
    }

    public class ProviderFieldDiff
    {
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
