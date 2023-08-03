using System;

namespace Kinetix.Internal
{
    internal static class KinetixAccountBehaviour
    {

        public static void ConnectWallet(string _WalletAddress, Action _OnSuccess = null)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<AccountManager>().ConnectWallet(_WalletAddress, _OnSuccess);
        }
        
        public static void DisconnectWallet(string _WalletAddress)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<AccountManager>().DisconnectWallet(_WalletAddress);
        }

        public static void DisconnectAllWallets()
        {
            KinetixCoreBehaviour.ManagerLocator.Get<AccountManager>().DisconnectAllWallets();
        }
    }
}

