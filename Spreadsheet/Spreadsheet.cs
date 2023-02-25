/// <summary>
/// Author:    Sasha Rybalkina
/// Partner:   None
/// Date:      Febuary 17, 2023
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Sasha Rybalkina - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, Sasha Rybalkina, certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents
/// The method GetCellContents, which returns the contents of a specified cell
/// The method GetNamesOfAllNonemptyCells, which returns a list of all nonempty
/// cells.
/// The SetContentsOfCell method, which sets the contents and values of cells.
/// If a cell's contents are a Formula class, the value of that cell gets
/// evaluated based on the formula contents.
/// The GetDirectDependents method, which returns all of the direct dependents
/// of a specified cell.
/// The GetSavedVersion method, which reads a given file and returns its versioning.
/// The Save method, which writes a brand new file with all current cells.
/// </summary>
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Xml.Linq;
using System.Xml;
using Microsoft.VisualBasic;
using SpreadsheetUtilities;
namespace SS
{

    public class Spreadsheet : AbstractSpreadsheet
    {
        private DependencyGraph dependencies = new();
        private Dictionary<string, Cell> cells = new();
        private string? PathToFile;

        public Spreadsheet() : this(s => true, s => s, "default")
        {
        }

        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
        }

        /// Constructor for getting cells and versioning from a file.
        public Spreadsheet(string pathToFile, Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            try
            {
                XmlReader read = XmlReader.Create(pathToFile);
                string? name = "";
                string? contents = "";
                while (read.Read())
                {
                    if (read.IsStartElement())
                    {
                        if (read.Name.Equals("spreadsheet"))
                        {
                            if (read["version"] != Version)
                            {
                                throw new SpreadsheetReadWriteException("Incorrect version");
                            }
                        }
                        else if (read.Name.Equals("cell"))
                        {

                        }
                        else if (read.Name.Equals("name"))
                        {
                            read.Read();
                            name = read.Value;
                        }
                        else if (read.Name.Equals("contents"))
                        {
                            read.Read();
                            contents = read.Value;
                        }
                        else
                        {
                            throw new SpreadsheetReadWriteException("Incorrect file structure");
                        }
                    }
                    else
                    {
                        if (read.Name.Equals("cell"))
                        {
                            SetContentsOfCell(name, contents);
                        }
                    }
                }
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Unable to construct file properly");
            }
        }

        /// <summary>
        /// Private helper method for the three SetCellContents
        /// methods.
        /// </summary>
        /// <param name="name">Name of cell</param>
        /// <param name="contents">The contents of the cell</param>
        /// <returns>A list of all cells that directly or indirectly depend on
        /// the given cell.</returns>
        /// <exception cref="InvalidNameException"></exception>
        private IList<string> SetCell(string name, object contents)
        {
            if (cells.ContainsKey(name))
            {
                cells[name].SetContents(contents);
                cells[name].SetValue(contents);
            }
            else if (contents + "" != "")
            {
                cells.Add(name, new Cell(contents));
            }
            return GetCellsToRecalculate(name).ToList();
        }

        public override bool Changed
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the contents or value of the given cell.
        /// </summary>
        /// <param name="name">The cell being looked at</param>
        /// <returns>The contents or value of the cell</returns>
        /// <exception cref="InvalidNameException"></exception>
        public override object GetCellContents(string name)
        {
            if (name != null && Regex.IsMatch(Normalize(name), @"^[a-z|A-Z|]+[0-9]+$"))
            {
                if (!cells.ContainsKey(Normalize(name)))
                {
                    return "";
                }
                return cells[Normalize(name)].GetContents();
            }
            throw new InvalidNameException();
        }

        /// <summary>
        /// Gets the value of the cell specified.
        /// </summary>
        /// <param name="name">The name of the cell</param>
        /// <returns>The value of the cell</returns>
        public override object GetCellValue(string name)
        {
            if (name == null || !Regex.IsMatch(Normalize(name), "^[a-z|A-Z]+[0-9]+$") || !IsValid(name))
            {
                throw new InvalidNameException();
            }
            if (!cells.ContainsKey(Normalize(name)))
            {
                return "";
            }
            return cells[Normalize(name)].GetValue();
        }

