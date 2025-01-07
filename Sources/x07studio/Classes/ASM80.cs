using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static x07studio.Classes.CodeGenerator;

namespace x07studio.Classes
{
    internal class ASM80
    {
        private List<OpDefinition> _Definitions = new();

        public ASM80()
        {
            MakeDefinitions();
        }

        public List<OutLine>? Assemble(string code)
        {
            Debug.WriteLine("");
            Debug.WriteLine(code);
            Debug.WriteLine("");

            var consts = new Dictionary<string, ConstDefinition>();
            var labels = new Dictionary<string, ushort>();
            var outLines = new List<OutLine>();
            var lines = code.Split("\r\n");
            UInt16 pc = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // On enlève les tabulations

                line = line.Replace("\t", "");

                // Si la ligne contient un commentaire on l'enlève avant de traiter la ligne

                var k = line.IndexOf("//");
                if (k > -1) line = line.Substring(0, k).Trim();

                if (line == "")
                {
                    // Ligne vide, on l'ignore
                }
                else if (line.StartsWith("@"))
                {
                    // Définition d'un label

                    // Le nom doit être valide
                    // Composé des lettre A..Z, a...z
                    // Des chiffres 0...9
                    // Des caractères _ -

                    var name = line.Substring(1, line.Length - 1);

                    for (int j = 0; j < name.Length; j++)
                    {
                        var c = name[j];

                        if (!((c >= 'A' && c <= 'Z') ||
                            (c >= 'a' && c <= 'z') ||
                            (c >= '0' && c <= '9') ||
                            c == '_' ||
                            c == '-'))
                        {
                            Debug.WriteLine($"NOM DE LABEL INVALIDE ! - {line}");
                            return null;
                        }
                    }

                    // On ajoute le label aux autre à l'adresse PC actuelle

                    labels.Add(name, pc);
                }
                else if (line.StartsWith("DATA "))
                {
                    var values = line.Substring(5);
                    var items = DispatchValues(values); 
                    
                    if (items.Count > 0)
                    {
                        // On prépare un hexa composé des valeurs 
                        // 2 caractères = V0 hexa
                        // 4 caractères = H1H2 hexa
                        // string = Caractères ASCII
                        // Autre = ERREUR !

                        var sbhexa = new StringBuilder();

                        for (int j = 0; j < items.Count; j++)
                        {
                            var item = items[j];

                            if (item.StartsWith("\"") && item.EndsWith("\""))
                            {
                                // string

                                var s = item.Substring(1, item.Length - 2);
                                var b = Encoding.ASCII.GetBytes(s);

                                for (int m = 0; m < b.Length; m++)
                                {
                                    sbhexa.Append(b[m].ToString("X2"));
                                }
                            }
                            else
                            {
                                // Un nombre hexa ou décimal, sur 8 ou 16 bits

                                if (item.StartsWith("$"))
                                {
                                    // Un nombre hexa

                                    if (ushort.TryParse(item.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out var v))
                                    {
                                        // Nombre hexa valide

                                        if (v < 256)
                                        {
                                            // Valeur sur 8 bits

                                            sbhexa.Append(v.ToString("X2"));
                                        }
                                        else
                                        {
                                            // Valeur sur 16 bits
                                            // On doit inverser H1 et H2

                                            var h16 = v.ToString("X4");
                                            sbhexa.Append(h16.Substring(2,2));
                                            sbhexa.Append(h16.Substring(0, 2));
                                        }
                                    }
                                    else
                                    {
                                        // Nombre hexa non valide

                                        Debug.WriteLine($"DATA INCORRECT ! - {line}");
                                        return null;
                                    }
                                }
                                else
                                {
                                    // Un nombre décimal

                                    if (ushort.TryParse(item, out var v))
                                    {
                                        // Nombre décimal valide

                                        if (v < 256)
                                        {
                                            // Valeur sur 8 bits

                                            sbhexa.Append(v.ToString("X2"));
                                        }
                                        else
                                        {
                                            // Valeur sur 16 bits
                                            // On doit inverser H1 et H2

                                            var h16 = v.ToString("X4");
                                            sbhexa.Append(h16.Substring(2, 2));
                                            sbhexa.Append(h16.Substring(0, 2));
                                        }
                                    }
                                    else
                                    {
                                        // Nombre décimal non valide

                                        Debug.WriteLine($"DATA INCORRECT ! - {line}");
                                        return null;
                                    }
                                }
                            }
                        }

                        // Si hexa > 256 char (> 128 bytes) ---> ERREUR !

                        if (sbhexa.Length > 256)
                        {
                            Debug.WriteLine($"DATA TROP LONG ! - {line}");
                            return null;
                        }

                        var hexa = sbhexa.ToString();

                        // On peut ajouter la ligne

                        var outLine = new OutLine
                            (pc,
                            $"DATA x ${hexa.Length.ToString("X2")}",
                            hexa, null,
                            null);
                        
                        outLines.Add(outLine);
                        pc += (ushort) (hexa.Length / 2);
                    }
                    else
                    {
                        Debug.WriteLine($"DATA NON VALIDE ! - {line}");
                        return null;
                    }
                }
                else if (line.StartsWith("DEFB "))
                {
                    // Constante de type BYTE
                    // Format = DEFB NAME V8
                    // DEFB TOTO 15
                    // DEFB TOTO $34

                    var items = line.Split(' ');

                    if (items.Length == 3 && !consts.ContainsKey(items[1]))
                    {
                        if (items[2].StartsWith("$"))
                        {
                            if (byte.TryParse(items[2].Substring(1), System.Globalization.NumberStyles.HexNumber, null, out var v))
                            {
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X2")));
                                continue;
                            }
                        }
                        else
                        {
                            if (byte.TryParse(items[2], out var v))
                            {
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X2")));
                                continue;
                            }
                        }
                    }

                    // Erreur !!!

                    Debug.WriteLine($"DEFB NON VALIDE ! - {line}");

                }
                else if (line.StartsWith("DEFW "))
                {
                    // Constante de type WORD
                    // Format = DEFW NAME V16
                    // DEFW TOTO 15
                    // DEFW TOTO $3456

                    var items = line.Split(' ');

                    if (items.Length == 3 && !consts.ContainsKey(items[1]))
                    {
                        if (items[2].StartsWith("$"))
                        {
                            if (ushort.TryParse(items[2].Substring(1), System.Globalization.NumberStyles.HexNumber, null, out var v))
                            {
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X4")));
                                continue;
                            }
                        }
                        else
                        {
                            if (ushort.TryParse(items[2], out var v))
                            {
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X4")));
                                continue;
                            }
                        }
                    }

                    // Erreur !!!

                    Debug.WriteLine($"DEFW NON VALIDE ! - {line}");

                }
                else if (line.StartsWith("ORG "))
                {
                    var addr = line.Substring(4);

                    if (addr.StartsWith("$"))
                    {
                        if (ushort.TryParse(addr.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out var a))
                        {
                            pc = a;                    
                        }
                        else
                        {
                            // ORG NON VALIDE !

                            Debug.WriteLine("ORG NON VALIDE !");
                            return null;
                        }
                    }
                    else
                    {
                        if (ushort.TryParse(addr, out var a))
                        {
                            pc = a;
                        }
                        else
                        {
                            // ORG NON VALIDE !

                            Debug.WriteLine("ORG NON VALIDE !");
                            return null;
                        }
                    }

                    var outLine = new OutLine(
                                pc,
                                "ORG",
                                "",
                                null,
                                null);

                    outLines.Add(outLine);
                }
                else
                {
                    // On remplace les constantes par leur valeur

                    var cname = GetConstantName(line);

                    if (cname != null)
                    {
                        if (consts.ContainsKey(cname))
                        {
                            var item = consts[cname];
                            line = line.Replace("#" + cname, item.Value);
                        }
                        else
                        {
                            // CONSTANTE INTROUVABLE !!!

                            Debug.WriteLine($"ERREUR - CONSTANTE INTROUVABLE - {line} !");
                            return null;
                        }
                    }

                    // On recherche l'opération correspondante

                    var operation = SearchOperation(line);

                    if (operation == null)
                    {
                        // OPERATION INTROUVABLE !!!

                        Debug.WriteLine($"ERREUR - OPERATION INTROUVABLE - {line} !");
                        return null;
                    }
                    else
                    {
                        if (operation.WithParameters)
                        {
                            // On extrait la valeur du paramètre
                            // LD A,(@ADDR1) ---> @ADDR1
                            // LD A,(FF45) ---> FF45
                            // Les paramètres seront résolus lors de la passe 2

                            string pValue;
                            int pValueStart;
                            int pValueEnd;

                            if (operation.End.Length == 0)
                            {
                                pValue = line.Substring(operation.Start.Length);
                                pValueStart = operation.Start.Length;
                                pValueEnd = 0;
                            }
                            else
                            {
                                pValueStart = operation.Start.Length;
                                int end = line.LastIndexOf(operation.End);
                                pValueEnd = end;
                                pValue = line.Substring(operation.Start.Length, end - operation.Start.Length);
                            }

                            // Si le paramètre est un label on place la ligne en attente en stipulant le label qu'il faudra résoudre en passe 2
                            // Les autres cas (DEFB et DEFW) sont déjà traités

                            string? labelName = null;

                            if (pValue.StartsWith("@"))
                            {
                                // C'est un label !

                                labelName = pValue.Substring(1);
                            }
                            else if (pValue.StartsWith("$"))
                            {
                                // Valeur en hexa à convertir en X2 ou X4 suivant le type de paramètre attendu
                                // H1H2 ou V0

                                if (operation.Parameter =="H1H2")
                                {
                                    // On attend une valeur hexa sur 16 bits

                                    if (ushort.TryParse(pValue.Substring(1), System.Globalization.NumberStyles.HexNumber,null, out var v16))
                                    {
                                        pValue = v16.ToString("X4");
                                        var newLine = line.Substring(0, pValueStart) + pValue;
                                        if (pValueEnd > 0) newLine += line.Substring(pValueEnd);
                                        line = newLine;
                                    }
                                    else
                                    {
                                        // Conversion impossible !

                                        Debug.WriteLine($"ERREUR - ERREUR CONVERSION HEXA 16 BITS - {line} !");
                                        return null;
                                    }                                   
                                }
                                else if (operation.Parameter == "V0")
                                {
                                    // On attend une valeur hexa sur 8 bits

                                    if (byte.TryParse(pValue.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out var v8))
                                    {
                                        pValue = v8.ToString("X2");
                                        var newLine = line.Substring(0, pValueStart) + pValue;
                                        if (pValueEnd > 0) newLine += line.Substring(pValueEnd);
                                        line = newLine;
                                    }
                                    else
                                    {
                                        // Conversion impossible !

                                        Debug.WriteLine($"ERREUR - ERREUR CONVERSION HEXA 8 BITS - {line} !");
                                        return null;
                                    }
                                }
                            }
                            else
                            {
                                // Valeur en décimal à convertir en X2 ou X4 suivant le type de paramètre attendu
                                // H1H2 ou V0

                                if (operation.Parameter == "H1H2")
                                {
                                    // On attend une valeur décimale

                                    if (ushort.TryParse(pValue, out var v16))
                                    {
                                        pValue = v16.ToString("X4");
                                        var newLine = line.Substring(0, pValueStart) + pValue;
                                        if (pValueEnd > 0) newLine += line.Substring(pValueEnd);
                                        line = newLine;
                                    }
                                    else
                                    {
                                        // Conversion impossible !

                                        Debug.WriteLine($"ERREUR - ERREUR CONVERSION DECIMAL VERS HEXA 16 BITS - {line} !");
                                        return null;
                                    }
                                }
                                else if (operation.Parameter == "V0")
                                {
                                    // On attend une valeur hexa sur 8 bits

                                    if (byte.TryParse(pValue, out var v8))
                                    {
                                        pValue = v8.ToString("X2");
                                        var newLine = line.Substring(0, pValueStart) + pValue;
                                        if (pValueEnd > 0) newLine += line.Substring(pValueEnd);
                                        line = newLine;
                                    }
                                    else
                                    {
                                        // Conversion impossible !

                                        Debug.WriteLine($"ERREUR - ERREUR CONVERSION DECIMAL VERS HEXA 8 BITS - {line} !");
                                        return null;
                                    }
                                }
                            }

                            var outLine = new OutLine(
                                pc, 
                                line, 
                                operation.Hexa, 
                                operation, 
                                labelName);
                            
                            outLines.Add(outLine);
                        }
                        else
                        {
                            // Rien à traiter
                            // On peut tout de suite ajouter la ligne

                            var outLine = new OutLine(
                                pc, 
                                line, 
                                operation.Hexa, 
                                operation, 
                                null);
                             
                            outLines.Add(outLine);
                        }

                        pc += operation.HexaSize;
                    }
                }
            }

