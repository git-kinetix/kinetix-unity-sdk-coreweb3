using System;

namespace Kinetix.Internal
{
    public class KinetixAccount
    {
        /// <summary>
        /// Event called upon new connection or disconnection
        /// </summary>
        public event Action OnUpdatedAccount;

        /// <summary>
        /// Event called upon new account connected
        /// </summary>
        public event Action OnConnectedAccount;



        /// <summary>
        /// Connect Wallet with Wallet Address
        /// </summary>
        /// <param name="_WalletAddress">Wallet address of user</param>
        public void ConnectWallet(string _WalletAddress)
        {
            KinetixAccountBehaviour.ConnectWallet(_WalletAddress);
        }
        
        /// <summary>
        /// Disconnect Wallet with Wallet Address
        /// </summary>
        /// <param name="_WalletAddress">Wallet address of user</param>
        public void DisconnectWallet(string _WalletAddress)
        {
            KinetixAccountBehaviour.DisconnectWallet(_WalletAddress);
        }

        /// <summary>
        /// Disconnect all Wallets
        /// </summary>
        public void DisconnectAllWallets()
        {
            KinetixAccountBehaviour.DisconnectAllWallets();
        }

        
        
        #region Internal

        public KinetixAccount()
        {
            AccountManager.OnUpdatedAccount += UpdatedAccount;
            AccountManager.OnConnectedAccount += ConnectedAccount;
        }

        private void UpdatedAccount()
        {
            OnUpdatedAccount?.Invoke();
        }

        private void ConnectedAccount()
        {
            OnConnectedAccount?.Invoke();
        }

        #endregion
    }
}
