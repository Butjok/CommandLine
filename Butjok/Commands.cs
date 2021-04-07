using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements.Experimental;
using Object = UnityEngine.Object;

namespace Butjok {

    [Serializable]
    public class Commands {

        // Even values indicate procedure.
        private enum CommandType {
            Invalid = -1, Method = 0, Lambda = 2, UnityEvent = 4, Field = 1, Property = 3, GetSet = 5
        }

        [Serializable]
        private class Command {
            public string name;
            public CommandType type = CommandType.Invalid;
            public string help;
            public string[] arguments;
            public string debugComment;
            public Object debugUnityObject;
            public UnityEvent<Arguments> unityEvent;
            public object TargetObject;
            public FieldInfo FieldInfo;
            public PropertyInfo PropertyInfo;
            public Func<object> Getter;
            public Action<object> Setter;
            public MethodInfo MethodInfo;
            public Action<Arguments> Lambda;
        }

        [SerializeField] private List<Command> list = new List<Command>();
        private SortedDictionary<string, Command> _cache;

        public IEnumerable<string> Names => _cache.Keys;

        private static readonly List<string> List = new List<string>();

        public Commands() {
            _cache = new SortedDictionary<string, Command>();
            foreach (var c in list) {
                ValidateCommand(c.name, c.type, c.arguments, c.unityEvent, c.TargetObject, c.FieldInfo, c.PropertyInfo,
                    c.Getter, c.MethodInfo, c.Lambda);
                Assert.That(!_cache.ContainsKey(c.name), c.name.ToString);
            }
            foreach (var c in list)
                _cache.Add(c.name, c);
        }

        public bool Exists(string name) {
            Assert.That(!string.IsNullOrWhiteSpace(name));
            Assert.That(_cache != null);
            return _cache.ContainsKey(name);
        }
        private void AssertExists(string name, out Command command) {
            Assert.That(Exists(name));
            command = _cache[name];
        }
        
        public bool IsVariable(string name) {
            AssertExists(name, out var command);
            return (int) command.type % 2 == 1;
        }
        
        public void Invoke(string name, params object[] values) {
            AssertExists(name, out var command);

            var arguments = new Arguments(values, command.arguments);
            switch (command.type) {
                case CommandType.Method:
                    command.MethodInfo.Invoke(command.TargetObject, new object[] {arguments});
                    break;
                case CommandType.Lambda:
                    command.Lambda.Invoke(arguments);
                    break;
                case CommandType.UnityEvent:
                    command.unityEvent.Invoke(arguments);
                    break;
                default:
                    throw new CheckException(name);
            }
        }
        
        public object GetValue(string name) {
            AssertExists(name, out var command);

            switch (command.type) {
                case CommandType.Field:
                    return command.FieldInfo.GetValue(command.TargetObject);
                case CommandType.Property:
                    return command.PropertyInfo.GetValue(command.TargetObject);
                case CommandType.GetSet:
                    return command.Getter();
                default:
                    throw new CheckException(name);
            }
        }
        
        public void SetValue(string name, object value) {
            AssertExists(name, out var command);

            switch (command.type) {
                case CommandType.Field:
                    command.FieldInfo.SetValue(command.TargetObject, value);
                    break;
                case CommandType.Property:
                    command.PropertyInfo.SetValue(command.TargetObject, value);
                    break;
                case CommandType.GetSet:
                    command.Getter();
                    break;
                default:
                    throw new CheckException(name);
            }
        }

