using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalSettings 
{
    //Not ideal should be cached somewhere.
    //Prone to error if GlobalSettings is renamed
    //Could possibly create a new setting if one is not found or search for asset of type GlobalSettingsData.
    public static GlobalSettingsData Settings => Resources.Load("Global Settings") as GlobalSettingsData;

}
