using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

using FluentAssertions;

using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Management.Smo;

using SqlBackup.Database;

using Xunit;

namespace SqlBackupTest
{
    public class BackupStoreTest
    {
        [Fact]
        public void BackupDatabaseFiles_ShouldReturnFullBackupFiles()
        {
            IOptions<BackupStoreSettings> options = Options.Create(new BackupStoreSettings { BackupFileExtensions = { "BAK" }, BackupPaths = { Path.Combine(AppContext.BaseDirectory, "Bak") } });

            var store = new BackupStore(GetServer(), new FileSystem(), options);

            List<BackupDatabaseFile> backupFiles = store.BackupDatabaseFiles;
            backupFiles.Should().HaveCount(10);
            BackupDatabaseFile file = backupFiles[0];
            file.LogicalName.Should().Be("Test");
            file.PhysicalName.Should().EndWith("Test.mdf");
            file = backupFiles[1];
            file.LogicalName.Should().Be("Test_log");
            file.PhysicalName.Should().EndWith("Test_log.ldf");
            file = backupFiles[2];
            file.LogicalName.Should().Be("Test");
            file.PhysicalName.Should().EndWith("Test.mdf");
            file = backupFiles[3];
            file.LogicalName.Should().Be("Test_log");
            file.PhysicalName.Should().EndWith("Test_log.ldf");
            file = backupFiles[4];
            file.LogicalName.Should().Be("Test");
            file.PhysicalName.Should().EndWith("Test.mdf");
            file = backupFiles[5];
            file.LogicalName.Should().Be("Test_log");
            file.PhysicalName.Should().EndWith("Test_log.ldf");
            file = backupFiles[6];
            file.LogicalName.Should().Be("Test");
            file.PhysicalName.Should().EndWith("Test.mdf");
            file = backupFiles[7];
            file.LogicalName.Should().Be("Test_log");
            file.PhysicalName.Should().EndWith("Test_log.ldf");
            file = backupFiles[8];
            file.LogicalName.Should().Be("Test");
            file.PhysicalName.Should().EndWith("Test.mdf");
            file = backupFiles[9];
            file.LogicalName.Should().Be("Test_log");
            file.PhysicalName.Should().EndWith("Test_log.ldf");
        }

