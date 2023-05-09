using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kinetix.Utils;

// FILE_WEB3

namespace Kinetix.Internal
{
    public class OperationContractsDownloader : OperationAsync<List<KinetixContract>>
    {
        private          List<KinetixContract> contracts;

        public override async Task<List<KinetixContract>> Execute()
        {
            if (ProgressStatus == EProgressStatus.NONE)
            {
                try
                {
                    Task<List<KinetixContract>> task = GetContracts();
                    ProgressStatus = EProgressStatus.PENDING;
                    Task           = task;
                    contracts        = await task;
                    return contracts;
                }
                catch (Exception e)
                {
                    ProgressStatus = EProgressStatus.NONE;
                    KinetixDebug.LogException(e);
                }
            }

            if (ProgressStatus == EProgressStatus.COMPLETED) 
                return contracts;
            
            contracts = await Task;
            return contracts;
        }

        private async Task<List<KinetixContract>> GetContracts()
        {
            string url = Path.Combine(KinetixConstants.c_BaseKinetixURL, KinetixConstants.c_KinetixCollectionEndpoint).Replace("\\", "/");

            TaskCompletionSource<List<KinetixContract>> tcs = new TaskCompletionSource<List<KinetixContract>>();
            string result = await WebRequestHandler.Instance.GetAsyncRaw(url, null);

            if (string.IsNullOrEmpty(result))
                tcs.SetException(new Exception("Failed to reach Kinetix API"));
            else
            {
                contracts = new List<KinetixContract>();

                KinetixContract[] results = JsonHelper.GetJsonArray<KinetixContract>(result);
                for (int i = 0; i < results.Length; i++)
                {
                    string contractAddress = results[i].contractAddress;
                    if (!contracts.Exists(cont => cont.contractAddress.Equals(contractAddress)))
                        contracts.Add(results[i]);
                }

                tcs.SetResult(contracts);
            }  
                
            return await tcs.Task;
        }
    }
}
