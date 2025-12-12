using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace material_design
{
    public static class Appp
    {
        public static Autorization CurrentUser { get; set; }

        public static void SetCurrentUser(Autorization user)
        {
            CurrentUser = user;
        }
    }
}
