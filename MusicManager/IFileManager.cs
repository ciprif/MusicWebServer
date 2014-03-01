using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MusicManager
{
    public interface IFile
    {
        FileInfo FileInfo { get; set; }
    }

    public interface IFileManager
    {
        List<MusicFile> GetRegisteredFiles();
        FileInfo Find(String path);
    }
}
