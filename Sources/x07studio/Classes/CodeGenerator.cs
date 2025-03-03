using Microsoft.VisualBasic;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static x07studio.Classes.ASM80;
using static x07studio.Classes.CodeGenerator;

namespace x07studio.Classes
{
    internal class CodeGenerator
    {
        public static CodeGenerator Default { get; } = new();   

        public GeneratorResult Generate(string source)
        {
            Debug.WriteLine("");

            // On enlève les tabulations éventuelles

            source = source.Replace("\t", "");

            // On crée un tableau des lignes de code

            var lines = source.Split("\n").ToList();

            // On ajoute les lignes et on stocke tous les labels avec la ligne qui correspond

            var labels = new Dictionary<string, int>();
            var vars = new Dictionary<string, List<string>>();
            var defs = new Dictionary<string, Definition>();
            int numLine;
            List<ExportLine> exports = new();
            List<string> imports = new();
            List<string> types = ["$", "%", "!", "#", "@"];

            foreach (var t in types) vars[t] = new();

            numLine = 90;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                var lineNumber = i + 1;

                if (line.Length > 0)
                {
                    // Les lignes de plus de 160 caractères ne sont pas supportées

                    if (line.Length > 160)
                    {
                        return new GeneratorResult(ResultStatusEnum.LineTooLong, lineNumber, line);
                    }
                    if (line.StartsWith("//"))
                    { 
                        // Commentaire ignoré dans l'export
                    }
                    else if (line.StartsWith("@"))
                    {
                        // On est en face d'une ligne de déclaration d'un label

                        // On vérifie que le label est valide 

                        if (!IsLabelValid(line))
                        {
                            // Label invalide !

                            return new GeneratorResult(ResultStatusEnum.IllegalLabelDeclaration, lineNumber, line);
                        }

                        // On stocke le label

                        if (labels.ContainsKey(line))
                        {
                            // S'il est déjà présent c'est une définition multiple --> Erreur

                            return new GeneratorResult(ResultStatusEnum.DuplicateLabelDeclaration, lineNumber, line);
                        }

                        labels[line] = numLine + 10;
                        Debug.WriteLine($"LABEL {line} FOUND AT {numLine + 10}");
                    }
                    else if (line.StartsWith("#IMPORT "))
                    {
                        // On est en face d'une ligne d'import 
                        // #IMPORT FICHIER LIB

                        var lib = line.Substring(8);

                        if (imports.Contains(lib))
                        {
                            // Cette lib a déjà été importée

                            // return new GeneratorResult(ResultStatusEnum.LibraryAlreadyImported, lineNumber, line);
                        }
                        else
                        {
                            var file = Path.Combine(AppGlobal.LibrarysFolder, lib);

                            if (!File.Exists(file))
                            {
                                // Le fichier n'existe pas !

                                return new GeneratorResult(ResultStatusEnum.LibraryNotFound, lineNumber, line);
                            }

                            // On charge la lib

                            var plib = Project.Load(file);

                            if (plib == null)
                            {
                                // Impossible de charger la lib !

                                return new GeneratorResult(ResultStatusEnum.LibraryLoadError, lineNumber, line);
                            }

                            if (string.IsNullOrEmpty(plib.Code))
                            {
                                // lib vide !

                                return new GeneratorResult(ResultStatusEnum.LibraryIsEmpty, lineNumber, line);
                            }

                            // Si c'est la 1ère lib qu'on importe on ajoute un END de sécurité juste avant

                            if (imports.Count == 0)
                            {
                                lines.Add("END");
                            }

                            // On prend le code de la lib et on ajoute les lines à la fin du code actuel

                            var l = plib.Code.Replace("\t", "").Split("\n");

                            for (int j = 0; j < l.Length; j++)
                            {
                                lines.Add(l[j]);
                            }

                            // On référence la lib pour ne pas l'utiliser plusieurs fois

                            imports.Add(lib);
                        }
                    }
                    else if (line.StartsWith("#VAR "))
                    {
                        if (line.Length < 6) return new(ResultStatusEnum.VarDefinitionError, lineNumber, line);

                        var varName = line.Substring(5);

                        for (int j = 0; j < varName.Length; j++)
                        {
                            var c = varName[j];

                            if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0'&& c <= '9') || c == '_' || c == '.' || c == '%' || c == '#' || c == '$' || c == '!'))
                            {
                                return new(ResultStatusEnum.VarDefinitionError, lineNumber, line);
                            }

                            // Les caractères ! % # $ doivent se trouver en dernière position dans le nom (type de la variable)

                            if ((c == '%' || c == '$' || c == '#' || c == '!') && j != (varName.Length -1))
                            {
                                return new(ResultStatusEnum.VarDefinitionError, lineNumber, line);
                            }
                        }

                        var lastChar = varName[varName.Length - 1];

                        var varType = lastChar switch
                        {
                            '$' => "$",
                            '%' => "%",
                            '#' => "#",
                            _ => "@"
                        };

                        if (vars[varType].Contains(varName)) return new(ResultStatusEnum.DuplicateVarDefinition, lineNumber, line);

                        vars[varType].Add(varName);  
                    }
                    else if (line.StartsWith("#DEF "))
                    {
                        var def = Definition.Parse(line);

                        if (def.IsValid && def.Name != null)
                        {
                            if (defs.ContainsKey(def.Name))
                            {
                                return new(ResultStatusEnum.DuplicateDefinition, lineNumber, line);
                            }
                            else
                            {
                                defs[def.Name] = def;
                            }
                        }
                        else
                        {
                            return new(ResultStatusEnum.DefinitionError, lineNumber, line);
                        }                      
                    }
                    else
                    {
                        numLine += 10;

                        var l = new ExportLine()
                        {
                            SourceCode = line,
                            SourceLineIndex = i,
                            FinalNumLine = numLine,
                            BasicLine = $"{numLine} {line}"
                        };

                        exports.Add(l);
                    }
                }
            }

