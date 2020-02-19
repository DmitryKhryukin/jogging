using System.Collections;
using System.Collections.Generic;

namespace JoggingTracker.Core.DTOs
{
    public class PagedResult<T> where T: class
    {
        public int? PageNumber { get; set; }
        
        public int? PageSize { get; set; }
        
        public int Total { get; set; }
        public List<T> Items { get; set; }
        
        public PagedResult()
        {
            Items = new List<T>();
        }
    }
}