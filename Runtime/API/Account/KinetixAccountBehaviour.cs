using System;

namespace Kinetix.Internal
{
    internal static class KinetixAccountBehaviour
    {

        public static void ConnectWallet(string _WalletAddress)
        {
            AccountManager.ConnectWallet(_WalletAddress);
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

