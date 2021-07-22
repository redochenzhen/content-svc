using System;
using System.Collections.Generic;
using System.Text;

namespace ContentSvc.Model.Minio
{
    public class Statement
    {
        public string Effect { get; set; }

        public List<string> Action { get; set; }

        public List<string> Resource { get; set; }
    }

}