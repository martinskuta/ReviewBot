using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Review.Core.DataModel
{
    /// <summary>
    ///     Represents context or state if you want for current group of reviewers.
    /// </summary>
    [DataContract]
    public class ReviewContext
    {
        [DataMember]
        public List<Reviewer> Reviewers { get; set; } = new List<Reviewer>();
    }
}