using System;
using System.IO;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Database.DAO.Core
{
    public class DaoBlob
    {
        private byte[] value;

        public DaoBlob(byte[] value){
            this.value = value;
        }

        public DaoBlob(FileInfo path)
        {
            this.FromFile(path);
        }

        public byte[] Value{
            get{
                return value;
            }
        }

        public Stream ToStream() {
            if (this.value == null)
                return null;
            else
                return new MemoryStream(this.value);
        }

        public void FromStream(Stream value){
            this.value = value.ReadAll();
        }

        public void ToFile(FileInfo path, bool overrideIfExists)
        {
            FileStream st=null;

            try
            {
                if (path.Exists && !overrideIfExists)
                    throw new DaoException("File '" + path + "' exists.");

                st = path.Create();
                st.WriteAll(this.value);
            }
            catch (Exception ex) {
                throw new DaoException("Error writing to file.", ex);
            }
            finally {
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
                this.value=st.ReadAll();
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

        public static implicit operator byte[](DaoBlob o){
            return o.value;
        }

        public static implicit operator DaoBlob (byte[] o){
            return new DaoBlob(o);
        }
    }
}
