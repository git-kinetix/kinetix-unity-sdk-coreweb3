using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kinetix.Utils;

// FILE_WEB3

namespace Kinetix.Internal
{
    public static class KinetixBackendAPI
    {
        private static List<KinetixContract>        contracts;
        private static OperationContractsDownloader contractDownloader;

        public static async Task<List<KinetixContract>> GetContracts()
        {
            if (contracts != null)
                return contracts;

            contractDownloader ??= new OperationContractsDownloader();
            contracts          =   await contractDownloader.Execute();
            return contracts;
        }

        public static bool IsContractUGC(string _ContractAddress)
        {
            if (contracts == null)
                return false;
            if (!contracts.Exists(contract => contract.contractAddress == _ContractAddress))
                return false;
            return contracts.First(contract => contract.contractAddress == _ContractAddress).type == "ugc";
        }
        
        public static string GetContractName(string _ContractAddress)
        {
            if (contracts == null)
                return string.Empty;
            if (contracts.Exists(contract => contract.contractAddress == _ContractAddress))
                return contracts.First(contract => contract.contractAddress == _ContractAddress).name;
            return string.Empty;
        }
        
        public static string GetContractDescription(string _ContractAddress)
        {
            if (contracts == null)
                return string.Empty;
            if (contracts.Exists(contract => contract.contractAddress == _ContractAddress))
                return contracts.First(contract => contract.contractAddress == _ContractAddress).description;
            return string.Empty;
        }
    }
}

