using Backend.Application.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Infrastructure.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        public string GenerateSecretKey()
        {
            var bytes = new byte[20];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }
            return Base32Encode(bytes);
        }

        public string GenerateQrCodeUri(string email, string secret_key)
        {
            var issuer = Uri.EscapeDataString("ft_transcendence");
            var account = Uri.EscapeDataString(email);
            return $"otpauth://totp/{issuer}:{account}?secret={secret_key}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";
        }

        public bool VerifyCode(string secret_key, string code)
        {
            if (string.IsNullOrEmpty(secret_key) || string.IsNullOrEmpty(code))
                return false;

            var cleaned_code = code.Trim();
            if (cleaned_code.Length != 6)
                return false;

            try
            {
                var key_bytes = Base32Decode(secret_key);
                var unix_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var counter = unix_time / 30;

                for (int i = -1; i <= 1; i++)
                {
                    var temp_code = GenerateTotp(key_bytes, counter + i);
                    if (temp_code == cleaned_code)
                        return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private string Base32Encode(byte[] data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder();
            var buffer = 0;
            var bits_left = 0;

            foreach (var b in data)
            {
                buffer = (buffer << 8) | b;
                bits_left += 8;
                while (bits_left >= 5)
                {
                    var index = (buffer >> (bits_left - 5)) & 0x1F;
                    result.Append(alphabet[index]);
                    bits_left -= 5;
                }
            }

            if (bits_left > 0)
            {
                var index = (buffer << (5 - bits_left)) & 0x1F;
                result.Append(alphabet[index]);
            }

            return result.ToString();
        }

        private byte[] Base32Decode(string input)
        {
            var cleaned = input.Replace("=", "").ToUpperInvariant().Replace(" ", "");
            if (string.IsNullOrEmpty(cleaned))
                return Array.Empty<byte>();

            var byte_list = new List<byte>();
            var buffer = 0;
            var bits_left = 0;

            foreach (var c in cleaned)
            {
                var val = c switch
                {
                    >= 'A' and <= 'Z' => c - 'A',
                    >= '2' and <= '7' => c - '2' + 26,
                    _ => throw new ArgumentException("Invalid Base32 character")
                };

                buffer = (buffer << 5) | val;
                bits_left += 5;
                if (bits_left >= 8)
                {
                    byte_list.Add((byte)(buffer >> (bits_left - 8)));
                    bits_left -= 8;
                    buffer &= (1 << bits_left) - 1;
                }
            }

            return byte_list.ToArray();
        }

        private string GenerateTotp(byte[] key, long counter)
        {
            var counter_bytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counter_bytes);
            }

            using (var hmac = new HMACSHA1(key))
            {
                var hash = hmac.ComputeHash(counter_bytes);
                var offset = hash[hash.Length - 1] & 0x0F;
                var binary_code = ((hash[offset] & 0x7F) << 24)
                                | ((hash[offset + 1] & 0xFF) << 16)
                                | ((hash[offset + 2] & 0xFF) << 8)
                                | (hash[offset + 3] & 0xFF);

                var otp = binary_code % 1_000_000;
                return otp.ToString("D6");
            }
        }
    }
}
