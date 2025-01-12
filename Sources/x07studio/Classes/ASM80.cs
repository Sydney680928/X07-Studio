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

        public AssembleResult Assemble(string code)
        {
            Debug.WriteLine("");
            Debug.WriteLine(code);
            Debug.WriteLine("");

            var consts = new Dictionary<string, ConstDefinition>();
            var labels = new Dictionary<string, ushort>();
            var outLines = new List<OutLine>();
            var lines = code.Split("\r\n");
            UInt16 pc = 0x1000;

            for (int i = 0; i < lines.Length; i++)
            {
                var source = lines[i];
                var line = source;              

                // On enlève les tabulations

                line = line.Replace("\t", "");

                // Si la ligne contient un commentaire on l'enlève avant de traiter la ligne

                var k = line.IndexOf("//");
                if (k > -1) line = line.Substring(0, k).Trim();

                var upperLine = line.ToUpper();

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
                            return new AssembleResult(i, "NOM DE LABEL INVALIDE !", line);
                        }
                    }

                    // On ajoute le label aux autre à l'adresse PC actuelle

                    labels.Add(name, pc);
                }
                else if (upperLine.StartsWith("DATA "))
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
                                            sbhexa.Append(h16.Substring(2, 2));
                                            sbhexa.Append(h16.Substring(0, 2));
                                        }
                                    }
                                    else
                                    {
                                        // Nombre hexa non valide

                                        Debug.WriteLine($"DATA INCORRECT ! - {line}");
                                        return new AssembleResult(i, "DATA INCORRECT !", line);
                                    }
                                }
                                else if (item.StartsWith("%"))
                                {
                                    // Un nombre binaire (max 16 bits)

                                    UInt16 v = 0;

                                    try
                                    {
                                        v = Convert.ToUInt16(item.Substring(1), 2);
                                    }
                                    catch (Exception ex)
                                    {
                                        // Erreur de convertion, non reconnu ou en dehors du UInt16

                                        Debug.WriteLine($"DATA INCORRECT ! - {line}");
                                        return new AssembleResult(i, "DATA INCORRECT !", line);
                                    }

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
                                        return new AssembleResult(i, "DATA INCORRECT !", line);
                                    }
                                }
                            }
                        }

                        // Si hexa > 256 char (> 128 bytes) ---> ERREUR !

                        if (sbhexa.Length > 256)
                        {
                            Debug.WriteLine($"DATA TROP LONG ! - {line}");
                            return new AssembleResult(i, "DATA TROP LONG !", line);
                        }

                        var hexa = sbhexa.ToString();
                        
                        // On peut ajouter la ligne

                        var outLine = new OutLine
                            (pc,
                            i,
                            source,
                            $"DATA",
                            hexa, null,
                            null);

                        outLines.Add(outLine);
                        pc += (ushort)(hexa.Length / 2);
                    }
                    else
                    {
                        Debug.WriteLine($"DATA NON VALIDE ! - {line}");
                        return new AssembleResult(i, "DATA NON VALIDE !", line);
                    }
                }
                else if (upperLine.StartsWith("DEFB "))
                {
                    // Constante de type BYTE
                    // Format = DEFB NAME V8
                    // DEFB TOTO 15
                    // DEFB TOTO $34
                    // DEFB TOTO "A"
                    // DEFB TOTO %11001101

                    var items = line.Split(' ');

                    if (items.Length == 3 && !consts.ContainsKey(items[1]))
                    {
                        var item = items[2];

                        if (item.StartsWith("$"))
                        {
                            // Valeur HEXA

                            if (byte.TryParse(item.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out var v))
                            {
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X2")));
                                continue;
                            }
                        }
                        else if (item.StartsWith("%"))
                        {
                            // Valeur BINAIRE sur 8 BITS max

                            try
                            {
                                var v = Convert.ToByte(item.Substring(1), 2);
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X2")));
                                continue;
                            }
                            catch (Exception ex)
                            {
                                // Erreur de conversion !
                            }
                        }
                        else if (item.StartsWith("\""))
                        {
                            // CARACTERE -> Pattern = "X"

                            if (item[2] == '"')
                            {
                                var v = (byte)item[1];
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X2")));
                                continue;
                            }
                            else
                            {
                                // Erreur de pattern
                            }
                        }
                        else
                        {
                            // Valeur DECIMALE sur 8 BITS max

                            if (byte.TryParse(item, out var v))
                            {
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X2")));
                                continue;
                            }
                        }
                    }

                    // Erreur !!!

                    Debug.WriteLine($"DEFB NON VALIDE ! - {line}");
                    return new AssembleResult(i, "DEFB NON VALIDE !", line);
                }
                else if (upperLine.StartsWith("DEFW "))
                {
                    // Constante de type WORD
                    // Format = DEFW NAME V16
                    // DEFW TOTO 15
                    // DEFW TOTO $3456
                    // DEFW TOTO %1110011011

                    var items = line.Split(' ');

                    if (items.Length == 3 && !consts.ContainsKey(items[1]))
                    {
                        var item = items[2];

                        if (item.StartsWith("$"))
                        {
                            // Valeur HEXA

                            if (ushort.TryParse(item.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out var v))
                            {
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X4")));
                                continue;
                            }
                        }
                        else if (item.StartsWith("%"))
                        {
                            // Valeur BINAIRE sur 16 BITS max

                            try
                            {
                                var v = Convert.ToUInt16(item.Substring(1), 2);
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X4")));
                                continue;
                            }
                            catch (Exception ex)
                            {
                                // Erreur de conversion !
                            }
                        }
                        else
                        {
                            // Valeur DECIMALE

                            if (ushort.TryParse(item, out var v))
                            {
                                consts.Add(items[1], new ConstDefinition(items[1], "$" + v.ToString("X4")));
                                continue;
                            }
                        }
                    }

                    // Erreur !!!

                    Debug.WriteLine($"DEFW NON VALIDE ! - {line}");
                    return new AssembleResult(i, "DEFW NON VALIDE !", line);

                }
                else if (upperLine.StartsWith("ORG "))
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
                            return new AssembleResult(i, "ORG NON VALIDE !", line);
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
                            return new AssembleResult(i, "ORG NON VALIDE !", line);
                        }
                    }

                    var outLine = new OutLine(
                                pc,
                                i,
                                source,
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

                            Debug.WriteLine($"CONSTANTE INTROUVABLE ! - {line}");
                            return new AssembleResult(i, "CONSTANTE INTROUVABLE !", line);
                        }
                    }

                    // On recherche l'opération correspondante

                    var operation = SearchOperation(upperLine);

                    if (operation == null)
                    {
                        // OPERATION INTROUVABLE !!!

                        Debug.WriteLine($"ERREUR - OPERATION INTROUVABLE - {line} !");
                        return new AssembleResult(i, "OPERATION INTROUVABLE !", line);
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
                                int end = line.ToUpper().LastIndexOf(operation.End);
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

                                if (operation.Parameter == "H1H2")
                                {
                                    // On attend une valeur hexa sur 16 bits

                                    if (ushort.TryParse(pValue.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out var v16))
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
                                        return new AssembleResult(i, "ERREUR CONVERSION HEXA 16 BITS !", line);
                                    }
                                }
                                else if (operation.Parameter == "V0" || operation.Parameter == "VP" || operation.Parameter == "VN")
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
                                        return new AssembleResult(i, "ERREUR CONVERSION HEXA 8 BITS !", line);
                                    }
                                }
                            }
                            else if (pValue.StartsWith("\""))
                            {
                                // Valeur en caractère (1 seul caractère admis) à convertir en byte
                                // "A" --> 65
                                // Le patern est donc obligatoirement "X"
                                // N'est valable qu'avec un paramètre de type V0

                                if (operation.Parameter == "V0" || operation.Parameter == "VP" || operation.Parameter == "VN")
                                {
                                    if (pValue[2] == '"')
                                    {
                                        byte v8 = (byte)pValue[1];
                                        pValue = v8.ToString("X2");
                                        var newLine = line.Substring(0, pValueStart) + pValue;
                                        if (pValueEnd > 0) newLine += line.Substring(pValueEnd);
                                        line = newLine;
                                    }
                                    else
                                    {
                                        // Mauvais format !

                                        Debug.WriteLine($"ERREUR - ERREUR CONVERSION CARACTERE VERS HEXA 8 BITS - {line} !");
                                        return new AssembleResult(i, "ERREUR CONVERSION CARACTERE VERS HEXA 8 BITS !", line);
                                    }
                                }
                                else
                                {
                                    // Pas bon !

                                    Debug.WriteLine($"ERREUR - ERREUR CONVERSION CARACTERE NON ATTENDUE - {line} !");
                                    return new AssembleResult(i, "ERREUR CONVERSION CARACTERE NON ATTENDUE !", line);
                                }
                            }
                            else if (pValue.StartsWith("%"))
                            {
                                // Valeur en binaire
                                // %110011 ou %111001111 
                                // max 16 bits

                                UInt16 v16 = 0;

                                try
                                {
                                    v16 = Convert.ToUInt16(pValue.Substring(1), 2);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"ERREUR - ERREUR CONVERSION BINAIRE ! - {line} !");
                                    return new AssembleResult(i, "ERREUR CONVERSION BINAIRE !", line);
                                }

                                // Valeur en décimal à convertir en X2 ou X4 suivant le type de paramètre attendu
                                // H1H2 ou V0

                                if (operation.Parameter == "H1H2")
                                {
                                    // On attend une valeur hexa sur 16 bits

                                    pValue = v16.ToString("X4");
                                    var newLine = line.Substring(0, pValueStart) + pValue;
                                    if (pValueEnd > 0) newLine += line.Substring(pValueEnd);
                                    line = newLine;
                                }
                                else if (operation.Parameter == "V0" || operation.Parameter == "VP" || operation.Parameter == "VN")
                                {
                                    // On attend une valeur hexa sur 8 bits

                                    if (v16 < 256)
                                    {
                                        pValue = v16.ToString("X2");
                                        var newLine = line.Substring(0, pValueStart) + pValue;
                                        if (pValueEnd > 0) newLine += line.Substring(pValueEnd);
                                        line = newLine;
                                    }
                                    else
                                    {
                                        // Valeur trop grande !

                                        Debug.WriteLine($"ERREUR - VALEUR TROP GRANDE ! - {line} !");
                                        return new AssembleResult(i, "ERREUR VALEUR TROP GRANDE !", line);
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
                                        return new AssembleResult(i, "ERREUR CONVERSION DECIMAL VERS HEXA 16 BITS !", line);
                                    }
                                }
                                else if (operation.Parameter == "V0" || operation.Parameter == "VP" || operation.Parameter == "VN")
                                {
                                    // On attend une valeur numérique sur 8 bits non signé

                                    if (byte.TryParse(pValue, out var v))
                                    {
                                        pValue = v.ToString("X2");
                                        var newLine = line.Substring(0, pValueStart) + pValue;
                                        if (pValueEnd > 0) newLine += line.Substring(pValueEnd);
                                        line = newLine;
                                    }
                                    else
                                    {
                                        // Conversion impossible !

                                        Debug.WriteLine($"ERREUR - ERREUR CONVERSION DECIMAL VERS HEXA 8 BITS - {line} !");
                                        return new AssembleResult(i, "ERREUR CONVERSION DECIMAL VERS HEXA 8 BITS !", line);
                                    }
                                }
                            }

                            var outLine = new OutLine(
                                pc,
                                i,
                                source,
                                line.ToUpper(),
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
                                i,
                                source,
                                line.ToUpper(),
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
                        // Soit H1H2 ---> valeur hexa sur 16 bits nono signé
                        // Soit V0   ---> valeur hexa sur 8 bits non signé
                        // Soit VP ou VN ---> valeur hexa signé à convertir en hexa 8 bit non signé
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
                                return new AssembleResult(item.CodeLineNumber, "LABEL INTROUVABLE !", line);
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
                                return new AssembleResult(item.CodeLineNumber, "PARTIE VARIABLE INCORRECTE !", line);
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
                                        i,
                                        item.Source,
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
                                    return new AssembleResult(item.CodeLineNumber, "PARTIE VARIABLE INCORRECTE !", line);
                                }
                            }
                        }
                        else if (operation.Parameter == "V0")
                        {
                            // On extrait la partie paramètre de line

                            var p = line.Substring(operation.Start.Length, 2);

                            // On essaye de la convertir en hexa 8 bits

                            if (byte.TryParse(p, System.Globalization.NumberStyles.HexNumber, null, out var v))
                            {
                                // On peut composer la séquence finale hexa

                                var hexa = operation.Hexa.Replace("V0", p);

                                var outLine = new OutLine(
                                    item.Address,
                                    i,
                                    item.Source,
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
                                return new AssembleResult(item.CodeLineNumber, "PARTIE VARIABLE INCORRECTE !", line);
                            }
                        }
                        else if (operation.Parameter == "VP")
                        {
                            // On extrait la partie paramètre de line

                            var p = line.Substring(operation.Start.Length, 2);

                            // On essaye de la convertir en hexa 8 bits signé
                            // Le résultat DOIT être positif (VP et VN sont définis en valeur absolue)

                            if (sbyte.TryParse(p, System.Globalization.NumberStyles.HexNumber, null, out var v8) && v8 >= 0)
                            {
                                // On peut composer la séquence finale hexa

                                var hexa = operation.Hexa.Replace("VP", p);

                                var outLine = new OutLine(
                                    item.Address,
                                    i,
                                    item.Source,
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
                                return new AssembleResult(item.CodeLineNumber, "PARTIE VARIABLE INCORRECTE !", line);
                            }
                        }
                        else if (operation.Parameter == "VN")
                        {
                            // On extrait la partie paramètre de line

                            var p = line.Substring(operation.Start.Length, 2);

                            // On essaye de la convertir en hexa 8 bits signé
                            // Le résultat DOIT être positif (VP et VN sont définis en valeur absolue)

                            if (sbyte.TryParse(p, System.Globalization.NumberStyles.HexNumber, null, out var v8) && v8 >= 0)
                            {
                                // On doit placer dans la séquence hexa la valeur négative de v8 

                                var h = $"{(sbyte)-v8:X2}";

                                // On peut composer la séquence finale hexa

                                var hexa = operation.Hexa.Replace("VN", h);

                                var outLine = new OutLine(
                                    item.Address,
                                    i,
                                    item.Source,
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
                                return new AssembleResult(item.CodeLineNumber, "PARTIE VARIABLE INCORRECTE !", line);
                            }
                        }
                        else
                        {
                            // Paramètre inconnu !!!

                            Debug.WriteLine("ERREUR - PARTIE VARIABLE INCONNUE !");
                            return new AssembleResult(item.CodeLineNumber, "PARTIE VARIABLE INCONNUE !", line);
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

            return new AssembleResult(outLines);
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

                    sb.AppendLine($"{numLine} DATA \"ORG\",{line.Address.ToString("X4")}");
                    numLine += 10;
                }
                else
                {
                    for (int j = 0; j < line.Hexa.Length; j += 2)
                    {
                        if (currentDataLine.Length > 0) currentDataLine.Append(",");
                        currentDataLine.Append(line.Hexa.Substring(j, 2));
                    }

                    if (currentDataLine.Length > 20)
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

            sb.AppendLine($"{numLine} DATA \"END\"");

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
            
            AddDefinition("ADC A,(IX+VP)", "DD8EVP");
            AddDefinition("ADC A,(IY+VP)", "FD8EVP");

            AddDefinition("ADC A,(IX-VN)", "DD8EVN");
            AddDefinition("ADC A,(IY-VN)", "FD8EVN");

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

            AddDefinition("ADD A,(IX+VP)", "DD86VP");
            AddDefinition("ADD A,(IY+VP)", "FD86VP");

            AddDefinition("ADD A,(IX-VN)", "DD86VN");
            AddDefinition("ADD A,(IY-VN)", "FD86VN");

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

            AddDefinition("AND (IX+VP)", "DDA6VP");
            AddDefinition("AND (IY+VP)", "FDA6VP");

            AddDefinition("AND (IX-VN)", "DDA6VN");
            AddDefinition("AND (IY-VN)", "FDA6VN");

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

            AddDefinition("BIT 0,(IX+VP)", "DDCBVP46");
            AddDefinition("BIT 0,(IY+VP)", "FDCBVP46");

            AddDefinition("BIT 0,(IX-VN)", "DDCBVN46");
            AddDefinition("BIT 0,(IY-VN)", "FDCBVN46");

            AddDefinition("BIT 0,A", "CB47");
            AddDefinition("BIT 0,B", "CB40");
            AddDefinition("BIT 0,C", "CB41");
            AddDefinition("BIT 0,D", "CB42");
            AddDefinition("BIT 0,E", "CB43");
            AddDefinition("BIT 0,H", "CB44");
            AddDefinition("BIT 0,L", "CB45");

            AddDefinition("BIT 1,(HL)", "CB4E");

            AddDefinition("BIT 1,(IX+VP)", "DDCBVP4E");
            AddDefinition("BIT 1,(IY+VP)", "FDCBVP4E");

            AddDefinition("BIT 1,(IX-VN)", "DDCBVN4E");
            AddDefinition("BIT 1,(IY-VN)", "FDCBVN4E");

            AddDefinition("BIT 1,A", "CB4F");
            AddDefinition("BIT 1,B", "CB48");
            AddDefinition("BIT 1,C", "CB49");
            AddDefinition("BIT 1,D", "CB4A");
            AddDefinition("BIT 1,E", "CB4B");
            AddDefinition("BIT 1,H", "CB4C");
            AddDefinition("BIT 1,L", "CB4D");

            AddDefinition("BIT 2,(HL)", "CB56");

            AddDefinition("BIT 2,(IX+VP)", "DDCBVP56");
            AddDefinition("BIT 2,(IY+VP)", "FDCBVP56");

            AddDefinition("BIT 2,(IX-VN)", "DDCBVN56");
            AddDefinition("BIT 2,(IY-VN)", "FDCBVN56");

            AddDefinition("BIT 2,A", "CB57");
            AddDefinition("BIT 2,B", "CB50");
            AddDefinition("BIT 2,C", "CB51");
            AddDefinition("BIT 2,D", "CB52");
            AddDefinition("BIT 2,E", "CB53");
            AddDefinition("BIT 2,H", "CB54");
            AddDefinition("BIT 2,L", "CB55");

            AddDefinition("BIT 3,(HL)", "CB5E");

            AddDefinition("BIT 3,(IX+VP)", "DDCBVP5E");
            AddDefinition("BIT 3,(IY+VP)", "FDCBVP5E");

            AddDefinition("BIT 3,(IX-VN)", "DDCBVN5E");
            AddDefinition("BIT 3,(IY-VN)", "FDCBVN5E");

            AddDefinition("BIT 3,A", "CB5F");
            AddDefinition("BIT 3,B", "CB58");
            AddDefinition("BIT 3,C", "CB59");
            AddDefinition("BIT 3,D", "CB5A");
            AddDefinition("BIT 3,E", "CB5B");
            AddDefinition("BIT 3,H", "CB5C");
            AddDefinition("BIT 3,L", "CB5D");

            AddDefinition("BIT 4,(HL)", "CB66");

            AddDefinition("BIT 4,(IX+VP)", "DDCBVP66");
            AddDefinition("BIT 4,(IY+VP)", "FDCBVP66");

            AddDefinition("BIT 4,(IX-VN)", "DDCBVN66");
            AddDefinition("BIT 4,(IY-VN)", "FDCBVN66");

            AddDefinition("BIT 4,A", "CB67");
            AddDefinition("BIT 4,B", "CB60");
            AddDefinition("BIT 4,C", "CB61");
            AddDefinition("BIT 4,D", "CB62");
            AddDefinition("BIT 4,E", "CB63");
            AddDefinition("BIT 4,H", "CB64");
            AddDefinition("BIT 4,L", "CB65");

            AddDefinition("BIT 5,(HL)", "CB6E");

            AddDefinition("BIT 5,(IX+VP)", "DDCBVP6E");
            AddDefinition("BIT 5,(IY+VP)", "FDCBVP6E");

            AddDefinition("BIT 5,(IX-VN)", "DDCBVN6E");
            AddDefinition("BIT 5,(IY-VN)", "FDCBVN6E");

            AddDefinition("BIT 5,A", "CB6F");
            AddDefinition("BIT 5,B", "CB68");
            AddDefinition("BIT 5,C", "CB69");
            AddDefinition("BIT 5,D", "CB6A");
            AddDefinition("BIT 5,E", "CB6B");
            AddDefinition("BIT 5,H", "CB6C");
            AddDefinition("BIT 5,L", "CB6D");

            AddDefinition("BIT 6,(HL)", "CB76");

            AddDefinition("BIT 6,(IX+VP)", "DDCBVP76");
            AddDefinition("BIT 6,(IY+VP)", "FDCBVP76");

            AddDefinition("BIT 6,(IX-VN)", "DDCBVN76");
            AddDefinition("BIT 6,(IY-VN)", "FDCBVN76");

            AddDefinition("BIT 6,A", "CB77");
            AddDefinition("BIT 6,B", "CB70");
            AddDefinition("BIT 6,C", "CB71");
            AddDefinition("BIT 6,D", "CB72");
            AddDefinition("BIT 6,E", "CB73");
            AddDefinition("BIT 6,H", "CB74");
            AddDefinition("BIT 6,L", "CB75");

            AddDefinition("BIT 7,(HL)", "CB7E");

            AddDefinition("BIT 7,(IX+VP)", "DDCBVP7E");
            AddDefinition("BIT 7,(IY+VP)", "FDCBVP7E");

            AddDefinition("BIT 7,(IX-VN)", "DDCBVN7E");
            AddDefinition("BIT 7,(IY-VN)", "FDCBVN7E");

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

            AddDefinition("CP (IX+VP)", "DDBEVP");
            AddDefinition("CP (IY+VP)", "FDBEVP");

            AddDefinition("CP (IX-VN)", "DDBEVN");
            AddDefinition("CP (IY-VN)", "FDBEVN");

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

            AddDefinition("DEC (IX+VP)", "DD35VP");
            AddDefinition("DEC (IY+VP)", "FD35VP");

            AddDefinition("DEC (IX-VN)", "DD35VN");
            AddDefinition("DEC (IY-VN)", "FD35VN");

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

            AddDefinition("INC (IX+VP)", "DD34VP");
            AddDefinition("INC (IY+VP)", "FD34VP");

            AddDefinition("INC (IX-VN)", "DD34VN");
            AddDefinition("INC (IY-VN)", "FD34VN");

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

            AddDefinition("LD (IX+VP),A", "DD77VP");
            AddDefinition("LD (IX+VP),B", "DD70VP");
            AddDefinition("LD (IX+VP),C", "DD71VP");
            AddDefinition("LD (IX+VP),D", "DD72VP");
            AddDefinition("LD (IX+VP),E", "DD73VP");
            AddDefinition("LD (IX+VP),H", "DD74VP");
            AddDefinition("LD (IX+VP),L", "DD75VP");
            AddDefinition("LD (IX+VP),V0", "DD36VPV0");

            AddDefinition("LD (IX-VN),A", "DD77VN");
            AddDefinition("LD (IX-VN),B", "DD70VN");
            AddDefinition("LD (IX-VN),C", "DD71VN");
            AddDefinition("LD (IX-VN),D", "DD72VN");
            AddDefinition("LD (IX-VN),E", "DD73VN");
            AddDefinition("LD (IX-VN),H", "DD74VN");
            AddDefinition("LD (IX-VN),L", "DD75VN");
            AddDefinition("LD (IX-VN),V0", "DD36VNV0");

            AddDefinition("LD (IY+VP),A", "FD77VP");
            AddDefinition("LD (IY+VP),B", "FD70VP");
            AddDefinition("LD (IY+VP),C", "FD71VP");
            AddDefinition("LD (IY+VP),D", "FD72VP");
            AddDefinition("LD (IY+VP),E", "FD73VP");
            AddDefinition("LD (IY+VP),H", "FD74VP");
            AddDefinition("LD (IY+VP),L", "FD75VP");
            AddDefinition("LD (IY+VP),V0", "FD36VPV0");

            AddDefinition("LD (IY-VN),A", "FD77VN");
            AddDefinition("LD (IY-VN),B", "FD70VN");
            AddDefinition("LD (IY-VN),C", "FD71VN");
            AddDefinition("LD (IY-VN),D", "FD72VN");
            AddDefinition("LD (IY-VN),E", "FD73VN");
            AddDefinition("LD (IY-VN),H", "FD74VN");
            AddDefinition("LD (IY-VN),L", "FD75VN");
            AddDefinition("LD (IY-VN),V0", "FD36VNV0");

            AddDefinition("LD (H1H2),A", "32H2H1");
            AddDefinition("LD (H1H2),BC", "ED43H2H1");
            AddDefinition("LD (H1H2),DE", "ED53H2H1");
            AddDefinition("LD (H1H2),HL", "22H2H1");

            AddDefinition("LD (H1H2), IX", "DD22H2H1");
            AddDefinition("LD (H1H2), IY", "FD22H2H1");
            AddDefinition("LD (H1H2), SP", "ED22H2H1");

            AddDefinition("LD A,(BC)", "0A");
            AddDefinition("LD A,(DE)", "1A");
            AddDefinition("LD A,(HL)", "7E");

            AddDefinition("LD A,(IX+VP)", "DD7EVP");
            AddDefinition("LD A,(IY+VP)", "FD7EVP");

            AddDefinition("LD A,(IX-VN)", "DD7EVN");
            AddDefinition("LD A,(IY-VN)", "FD7EVN");

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

            AddDefinition("LD B,(IX+VP)", "DD46VP");
            AddDefinition("LD B,(IY+VP)", "FD46VP");

            AddDefinition("LD B,(IX-VN)", "DD46VN");
            AddDefinition("LD B,(IY-VN)", "FD46VN");

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

            AddDefinition("LD C,(IX+VP)", "DD4EVP");
            AddDefinition("LD C,(IY+VP)", "FD4EVP");

            AddDefinition("LD C,(IX-VN)", "DD4EVN");
            AddDefinition("LD C,(IY-VN)", "FD4EVN");

            AddDefinition("LD C,A", "4F");
            AddDefinition("LD C,B", "48");
            AddDefinition("LD C,C", "49");
            AddDefinition("LD C,D", "4A");
            AddDefinition("LD C,E", "4B");
            AddDefinition("LD C,H", "4C");
            AddDefinition("LD C,L", "4D");
            AddDefinition("LD C,V0", "0EV0");

            AddDefinition("LD D,(HL)", "56");

            AddDefinition("LD D,(IX+VP)", "DD56VP");
            AddDefinition("LD D,(IY+VP)", "FD56VP");

            AddDefinition("LD D,(IX-VN)", "DD56VN");
            AddDefinition("LD D,(IY-VN)", "FD56VN");

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

            AddDefinition("LD E,(IX+VP)", "DD5EVP");
            AddDefinition("LD E,(IY+VP)", "FD5EVP");

            AddDefinition("LD E,(IX-VN)", "DD5EVN");
            AddDefinition("LD E,(IY-VN)", "FD5EVN");

            AddDefinition("LD E,A", "5F");
            AddDefinition("LD E,B", "58");
            AddDefinition("LD E,C", "59");
            AddDefinition("LD E,D", "5A");
            AddDefinition("LD E,E", "5B");
            AddDefinition("LD E,H", "5C");
            AddDefinition("LD E,L", "5D");
            AddDefinition("LD E,V0", "1EV0");

            AddDefinition("LD H,(HL)", "66");

            AddDefinition("LD H,(IX+VP)", "DD66VP");
            AddDefinition("LD H,(IY+VP)", "FD66VP");

            AddDefinition("LD H,(IX-VN)", "DD66VN");
            AddDefinition("LD H,(IY-VN)", "FD66VN");

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

            AddDefinition("LD L,(IX+VP)", "DD6EVP");
            AddDefinition("LD L,(IY+VP)", "FD6EVP");

            AddDefinition("LD L,(IX-VN)", "DD6EVN");
            AddDefinition("LD L,(IY-VN)", "FD6EVN");

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

            AddDefinition("OR (IX+VP)", "DDB6VP");
            AddDefinition("OR (IY+VP)", "FDB6VP");

            AddDefinition("OR (IX-VN)", "DDB6VN");
            AddDefinition("OR (IY-VN)", "FDB6VN");

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

            AddDefinition("RES 0,(IX+VP)", "DDCBVP86");
            AddDefinition("RES 0,(IY+VP)", "FDCBVP86");

            AddDefinition("RES 0,(IX-VN)", "DDCBVN86");
            AddDefinition("RES 0,(IY-VN)", "FDCBVN86");

            AddDefinition("RES 0,A", "CB87");
            AddDefinition("RES 0,B", "CB80");
            AddDefinition("RES 0,C", "CB81");
            AddDefinition("RES 0,D", "CB82");
            AddDefinition("RES 0,E", "CB83");
            AddDefinition("RES 0,H", "CB84");
            AddDefinition("RES 0,L", "CB85");

            AddDefinition("RES 1,(HL)", "CB8E");

            AddDefinition("RES 1,(IX+VP)", "DDCBVP8E");
            AddDefinition("RES 1,(IY+VP)", "FDCBVP8E");

            AddDefinition("RES 1,(IX-VN)", "DDCBVN8E");
            AddDefinition("RES 1,(IY-VN)", "FDCBVN8E");

            AddDefinition("RES 1,A", "CB8F");
            AddDefinition("RES 1,B", "CB88");
            AddDefinition("RES 1,C", "CB89");
            AddDefinition("RES 1,D", "CB8A");
            AddDefinition("RES 1,E", "CB8B");
            AddDefinition("RES 1,H", "CB8C");
            AddDefinition("RES 1,L", "CB8D");

            AddDefinition("RES 2,(HL)", "CB96");

            AddDefinition("RES 2,(IX+VP)", "DDCBVP96");
            AddDefinition("RES 2,(IY+VP)", "FDCBVP96");

            AddDefinition("RES 2,(IX-VN)", "DDCBVN96");
            AddDefinition("RES 2,(IY-VN)", "FDCBVN96");

            AddDefinition("RES 2,A", "CB97");
            AddDefinition("RES 2,B", "CB90");
            AddDefinition("RES 2,C", "CB91");
            AddDefinition("RES 2,D", "CB92");
            AddDefinition("RES 2,E", "CB93");
            AddDefinition("RES 2,H", "CB94");
            AddDefinition("RES 2,L", "CB95");

            AddDefinition("RES 3,(HL)", "CB9E");

            AddDefinition("RES 3,(IX+VP)", "DDCBVP9E");
            AddDefinition("RES 3,(IY+VP)", "FDCBVP9E");

            AddDefinition("RES 3,(IX-VN)", "DDCBVN9E");
            AddDefinition("RES 3,(IY-VN)", "FDCBVN9E");

            AddDefinition("RES 3,A", "CB9F");
            AddDefinition("RES 3,B", "CB98");
            AddDefinition("RES 3,C", "CB99");
            AddDefinition("RES 3,D", "CB9A");
            AddDefinition("RES 3,E", "CB9B");
            AddDefinition("RES 3,H", "CB9C");
            AddDefinition("RES 3,L", "CB9D");

            AddDefinition("RES 4,(HL)", "CBA6");

            AddDefinition("RES 4,(IX+VP)", "DDCBVPA6");
            AddDefinition("RES 4,(IY+VP)", "FDCBVPA6");

            AddDefinition("RES 4,(IX-VN)", "DDCBVNA6");
            AddDefinition("RES 4,(IY-VN)", "FDCBVNA6");

            AddDefinition("RES 4,A", "CBA7");
            AddDefinition("RES 4,B", "CBA0");
            AddDefinition("RES 4,C", "CBA1");
            AddDefinition("RES 4,D", "CBA2");
            AddDefinition("RES 4,E", "CBA3");
            AddDefinition("RES 4,H", "CBA4");
            AddDefinition("RES 4,L", "CBA5");

            AddDefinition("RES 5,(HL)", "CBAE");

            AddDefinition("RES 5,(IX+VP)", "DDCBVPAE");
            AddDefinition("RES 5,(IY+VP)", "FDCBVPAE");

            AddDefinition("RES 5,(IX-VN)", "DDCBVNAE");
            AddDefinition("RES 5,(IY-VN)", "FDCBVNAE");

            AddDefinition("RES 5,A", "CBAF");
            AddDefinition("RES 5,B", "CBA8");
            AddDefinition("RES 5,C", "CBA9");
            AddDefinition("RES 5,D", "CBAA");
            AddDefinition("RES 5,E", "CBAB");
            AddDefinition("RES 5,H", "CBAC");
            AddDefinition("RES 5,L", "CBAD");

            AddDefinition("RES 6,(HL)", "CBB6");

            AddDefinition("RES 6,(IX+VP)", "DDCBVPB6");
            AddDefinition("RES 6,(IY+VP)", "FDCBVPB6");

            AddDefinition("RES 6,(IX-VN)", "DDCBVNB6");
            AddDefinition("RES 6,(IY-VN)", "FDCBVNB6");

            AddDefinition("RES 6,A", "CBB7");
            AddDefinition("RES 6,B", "CBB0");
            AddDefinition("RES 6,C", "CBB1");
            AddDefinition("RES 6,D", "CBB2");
            AddDefinition("RES 6,E", "CBB3");
            AddDefinition("RES 6,H", "CBB4");
            AddDefinition("RES 6,L", "CBB5");

            AddDefinition("RES 7,(HL)", "CBBE");

            AddDefinition("RES 7,(IX+VP)", "DDCBVPBE");
            AddDefinition("RES 7,(IY+VP)", "FDCBVPBE");

            AddDefinition("RES 7,(IX-VN)", "DDCBVNBE");
            AddDefinition("RES 7,(IY-VN)", "FDCBVNBE");

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

            AddDefinition("RL (IX+VP)", "DDCBVP16");
            AddDefinition("RL (IY+VP)", "FDCBVP16");

            AddDefinition("RL (IX-VN)", "DDCBVN16");
            AddDefinition("RL (IY-VN)", "FDCBVN16");

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

            AddDefinition("RLC (IX+VP)", "DDCBVP06");
            AddDefinition("RLC (IY+VP)", "FDCBVP06");

            AddDefinition("RLC (IX-VN)", "DDCBVN06");
            AddDefinition("RLC (IY-VN)", "FDCBVN06");

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

            AddDefinition("RR (IX+VP)", "DDCBVP1E");
            AddDefinition("RR (IY+VP)", "FDCBVP1E");

            AddDefinition("RR (IX-VN)", "DDCBVN1E");
            AddDefinition("RR (IY-VN)", "FDCBVN1E");

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

            AddDefinition("RRC (IX+VP)", "DDCBVP0E");
            AddDefinition("RRC (IY+VP)", "FDCBVP0E");

            AddDefinition("RRC (IX-VN)", "DDCBVN0E");
            AddDefinition("RRC (IY-VN)", "FDCBVN0E");

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

            AddDefinition("SBC A,(IX+VP)", "DD9EVP");
            AddDefinition("SBC A,(IY+VP)", "FD9EVP");

            AddDefinition("SBC A,(IX-VN)", "DD9EVN");
            AddDefinition("SBC A,(IY-VN)", "FD9EVN");

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

            AddDefinition("SET 0,(IX+VP)", "DDCBVPC6");
            AddDefinition("SET 0,(IY+VP)", "FDCBVPC6");

            AddDefinition("SET 0,(IX-VN)", "DDCBVNC6");
            AddDefinition("SET 0,(IY-VN)", "FDCBVNC6");

            AddDefinition("SET 0,A", "CBC7");
            AddDefinition("SET 0,B", "CBC0");
            AddDefinition("SET 0,C", "CBC1");
            AddDefinition("SET 0,D", "CBC2");
            AddDefinition("SET 0,E", "CBC3");
            AddDefinition("SET 0,H", "CBC4");
            AddDefinition("SET 0,L", "CBC5");

            AddDefinition("SET 1,(HL)", "CBCE");

            AddDefinition("SET 1,(IX+VP)", "DDCBVPCE");
            AddDefinition("SET 1,(IY+VP)", "FDCBVPCE");

            AddDefinition("SET 1,(IX-VN)", "DDCBVNCE");
            AddDefinition("SET 1,(IY-VN)", "FDCBVNCE");

            AddDefinition("SET 1,A", "CBCF");
            AddDefinition("SET 1,B", "CBC8");
            AddDefinition("SET 1,C", "CBC9");
            AddDefinition("SET 1,D", "CBCA");
            AddDefinition("SET 1,E", "CBCB");
            AddDefinition("SET 1,H", "CBCC");
            AddDefinition("SET 1,L", "CBCD");

            AddDefinition("SET 2,(HL)", "CBD6");

            AddDefinition("SET 2,(IX+VP)", "DDCBVPD6");
            AddDefinition("SET 2,(IY+VP)", "FDCBVPD6");

            AddDefinition("SET 2,(IX-VN)", "DDCBVND6");
            AddDefinition("SET 2,(IY-VN)", "FDCBVND6");

            AddDefinition("SET 2,A", "CBD7");
            AddDefinition("SET 2,B", "CBD0");
            AddDefinition("SET 2,C", "CBD1");
            AddDefinition("SET 2,D", "CBD2");
            AddDefinition("SET 2,E", "CBD3");
            AddDefinition("SET 2,H", "CBD4");
            AddDefinition("SET 2,L", "CBD5");

            AddDefinition("SET 3,(HL)", "CBDE");

            AddDefinition("SET 3,(IX+VP)", "DDCBVPDE");
            AddDefinition("SET 3,(IY+VP)", "FDCBVPDE");

            AddDefinition("SET 3,(IX-VN)", "DDCBVNDE");
            AddDefinition("SET 3,(IY-VN)", "FDCBVNDE");

            AddDefinition("SET 3,A", "CBDF");
            AddDefinition("SET 3,B", "CBD8");
            AddDefinition("SET 3,C", "CBD9");
            AddDefinition("SET 3,D", "CBDA");
            AddDefinition("SET 3,E", "CBDB");
            AddDefinition("SET 3,H", "CBDC");
            AddDefinition("SET 3,L", "CBDD");

            AddDefinition("SET 4,(HL)", "CBE6");

            AddDefinition("SET 4,(IX+VP)", "DDCBVPE6");
            AddDefinition("SET 4,(IY+VP)", "FDCBVPE6");

            AddDefinition("SET 4,(IX-VN)", "DDCBVNE6");
            AddDefinition("SET 4,(IY-VN)", "FDCBVNE6");

            AddDefinition("SET 4,A", "CBE7");
            AddDefinition("SET 4,B", "CBE0");
            AddDefinition("SET 4,C", "CBE1");
            AddDefinition("SET 4,D", "CBE2");
            AddDefinition("SET 4,E", "CBE3");
            AddDefinition("SET 4,H", "CBE4");
            AddDefinition("SET 4,L", "CBE5");

            AddDefinition("SET 5,(HL)", "CBEE");

            AddDefinition("SET 5,(IX+VP)", "DDCBVPEE");
            AddDefinition("SET 5,(IY+VP)", "FDCBVPEE");

            AddDefinition("SET 5,(IX-VN)", "DDCBVNEE");
            AddDefinition("SET 5,(IY-VN)", "FDCBVNEE");

            AddDefinition("SET 5,A", "CBEF");
            AddDefinition("SET 5,B", "CBE8");
            AddDefinition("SET 5,C", "CBE9");
            AddDefinition("SET 5,D", "CBEA");
            AddDefinition("SET 5,E", "CBEB");
            AddDefinition("SET 5,H", "CBEC");
            AddDefinition("SET 5,L", "CBED");

            AddDefinition("SET 6,(HL)", "CBF6");

            AddDefinition("SET 6,(IX+VP)", "DDCBVPF6");
            AddDefinition("SET 6,(IY+VP)", "FDCBVPF6");

            AddDefinition("SET 6,(IX-VN)", "DDCBVNF6");
            AddDefinition("SET 6,(IY-VN)", "FDCBVNF6");

            AddDefinition("SET 6,A", "CBF7");
            AddDefinition("SET 6,B", "CBF0");
            AddDefinition("SET 6,C", "CBF1");
            AddDefinition("SET 6,D", "CBF2");
            AddDefinition("SET 6,E", "CBF3");
            AddDefinition("SET 6,H", "CBF4");
            AddDefinition("SET 6,L", "CBF5");

            AddDefinition("SET 7,(HL)", "CBFE");

            AddDefinition("SET 7,(IX+VP)", "DDCBVPFE");
            AddDefinition("SET 7,(IY+VP)", "FDCBVPFE");

            AddDefinition("SET 7,(IX-VN)", "DDCBVNFE");
            AddDefinition("SET 7,(IY-VN)", "FDCBVNFE");

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

            AddDefinition("SRA (IX+VP)", "DDCBVP2E");
            AddDefinition("SRA (IY+VP)", "FDCBVP2E");

            AddDefinition("SRA (IX-VN)", "DDCBVN2E");
            AddDefinition("SRA (IY-VN)", "FDCBVN2E");

            AddDefinition("SRA A", "CB2F");
            AddDefinition("SRA B", "CB28");
            AddDefinition("SRA C", "CB29");
            AddDefinition("SRA D", "CB2A");
            AddDefinition("SRA E", "CB2B");
            AddDefinition("SRA H", "CB2C");
            AddDefinition("SRA L", "CB2D");

            // SRL

            AddDefinition("SRL (HL)", "CB3E");

            AddDefinition("SRL (IX+VP)", "DDCBVP3E");
            AddDefinition("SRL (IY+VP)", "FDCBVP3E");

            AddDefinition("SRL (IX-VN)", "DDCBVN3E");
            AddDefinition("SRL (IY-VN)", "FDCBVN3E");

            AddDefinition("SRL A", "CB3F");
            AddDefinition("SRL B", "CB38");
            AddDefinition("SRL C", "CB39");
            AddDefinition("SRL D", "CB3A");
            AddDefinition("SRL E", "CB3B");
            AddDefinition("SRL H", "CB3C");
            AddDefinition("SRL L", "CB3D");

            // SUB

            AddDefinition("SUB (HL)", "96");

            AddDefinition("SUB (IX+VP)", "DD96VP");
            AddDefinition("SUB (IY+VP)", "FD96VP");

            AddDefinition("SUB (IX-VN)", "DD96VN");
            AddDefinition("SUB (IY-VN)", "FD96VN");

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

            AddDefinition("XOR (IX+VP)", "DDAEVP");
            AddDefinition("XOR (IY+VP)", "FDAEVP");

            AddDefinition("XOR (IX-VN)", "DDAEVN");
            AddDefinition("XOR (IY-VN)", "FDAEVN");

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

                // 4 paramètres possibles
                // V0 = UINT8
                // VP = INT8 
                // VN = INT8
                // H1H2 = UINT16
                
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
                    k = op.IndexOf("VP");

                    if (k > -1)
                    {
                        // VP

                        WithParameters = true;
                        Parameter = "VP";
                        Start = op.Substring(0, k);
                        if (k + 2 < op.Length) End = op.Substring(k + 2);
                    }
                    else
                    {
                        k = op.IndexOf("VN");

                        if (k > -1)
                        {
                            // VN

                            WithParameters = true;
                            Parameter = "VN";
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
            public int CodeLineNumber { get; set; }

            public string Source { get; set; }

            public UInt16 Address { get; set; }
            
            public string Code { get; set; }

            public string Hexa { get; set; }

            public OpDefinition? Operation { get; set; }

            public string? LabelName { get; set; }

            public OutLine(UInt16 address, int codeLineNumber, string source, string code, string hexa, OpDefinition? operation, string? labelName)
            {
                Address = address;
                CodeLineNumber = codeLineNumber;
                Source = source;    
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

        public enum AssembleResultStatusEnum
        {
            None,
            Success,
            Error
        }

        public class AssembleResult
        {
            
            public AssembleResultStatusEnum Status = AssembleResultStatusEnum.None;

            public int ErrorLine { get; set; }

            public string? ErrorCode { get; set; }

            public string? ErrorMessage { get; set; }

            public List<OutLine> Outlines { get; set; } = new();

            public AssembleResult(List<OutLine> outlines)
            {
                Status = AssembleResultStatusEnum.Success;
                Outlines = outlines;
            }

            public AssembleResult(int errorLine, string errorMessage, string errorCode)
            {
                Status = AssembleResultStatusEnum.Error;
                ErrorLine = errorLine;
                ErrorMessage = errorMessage;
                ErrorCode = errorCode;
            }
        }
    }
}
