using UnityEngine;
using MessageBus;

public class AsyncClient : MonoBehaviour
{

    private Asynchronus_NamedPipe_Client mClient;
    private long counter = 0;
	// Use this for initialization
	void Start ()
    {
        mClient = new Asynchronus_NamedPipe_Client("AshSample");
	}
	
	// Update is called once per frame
	void Update ()
    {
        mClient.Write_to_Server_Async(string.Format("test {0}", counter++));
	}
}
