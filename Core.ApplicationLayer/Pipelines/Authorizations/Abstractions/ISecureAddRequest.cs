using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationLayer.Pipelines.Authorizations.Abstractions;

public interface ISecureAddRequest
{
    public string[] Roles { get; }
}
