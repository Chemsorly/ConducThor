using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConducThor_Client.Machine
{
    interface IFilesystemManager
    {
        byte[] GetFileFromFilesystem(String pFile);

        //https://stackoverflow.com/questions/38168391/cross-platform-file-name-handling-in-net-core
        //Console.WriteLine("..{0}Data{0}uploads{0}{{filename}}", Path.DirectorySeparatorChar);
    }
}
