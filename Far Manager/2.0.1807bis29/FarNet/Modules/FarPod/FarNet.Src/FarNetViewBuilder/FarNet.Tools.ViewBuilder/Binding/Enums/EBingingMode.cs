using System;

namespace FarNet.Tools.ViewBuilder.Binding.Enums
{
    [Flags]
    public enum EBingingMode
    {
        Node = 0,

        OneTime = 1,
        OneWay = 2,

        // OneWayToSource        

        TwoWay = 4,
        TwoWayManual = 8,
        TwoWayOnClose = 16,

        AllTwoWay = TwoWay | TwoWayManual | TwoWayOnClose,
    }
}