            // On remplace les définitions par leur vraie valeur

            foreach (var defName in defs.Keys)
            {
                for (int i = 0; i < exports.Count; i++)
                {
                    var e = exports[i];

                    if (e.FinalNumLine >= 100)
                    {
                        var newLine = defs[defName].UpdateLine(e.BasicLine);

                        if (newLine != null)
                        {
                            e.BasicLine = newLine;
                        }
                        else
                        {
                            return new(ResultStatusEnum.CallDefinitionError, e.SourceLineIndex + 1, e.SourceCode);
                        }
                    }
                }
            }

            // On trie les labels du plus grand au plus petit

            var labelKeys = labels.Keys.ToList();
            int labelSort(string l1, string l2) => l2.CompareTo(l1);
            labelKeys.Sort(labelSort);

            // On remplace les labels dans les lignes par leur numéro de ligne

            for (int i = 0; i < exports.Count; i++)
            {
                foreach (var key in labelKeys)
                {
                    exports[i].BasicLine = exports[i].BasicLine.Replace(key, labels[key].ToString());
                }
            }

            // On remplace les définitions ASCII par les caractères correspondants

            foreach (var line in exports)
            {
                for (int i = 0; i < 256; i++)
                {
                    var s = new string((char)i, 1);
                    line.BasicLine = line.BasicLine.Replace($"(${i})", s);
                }
            }

            // S'il reste à l'issue des @... présents dans le code c'est qu'on a utilisé des noms de labels erronés
            // Et ils n'ont pas été remplacés

            foreach (var line in exports)
            {
                if (line.BasicLine.Contains(" @")) return new GeneratorResult(ResultStatusEnum.LabelNotFound, line.SourceLineIndex + 1, line.SourceCode);
            }

            // On ajoute en debut de programme la déclaration des variables étendues

            int varCount = 0;

            foreach (string varType in types)
            {
                varCount += vars[varType].Count;
            }

