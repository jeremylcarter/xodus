using System;

namespace Xodus
{
    public class LoadingChangedEventArgs : EventArgs
    {
        public LoadingChangedEventArgs(bool isLoading)
        {
            Loading = isLoading;
        }

        public bool Loading { get; set; }
    }
}