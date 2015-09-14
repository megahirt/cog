﻿using CommandLine;

namespace SIL.Cog.CommandLine
{
	public enum ReturnCodes
	{
		Okay = 0,
		InputError = 1,
		NotImplemented = 2,
		UnknownVerb = 3
	}

	class Program
	{
		static int Main(string[] args)
		{
			ReturnCodes retcode = Parser.Default.ParseArguments<SyllabifyVerb, MakePairsVerb, DistanceVerb, ClusterVerb>(args)
				.Return((VerbBase opts) => opts.RunAsPipe(), (errs) => ReturnCodes.UnknownVerb);
			return (int)retcode;
		}
	}

}