            int nl = 50 + varCount;

            foreach (string varType in types)
            {
                var realType = varType switch
                {
                    "$" => "$",
                    "%" => "%",
                    "!" => "!",
                    "#" => "#",
                    _ => ""
                };

                var varList = vars[varType];

                if (varList.Count > 0)
                {
                    nl -= 1;
                    var line = $"DIM X7{realType}({varList.Count - 1})";

                    var e = new ExportLine()
                    {
                        SourceCode = "#VAR DECLARATION",
                        SourceLineIndex = 0,
                        FinalNumLine = nl,
                        BasicLine = $"{nl} {line}"
                    };

                    exports.Insert(0, e);
                }
            }

            // On remplace les variables étendues par leur variable tableaux calculées

            foreach (var varType in types)
            {
                var realType = varType switch
                {
                    "$" => "$",
                    "%" => "%",
                    "!" => "!",
                    "#" => "#",
                    _ => ""
                };

                var varList = vars[varType];

                // On trie les variables de celles avec le plus grand nom à celle avec le plus petit nom
                // Sans cet ordre des soucis de remplacement partiels apparaissent avec des varables dont le nom est proce
                // Ex SCORE% et DISPLAY_SCORE% par exemple

                varList.Sort();

                for (int j = 0; j < varList.Count; j++)
                {
                    var oldname = $"{varList[j]}";
                    var newName = $"X7{realType}({j})";

                    for (int i = 0; i < exports.Count; i++)
                    {
                        var e = exports[i];
                        if (e.FinalNumLine >= 100) e.BasicLine = e.BasicLine.Replace(oldname, newName);
                    }
                }
            }

            exports.Insert(0, new ExportLine()
            {
                SourceCode = "** X07 STUDIO HEADER **",
                SourceLineIndex = 0,
                FinalNumLine = 30,
                BasicLine = $"30 REM BY STEPHANE SIBUE"
            });

            exports.Insert(0, new ExportLine()
            {
                SourceCode = "** X07 STUDIO HEADER **",
                SourceLineIndex = 0,
                FinalNumLine = 20,
                BasicLine = $"20 REM GENERATED WITH X07 STUDIO"
            });


            var projectName = Path.GetFileNameWithoutExtension(Project.Default.Filename ?? "SANS NOM");

            exports.Insert(0, new ExportLine()
            {
                SourceCode = "** X07 STUDIO HEADER **",
                SourceLineIndex = 0,
                FinalNumLine = 10,
                BasicLine = $"10 REM PROGRAM {projectName}"
            });
          
            var sb = new StringBuilder();
            foreach( var line in exports) sb.AppendLine(line.BasicLine);

            return new GeneratorResult(sb.ToString());
        }

        private bool IsLabelValid(string line)
        {
            // Commence par un @
            // Composé des lettres A...Z, a...z, 0...9, _, -, . uniquement

            if (!line.StartsWith("@")) return false;

            var l = line.Substring(1).ToCharArray();
            
            foreach (var c in l)
            {
                if (!(
                    (c >= 'A' && c <= 'Z') || 
                    (c >= 'a' && c <= 'z') || 
                    (c >= '0' && c <= '9') ||
                    c == '_' || 
                    c == '-' ||
                    c == '.')
                    ) return false;
            }
            
            return true;
        }

