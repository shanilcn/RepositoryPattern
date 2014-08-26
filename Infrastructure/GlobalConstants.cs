using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LAMSFinishingDA.Infrastructure
{
    public static class GlobalConstants
    {
        //Context Variables
        public const string CONTEXT_ReadCore = "ReadCore";
        public const string CONTEXT_EditCore = "EditCore";
        public const string CONTEXT_CompCore = "CompCore";

        public const string CONTEXT_Read_Context = "Read";
        public const string CONTEXT_Edit_Context = "Edit";
        public const string CONTEXT_CompService_Context = "Comp";


        //Environtment Variables
        public const string ENVIRONMENT_Development = "Dev";
        public const string ENVIRONMENT_Testing = "Test";
        public const string ENVIRONMENT_Production = "Prod";
    }
}
