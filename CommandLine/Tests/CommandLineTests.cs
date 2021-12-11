using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Butjok.CommandLine
{
    [TestFixture]
    public class Tests
    {
        [OneTimeSetUp]
        public static void Initialize() {
            Commands.Fetch(new[] { Assembly.GetExecutingAssembly() });
        }

        [Command]
        public static int Double(int input) => 2 * input;
        [Command]
        public static int Identity(int input) => input;
        [Command]
        public const float pi = Mathf.PI;

        [TestCase("Butjok.CommandLine.Tests.Double 1 + 2", ExpectedResult = 6)]
        [TestCase("Butjok.CommandLine.Tests.Double ${Butjok.CommandLine.Tests.Identity 42}", ExpectedResult = 84)]
        [TestCase("Butjok.CommandLine.Tests.pi", ExpectedResult = pi)]
        [TestCase("Butjok.CommandLine.Tests.Identity 1; Butjok.CommandLine.Tests.Identity 2", ExpectedResult = 2)]
        public static object TestExecute(string input) {
            return Interpreter.Execute(input);
        }

        [TestCase("$Butjok.CommandLine.Tests.pi", ExpectedResult = pi)]
        [TestCase("${Butjok.CommandLine.Tests.pi}", ExpectedResult = pi)]
        [TestCase("90 / 2", ExpectedResult = 45)]
        [TestCase("5 % 3", ExpectedResult = 2)]
        [TestCase("3 + 4 * 5", ExpectedResult = 23)]
        [TestCase("\"hello\\\"world\\r\\n\\f\"", ExpectedResult = "hello\"world\r\n\f")]
        public static object TestEvaluate(string input) {
            return Interpreter.Evaluate(input);
        }

        [TestCase("Butjok.CommandLine.Tests.pi", ExpectedResult = true)]
        [TestCase("Butjok.CommandLine.Tests.Double", ExpectedResult = false)]
        public static bool TestIsVariable(string name) {
            return Commands.IsVariable(name);
        }

        [Test]
        public static void TestTokenGeneration() {
            GenerateTokens.Run();
        }

        [TestCase("kitten", "sitting", ExpectedResult = 3)]
        [TestCase("hello", "", ExpectedResult = 5)]
        [TestCase("hello", "HELLO", false, ExpectedResult = 5)]
        [TestCase("hello", "HELLO", true, ExpectedResult = 0)]
        public static int TestLevenshtein(string a, string b, bool ignoreCase = false) {
            return Levenshtein.Distance(a, b, ignoreCase);
        }
    }
}