using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Butjok {

    public enum CommandType { Method = 0, Lambda = 2, UnityEvent = 4, Field = 1, Property = 3, GetSet = 5 }
    public static class CommandTypeExtensions {
        public static bool IsProcedure(this CommandType type) => ((int) type & 1) == 0;
        public static bool IsVariable(this CommandType type) => ((int) type & 1) == 1;
    }

    [Serializable]
    public class Command {

        [SerializeField] private string name;
        [SerializeField] private CommandType type;
        [Tooltip("Help text which is shown in completion list.")]
        [SerializeField] private string help;
        [Tooltip("Arguments names for commands of procedure types: Method, Lambda, UnityEvent.")]
        [SerializeField] private string[] arguments;
        [Tooltip("Debug comment.")]
        [SerializeField] private string debugComment;
        [Tooltip("Debug info: if a command belongs to a Unity Object class, then it is displayed here.")]
        [SerializeField] private Object debugTargetUnityObject;
        [Tooltip("If command's type is UnityEvent then these are callbacks for the command.")]
        [SerializeField] private UnityEvent<Arguments> unityEvent;

        public object TargetObject { get; }
        public FieldInfo Field { get; }
        public PropertyInfo Property { get; }
        public Func<object> Getter { get; }
        public Action<object> Setter { get; }
        public MethodInfo Method { get; }
        public Action<Arguments> Lambda { get; }
        private Dictionary<string, int> _indexes;

        public string Name => name;
        public string Help => help;
        public CommandType Type => type;
        public Dictionary<string, int> Indexes {
            get {
                if (arguments == null || arguments.Length == 0) {
                    _indexes = null;
                    return null;
                }
                if (_indexes != null)
                    return _indexes;

                var indexes = new Dictionary<string, int>();
                for (var i = 0; i < arguments.Length; i++) {
                    var argument = arguments[i];
                    Check.That(argument != null);
                    Check.That(ArgumentRegex.IsMatch(argument), argument.ToString);
                    Check.That(!indexes.ContainsKey(argument), argument.ToString);
                    indexes.Add(argument, i);
                }
                _indexes = indexes;
                return _indexes;
            }
        }

        private static readonly List<string> StringList = new List<string>();

        private static readonly Regex NameRegex = new Regex(
            @"^ [a-zA-Z_][a-zA-Z_0-9]* (?: \. [a-zA-Z_][a-zA-Z_0-9]* )* $",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex ArgumentRegex = new Regex(
            @"^ [a-zA-Z_][a-zA-Z_0-9]* $",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public Command(CommandType type, string name = null, MethodInfo method = null, Action<Arguments> lambda = null,
            FieldInfo field = null, PropertyInfo property = null, Func<object> getter = null,
            Action<object> setter = null, object targetObject = null, string[] arguments = null,
            string help = null) {

            this.type = type;
            this.help = string.IsNullOrWhiteSpace(help) ? "" : help.Trim();
            this.arguments = arguments;
            
            // Precache arguments' indexes.
            var _ = Indexes;

            switch (type) {

                case CommandType.Method: {

                    Check.That(method != null);
                    Check.That(method.IsStatic == (targetObject == null),
                        () => $"{method.IsStatic}, {targetObject}");

                    InitializeName(name, method);

                    Method = method;
                    TargetObject = targetObject;
                    if (targetObject is Object unityObject)
                        debugTargetUnityObject = unityObject;

                    debugComment =
                        $"{nameof(CommandType.Method)} {method} in {targetObject ?? method.DeclaringType}";
                    break;
                }

                case CommandType.Lambda:
                    Check.That(lambda != null);
                    Lambda = lambda;
                    debugComment = nameof(CommandType.Lambda);
                    break;

                case CommandType.UnityEvent:
                    debugComment = nameof(CommandType.UnityEvent);
                    break;

                case CommandType.Field: {

                    Check.That(field != null);
                    Check.That(field.IsStatic == (targetObject == null),
                        () => $"{field.IsStatic}, {targetObject}");

                    InitializeName(name, field);

                    Field = field;
                    TargetObject = targetObject;
                    if (targetObject is Object unityObject)
                        debugTargetUnityObject = unityObject;

                    debugComment =
                        $"{nameof(CommandType.Field)} {field} in {targetObject ?? field.DeclaringType}";
                    break;
                }

                case CommandType.Property: {

                    Check.That(property != null);
                    var getMethod = property.GetMethod;
                    var setMethod = property.SetMethod;
                    Check.That(getMethod != null);
                    if (setMethod != null)
                        Check.That(getMethod.IsStatic == setMethod.IsStatic,
                            () => $"{getMethod.IsStatic}, {setMethod.IsStatic}");
                    Check.That(getMethod.IsStatic == (targetObject == null),
                        () => $"{getMethod.IsStatic}, {targetObject}");

                    InitializeName(name, property);

                    Property = property;
                    TargetObject = targetObject;
                    if (targetObject is Object unityObject)
                        debugTargetUnityObject = unityObject;

                    debugComment =
                        $"{nameof(CommandType.Property)} {property} in {targetObject ?? property.DeclaringType}";
                    break;
                }

                case CommandType.GetSet:
                    Check.That(getter != null);
                    Getter = getter;
                    Setter = setter;
                    debugComment = nameof(CommandType.GetSet);
                    break;

                default:
                    throw new CheckException(type.ToString());
            }
        }
        private void InitializeName(string name, MemberInfo memberInfo) {
            if (!string.IsNullOrWhiteSpace(name)) {
                var trimmed = name.Trim();
                Check.That(NameRegex.IsMatch(trimmed), name.ToString);
                Check.That(!TokenLiterals.Values.Contains(trimmed), trimmed.ToString);
                this.name = trimmed;
            }
            else {
                StringList.Clear();
                for (var type = memberInfo.DeclaringType; type != null; type = type.DeclaringType)
                    StringList.Add(type.Name);
                StringList.Reverse();
                this.name = string.Join(".", StringList) + "." + memberInfo.Name;
            }
        }

        public object Value {
            get {
                switch (type) {
                    case CommandType.Field:
                        return Field.GetValue(TargetObject);
                    case CommandType.Property:
                        return Property.GetValue(TargetObject);
                    case CommandType.GetSet:
                        return Getter();
                    default:
                        throw new CheckException(Name);
                }
            }
            set {
                switch (type) {
                    case CommandType.Field:
                        Field.SetValue(TargetObject, value);
                        break;
                    case CommandType.Property:
                        Check.That(Property.SetMethod != null, name.ToString);
                        Property.SetValue(TargetObject, value);
                        break;
                    case CommandType.GetSet:
                        Check.That(Setter != null, name.ToString);
                        Setter(value);
                        break;
                    default:
                        throw new CheckException(Name);
                }
            }
        }
        public void Invoke(object[] values) {
            var arguments = new Arguments(values);
            switch (type) {
                case CommandType.Method:
                    Method.Invoke(TargetObject, new object[] {arguments});
                    break;
                case CommandType.Lambda:
                    Lambda(arguments);
                    break;
                case CommandType.UnityEvent:
                    unityEvent?.Invoke(arguments);
                    break;
                default:
                    throw new CheckException(name);
            }
        }
    }

    public class Arguments {
        private object[] _values;
        public Arguments(object[] values) {
            _values = values;
        }
    }

    public partial class CommandLine {

        [SerializeField] private List<Command> commandList = new List<Command>();
        private  Dictionary<string, Command> _commands;

        private void InitializeCommands() {
            _commands = new Dictionary<string, Command>();
            foreach (var command in commandList) {
                Check.That(!_commands.ContainsKey(command.Name), command.Name.ToString);
                _commands.Add(command.Name,command);
            }
        }
        public void AddCommand(Command command) {
            Check.That(!_commands.ContainsKey(command.Name), command.Name.ToString);
            _commands.Add(command.Name, command);
            commandList.Add(command);
        }
    };
}