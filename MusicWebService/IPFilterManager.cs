using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebService
{
    /// <summary>
    /// Responsible for blocking multiple resource requests from the same IP in an interval of time.
    /// </summary>
    public class IPFilterManager
    {
        /// <summary>
        /// The time interval the resource will be blocked when requested from the same IP
        /// </summary>
        private readonly TimeSpan BlockInterval = TimeSpan.FromMinutes(1);

        private static IDictionary<string, DateTime> RequestedResource = new Dictionary<string, DateTime>(); 

        /// <summary>
        /// Gets the current IP for the request.
        /// </summary>
        /// <returns></returns>
        private string GetCurrentIP()
        {
            string ip = null;

            var props = OperationContext.Current.IncomingMessageProperties;
            var endpointProperty = props[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            if (endpointProperty != null)
            {
                ip = endpointProperty.Address;
            }

            return ip;
        }

        /// <summary>
        /// Determines whether [the specified resource name] [is filtered out].
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns></returns>
        public bool IsFilteredOut(string resourceName,  params string []args)
        {
            bool isResourceFree = true;

            // 1. Check if the resource was accessed in the "block interval" by the same IP
            string key = string.Format("{0}_{1}_", resourceName, GetCurrentIP()) + string.Join("_", args);
           
            if (RequestedResource.ContainsKey(key))
            {
                var timeFromLastRequest = DateTime.UtcNow - RequestedResource[key];
                if (timeFromLastRequest > BlockInterval)
                {
                    RequestedResource[key] = DateTime.UtcNow;
                }
                else
                {
                    isResourceFree = false;
                }
            }
            else
            {
                RequestedResource[key] = DateTime.UtcNow;
            }

            return !isResourceFree;
        }
    }
}
