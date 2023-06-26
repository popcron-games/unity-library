#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Object = UnityEngine.Object;

namespace Popcron
{
    public class PlayabilityCheck : IEvent
    {
        private const int skipFrames = 1;
        private const bool needFileInfo = true;

        private readonly List<Issue> issues = new List<Issue>();

        public IReadOnlyList<Issue> Issues => issues;

        public void CantBecause(Exception exception, Object context)
        {
            issues.Add(new Issue(context, exception.ToString(), true, new StackTrace(exception, skipFrames, needFileInfo)));
        }

        public void CantBecause(ReadOnlySpan<char> message)
        {
            issues.Add(new Issue(null, message.ToString(), false, new StackTrace(skipFrames, needFileInfo)));
        }

        public void CantBecause(ReadOnlySpan<char> message, Object context)
        {
            issues.Add(new Issue(context, message.ToString(), true, new StackTrace(skipFrames, needFileInfo)));
        }

        public void CantIfNull<T>(T value, string message)
        {
            if (value == null)
            {
                issues.Add(new Issue(null, message, false, new StackTrace(skipFrames, needFileInfo)));
            }
            else if (value is Object unityObject && unityObject == null)
            {
                issues.Add(new Issue(null, message, false, new StackTrace(skipFrames, needFileInfo)));
            }
        }

        public void CantIfNull<T>(T value, ReadOnlySpan<char> message)
        {
            CantIfNull(value, message.ToString());
        }

        public void CantIfNull<T>(T value, string message, Object context)
        {
            if (value == null)
            {
                issues.Add(new Issue(context, message, true, new StackTrace(skipFrames, needFileInfo)));
            }
            else if (value is Object unityObject && unityObject == null)
            {
                issues.Add(new Issue(context, message, true, new StackTrace(skipFrames, needFileInfo)));
            }
        }

        public void CantIfNull<T>(T value, ReadOnlySpan<char> message, Object context)
        {
            CantIfNull(value, message.ToString(), context);
        }

        public void CantIfFalse(bool value, ReadOnlySpan<char> message)
        {
            if (!value)
            {
                issues.Add(new Issue(null, message.ToString(), false, new StackTrace(skipFrames, needFileInfo)));
            }
        }

        public void CantIfFalse(bool value, ReadOnlySpan<char> message, Object context)
        {
            if (!value)
            {
                issues.Add(new Issue(context, message.ToString(), true, new StackTrace(skipFrames, needFileInfo)));
            }
        }

        public static IEnumerable<Issue> GetIssues(params PlayabilityCheck[] checks)
        {
            HashSet<Issue> uniqueIssues = new HashSet<Issue>();
            foreach (PlayabilityCheck check in checks)
            {
                foreach (Issue issue in check.Issues)
                {
                    uniqueIssues.Add(issue);
                }
            }

            return uniqueIssues;
        }

        public readonly struct Issue : IEquatable<Issue>
        {
            public readonly Object? context;
            public readonly string message;
            public readonly bool contextGiven;
            public readonly StackTrace stackTrace;

            public Issue(Object? context, string message, bool contextGiven, StackTrace stackTrace)
            {
                this.context = context;
                this.message = message;
                this.contextGiven = contextGiven;
                this.stackTrace = stackTrace;
            }

            public override bool Equals(object? obj) => obj is Issue issue && Equals(issue);

            public bool Equals(Issue other)
            {
                return EqualityComparer<Object?>.Default.Equals(context, other.context) &&
                       message == other.message &&
                       contextGiven == other.contextGiven &&
                       EqualityComparer<StackTrace>.Default.Equals(stackTrace, other.stackTrace);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 0;
                    hash = (hash * 397) ^ EqualityComparer<Object?>.Default.GetHashCode(context);
                    hash = (hash * 397) ^ message.GetHashCode();
                    hash = (hash * 397) ^ contextGiven.GetHashCode();
                    hash = (hash * 397) ^ EqualityComparer<StackTrace>.Default.GetHashCode(stackTrace);
                    return hash;
                }
            }

            public static bool operator ==(Issue left, Issue right) => left.Equals(right);
            public static bool operator !=(Issue left, Issue right) => !(left == right);
        }
    }
}