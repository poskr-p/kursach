using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using material_design.Repositories;

namespace material_design.Services
{
    public class BackupService : IBackupService
    {
        private readonly cafe_barEntities _context;

        public BackupService(cafe_barEntities context)
        {
            _context = context;
        }

        public void BackupDatabase()
        {
            string backupFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CafeBarBackups");
            Directory.CreateDirectory(backupFolder);

            string backupFile = Path.Combine(backupFolder, $"cafe_bar_backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");

            string backupQuery = $@"BACKUP DATABASE [cafe_bar] TO DISK = '{backupFile}' WITH FORMAT, COMPRESSION;";
            _context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, backupQuery);
        }

        public void RestoreDatabase(string backupFilePath)
        {
            // Переключаемся на master и восстанавливаем
            _context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,
                "USE master; ALTER DATABASE [cafe_bar] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");

            string restoreQuery = $@"RESTORE DATABASE [cafe_bar] FROM DISK = '{backupFilePath}' WITH REPLACE;";
            _context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, restoreQuery);

            _context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,
                "ALTER DATABASE [cafe_bar] SET MULTI_USER;");
        }
    }
}