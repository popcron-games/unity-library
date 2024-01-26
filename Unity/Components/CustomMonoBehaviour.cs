#nullable enable
using Library;
using Library.Unity;
using System.Threading;

public abstract class CustomMonoBehaviour : UnityEngine.MonoBehaviour
{
    protected static VirtualMachine VM => Host.VirtualMachine;
    protected static UnityObjects Objects => Host.VirtualMachine.GetSystem<UnityObjects>();

    private CancellationTokenSource? disableCts = null;

    /// <summary>
    /// Cancellation token thats raised when <see cref="OnDisable"/> is called.
    /// </summary>
    public CancellationToken DisableCancellationToken
    {
        get
        {
            if (disableCts is null)
            {
                disableCts = new CancellationTokenSource();
            }

            return disableCts.Token;
        }
    }

    protected virtual void OnEnable()
    {
        Objects.Register(this);
    }

    protected virtual void OnDisable()
    {
        if (disableCts is not null)
        {
            disableCts.Cancel();
        }

        Objects.Unregister(this);
    }
}
