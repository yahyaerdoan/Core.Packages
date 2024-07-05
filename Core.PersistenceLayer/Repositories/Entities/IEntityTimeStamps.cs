using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.PersistenceLayer.Repositories.Entities;

public interface IEntityTimeStamps
{
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
