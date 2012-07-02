// -----------------------------------------------------------------------
// <copyright file="RelinkArguments.cs" company="Secretariat of the Pacific Community">
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
    using CmdLine;

    /// <summary>
    /// Structure containing passed in command line arguments.
    /// </summary>
    [CommandLineArguments(Program = "RelinkSSRS", Title = "Relink SSRS Reports", Description = "Merge shared datasets into an SSRS report definition")]
    public class RelinkArguments
    {
        [CommandLineParameter(Command = "?", Default = false, Description = "Show Help", Name = "Help", IsHelp = true)]
        public bool Help { get; set; }

        [CommandLineParameter(Name = "source", ParameterIndex = 1, Required = true, Description = "Input file name")]
        public string Source { get; set; }

        [CommandLineParameter(Name = "destination", ParameterIndex = 2, Required = true, Description = "Output file name")]
        public string Destination { get; set; }

        [CommandLineParameter(Command = "D", Default = @".\", Description = "Directory holding dataset definitions.  Only use if the definitions aren't in the directory with the .rdl file")]
        public string DatasetDirectory { get; set; }

        [CommandLineParameter(Command = "Y", Default = false, Description = "Overwrite existing files")]
        public bool ShouldOverwrite { get; set; }
    }
}
