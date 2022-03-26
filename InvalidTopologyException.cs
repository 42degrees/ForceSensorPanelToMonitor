using System;
using System.Runtime.Serialization;

namespace ForceSensorPanelToMonitor
{
    [Serializable]
    internal class InvalidTopologyException : Exception
    {
        public InvalidTopologyException()
        {
        }

        public InvalidTopologyException(string message) : base(message)
        {
        }

        public InvalidTopologyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidTopologyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}