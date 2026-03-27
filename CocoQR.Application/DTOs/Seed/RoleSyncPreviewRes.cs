namespace CocoQR.Application.DTOs.Seed
{
    public class RoleSyncPreviewRes
    {
        public string SourceFile { get; set; }
        public DateTime FileLastModified { get; set; }
        public RoleSyncSummary Summary { get; set; }
        public List<RoleSyncDiffItem> Changes { get; set; }
    }

    public class RoleSyncSummary
    {
        public int TotalInFile { get; set; }
        public int TotalInDb { get; set; }
        public int ToInsert { get; set; }
        public int ToUpdate { get; set; }
        public int ToDelete { get; set; }
        public int Unchanged { get; set; }
    }
    public enum RoleSyncAction { Insert, Update, Delete, Unchanged }

    public class RoleSyncDiffItem
    {
        public RoleSyncAction Action { get; set; }
        public string RoleName { get; set; }

        // Chỉ có giá trị khi Action = Update
        public List<RoleFieldDiff> Diffs { get; set; }
    }

    public class RoleFieldDiff
    {
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

}