            // Passe 2
            // Traitement des paramètres

            for (int i = 0; i < outLines.Count; i++)
            {
                var item = outLines[i];

                if (item.Operation != null)
                {
                    var operation = item.Operation;

                    if (operation.WithParameters)
                    {
                        var line = item.Code;

                        // Il faut traiter la présence d'un paramètre                            
                        // Soit H1H2 ---> valeur hexa sur 16 bits
                        // Soit V0   ---> valeur hexa sur 8 bits
                        // Le paramètre est placé obligatoirement entre le start et le end

                        // Si label on le remplace par son adresse avant le traitement

                        if (item.LabelName != null)
                        {
                            if (labels.ContainsKey(item.LabelName))
                            {
                                // On remplace le label par sa valeur

                                var addr = labels[item.LabelName];
                                line = line.Replace("@" + item.LabelName, addr.ToString("X4"));
                            }
                            else
                            {
                                Debug.WriteLine($"LABEL INTROUVABLE ! - {line}");
                                return null;
                            }
                        }

                        // Quel est le type de paramètre présent ?

                        if (operation.Parameter == "H1H2")
                        {
                            // La taille totale de l'opération est Start+Param+End = Start+4+End
                            // Il faut que ça match avec la line

                            var size = operation.Start.Length + operation.End.Length + 4;

                            if (line.Length != size)
                            {
                                // la partie paramètre n'est pas de la bonne taille !!!

                                Debug.WriteLine($"ERREUR - PARTIE VARIABLE INCORRECTE ! - {line}");
                                return null;
                            }
                            else
                            {
                                // On extrait la partie paramètre de line

                                var p = line.Substring(operation.Start.Length, 4);

                                // On essaye de la convertir en hexa 16 bits

                                if (ushort.TryParse(p, System.Globalization.NumberStyles.HexNumber, null, out var v16))
                                {
                                    // On peut composer la séquence finale hexa

                                    var h1 = p.Substring(0, 2);
                                    var h2 = p.Substring(2, 2);

                                    var hexa = operation.Hexa.Replace("H1", h1);
                                    hexa = hexa.Replace("H2", h2);

                                    var outLine = new OutLine(
                                        item.Address, 
                                        line, hexa, 
                                        operation, 
                                        null);

                                    outLines[i] = outLine;
                                }
                                else
                                {
                                    // Impossible de convertir la valeur hexa ---> Erreur !!!!

                                    Debug.WriteLine("ERREUR - PARTIE VARIABLE INCORRECTE !");
                                    return null;
                                }
                            }
                        }
                        else if (operation.Parameter == "V0")
                        {
                            // On extrait la partie paramètre de line

                            var p = line.Substring(operation.Start.Length, 2);

                            // On essaye de la convertir en hexa 8 bits

                            if (int.TryParse(p, System.Globalization.NumberStyles.HexNumber, null, out var v8))
                            {
                                // On peut composer la séquence finale hexa

                                var hexa = operation.Hexa.Replace("V0", p);

                                var outLine = new OutLine(
                                    item.Address, 
                                    line, 
                                    hexa, 
                                    operation, 
                                    null);

                                outLines[i] = outLine;
                            }
                            else
                            {
                                // Impossible de convertir la valeur hexa ---> Erreur !!!!

                                Debug.WriteLine("ERREUR - PARTIE VARIABLE INCORRECTE !");
                                return null;
                            }
                        }
                        else
                        {
                            // Paramètre inconnu !!!

                            Debug.WriteLine("ERREUR - PARTIE VARIABLE INCONNUE !");
                            return null;
                        }
                    }
                }
            }

