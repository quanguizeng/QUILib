using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QUI;

namespace _360SafeDemo
{
    public class ComputerExamineUI : ContainerUI
    {
        public ComputerExamineUI(PaintManagerUI manager = null)
        {
            DialogBuilder builder = new DialogBuilder();
            ContainerUI pComputerExamine = (ContainerUI)builder.createFromFile("ComputerExamine.xml", null, manager);
            builder = null;
            if (pComputerExamine != null)
            {
                this.add(pComputerExamine);
            }
            else
            {
                this.removeAll();
                return;
            }
        }
    };

    public class C360SafeDialogBuilderCallbackEx : IDialogBuilderCallback
    {
        public virtual ControlUI createControl(string pstrClass, PaintManagerUI manager = null)
        {
            if (pstrClass == "ComputerExamine") return new ComputerExamineUI(manager);
            return null;
        }
    };

}
