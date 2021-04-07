using System;
using System.Reflection;
using Butjok;
using UnityEngine;

[CLSCompliant(false)]
public class TestCommandLine : CommandLine {

    private int Answer { get; } = 42;
    [SerializeField] private bool fly;
    
    protected override void Awake() {
        base.Awake();

        for (var i = 0; i < 100; i++)
            commands.Add(name: $"fly{i}",
                fieldInfo: GetType().GetField("fly", BindingFlags.Instance | BindingFlags.NonPublic),
                targetObject: this);

        commands.Add(propertyInfo: GetType().GetProperty("Answer", BindingFlags.Instance | BindingFlags.NonPublic),
            targetObject: this);
    }
}