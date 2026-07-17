using System;
using System.IO;
using Newtonsoft.Json;

namespace ExcelBinder.Services
{
    /// <summary>
    /// 설정/산출물 파일의 손상을 막는 안전 파일 IO 헬퍼.
    /// 쓰기는 임시 파일 → 원자적 교체, 읽기는 손상 시 .bak 보존 후 기본값 반환.
    /// </summary>
    public static class SafeFile
    {
        /// <summary>
        /// JSON 파일을 로드합니다. 파일이 없으면 기본값, 손상됐으면 .bak으로 옮기고 기본값을 반환합니다.
        /// </summary>
        public static T LoadJsonOrDefault<T>(string path) where T : class, new()
        {
            if (!File.Exists(path)) return new T();
            try
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) ?? new T();
            }
            catch (Exception ex)
            {
                LogService.Instance.Warning($"Failed to load {path}: {ex.Message}. Corrupted file moved to .bak, using defaults.");
                TryBackupCorrupted(path);
                return new T();
            }
        }

        private static void TryBackupCorrupted(string path)
        {
            try
            {
                File.Move(path, path + ".bak", overwrite: true);
            }
            catch (Exception ex)
            {
                LogService.Instance.Warning($"Failed to back up corrupted file {path}: {ex.Message}");
            }
        }

        /// <summary>같은 디렉토리의 임시 파일에 문자열을 쓴 뒤 원자적으로 교체합니다.</summary>
        public static void AtomicWriteText(string path, string content)
        {
            AtomicWrite(path, tmp => File.WriteAllText(tmp, content));
        }

        /// <summary>
        /// 임시 파일 경로를 콜백에 넘겨 쓰게 한 뒤 대상 경로로 원자적으로 교체합니다.
        /// 콜백이 실패하면 기존 파일은 건드리지 않고 임시 파일만 정리합니다.
        /// </summary>
        public static void AtomicWrite(string path, Action<string> writeToTemp)
        {
            string tmp = path + ".tmp";
            try
            {
                writeToTemp(tmp);
                if (File.Exists(path)) File.Replace(tmp, path, destinationBackupFileName: null);
                else File.Move(tmp, path);
            }
            catch
            {
                try
                {
                    if (File.Exists(tmp)) File.Delete(tmp);
                }
                catch (Exception cleanupEx)
                {
                    LogService.Instance.Warning($"Failed to clean up temp file {tmp}: {cleanupEx.Message}");
                }
                throw;
            }
        }
    }
}
