using System;

namespace WindowRecommender
{
    internal struct WindowRecord : IEquatable<WindowRecord>
    {
        internal readonly IntPtr Handle;
        internal string Title;
        internal string ProcessName;

        internal WindowRecord(IntPtr windowHandle)
        {
            Handle = windowHandle;
            Title = "";
            ProcessName = "";
        }

        internal WindowRecord(IntPtr windowHandle, string windowTitle, string processName)
        {
            Handle = windowHandle;
            Title = windowTitle;
            ProcessName = processName;
        }

        public bool Equals(WindowRecord other)
        {
            return Handle.Equals(other.Handle);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is WindowRecord other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        public static bool operator ==(WindowRecord left, WindowRecord right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WindowRecord left, WindowRecord right)
        {
            return !left.Equals(right);
        }
    }
}
