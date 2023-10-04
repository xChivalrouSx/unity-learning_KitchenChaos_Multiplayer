using System;

public interface IHasProgress
{

    public event EventHandler<OnProgressChangeEventArgs> OnProgressChanged;
    public class OnProgressChangeEventArgs : EventArgs
    {
        public float progressNormalized;
    }

}