        [Fact]
        public void BackupFiles_MockC_ShouldReturnOneFile()
        {
            IOptions<BackupStoreSettings> options = Options.Create(new BackupStoreSettings { BackupFileExtensions = { "BAK" }, BackupPaths = { @"C:\" } });

            var store = new BackupStore(GetServer(), GetFileSystem(), options);

            List<string> backupFiles = store.BackupFiles;
            backupFiles.Should().HaveCount(1);
            backupFiles.Should().Contain(@"c:\backfile1.bak");
        }

        [Fact]
        public void BackupFiles_ShouldReturnFiles()
        {
            IOptions<BackupStoreSettings> options = Options.Create(new BackupStoreSettings { BackupFileExtensions = { "BAK" }, BackupPaths = { Path.Combine(AppContext.BaseDirectory, "Bak") } });

            var store = new BackupStore(GetServer(), new FileSystem(), options);

            List<string> backupFiles = store.BackupFiles;
            backupFiles.Should().HaveCount(5);
            backupFiles.Should().ContainMatch(@"*TestFull1.bak");
            backupFiles.Should().ContainMatch(@"*TestFull2.bak");
            backupFiles.Should().ContainMatch(@"*TestDiff1_1.bak");
            backupFiles.Should().ContainMatch(@"*TestLog1_1_1.bak");
            backupFiles.Should().ContainMatch(@"*TestLog1_1_2_Diff_1_2.bak");
        }

        [Fact]
        public void BackupHeaders_ShouldReturnFullBackupHeader()
        {
            IOptions<BackupStoreSettings> options = Options.Create(new BackupStoreSettings { BackupFileExtensions = { "BAK" }, BackupPaths = { Path.Combine(AppContext.BaseDirectory, "Bak") } });

            var store = new BackupStore(GetServer(), new FileSystem(), options);

            List<BackupHeader> backupHeaders = store.BackupHeaders;
            backupHeaders.Should().HaveCount(6);
            BackupHeader info = backupHeaders[0];
            info.BackupType.Should().Be(BackupType.Differential);
            info.DatabaseName.Should().Be("Test");
            info.BackupName.Should().Be("Test-Diff Database Backup");
            info.StartDate.Should().Be(new DateTime(2020, 7, 27, 9, 32, 5));
            info.FinishDate.Should().Be(new DateTime(2020, 7, 27, 9, 32, 5));
            info.FirstLSN.Should().Be(37000000102900001M);
            info.LastLSN.Should().Be(37000000103200001M);
            info.Position.Should().Be(1);
            info.SoftwareVersionMajor.Should().Be(15);
            info.Values.Count.Should().Be(56);
            info = backupHeaders[1];
            info.BackupType.Should().Be(BackupType.Full);
            info.DatabaseName.Should().Be("Test");
            info.BackupName.Should().Be("Test-Full Database Backup");
            info.StartDate.Should().Be(new DateTime(2020, 7, 25, 12, 45, 00));
            info.FinishDate.Should().Be(new DateTime(2020, 7, 25, 12, 45, 00));
            info.FirstLSN.Should().Be(37000000091400001M);
            info.LastLSN.Should().Be(37000000091700001M);
            info.Position.Should().Be(1);
            info.SoftwareVersionMajor.Should().Be(15);
            info.Values.Count.Should().Be(56);
            info = backupHeaders[2];
            info.BackupType.Should().Be(BackupType.Full);
            info.DatabaseName.Should().Be("Test");
            info.BackupName.Should().Be("Test-Full Database Backup");
            info.StartDate.Should().Be(new DateTime(2020, 8, 3, 16, 15, 53));
            info.FinishDate.Should().Be(new DateTime(2020, 8, 3, 16, 15, 53));
            info.FirstLSN.Should().Be(37000000136800001M);
            info.LastLSN.Should().Be(37000000137100001M);
            info.Position.Should().Be(1);
            info.SoftwareVersionMajor.Should().Be(15);
            info.Values.Count.Should().Be(56);
        }

        [Fact]
        public void GetLatestFull_ReturnFull2()
        {
            IOptions<BackupStoreSettings> options = Options.Create(new BackupStoreSettings { BackupFileExtensions = { "BAK" }, BackupPaths = { Path.Combine(AppContext.BaseDirectory, "Bak") } });

            var store = new BackupStore(GetServer(), new FileSystem(), options);

            BackupHeader info = store.GetLatestFull("DESKTOP-NVACFK6", "Test");
            info.Should().NotBeNull();
            info.BackupType.Should().Be(BackupType.Full);
            info.DatabaseName.Should().Be("Test");
            info.BackupName.Should().Be("Test-Full Database Backup");
            info.StartDate.Should().Be(new DateTime(2020, 8, 3, 16, 15, 53));
            info.FinishDate.Should().Be(new DateTime(2020, 8, 3, 16, 15, 53));
            info.FirstLSN.Should().Be(37000000136800001M);
            info.LastLSN.Should().Be(37000000137100001M);
            info.Position.Should().Be(1);
            info.SoftwareVersionMajor.Should().Be(15);
            info.Values.Count.Should().Be(56);
        }

        [Fact]
        public void GetLatestDiffWithFull_ReturnFull1Diff2()
        {
            IOptions<BackupStoreSettings> options = Options.Create(new BackupStoreSettings { BackupFileExtensions = { "BAK" }, BackupPaths = { Path.Combine(AppContext.BaseDirectory, "Bak") } });

            var store = new BackupStore(GetServer(), new FileSystem(), options);

            IEnumerable<BackupHeader> infos = store.GetLatestDiffWithFull("DESKTOP-NVACFK6", "Test");
            infos.Should().HaveCount(2);
            BackupHeader info = infos.First();
            info.Should().NotBeNull();
            info.BackupType.Should().Be(BackupType.Full);
            info.DatabaseName.Should().Be("Test");
            info.BackupName.Should().Be("Test-Full Database Backup");
            info.StartDate.Should().Be(new DateTime(2020, 7, 25, 12, 45, 00));
            info.FinishDate.Should().Be(new DateTime(2020, 7, 25, 12, 45, 00));
            info.FirstLSN.Should().Be(37000000091400001M);
            info.LastLSN.Should().Be(37000000091700001M);
            info.Position.Should().Be(1);
            info.SoftwareVersionMajor.Should().Be(15);
            info.Values.Count.Should().Be(56);
            info = infos.Last();
            info.Should().NotBeNull();
            info.BackupType.Should().Be(BackupType.Differential);
            info.DatabaseName.Should().Be("Test");
            info.BackupName.Should().Be("Test-Diff Database Backup");
            info.StartDate.Should().Be(new DateTime(2020, 8, 3, 14, 47, 32));
            info.FinishDate.Should().Be(new DateTime(2020, 8, 3, 14, 47, 32));
            info.FirstLSN.Should().Be(37000000133600001M);
            info.LastLSN.Should().Be(37000000133900001M);
            info.Position.Should().Be(2);
            info.SoftwareVersionMajor.Should().Be(15);
            info.Values.Count.Should().Be(56);
        }

        [Fact]
        public void BackupMediaHeaders_ShouldReturnFullMediaHeader()
        {
            IOptions<BackupStoreSettings> options = Options.Create(new BackupStoreSettings { BackupFileExtensions = { "BAK" }, BackupPaths = { Path.Combine(AppContext.BaseDirectory, "Bak") } });

            var store = new BackupStore(GetServer(), new FileSystem(), options);

            List<BackupMediaHeader> backupMediaHeaders = store.BackupMediaHeaders;
            backupMediaHeaders.Should().HaveCount(5);
            BackupMediaHeader media = backupMediaHeaders[0];
            media.MediaSetId.Should().Be(new Guid("68727d35-8696-462f-a312-25d596dd0705"));
            media.MediaDate.Should().Be(new DateTime(2020, 7, 27, 9, 32, 5));
            media = backupMediaHeaders[1];
            media.MediaSetId.Should().Be(new Guid("02375638-8846-470a-aa7f-5e803c3e102e"));
            media.MediaDate.Should().Be(new DateTime(2020, 7, 25, 12, 45, 00));
            media = backupMediaHeaders[2];
            media.MediaSetId.Should().Be(new Guid("7876e286-e78e-48b7-8ccd-3c102c2ca74d"));
            media.MediaDate.Should().Be(new DateTime(2020, 8, 3, 16, 15, 53));
            media = backupMediaHeaders[3];
            media.MediaSetId.Should().Be(new Guid("c844e72e-6d3c-4402-ba6a-bc618c53a233"));
            media.MediaDate.Should().Be(new DateTime(2020, 8, 3, 12, 19, 56));
            media = backupMediaHeaders[4];
            media.MediaSetId.Should().Be(new Guid("22288477-48c3-4ce2-a553-ce6390d82968"));
            media.MediaDate.Should().Be(new DateTime(2020, 8, 3, 14, 30, 58));
        }

        [Fact]
        public void GetFileNames_MockAllDirectories_ShouldReturnFiveFiles()
        {
            IOptions<BackupStoreSettings> options = Options.Create(new BackupStoreSettings { BackupFileExtensions = { "BAK" }, BackupPaths = { @"C:\", @"C:\Backup", @"C:\Backup\FULLBAK", @"C:\Backup\diffbak", @"C:\Backup\LogBak" } });

            var store = new BackupStore(GetServer(), GetFileSystem(), options);

            List<string> backupFiles = store.BackupFiles;
            backupFiles.Should().HaveCount(5);
            backupFiles.Should().Contain(@"c:\backfile1.bak");
            backupFiles.Should().Contain(@"c:\backup\backfile1.bak");
            backupFiles.Should().Contain(@"c:\backup\fullbak\backfile1.bak");
            backupFiles.Should().Contain(@"c:\backup\diffbak\backfile1.bak");
            backupFiles.Should().Contain(@"c:\backup\logbak\backfile1.bak");
        }

        [Fact]
        public void GetFileNames_MockFsEmpty_ShouldReturnEmptyList()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            IOptions<BackupStoreSettings> options = Options.Create(new BackupStoreSettings { BackupFileExtensions = { "BAK" }, BackupPaths = { @"C:\" } });

            var store = new BackupStore(GetServer(), fileSystem, options);

            List<string> backupFiles = store.BackupFiles;
            backupFiles.Should().BeEmpty();
        }

        private static MockFileSystem GetFileSystem()
                                    => new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\textfile1.txt", new MockFileData("Not a backup") },
                { @"c:\backfile1.bak", new MockFileData("A backup") },
                { @"c:\backup\textfile1.txt", new MockFileData("Not a backup") },
                { @"c:\backup\backfile1.bak", new MockFileData("A backup") },
                { @"c:\backup\fullbak\textfile1.txt", new MockFileData("Not a backup") },
                { @"c:\backup\fullbak\backfile1.bak", new MockFileData("A backup") },
                { @"c:\backup\diffbak\textfile1.txt", new MockFileData("Not a backup") },
                { @"c:\backup\diffbak\backfile1.bak", new MockFileData("A backup") },
                { @"c:\backup\logbak\textfile1.txt", new MockFileData("Not a backup") },
                { @"c:\backup\logbak\backfile1.bak", new MockFileData("A backup") },
            });

        private static Server GetServer() => new Server(TestSettings.SqlServerName);
    }
}