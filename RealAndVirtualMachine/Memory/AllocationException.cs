using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RealAndVirtualMachine.Memory
{
    //This attribute indicates that a class can be serialized.
    //This class cannot be inherited.
    [Serializable]
    //Simple allocation exception class
    public class AllocationException : Exception
    {
         public AllocationException ()
    {}

    public AllocationException (string message) 
        : base(message)
    {}

    public AllocationException (string message, Exception innerException)
        : base (message, innerException)
    {}

    protected AllocationException(SerializationInfo info, StreamingContext context)
        : base (info, context)
    {}

    }
}
