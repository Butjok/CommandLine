using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Butjok
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    { }

    public static class Commands
    {
        public const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        private static readonly Dictionary<string, MemberInfo> commands = new Dictionary<string, MemberInfo>();
        public static IEnumerable<string> Names => commands.Keys;

        public static bool IsVariable(string name) {
            if (!commands.TryGetValue(name, out var command))
                throw new CommandNotFoundException(name);
            return command is FieldInfo || command is PropertyInfo propertyInfo && propertyInfo.GetMethod != null;
        }

        public static object Invoke(string name, IList<object> arguments = null) {

            if (!commands.TryGetValue(name, out var memberInfo))
                throw new CommandNotFoundException(name);

            arguments ??= Array.Empty<object>();

            switch (memberInfo) {

                case MethodInfo methodInfo: {
                    var targets = methodInfo.IsStatic ? new object[] { null } : UnityEngine.Object.FindObjectsOfType(methodInfo.DeclaringType);
                    var parameterInfos = methodInfo.GetParameters();
                    var defaultValues = new List<object>();
                    for (var i = arguments.Count; i < parameterInfos.Length; i++) {
                        if (!parameterInfos[i].IsOptional)
                            throw new TargetParameterCountException($"Parameter {parameterInfos[i]} is not optional, please provide a value for it.");
                        defaultValues.Add(parameterInfos[i].DefaultValue);
                    }
                    object returnValue = null;
                    try {
                        foreach (var target in targets)
                            returnValue = methodInfo.Invoke(target, arguments.Concat(defaultValues).ToArray());
                    }
                    catch (TargetParameterCountException e) {
                        throw new TargetParameterCountException($"Incorrect arguments number for the method {methodInfo}: expected {methodInfo.GetParameters().Length}, got {arguments.Count}.", e);
                    }
                    return returnValue;
                }

                case FieldInfo fieldInfo: {
                    var type = fieldInfo.DeclaringType;
                    var targets = fieldInfo.IsStatic ? new object[] { null } : UnityEngine.Object.FindObjectsOfType(type);
                    switch (arguments.Count) {
                        case 0:
                            if (targets.Length == 0)
                                throw new NoObjectsException($"No objects of type {type} found.");
                            if (targets.Length > 1)
                                throw new MultipleObjectsException($"Multiple objects of type {type} are present.", targets, targets.Select(t => fieldInfo.GetValue(t)).ToArray());
                            return fieldInfo.GetValue(targets.Last());
                        case 1:
                            foreach (var target in targets)
                                fieldInfo.SetValue(target, arguments[0]);
                            return null;
                        default:
                            throw new Exception($"Command {name} is a field {fieldInfo} and requires either one or no arguments.");
                    }
                }

                case PropertyInfo propertyInfo: {
                    var type = propertyInfo.DeclaringType;
                    var targets = propertyInfo.GetMethod?.IsStatic ?? propertyInfo.SetMethod.IsStatic ? new object[] { null } : UnityEngine.Object.FindObjectsOfType(type);
                    switch (arguments.Count) {
                        case 0:
                            if (targets.Length == 0)
                                throw new NoObjectsException($"No objects of type {type} found.");
                            if (targets.Length > 1)
                                throw new MultipleObjectsException($"Multiple objects of type {type} are present.", targets, targets.Select(t => propertyInfo.GetValue(t)).ToArray());
                            return propertyInfo.GetValue(targets.Last());
                        case 1:
                            foreach (var target in targets)
                                propertyInfo.SetValue(target, arguments[0]);
                            return null;
                        default:
                            throw new Exception($"Command {name} is a property {propertyInfo} and requires either one or no arguments.");
                    }
                }

                default: throw new ArgumentOutOfRangeException(memberInfo.ToString());
            }
        }

        public static void Fetch(IEnumerable<Assembly> assemblies, bool includeNamespaceName = true) {

            void Validate(MemberInfo memberInfo) {
                if (typeof(UnityEngine.Object).IsAssignableFrom(memberInfo.DeclaringType)) 
                    return;
                if (memberInfo is MethodInfo { IsStatic: false } ||
                    memberInfo is FieldInfo { IsStatic: false } ||
                    memberInfo is PropertyInfo propertyInfo && !(propertyInfo.GetMethod ?? propertyInfo.SetMethod).IsStatic)
                    throw new InvalidMemberException($"[Command] attribute cannot be added to a non-static member of a class not derived from UnityEngine.Object: {memberInfo.DeclaringType.FullName}.{memberInfo.Name}", memberInfo);
            }
            void AddCommand(MemberInfo memberInfo) {
                Validate(memberInfo);
                var prefix = (includeNamespaceName ? memberInfo.DeclaringType.FullName : memberInfo.DeclaringType.Name) + '.';
                var name = prefix + memberInfo.Name;
                commands[name] = memberInfo;
            }

            commands.Clear();
            foreach (var assembly in assemblies)
            foreach (var type in assembly.GetTypes()) {
                foreach (var methodInfo in type.GetMethods(bindingFlags).Where(m => m.GetCustomAttribute<CommandAttribute>() != null))
                    AddCommand(methodInfo);
                foreach (var fieldInfo in type.GetFields(bindingFlags).Where(m => m.GetCustomAttribute<CommandAttribute>() != null))
                    AddCommand(fieldInfo);
                foreach (var propertyInfo in type.GetProperties(bindingFlags).Where(m => m.GetCustomAttribute<CommandAttribute>() != null))
                    AddCommand(propertyInfo);
            }
        }

        [Command]
        public static string Info(string name) {
            if (!commands.TryGetValue(name, out var command))
                throw new CommandNotFoundException(name);
            return $"{command.DeclaringType} :: {command}";
        }
    }
}