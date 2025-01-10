using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x07studio.Classes
{
    internal class AsmListViewItem : ListViewItem
    {
        public ASM80.OutLine AsmLine { get; init; }

        public AsmListViewItem(ASM80.OutLine asmLine)
        {        
            AsmLine = asmLine;

            SubItems[0].Text = asmLine.Source;
            SubItems.Add(asmLine.Address.ToString("X4"));
            SubItems.Add(asmLine.Code);
            SubItems.Add(asmLine.Hexa);
        }
    }
}
