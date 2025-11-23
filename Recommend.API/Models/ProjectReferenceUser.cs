using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.Models
{
    public class ProjectReferenceUser
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string UserAvatar { get; set; }
    }
}