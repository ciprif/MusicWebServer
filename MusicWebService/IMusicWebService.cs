using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using MusicManager;
using System.ServiceModel.Web;
using System.Runtime.Serialization;

namespace MusicWebService
{
    [ServiceContract]
    interface IMusicWebService
    {        
        [WebGet(UriTemplate="items")]
        [OperationContract] 
        List<MusicFile> GetItems();
            
        [WebGet(UriTemplate = "GetItemsPaged?page={page}&pageSize={pageSize}")]
        [OperationContract]
        IEnumerable<MusicFile> GetItemsPaged(int page, int pageSize);

        [WebGet(UriTemplate = "GetQueuedItems")]
        [OperationContract]
        List<MusicFile> GetQueuedItems();

        [WebGet(UriTemplate="items/{fileid}")]
        [OperationContract]
        MusicFileTagInfo GetItem(string fileid);

        [WebInvoke(Method="POST", UriTemplate="items/Play")]
        [OperationContract(IsOneWay = true)]
        void RequestPlay();
        
        [WebInvoke(Method="POST", UriTemplate="items/Pause")]
        [OperationContract(IsOneWay = true)]
        void RequestPause();
        
        [WebInvoke(Method="POST", UriTemplate="items/Stop")]
        [OperationContract(IsOneWay = true)]
        void RequestStop();

        [WebInvoke(Method = "GET", UriTemplate = "items/Pos")]
        [OperationContract]
        pairIdPos RequestPos();
        
        [WebInvoke(Method="POST", UriTemplate="items/jump/{fileid}")]
        [OperationContract(IsOneWay = true)]
        void RequestJumpToFile(string fileid);

        [WebInvoke(Method = "POST", UriTemplate = "items/enqueue/{fileid}")]
        [OperationContract(IsOneWay = true)]
        void RequestEnqueue(string fileid);

        [WebInvoke(Method = "POST", UriTemplate = "items/jumpToPos/{percentage}")]
        [OperationContract(IsOneWay = true)]
        void RequestJumpToPos(string percentage);
    }
}