            foreach (var outLine in outLines)
            {
                var addr = outLine.Address.ToString("X4");

                var op = outLine.Code;
                if (op.Length < 15) op += new string(' ', 15 - op.Length);
                                               
                Debug.WriteLine($"{addr} - {op} - {outLine.Hexa}");
            }

            Debug.WriteLine("");

            return outLines;
        }

        public string CreateBasicLoader(List<OutLine> outLines)
        {
            var sb = new StringBuilder();

            sb.AppendLine("10 CLS");
            sb.AppendLine("20 A%=&H1000");
            sb.AppendLine("30 READ D$");
            sb.AppendLine("40 IF D$=\"END\" THEN END");
            sb.AppendLine("50 IF D$=\"ORG\" THEN READ D$:A%=VAL(\"&H\"+D$):GOTO 30");
            sb.AppendLine("60 POKE A%,VAL(\"&H\"+D$)");
            sb.AppendLine("70 A%=A%+1");
            sb.AppendLine("80 GOTO 30");

            int numLine = 100;
            var currentDataLine = new StringBuilder();

            for (int i = 0; i < outLines.Count; i++)
            {
                var line = outLines[i];

                if (line.Code == "ORG")
                {
                    if (currentDataLine.Length > 0)
                    {
                        sb.AppendLine($"{numLine} DATA {currentDataLine.ToString()}");
                        numLine += 10;
                        currentDataLine.Clear();
                    }

                    sb.AppendLine($"{numLine} DATA ORG,{line.Address.ToString("X4")}");
                    numLine += 10;
                }
                else
                {
                    for (int j = 0; j < line.Hexa.Length; j += 2)
                    {
                        if (currentDataLine.Length > 0) currentDataLine.Append(",");
                        currentDataLine.Append(line.Hexa.Substring(j, 2));
                    }

                    if (currentDataLine.Length > 30)
                    {
                        sb.AppendLine($"{numLine} DATA {currentDataLine.ToString()}");
                        numLine += 10;
                        currentDataLine.Clear();
                    }
                }
            }

            if (currentDataLine.Length > 0)
            {
                sb.AppendLine($"{numLine} DATA {currentDataLine.ToString()}");
                numLine += 10;
                currentDataLine.Clear();
            }

            sb.AppendLine($"{numLine} DATA END");

            return sb.ToString();
        }

        private string? GetConstantName(string line)
        {
            var k = line.IndexOf('#');

            if (k > -1)
            {
                var sb = new StringBuilder();

                k += 1;
                
                while (k < line.Length)
                {
                    var c = line[k];

                    if ((c >= 'A' && c <= 'Z') ||
                        (c >= 'a' && c <= 'z') ||
                        (c >= '0' && c <= '9') ||
                        c == '_' ||
                        c == '-')
                    {
                        sb.Append(c);
                        k += 1;
                    }
                    else
                    {
                        break;
                    }
                }

                return sb.ToString();           
            }

            return null;
        }

        private OpDefinition? SearchOperation(string line)
        {
            foreach (var op in _Definitions)
            {
                if (op.WithParameters)
                {
                    if (line.StartsWith(op.Start) && line.EndsWith(op.End))
                    {
                        return op;
                    }
                }
                else
                {
                    if (line == op.Start) return op;
                }
            }

            return null;
        }

        private List<string> DispatchValues(string line)
        {
            // 56 34 FF12 "CECI EST UNE STRING" 67 --> 5 éléments

            line = line.Trim();

            var items = new List<string>();
            var inString = false;
            var sb = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '\"')
                {
                    inString = !inString;
                    sb.Append(c);
                }
                else if (c == ',')
                {
                    if (inString)
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        items.Add(sb.ToString());
                        sb.Clear();                      
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length > 0) items.Add(sb.ToString());

