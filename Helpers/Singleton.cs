using UnityEngine;

public class Singleton<TInstance> : MonoBehaviour where TInstance : Singleton<TInstance>
{
    public static bool HasInstance()
    {
        return Instance;
    }

    public static TInstance Instance{ get; private set; }

    public virtual void Awake()
    {
        Debug.Assert( Instance == null, typeof( TInstance ) + " Instance is not null", this );

        Instance = this as TInstance;
    }
}
