using System;
using Asap2;
using Asap2Test;

namespace Scratch
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var errorHandler = new ErrorHandler();
            var parser = new Parser("C:\\SN\\__todo\\sulink\\fahrregler\\_subuild_DERFAHRER\\DERFAHRER.a2l",
                errorHandler);
            var tree = parser.DoParse();
            
        }
    }
}