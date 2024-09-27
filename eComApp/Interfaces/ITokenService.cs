using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eComApp.Models;

namespace eComApp.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}