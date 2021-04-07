using NUnit.Framework;
using UnityEngine;
using Butjok;
using Assert = Butjok.Assert;

public class NewTestScript {
    // A Test behaves as an ordinary method
    [Test]
    public void NewTestScriptSimplePasses() {
        var source = "hello world \"string\" true #$%#$% false";
        var parsed = new TextParser{Text=source};
        var text = "";
        for (var i = 0; i < source.Length; i++) {
            var (found,token) = parsed.TokenAt(i);
            Assert.That(found, i.ToString);
            text +=
                $"{i}: {CommandLineLexer.DefaultVocabulary.GetDisplayName(token.Type)}: {source.Substring(token.Start, token.Stop - token.Start + 1)}\n";
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
        commands.Invoke("yolo", false, 123, true, false, new string('a', 10));
    }
}