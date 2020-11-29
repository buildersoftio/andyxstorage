using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Utilities.Extensions
{
    public static class Randoms
    {
        public static string Generate6Digits()
        {
            return new Random().Next(0, 1000000).ToString("D6");
        }
    }
}
