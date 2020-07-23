using System;
using System.IO;
using System.Text;
using Asap2;

namespace Asap2Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var errorHandler = new ErrorHandler();
            var parser = new Parser("../../../testFile.a2l", errorHandler);
            var comment =
                new FileComment(Environment.NewLine + "A2l file for testing ASAP2 parser." + Environment.NewLine, true);
            var tree = parser.DoParse();
            if (tree != null)
                try
                {
                    if (errorHandler.warnings == 0)
                        Console.WriteLine("Parsed file with no warnings.");
                    else
                        Console.WriteLine("Parsed file with {0} warnings.", errorHandler.warnings);

                    errorHandler = new ErrorHandler();
                    tree.Validate(errorHandler);

                    if (errorHandler.warnings == 0)
                        Console.WriteLine("Validated parsed data with no warnings.");
                    else
                        Console.WriteLine("Validated parsed data with {0} warnings.", errorHandler.warnings);

                    Console.WriteLine("Press enter to serialise data.");
                    Console.ReadLine();

                    tree.elements.Insert(0, comment);
                    var ms = new MemoryStream();
                    var stream = new StreamWriter(ms, new UTF8Encoding(true));
                    parser.Serialise(tree, stream);
                    ms.Position = 0;
                    var sr = new StreamReader(ms);
                    var myStr = sr.ReadToEnd();
                    Console.WriteLine(myStr);
                }
                catch (ValidationErrorException e)
                {
                    Console.WriteLine("Validation of parsed data failed!");
                    Console.WriteLine(e.ToString());
                }
            else
                Console.WriteLine("Parsing failed!");

            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
        }
    }
}