namespace MyWallet.Application.DTOs.Seed
{
    public class BankSyncPreviewRes
    {
        public string SourceFile { get; set; }        // "banks_v1.json"
        public DateTime FileLastModified { get; set; }
        public BankSyncSummary Summary { get; set; }
        public List<BankSyncDiffItem> Changes { get; set; }
    }

    public class BankSyncSummary
    {
        public int TotalInFile { get; set; }
        public int TotalInDb { get; set; }
        public int ToInsert { get; set; }
        public int ToUpdate { get; set; }
        public int ToDelete { get; set; }
        public int Unchanged { get; set; }
    }
    public enum BankSyncAction { Insert, Update, Delete, Unchanged }

    public class BankSyncDiffItem
    {
        public BankSyncAction Action { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public bool IsActive { get; set; }

        // Chỉ có giá trị khi Action = Update
        public List<BankFieldDiff> Diffs { get; set; }
    }

    public class BankFieldDiff
    {
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