        /// <summary>
        /// This method gets all of the cells in the spreadsheet that are nonempty.
        /// </summary>
        /// <returns>A list of the names of the nonempty cells</returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return cells.Keys;
        }

        /// <summary>
        /// This method gets the version of the file being looked at by reading
        /// every line of the file, going into the "spreadsheet" element, then
        /// into the "version" element and reading it. If there is no versioning,
        /// or if any issues come up with reading the file, and exception is
        /// thrown.
        /// </summary>
        /// <param name="filename">The file being looked at</param>
        /// <returns>The versioning of the file</returns>
        /// <exception cref="SpreadsheetReadWriteException"></exception>
        public override string GetSavedVersion(string filename)
        {
            try
            {
                using XmlReader read = XmlReader.Create(filename);
                while (read.Read())
                {
                    if (read.IsStartElement())
                    {
                        if (read.Name.Equals("spreadsheet"))
                        {
                            string? version = read["version"];
                            if (version is null)
                            {
                                throw new SpreadsheetReadWriteException("Cannot find version element");
                            }
                            else
                            {
                                return version;
                            }
                        }
                    }
                }
            }
            ///If anything goes wrong reading the file, or if spreadsheet does
            ///not contain a version element, an exception is thrown.
            catch
            {
                throw new SpreadsheetReadWriteException("Unable to read file properly");
            }
            throw new SpreadsheetReadWriteException("Cannot find version element");
        }

        /// <summary>
        /// Write a brand new file based on the non empty cells that spreadsheet
        /// has.
        /// </summary>
        /// <param name="filename">The file being looked at</param>
        public override void Save(string filename)
        {
            try
            {
                using XmlWriter write = XmlWriter.Create(filename);
                ///Writes spreadsheet start element
                write.WriteStartDocument();
                write.WriteStartElement("spreadsheet");
                write.WriteAttributeString("version", Version);

                foreach (string cell in cells.Keys)
                {
                    write.WriteStartElement("cell");
                    string name = cell;
                    string contents = "";
                    object objectContents = cells[cell].GetContents();
                    ///If statements for handling different types of contents.
                    if (objectContents is double)
                    {
                        contents = objectContents.ToString();
                    }
                    else if (objectContents is string)
                    {
                        contents = (string)objectContents;
                    }
                    else if (objectContents is Formula)
                    {
                        contents = "=" + objectContents.ToString();
                    }
                    write.WriteElementString("name", name);
                    write.WriteElementString("contents", contents);
                    write.WriteEndElement();
                }
                write.WriteEndElement();
                write.WriteEndDocument();
                Changed = false;
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Cannot find file");
            }
        }

        /// <summary>
        /// Checks if the string content given is a double, string, or Formula,
        /// and uses the appropriate SetCellContents method to set the cell to
        /// its right contents and value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <exception cref="InvalidNameException"></exception>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            List<string> list = new();
            Changed = true;
            string normname = Normalize(name);
            if (content != "")
            {
                if (name == null || !Regex.IsMatch(name, "^[a-z|A-Z]+[0-9]+$") || !IsValid(name))
                {
                    throw new InvalidNameException();
                }
                else if (Double.TryParse(content, result: out double Result))
                {
                    list = SetCellContents(normname, Result).ToList();
                }
                else if (!(content[0] == '='))
                {
                    list = SetCellContents(normname, content).ToList();
                }
                else
                {
                    string expression = content[1..];
                    Formula formula = new Formula(expression, Normalize, IsValid);
                    list = SetCellContents(normname, formula).ToList();
                }
                ///Recalculates 
                foreach (string s in list)
                {
                    if (GetCellContents(s) is Formula f)
                    {
                        cells[s].SetValue(f.Evaluate(lookup));
                        GetCellsToRecalculate(s);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Gets all of the direct dependents of the given cell.
        /// </summary>
        /// <param name="name">The cell being checked for dependents</param>
        /// <returns>A list of all of the direct dependents of the given cell.</returns>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null || !Regex.IsMatch(name, "^[a-z|A-Z]+[0-9]+$") || !IsValid(name))
            {
                throw new InvalidNameException();
            }
            return dependencies.GetDependents(name);
        }

        /// <summary>
        /// This method either adds a new cell to the cell dictionary created
        /// earlier, or replaces the current contents of the given cell if that
        /// cell exists in the dictionary already. Then gets every cell that
        /// depends on the "name" cell.
        /// </summary>
        /// <param name="name">The name of the cell</param>
        /// <param name="number">The numerical value of the cell</param>
        /// <returns>A list of all cells that directly or indirectly depend on
        /// the given cell.</returns>
        /// <exception cref="InvalidNameException"></exception>
        protected override IList<string> SetCellContents(string name, double number)
        {
            dependencies.ReplaceDependees(name, new HashSet<string>());
            return SetCell(name, number);
        }

        /// <summary>
        /// This method either adds a new cell to the cell dictionary created
        /// earlier, or replaces the current contents of the given cell if that
        /// cell exists in the dictionary already. Then gets every cell that
        /// depends on the "name" cell.
        /// </summary>
        /// <param name="name">The name of the cell</param>
        /// <param name="text">The contents or value of the cell in string form.</param>
        /// <returns>A list of all cells that directly or indirectly depend on
        /// the given cell.</returns>
        /// <exception cref="InvalidNameException"></exception>
        protected override IList<string> SetCellContents(string name, string text)
        {
            dependencies.ReplaceDependees(name, new HashSet<string>());
            return SetCell(name, text);
        }

        /// <summary>
        /// This method either adds a new cell to the cell dictionary created
        /// earlier, or replaces the current contents of the given cell if that
        /// cell exists in the dictionary already. Then gets every cell that
        /// depends on the given cell.
        /// </summary>
        /// <param name="name">The name of the cell</param>
        /// <param name="formula">The formula content in the cell</param>
        /// <returns>A list of all cells that directly or indirectly depend on
        /// the given cell.</returns>
        /// <exception cref="InvalidNameException"></exception>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            List<string> ToSave = dependencies.GetDependents(name).ToList<string>();
            dependencies.ReplaceDependees(name, formula.GetVariables());

            try
            {
                GetCellsToRecalculate(name);
            }
            catch
            {
                dependencies.ReplaceDependees(name, ToSave);
                throw;
            }

            return SetCell(name, formula);
        }

        /// <summary>
        /// Private delagate for looking up variables.
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        /// <exception cref="InvalidNameException"></exception>
        private double lookup(string var)
        {
            if (!(GetCellValue(var) is double))
            {
                throw new ArgumentException();
            }
            return (double)GetCellValue(var);
        }
    }
}
public class Cell
{
    private object contents;
    private object value;

    public Cell(object v)
    {
        contents = v;
        value = v;
    }
    public object GetValue()
    {
        return value;
    }
    public object GetContents()
    {
        return contents;
    }
    public void SetValue(object newValue)
    {
        value = newValue;
    }
    public void SetContents(object newContents)
    {
        contents = newContents;
    }
}
