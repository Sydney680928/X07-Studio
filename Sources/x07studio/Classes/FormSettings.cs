using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x07studio.Classes
{
    internal class FormSettings
    {
        // En mode non MDI, il y a une entrée avec son nom (ex FormMain)
        // En mode MDI, il y a une entée avec son nom précédé d'un marqueur (ex MDI:FormMain)
        // Le mode est indiqué par la propriété IsMDIChild

        public string Name { get; set; } = "";

        public int Left { get; set; }

        public int Top { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public FormWindowState WindowState { get; set; }

        public FormSettings()
        {

        }

        public FormSettings(string name, int left, int top, int width, int height, FormWindowState windowState)
        {
            Name = name;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            WindowState = windowState;
        }

        public FormSettings(Form form)
        {
            Name = form.IsMdiChild ? $"MDI:{form.Name}" : form.Name;
            Left = form.Left < 0 ? 0 : form.Left;
            Top = form.Top < 0 ? 0 : form.Top;
            Width = form.Width < 100 ? 100 : form.Width;
            Height = form.Height < 100 ? 100 : form.Height;
            WindowState = form.WindowState;
        }
    }
}
