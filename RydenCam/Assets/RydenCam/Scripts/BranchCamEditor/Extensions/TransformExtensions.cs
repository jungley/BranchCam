using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Extensions
{
    public static class StringExtensions
    {
        public static bool ConvertToBool(this string input)
        {
            if (string.Equals(input, "true", StringComparison.OrdinalIgnoreCase))
                return true;
            else if (string.Equals(input, "false", StringComparison.OrdinalIgnoreCase))
                return false;
            else
                throw new ArgumentException("Invalid string representation for boolean: " + input);
        }
    }
}
