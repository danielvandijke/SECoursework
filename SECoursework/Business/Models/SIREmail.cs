using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SECoursework.Business.Models
{
    internal class SIREmail : Email
    {
        public string SortCode { get; set; }
        public string IncidentNature { get; set; }
    }
}
