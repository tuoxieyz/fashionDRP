using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model.Extension
{
    public interface IDCodeNameEntity : IDEntity
    {
        string Code { get; set; }
        string Name { get; set; }
    }
}
