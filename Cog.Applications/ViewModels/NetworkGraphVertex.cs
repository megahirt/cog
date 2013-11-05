using SIL.Cog.Domain;

namespace SIL.Cog.Applications.ViewModels
{
	public class NetworkGraphVertex : WrapperViewModel
	{
		private readonly Variety _variety;

		public NetworkGraphVertex(Variety variety)
			: base(variety)
		{
			_variety = variety;
		}

		public string Name
		{
			get { return _variety.Name; }
		}
	}
}