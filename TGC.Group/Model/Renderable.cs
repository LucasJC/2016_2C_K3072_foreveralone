using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    /// <summary>
    ///     elemento renderizable
    /// </summary>
    interface Renderable
    {
        void update();
        void render();
        void dispose();
    }
}
