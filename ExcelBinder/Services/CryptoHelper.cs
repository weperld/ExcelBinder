using System;
using System.Security.Cryptography;
using System.Text;

namespace ExcelBinder.Services
{
    /// <summary>
    /// Windows DPAPI를 사용하여 문자열을 암호화/복호화하는 유틸리티입니다.
    /// 현재 사용자 범위(CurrentUser)로 보호되며, 동일 Windows 계정에서만 복호화할 수 있습니다.
    /// </summary>
    public static class CryptoHelper
    {
        /// <summary>
        /// 문자열을 DPAPI로 암호화하여 Base64 문자열로 반환합니다.
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// DPAPI로 암호화된 Base64 문자열을 복호화합니다.
        /// 복호화 실패 시(기존 평문 데이터 등) 원본 문자열을 그대로 반환합니다.
        /// </summary>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                // 복호화 실패 시 평문으로 간주 (기존 settings.json 하위 호환)
                return encryptedText;
            }
        }
    }
}
