using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model.Extension
{
    public interface IDNameEntity : IDEntity
    {
        string Name { get; set; }
    }
}
