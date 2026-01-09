using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace x07studio.Forms
{
    public partial class FormAsmLoader : Form
    {
        public FormAsmLoader()
        {
            InitializeComponent();
        }

        private async void GenerateButton_Click(object sender, EventArgs e)
        {
            GenerateButton.Enabled = false;
            
            await GenerateLoader(HexTextBox.Text);

            GenerateButton.Enabled = true;
        }

        private async Task<bool> GenerateLoader(string hex)
        {
            return await Task<bool>.Run(() =>
            {
                var lines = hex.Replace("\r", "").Trim().Split("\n");
                var code = new StringBuilder();
                int startAddress;
                int numLine = 100;

                if (lines.Length > 0)
                {
                    var error = false;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];

                        if (line.StartsWith(":") && line.Length >= 11)
                        {
                            var bb = GetValue(line, 1, 2);
                            var aaaa = GetValue(line, 3, 4);
                            var tt = GetValue(line, 7, 2);
                            var cc = GetValue(line, line.Length - 2, 2);

                            if (bb == null || aaaa == null || tt == null || cc == null)
                            {
                                error = true;
                                break;
                            }

                            if (line.Length == bb * 2 + 11)
                            {
                                if (tt == 0)
                                {
                                    // Ligne de données

                                    if (code.Length == 0)
                                    {
                                        // Ligne définisant l'adresse de départ

                                        startAddress = aaaa.Value;
                                        code.AppendLine($"10 CLS:A%=&H{startAddress:X4}");
                                        code.AppendLine($"15 PRINT \"@ EXEC: &H\";HEX$(A%)");
                                        code.AppendLine("20 READ A$:IF A$=\"ZZ\" THEN 90");
                                        code.AppendLine("25 FOR I%=1 TO LEN(A$) STEP 2");
                                        code.AppendLine("30 V=VAL(\"&H\"+MID$(A$,I%,2))");
                                        code.AppendLine("35 POKE A%,V: A%=A%+1");
                                        code.AppendLine("40 NEXT I%");
                                        code.AppendLine("45 GOTO 20");
                                        code.AppendLine($"90 EXEC &H{startAddress:X4}");
                                        code.AppendLine("95 END");
                                    }

                                    var data = line.Substring(9, bb.Value * 2);
                                    code.AppendLine($"{numLine++} DATA {data}");
                                }
                                else if (tt == 1)
                                {
                                    // Fin des données

                                    code.AppendLine($"{numLine} DATA ZZ");

                                    break;
                                }
                                else
                                {
                                    // Autres valeurs non attendues et non gérées !

                                    error = true;
                                    break;
                                }
                            }
                            else
                            {
                                error = true;
                                break;
                            }
                        }
                        else
                        {
                            error = true;
                            break;
                        }
                    }

                    if (!error)
                    {
                        // Toutes les lignes ont été correctement traitées

                        Invoke(() =>
                        {
                            var f = new FormProgramEditor(code.ToString(), "HEX_LOADER");
                            f.MdiParent = FormMain.Default;
                            f.Show();
                        });

                        return true;
                    }
                }

                MessageBox.Show("Données non conformes !", "X-07 STUDIO");
                return false;
            });
        }

        private int? GetValue(string line, int index, int size)
        {
            if (index + size > line.Length)
                return null;

            var h = line.Substring(index, size);

            if (int.TryParse(h, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v))
                return v;

            return null;
        }
    }
}
