// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)
//
// Final version written by Sasha Rybalkina, January 2023.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace SpreadsheetUtilities
{
    /// <summary>
    /// This is a representation of a dependency graph that uses two dictionaries in order to indicate the
    /// dependents and dependees of every string in the graph. Whenever a new pair is added, such as (s, t),
    /// s is added a new key in the "Dependents" dictionary, and t is added to a list which is then added
    /// to the dictionary as the value of key s. Then, t is added as a new key in the "Dependee" dictionary,
    /// and s is added to a new list which is then added as the value of key t. If the dictionaries already
    /// contain the keys for the pair (s, t) when it is being added, t will be added to Dependents[s], and s
    /// will be added to Dependees[t].
    /// </summary>
    public class DependencyGraph
    {
        private Dictionary<string, List<string>> Dependents;
        /// This is the tracker of the size of the graph
        private int size = 0;
        /// This creates a dictionary for the dependents of every string
        private Dictionary<string, List<string>> Dependees;
        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            Dependents = new();
            Dependees = new();
        }
        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return size; }
        }
        /// <summary>
        /// Returns the number of dependees that a dependent has.
        /// </summary>
        public int this[string s]
        {
            get { if (!Dependees.ContainsKey(s)) { return 0; } return Dependees[s].Count; }
        }
        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            if (Dependents.ContainsKey(s))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            if (Dependees.ContainsKey(s))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Returns the dependents of s in IEnumerable form.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (Dependents.ContainsKey(s))
            {
                return new List<string>(Dependents[s]);
            }
            List<string> empty = new();
            return empty;
        }
        /// <summary>
        /// Returns the dependees of s in IEnumerable form.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (Dependees.ContainsKey(s))
            {
                return new List<string>(Dependees[s]);
            }
            List<string> empty = new();
            return empty;
        }
        /// <summary>
        /// Adds the pair (s, t) if it does not exist in the dictionaries. If the
        /// key s is already in the "Dependents" dictionary, t is simply added to
        /// the list of Dependents[s], and t will be added as a new key in the
        /// Dependees dictionary with s as the only value in Dependees[t]. Vice
        /// versa for if t is already a key in the "Dependees" dictionary.
        /// </summary>
        /// <param name="s"> The dependee of t</param>
        /// <param name="t"> The dependent of s</param>
        public void AddDependency(string s, string t)
        {
            /// This if statement deals with how the pair should be added to the "Dependents"
            /// dictionary. If s is a key in the dictionary, checks if (s, t) is already an
            /// ordered pair, and then adds the pair if it is not a duplicate.
            if (Dependents.ContainsKey(s))
            {
                if (!(Dependents[s].Contains(t)))
                {
                    Dependents[s].Add(t);
                    size++;
                }
            }
            /// If s isn't already a key in the dictionary, then adds a new key, value pair to
            /// the dictionary, with s as the key, and a new list containing t as the value.
            else
            {   
                Dependents.Add(s, new List<string>() { t });
                size++;
            }
            /// This if statement deals with how the pair should be added to the "Dependees"
            /// dictionary. If t is a key in the dictionary, checks if (s, t) is already an
            /// ordered pair, and then adds the pair if it is not a duplicate.
            if (Dependees.ContainsKey(t))
            {
                if (!(Dependees[t].Contains(s)))
                {
                    Dependees[t].Add(s);
                }
            }
            /// If t isn't already a key in the dictionary, then adds a new key, value pair to
            /// the dictionary, with t as the key, and a new list containing s as the value.
            else
            {
                Dependees.Add(t, new List<string>() { s });
            }
        }
        /// <summary>
        /// Removes the ordered pair (s,t) from the "Dependents" and "Dependees"
        /// dictionaries if the pair exists. If Dependents[s] only contains t,
        /// and Dependees[t] only contains s, the s and t keys get removed
        /// entirely from the dictionaries. Else, t and s are simply removed
        /// from their respective lists in the dictionaries.
        /// </summary>
        /// <param name="s">The dependee of the pair</param>
        /// <param name="t">The dependent of the pair</param>
        public void RemoveDependency(string s, string t)
        {
            if (Dependents.ContainsKey(s))
            {
                /// This if statement deals with how the pair gets removed from the
                /// Dependents dictionary.
                if (Dependents[s].Count == 1 && Dependents[s].Contains(t))
                {
                    /// Removes the entire key, value pair
                    Dependents.Remove(s);
                }
                else
                {
                    /// Only removes t from the value list of s.
                    Dependents[s].Remove(t);
                }
                size--;
                /// This if statement deals with how the pair gets removed from the
                /// Dependees dictionary.
                if (Dependees[t].Count == 1 && Dependees[t].Contains(s))
                {
                    /// Removes the entire key, value pair
                    Dependees.Remove(t);
                }
                else
                {
                    /// Only removes s from the value list of t.
                    Dependees[t].Remove(s);
                }
            }
        }
        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r). Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            foreach (string r in GetDependents(s))
            {
                RemoveDependency(s, r);
            }
            foreach (string t in newDependents)
            {
                AddDependency(s, t);
            }
        }
        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            foreach (string r in GetDependees(s))
            {
                RemoveDependency(r, s);
            }
            foreach (string t in newDependees)
            {
                AddDependency(t, s);
            }
        }
    }
}
