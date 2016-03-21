using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RealAndVirtualMachine.Machines
{

    //This attribute indicates that a class can be serialized.
    //This class cannot be inherited.
    [Serializable]
    //Simple program contract exception class
    public class ProgramContractException : Exception
    {
        public ProgramContractException()
        { }

        public ProgramContractException(string message)
            : base(message)
        { }

        public ProgramContractException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected ProgramContractException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

    }

}
