using System;

namespace Kinetix.Internal
{
    internal static class KinetixAccountBehaviour
    {

        public static void ConnectWallet(string _WalletAddress, Action _OnSuccess = null)
        {
            AccountManager.ConnectWallet(_WalletAddress, _OnSuccess);
        }
        
        public static void DisconnectWallet(string _WalletAddress)
        {
            AccountManager.DisconnectWallet(_WalletAddress);
        }

        public static void DisconnectAllWallets()
        {
            AccountManager.DisconnectAllWallets();
        }
    }
}

