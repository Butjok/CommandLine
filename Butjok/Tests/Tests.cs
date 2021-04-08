using NUnit.Framework;
using UnityEngine;
using Butjok;
using Assert = NUnit.Framework.Assert;

public class Tests {

    [Test]
    public void TestTextParser() {
        var source = "hello world \"string\" true #$%#$% false";
        var parser = new TextParser();
        parser.Parse(source);
        var text = "";
        for (var i = 0; i < source.Length; i++) {
            var (found, token) = parser.TokenAt(i);
            Assert.That(found, i.ToString);
            text +=
                $"{i}: {CommandLineLexer.DefaultVocabulary.GetDisplayName(token.Type)}: {token.FindText()}\n";
        }
        Debug.Log(text);
    }

    [Test]
    public void TestCommandsArgumentsDispatch() {
        var commands = new Commands();
        commands.Add(
            name: "yolo",
            arguments: new[] {"name", "age"},
            lambda: arguments => {
                Debug.Log($"name: {arguments.Get<string>("name")}, age: {arguments.Get<int>("age")}");
            });
        Assert.That(() => commands.Invoke("yolo", false, 123, true, false, new string('a', 10)), Throws.Exception);
    }
}