        private bool IsDefinitionNameValid(string constName)
        {
            // Composé des lettres A...Z, a...z, 0...9, _, -, . uniquement
            // Se termine par () obligatoirement

            if (constName.Length < 3 || !constName.EndsWith("()")) return false;

            foreach (var c in constName.Substring(0, constName.Length - 2))
            {
                if (!(
                    (c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9') ||
                    c == '_' ||
                    c == '-' ||
                    c == '.')
                    ) return false;
            }

            return true;
        }

        private class ExportLine
        {
            public int SourceLineIndex { get; set; }

            public string SourceCode { get; set; } = "";

            public string BasicLine { get; set; } = "";

            public int FinalNumLine { get; set; }
        }

        public enum ResultStatusEnum
        {
            None,
            Success,
            LineTooLong,
            IllegalLabelDeclaration,
            DuplicateLabelDeclaration,
            LabelNotFound,
            LibraryAlreadyImported,
            LibraryNotFound,
            LibraryLoadError,
            LibraryIsEmpty,
            VarDefinitionError,
            DuplicateVarDefinition,
            DefinitionError,
            DuplicateDefinition,
            CallDefinitionError
        }

        public class GeneratorResult
        {
            public ResultStatusEnum Status { get; set; } = ResultStatusEnum.None;

            public int ErrorLineNumber {  get; set; }

            public string? ErrorLineCode { get; set; }  

            public string? Code { get; set; }  
            
            public string? ErrorMessage
            {
                get
                {
                    var sb = new StringBuilder();

                    if (Status == ResultStatusEnum.Success)
                    {
                        sb.Append("OK");
                    }
                    else
                    {
                        var errorLabel = Status switch
                        {
                            ResultStatusEnum.Success => "Succès",
                            ResultStatusEnum.DuplicateLabelDeclaration => "Label déclaré plusieurs fois",
                            ResultStatusEnum.IllegalLabelDeclaration => "Déclaration de label invalide",
                            ResultStatusEnum.LabelNotFound => "Label introuvable",
                            ResultStatusEnum.LibraryNotFound => "Bibliothèque introuvable",
                            ResultStatusEnum.LibraryIsEmpty => "Bibliothèque vide",
                            ResultStatusEnum.LibraryAlreadyImported => "Bibliothèque déjà importée",
                            ResultStatusEnum.LibraryLoadError => "Chargement de la bibliothèque impossible",
                            ResultStatusEnum.LineTooLong => "Ligne trop longue",
                            ResultStatusEnum.VarDefinitionError => "Erreur de définition de variable étendue",
                            ResultStatusEnum.DuplicateVarDefinition => "Variable étendue déclarée plusieurs fois",
                            ResultStatusEnum.DefinitionError => "Définition non valide",
                            ResultStatusEnum.DuplicateDefinition => "Définition déclarée plusieurs fois",
                            ResultStatusEnum.CallDefinitionError => "Appel incorrect d'une définition",
                            ResultStatusEnum.None => "Aucun",
                            _ => "Statut inconnu"
                        };

                        sb.AppendLine(errorLabel);
                        sb.AppendLine($"à la ligne {ErrorLineNumber}");
                        sb.AppendLine("");
                        sb.AppendLine(ErrorLineCode);
                    }

                    return sb.ToString();
                }
            }

            public GeneratorResult(ResultStatusEnum status, int errorLineNumber, string errorLineCode)
            {
                Status = status;
                ErrorLineNumber = errorLineNumber;
                ErrorLineCode = errorLineCode;
            }

            public GeneratorResult(string code)
            {
                Status = ResultStatusEnum.Success;
                Code = code;
            }
        }

        public class Definition
        {
            public string? Name { get; set; }

            public List<string> Parameters { get; } = [];

            public string? Value { get; set; }

            public bool IsValid { get; private set; }

            private static bool IsValidName(string name)
            {
                // Composé des lettres A...Z, a...z, 0...9, _, -, . uniquement

                foreach (var c in name.Substring(0, name.Length - 2))
                {
                    if (!(
                        (c >= 'A' && c <= 'Z') ||
                        (c >= 'a' && c <= 'z') ||
                        (c >= '0' && c <= '9') ||
                        c == '_' ||
                        c == '-' ||
                        c == '.')
                        ) return false;
                }

                return true;
            }

            public static Definition Parse(string input)
            {
                // Format attendu 
                // #DEF name(v1,v2,v3,....,vn)=BASIC DEFINITION
                // #DEF display(m,x,y)=LOCATE {x},{y}:PRINT {m};

                var def = new Definition();

                if (input != null  && input.StartsWith("#DEF "))
                {
                    var k1 = input.IndexOf('[');

                    if (k1 > -1)
                    {
                        var name = input.Substring(5, k1 - 5);

                        if (IsValidName(name))
                        {
                            var k2 = input.IndexOf("=");

                            if (k2 > -1)
                            {
                                var basicInstructions = input.Substring(k2 + 1);

                                var p = input.Substring(5 + name.Length + 1, k2 - 5 - name.Length - 2);
                                var ps = p.Split(',');

                                def.Name = name;
                                def.Value = basicInstructions;

                                for (int i = 0; i < ps.Length; i++)
                                {
                                    if (ps[i].Length > 0) def.Parameters.Add(ps[i]);
                                }
                                
                                def.IsValid = true; 
                            }
                        }                        
                    }
                }

                return def;
            }

            public string? UpdateLine(string line)
            {
                string newLine = line;

                if (Name != null && Value != null && IsValid)
                {
                    while (true)
                    {
                        var k1 = newLine.IndexOf($"{Name}[");
                        var k2 = 0;
                        var parametersCount = 0;

                        if (k1 > -1)
                        {
                            var inString = false;
                            var index = k1 + Name.Length + 1;
                            var currentValue = "";
                            var values = new Dictionary<string, string>();

                            while (index < newLine.Length)
                            {
                                var c = newLine[index];

                                if (currentValue.Length == 0)
                                {
                                    // On commence une nouvelle valeur

                                    if (c == '\"')
                                    {
                                        // On commence une string

                                        inString = true;
                                    }
                                    else if (c == ']')
                                    {
                                        // Fin (pas de paramètre)

                                        k2 = index;
                                        break;
                                    }
                                    else
                                    {
                                        // On commence autre chose qu'une string
                                    }

                                    // On stocke le caractère

                                    currentValue += c;
                                }
                                else
                                {
                                    if (inString)
                                    {
                                        // On est en train de composer une string

                                        currentValue += c;

                                        // On attend un " pour la terminer

                                        if (c == '\"')
                                        {
                                            inString = false;
                                        }
                                    }
                                    else
                                    {
                                        // On est en train de composer autre chose qu'une string
                                        // Si on trouve une , on passe à la valeur suivante
                                        // Si on trouve une ) on arrête tout là et on stocke l'emplacement de )

                                        if (c == ',')
                                        {
                                            parametersCount += 1;

                                            if (Parameters.Count >= values.Count + 1)
                                            {
                                                var p = Parameters[values.Count];
                                                values[p] = currentValue;
                                            }
                                            currentValue = "";
                                        }
                                        else if (c == ']')
                                        {
                                            parametersCount += 1;

                                            if (Parameters.Count >= values.Count + 1)
                                            {
                                                var p = Parameters[values.Count];
                                                values[p] = currentValue;
                                            }
                                            k2 = index;
                                            break;
                                        }
                                        else
                                        {
                                            currentValue += c;
                                        }
                                    }
                                }

                                index += 1;
                            }

                            if (k2 == 0)
                            {
                                // Si k2=0 c'est qu'on n'a pas trouvé la ) de fin
                                // Erreur !!!

                                return null;
                            }
                            else if (parametersCount != Parameters.Count)
                            {
                                // Le nombre de paramètres n'est pas bon !

                                return null;
                            }
                            else
                            {
                                // On décompose la ligne en 2 parties
                                // Avant et après l'appel à la définition

                                var begin = newLine.Substring(0, k1);
                                var end = newLine.Substring(k2 + 1);

                                // On doit remplacer chaque paramètre {x} par sa valeur dans l'instruction basic

                                var basic = Value;

                                foreach (var key in values.Keys)
                                {
                                    var pn = $"[{key}]";
                                    basic = basic.Replace(pn, values[key]);
                                }

                                newLine = $"{begin}{basic}{end}";
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return newLine;
            }
        }
    }
}
