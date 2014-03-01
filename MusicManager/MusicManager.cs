using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Timers;
using System.Threading;
using System.Collections;

namespace MusicManager
{
    public class pairIdVote
    {
        public int vote = 0;
        public Int64 id;

        public pairIdVote(Int64 id, int vote)
        {
            this.id = id;
            this.vote = vote;            
        }
    }

    public class MusicManager : IFileManager
    {
        private static List<MusicFile> registeredFiles;
        
        private static DirectoryInfo rootDir;
        private static MusicManager singleton = null;
        private static Int64 ID = 0;
        private static string[] extensions = { ".mp3", ".wav", ".mp4" };
        private static Int64 currentID = 0;
        private static System.Timers.Timer trackTimer = new System.Timers.Timer();
        private static List<pairIdVote> queue;

        public static string RootDirectory { get; set; }
        public static string CLAmpLocation { get; set; }

        #region GeneralFunction

        //comparer
        private class sortVoteDescendingHelper : Comparer<pairIdVote>
        {
            public override int Compare(pairIdVote a, pairIdVote b)
            {
                if (a.vote < b.vote)
                    return 1;

                if (a.vote > b.vote)
                    return -1;
                else
                    return 0;
            }
        }

        /**
         * Constructor
         */
        private MusicManager()
        {
            CLAmpLocation = "CLAmp";
            registeredFiles = new List<MusicFile>();

            if (IsDir(RootDirectory))
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();

                p.StartInfo.FileName = CLAmpLocation;
                p.StartInfo.Arguments = "/START" + " /PLCLEAR";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();

                rootDir = OpenDir(RootDirectory);

                PopulateList(rootDir);
                queue = new List<pairIdVote>();

                MusicManager.trackTimer.Elapsed += trackTimer_Elapsed;
            }
            else
            {
                throw new NullReferenceException("Set the RootDirectory before requesting an instance of the MusicManager object!");
            }
        }

        /**
         * Get instance
         */
        public static MusicManager GetInstance()
        {
            if (singleton == null)
            {
                singleton = new MusicManager();
                return singleton;
            }
            else return singleton;
        }
        
        /**
         * Populates the list of registered files and the winamp playlist from the configurated dir
         */
        private void PopulateList(DirectoryInfo directoryInfo)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            foreach (var file in directoryInfo.GetFiles())
            {
                if (extensions.Contains(file.Extension))
                {
                    MusicFileTagInfo m = new MusicFileTagInfo(file.FullName);
                    MusicFile f = new MusicFile(file); 
                    f.Artist = m.Artist;
                    f.Title = m.Title;

                    registeredFiles.Add(f);
                }
            }

