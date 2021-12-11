using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Butjok
{
    [Serializable]
    public class InvalidMemberException : Exception
    {
        public readonly MemberInfo memberInfo;
        public InvalidMemberException() { }
        public InvalidMemberException(string message) : base(message) { }
        public InvalidMemberException(string message, Exception inner) : base(message, inner) { }
        public InvalidMemberException(string message, MemberInfo memberInfo) : base(message) {
            this.memberInfo = memberInfo;
        }
        protected InvalidMemberException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class CommandNotFoundException : Exception
    {
        public CommandNotFoundException() { }
        public CommandNotFoundException(string message) : base(message) { }
        public CommandNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected CommandNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class NoObjectsException : Exception
    {
        public NoObjectsException() { }
        public NoObjectsException(string message) : base(message) { }
        public NoObjectsException(string message, Exception inner) : base(message, inner) { }
        protected NoObjectsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class MultipleObjectsException : Exception
    {
        public readonly IList<object> targets, values;
        public MultipleObjectsException() { }
        public MultipleObjectsException(string message) : base(message) { }
        public MultipleObjectsException(string message, Exception inner) : base(message, inner) { }
        public MultipleObjectsException(string message, IList<object> targets, IList<object> values) : base(message) {
            this.targets = targets;
            this.values = values;
        }
        protected MultipleObjectsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}