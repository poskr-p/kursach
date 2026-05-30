namespace material_design.Services
{
    public interface IBackupService
    {
        void BackupDatabase();
        void RestoreDatabase(string backupFilePath);
    }
}