            return items;
        }

        private void MakeDefinitions()
        {
            // ADC

            AddDefinition("ADC A,(HL)", "8E");         
            AddDefinition("ADC A,(IX+V0)", "DD8EV0");
            AddDefinition("ADC A,(IY+V0)", "FD8EV0");            
            AddDefinition("ADC A,A", "8F");
            AddDefinition("ADC A,B", "88");
            AddDefinition("ADC A,C", "89");
            AddDefinition("ADC A,D", "8A");
            AddDefinition("ADC A,E", "8B");
            AddDefinition("ADC A,H", "8C");
            AddDefinition("ADC A,L", "8D");
            AddDefinition("ADC A,V0", "CEV0");
            AddDefinition("ADC HL,BC", "ED4A");
            AddDefinition("ADC HL,DE", "ED5A");
            AddDefinition("ADC HL,HL", "ED6A");
            AddDefinition("ADC HL,SP", "ED7A");

            // ADD

            AddDefinition("ADD A,(HL)", "86");
            AddDefinition("ADD A,(IX+V0)", "DD86V0");
            AddDefinition("ADD A,(IY+V0)", "FD86V0");
            AddDefinition("ADD A,A", "87");
            AddDefinition("ADD A,B", "80");
            AddDefinition("ADD A,C", "81");
            AddDefinition("ADD A,D", "82");
            AddDefinition("ADD A,E", "83");
            AddDefinition("ADD A,H", "84");
            AddDefinition("ADD A,L", "85");
            AddDefinition("ADD A,V0", "C6V0");
            AddDefinition("ADD HL,BC", "09");
            AddDefinition("ADD HL,DE", "19");
            AddDefinition("ADD HL,HL", "29");
            AddDefinition("ADD HL,SP", "39");
            AddDefinition("ADD IX,BC", "DD09");
            AddDefinition("ADD IX,DE", "DD19");
            AddDefinition("ADD IX,IX", "DD29");
            AddDefinition("ADD IX,SP", "DD39");
            AddDefinition("ADD IY,BC", "FD09");
            AddDefinition("ADD IY,DE", "FD19");
            AddDefinition("ADD IY,IY", "FD29");
            AddDefinition("ADD IY,SP", "FD39");

            // AND

            AddDefinition("AND (HL)", "A6");
            AddDefinition("AND (IX+V0)", "DDA6V0");
            AddDefinition("AND (IY+V0)", "FDA6V0");
            AddDefinition("AND A", "A7");
            AddDefinition("AND B", "A0");
            AddDefinition("AND C", "A1");
            AddDefinition("AND D", "A2");
            AddDefinition("AND E", "A3");
            AddDefinition("AND H", "A4");
            AddDefinition("AND L", "A5");
            AddDefinition("AND V0", "E6V0");

            // BIT

            AddDefinition("BIT 0,(HL)", "CB46");
            AddDefinition("BIT 0,(IX+V0)", "DDCBV046");
            AddDefinition("BIT 0,(IY+V0)", "FDCBV046");
            AddDefinition("BIT 0,A", "CB47");
            AddDefinition("BIT 0,B", "CB40");
            AddDefinition("BIT 0,C", "CB41");
            AddDefinition("BIT 0,D", "CB42");
            AddDefinition("BIT 0,E", "CB43");
            AddDefinition("BIT 0,H", "CB44");
            AddDefinition("BIT 0,L", "CB45");

            AddDefinition("BIT 1,(HL)", "CB4E");
            AddDefinition("BIT 1,(IX+V0)", "DDCBV04E");
            AddDefinition("BIT 1,(IY+V0)", "FDCBV04E");
            AddDefinition("BIT 1,A", "CB4F");
            AddDefinition("BIT 1,B", "CB48");
            AddDefinition("BIT 1,C", "CB49");
            AddDefinition("BIT 1,D", "CB4A");
            AddDefinition("BIT 1,E", "CB4B");
            AddDefinition("BIT 1,H", "CB4C");
            AddDefinition("BIT 1,L", "CB4D");

            AddDefinition("BIT 2,(HL)", "CB56");
            AddDefinition("BIT 2,(IX+V0)", "DDCBV056");
            AddDefinition("BIT 2,(IY+V0)", "FDCBV056");
            AddDefinition("BIT 2,A", "CB57");
            AddDefinition("BIT 2,B", "CB50");
            AddDefinition("BIT 2,C", "CB51");
            AddDefinition("BIT 2,D", "CB52");
            AddDefinition("BIT 2,E", "CB53");
            AddDefinition("BIT 2,H", "CB54");
            AddDefinition("BIT 2,L", "CB55");

            AddDefinition("BIT 3,(HL)", "CB5E");
            AddDefinition("BIT 3,(IX+V0)", "DDCBV05E");
            AddDefinition("BIT 3,(IY+V0)", "FDCBV05E");
            AddDefinition("BIT 3,A", "CB5F");
            AddDefinition("BIT 3,B", "CB58");
            AddDefinition("BIT 3,C", "CB59");
            AddDefinition("BIT 3,D", "CB5A");
            AddDefinition("BIT 3,E", "CB5B");
            AddDefinition("BIT 3,H", "CB5C");
            AddDefinition("BIT 3,L", "CB5D");

            AddDefinition("BIT 4,(HL)", "CB66");
            AddDefinition("BIT 4,(IX+V0)", "DDCBV066");
            AddDefinition("BIT 4,(IY+V0)", "FDCBV066");
            AddDefinition("BIT 4,A", "CB67");
            AddDefinition("BIT 4,B", "CB60");
            AddDefinition("BIT 4,C", "CB61");
            AddDefinition("BIT 4,D", "CB62");
            AddDefinition("BIT 4,E", "CB63");
            AddDefinition("BIT 4,H", "CB64");
            AddDefinition("BIT 4,L", "CB65");

            AddDefinition("BIT 5,(HL)", "CB6E");
            AddDefinition("BIT 5,(IX+V0)", "DDCBV06E");
            AddDefinition("BIT 5,(IY+V0)", "FDCBV06E");
            AddDefinition("BIT 5,A", "CB6F");
            AddDefinition("BIT 5,B", "CB68");
            AddDefinition("BIT 5,C", "CB69");
            AddDefinition("BIT 5,D", "CB6A");
            AddDefinition("BIT 5,E", "CB6B");
            AddDefinition("BIT 5,H", "CB6C");
            AddDefinition("BIT 5,L", "CB6D");

            AddDefinition("BIT 6,(HL)", "CB76");
            AddDefinition("BIT 6,(IX+V0)", "DDCBV076");
            AddDefinition("BIT 6,(IY+V0)", "FDCBV076");
            AddDefinition("BIT 6,A", "CB77");
            AddDefinition("BIT 6,B", "CB70");
            AddDefinition("BIT 6,C", "CB71");
            AddDefinition("BIT 6,D", "CB72");
            AddDefinition("BIT 6,E", "CB73");
            AddDefinition("BIT 6,H", "CB74");
            AddDefinition("BIT 6,L", "CB75");

            AddDefinition("BIT 7,(HL)", "CB7E");
            AddDefinition("BIT 7,(IX+V0)", "DDCBV07E");
            AddDefinition("BIT 7,(IY+V0)", "FDCBV07E");
            AddDefinition("BIT 7,A", "CB7F");
            AddDefinition("BIT 7,B", "CB78");
            AddDefinition("BIT 7,C", "CB79");
            AddDefinition("BIT 7,D", "CB7A");
            AddDefinition("BIT 7,E", "CB7B");
            AddDefinition("BIT 7,H", "CB7C");
            AddDefinition("BIT 7,L", "CB7D");

            // CALL

            AddDefinition("CALL C,H1H2", "DCH2H1");
            AddDefinition("CALL M,H1H2", "FCH2H1");
            AddDefinition("CALL NC,H1H2", "D4H2H1");
            AddDefinition("CALL H1H2", "CDH2H1");
            AddDefinition("CALL NZ,H1H2", "C4H2H1");
            AddDefinition("CALL P,H1H2", "F4H2H1");
            AddDefinition("CALL PE,H1H2", "ECH2H1");
            AddDefinition("CALL PO,H1H2", "E4H2H1");
            AddDefinition("CALL Z,H1H2", "CCH2H1");

            // CCF

            AddDefinition("CCF", "3F");

            // CP

            AddDefinition("CP (HL)", "BE");
            AddDefinition("CP (IX+V0)", "DDBEV0");
            AddDefinition("CP (IY+V0)", "FDBEV0");
            AddDefinition("CP A", "BF");
            AddDefinition("CP B", "B8");
            AddDefinition("CP C", "B9");
            AddDefinition("CP D", "BZ");
            AddDefinition("CP E", "BB");
            AddDefinition("CP H", "BC");
            AddDefinition("CP L", "BD");
            AddDefinition("CP V0", "FEV0");

            // CPD

            AddDefinition("CPD", "EDA9");

            // CPDR

            AddDefinition("CPDR", "EDB9");

            // CPI

            AddDefinition("CPI", "EDA1");

            // CPIR

            AddDefinition("CPIR", "EDB1");

            // CPL

            AddDefinition("CPL", "2F");

            // DEC

            AddDefinition("DEC (HL)", "35");
            AddDefinition("DEC (IX+V0)", "DD35V0");
            AddDefinition("DEC (IY+V0)", "FD35V0");
            AddDefinition("DEC A", "3D");
            AddDefinition("DEC B", "05");
            AddDefinition("DEC BC", "0B");
            AddDefinition("DEC C", "0D");
            AddDefinition("DEC D", "15");
            AddDefinition("DEC DE", "1B");
            AddDefinition("DEC E", "1D");
            AddDefinition("DEC H", "25");
            AddDefinition("DEC HL", "2B");
            AddDefinition("DEC IX", "DD2B");
            AddDefinition("DEC IY", "FD2B");
            AddDefinition("DEC L", "2D");
            AddDefinition("DEC SP", "3B");

            // DI

            AddDefinition("DI", "F3");

            // DJNZ

            AddDefinition("DJNZ H1H2", "10H2H1");

            // EI

            AddDefinition("EI", "FB");

            // EX

            AddDefinition("EX (SP),HL", "E3");
            AddDefinition("EX (SP),IX", "DDE3");
            AddDefinition("EX (SP),IY", "FDE3");
            AddDefinition("EX AF,AF'", "08");
            AddDefinition("EX DE,HL", "EB");

            // EXX

            AddDefinition("EXX", "D9");

            // HALT

            AddDefinition("HALT", "76");

            // IM

            AddDefinition("IM 0", "ED46");
            AddDefinition("IM 1", "ED56");
            AddDefinition("IM 2", "ED5E");

            // IN

            AddDefinition("IN A,(C)", "ED78");
            AddDefinition("IN A,(V0)", "DBV0");
            AddDefinition("IN B,(C)", "ED40");
            AddDefinition("IN C,(C)", "ED48");
            AddDefinition("IN D,(C)", "ED50");
            AddDefinition("IN E,(C)", "ED58");
            AddDefinition("IN H,(C)", "ED60");
            AddDefinition("IN L,(C)", "ED68");

            // INC

            AddDefinition("INC (HL)", "34");
            AddDefinition("INC (IX+V0)", "DD34V0");
            AddDefinition("INC (IY+V0)", "FD34V0");
            AddDefinition("INC A", "3C");
            AddDefinition("INC B", "04");
            AddDefinition("INC BC", "03");
            AddDefinition("INC C", "0C");
            AddDefinition("INC D", "14");
            AddDefinition("INC DE", "13");
            AddDefinition("INC E", "1C");
            AddDefinition("INC H", "24");
            AddDefinition("INC HL", "23");
            AddDefinition("INC IX", "DD23");
            AddDefinition("INC IY", "FD23");
            AddDefinition("INC L", "2C");
            AddDefinition("INC SP", "33");

            // IND

            AddDefinition("IND", "EDAA");

            // INDR

            AddDefinition("INDR", "EDBA");

            // INI

            AddDefinition("INI", "EDA2");

            // INIR

            AddDefinition("INIR", "EDB2");

            // JP

            AddDefinition("JP (HL)", "E9");
            AddDefinition("JP (IX)", "DDE9");
            AddDefinition("JP (IY)", "FDE9");
            AddDefinition("JP C,H1H2", "DAH2H1");
            AddDefinition("JP M,H1H2", "FAH2H1");
            AddDefinition("JP NC,H1H2", "D2H2H1");
            AddDefinition("JP H1H2", "C3H2H1");
            AddDefinition("JP NZ,H1H2", "C2H2H1");
            AddDefinition("JP P,H1H2", "F2H2H1");
            AddDefinition("JP PE,H1H2", "EAH2H1");
            AddDefinition("JP PO,H1H2", "E2H2H1");
            AddDefinition("JP Z,H1H2", "CAH2H1");

            // JR

            AddDefinition("JR C,V0", "38V0");
            AddDefinition("JR V0", "18V0");
            AddDefinition("JR NC,V0", "30V0");
            AddDefinition("JR NZ,V0", "20V0");
            AddDefinition("JR Z,V0", "28V0");

            // LD

            AddDefinition("LD (BC),A", "02");
            AddDefinition("LD (DE),A", "12");
            AddDefinition("LD (HL),A", "77");
            AddDefinition("LD (HL),B", "70");
            AddDefinition("LD (HL),C", "71");
            AddDefinition("LD (HL),D", "72");
            AddDefinition("LD (HL),E", "73");
            AddDefinition("LD (HL),H", "74");
            AddDefinition("LD (HL),L", "75");
            AddDefinition("LD (HL),V0", "36V0");

            AddDefinition("LD (IX+V0),A", "DD77V0");
            AddDefinition("LD (IX+V0),B", "DD70V0");
            AddDefinition("LD (IX+V0),C", "DD71V0");
            AddDefinition("LD (IX+V0),D", "DD72V0");
            AddDefinition("LD (IX+V0),E", "DD73V0");
            AddDefinition("LD (IX+V0),H", "DD74V0");
            AddDefinition("LD (IX+V0),L", "DD75V0");
            AddDefinition("LD (IX+V0),V1", "DD36V0V1");

            AddDefinition("LD (IY+V0),A", "FD77V0");
            AddDefinition("LD (IY+V0),B", "FD70V0");
            AddDefinition("LD (IY+V0),C", "FD71V0");
            AddDefinition("LD (IY+V0),D", "FD72V0");
            AddDefinition("LD (IY+V0),E", "FD73V0");
            AddDefinition("LD (IY+V0),H", "FD74V0");
            AddDefinition("LD (IY+V0),L", "FD75V0");
            AddDefinition("LD (IY+V0),V1", "FD36V0V1");

            AddDefinition("LD (H1H2), IX", "DD22H2H1");
            AddDefinition("LD (H1H2), IY", "FD22H2H1");
            AddDefinition("LD (H1H2), SP", "ED22H2H1");

            AddDefinition("LD A,(BC)", "0A");
            AddDefinition("LD A,(DE)", "1A");
            AddDefinition("LD A,(HL)", "7E");
            AddDefinition("LD A,(IX+V0)", "DD7EV0");
            AddDefinition("LD A,(IY+V0)", "FD7EV0");
            AddDefinition("LD A,(H1H2)", "3AH2H1");
            AddDefinition("LD A,A", "7F");
            AddDefinition("LD A,B", "78");
            AddDefinition("LD A,C", "79");
            AddDefinition("LD A,D", "7A");
            AddDefinition("LD A,E", "7B");
            AddDefinition("LD A,H", "7C");
            AddDefinition("LD A,I", "ED57");
            AddDefinition("LD A,L", "7D");
            AddDefinition("LD A,V0", "3EV0");
            AddDefinition("LD A,R", "ED5F");

            AddDefinition("LD B,(HL)", "46");
            AddDefinition("LD B,(IX+V0)", "DD46V0");
            AddDefinition("LD B,(IY+V0)", "FD46V0");
            AddDefinition("LD B,A", "47");
            AddDefinition("LD B,B", "40");
            AddDefinition("LD B,C", "41");
            AddDefinition("LD B,D", "42");
            AddDefinition("LD B,E", "43");
            AddDefinition("LD B,H", "44");
            AddDefinition("LD B,L", "45");
            AddDefinition("LD B,V0", "06V0");
            AddDefinition("LD BC,(H1H2)", "ED4BH2H1");
            AddDefinition("LD BC,H1H2", "01H2H1");

            AddDefinition("LD C,(HL)", "4E");
            AddDefinition("LD C,(IX+V0)", "DD4EV0");
            AddDefinition("LD C,(IY+V0)", "FD4EV0");
            AddDefinition("LD C,A", "4F");
            AddDefinition("LD C,B", "48");
            AddDefinition("LD C,C", "49");
            AddDefinition("LD C,D", "4A");
            AddDefinition("LD C,E", "4B");
            AddDefinition("LD C,H", "4C");
            AddDefinition("LD C,L", "4D");
            AddDefinition("LD C,V0", "0EV0");

            AddDefinition("LD D,(HL)", "56");
            AddDefinition("LD D,(IX+V0)", "DD56V0");
            AddDefinition("LD D,(IY+V0)", "FD56V0");
            AddDefinition("LD D,A", "57");
            AddDefinition("LD D,B", "50");
            AddDefinition("LD D,C", "51");
            AddDefinition("LD D,D", "52");
            AddDefinition("LD D,E", "53");
            AddDefinition("LD D,H", "54");
            AddDefinition("LD D,L", "55");
            AddDefinition("LD D,V0", "16V0");
            AddDefinition("LD DE,(H1H2)", "ED5BH2H1");
            AddDefinition("LD DE,H1H2", "11H2H1");

            AddDefinition("LD E,(HL)", "5E");
            AddDefinition("LD E,(IX+V0)", "DD5EV0");
            AddDefinition("LD E,(IY+V0)", "FD5EV0");
            AddDefinition("LD E,A", "5F");
            AddDefinition("LD E,B", "58");
            AddDefinition("LD E,C", "59");
            AddDefinition("LD E,D", "5A");
            AddDefinition("LD E,E", "5B");
            AddDefinition("LD E,H", "5C");
            AddDefinition("LD E,L", "5D");
            AddDefinition("LD E,V0", "1EV0");

            AddDefinition("LD H,(HL)", "66");
            AddDefinition("LD H,(IX+V0)", "DD66V0");
            AddDefinition("LD H,(IY+V0)", "FD66V0");
            AddDefinition("LD H,A", "67");
            AddDefinition("LD H,B", "60");
            AddDefinition("LD H,C", "61");
            AddDefinition("LD H,D", "62");
            AddDefinition("LD H,E", "63");
            AddDefinition("LD H,H", "64");
            AddDefinition("LD H,L", "65");
            AddDefinition("LD H,V0", "26V0");
            AddDefinition("LD HL,(H1H2)", "2AH2H1");
            AddDefinition("LD HL,H1H2", "21H2H1");

            AddDefinition("LD I,A", "ED47");
            AddDefinition("LD IX,(H1H2)", "DD2AH2H1");
            AddDefinition("LD IX,H1H2", "DD21H2H1");
            AddDefinition("LD IY,(H1H2)", "FD2AH2H1");
            AddDefinition("LD IY,H1H2", "FD21H2H1");

            AddDefinition("LD L,(HL)", "6E");
            AddDefinition("LD L,(IX+V0)", "DD6EV0");
            AddDefinition("LD L,(IY+V0)", "FD6EV0");
            AddDefinition("LD L,A", "6F");
            AddDefinition("LD L,B", "68");
            AddDefinition("LD L,C", "69");
            AddDefinition("LD L,D", "6A");
            AddDefinition("LD L,E", "6B");
            AddDefinition("LD L,H", "6C");
            AddDefinition("LD L,L", "6D");
            AddDefinition("LD L,V0", "2EV0");

            AddDefinition("LD R,A", "ED47");

            AddDefinition("LD SP,(H1H2)", "ED7BH2H1");
            AddDefinition("LD SP,HL", "F9");
            AddDefinition("LD SP,IX", "DDF9");
            AddDefinition("LD SP,IY", "FDF9");
            AddDefinition("LD SP,H1H2", "31H2H1");

            // LDD

            AddDefinition("LDD", "EDA8");

            // LDIR

            AddDefinition("LDIR", "EDB0");

            // NEG

            AddDefinition("NEG", "ED44");

            // NOP

            AddDefinition("NOP", "00");

            // OR

            AddDefinition("OR (HL)", "B6");
            AddDefinition("OR (IX+V0)", "DDB6V0");
            AddDefinition("OR (IY+V0)", "FDB6V0");
            AddDefinition("OR A", "B7");
            AddDefinition("OR B", "B0");
            AddDefinition("OR C", "B1");
            AddDefinition("OR D", "B2");
            AddDefinition("OR E", "B3");
            AddDefinition("OR H", "B4");
            AddDefinition("OR L", "B5");
            AddDefinition("OR V0", "F6V0");

            // OTDR

            AddDefinition("OTDR", "EDBB");

            // OTIR

            AddDefinition("OTIR", "EDB3");

            // OUT

            AddDefinition("OUT (C),A", "ED79");
            AddDefinition("OUT (C),B", "ED41");
            AddDefinition("OUT (C),C", "ED49");
            AddDefinition("OUT (C),D", "ED51");
            AddDefinition("OUT (C),E", "ED59");
            AddDefinition("OUT (C),H", "ED61");
            AddDefinition("OUT (C),L", "ED69");
            AddDefinition("OUT (V0),A", "D3V0");

            // OUTD

            AddDefinition("OUTD", "EDAB");

            // OUTI

            AddDefinition("OUTI", "EDA3");

            // POP

            AddDefinition("POP AF", "F1");
            AddDefinition("POP BC", "C1");
            AddDefinition("POP DE", "D1");
            AddDefinition("POP HL", "E1");
            AddDefinition("POP IX", "DDE1");
            AddDefinition("POP IY", "FDE1");

            // PUSH

            AddDefinition("PUSH AF", "F5");
            AddDefinition("PUSH BC", "C5");
            AddDefinition("PUSH DE", "D5");
            AddDefinition("PUSH HL", "E5");
            AddDefinition("PUSH IX", "DDE5");
            AddDefinition("PUSH IY", "FDE5");

            // RES

            AddDefinition("RES 0,(HL)", "CB86");
            AddDefinition("RES 0,(IX+V0)", "DDCBV086");
            AddDefinition("RES 0,(IY+V0)", "FDCBV086");
            AddDefinition("RES 0,A", "CB87");
            AddDefinition("RES 0,B", "CB80");
            AddDefinition("RES 0,C", "CB81");
            AddDefinition("RES 0,D", "CB82");
            AddDefinition("RES 0,E", "CB83");
            AddDefinition("RES 0,H", "CB84");
            AddDefinition("RES 0,L", "CB85");

            AddDefinition("RES 1,(HL)", "CB8E");
            AddDefinition("RES 1,(IX+V0)", "DDCBV08E");
            AddDefinition("RES 1,(IY+V0)", "FDCBV08E");
            AddDefinition("RES 1,A", "CB8F");
            AddDefinition("RES 1,B", "CB88");
            AddDefinition("RES 1,C", "CB89");
            AddDefinition("RES 1,D", "CB8A");
            AddDefinition("RES 1,E", "CB8B");
            AddDefinition("RES 1,H", "CB8C");
            AddDefinition("RES 1,L", "CB8D");

            AddDefinition("RES 2,(HL)", "CB96");
            AddDefinition("RES 2,(IX+V0)", "DDCBV096");
            AddDefinition("RES 2,(IY+V0)", "FDCBV096");
            AddDefinition("RES 2,A", "CB97");
            AddDefinition("RES 2,B", "CB90");
            AddDefinition("RES 2,C", "CB91");
            AddDefinition("RES 2,D", "CB92");
            AddDefinition("RES 2,E", "CB93");
            AddDefinition("RES 2,H", "CB94");
            AddDefinition("RES 2,L", "CB95");

            AddDefinition("RES 3,(HL)", "CB9E");
            AddDefinition("RES 3,(IX+V0)", "DDCBV09E");
            AddDefinition("RES 3,(IY+V0)", "FDCBV09E");
            AddDefinition("RES 3,A", "CB9F");
            AddDefinition("RES 3,B", "CB98");
            AddDefinition("RES 3,C", "CB99");
            AddDefinition("RES 3,D", "CB9A");
            AddDefinition("RES 3,E", "CB9B");
            AddDefinition("RES 3,H", "CB9C");
            AddDefinition("RES 3,L", "CB9D");

            AddDefinition("RES 4,(HL)", "CBA6");
            AddDefinition("RES 4,(IX+V0)", "DDCBV0A6");
            AddDefinition("RES 4,(IY+V0)", "FDCBV0A6");
            AddDefinition("RES 4,A", "CBA7");
            AddDefinition("RES 4,B", "CBA0");
            AddDefinition("RES 4,C", "CBA1");
            AddDefinition("RES 4,D", "CBA2");
            AddDefinition("RES 4,E", "CBA3");
            AddDefinition("RES 4,H", "CBA4");
            AddDefinition("RES 4,L", "CBA5");

            AddDefinition("RES 5,(HL)", "CBAE");
            AddDefinition("RES 5,(IX+V0)", "DDCBV0AE");
            AddDefinition("RES 5,(IY+V0)", "FDCBV0AE");
            AddDefinition("RES 5,A", "CBAF");
            AddDefinition("RES 5,B", "CBA8");
            AddDefinition("RES 5,C", "CBA9");
            AddDefinition("RES 5,D", "CBAA");
            AddDefinition("RES 5,E", "CBAB");
            AddDefinition("RES 5,H", "CBAC");
            AddDefinition("RES 5,L", "CBAD");

            AddDefinition("RES 6,(HL)", "CBB6");
            AddDefinition("RES 6,(IX+V0)", "DDCBV0B6");
            AddDefinition("RES 6,(IY+V0)", "FDCBV0B6");
            AddDefinition("RES 6,A", "CBB7");
            AddDefinition("RES 6,B", "CBB0");
            AddDefinition("RES 6,C", "CBB1");
            AddDefinition("RES 6,D", "CBB2");
            AddDefinition("RES 6,E", "CBB3");
            AddDefinition("RES 6,H", "CBB4");
            AddDefinition("RES 6,L", "CBB5");

            AddDefinition("RES 7,(HL)", "CBBE");
            AddDefinition("RES 7,(IX+V0)", "DDCBV0BE");
            AddDefinition("RES 7,(IY+V0)", "FDCBV0BE");
            AddDefinition("RES 7,A", "CBBF");
            AddDefinition("RES 7,B", "CBB8");
            AddDefinition("RES 7,C", "CBB9");
            AddDefinition("RES 7,D", "CBBA");
            AddDefinition("RES 7,E", "CBBB");
            AddDefinition("RES 7,H", "CBBC");
            AddDefinition("RES 7,L", "CBBD");

            // RET

            AddDefinition("RET", "C9");
            AddDefinition("RET C", "D8");
            AddDefinition("RET M", "F8");
            AddDefinition("RET NC", "D0");
            AddDefinition("RET NZ", "C0");
            AddDefinition("RET P", "F0");
            AddDefinition("RET PE", "E8");
            AddDefinition("RET PO", "E0");
            AddDefinition("RET Z", "C8");

            // RETI

            AddDefinition("RETI", "ED4D");

            // RTN

            AddDefinition("RETN", "ED45");

            // RL

            AddDefinition("RL (HL)", "CB16");
            AddDefinition("RL (IX+V0)", "DDCBV016");
            AddDefinition("RL (IY+V0)", "FDCBV016");
            AddDefinition("RL A", "CB17");
            AddDefinition("RL B", "CB10");
            AddDefinition("RL C", "CB11");
            AddDefinition("RL D", "CB12");
            AddDefinition("RL E", "CB13");
            AddDefinition("RL H", "CB14");
            AddDefinition("RL L", "CB15");

            // RLA

            AddDefinition("RLA", "17");

            // RLC

            AddDefinition("RLC (HL)", "CB06");
            AddDefinition("RLC (IX+V0)", "DDCBV006");
            AddDefinition("RLC (IY+V0)", "FDCBV006");
            AddDefinition("RLC A", "CB07");
            AddDefinition("RLC B", "CB00");
            AddDefinition("RLC C", "CB01");
            AddDefinition("RLC D", "CB02");
            AddDefinition("RLC E", "CB03");
            AddDefinition("RLC H", "CB04");
            AddDefinition("RLC L", "CB05");

            // RLCA

            AddDefinition("RLCA", "07");

            // RLD

            AddDefinition("RLD", "ED6F");

            // RR

            AddDefinition("RR (HL)", "CB1E");
            AddDefinition("RR (IX+V0)", "DDCBV01E");
            AddDefinition("RR (IY+V0)", "FDCBV01E");
            AddDefinition("RR A", "CB1F");
            AddDefinition("RR B", "CB18");
            AddDefinition("RR C", "CB19");
            AddDefinition("RR D", "CB1A");
            AddDefinition("RR E", "CB1B");
            AddDefinition("RR H", "CB1C");
            AddDefinition("RR L", "CB1D");

            // RRA

            AddDefinition("RRA", "1F");

            // RRC

            AddDefinition("RRC (HL)", "CB0E");
            AddDefinition("RRC (IX+V0)", "DDCBV00E");
            AddDefinition("RRC (IY+V0)", "FDCBV00E");
            AddDefinition("RRC A", "CB0F");
            AddDefinition("RRC B", "CB08");
            AddDefinition("RRC C", "CB09");
            AddDefinition("RRC D", "CB0A");
            AddDefinition("RRC E", "CB0B");
            AddDefinition("RRC H", "CB0C");
            AddDefinition("RRC L", "CB0D");

            // RRCA

            AddDefinition("RRCA", "0F");

            // RRD

            AddDefinition("RRD", "ED67");

            // RST

            AddDefinition("RST $00", "C7");
            AddDefinition("RST $08", "CF");
            AddDefinition("RST $10", "D7");
            AddDefinition("RST $18", "DF");
            AddDefinition("RST $20", "E7");
            AddDefinition("RST $28", "EF");
            AddDefinition("RST $30", "F7");
            AddDefinition("RST $38", "FF");

            // SBC

            AddDefinition("SBC A,(HL)", "9E");
            AddDefinition("SBC A,(IX+V0)", "DD9EV0");
            AddDefinition("SBC A,(IY+V0)", "FD9EV0");
            AddDefinition("SBC A,A", "9F");
            AddDefinition("SBC A,B", "98");
            AddDefinition("SBC A,C", "99");
            AddDefinition("SBC A,D", "9A");
            AddDefinition("SBC A,E", "9B");
            AddDefinition("SBC A,H", "9C");
            AddDefinition("SBC A,L", "9D");
            AddDefinition("SBC A,V0", "DEV0");
            AddDefinition("SBC HL,BC", "ED42");
            AddDefinition("SBC HL,DE", "ED52");
            AddDefinition("SBC HL,HL", "ED62");
            AddDefinition("SBC HL,SP", "ED72");

            // SCF

            AddDefinition("SCF", "37");

            // SET

            AddDefinition("SET 0,(HL)", "CBC6");
            AddDefinition("SET 0,(IX+V0)", "DDCBV0C6");
            AddDefinition("SET 0,(IY+V0)", "FDCBV0C6");
            AddDefinition("SET 0,A", "CBC7");
            AddDefinition("SET 0,B", "CBC0");
            AddDefinition("SET 0,C", "CBC1");
            AddDefinition("SET 0,D", "CBC2");
            AddDefinition("SET 0,E", "CBC3");
            AddDefinition("SET 0,H", "CBC4");
            AddDefinition("SET 0,L", "CBC5");

            AddDefinition("SET 1,(HL)", "CBCE");
            AddDefinition("SET 1,(IX+V0)", "DDCBV0CE");
            AddDefinition("SET 1,(IY+V0)", "FDCBV0CE");
            AddDefinition("SET 1,A", "CBCF");
            AddDefinition("SET 1,B", "CBC8");
            AddDefinition("SET 1,C", "CBC9");
            AddDefinition("SET 1,D", "CBCA");
            AddDefinition("SET 1,E", "CBCB");
            AddDefinition("SET 1,H", "CBCC");
            AddDefinition("SET 1,L", "CBCD");

            AddDefinition("SET 2,(HL)", "CBD6");
            AddDefinition("SET 2,(IX+V0)", "DDCBV0D6");
            AddDefinition("SET 2,(IY+V0)", "FDCBV0D6");
            AddDefinition("SET 2,A", "CBD7");
            AddDefinition("SET 2,B", "CBD0");
            AddDefinition("SET 2,C", "CBD1");
            AddDefinition("SET 2,D", "CBD2");
            AddDefinition("SET 2,E", "CBD3");
            AddDefinition("SET 2,H", "CBD4");
            AddDefinition("SET 2,L", "CBD5");

            AddDefinition("SET 3,(HL)", "CBDE");
            AddDefinition("SET 3,(IX+V0)", "DDCBV0DE");
            AddDefinition("SET 3,(IY+V0)", "FDCBV0DE");
            AddDefinition("SET 3,A", "CBDF");
            AddDefinition("SET 3,B", "CBD8");
            AddDefinition("SET 3,C", "CBD9");
            AddDefinition("SET 3,D", "CBDA");
            AddDefinition("SET 3,E", "CBDB");
            AddDefinition("SET 3,H", "CBDC");
            AddDefinition("SET 3,L", "CBDD");

            AddDefinition("SET 4,(HL)", "CBE6");
            AddDefinition("SET 4,(IX+V0)", "DDCBV0E6");
            AddDefinition("SET 4,(IY+V0)", "FDCBV0E6");
            AddDefinition("SET 4,A", "CBE7");
            AddDefinition("SET 4,B", "CBE0");
            AddDefinition("SET 4,C", "CBE1");
            AddDefinition("SET 4,D", "CBE2");
            AddDefinition("SET 4,E", "CBE3");
            AddDefinition("SET 4,H", "CBE4");
            AddDefinition("SET 4,L", "CBE5");

            AddDefinition("SET 5,(HL)", "CBEE");
            AddDefinition("SET 5,(IX+V0)", "DDCBV0EE");
            AddDefinition("SET 5,(IY+V0)", "FDCBV0EE");
            AddDefinition("SET 5,A", "CBEF");
            AddDefinition("SET 5,B", "CBE8");
            AddDefinition("SET 5,C", "CBE9");
            AddDefinition("SET 5,D", "CBEA");
            AddDefinition("SET 5,E", "CBEB");
            AddDefinition("SET 5,H", "CBEC");
            AddDefinition("SET 5,L", "CBED");

            AddDefinition("SET 6,(HL)", "CBF6");
            AddDefinition("SET 6,(IX+V0)", "DDCBV0F6");
            AddDefinition("SET 6,(IY+V0)", "FDCBV0F6");
            AddDefinition("SET 6,A", "CBF7");
            AddDefinition("SET 6,B", "CBF0");
            AddDefinition("SET 6,C", "CBF1");
            AddDefinition("SET 6,D", "CBF2");
            AddDefinition("SET 6,E", "CBF3");
            AddDefinition("SET 6,H", "CBF4");
            AddDefinition("SET 6,L", "CBF5");

            AddDefinition("SET 7,(HL)", "CBFE");
            AddDefinition("SET 7,(IX+V0)", "DDCBV0FE");
            AddDefinition("SET 7,(IY+V0)", "FDCBV0FE");
            AddDefinition("SET 7,A", "CBFF");
            AddDefinition("SET 7,B", "CBF8");
            AddDefinition("SET 7,C", "CBF9");
            AddDefinition("SET 7,D", "CBFA");
            AddDefinition("SET 7,E", "CBFB");
            AddDefinition("SET 7,H", "CBFC");
            AddDefinition("SET 7,L", "CBFD");

            // SLA

            AddDefinition("SLA (HL)", "CB26");
            AddDefinition("SLA (IX+V0)", "DDCBV026");
            AddDefinition("SLA (IY+V0)", "FDCBV026");
            AddDefinition("SLA A", "CB27");
            AddDefinition("SLA B", "CB20");
            AddDefinition("SLA C", "CB21");
            AddDefinition("SLA D", "CB22");
            AddDefinition("SLA E", "CB23");
            AddDefinition("SLA H", "CB24");
            AddDefinition("SLA L", "CB25");

            // SRA

            AddDefinition("SRA (HL)", "CB2E");
            AddDefinition("SRA (IX+V0)", "DDCBV02E");
            AddDefinition("SRA (IY+V0)", "FDCBV02E");
            AddDefinition("SRA A", "CB2F");
            AddDefinition("SRA B", "CB28");
            AddDefinition("SRA C", "CB29");
            AddDefinition("SRA D", "CB2A");
            AddDefinition("SRA E", "CB2B");
            AddDefinition("SRA H", "CB2C");
            AddDefinition("SRA L", "CB2D");

            // SRL

            AddDefinition("SRL (HL)", "CB3E");
            AddDefinition("SRL (IX+V0)", "DDCBV03E");
            AddDefinition("SRL (IY+V0)", "FDCBV03E");
            AddDefinition("SRL A", "CB3F");
            AddDefinition("SRL B", "CB38");
            AddDefinition("SRL C", "CB39");
            AddDefinition("SRL D", "CB3A");
            AddDefinition("SRL E", "CB3B");
            AddDefinition("SRL H", "CB3C");
            AddDefinition("SRL L", "CB3D");

            // SUB

            AddDefinition("SUB (HL)", "96");
            AddDefinition("SUB (IX+V0)", "DD96V0");
            AddDefinition("SUB (IY+V0)", "FD96V0");
            AddDefinition("SUB A", "97");
            AddDefinition("SUB B", "90");
            AddDefinition("SUB C", "91");
            AddDefinition("SUB D", "92");
            AddDefinition("SUB E", "93");
            AddDefinition("SUB H", "94");
            AddDefinition("SUB L", "95");
            AddDefinition("SUB V0", "D6V0");

            // XOR

            AddDefinition("XOR (HL)", "AE");
            AddDefinition("XOR (IX+V0)", "DDAEV0");
            AddDefinition("XOR (IY+V0)", "FDAEV0");
            AddDefinition("XOR A", "AF");
            AddDefinition("XOR B", "A8");
            AddDefinition("XOR C", "A9");
            AddDefinition("XOR D", "AA");
            AddDefinition("XOR E", "AB");
            AddDefinition("XOR H", "AC");
            AddDefinition("XOR L", "AD");
            AddDefinition("XOR V0", "EEV0");

            // On trie les définitions du plus grand au plus petit

            int DefinitionSort(OpDefinition d1,  OpDefinition d2) => d2.Start.CompareTo(d1.Start);  
            _Definitions.Sort(DefinitionSort);
        }

        private void AddDefinition(string op, string hexa)
        {
            _Definitions.Add(new OpDefinition(op, hexa));
        }

        public class OpDefinition
        {
            public bool WithParameters { get; set; }
            
            public string Start { get; set; } = "";
            
            public string End { get; set; } = "";
            
            public string Parameter { get; set; } = "";
            
            public string Hexa { get; set; } = "";

            public UInt16 HexaSize => (UInt16)(Hexa.Length / 2);

            public OpDefinition(string op, string hexa)
            {
                Hexa = hexa;

                // 2 paramètres possibles
                // V0 et H1H2
                
                var k = op.IndexOf("V0");

                if (k > -1)
                {
                    // V0

                    WithParameters = true;
                    Parameter = "V0";
                    Start = op.Substring(0, k);
                    if (k + 2 < op.Length) End = op.Substring(k + 2);
                }
                else
                {
                    k = op.IndexOf("H1H2");

                    if (k > -1)
                    {
                        // H1H2

                        WithParameters = true;
                        Parameter = "H1H2";
                        Start = op.Substring(0, k);
                        if (k + 4 < op.Length) End = op.Substring(k + 4);
                    }
                    else
                    {
                        WithParameters = false;
                        Start = op;
                    }
                }                 
            }

            public OpDefinition(string start, string end, string parameter, string hexa)
            {
                WithParameters = true;  
                Start = start;
                End = end;
                Parameter = parameter;
                Hexa = hexa;
            }
        }

        public class OutLine
        {
            public UInt16 Address { get; set; }
            
            public string Code { get; set; }

            public string Hexa { get; set; }

            public OpDefinition? Operation { get; set; }

            public string? LabelName { get; set; }

            public OutLine(UInt16 address, string code, string hexa, OpDefinition? operation, string? labelName)
            {
                Address = address;
                Code = code;
                Hexa = hexa;
                Operation = operation;
                LabelName = labelName;             
            }
        }

        public class ConstDefinition
        {
            public string Name { get; set; }

            public string Value { get; set; }

            public ConstDefinition(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }
    }
}
