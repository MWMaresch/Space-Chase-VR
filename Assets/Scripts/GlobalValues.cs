using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GlobalValues : NetworkBehaviour {

    [SyncVar]
    public int numberOfPlayers =  0;

    [Command]
    public void CmdIncreasePlayerCount()
    {
        numberOfPlayers++;
    }
}
