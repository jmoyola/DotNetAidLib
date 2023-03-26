using System;
using System.IO;
using System.Security.Cryptography;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Core.Cryptography.Encrypt
{
    public enum AsymmetricEncriptAlgorithmType
    {
        RSA
    }

    public class AsymmetricEncriptAlgorithm
    {
        private readonly RSA _CriptoServiceProvider;

        public AsymmetricEncriptAlgorithm()
        {
            _CriptoServiceProvider = RSA.Create();
        }

        public RSAParameters Keys
        {
            get => _CriptoServiceProvider.ExportParameters(true);
            set => _CriptoServiceProvider.ImportParameters(value);
        }

        public byte[] Encrypt(byte[] input)
        {
            byte[] ret;

            try
            {
                if (input == null)
                    throw new ArgumentNullException("input can't be null.");

                ret = ((RSACryptoServiceProvider) _CriptoServiceProvider).Encrypt(input, false);

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error encryting array.\r\n" + ex, ex);
            }
            finally
            {
                if (_CriptoServiceProvider != null) _CriptoServiceProvider.Clear();
            }
        }

        public byte[] Decrypt(byte[] input)
        {
            byte[] ret;

            try
            {
                if (input == null)
                    throw new ArgumentNullException("input can't be null.");


                ret = ((RSACryptoServiceProvider) _CriptoServiceProvider).Decrypt(input, false);

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error decryting array.\r\n" + ex, ex);
            }
            finally
            {
                if (_CriptoServiceProvider != null) _CriptoServiceProvider.Clear();
            }
        }

        public void Encrypt(Stream inputStream, Stream outputStream)
        {
            try
            {
                outputStream.WriteAll(Encrypt(inputStream.ReadAll()));
            }
            catch (Exception ex)
            {
                throw new Exception("Error encryting stream.\r\n" + ex, ex);
            }
            finally
            {
                inputStream.Close();
                outputStream.Close();
            }
        }

        public void Decrypt(Stream inputStream, Stream outputStream)
        {
            try
            {
                outputStream.WriteAll(Decrypt(inputStream.ReadAll()));
            }
            catch (Exception ex)
            {
                throw new Exception("Error decryting stream.\r\n" + ex, ex);
            }
            finally
            {
                inputStream.Close();
                outputStream.Close();
            }
        }
    }
}