            foreach (var dir in directoryInfo.GetDirectories())
            {
                PopulateList(dir);
            }
        }

        public List<MusicFile> GetRegisteredFiles()
        {
            return registeredFiles;
        }

        public List<MusicFile> GetQueuedFiles()
        {
            var  queuedFiles = new List<MusicFile>();

            foreach (var queuedItem in queue)
            {
                var musicFile = registeredFiles.Find(x => x.ID == queuedItem.id);
                queuedFiles.Add(musicFile);
            }

            return queuedFiles;
        }
            
        #endregion
        
        #region Helpers

        public static bool IsDir(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            return dir.Exists;
        }

        public static System.IO.DirectoryInfo OpenDir(string path)
        {
            if (IsDir(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                return dir;
            }

            return null;
        }

        public static Predicate<MusicFile> ByFilename(string fileName)
        {
            return delegate(MusicFile file)
            {
                return file.Filename == fileName;
            };
        }

        public static Predicate<MusicFile> ById(Int64 fileId)
        {
            return delegate(MusicFile file)
            {
                return file.ID == fileId;
            };
        }
        
        public static MusicFileTagInfo CreateTagObject(Int64 fileid)
        {
            MusicFileTagInfo m = new MusicFileTagInfo(registeredFiles.Find(MusicManager.ById(fileid)).Fullpath);
            return m;
        }

        public static Int64 GiveValidID()
        {
            return ID++;
        }

        /**
        * Timer elapsed callback
        */
        void trackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (queue.Count == 0)
            {
                JumpToFile(((currentID + 1) % registeredFiles.Count).ToString());
            }
            else
            {
                JumpToFile(((queue[0].id) % registeredFiles.Count).ToString());
                queue.RemoveAt(0);
            }
        }

        private static void SetupTimer()
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();

            p.StartInfo.FileName = CLAmpLocation;
            p.StartInfo.Arguments = "/POS";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            string[] tokens = p.StandardOutput.ReadLine().Split('/');
            ////mockup codruta
            //string[] tokens = new string[] { "0:00", "0:00" };
            int length = 0;

            if (tokens.Length == 2)
            {
                string[] numbers = tokens[0].Split(':');
                int secondsElapsed = int.Parse(numbers[0]) * 60 + int.Parse(numbers[1]);

                numbers = tokens[1].Split(':');
                length = int.Parse(numbers[0]) * 60 + int.Parse(numbers[1]) - secondsElapsed;
            }

            if (MusicManager.trackTimer.Enabled)
                MusicManager.trackTimer.Stop();
            try
            {
                MusicManager.trackTimer.Interval = length * 1000;
                MusicManager.trackTimer.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region PlaybackRelatedFunctionalities

        public void PlayFile()
        {
            System.Diagnostics.Process.Start(CLAmpLocation, "/PLAY");
            
            System.Threading.Thread.Sleep(50);

            SetupTimer();
            Console.WriteLine("play");
        }

        public void PausePlayback()
        {
            System.Diagnostics.Process.Start(CLAmpLocation, "/PAUSE");
            trackTimer.Stop();
            Console.WriteLine("pause");
        }

        public void StopPlayback()
        {
            System.Diagnostics.Process.Start(CLAmpLocation, "/STOP");
            Console.WriteLine("stop");
        }

        public void JumpToFile(string fileId)
        {
            Int64 id = -1;
            if (Int64.TryParse(fileId, out id))
            {
                MusicFile file = (registeredFiles[registeredFiles.FindIndex(MusicManager.ById(id))]);
                System.Diagnostics.Process.Start(CLAmpLocation, "/PLCLEAR").WaitForExit();
                System.Diagnostics.Process.Start(CLAmpLocation, "/PLADD " + "\"" + file.Fullpath).WaitForExit(); //+ "\"" + "/PLSET " + 1
                currentID = id;
                PlayFile();
                Console.WriteLine("jump to: {0}", registeredFiles.Find(MusicManager.ById(id)).Filename);
            }
        }

        public void Enqueue(string fileId)
        {
            Int64 id = -1;
            if (Int64.TryParse(fileId, out id))
            {
                if (queue.Exists(x => x.id == id))
                {
                    queue.Find(x => x.id == id).vote++;
                }
                else
                {
                    queue.Add(new pairIdVote(id, 0));
                }

                queue.Sort(new sortVoteDescendingHelper());

                if (queue.Count == 1)
                        JumpToFile(queue[0].id.ToString());
            }
        }

        public pairIdPos CurrentPos()
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();

            p.StartInfo.FileName = CLAmpLocation;
            p.StartInfo.Arguments = "/POS";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            string[] tokens = p.StandardOutput.ReadLine().Split('/');
            ////mockup codruta
            //string[] tokens = new string[] { "0:00", "0:00" };

            if (tokens.Length == 2)
            {
                string[] numbers = tokens[0].Split(':');
                int secondsElapsed = int.Parse(numbers[0]) * 60 + int.Parse(numbers[1]);

                numbers = tokens[1].Split(':');
                int from = int.Parse(numbers[0]) * 60 + int.Parse(numbers[1]);
                if (from > 0)
                {
                    return new pairIdPos(secondsElapsed * 100 / from, currentID);
                }
            }

            return new pairIdPos(-1, currentID);
        }

        public void UpdateItems()
        {
            ID = 0;
            registeredFiles.Clear();
            System.Diagnostics.Process.Start(CLAmpLocation, "/PLCLEAR");
            PopulateList(rootDir);
            System.Diagnostics.Process.Start(CLAmpLocation, "/PLSET " + 1);
        }

        #endregion

        public void JumpToPos(string percentage)
        {
            double duration = CreateTagObject(currentID).Duration;
            int miliseconds =(int)((double.Parse(percentage) / 100) * duration * 1000);

            System.Diagnostics.Process.Start(CLAmpLocation, "/JUMP " + miliseconds);
            SetupTimer();
        }

        public FileInfo Find(string path)
        {
            throw new NotImplementedException();
        }
    }
}
