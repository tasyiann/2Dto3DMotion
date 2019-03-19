using System;
using UnityEngine;


public class DatabaseLoaderEventArgs : EventArgs
{
    public string jsonString;
    public int numberOfFrames;

   // public DatabaseLoaderEventArgs(string j = )
}
public class DatabaseLoader : MonoBehaviour
{
    private string DbData
    {
        set
        {
           // asdl = value;
            //OnDbDataChanged(new EGa)
        }
    }
    public event EventHandler<EventArgs> OnDatabaseLoad;

    protected virtual void OnDatabaseLoadRaiseEvent(DatabaseLoaderEventArgs e)
    {
        if(OnDatabaseLoad != null)
            OnDatabaseLoad.Invoke(this, e);
    }

    public void LoadDatabase()
    {


        OnDatabaseLoadRaiseEvent(new DatabaseLoaderEventArgs());
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class Subscriber : MonoBehaviour
{
    DatabaseLoader dbLoader;

    private void Awake()
    {
        dbLoader.OnDatabaseLoad += DbLoader_OnDatabaseLoad;
    }

    private void OnDisable()
    {
        dbLoader.OnDatabaseLoad -= DbLoader_OnDatabaseLoad;
    }

    private void DbLoader_OnDatabaseLoad(object sender, EventArgs e)
    {
        var loader = (DatabaseLoader)sender;

        throw new NotImplementedException();
    }
}