        public void Add(string name = null, string help = null, string[] arguments = null, string debugComment = null,
            UnityEvent<Arguments> unityEvent = null, object targetObject = null, FieldInfo fieldInfo = null,
            PropertyInfo propertyInfo = null, Func<object> getter = null, Action<object> setter = null,
            MethodInfo methodInfo = null, Action<Arguments> lambda = null) {

            var type = CommandType.Invalid;
            if (unityEvent != null) {
                type = CommandType.UnityEvent;
            }
            if (fieldInfo != null) {
                Assert.That(type == CommandType.Invalid);
                type = CommandType.Field;
            }
            if (propertyInfo != null) {
                Assert.That(type == CommandType.Invalid);
                type = CommandType.Property;
            }
            if (getter != null) {
                Assert.That(type == CommandType.Invalid);
                type = CommandType.GetSet;
            }
            if (methodInfo != null) {
                Assert.That(type == CommandType.Invalid);
                type = CommandType.Method;
            }
            if (lambda != null) {
                Assert.That(type == CommandType.Invalid);
                type = CommandType.Lambda;
            }

            if (type == CommandType.Invalid && fieldInfo == null && propertyInfo == null && methodInfo == null)
                throw new CheckException(
                    "You are trying to add an invalid command, this usually happens when you pass null FieldInfo, PropertyInfo or MethodInfo.");

            ValidateCommand(name, type, arguments, unityEvent, targetObject, fieldInfo, propertyInfo, getter,
                methodInfo, lambda);

            switch (type) {
                case CommandType.Method:
                    if (string.IsNullOrWhiteSpace(name))
                        name = MakeName(methodInfo);
                    if (string.IsNullOrWhiteSpace(debugComment))
                        debugComment = $"Method '{methodInfo.Name}' in {targetObject ?? methodInfo.DeclaringType}";
                    break;
                case CommandType.Lambda:
                    if (string.IsNullOrWhiteSpace(debugComment))
                        debugComment = $"Lambda";
                    break;
                case CommandType.UnityEvent:
                    if (string.IsNullOrWhiteSpace(debugComment))
                        debugComment = $"UnityEvent";
                    break;
                case CommandType.Field:
                    if (string.IsNullOrWhiteSpace(name))
                        name = MakeName(fieldInfo);
                    if (string.IsNullOrWhiteSpace(debugComment))
                        debugComment = $"Field '{fieldInfo.Name}' in {targetObject ?? methodInfo.DeclaringType}";
                    break;
                case CommandType.Property:
                    if (string.IsNullOrWhiteSpace(name))
                        name = MakeName(propertyInfo);
                    if (string.IsNullOrWhiteSpace(debugComment))
                        debugComment =
                            $"{(propertyInfo.SetMethod == null ? "Read-only property" : "Property")} '{propertyInfo.Name}' in {targetObject ?? methodInfo.DeclaringType}";
                    break;
                case CommandType.GetSet:
                    if (string.IsNullOrWhiteSpace(debugComment))
                        debugComment = $"Getter/setter pair";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var command = new Command {
                name = name,
                type = type,
                help = string.IsNullOrWhiteSpace(help) ? "" : help,
                arguments = arguments,
                debugComment = string.IsNullOrWhiteSpace(debugComment) ? "" : debugComment,
                debugUnityObject = targetObject is Object unityObject ? unityObject : null,
                unityEvent = unityEvent,
                TargetObject = targetObject,
                FieldInfo = fieldInfo,
                PropertyInfo = propertyInfo,
                Getter = getter,
                Setter = setter,
                MethodInfo = methodInfo,
                Lambda = lambda
            };
            list.Add(command);
            _cache.Add(command.name, command);
        }

        private static void ValidateCommand(string name, CommandType type, string[] arguments,
            UnityEvent<Arguments> unityEvent, object targetObject, FieldInfo fieldInfo, PropertyInfo propertyInfo,
            Func<object> getter, MethodInfo methodInfo, Action<Arguments> lambda) {

            switch (type) {

                case CommandType.Invalid:
                    throw new CheckException(name);

                case CommandType.Method:
                    Assert.That(methodInfo != null);
                    Assert.That(methodInfo.IsStatic == (targetObject == null));
                    if (arguments != null && arguments.Length > 0)
                        ValidateArguments(arguments);
                    break;

                case CommandType.Lambda:
                    Assert.That(!string.IsNullOrWhiteSpace(name));
                    Assert.That(lambda != null);
                    if (arguments != null && arguments.Length > 0)
                        ValidateArguments(arguments);
                    break;

                case CommandType.UnityEvent:
                    Assert.That(!string.IsNullOrWhiteSpace(name));
                    Assert.That(unityEvent != null);
                    if (arguments != null && arguments.Length > 0)
                        ValidateArguments(arguments);
                    break;

                case CommandType.Field:
                    Assert.That(fieldInfo != null);
                    Assert.That(fieldInfo.IsStatic == (targetObject == null));
                    break;

                case CommandType.Property:
                    Assert.That(propertyInfo != null);
                    Assert.That(propertyInfo.GetMethod != null);
                    Assert.That(propertyInfo.GetMethod.IsStatic == (targetObject == null));
                    if (propertyInfo.SetMethod != null) {
                        Assert.That(propertyInfo.SetMethod.IsStatic == propertyInfo.GetMethod.IsStatic);
                        Assert.That(propertyInfo.SetMethod.IsStatic == (targetObject == null));
                    }
                    break;

                case CommandType.GetSet:
                    Assert.That(!string.IsNullOrWhiteSpace(name));
                    Assert.That(getter != null);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static void ValidateArguments(string[] arguments) {
            Assert.That(arguments != null);

            foreach (var argument in arguments) {
                Assert.That(!string.IsNullOrWhiteSpace(argument));
                Assert.That(arguments.Count(name => name == argument) == 1, argument.ToString);
            }
        }

        private static string MakeName(MemberInfo memberInfo) {
            Assert.That(memberInfo != null);

            List.Clear();
            for (var type = memberInfo.DeclaringType; type != null; type = type.DeclaringType)
                List.Add(type.Name);
            List.Reverse();
            return string.Join(".", List) + "." + memberInfo.Name;
        }

        public void Remove(string name) {
            Assert.That(!string.IsNullOrWhiteSpace(name));

            var index = list.FindIndex(c => c.name == name);
            Assert.That(index != -1, name.ToString);
            var command = list[index];
            list.RemoveAt(index);
            Assert.That(_cache.ContainsKey(name));
            _cache.Remove(name);
        }
    }

    public readonly struct Arguments {
        public readonly IReadOnlyList<object> Values;
        public readonly IReadOnlyList<string> Names;
        public Arguments(IReadOnlyList<object> values, IReadOnlyList<string> names) {
            Values = values;
            Names = names;
        }
    }

    public static class ArgumentsExtensions {
        public static int Count(this Arguments arguments) {
            Assert.That(arguments.Values != null);
            return arguments.Values.Count;
        }
        public static T Get<T>(this Arguments arguments, int index) {
            Assert.That(arguments.Values != null);
            Assert.That(index >= 0);
            Assert.That(index < arguments.Values.Count);
            var value = arguments.Values[index];
            try {
                return (T) value;
            }
            catch (InvalidCastException) {
                throw new CheckException((index, typeof(T), value.GetType()).ToString());
            }
        }
        public static T Get<T>(this Arguments arguments, string name) {
            Assert.That(arguments.Names != null);

            var index = -1;
            for (var i = 0; i < arguments.Names.Count; i++)
                if (arguments.Names[i] == name) {
                    index = i;
                    break;
                }
            Assert.That(index != -1);
            return Get<T>(arguments, index);
        }
    }
}