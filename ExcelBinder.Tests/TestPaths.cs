using System;
using System.IO;

namespace ExcelBinder.Tests
{
    /// <summary>
    /// 테스트 실행 위치에서 저장소 루트를 찾아 ExternalTestData 경로를 제공합니다.
    /// </summary>
    public static class TestPaths
    {
        public static string RepoRoot { get; } = FindRepoRoot();

        public static string ExternalTestData => Path.Combine(RepoRoot, "ExternalTestData");
        public static string StaticData => Path.Combine(ExternalTestData, "StaticData");

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "ExcelBinder.slnx"))) return dir.FullName;
                dir = dir.Parent;
            }
            throw new InvalidOperationException("Repo root (ExcelBinder.slnx) not found above " + AppContext.BaseDirectory);
        }
    }
}
