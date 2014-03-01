using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<MusicFile> GetItems()
        {
            registeredFiles = musicManager.GetRegisteredFiles().ConvertAll(x => (MusicFile)x);

            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return registeredFiles;
        }

        public MusicFileTagInfo GetItem(string fileId)
        {
            Int64 id = -1;
            if (Int64.TryParse(fileId, out id))
            {
                if (registeredFiles == null)
                {
                    registeredFiles = musicManager.GetRegisteredFiles().ConvertAll(x => (MusicFile)x);
                }

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

        public void RequestEnqueue(string fileId)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            musicManager.Enqueue(fileId);
        }
    }
}
