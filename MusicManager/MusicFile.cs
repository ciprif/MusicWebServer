﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;
using Mp3Lib;

namespace MusicManager
{

    [DataContract]
    public class pairIdPos
    {
        [DataMember]
        public int pos;

        [DataMember]
        public Int64 id;

        public pairIdPos(int pos, Int64 id)
        {
            this.pos = pos;
            this.id = id;
        }
    }

    [DataContract]
    public class MusicFileTagInfo
    {
        private Mp3File mp3File;

        [DataMember]
        public String Title = "";

        [DataMember]
        public String Artist = "";

        [DataMember]
        public String Album = "";

        [DataMember]
        public String Year = "";

        [DataMember]
        public String Comment = "";

        [DataMember]
        public String Genre = "";

        [DataMember]
        public double Duration = 0;

        public MusicFileTagInfo(string filePath)
        {
            try
            {
                mp3File = new Mp3File(filePath);
                Title = mp3File.TagHandler.Title;
                Album = mp3File.TagHandler.Album;
                Artist = mp3File.TagHandler.Artist;
                Year = mp3File.TagHandler.Year;
                Comment = mp3File.TagHandler.Comment;
                Genre = mp3File.TagHandler.Genre;
                Duration = mp3File.Audio.Duration;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    [DataContract]
    public class MusicFile : IFile
    {
        [DataMember]
        public Int64 ID { get; set; }
        [DataMember]
        public String Filename { get; set; }
        [DataMember]
        public String Fullpath { get; set; }
        [DataMember]
        public FileInfo FileInfo { get; set; }

        public MusicFile(String fullPath)
        {
            ID = MusicManager.GiveValidID();
            Fullpath = fullPath;
            FileInfo = new FileInfo(fullPath);
            Filename = FileInfo.Name;
        }

        public MusicFile(FileInfo fileInfo)
        {
            ID = MusicManager.GiveValidID();
            Fullpath = fileInfo.FullName;
            FileInfo = fileInfo;
            Filename = fileInfo.Name;
        }
    }
}
