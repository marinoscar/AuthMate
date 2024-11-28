using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luval.AuthMate.Entities
{
    public class BaseEntity : BaseEntity
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,  new JsonSerializerOptions() { WriteIndented = true });
        }
    }
}
