using System;
using System.Collections.Generic;
using System.Text;

namespace ConducThor_Client.Machine
{
    class LinuxFilesystemManager : IFilesystemManager
    {
        public byte[] GetFileFromFilesystem(string pFile)
        {
            return System.IO.File.ReadAllBytes(pFile);
        }
    }
}
