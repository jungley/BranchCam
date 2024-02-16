using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalSettings 
{
    //Not ideal. Will be changed later.
    public static GlobalSettingsData Settings => Resources.Load("Global Settings") as GlobalSettingsData;

}
