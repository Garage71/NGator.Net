using NGator.Net.Infrastructure;

namespace NGator.Net.Models
{
    /// <summary>
    /// Interface for error processing 
    /// </summary>
    public interface IErrorDescriptor
    {   
        //todo: implement Error reporting engine
        /// <summary>
        /// Obtains error description
        /// </summary>
        /// <returns>Error description</returns>
        ErrorDescription GetError();
    }
}
