using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoDevelop.Ide.Gui
{
    /// <summary>
    /// CS添加接口文件,分离PadCodon和DefaultWorkbench之间的依赖
    /// </summary>
    public interface IWorkbench
    {
        bool IsContentVisible(Codons.PadCodon codon);

        bool IsSticky(Codons.PadCodon codon);

        void SetSticky(Codons.PadCodon codon, bool value);

        void ActivatePad(Codons.PadCodon codon, bool giveFocus);
    }
}
