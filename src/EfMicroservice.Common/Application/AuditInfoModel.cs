using System;
using System.Collections.Generic;
using System.Text;

namespace EfMicroservice.Common.Application
{
    public class AuditInfoModel
    {
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}
