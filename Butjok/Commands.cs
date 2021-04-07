using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Butjok {

    public class Commands : MonoBehaviour {

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
            public UnityEvent<Func<object, object>> unityEvent;
            public object TargetObject;
            public FieldInfo FieldInfo;
            public PropertyInfo PropertyInfo;
            public Func<object> Getter;
            public Action<object> Setter;
            public MethodInfo MethodInfo;
            public Action<Func<object, object>> Lambda;
        }

        [SerializeField] private List<Command> list;
        private SortedDictionary<string, Command> _cache;
        private Dictionary<Command, Dictionary<string, int>> _arguments;

        public IEnumerable<string> Names => _cache.Keys;

        private static readonly List<string> List = new List<string>();

        private void Reset() {
            list = new List<Command>();
        }

        public void Initialize() {
            Check.That(_cache == null);
            Check.That(_arguments == null);

            _cache = new SortedDictionary<string, Command>();
            foreach (var c in list) {
                ValidateCommand(name, c.type, c.arguments, c.unityEvent, c.TargetObject, c.FieldInfo, c.PropertyInfo,
                    c.Getter, c.MethodInfo, c.Lambda);
                Check.That(!_cache.ContainsKey(c.name), c.name.ToString);
            }
            _arguments = new Dictionary<Command, Dictionary<string, int>>();
            foreach (var c in list)
                CacheCommand(c);
        }

        public (bool Found, Butjok.Command Command) Get(string name) {
            Check.That(!string.IsNullOrWhiteSpace(name));
            Check.That(_cache != null);
            Check.That(_arguments != null);

            var result = new Butjok.Command {Name = name};
            if (!_cache.TryGetValue(name, out var command))
                return (false, new Butjok.Command());

            switch (command.type) {
                case CommandType.Method:
                    result.Procedure = arguments
                        => command.MethodInfo.Invoke(command.TargetObject, new object[] {arguments});
                    break;
                case CommandType.Lambda:
                    result.Procedure = values => command.Lambda(MakeArgumentsGetter(command, values));
                    break;
                case CommandType.UnityEvent:
                    result.Procedure = values => command.unityEvent?.Invoke(MakeArgumentsGetter(command, values));
                    break;
                case CommandType.Field:
                    result.VariableGet = () => command.FieldInfo.GetValue(command.TargetObject);
                    result.VariableSet = value => command.FieldInfo.SetValue(command.TargetObject, value);
                    break;
                case CommandType.Property:
                    result.VariableGet = () => command.PropertyInfo.GetValue(command.TargetObject);
                    if (command.PropertyInfo.SetMethod != null)
                        result.VariableSet = value => command.PropertyInfo.SetValue(command.TargetObject, value);
                    break;
                case CommandType.GetSet:
                    result.VariableGet = command.Getter;
                    result.VariableSet = command.Setter;
                    break;
                default:
                    throw new CheckException(command.type.ToString());
            }
            return (true, result);
        }

        private Func<object, object> MakeArgumentsGetter(Command command, object[] values) {
            Check.That(command != null);
            Check.That(values != null);

            return value => {

                if (value == null)
                    return values.Length;

                if (value is int index) {
                    Check.That(index >= 0, () => $"{command.name}: invalid argument index: {index}.");
                    Check.That(index < values.Length, ()
                        => $"{command.name}: argument index {index} is outside valid range 0..{values.Length - 1} (inclusive).");
                    return values[index];
                }
                if (value is string name) {
                    var foundArguments = _arguments.TryGetValue(command, out var arguments);
                    Check.That(foundArguments, () => $"{command.name}: command does not have named arguments.");
                    Check.That(arguments != null, command.name.ToString);
                    var foundArgument = arguments.TryGetValue(name, out var index2);
                    Check.That(foundArgument, () => $"{command.name}: unknown argument '{name}'.");
                    Check.That(index2 < values.Length, () => $"{command.name}: missing value for argument '{name}'.");
                    return values[index2];
                }

                throw new CheckException(value.GetType().ToString());
            };
        }

        public static readonly MemberInfo Null = typeof(Commands).GetMember("Null")[0];

        public void Add(string name = null, string help = null, string[] arguments = null, string debugComment = null,
            UnityEvent<Func<object, object>> unityEvent = null, object targetObject = null, FieldInfo fieldInfo = null,
            PropertyInfo propertyInfo = null, Func<object> getter = null, Action<object> setter = null,
            MethodInfo methodInfo = null, Action<Func<object, object>> lambda = null) {

            var type = CommandType.Invalid;
            if (unityEvent != null) {
                type = CommandType.UnityEvent;
            }
            if (fieldInfo != null) {
                Check.That(type == CommandType.Invalid);
                type = CommandType.Field;
            }
            if (propertyInfo != null) {
                Check.That(type == CommandType.Invalid);
                type = CommandType.Property;
            }
            if (getter != null) {
                Check.That(type == CommandType.Invalid);
                type = CommandType.GetSet;
            }
            if (methodInfo != null) {
                Check.That(type == CommandType.Invalid);
                type = CommandType.Method;
            }
            if (lambda != null) {
                Check.That(type == CommandType.Invalid);
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
            CacheCommand(command);
        }

        private static void ValidateCommand(string name, CommandType type, string[] arguments,
            UnityEvent<Func<object, object>> unityEvent, object targetObject, FieldInfo fieldInfo,
            PropertyInfo propertyInfo, Func<object> getter, MethodInfo methodInfo,
            Action<Func<object, object>> lambda) {

            switch (type) {

                case CommandType.Invalid:
                    throw new CheckException(name);

                case CommandType.Method:
                    Check.That(methodInfo != null);
                    Check.That(methodInfo.IsStatic == (targetObject == null));
                    if (arguments != null && arguments.Length > 0)
                        ValidateArguments(arguments);
                    break;

                case CommandType.Lambda:
                    Check.That(!string.IsNullOrWhiteSpace(name));
                    Check.That(lambda != null);
                    if (arguments != null && arguments.Length > 0)
                        ValidateArguments(arguments);
                    break;

                case CommandType.UnityEvent:
                    Check.That(!string.IsNullOrWhiteSpace(name));
                    Check.That(unityEvent != null);
                    if (arguments != null && arguments.Length > 0)
                        ValidateArguments(arguments);
                    break;

                case CommandType.Field:
                    Check.That(fieldInfo != null);
                    Check.That(fieldInfo.IsStatic == (targetObject == null));
                    break;

                case CommandType.Property:
                    Check.That(propertyInfo != null);
                    Check.That(propertyInfo.GetMethod != null);
                    Check.That(propertyInfo.GetMethod.IsStatic == (targetObject == null));
                    if (propertyInfo.SetMethod != null) {
                        Check.That(propertyInfo.SetMethod.IsStatic == propertyInfo.GetMethod.IsStatic);
                        Check.That(propertyInfo.SetMethod.IsStatic == (targetObject == null));
                    }
                    break;

                case CommandType.GetSet:
                    Check.That(!string.IsNullOrWhiteSpace(name));
                    Check.That(getter != null);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static void ValidateArguments(string[] arguments) {
            Check.That(arguments != null);

            foreach (var argument in arguments) {
                Check.That(!string.IsNullOrWhiteSpace(argument));
                Check.That(arguments.Count(name => name == argument) == 1, argument.ToString);
            }
        }

        private static string MakeName(MemberInfo memberInfo) {
            Check.That(memberInfo != null);

            List.Clear();
            for (var type = memberInfo.DeclaringType; type != null; type = type.DeclaringType)
                List.Add(type.Name);
            List.Reverse();
            return string.Join(".", List) + "." + memberInfo.Name;
        }

        private void CacheCommand(Command command) {
            Check.That(command != null);
            Check.That(command.type != CommandType.Invalid);

            _cache.Add(command.name, command);

            if ((int) command.type % 2 == 0 && command.arguments != null && command.arguments.Length > 0) {
                _arguments.Add(command, command.arguments
                    .Select((name, index) => (Name: name, Index: index))
                    .ToDictionary(pair => pair.Name, pair => pair.Index));
            }
        }

        public void Remove(string name) {
            Check.That(!string.IsNullOrWhiteSpace(name));

            var index = list.FindIndex(c => c.name == name);
            Check.That(index != -1, name.ToString);
            var command = list[index];
            list.RemoveAt(index);
            Check.That(_cache.ContainsKey(name));
            _cache.Remove(name);
            if (_arguments.ContainsKey(command))
                _arguments.Remove(command);
        }
    }

    public struct Command {
        public string Name;
        public Action<object[]> Procedure;
        public Func<object> VariableGet;
        public Action<object> VariableSet;
    }

    public static class ArgumentsExtensions {
        public static int Count(this Func<object, object> arguments) {
            return (int) arguments(null);
        }
        public static T Get<T>(this Func<object, object> arguments, object index) {
            var value = arguments(index);
            try {
                return (T) value;
            }
            catch (InvalidCastException) {
                throw new CheckException($"Cannot cast argument '{index}' from {value.GetType()} to {typeof(T)}.");
            }
        }
    }
}