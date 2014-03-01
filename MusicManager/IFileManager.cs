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

        List<MusicFile> GetQueuedFiles();

        FileInfo Find(String path);
    }
}
