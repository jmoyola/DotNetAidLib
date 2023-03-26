using System;
using System.IO;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoBlob
    {
        public DaoBlob(byte[] value)
        {
            this.Value = value;
        }

        public DaoBlob(FileInfo path)
        {
            FromFile(path);
        }

        public byte[] Value { get; private set; }

        public Stream ToStream()
        {
            if (Value == null)
                return null;
            return new MemoryStream(Value);
        }

        public void FromStream(Stream value)
        {
            this.Value = value.ReadAll();
        }

        public void ToFile(FileInfo path, bool overrideIfExists)
        {
            FileStream st = null;

            try
            {
                if (path.Exists && !overrideIfExists)
                    throw new DaoException("File '" + path + "' exists.");

                st = path.Create();
                st.WriteAll(Value);
            }
            catch (Exception ex)
            {
                throw new DaoException("Error writing to file.", ex);
            }
            finally
            {
                if (st != null)
                    st.Close();
            }
        }

        public void FromFile(FileInfo path)
        {
            FileStream st = null;

            try
            {
                st = path.Open(FileMode.Open);
                Value = st.ReadAll();
            }
            catch (Exception ex)
            {
                throw new DaoException("Error reading from file.", ex);
            }
            finally
            {
                if (st != null)
                    st.Close();
            }
        }

        public static implicit operator byte[](DaoBlob o)
        {
            return o.Value;
        }

        public static implicit operator DaoBlob(byte[] o)
        {
            return new DaoBlob(o);
        }
    }
}