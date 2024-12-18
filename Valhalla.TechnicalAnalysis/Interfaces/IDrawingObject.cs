using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Valhalla.TechnicalAnalysis.Interfaces
{
    public interface IDrawingObject
    {
        string Name { get; set; }

        bool IsVisible { get; set; }

        bool IsDraggable { get; set; }

        void Refresh();
    }

    public interface ISingleCoordinateDrawingObject
    {
        DateTime X { get; set; }
        double Y { get; set; }
    }

    public interface IDoubleCoordinateDrawingObject
    {
        DateTime X1 { get; set; }
        double Y1 { get; set; }
        DateTime X2 { get; set; }
        double Y2 { get; set; }
    }
}
