using System;
using System.IO;
using ExcelBinder.Models;
using ExcelBinder.Services;
using Xunit;

namespace ExcelBinder.Tests
{
    public class SafeFileTests
    {
        private static string TempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ExcelBinderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        [Fact]
        public void LoadJsonOrDefault_MissingFile_ReturnsDefault()
        {
            var result = SafeFile.LoadJsonOrDefault<AppSettings>(Path.Combine(TempDir(), "none.json"));
            Assert.NotNull(result);
        }

        [Fact]
        public void LoadJsonOrDefault_CorruptedFile_ReturnsDefault_AndMovesToBak()
        {
            string path = Path.Combine(TempDir(), "settings.json");
            File.WriteAllText(path, "{ this is not json !!");

            var result = SafeFile.LoadJsonOrDefault<AppSettings>(path);

            Assert.NotNull(result);
            Assert.False(File.Exists(path));           // 손상 파일은 치워짐
            Assert.True(File.Exists(path + ".bak"));   // .bak으로 보존됨
        }

        [Fact]
        public void LoadJsonOrDefault_ValidFile_RoundTrips()
        {
            string path = Path.Combine(TempDir(), "settings.json");
            File.WriteAllText(path, "{\"LastFeatureId\":\"abc\"}");

            var result = SafeFile.LoadJsonOrDefault<AppSettings>(path);
            Assert.Equal("abc", result.LastFeatureId);
        }

        [Fact]
        public void AtomicWriteText_CreatesAndReplaces()
        {
            string path = Path.Combine(TempDir(), "out.json");

            SafeFile.AtomicWriteText(path, "v1");
            Assert.Equal("v1", File.ReadAllText(path));

            SafeFile.AtomicWriteText(path, "v2");
            Assert.Equal("v2", File.ReadAllText(path));
            Assert.False(File.Exists(path + ".tmp"));
        }

        [Fact]
        public void AtomicWrite_OnFailure_PreservesExistingFile()
        {
            string path = Path.Combine(TempDir(), "out.bytes");
            File.WriteAllText(path, "original");

            Assert.Throws<InvalidOperationException>(() =>
                SafeFile.AtomicWrite(path, tmp =>
                {
                    File.WriteAllText(tmp, "partial");
                    throw new InvalidOperationException("simulated failure");
                }));

            Assert.Equal("original", File.ReadAllText(path)); // 기존 산출물 보존
            Assert.False(File.Exists(path + ".tmp"));         // 임시 파일 정리
        }
    }
}
