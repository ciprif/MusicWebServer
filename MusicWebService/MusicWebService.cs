﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using MusicManager;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace MusicWebService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class MusicWebService : IMusicWebService
    {
        private MusicManager.MusicManager musicManager;

        private MusicWebService()
        {
            musicManager = MusicManager.MusicManager.GetInstance();
        }

        private List<MusicFile> registeredFiles = null;

        private List<MusicFile> RegisteredFiles
        {
            get
            {
                if (registeredFiles == null)
                {
                    registeredFiles = musicManager.GetRegisteredFiles();
                }
                return registeredFiles;
            }
        }

        private List<MusicFile> queuedFiles = null;

        private List<MusicFile> QueuedFiles
        {
            get
            {
                if (queuedFiles == null)
                {
                    queuedFiles = musicManager.GetQueuedFiles();
                }
                return queuedFiles;
            }
        }

        public List<MusicFile> GetItems()
        {
            musicManager.UpdateItems();
            registeredFiles = RegisteredFiles;

            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return registeredFiles;
        }

        public List<MusicFile> GetQueuedItems()
        {
            //musicManager.UpdateItems();
            //registeredFiles = RegisteredFiles;

            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return musicManager.GetQueuedFiles();
        }

        public MusicFileTagInfo GetItem(string fileId)
        {
            Int64 id = -1;
            if (Int64.TryParse(fileId, out id))
            {
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                return MusicManager.MusicManager.CreateTagObject(id);
            }
            return null;
        }

        public void RequestPlay()
        {
            musicManager.PlayFile();
        }

        public void RequestPause()
        {
            musicManager.PausePlayback();
        }

        public void RequestStop()
        {
            musicManager.StopPlayback();
        }

        public void RequestJumpToFile(string fileId)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            musicManager.JumpToFile(fileId);
        }


        public pairIdPos RequestPos()
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return musicManager.CurrentPos();
        }

        public void RequestJumpToPos(string percentage)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            musicManager.JumpToPos(percentage);
        }

        public bool RequestEnqueue(string fileId)
        {
            var ipFilterManager = new IPFilterManager();
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            if (ipFilterManager.IsFilteredOut("RequestEnqueue",fileId))
            {
                return false;
            }

            musicManager.Enqueue(fileId);
            return true;
        }

        public IEnumerable<MusicFile> GetItemsPaged(int page, int pageSize)
        {
            if ((page - 1) * pageSize > RegisteredFiles.Count - 1)
            {
                throw new Exception("Request page exceeds the total item count.");
            }

            // compute currentPageSize, which might be smaller than pageSize if we are on the last page
            var currentPageSize = pageSize;

            if ((page - 1) * pageSize < RegisteredFiles.Count && RegisteredFiles.Count < (page) * pageSize)
            {
                currentPageSize = RegisteredFiles.Count - (page - 1) * pageSize;
            }

            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return RegisteredFiles.Skip((page - 1) * pageSize).Take(currentPageSize);
        }
    }
}
