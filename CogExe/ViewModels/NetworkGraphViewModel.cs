﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using QuickGraph;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class NetworkGraphViewModel : WorkspaceViewModelBase
	{
		private IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> _graph;
		private CogProject _project;
		private SimilarityMetric _similarityMetric;
		private readonly IExportService _exportService;
		private double _similarityScoreFilter;

		public NetworkGraphViewModel(IExportService exportService)
			: base("Network Graph")
		{
			_exportService = exportService;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => Graph = _project.GenerateNetworkGraph(_similarityMetric));
			Messenger.Default.Register<ModelChangingMessage>(this, msg => Graph = null);

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Similarity metric",
				new TaskAreaCommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new TaskAreaCommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export this graph", new RelayCommand(Export))));
			_similarityScoreFilter = 0.7;
		}

		private void Export()
		{
			_exportService.ExportCurrentNetworkGraph(this);
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Graph = _project.VarietyPairs.Count > 0 ? _project.GenerateNetworkGraph(_similarityMetric) : null;
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value) && _graph != null)
					Graph = _project.GenerateNetworkGraph(_similarityMetric);
			}
		}

		public double SimilarityScoreFilter
		{
			get { return _similarityScoreFilter; }
			set { Set(() => SimilarityScoreFilter, ref _similarityScoreFilter, value); }
		}

		public IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> Graph
		{
			get { return _graph; }
			set { Set(() => Graph, ref _graph, value); }
		}
	}
}
