using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace x07studio.Forms
{
    public partial class FormMakeFont : Form
    {
        private Label[,] _Pixels;
        private byte[] _ColumnsValue;
        private Color _ColorON = Color.FromArgb(100, 100, 100);
        private Color _ColorOFF = Color.White;

        public FormMakeFont()
        {
            InitializeComponent();

            _Pixels = new Label[8, 6]
            {
                {Pixel00Label, Pixel10Label, Pixel20Label, Pixel30Label, Pixel40Label, Pixel50Label},
                {Pixel01Label, Pixel11Label, Pixel21Label, Pixel31Label, Pixel41Label, Pixel51Label},
                {Pixel02Label, Pixel12Label, Pixel22Label, Pixel32Label, Pixel42Label, Pixel52Label},
                {Pixel03Label, Pixel13Label, Pixel23Label, Pixel33Label, Pixel43Label, Pixel53Label},
                {Pixel04Label, Pixel14Label, Pixel24Label, Pixel34Label, Pixel44Label, Pixel54Label},
                {Pixel05Label, Pixel15Label, Pixel25Label, Pixel35Label, Pixel45Label, Pixel55Label},
                {Pixel06Label, Pixel16Label, Pixel26Label, Pixel36Label, Pixel46Label, Pixel56Label},
                {Pixel07Label, Pixel17Label, Pixel27Label, Pixel37Label, Pixel47Label, Pixel57Label}
            };

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    _Pixels[y, x].Click += Pixels_Click;
                    _Pixels[y, x].BackColor = _ColorOFF;
                }
            }

            _ColumnsValue = [128, 64, 32, 16, 8, 4];

            MakeInstruction();
            ViewSymbol();
        }

        private void Pixels_Click(object? sender, EventArgs e)
        {
            if (sender is Label pixel)
            {
                if (pixel.BackColor == _ColorOFF)
                {
                    pixel.BackColor = _ColorON;
                }
                else
                {
                    pixel.BackColor = _ColorOFF;
                }
            }

            MakeInstruction();
            ViewSymbol();
        }

        private void MakeInstruction()
        {
            var sb = new StringBuilder("FONT$(x)=\"");

            for (int y = 0; y < 8; y++)
            {
                byte v = 0;

                for (int x = 0; x < 6; x++)
                {
                    if (_Pixels[y, x].BackColor == _ColorON)
                    {
                        v += _ColumnsValue[x];
                    }
                }

                if (y > 0) sb.Append(',');
                sb.Append(v);
            }

            sb.Append('\"');

            ResultFontLabel.Text = sb.ToString();
        }

        private void ViewSymbol()
        {
            var image = new Bitmap(48, 64);
            using var g = Graphics.FromImage(image);
            using var black = new SolidBrush(Color.Black);
            using var white = new SolidBrush(Color.White);   

            for (int y = 0; y < 8; y++)
            {
                int py = y * 8;

                for (int x = 0; x < 6; x++)
                {
                    int px = x * 8;
                    var rect = new Rectangle(px, py, 8, 8);

                    if (_Pixels[y, x].BackColor == _ColorON)
                    {
                        g.FillRectangle(black, rect);
                    }
                    else
                    {
                        g.FillRectangle(white, rect);
                    }
                }
            }

            ViewPictureBox.Image = image;
        }

        private void EraseAllMenu_Click(object sender, EventArgs e)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    _Pixels[y, x].BackColor = _ColorOFF; ;
                }
            }

            MakeInstruction();
            ViewSymbol();
        }

        private void BlackAllMenu_Click(object sender, EventArgs e)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    _Pixels[y, x].BackColor = _ColorON;
                }
            }

            MakeInstruction();
            ViewSymbol();
        }

        private void CopyMenu_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ResultFontLabel.Text);
        }

        private void PasteMenu_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                var f = Clipboard.GetText();

                // Format attendu = "FONT$(....)="x,x,x,x,x,x,x,x"

                if (f.StartsWith("FONT$("))
                {
                    var k = f.IndexOf(")=");

                    if (k > -1)
                    {
                        var s = f.Substring(k + 2);

                        if (s.Length > 2 && s.StartsWith("\"") && s.EndsWith("\""))
                        {
                            s = s.Substring(1, s.Length - 2);
                            var items = s.Split(',');

                            if (items.Length == 8)
                            {
                                byte[] b = [0, 0, 0, 0, 0, 0, 0, 0];

                                for (int i = 0; i < 8; i++)
                                {
                                    if (!byte.TryParse(items[i], out b[i]))
                                    {
                                        return;
                                    }
                                }

                                for (int y = 0; y < 8; y++)
                                {
                                    for (int x = 0; x < 6; x++)
                                    {
                                        if ((b[y] & _ColumnsValue[x]) > 0)
                                        {
                                            _Pixels[y, x].BackColor = _ColorON;
                                        }
                                        else
                                        {
                                            _Pixels[y, x].BackColor = _ColorOFF;
                                        }
                                    }
                                }

                                MakeInstruction();
                                ViewSymbol();
                            }
                        }
                    }
                }
            }
        }

        private void ReverseMenu_Click(object sender, EventArgs e)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    if (_Pixels[y, x].BackColor == _ColorOFF)
                    {
                        _Pixels[y, x].BackColor = _ColorON;
                    }
                    else
                    {
                        _Pixels[y, x].BackColor = _ColorOFF;
                    }
                }
            }

            MakeInstruction();
            ViewSymbol();
        }
    }
}
