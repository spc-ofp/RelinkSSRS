RelinkSSRS
==============

The RelinkSSRS application is a utility for inlining SSRS
shared datasets into report definitions.

It was written for TUBS, to support satellite installations where
the express version of SQL Server is used.

The transformation was determined by inspection of .rdl files
with and without shared datasets.  As a simplifying assumption,
the utility assumes that the datasource name in the shared dataset will
be the name of the dataset embedded in the stand alone .rdl.

Execution of the utility is as follows:
RelinkSSRS.exe c:\temp\PS-TripPositions.rdl c:\temp\foo.rdl /D:"Path/To/Directory/Holding/RSD-files/"

The utility will exit if the target file exists unless you pass in a "/Y" parameter.  The "/D" parameter
is optional and defaults to the current directory if not provided.