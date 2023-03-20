﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using DotNetAidLib.Core.IO.Streams;

namespace DotNetAidLib.Core.IO.Streams.Imp
{
    public class PosixRawStreamFactory:RawStreamFactory
    {
        private FileInfo file;

        public PosixRawStreamFactory(String path)
            :base(path){
            file = new FileInfo(path);
            if (!file.Exists)
                throw new IOException("Unable to access drive/partition '" + path + "' . Win32 Error Code " + Marshal.GetLastWin32Error());
        }

        public override FileStream Open(FileAccess fileAccess){
            return file.Open(FileMode.Open, fileAccess);
        }

        public override void Dispose(bool disposing){
            
        }
    }
}