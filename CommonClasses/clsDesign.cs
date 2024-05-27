using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses
{
    public static class clsDesign
    {
        public static int GetControlXcenterPosition(int ContainerWidth, int ControlWidth)
        {
            return (ContainerWidth - ControlWidth) / 2;
        }

        public static int GetControlYcenterPosition(int ContainerHeight, int ControlHeight)
        {
            return (ContainerHeight - ControlHeight) / 2;
        }
    }
}
