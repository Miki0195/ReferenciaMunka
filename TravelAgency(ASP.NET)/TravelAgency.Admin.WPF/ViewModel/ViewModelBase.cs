using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ELTE.TravelAgency.Admin.ViewModel
{
    /// <summary>
    /// Nézetmodell ősosztály típusa.
    /// </summary>
    public abstract class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// Üzenet küldésének eseménye.
        /// </summary>
        public event EventHandler<MessageEventArgs>? MessageApplication;

        /// <summary>
        /// Nézetmodell ősosztály példányosítása.
        /// </summary>
        protected ViewModelBase() { }

        /// <summary>
        /// Üzenet küldésének eseménykiváltása.
        /// </summary>
        /// <param name="message">Az üzenet.</param>
        protected void OnMessageApplication(string message)
        {
            if (MessageApplication != null)
                MessageApplication(this, new MessageEventArgs(message));
        }
    }
}
