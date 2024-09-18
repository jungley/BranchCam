using RydenCam.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionScipt : MonoBehaviour
{
    public void TestMethod_A()
    {
        BranchLog.Log("TESTMETHOD_A");
    }  

    public static void TestMethod_B()
    {
        BranchLog.Log("TESTMETHOD_B");
    }

    public void TestMethod_C(string option1_string, bool option2_boolean)
    {
        BranchLog.Log("TESTMETHOD_C \n" + option1_string + " | " + option2_boolean.ToString());
       
    }

    public void TestMethod_D(string option1_string, bool option2_boolean, int option3_int, float option4_float, double option5_double)
    {
        BranchLog.Log("TESTMETHOD_D \n" + option1_string + " | " + option2_boolean + "| " + option3_int + " | " + option4_float + " | " + option5_double);
    }
}
