// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Secretariat of the Pacific Community">
// Copyright (C) 2012 Secretariat of the Pacific Community
// </copyright>
// -----------------------------------------------------------------------

namespace Spc.Ofp.RelinkSSRS
{
    /*
    * This file is part of TUBS.
    *
    * TUBS is free software: you can redistribute it and/or modify
    * it under the terms of the GNU Affero General Public License as published by
    * the Free Software Foundation, either version 3 of the License, or
    * (at your option) any later version.
    *  
    * TUBS is distributed in the hope that it will be useful,
    * but WITHOUT ANY WARRANTY; without even the implied warranty of
    * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    * GNU Affero General Public License for more details.
    *  
    * You should have received a copy of the GNU Affero General Public License
    * along with TUBS.  If not, see <http://www.gnu.org/licenses/>.
    */
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using CmdLine;
    
    /// <summary>
    /// Simple utility for merging SSRS shared datasets into a report definition
    /// for use with SSRS Express.
    /// </summary>
    public class Program
    {
        public const string ReportingServicesNamespace = @"http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition";
        
        static void Main(string[] args)
        {
            RelinkArguments arguments = null;
            try
            {
                arguments = CommandLine.Parse<RelinkArguments>();
            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e.ArgumentHelp.Message);
                Console.WriteLine(e.ArgumentHelp.GetHelpText(Console.BufferWidth));
                return;
            }

            if (null == arguments)
            {
                Console.WriteLine("Invalid argument structure");
                return;
            }

            if (!File.Exists(arguments.Source))
            {
                Console.WriteLine("Source file [{0}] not found", arguments.Source);
                return;
            }

            if (File.Exists(arguments.Destination) && !arguments.ShouldOverwrite)
            {
                Console.WriteLine("Destination file [{0}] already exists.  To overwrite, use /Y", arguments.Destination);
                return;
            }

            // Ensure DatasetDirectory argument has a trailing slash
            if (!arguments.DatasetDirectory.EndsWith(@"\"))
                arguments.DatasetDirectory += @"\";

            bool results = Relink(arguments.Source, arguments.Destination, arguments.DatasetDirectory);
            if (results)
            {
                Console.WriteLine("Modified report definition written to [{0}]", arguments.Destination);
            }
            else
            {
                Console.WriteLine("Failed to modify report definition");
            }
                
        }

        protected static bool Relink(string source, string destination, string datasetDir)
        {
            bool success = false;
            var rdl = XDocument.Load(source);
            XNamespace ns = ReportingServicesNamespace;
            var datasetLookup = new Dictionary<string, string>();
            var queryLookup = new Dictionary<string, Query>();

            // SharedDataSetReference is found via:
            // DataSet/SharedDataSet/SharedDataSetReference

            // Iterate over the datasets
            var datasets = 
                from x in rdl.Descendants(XName.Get("DataSet", ReportingServicesNamespace))
                where x.Element(XName.Get("SharedDataSet", ReportingServicesNamespace)) != null
                select x;

            foreach (var dataset in datasets)
            {
                var dsName = dataset.Attribute("Name").Value;
                Console.WriteLine("Checking {0} for presence of shared dataset...", dsName);
                var childQuery =
                    from n in dataset.Descendants(XName.Get("SharedDataSetReference", ReportingServicesNamespace))
                    select n.Value;
                var sharedDatasetName = childQuery.FirstOrDefault();
                if (!String.IsNullOrEmpty(sharedDatasetName))
                {
                    Console.WriteLine("DataSet uses contents of {0}.rsd", sharedDatasetName);
                    datasetLookup.Add(dsName, sharedDatasetName);
                }
            }

            // Exit if this is already compatible with SSRS Express
            if (datasetLookup.Keys.Count == 0)
            {
                Console.WriteLine("No shared datasets, no changes required");
                return false;
            }

            foreach (var ds in datasetLookup.Keys)
            {
                var fname = String.Format("{0}.rsd", datasetLookup[ds]);               
                var rsdPath = Path.GetFullPath(datasetDir + fname);
                Console.WriteLine("Parsing SharedDataSet [{0}]", rsdPath);
                var q = Query.Load(rsdPath);
                queryLookup.Add(ds, q); // Store using DataSet name
            }

            // Iterate over datasets again (only look at those that have shared datasets)
            foreach (var dataset in datasets)
            {
                var dsName = dataset.Attribute("Name").Value;
                var sds = dataset.Element(XName.Get("SharedDataSet", ReportingServicesNamespace));
                if (null == sds)
                {
                    continue;
                }
                sds.Remove();
                dataset.AddFirst(queryLookup[dsName].AsXElement());
            }

            // Write document to output file
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "    "
            };
            using (var fstream = new FileStream(destination, FileMode.OpenOrCreate))
            using (var writer = XmlWriter.Create(fstream, settings))
            {
                rdl.WriteTo(writer);
                success = true;
            }           

            return success;
        }
    }
}
