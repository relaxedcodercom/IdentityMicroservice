using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using IdentityMicroservice.Services.Contracts;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace IdentityMicroservice.Services
{
    public class CryptoService : ICryptoService
    {
        private static readonly RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

        private const int PBKDF2IterCount = 10000;
        private const int PBKDF2SubkeyLength = 256 / 8;
        private const int SaltSize = 128 / 8;

        public string GetPasswordHash(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            return HashPassword(password);
        }

        public bool VerifyPasswordHash(string hashedPassword, string password)
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException(nameof(hashedPassword));
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            return VerifyHashedPassword(hashedPassword, password);
        }

        private string HashPassword(string password)
        {
            var salt = new byte[SaltSize];
            randomNumberGenerator.GetBytes(salt);
            var subkey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, PBKDF2IterCount, PBKDF2SubkeyLength);

            var outputBytes = new byte[13 + salt.Length + subkey.Length];

            outputBytes[0] = 0x01;

            WriteNetworkByteOrder(outputBytes, 1, (uint)KeyDerivationPrf.HMACSHA256);

            WriteNetworkByteOrder(outputBytes, 5, (uint)PBKDF2IterCount);

            WriteNetworkByteOrder(outputBytes, 9, (uint)SaltSize);

            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);

            Buffer.BlockCopy(subkey, 0, outputBytes, 13 + SaltSize, subkey.Length);

            return Convert.ToBase64String(outputBytes);
        }

        private bool VerifyHashedPassword(string hashedPassword, string password)
        {
            var decodedHashedText = Convert.FromBase64String(hashedPassword);

            if (decodedHashedText.Length == 0)
            {
                return false;
            }

            try
            {
                if (decodedHashedText[0] != 0x01)
                {
                    return false;
                }

                var prf = (KeyDerivationPrf)ReadNetworkByteOrder(decodedHashedText, 1);

                var iterCount = (int)ReadNetworkByteOrder(decodedHashedText, 5);

                var saltLength = (int)ReadNetworkByteOrder(decodedHashedText, 9);

                if (saltLength < 128 / 8)
                {
                    return false;
                }

                var salt = new byte[saltLength];
                Buffer.BlockCopy(decodedHashedText, 13, salt, 0, salt.Length);

                var subkeyLength = decodedHashedText.Length - 13 - salt.Length;
                if (subkeyLength < 128 / 8)
                {
                    return false;
                }

                var expectedSubkey = new byte[subkeyLength];
                Buffer.BlockCopy(decodedHashedText, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

                var actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, subkeyLength);
                return ByteArraysEqual(actualSubkey, expectedSubkey);
            }
            catch
            {
                return false;
            }
        }

        private uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return ((uint)buffer[offset + 0] << 24)
                | ((uint)buffer[offset + 1] << 16)
                | ((uint)buffer[offset + 2] << 8)
                | buffer[offset + 3];
        }

        private void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            var areSame = true;
            for (var i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }
    }
}
