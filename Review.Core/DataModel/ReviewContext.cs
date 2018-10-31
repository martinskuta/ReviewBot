using System;
using System.Collections.Generic;

namespace Review.Core.DataModel
{
    /// <summary>
    ///     Represents context or state if you want for current group of reviewers.
    /// </summary>
    [Serializable]
    public class ReviewContext
    {
        public List<Reviewer> Reviewers { get; set; } = new List<Reviewer>();
    }
}