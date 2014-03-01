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

namespace MusicManager
{
    public class MusicManager : IFileManager
    {
        private static List<MusicFile> registeredFiles;
        private static DirectoryInfo rootDir;
        private static MusicManager singleton = null;
        private static Int64 ID = 0;
        private static string[] extensions = { ".mp3", ".wav", ".mp4" };
        private static Int64 currentID = 0;
        private static System.Timers.Timer trackTimer = new System.Timers.Timer();

        public static string RootDirectory { get; set; }
        public static string CLAmpLocation { get; set; }

        #region GeneralFunction

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
                    registeredFiles.Add(new MusicFile(file as FileInfo));
                    try
                    {
                        p.StartInfo.FileName = CLAmpLocation;
                        p.StartInfo.Arguments = "/PLADD " + "\"" + (file as FileInfo).FullName + "\"";
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = false;
                        p.Start();
                        p.WaitForExit();
                    }
                    catch (Exception e)
                    {
                        StreamWriter sw =
                            new StreamWriter(@"log.txt");
                        sw.WriteLine(e.Message);
                    } //suppress
                }
            }

            foreach (var dir in directoryInfo.GetDirectories())
            {
                PopulateList(dir);
            }
        }

        public List<IFile> GetRegisteredFiles()
        {
            return registeredFiles.ToList<IFile>();
        }

        public System.IO.FileInfo Find(string path)
        {
            return registeredFiles.Find(ByFilename(path)).FileInfo;
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
            JumpToFile(((currentID + 1) % registeredFiles.Count).ToString());
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
                System.Diagnostics.Process.Start(CLAmpLocation, "/PLSET " + (registeredFiles.FindIndex(MusicManager.ById(id)) + 1));
                currentID = id;
                PlayFile();
                Console.WriteLine("jump to: {0}", registeredFiles.Find(MusicManager.ById(id)).Filename);
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

            System.Diagnostics.Process.Start(CLAmpLocation, "/PLAY /JUMP " + miliseconds);
        }
    }
}
