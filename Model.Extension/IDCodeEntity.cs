using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model.Extension
{
    public interface IDCodeEntity : IDEntity
    {
        string Code { get; set; }
    }
}
