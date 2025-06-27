#region using

using System.Collections.Generic;
using System.Runtime.Serialization;

#endregion

namespace Review.Core.DataModel
{
    /// <summary>
    ///     Represents context or state if you want for current group of reviewers.
    /// </summary>
    [DataContract]
    public class ReviewContext
    {
        [DataMember(Order = 1)]
        public string Id { get; set; }

        [DataMember(Order = 2)]
        public string ETag { get; set; }

        [DataMember(Order = 3)]
        public List<Reviewer> Reviewers { get; set; } = new List<Reviewer>();
    }
}